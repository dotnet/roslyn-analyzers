// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.CSharp.Analyzers.Runtime;
using Microsoft.NetCore.VisualBasic.Analyzers.Runtime;
using Test.Utilities;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ImplementSerializationMethodsCorrectlyTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicImplementSerializationMethodsCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpImplementSerializationMethodsCorrectlyAnalyzer();
        }
    }
}