// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.CommandLine;

namespace PerfDiff
{
    internal static class DiffCommand
    {
        internal static string[] VerbosityLevels => new[] { "q", "quiet", "m", "minimal", "n", "normal", "d", "detailed", "diag", "diagnostic" };

        internal static RootCommand CreateCommandLineOptions()
        {
            var baseline = new Option<string>("--baseline")
            {
                Description = "folder that contains the baseline performance run data"
            };
            baseline.AcceptLegalFilePathsOnly();
            var results = new Option<string>("--results")
            {
                Description = "folder that contains the performance restults"
            };
            results.AcceptLegalFilePathsOnly();
            var verbosity = new Option<string>("--verbosity", "-v")
            {
                Description = "Set the verbosity level. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]"
            };
            verbosity.AcceptOnlyFromAmong(VerbosityLevels);
            var failOnRegression = new Option<bool>("--failOnRegression")
            {
                Description = "Should return non-zero exit code if regression detected"
            };

            var rootCommand = new RootCommand
            {
                baseline,
                results,
                verbosity,
                failOnRegression
            };

            rootCommand.Description = "diff two sets of performance results";

            rootCommand.SetAction((parseResult, ct) => Program.RunAsync(
                baseline: parseResult.GetValue(baseline)!,
                results: parseResult.GetValue(results)!,
                verbosity: parseResult.GetValue(verbosity),
                failOnRegression: parseResult.GetValue(failOnRegression),
                token: ct
            ));

            return rootCommand;
        }
    }
}
