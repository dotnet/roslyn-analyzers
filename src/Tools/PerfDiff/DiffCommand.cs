// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.CommandLine;
using System.Threading.Tasks;

namespace PerfDiff
{
    internal static class DiffCommand
    {
        // This delegate should be kept in Sync with the FormatCommand options and argument names
        // so that values bind correctly.
        internal delegate Task<int> Handler(
            string baseline,
            string results,
            string? verbosity,
            bool failOnRegression,
            IConsole console);

        internal static string[] VerbosityLevels => new[] { "q", "quiet", "m", "minimal", "n", "normal", "d", "detailed", "diag", "diagnostic" };

        internal static RootCommand CreateCommandLineOptions()
        {
            // Sync changes to option and argument names with the FormatCommant.Handler above.
            var rootCommand = new RootCommand
            {
                new Option<string?>("--baseline", () => null, "folder that contains the baseline performance run data").LegalFilePathsOnly(),
                new Option<string?>("--results", () => null, "folder that contains the performance restults").LegalFilePathsOnly(),
                new Option<string>(new[] { "--verbosity", "-v" }, "Set the verbosity level. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]").FromAmong(VerbosityLevels),
                new Option<bool>(new[] { "--failOnRegression" }, "Should return non-zero exit code if regression detected"),
            };

            rootCommand.Description = "diff two sets of performance results";

            return rootCommand;
        }
    }
}
