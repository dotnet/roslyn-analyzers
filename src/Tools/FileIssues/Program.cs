using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CsvHelper;
using log4net;
using Octokit;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace FileIssues
{
    internal class Program
    {
        private const int Succeeded = 0;
        private const int Failed = 1;

        private const string FxCopPortLabel = "FxCop Port";
        private const string NeedsReviewLabel = "Needs Review";
        private const string CutLabel = "Resolution-Won't Fix";
        private const string PortedLabel = "Resolution-Fixed";

        private readonly ILog _log = LogHelper.GetLogger();
        private readonly FileIssuesOptions _options;

        private readonly IIssuesClient _issuesClient;
        private readonly IIssuesLabelsClient _issuesLabelsClient;

        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<FileIssuesOptions>(args)
                .MapResult(
                    options => new Program(options).Run().Result,
                    errs => Failed
                    );
        }

        private Program(FileIssuesOptions options)
        {
            _options = options;

            _issuesClient = GitHub.GetIssuesClient(_options.Token);
            _issuesLabelsClient = GitHub.GetIssuesLabelsClient(_options.Token);
        }

        private async Task<int> Run()
        {
            if (!File.Exists(_options.RulesFilePath))
            {
                _log.ErrorFormat(Resources.ErrorRulesFileNotFound, _options.RulesFilePath);
                return Failed;
            }

            var rulesToPort = GetRulePortingInfo();
            await CreateIssues(rulesToPort);

            return Succeeded;
        }

        private IEnumerable<PortingInfo> GetRulePortingInfo()
        {
            var rulesToPort = new List<PortingInfo>();

            var reader = new StreamReader(new FileStream(_options.RulesFilePath, System.IO.FileMode.Open));
            var csv = new CsvReader(reader);
            csv.Configuration.RegisterClassMap<PortingInfoClassMap>();

            while (csv.Read())
            {
                var portingInfo = csv.GetRecord<PortingInfo>();
                if (portingInfo.Disposition != Disposition.Unknown)
                {
                    rulesToPort.Add(portingInfo);
                }
            }

            return rulesToPort;
        }

        private async Task CreateIssues(IEnumerable<PortingInfo> rulesToPort)
        {
            var existingIssues = await _issuesClient.GetAllForRepository(
                _options.RepoOwner,
                _options.RepoName,
                new RepositoryIssueRequest
                {
                    Filter = IssueFilter.All,   // Get issues whether or not they're assigned to me.
                    State = ItemState.All       // Get issues whether they're open or closed.
                });

            foreach (var ruleToPort in rulesToPort)
            {
                string title = MakeIssueTitle(ruleToPort);
                var matchingIssues = existingIssues.Where(issue => issue.Title == title);
                int numMatchingIssues = matchingIssues.Count();

                if (numMatchingIssues == 0)
                {
                    _log.InfoFormat(Resources.InfoFilingNewIssue, ruleToPort.Id);

                    Issue issue = FileIssueAsync(ruleToPort, title).Result;

                    // If the issue we just filed has already been dealt with, one way
                    // or another, then immediately close it. UpdateIssueAsync will do that.
                    if (ruleToPort.Disposition == Disposition.Cut || ruleToPort.Disposition == Disposition.Ported)
                    {
                        await UpdateIssueAsync(ruleToPort, issue);
                    }
                }
                else if (numMatchingIssues == 1)
                {
                    Issue existingIssue = matchingIssues.First();
                    int issueNumber = existingIssue.Number;

                    _log.InfoFormat(Resources.InfoUpdatingExistingIssue, issueNumber, ruleToPort.Id);

                    await UpdateIssueAsync(ruleToPort, existingIssue);
                }
                else if (numMatchingIssues > 1)
                {
                    _log.WarnFormat(
                        Resources.WarningMultipleIssuesExist,
                        ruleToPort.Id,
                        string.Join(", ", matchingIssues.Select(i => i.Number.ToString()).ToArray()));
                }

                if (_options.Delay > 0 && !_options.DryRun)
                {
                    _log.DebugFormat(Resources.DebugDelaying, _options.Delay);
                    Thread.Sleep(_options.Delay);
                }
            }
        }

        private async Task<Issue> FileIssueAsync(PortingInfo ruleToPort, string title)
        {
            Issue issue = null;

            NewIssue newIssue = CreateNewIssue(ruleToPort, title);
            if (_options.DryRun)
            {
                _log.Info(Resources.InfoDryRunIssueNotCreated);
                issue = MakeFakeIssue(newIssue);
            }
            else
            {
                issue = await _issuesClient.Create(_options.RepoOwner, _options.RepoName, newIssue);
                _log.InfoFormat(Resources.InfoIssueCreated, issue.Number);
            }

            return issue;
        }

        /// <summary>
        /// Synthesize an Issue object from the NewIssue object representing
        /// the issue we sent to GitHub to be filed.
        /// </summary>
        /// <param name="newIssue">
        /// Object representing the issue we sent to GitHub.
        /// </param>
        /// <returns>
        /// An Issue object, partially filled out with information from <paramref name="newIssue"/>.
        /// </returns>
        /// <remarks>
        /// In a dry run, we don't actually send the NewIssue object to GitHub, so we
        /// don't get a real Issue object back. If we then want to update the Issue object,
        /// as we do in the case where we want to immediately close it, we must
        /// synthesize an issue object so that the dry run correctly reports what
        /// would have happened in the course of the update.
        /// </remarks>
        private Issue MakeFakeIssue(NewIssue newIssue)
        {
            return new Issue(
                null,               // url
                null,               // htmlUrl
                null,               // commentsUrl
                9999,               // issue number
                ItemState.Open,     // state
                newIssue.Title,     // title
                newIssue.Body,      // body
                null,               // user
                newIssue.Labels     // labels (with conversion from label names to Label objects
                    .Select(labelName => new Label(null, labelName, "no color"))
                    .ToList().
                    AsReadOnly(),
                null,               // assignee
                null,               // milestone
                0,                  // number of comments
                null,               // pull request
                null,               // closed date
                DateTimeOffset.Now, // created date
                null                // updated date
                );
        }

        private async Task UpdateIssueAsync(PortingInfo ruleToPort, Issue existingIssue)
        {
            int issueNumber = existingIssue.Number;

            IssueUpdate issueUpdate = CreateIssueUpdate(ruleToPort, existingIssue);
            if (_options.DryRun)
            {
                _log.Info(Resources.InfoDryRunIssueNotUpdated);
            }
            else
            {
                await _issuesClient.Update(_options.RepoOwner, _options.RepoName, issueNumber, issueUpdate);
                _log.InfoFormat(Resources.InfoIssueUpdated, issueNumber);
            }

            // The GitHub Issues API doesn't let you add or remove individual labels in the course of
            // an Update operation. Use the IssuesLabels API to do that.
            await UpdateIssueLabelsAsync(ruleToPort, existingIssue);
        }

        private string MakeIssueTitle(PortingInfo ruleToPort)
        {
            // Don't localize this. Otherwise people with different locales would file issues
            // with different titles, and you would get duplicates.
            const string FxCopPortTitlePrefix = "Port FxCop rule";

            return $"{FxCopPortTitlePrefix} {ruleToPort.Id}: {ruleToPort.Name}";
        }

        private NewIssue CreateNewIssue(PortingInfo ruleToPort, string title)
        {
            var newIssue = new NewIssue(title);
            AddLabel(FxCopPortLabel, newIssue.Labels);

            switch (ruleToPort.Disposition)
            {
                case Disposition.NeedsReview:
                    AddLabel(NeedsReviewLabel, newIssue.Labels);
                    break;

                case Disposition.Cut:
                    AddLabel(CutLabel, newIssue.Labels);
                    break;

                case Disposition.Ported:
                    AddLabel(PortedLabel, newIssue.Labels);
                    break;
            }

            newIssue.Body = FormatIssueBody(ruleToPort);

            return newIssue;
        }

        private IssueUpdate CreateIssueUpdate(PortingInfo ruleToPort, Issue existingIssue)
        {
            IssueUpdate issueUpdate = existingIssue.ToUpdate();
            issueUpdate.Body = FormatIssueBody(ruleToPort);

            if (existingIssue.State == ItemState.Open && (ruleToPort.Disposition == Disposition.Cut || ruleToPort.Disposition == Disposition.Ported))
            {
                _log.InfoFormat(Resources.InfoClosingIssue, existingIssue.Number);
                issueUpdate.State = ItemState.Closed;
            }

            if (existingIssue.State == ItemState.Closed && (ruleToPort.Disposition != Disposition.Cut && ruleToPort.Disposition != Disposition.Ported))
            {
                _log.InfoFormat(Resources.InfoReopeningIssue, existingIssue.Number);
                issueUpdate.State = ItemState.Open;
            }

            return issueUpdate;
        }

        private async Task UpdateIssueLabelsAsync(PortingInfo ruleToPort, Issue existingIssue)
        {
            var existingLabelNames = new Collection<string>(existingIssue.Labels.Select(label => label.Name).ToList());

            var labelNamesToAdd = new Collection<string>();
            var labelNamesToRemove = new Collection<string>();

            AddLabel(FxCopPortLabel, labelNamesToAdd, existingLabelNames);

            switch (ruleToPort.Disposition)
            {
                case Disposition.NeedsReview:
                    AddLabel(NeedsReviewLabel, labelNamesToAdd, existingLabelNames);
                    RemoveLabel(CutLabel, labelNamesToRemove, existingLabelNames);
                    RemoveLabel(PortedLabel, labelNamesToRemove, existingLabelNames);
                    break;

                case Disposition.Port:
                    RemoveLabel(NeedsReviewLabel, labelNamesToRemove, existingLabelNames);
                    RemoveLabel(CutLabel, labelNamesToRemove, existingLabelNames);
                    RemoveLabel(PortedLabel, labelNamesToRemove, existingLabelNames);
                    break;

                case Disposition.Cut:
                    AddLabel(CutLabel, labelNamesToAdd, existingLabelNames);
                    RemoveLabel(NeedsReviewLabel, labelNamesToRemove, existingLabelNames);
                    RemoveLabel(PortedLabel, labelNamesToRemove, existingLabelNames);
                    break;

                case Disposition.Ported:
                    AddLabel(PortedLabel, labelNamesToAdd, existingLabelNames);
                    RemoveLabel(CutLabel, labelNamesToRemove, existingLabelNames);
                    RemoveLabel(NeedsReviewLabel, labelNamesToRemove, existingLabelNames);
                    break;
            }

            if (_options.DryRun)
            {
                _log.Info(Resources.InfoDryRunLabelsNotUpdated);
            }
            else
            {
                if (labelNamesToAdd.Any())
                {
                    await _issuesLabelsClient.AddToIssue(
                        _options.RepoOwner,
                        _options.RepoName,
                        existingIssue.Number,
                        labelNamesToAdd.ToArray());
                }

                // For some reason the "Remove" API doesn't take an array.
                foreach (string labelName in labelNamesToRemove)
                {
                    await _issuesLabelsClient.RemoveFromIssue(
                        _options.RepoOwner,
                        _options.RepoName,
                        existingIssue.Number,
                        labelName);
                }
            }
        }

        private void AddLabel(string label, Collection<string> labels, Collection<string> existingLabels = null)
        {
            if (existingLabels == null || !existingLabels.Contains(label))
            {
                labels.Add(label);
                _log.InfoFormat(Resources.InfoAddingLabel, label);
            }
        }

        private void RemoveLabel(string label, Collection<string> labels, Collection<string> existingLabels = null)
        {
            if (existingLabels == null || existingLabels.Contains(label))
            {
                labels.Add(label);
                _log.InfoFormat(Resources.InfoRemovingLabel, label);
            }
        }

        private string FormatIssueBody(PortingInfo ruleToPort)
        {
            // Don't localize this. We want all the issues in English.
            return
                $"**Title:** {ruleToPort.Title}\n\n" +
                $"**Description:**\n\n{ruleToPort.Description}\n\n" +
                $"**Proposed analyzer:** {ruleToPort.ProposedAnalyzer}\n\n" +
                $"**Notes:**\n\n{ruleToPort.Notes}";
        }
    }
}
