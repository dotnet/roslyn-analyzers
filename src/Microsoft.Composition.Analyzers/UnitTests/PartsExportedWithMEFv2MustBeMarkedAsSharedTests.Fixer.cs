// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Composition.CSharp.Analyzers;
using Microsoft.Composition.VisualBasic.Analyzers;
using Test.Utilities;

namespace Microsoft.Composition.Analyzers.UnitTests
{
    public class PartsExportedWithMEFv2MustBeMarkedAsSharedFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new PartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicPartsExportedWithMEFv2MustBeMarkedAsSharedFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpPartsExportedWithMEFv2MustBeMarkedAsSharedFixer();
        }
    }
}