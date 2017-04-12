// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maintainability.CSharp.Analyzers;
using Microsoft.Maintainability.VisualBasic.Analyzers;
using Test.Utilities;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class AvoidUninstantiatedInternalClassesFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidUninstantiatedInternalClassesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidUninstantiatedInternalClassesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicAvoidUninstantiatedInternalClassesFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpAvoidUninstantiatedInternalClassesFixer();
        }
    }
}