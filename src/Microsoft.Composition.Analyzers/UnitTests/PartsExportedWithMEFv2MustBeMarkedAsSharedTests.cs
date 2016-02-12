// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;

namespace Microsoft.Composition.Analyzers.UnitTests
{
    public class PartsExportedWithMEFv2MustBeMarkedAsSharedTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicPartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpPartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer();
        }
    }
}