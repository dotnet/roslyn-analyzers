// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class DoNotRaiseExceptionsInExceptionClausesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotRaiseExceptionsInExceptionClausesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotRaiseExceptionsInExceptionClausesAnalyzer();
        }

        [Fact]
        public void CSharpSimpleCase()
        {
            var code = @"
using System;

public class Test
{
    public void Method()
    {
        try
        {
            throw new Exception();
        }
        catch (ArgumentException e)
        {
            throw new Exception();
        }
        catch
        {
            throw new Exception();
        }
        finally
        {
            throw new Exception();
        }
    
    }
}
";
            VerifyCSharp(code,
                GetCSharpResultAt(20, 13, DoNotRaiseExceptionsInExceptionClausesAnalyzer.FinallyRule));
        }

        [Fact]
        public void BasicSimpleCase()
        {
            var code = @"
Imports System

Public Class Test
    Public Sub Method()
        Try
            Throw New Exception()
        Catch e As ArgumentException
            Throw New Exception()
        Catch
            Throw New Exception()
        Finally
            Throw New Exception()
        End Try    
    End Sub
End Class
";
            VerifyBasic(code,
                GetBasicResultAt(20, 13, DoNotRaiseExceptionsInExceptionClausesAnalyzer.FinallyRule));
        }
    }
}