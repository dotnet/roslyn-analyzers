// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class UseIntegralOrStringArgumentForIndexersTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseIntegralOrStringArgumentForIndexersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseIntegralOrStringArgumentForIndexersAnalyzer();
        }

        [Fact]
        public void TestBasicUseIntegralOrStringArgumentForIndexersWarning1()
        {
            VerifyBasic(@"
    Imports System

    Public Class Months
        Private month() As String = {""Jan"", ""Feb"", ""...""}
        Default ReadOnly Property Item(index As Single) As String
            Get
                Return month(index)
            End Get
        End Property
    End Class
", CreateBasicResult(6, 35));
        }
        [Fact]
        public void TestBasicUseIntegralOrStringArgumentForIndexersNoWarning1()
        {
            VerifyBasic(@"
    Public Class Months
        Private month() As String = {""Jan"", ""Feb"", ""...""}
        Default ReadOnly Property Item(index As String) As String
            Get
                Return month(index)
            End Get
        End Property
    End Class
");
        }

        [Fact]
        public void TestCSharpUseIntegralOrStringArgumentForIndexersWarning1()
        {
            VerifyCSharp(@"
    public class Months
    {
        string[] month = new string[] {""Jan"", ""Feb"", ""...""};
        public string this[char index]
        {
            get
            {
                return month[index];
            }
        }
    }", CreateCSharpResult(5, 23));
        }

        [Fact]
        public void TestCSharpUseIntegralOrStringArgumentForIndexersNoWarning1()
        {
            VerifyCSharp(@"
    public class Months
    {
        string[] month = new string[] {""Jan"", ""Feb"", ""...""};
        public string this[int index]
        {
            get
            {
                return month[index];
            }
        }
    }");
        }

        [Fact]
        public void TestCSharpGenericIndexer()
        {
            VerifyCSharp(@"
    public class Months<T>
    {
        public string this[T index]
        {
            get
            {
                return null;
            }
        }
    }");
        }

        [Fact]
        public void TestBasicGenericIndexer()
        {
            VerifyBasic(@"
    Public Class Months(Of T)
        Default Public ReadOnly Property Item(index As T)
            Get
                Return Nothing
            End Get
        End Property
    End Class");
        }

        [Fact]
        public void TestCSharpEnumIndexer()
        {
            VerifyCSharp(@"
    public class Months<T>
    {
        public enum Foo { }

        public string this[Foo index]
        {
            get
            {
                return null;
            }
        }
    }");
        }

        [Fact]
        public void TestBasicEnumIndexer()
        {
            VerifyBasic(@"
    Public Class Months(Of T)
        Public Enum Foo
            Val1
        End Enum

        Default Public ReadOnly Property Item(index As Foo)
            Get
                Return Nothing
            End Get
        End Property
    End Class");
        }

        private static DiagnosticResult CreateCSharpResult(int line, int col)
        {
            return GetCSharpResultAt(line, col, UseIntegralOrStringArgumentForIndexersAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.UseIntegralOrStringArgumentForIndexersMessage);
        }

        private static DiagnosticResult CreateBasicResult(int line, int col)
        {
            return GetBasicResultAt(line, col, UseIntegralOrStringArgumentForIndexersAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.UseIntegralOrStringArgumentForIndexersMessage);
        }
    }
}