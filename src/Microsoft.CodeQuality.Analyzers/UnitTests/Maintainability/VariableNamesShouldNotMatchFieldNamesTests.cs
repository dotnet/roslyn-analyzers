// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.Maintainability;
using Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability;
using Test.Utilities;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class VariableNamesShouldNotMatchFieldNamesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicVariableNamesShouldNotMatchFieldNamesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpVariableNamesShouldNotMatchFieldNamesAnalyzer();
        }
    }
}