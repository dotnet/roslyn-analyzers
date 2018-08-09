// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.CSharp.Analyzers.Runtime;
using Microsoft.NetCore.VisualBasic.Analyzers.Runtime;
using Test.Utilities;

namespace Microsoft.NetFramework.Analyzers.UnitTests
{
    public class ProvideDeserializationMethodsForOptionalFieldsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicProvideDeserializationMethodsForOptionalFieldsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpProvideDeserializationMethodsForOptionalFieldsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicProvideDeserializationMethodsForOptionalFieldsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpProvideDeserializationMethodsForOptionalFieldsFixer();
        }
    }
}