// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;

namespace PerformanceTests.Utilities
{
    public class SourceFileList(string defaultPrefix, string defaultExtension) : SourceFileCollection
    {
        private readonly string _defaultPrefix = defaultPrefix;
        private readonly string _defaultExtension = defaultExtension;

        public void Add(string content)
        {
            Add(($"{_defaultPrefix}{Count}.{_defaultExtension}", content));
        }

        public void Add(SourceText content)
        {
            Add(($"{_defaultPrefix}{Count}.{_defaultExtension}", content));
        }
    }
}
