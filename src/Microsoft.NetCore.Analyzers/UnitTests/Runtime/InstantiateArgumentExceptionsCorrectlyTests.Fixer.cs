// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetCore.CSharp.Analyzers.Runtime;
using Microsoft.NetCore.VisualBasic.Analyzers.Runtime;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class InstantiateArgumentExceptionsCorrectlyFixerTests : CodeFixTestBase
    {
        private static readonly string s_noArguments = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageNoArguments;
        private static readonly string s_incorrectMessage = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectMessage;
        private static readonly string s_incorrectParameterName = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectParameterName;
        private static readonly string s_swappedMessageAndParameterName = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageSwappedMessageAndParameterName;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new InstantiateArgumentExceptionsCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InstantiateArgumentExceptionsCorrectlyAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicInstantiateArgumentExceptionsCorrectlyFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpInstantiateArgumentExceptionsCorrectlyFixer();
        }

        [Fact]
        public void ArgumentException_NoArguments_Warns()
        {
            string oldSourceCS = @"
public class Class
{
    public void Test(string first)
    {
        throw new System.ArgumentException();
    }
}";

            string newSourceCS0 = oldSourceCS.Replace(
                @"new System.ArgumentException()",
                @"new System.ArgumentException(""message"")");
            string newSourceCS1 = oldSourceCS.Replace(
                @"new System.ArgumentException()",
                @"new System.ArgumentException(""message"", ""paramName"")");

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 15, s_noArguments, "ArgumentException"));

            VerifyCSharpFix(oldSourceCS, newSourceCS0, codeFixIndex: 0);
            VerifyCSharpFix(oldSourceCS, newSourceCS1, codeFixIndex: 1);

            string oldSourceVB = @"
Public Class [MyClass]
    Public Sub Test(first As String)
        Throw New System.ArgumentException()
    End Sub
End Class";

            string newSourceVB0 = oldSourceVB.Replace(
                @"New System.ArgumentException()",
                @"New System.ArgumentException(""message"")");
            string newSourceVB1 = oldSourceVB.Replace(
                @"New System.ArgumentException()",
                @"New System.ArgumentException(""message"", ""paramName"")");

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 15, s_noArguments, "ArgumentException"));

            VerifyBasicFix(oldSourceVB, newSourceVB0, codeFixIndex: 0);
            VerifyBasicFix(oldSourceVB, newSourceVB1, codeFixIndex: 1);
        }

        [Fact]
        public void ArgumentNullException_NoArguments_Warns()
        {
            string oldSourceCS = @"
public class Class
{
    public void Test(string first)
    {
        throw new System.ArgumentNullException();
    }
}";

            string newSourceCS0 = oldSourceCS.Replace(
                @"new System.ArgumentNullException()",
                @"new System.ArgumentNullException(""paramName"")");
            string newSourceCS1 = oldSourceCS.Replace(
                @"new System.ArgumentNullException()",
                @"new System.ArgumentNullException(""paramName"", ""message"")");

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 15, s_noArguments, "ArgumentNullException"));

            VerifyCSharpFix(oldSourceCS, newSourceCS0, codeFixIndex: 0, onlyFixFirstFixableDiagnostic: true);
            VerifyCSharpFix(oldSourceCS, newSourceCS1, codeFixIndex: 1);

            string oldSourceVB = @"
Public Class [MyClass]
    Public Sub Test(first As String)
        Throw New System.ArgumentNullException()
    End Sub
