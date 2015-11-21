using Octokit;

namespace FileIssues
{
    internal static class GitHub
    {
        private const string ApplicationName = "RoslynAnalyzerIssueFiler";

        internal static IIssuesClient GetIssuesClient(string token)
        {
            var gitHub = new GitHubClient(new ProductHeaderValue(ApplicationName));
            gitHub.Connection.Credentials = new Credentials(token);
            return gitHub.Issue;
        }
    }
}
