// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class DoNotRaiseReservedExceptionTypesFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotRaiseReservedExceptionTypesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotRaiseReservedExceptionTypesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotRaiseReservedExceptionTypesFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotRaiseReservedExceptionTypesFixer();
        }
    }
}