End Class";

            string newSourceVB0 = oldSourceVB.Replace(
                @"New System.ArgumentNullException()",
                @"New System.ArgumentNullException(""paramName"")");
            string newSourceVB1 = oldSourceVB.Replace(
                @"New System.ArgumentNullException()",
                @"New System.ArgumentNullException(""paramName"", ""message"")");

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 15, s_noArguments, "ArgumentNullException"));

            VerifyBasicFix(oldSourceVB, newSourceVB0, codeFixIndex: 0, onlyFixFirstFixableDiagnostic: true);
            VerifyBasicFix(oldSourceVB, newSourceVB1, codeFixIndex: 1);
        }

        [Fact]
        public void ArgumentOutOfRangeException_NoArguments_Warns()
        {
            string oldSourceCS = @"
public class Class
{
    public void Test(string first)
    {
        throw new System.ArgumentOutOfRangeException();
    }
}";

            string newSourceCS0 = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException()",
                @"new System.ArgumentOutOfRangeException(""paramName"")");
            string newSourceCS1 = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException()",
                @"new System.ArgumentOutOfRangeException(""paramName"", ""message"")");

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 15, s_noArguments, "ArgumentOutOfRangeException"));

            VerifyCSharpFix(oldSourceCS, newSourceCS0, codeFixIndex: 0, onlyFixFirstFixableDiagnostic: true);
            VerifyCSharpFix(oldSourceCS, newSourceCS1, codeFixIndex: 1);

            string oldSourceVB = @"
Public Class [MyClass]
    Public Sub Test(first As String)
        Throw New System.ArgumentOutOfRangeException()
    End Sub
End Class";

            string newSourceVB0 = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException()",
                @"New System.ArgumentOutOfRangeException(""paramName"")");
            string newSourceVB1 = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException()",
                @"New System.ArgumentOutOfRangeException(""paramName"", ""message"")");

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 15, s_noArguments, "ArgumentOutOfRangeException"));

            VerifyBasicFix(oldSourceVB, newSourceVB0, codeFixIndex: 0, onlyFixFirstFixableDiagnostic: true);
            VerifyBasicFix(oldSourceVB, newSourceVB1, codeFixIndex: 1);
        }

        [Fact]
        public void DuplicateWaitObjectException_NoArguments_Warns()
        {
            string oldSourceCS = @"
public class Class
{
    public void Test(string first)
    {
        throw new System.DuplicateWaitObjectException();
    }
}";

            string newSourceCS0 = oldSourceCS.Replace(
                @"new System.DuplicateWaitObjectException()",
                @"new System.DuplicateWaitObjectException(""parameterName"")");
            string newSourceCS1 = oldSourceCS.Replace(
                @"new System.DuplicateWaitObjectException()",
                @"new System.DuplicateWaitObjectException(""parameterName"", ""message"")");

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 15, s_noArguments, "DuplicateWaitObjectException"));

            VerifyCSharpFix(oldSourceCS, newSourceCS0, codeFixIndex: 0, onlyFixFirstFixableDiagnostic: true);
            VerifyCSharpFix(oldSourceCS, newSourceCS1, codeFixIndex: 1);

            string oldSourceVB = @"
Public Class [MyClass]
    Public Sub Test(first As String)
        Throw New System.DuplicateWaitObjectException()
    End Sub
End Class";

            string newSourceVB0 = oldSourceVB.Replace(
                @"New System.DuplicateWaitObjectException()",
                @"New System.DuplicateWaitObjectException(""parameterName"")");
            string newSourceVB1 = oldSourceVB.Replace(
                @"New System.DuplicateWaitObjectException()",
                @"New System.DuplicateWaitObjectException(""parameterName"", ""message"")");

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 15, s_noArguments, "DuplicateWaitObjectException"));

            VerifyBasicFix(oldSourceVB, newSourceVB0, codeFixIndex: 0, onlyFixFirstFixableDiagnostic: true);
            VerifyBasicFix(oldSourceVB, newSourceVB1, codeFixIndex: 1);
        }

        [Fact]
        public void InsideIfStatementCondition_NoArguments_OffersNameOf()
        {
            string oldSourceCS = @"
public class Class
{
    public void Test(int index, string s)
    {
        if (index < 0 || index >= s.Length)
        {
            throw new System.ArgumentOutOfRangeException();
        }
    }
}";

            string newSourceCS0 = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException()",
                @"new System.ArgumentOutOfRangeException(nameof(index))");
            string newSourceCS1 = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException()",
                @"new System.ArgumentOutOfRangeException(nameof(s))");
            string newSourceCS2 = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException()",
                @"new System.ArgumentOutOfRangeException(nameof(index), ""message"")");
            string newSourceCS3 = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException()",
                @"new System.ArgumentOutOfRangeException(nameof(s), ""message"")");

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(8, 19, s_noArguments, "ArgumentOutOfRangeException"));

            VerifyCSharpFix(oldSourceCS, newSourceCS0, codeFixIndex: 0);
            VerifyCSharpFix(oldSourceCS, newSourceCS1, codeFixIndex: 1);
            VerifyCSharpFix(oldSourceCS, newSourceCS2, codeFixIndex: 2);
            VerifyCSharpFix(oldSourceCS, newSourceCS3, codeFixIndex: 3);

            string oldSourceVB = @"
