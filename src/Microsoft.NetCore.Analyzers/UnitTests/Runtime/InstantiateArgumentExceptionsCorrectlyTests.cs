// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class InstantiateArgumentExceptionsCorrectlyTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string s_incorrectParameterName = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectParameterName;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new InstantiateArgumentExceptionsCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InstantiateArgumentExceptionsCorrectlyAnalyzer();
        }

        // TODO: Offer appropriate codefix in this scenario. 'nameof(foo)' should not be treated like a message.
        [Fact]
        public void ArgumentNullException_NameofNonParameter_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        var foo = new object();
                        throw new System.ArgumentNullException(nameof(foo));
                    }
                }",
                GetCSharpExpectedResult(7, 31, s_incorrectParameterName, "Test", "foo", "paramName", "ArgumentNullException"));

            VerifyBasic(@"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Dim foo As New Object()
                        Throw New System.ArgumentNullException(NameOf(foo))
                    End Sub
                End Class",
                GetBasicExpectedResult(5, 31, s_incorrectParameterName, "Test", "foo", "paramName", "ArgumentNullException"));
        }


        [Fact]
        public void ArgumentException_CorrectMessage_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentException(""first is incorrect"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.ArgumentException(""first is incorrect"")
                   End Sub
               End Class");
        }

        [Fact]
        public void ArgumentException_CorrectMessageAndParameterName_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentException(""first is incorrect"", ""first"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.ArgumentException(""first is incorrect"", ""first"")
                   End Sub
               End Class");
        }

        [Fact]
        public void ArgumentNullException_CorrectParameterName_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException(""first"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.ArgumentNullException(""first"")
                   End Sub
               End Class");
        }


        [Fact]

        public void ArgumentNullException_NameofParameter_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException(nameof(first));
                    }
                }");

            VerifyBasic(@"
                Public Class [MyClass]
                    Public Sub Test(first As String)
                        Throw New System.ArgumentNullException(NameOf(first))
                    End Sub
                End Class");
        }

        [Fact]
        public void ArgumentNull_CorrectParameterNameAndMessage_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException(""first"", ""first is null"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.ArgumentNullException(""first"", ""first is null"")
                   End Sub
               End Class");
        }

        [Fact]
        public void ArgumentOutOfRangeException_CorrectParameterName_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentOutOfRangeException(""first"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.ArgumentOutOfRangeException(""first"")
                   End Sub
               End Class");
        }

        [Fact]
        public void ArgumentOutOfRangeException_CorrectParameterNameAndMessage_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentOutOfRangeException(""first"", ""first is out of range"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.DuplicateWaitObjectException(""first"", ""first is out of range"")
                   End Sub
               End Class");
        }

        [Fact]
        public void DuplicateWaitObjectException_CorrectParameterName_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.DuplicateWaitObjectException(""first"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.DuplicateWaitObjectException(""first"")
                   End Sub
               End Class");
        }

        [Fact]
        public void DuplicateWaitObjectException_CorrectParameterNameAndMessage_DoesNotWarn()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.DuplicateWaitObjectException(""first"", ""first is duplicate"");
                    }
                }");

            VerifyBasic(@"
               Public Class [MyClass]
                   Public Sub Test(first As String)
                       Throw New System.DuplicateWaitObjectException(""first"", ""first is duplicate"")
                   End Sub
               End Class");
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