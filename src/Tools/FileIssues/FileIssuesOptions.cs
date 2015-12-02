using CommandLine;

namespace FileIssues
{
    public class FileIssuesOptions
    {
        [Option(
            'f',
            "rules-file-path",
            Required = true,
            HelpText = "The path to the CSV file containing the rule porting information")]
        public string RulesFilePath { get; set; }

        [Option(
            'o',
            "repo-owner",
            Required = true,
            HelpText = "The owner of the repo in which issues should be filed")]
        public string RepoOwner { get; set; }

        [Option(
            'r',
            "repo-name",
            Required = true,
            HelpText = "The name of the repo in which issues should be filed")]
        public string RepoName { get; set; }

        [Option(
            't',
            "token",
            Required = true,
            HelpText = "Your GitHub personal access token")]
        public string Token { get; set; }

        [Option(
            'd',
            "delay",
            HelpText = "Delay after each issue creation (msec)",
            Default = 120000)]
        // This prevents GitHub from failing multiple rapid issue creation requests
        // because it considers them "abuse".
        public int Delay { get; set; }

        [Option(
            'n',
            "dry-run",
            HelpText = "Don't actually do anything; just show what would be done.",
            Default = false)]
        public bool DryRun { get; set; }
    }
}
