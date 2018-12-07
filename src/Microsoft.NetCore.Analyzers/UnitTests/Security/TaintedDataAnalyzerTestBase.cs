// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
    public abstract class TaintedDataAnalyzerTestBase : DiagnosticAnalyzerTestBase
    {
        protected abstract DiagnosticDescriptor Rule { get; }

        protected DiagnosticResult GetCSharpResultAt(int sinkLine, int sinkColumn, int sourceLine, int sourceColumn, string sink, string sinkContainingMethod, string source, string sourceContainingMethod)
        {
            this.PrintActualDiagnosticsOnFailure = true;
            return GetCSharpResultAt(
                new[] {
                    Tuple.Create(sinkLine, sinkColumn),
                    Tuple.Create(sourceLine, sourceColumn)
                },
                this.Rule,
                sink,
                sinkContainingMethod,
                source,
                sourceContainingMethod);
        }

        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            this.VerifyCSharp(source, ReferenceFlags.AddTestReferenceAssembly, expected);
        }

        protected DiagnosticResult GetBasicResultAt(int sinkLine, int sinkColumn, int sourceLine, int sourceColumn, string sink, string sinkContainingMethod, string source, string sourceContainingMethod)
        {
            this.PrintActualDiagnosticsOnFailure = true;
            return GetBasicResultAt(
                new[] {
                    Tuple.Create(sinkLine, sinkColumn),
                    Tuple.Create(sourceLine, sourceColumn)
                },
                this.Rule,
                sink,
                sinkContainingMethod,
                source,
                sourceContainingMethod);
        }

        protected void VerifyBasicWithDependencies(string source, params DiagnosticResult[] expected)
        {
            this.VerifyBasic(source, ReferenceFlags.AddTestReferenceAssembly, expected);
        }
    }
}