Public Class [MyClass]
    Public Sub Test(index As Integer, s As String)
        If index < 0 AndAlso index > s.Length
            Throw New System.ArgumentOutOfRangeException()
        End If
    End Sub
End Class";

            string newSourceVB0 = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException()",
                @"New System.ArgumentOutOfRangeException(NameOf(index))");
            string newSourceVB1 = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException()",
                @"New System.ArgumentOutOfRangeException(NameOf(s))");
            string newSourceVB2 = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException()",
                @"New System.ArgumentOutOfRangeException(NameOf(index), ""message"")");
            string newSourceVB3 = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException()",
                @"New System.ArgumentOutOfRangeException(NameOf(s), ""message"")");

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(5, 19, s_noArguments, "ArgumentOutOfRangeException"));

            VerifyBasicFix(oldSourceVB, newSourceVB0, codeFixIndex: 0);
            VerifyBasicFix(oldSourceVB, newSourceVB1, codeFixIndex: 1);
            VerifyBasicFix(oldSourceVB, newSourceVB2, codeFixIndex: 2);
            VerifyBasicFix(oldSourceVB, newSourceVB3, codeFixIndex: 3);
        }

        [Fact]
        public void ArgumentException_ParameterNameAsMessage_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentException(""first"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_incorrectMessage, "Test", "first", "message", "ArgumentException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentException(""first"")",
                @"new System.ArgumentException(""message"", ""first"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentException(""first"")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_incorrectMessage, "Test", "first", "message", "ArgumentException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.ArgumentException(""first"")",
                @"New System.ArgumentException(""message"", ""first"")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void ArgumentException_ReversedArguments_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentException(""first"", ""first is incorrect"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_swappedMessageAndParameterName, "Test", "message", "paramName", "ArgumentException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentException(""first"", ""first is incorrect"")",
                @"new System.ArgumentException(""first is incorrect"", ""first"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentException(""first"", ""first is incorrect"")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_swappedMessageAndParameterName, "Test", "message", "paramName", "ArgumentException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.ArgumentException(""first"", ""first is incorrect"")",
                @"New System.ArgumentException(""first is incorrect"", ""first"")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void ArgumentNullException_EmptyParameterNameArgument_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException("""");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Test", "", "paramName", "ArgumentNullException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentNullException("""")",
                @"new System.ArgumentNullException(""paramName"", """")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentNullException("""")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Test", "", "paramName", "ArgumentNullException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.ArgumentNullException("""")",
                @"New System.ArgumentNullException(""paramName"", """")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void ArgumentNullException_SpaceParameterNameArgument_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException("" "");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Test", " ", "paramName", "ArgumentNullException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentNullException("" "")",
                @"new System.ArgumentNullException(""paramName"", "" "")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentNullException("" "")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Test", " ", "paramName", "ArgumentNullException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.ArgumentNullException("" "")",
                @"New System.ArgumentNullException(""paramName"", "" "")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void ArgumentNullException_MessageAsParameterName_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException(""first is null"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Test", "first is null", "paramName", "ArgumentNullException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentNullException(""first is null"")",
                @"new System.ArgumentNullException(""paramName"", ""first is null"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);
        }

        [Fact]
        public void ArgumentNullException_ReversedArguments_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException(""first is null"", ""first"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_swappedMessageAndParameterName, "Test", "message", "paramName", "ArgumentNullException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentNullException(""first is null"", ""first"")",
                @"new System.ArgumentNullException(""first"", ""first is null"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentNullException(""first is null"", ""first"")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_swappedMessageAndParameterName, "Test", "message", "paramName", "ArgumentNullException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.ArgumentNullException(""first is null"", ""first"")",
                @"New System.ArgumentNullException(""first"", ""first is null"")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void ArgumentOutOfRangeException_MessageAsParameterName_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentOutOfRangeException(""first is out of range"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Test", "first is out of range", "paramName", "ArgumentOutOfRangeException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException(""first is out of range"")",
                @"new System.ArgumentOutOfRangeException(""paramName"", ""first is out of range"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentOutOfRangeException(""first is out of range"")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Test", "first is out of range", "paramName", "ArgumentOutOfRangeException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException(""first is out of range"")",
                @"New System.ArgumentOutOfRangeException(""paramName"", ""first is out of range"")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void ArgumentOutOfRangeException_ReversedArguments_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentOutOfRangeException(""first is out of range"", ""first"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_swappedMessageAndParameterName, "Test", "message", "paramName", "ArgumentOutOfRangeException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.ArgumentOutOfRangeException(""first is out of range"", ""first"")",
                @"new System.ArgumentOutOfRangeException(""first"", ""first is out of range"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentOutOfRangeException(""first is out of range"", ""first"")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_swappedMessageAndParameterName, "Test", "message", "paramName", "ArgumentOutOfRangeException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.ArgumentOutOfRangeException(""first is out of range"", ""first"")",
                @"New System.ArgumentOutOfRangeException(""first"", ""first is out of range"")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void DuplicateWaitObjectException_MessageAsParameterName_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.DuplicateWaitObjectException(""first is duplicate"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Test", "first is duplicate", "parameterName", "DuplicateWaitObjectException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.DuplicateWaitObjectException(""first is duplicate"")",
                @"new System.DuplicateWaitObjectException(""parameterName"", ""first is duplicate"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.DuplicateWaitObjectException(""first is duplicate"")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Test", "first is duplicate", "parameterName", "DuplicateWaitObjectException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.DuplicateWaitObjectException(""first is duplicate"")",
                @"New System.DuplicateWaitObjectException(""parameterName"", ""first is duplicate"")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        [Fact]
        public void DuplicateWaitObjectException_ReversedArguments_Warns()
        {
            string oldSourceCS = @"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.DuplicateWaitObjectException(""first is duplicate"", ""first"");
                    }
                }";

            VerifyCSharp(oldSourceCS,
                GetCSharpExpectedResult(6, 31, s_swappedMessageAndParameterName, "Test", "message", "parameterName", "DuplicateWaitObjectException"));

            string newSourceCS = oldSourceCS.Replace(
                @"new System.DuplicateWaitObjectException(""first is duplicate"", ""first"")",
                @"new System.DuplicateWaitObjectException(""first"", ""first is duplicate"")");

            VerifyCSharpFix(oldSourceCS, newSourceCS);

            string oldSourceVB = @"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.DuplicateWaitObjectException(""first is duplicate"", ""first"")
                    End Sub
                End Class";

            VerifyBasic(oldSourceVB,
                GetBasicExpectedResult(4, 31, s_swappedMessageAndParameterName, "Test", "message", "parameterName", "DuplicateWaitObjectException"));

            string newSourceVB = oldSourceVB.Replace(
                @"New System.DuplicateWaitObjectException(""first is duplicate"", ""first"")",
                @"New System.DuplicateWaitObjectException(""first"", ""first is duplicate"")");

            VerifyBasicFix(oldSourceVB, newSourceVB);
        }

        private static DiagnosticResult GetCSharpExpectedResult(int line, int column, string format, params string[] args)
        {
            string message = string.Format(format, args);
            return GetCSharpResultAt(line, column, InstantiateArgumentExceptionsCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicExpectedResult(int line, int column, string format, params string[] args)
        {
            string message = string.Format(format, args);
            return GetBasicResultAt(line, column, InstantiateArgumentExceptionsCorrectlyAnalyzer.RuleId, message);
        }
    }
}