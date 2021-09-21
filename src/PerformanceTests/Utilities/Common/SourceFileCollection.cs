// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PerformanceTests.Utilities
{
    public class SourceFileCollection : List<(string filename, SourceText content)>
    {
        public void Add((string filename, string content) file)
        {
            Add((file.filename, SourceText.From(file.content)));
        }

        public void Add((Type sourceGeneratorType, string filename, string content) file)
        {
            var contentWithEncoding = SourceText.From(file.content, Encoding.UTF8);
            Add((file.sourceGeneratorType, file.filename, contentWithEncoding));
        }

        public void Add((Type sourceGeneratorType, string filename, SourceText content) file)
        {
            var generatedPath = Path.Combine(file.sourceGeneratorType.GetTypeInfo().Assembly.GetName().Name ?? string.Empty, file.sourceGeneratorType.FullName!, file.filename);
            Add((generatedPath, file.content));
        }
    }
}
