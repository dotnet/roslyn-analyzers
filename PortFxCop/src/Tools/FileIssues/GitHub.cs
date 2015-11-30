using System;
using Octokit;

namespace FileIssues
{
    internal static class GitHub
    {
        private const string ApplicationName = "RoslynAnalyzerIssueFiler";

        private static GitHubClient _gitHubClient;

        private static GitHubClient GetGitHubClient(string token)
        {
            if (_gitHubClient == null)
            {
                _gitHubClient = new GitHubClient(new ProductHeaderValue(ApplicationName));
                _gitHubClient.Connection.Credentials = new Credentials(token);
            }

            return _gitHubClient;
        }

        internal static IIssuesClient GetIssuesClient(string token)
        {
            return GetGitHubClient(token).Issue;
        }

        internal static IIssuesLabelsClient GetIssuesLabelsClient(string token)
        {
            return GetIssuesClient(token).Labels;
        }
    }
}
