// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class InstantiateArgumentExceptionsCorrectlyTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string s_ruleId = InstantiateArgumentExceptionsCorrectlyAnalyzer.RuleId;
        private static readonly string s_noArguments = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageNoArguments;
        private static readonly string s_incorrectMessage = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectMessage;
        private static readonly string s_incorrectParameterName = SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectParameterName;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new InstantiateArgumentExceptionsCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InstantiateArgumentExceptionsCorrectlyAnalyzer();
        }

        [Fact]
        public void ArgumentException_NoArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentException();
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_noArguments, "System.ArgumentException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentException()
                    End Sub
                End Class",
              GetBasicExpectedResult(4, 31, s_noArguments, "System.ArgumentException"));
        }

        [Fact]
        public void ArgumentException_EmptyParameterNameArgument_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException("""");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "", "paramName", "System.ArgumentNullException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentNullException("""")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "", "paramName", "System.ArgumentNullException"));
        }

        [Fact]
        public void ArgumentNullException_SpaceParameterArgument_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException("" "");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", " ", "paramName", "System.ArgumentNullException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentNullException("" "")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", " ", "paramName", "System.ArgumentNullException"));
        }

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
                GetCSharpExpectedResult(7, 31, s_incorrectParameterName, "Class.Test(string)", "foo", "paramName", "System.ArgumentNullException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Dim foo As New Object()
                        Throw New System.ArgumentNullException(NameOf(foo))
                    End Sub
                End Class",
                GetBasicExpectedResult(5, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "foo", "paramName", "System.ArgumentNullException"));
        }

        [Fact]
        public void ArgumentException_ParameterNameAsMessage_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentException(""first"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectMessage, "Class.Test(string)", "first", "message", "System.ArgumentException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentException(""first"")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectMessage, "Public Sub Test(first As String)", "first", "message", "System.ArgumentException"));
        }

        [Fact]
        public void ArgumentException_ReversedArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentException(""first"", ""first is incorrect"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectMessage, "Class.Test(string)", "first", "message", "System.ArgumentException"),
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "first is incorrect", "paramName", "System.ArgumentException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentException(""first"", ""first is incorrect"")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectMessage, "Public Sub Test(first As String)", "first", "message", "System.ArgumentException"),
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "first is incorrect", "paramName", "System.ArgumentException"));
        }

        [Fact]
        public void ArgumentNullException_NoArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException();
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_noArguments, "System.ArgumentNullException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentNullException()
                    End Sub
                End Class",
                 GetBasicExpectedResult(4, 31, s_noArguments, "System.ArgumentNullException"));
        }

        [Fact]
        public void ArgumentNullException_MessageAsParameterName_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException(""first is null"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "first is null", "paramName", "System.ArgumentNullException"));
        }

        [Fact]
        public void ArgumentNullException_ReversedArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentNullException(""first is null"", ""first"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "first is null", "paramName", "System.ArgumentNullException"),
                GetCSharpExpectedResult(6, 31, s_incorrectMessage, "Class.Test(string)", "first", "message", "System.ArgumentNullException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentNullException(""first is null"", ""first"")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "first is null", "paramName", "System.ArgumentNullException"),
                GetBasicExpectedResult(4, 31, s_incorrectMessage, "Public Sub Test(first As String)", "first", "message", "System.ArgumentNullException"));
        }

        [Fact]
        public void ArgumentOutOfRangeException_NoArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentOutOfRangeException();
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_noArguments, "System.ArgumentOutOfRangeException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentOutOfRangeException()
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_noArguments, "System.ArgumentOutOfRangeException"));
        }

        [Fact]
        public void ArgumentOutOfRangeException_MessageAsParameterName_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentOutOfRangeException(""first is out of range"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "first is out of range", "paramName", "System.ArgumentOutOfRangeException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentOutOfRangeException(""first is out of range"")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "first is out of range", "paramName", "System.ArgumentOutOfRangeException"));
        }

        [Fact]
        public void ArgumentOutOfRangeException_ReversedArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.ArgumentOutOfRangeException(""first is out of range"", ""first"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "first is out of range", "paramName", "System.ArgumentOutOfRangeException"),
                GetCSharpExpectedResult(6, 31, s_incorrectMessage, "Class.Test(string)", "first", "message", "System.ArgumentOutOfRangeException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.ArgumentOutOfRangeException(""first is out of range"", ""first"")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "first is out of range", "paramName", "System.ArgumentOutOfRangeException"),
                GetBasicExpectedResult(4, 31, s_incorrectMessage, "Public Sub Test(first As String)", "first", "message", "System.ArgumentOutOfRangeException"));
        }

        [Fact]
        public void DuplicateWaitObjectException_NoArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.DuplicateWaitObjectException();
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_noArguments, "System.DuplicateWaitObjectException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.DuplicateWaitObjectException()
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_noArguments, "System.DuplicateWaitObjectException"));
        }

        [Fact]
        public void DuplicateWaitObjectException_MessageAsParameterName_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.DuplicateWaitObjectException(""first is duplicate"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "first is duplicate", "parameterName", "System.DuplicateWaitObjectException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.DuplicateWaitObjectException(""first is duplicate"")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "first is duplicate", "parameterName", "System.DuplicateWaitObjectException"));
        }

        [Fact]
        public void DuplicateWaitObjectException_ReversedArguments_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public void Test(string first)
                    {
                        throw new System.DuplicateWaitObjectException(""first is duplicate"", ""first"");
                    }
                }",
                GetCSharpExpectedResult(6, 31, s_incorrectParameterName, "Class.Test(string)", "first is duplicate", "parameterName", "System.DuplicateWaitObjectException"),
                GetCSharpExpectedResult(6, 31, s_incorrectMessage, "Class.Test(string)", "first", "message", "System.DuplicateWaitObjectException"));

            VerifyBasic(@"
                Public Class MyClass
                    Public Sub Test(Dim first As String)
                        Throw New System.DuplicateWaitObjectException(""first is duplicate"", ""first"")
                    End Sub
                End Class",
                GetBasicExpectedResult(4, 31, s_incorrectParameterName, "Public Sub Test(first As String)", "first is duplicate", "parameterName", "System.DuplicateWaitObjectException"),
                GetBasicExpectedResult(4, 31, s_incorrectMessage, "Public Sub Test(first As String)", "first", "message", "System.DuplicateWaitObjectException"));
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
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
                Public Class MyClass
                    Public Sub Test(Dim first As String)
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
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
               Public Class MyClass
                   Public Sub Test(Dim first As String)
                       Throw New System.DuplicateWaitObjectException(""first"", ""first is duplicate"")
                   End Sub
               End Class");
        }

        private static DiagnosticResult GetCSharpExpectedResult(int line, int column, string format, params string[] args)
        {
            string message = string.Format(format, args);
            return GetCSharpResultAt(line, column, s_ruleId, message);
        }

        private static DiagnosticResult GetBasicExpectedResult(int line, int column, string format, params string[] args)
        {
            string message = string.Format(format, args);
            return GetBasicResultAt(line, column, s_ruleId, message);
        }
    }
}