// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace ReleaseNotesUtil
{
    internal sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
    {
        public static IAnalyzerAssemblyLoader Instance = new AnalyzerAssemblyLoader();

        private AnalyzerAssemblyLoader() { }

        public void AddDependencyLocation(string fullPath)
        {
        }

        public Assembly LoadFromPath(string fullPath)
        {
            return Assembly.LoadFrom(fullPath);
        }
    }
}
