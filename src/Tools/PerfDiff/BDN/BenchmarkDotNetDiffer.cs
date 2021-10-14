// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataTransferContracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Perfolizer.Mathematics.SignificanceTesting;
using Perfolizer.Mathematics.Thresholds;

namespace PerfDiff
{
    public static class BenchmarkDotNetDiffer
    {
        public static async Task<(bool compareSucceeded, bool regressionDetected)> TryCompareBenchmarkDotNetResultsAsync(string baselineFolder, string resultsFolder, ILogger logger)
        {
            bool regressionDetected = false;

            // search folder for benchmark dotnet results
            var comparison = await TryGetBdnResultsAsync(baselineFolder, resultsFolder, logger).ConfigureAwait(false);
            if (comparison is null)
            {
                return (false, regressionDetected);
            }

            // compare bdn results
            // TODO: let these be optional parameters
            _ = Threshold.TryParse("35%", out var testThreshold);

            var notSame = FindRegressions(comparison, testThreshold);

            if (!notSame.Any())
            {
                logger.LogInformation($"No differences found between the benchmark results with threshold {testThreshold}.");
                return (true, regressionDetected);
            }

            var better = notSame.Where(result => result.conclusion == EquivalenceTestConclusion.Faster);
            var worse = notSame.Where(result => result.conclusion == EquivalenceTestConclusion.Slower);
            var betterCount = better.Count();
            int worseCount = worse.Count();

            // If the baseline doesn't have the same set of tests, you wind up with Infinity in the list of diffs.
            // Exclude them for purposes of geomean.
            worse = worse.Where(x => GetRatio(x) != double.PositiveInfinity);
            better = better.Where(x => GetRatio(x) != double.PositiveInfinity);

            if (betterCount > 0)
            {
                var betterGeoMean = Math.Pow(10, better.Skip(1).Aggregate(Math.Log10(GetRatio(better.First())), (x, y) => x + Math.Log10(GetRatio(y))) / better.Count());
                logger.LogInformation($"better: {betterCount}, geomean: {betterGeoMean:F3}%");
                foreach (var (betterId, betterBaseResult, betterDiffResult, conclusion) in worse)
                {
                    var mean = GetRatio(conclusion, betterBaseResult, betterDiffResult);
                    logger.LogInformation($"' test: '{betterId}' tool '{mean:F3}' times less");
                }
            }

            if (worseCount > 0)
            {
                var worseGeoMean = Math.Pow(10, worse.Skip(1).Aggregate(Math.Log10(GetRatio(worse.First())), (x, y) => x + Math.Log10(GetRatio(y))) / worse.Count());
                logger.LogInformation($"' worse: {worseCount}, geomean: {worseGeoMean:F3}%");
                foreach (var (worseId, worseBaseResult, worseDiffResult, conclusion) in worse)
                {
                    var mean = GetRatio(conclusion, worseBaseResult, worseDiffResult);
                    logger.LogInformation($"' test: '{worseId}' took '{mean:F3}' times longer");
                }
            }

            if (worseCount > 0)
            {
                regressionDetected = true;
            }

            return (true, regressionDetected);
        }

        private static double GetRatio((string id, Benchmark baseResult, Benchmark diffResult, EquivalenceTestConclusion conclusion) item) => GetRatio(item.conclusion, item.baseResult, item.diffResult);

        private static double GetRatio(EquivalenceTestConclusion conclusion, Benchmark baseResult, Benchmark diffResult)
            => conclusion == EquivalenceTestConclusion.Faster
                ? baseResult.Statistics.Median / diffResult.Statistics.Median
                : diffResult.Statistics.Median / baseResult.Statistics.Median;

