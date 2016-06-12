// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices.CSharp.Analyzers;
using System.Runtime.InteropServices.VisualBasic.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace System.Runtime.InteropServices.Analyzers.UnitTests
{
    public class AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new CSharpAlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BasicAlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer();
        }
    }
}