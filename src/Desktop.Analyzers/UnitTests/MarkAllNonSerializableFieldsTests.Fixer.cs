// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public class MarkAllNonSerializableFieldsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicMarkAllNonSerializableFieldsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpMarkAllNonSerializableFieldsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicMarkAllNonSerializableFieldsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpMarkAllNonSerializableFieldsFixer();
        }
    }
}