// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PerfDiff.Logging;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PerfDiff
{
    internal sealed class Program
    {
        internal const int UnhandledExceptionExitCode = 1;
        private static ParseResult? s_parseResult;

        private static async Task<int> Main(string[] args)
        {
            var rootCommand = DiffCommand.CreateCommandLineOptions();
            rootCommand.Handler = CommandHandler.Create(new DiffCommand.Handler(RunAsync));

            // Parse the incoming args so we can give warnings when deprecated options are used.
            s_parseResult = rootCommand.Parse(args);

            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
        }

        public static async Task<int> RunAsync(
            string baseline,
            string results,
            string? verbosity,
            bool failOnRegression,
            IConsole console)
        {
            if (s_parseResult == null)
            {
                return 1;
            }

            // Setup logging.
            var logLevel = GetLogLevel(verbosity);
            var logger = SetupLogging(console, minimalLogLevel: logLevel, minimalErrorLevel: LogLevel.Warning);

            // Hook so we can cancel and exit when ctrl+c is pressed.
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            var currentDirectory = string.Empty;

            try
            {
                var exitCode = await PerfDiff.CompareAsync(baseline, results, failOnRegression, logger, cancellationTokenSource.Token).ConfigureAwait(false);
                return exitCode;
            }
            catch (FileNotFoundException fex)
            {
#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError(ILogger, string?, params object?[])'
#pragma warning disable CA2254 // The logging message template should not vary between calls to 'LoggerExtensions.LogError(ILogger, string?, params object?[])'
                logger.LogError(fex.Message);
#pragma warning restore CA2254 // The logging message template should not vary between calls to 'LoggerExtensions.LogError(ILogger, string?, params object?[])'
#pragma warning restore CA1848 // For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError(ILogger, string?, params object?[])'
                return UnhandledExceptionExitCode;
            }
            catch (OperationCanceledException)
            {
                return UnhandledExceptionExitCode;
            }
            finally
            {
                if (!string.IsNullOrEmpty(currentDirectory))
                {
                    Environment.CurrentDirectory = currentDirectory;
                }
            }

            static ILogger<Program> SetupLogging(IConsole console, LogLevel minimalLogLevel, LogLevel minimalErrorLevel)
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(new LoggerFactory().AddSimpleConsole(console, minimalLogLevel, minimalErrorLevel));
                serviceCollection.AddLogging();

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var logger = serviceProvider.GetService<ILogger<Program>>();

                return logger!;
            }

            static LogLevel GetLogLevel(string? verbosity)
                => verbosity switch
                {
                    "q" or "quiet" => LogLevel.Error,
                    "m" or "minimal" => LogLevel.Warning,
                    "n" or "normal" => LogLevel.Information,
                    "d" or "detailed" => LogLevel.Debug,
                    "diag" or "diagnostic" => LogLevel.Trace,
                    _ => LogLevel.Information,
                };
        }
    }
}
