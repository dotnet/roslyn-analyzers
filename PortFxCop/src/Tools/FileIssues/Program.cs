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

                    if (_options.DryRun)
                    {
                        _log.Info(Resources.InfoDryRunIssueNotCreated);
                    }
                    else
                    {
                        await FileIssueAsync(ruleToPort, title);
                    }

                }
                else if (numMatchingIssues == 1)
                {
                    Issue existingIssue = matchingIssues.First();
                    int issueNumber = existingIssue.Number;

                    _log.InfoFormat(Resources.InfoUpdatingExistingIssue, issueNumber, ruleToPort.Id);

                    if (_options.DryRun)
                    {
                        _log.Info(Resources.InfoDryRunIssueNotUpdated);
                    }
                    else
                    {
                        await UpdateIssueAsync(ruleToPort, existingIssue);
                    }
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

        private async Task FileIssueAsync(PortingInfo ruleToPort, string title)
        {
            NewIssue newIssue = CreateNewIssue(ruleToPort, title);
            Issue issue = await _issuesClient.Create(_options.RepoOwner, _options.RepoName, newIssue);
            _log.InfoFormat(Resources.InfoIssueCreated, issue.Number);
        }

        private async Task UpdateIssueAsync(PortingInfo ruleToPort, Issue existingIssue)
        {
            int issueNumber = existingIssue.Number;

            IssueUpdate issueUpdate = CreateIssueUpdate(ruleToPort, existingIssue);
            await _issuesClient.Update(_options.RepoOwner, _options.RepoName, issueNumber, issueUpdate);

            // The GitHub Issues API doesn't let you add or remove individual labels in the course of
            // an Update operation. Use the IssuesLabels API to do that.
            await UpdateIssueLabelsAsync(ruleToPort, existingIssue);

            _log.InfoFormat(Resources.InfoIssueUpdated, issueNumber);
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
            AddLabel(newIssue.Labels, FxCopPortLabel);

            switch (ruleToPort.Disposition)
            {
                case Disposition.NeedsReview:
                    AddLabel(newIssue.Labels, NeedsReviewLabel);
                    break;

                case Disposition.Cut:
                    AddLabel(newIssue.Labels, CutLabel);
                    break;
            }

            newIssue.Body = FormatIssueBody(ruleToPort);

            return newIssue;
        }

        private IssueUpdate CreateIssueUpdate(PortingInfo ruleToPort, Issue existingIssue)
        {
            IssueUpdate issueUpdate = existingIssue.ToUpdate();
            issueUpdate.Body = FormatIssueBody(ruleToPort);

            if (existingIssue.State == ItemState.Open && ruleToPort.Disposition == Disposition.Cut)
            {
                _log.InfoFormat(Resources.InfoClosingIssue, existingIssue.Number);
                issueUpdate.State = ItemState.Closed;
            }

            if (existingIssue.State == ItemState.Closed && ruleToPort.Disposition != Disposition.Cut)
            {
                _log.InfoFormat(Resources.InfoReopeningIssue, existingIssue.Number);
                issueUpdate.State = ItemState.Open;
            }

            return issueUpdate;
        }

        private async Task UpdateIssueLabelsAsync(PortingInfo ruleToPort, Issue existingIssue)
        {
            var existingLabelNames = existingIssue.Labels.Select(label => label.Name);

            var labelNamesToAdd = new Collection<string>();
            var labelNamesToRemove = new Collection<string>();

            AddLabel(labelNamesToAdd, FxCopPortLabel);

            switch (ruleToPort.Disposition)
            {
                case Disposition.NeedsReview:
                    AddLabel(labelNamesToAdd, NeedsReviewLabel);
                    RemoveLabel(labelNamesToRemove, CutLabel);
                    break;

                case Disposition.Port:
                    RemoveLabel(labelNamesToAdd, NeedsReviewLabel);
                    RemoveLabel(labelNamesToRemove, CutLabel);
                    break;

                case Disposition.Cut:
                    AddLabel(labelNamesToAdd, CutLabel);
                    RemoveLabel(labelNamesToAdd, NeedsReviewLabel);
                    break;
            }

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

        private void AddLabel(Collection<string> labels, string label)
        {
            if (!labels.Contains(label))
            {
                labels.Add(label);
                _log.InfoFormat(Resources.InfoAddingLabel, label);
            }
        }

        private void RemoveLabel(Collection<string> labels, string label)
        {
            if (labels.Contains(label))
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
