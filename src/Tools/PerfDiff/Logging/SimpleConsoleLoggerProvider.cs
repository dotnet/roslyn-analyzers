// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace PerfDiff.Logging
{
    internal sealed class SimpleConsoleLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel _minimalLogLevel;
        private readonly LogLevel _minimalErrorLevel;

        public SimpleConsoleLoggerProvider(LogLevel minimalLogLevel, LogLevel minimalErrorLevel)
        {
            _minimalLogLevel = minimalLogLevel;
            _minimalErrorLevel = minimalErrorLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SimpleConsoleLogger(_minimalLogLevel, _minimalErrorLevel);
        }

        public void Dispose()
        {
        }
    }
}