        private static async Task<(string id, Benchmark baseResult, Benchmark diffResult)[]?> TryGetBdnResultsAsync(
            string baselineFolder,
            string resultsFolder,
            ILogger logger)
        {
            if (!TryGetFilesToParse(baselineFolder, out var baseFiles))
            {
                logger.LogError($"Provided path does NOT exist or does not contain perf results '{baselineFolder}'");
                return null;
            }

            if (!TryGetFilesToParse(resultsFolder, out var resultsFiles))
            {
                logger.LogError($"Provided path does NOT exist or does not contain perf results '{resultsFolder}'");
                return null;
            }

            if (!baseFiles.Any() || !resultsFiles.Any())
            {
                logger.LogError($"Provided paths contained no '{FullBdnJsonFileExtension}' files.");
                return null;
            }

            var (baseResultsSuccess, baseResults) = await TryGetBdnResultAsync(baseFiles, logger).ConfigureAwait(false);
            if (!baseResultsSuccess)
            {
                return null;
            }

            var (resultsSuccess, diffResults) = await TryGetBdnResultAsync(resultsFiles, logger).ConfigureAwait(false);
            if (!resultsSuccess)
            {
                return null;
            }

            var benchmarkIdToDiffResults = diffResults
                .SelectMany(result => result.Benchmarks)
                .ToDictionary(benchmarkResult => benchmarkResult.FullName, benchmarkResult => benchmarkResult);

            var benchmarkIdToBaseResults = baseResults
                .SelectMany(result => result.Benchmarks)
                .ToDictionary(benchmarkResult => benchmarkResult.FullName, benchmarkResult => benchmarkResult); // we use ToDictionary to make sure the results have unique IDs

            return benchmarkIdToBaseResults
                .Where(baseResult => benchmarkIdToDiffResults.ContainsKey(baseResult.Key))
                .Select(baseResult => (id: baseResult.Key, baseResult: baseResult.Value, diffResult: benchmarkIdToDiffResults[baseResult.Key]))
                .ToArray();
        }

        private const string FullBdnJsonFileExtension = "full-compressed.json";

        private static bool TryGetFilesToParse(string path, [NotNullWhen(true)] out string[]? files)
        {
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, $"*{FullBdnJsonFileExtension}", SearchOption.AllDirectories);
                return true;
            }
            else if (File.Exists(path) || !path.EndsWith(FullBdnJsonFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                files = new[] { path };
                return true;
            }

            files = null;
            return false;
        }

        private static async Task<(bool success, BdnResult[] results)> TryGetBdnResultAsync(string[] paths, ILogger logger)
        {
            var results = await Task.WhenAll(paths.Select(path => ReadFromFileAsync(path, logger))).ConfigureAwait(false);
            return (!results.Any(x => x is null), results)!;
        }

        private static async Task<BdnResult?> ReadFromFileAsync(string resultFilePath, ILogger logger)
        {
            try
            {
                return JsonConvert.DeserializeObject<BdnResult>(await File.ReadAllTextAsync(resultFilePath).ConfigureAwait(false));
            }
            catch (JsonSerializationException)
            {
                logger.LogError($"Exception while reading the {resultFilePath} file.");
                return null;
            }
        }

        private static (string id, Benchmark baseResult, Benchmark diffResult, EquivalenceTestConclusion conclusion)[] FindRegressions((string id, Benchmark baseResult, Benchmark diffResult)[] comparison, Threshold testThreshold)
        {
            var results = new List<(string id, Benchmark baseResult, Benchmark diffResult, EquivalenceTestConclusion conclusion)>();
            foreach ((string id, Benchmark baseResult, Benchmark diffResult) in comparison
                .Where(result => result.baseResult.Statistics != null && result.diffResult.Statistics != null)) // failures
            {
                var baseValues = baseResult.GetOriginalValues();
                var diffValues = diffResult.GetOriginalValues();

                var userTresholdResult = StatisticalTestHelper.CalculateTost(MannWhitneyTest.Instance, baseValues, diffValues, testThreshold);
                if (userTresholdResult.Conclusion == EquivalenceTestConclusion.Same)
                    continue;

                results.Add((id, baseResult, diffResult, conclusion: userTresholdResult.Conclusion));
            }

            return results.ToArray();
        }
    }
}
