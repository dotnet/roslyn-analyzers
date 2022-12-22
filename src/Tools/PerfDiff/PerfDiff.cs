// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace PerfDiff
{
    public static partial class PerfDiff
    {
        public static async Task<int> CompareAsync(
            string baselineFolder, string resultsFolder, bool failOnRegression, ILogger logger, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var (compareSucceeded, regressionDetected) = await BenchmarkDotNetDiffer.TryCompareBenchmarkDotNetResultsAsync(baselineFolder, resultsFolder, logger).ConfigureAwait(false);

            if (!compareSucceeded)
            {
#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError(ILogger, string?, params object?[])'
                logger.LogError("Failed to compare the performance results see log.");
#pragma warning restore CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError(ILogger, string?, params object?[])'
                return 1;
            }

            if (!regressionDetected)
            {
#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
                logger.LogTrace("No performance regression found.");
#pragma warning restore CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
                return 0;
            }

            var (etlCompareSucceeded, etlRegressionDetected) = CheckEltTraces(baselineFolder, resultsFolder, failOnRegression);
            if (!etlCompareSucceeded)
            {
#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
                logger.LogTrace("We detected a regression in BMDN and there is no ETL info.");
#pragma warning restore CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
                return 1;
            }

            if (etlRegressionDetected)
            {
#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
                logger.LogTrace(" We detected a regression in BMDN and there _is_ ETL info which agress there was a regression.");
#pragma warning restore CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
                return 1;
            }

#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
            logger.LogTrace("We detected a regression in BMDN but examining the ETL trace determined that is was noise.");
#pragma warning restore CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogTrace(ILogger, string?, params object?[])'
            return 0;
        }

        private static (bool compareSucceeded, bool regressionDetected) CheckEltTraces(string baselineFolder, string resultsFolder, bool failOnRegression)
        {
            var regressionDetected = false;

            // try look for ETL traces 
            if (!TryGetETLPaths(baselineFolder, out var baselineEtlPath))
            {
                return (false, regressionDetected);
            }

            if (!TryGetETLPaths(resultsFolder, out var resultsEtlPath))
            {
                return (false, regressionDetected);
            }

            // Compare ETL
            if (!EtlDiffer.TryCompareETL(resultsEtlPath, baselineEtlPath, out regressionDetected))
            {
                return (false, regressionDetected);
            }

            if (regressionDetected && failOnRegression)
            {
                return (true, regressionDetected);
            }

            return (false, regressionDetected);
        }

        private const string ETLFileExtension = "etl.zip";

        private static bool TryGetETLPaths(string path, [NotNullWhen(true)] out string? etlPath)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, $"*{ETLFileExtension}", SearchOption.AllDirectories);
                etlPath = files.SingleOrDefault();
                if (etlPath is null)
                {
                    etlPath = null;
                    return false;
                }

                return true;
            }
            else if (File.Exists(path) || !path.EndsWith(ETLFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                etlPath = path;
                return true;
            }

            etlPath = null;
            return false;
        }
    }
}
