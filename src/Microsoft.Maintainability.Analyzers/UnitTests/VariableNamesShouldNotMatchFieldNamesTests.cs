// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maintainability.CSharp.Analyzers;
using Microsoft.Maintainability.VisualBasic.Analyzers;
using Test.Utilities;

namespace Microsoft.Maintainability.Analyzers.UnitTests
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