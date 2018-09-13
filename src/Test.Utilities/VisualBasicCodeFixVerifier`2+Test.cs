// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace Test.Utilities
{
    public static partial class VisualBasicCodeFixVerifier<TAnalyzer, TCodeFix>
    {
#pragma warning disable CA1724 // Type names should not match namespaces
#pragma warning disable CA1034 // Nested types should not be visible
        public class Test : VisualBasicCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
#pragma warning restore CA1034 // Nested types should not be visible
#pragma warning restore CA1724 // Type names should not match namespaces
        {
            public Test()
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    if (IncludeSystemData)
                    {
                        solution = solution.AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(System.Data.DataSet).Assembly.Location));
                    }

                    return solution;
                });
            }

            public bool IncludeSystemData { get; set; } = true;
        }
    }
}
