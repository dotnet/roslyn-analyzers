// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public class ProvideDeserializationMethodsForOptionalFieldsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicProvideDeserializationMethodsForOptionalFieldsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpProvideDeserializationMethodsForOptionalFieldsAnalyzer();
        }
    }
}