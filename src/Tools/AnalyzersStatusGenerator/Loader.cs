// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace AnalyzersStatusGenerator
{
    /// <summary>
    /// An assembly loader that simply loads using the LoadFrom context.
    /// </summary>
    public sealed class Loader : IAnalyzerAssemblyLoader
    {
        public void AddDependencyLocation(string fullPath)
        {
        }

        public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
    }
}
