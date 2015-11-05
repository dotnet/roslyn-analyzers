// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public class MarkISerializableTypesWithSerializableFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicMarkISerializableTypesWithSerializableAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpMarkISerializableTypesWithSerializableAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicMarkISerializableTypesWithSerializableFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpMarkISerializableTypesWithSerializableFixer();
        }
    }
}