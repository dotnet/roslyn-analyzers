// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class ParameterNamesShouldMatchBaseDeclarationTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void VerifyNoFalsePositivesAreReported()
        {
            VerifyCSharp(@"public class TestClass
                           {
                               public void TestMethod() { }
                           }");

            VerifyCSharp(@"public class TestClass
                           {
                               public void TestMethod(string arg1, string arg2) { }
                           }");

            VerifyCSharp(@"public class TestClass
                           {
                               public void TestMethod(string arg1, string arg2, __arglist) { }
                           }");

            VerifyCSharp(@"public class TestClass
                           {
                               public void TestMethod(string arg1, string arg2, params string[] arg3) { }
                           }");

            VerifyBasic(@"Public Class TestClass
                              Public Sub TestMethod()
                              End Sub
                          End Class");

            VerifyBasic(@"Public Class TestClass
                              Public Sub TestMethod(arg1 As String, arg2 As String) { }
                              End Sub
                          End Class");

            VerifyBasic(@"Public Class TestClass
                              Public Sub TestMethod(arg1 As String, arg2 As String, ParamArray arg3() As String)
                              End Sub
                          End Class");
        }

        [Fact]
        public void VerifyOverrideWithWrongParameterNames()
        {
            VerifyCSharp(@"public abstract class BaseClass
                           {
                               public abstract void TestMethod(string baseArg1, string baseArg2);
                           }

                           public class TestClass : BaseClass
                           {
                               public override void TestMethod(string arg1, string arg2);
                           }",
                         GetCSharpResultAt(8, 71),
                         GetCSharpResultAt(8, 84));

            VerifyCSharp(@"public abstract class BaseClass
                           {
                               public abstract void TestMethod(string baseArg1, string baseArg2, __arglist);
                           }

                           public class TestClass : BaseClass
                           {
                               public override void TestMethod(string arg1, string arg2, __arglist);
                           }",
                         GetCSharpResultAt(8, 71),
                         GetCSharpResultAt(8, 84));

            VerifyCSharp(@"public abstract class BaseClass
                           {
                               public abstract void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                           }

                           public class TestClass : BaseClass
                           {
                               public override void TestMethod(string arg1, string arg2, params string[] arg3);
                           }",
                         GetCSharpResultAt(8, 71),
                         GetCSharpResultAt(8, 84),
                         GetCSharpResultAt(8, 106));

            VerifyBasic(@"Public MustInherit Class BaseClass
                              Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String)
                          End Class

                          Public Class TestClass 
                              Inherits BaseClass

                              Public Overrides Sub TestMethod(arg1 as String, arg2 as String)
                              End Sub
                          End Class",
                         GetBasicResultAt(8, 63),
                         GetBasicResultAt(8, 79));

            VerifyBasic(@"Public MustInherit Class BaseClass
                              Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                          End Class

                          Public Class TestClass
                              Inherits BaseClass

                              Public Overrides Sub TestMethod(arg1 as String, arg2 as String, ParamArray arg3() As String)
                              End Sub
                          End Class",
                         GetBasicResultAt(8, 63),
                         GetBasicResultAt(8, 79),
                         GetBasicResultAt(8, 106));
        }

        [Fact]
        public void VerifyInterfaceImplementationWithWrongParameterNames()
        {
            VerifyCSharp(@"public interface IBase
                           {
                               void TestMethod(string baseArg1, string baseArg2);
                           }

                           public class TestClass : IBase
                           {
                               public void TestMethod(string arg1, string arg2);
                           }",
                         GetCSharpResultAt(8, 62),
                         GetCSharpResultAt(8, 75));

            VerifyCSharp(@"public interface IBase
                           {
                               void TestMethod(string baseArg1, string baseArg2, __arglist);
                           }

                           public class TestClass : IBase
                           {
                               public void TestMethod(string arg1, string arg2, __arglist);
                           }",
                         GetCSharpResultAt(8, 62),
                         GetCSharpResultAt(8, 75));

            VerifyCSharp(@"public interface IBase
                           {
                               void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                           }

                           public class TestClass : IBase
                           {
                               public void TestMethod(string arg1, string arg2, params string[] arg3);
                           }",
                         GetCSharpResultAt(8, 62),
                         GetCSharpResultAt(8, 75),
                         GetCSharpResultAt(8, 97));

            VerifyBasic(@"Public Interface IBase
                              Sub TestMethod(baseArg1 As String, baseArg2 As String)
                          End Interface

                          Public Class TestClass
                              Implements IBase

                              Public Sub TestMethod(arg1 As String, arg2 As String) Implements IBase.TestMethod
                              End Sub
                          End Class",
                        GetBasicResultAt(8, 53),
                        GetBasicResultAt(8, 69));

            VerifyBasic(@"Public Interface IBase
                              Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                          End Interface

                          Public Class TestClass
                              Implements IBase

                              Public Sub TestMethod(arg1 As String, arg2 As String, ParamArray arg3() As String) Implements IBase.TestMethod
                              End Sub
                          End Class",
                        GetBasicResultAt(8, 53),
                        GetBasicResultAt(8, 69),
                        GetBasicResultAt(8, 96));
        }

        [Fact]
        public void VerifyExplicitInterfaceImplementationWithWrongParameterNames()
        {
            VerifyCSharp(@"public interface IBase
                           {
                               void TestMethod(string baseArg1, string baseArg2);
                           }

                           public class TestClass : IBase
                           {
                               void IBase.TestMethod(string arg1, string arg2);
                           }",
                         GetCSharpResultAt(8, 61),
                         GetCSharpResultAt(8, 74));

            VerifyCSharp(@"public interface IBase
                           {
                               void TestMethod(string baseArg1, string baseArg2, __arglist);
                           }

                           public class TestClass : IBase
                           {
                               void IBase.TestMethod(string arg1, string arg2, __arglist);
                           }",
                         GetCSharpResultAt(8, 61),
                         GetCSharpResultAt(8, 74));

            VerifyCSharp(@"public interface IBase
                           {
                               void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                           }

                           public class TestClass : IBase
                           {
                               void IBase.TestMethod(string arg1, string arg2, params string[] arg3);
                           }",
                         GetCSharpResultAt(8, 61),
                         GetCSharpResultAt(8, 74),
                         GetCSharpResultAt(8, 96));
        }

        [Fact]
        public void VerifyInterfaceImplementationWithDifferentMethodName()
        {
            VerifyBasic(@"Public Interface IBase
                              Sub TestMethod(baseArg1 As String, baseArg2 As String)
                          End Interface

                          Public Class TestClass
                              Implements IBase

                              Public Sub AnotherTestMethod(arg1 As String, arg2 As String) Implements IBase.TestMethod
                              End Sub
                          End Class",
                        GetBasicResultAt(8, 60),
                        GetBasicResultAt(8, 76));

            VerifyBasic(@"Public Interface IBase
                              Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                          End Interface

                          Public Class TestClass
                              Implements IBase

                              Public Sub AnotherTestMethod(arg1 As String, arg2 As String, ParamArray arg3() As String) Implements IBase.TestMethod
                              End Sub
                          End Class",
                        GetBasicResultAt(8, 60),
                        GetBasicResultAt(8, 76),
                        GetBasicResultAt(8, 103));
        }

        [Fact]
        public void VerifyThatInvalidOverrideIsNotReported()
        {
            VerifyCSharp(@"public class TestClass
                           {
                               public override void TestMethod(string arg1, string arg2);
                           }");

            VerifyBasic(@"Public Class TestClass
                              Public Overrides Sub TestMethod(arg1 As String, arg2 As String)
                              End Sub
                          End Class");
        }

        [Fact]
        public void VerifyOverrideWithInheritanceChain()
        {
            VerifyCSharp(@"public abstract class BaseClass
                           {
                               public abstract void TestMethod(string baseArg1, string baseArg2);
                           }

                           public abstract class IntermediateBaseClass : BaseClass
                           {
                           }

                           public class TestClass : IntermediateBaseClass
                           {
                               public override void TestMethod(string arg1, string arg2);
                           }",
                         GetCSharpResultAt(12, 71),
                         GetCSharpResultAt(12, 84));

            VerifyBasic(@"Public MustInherit Class BaseClass
                              Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String)
                          End Class

                          Public MustInherit Class IntermediateBaseClass
                              Inherits BaseClass
                          End Class

                          Public Class TestClass
                              Inherits IntermediateBaseClass

                              Public Overrides Sub TestMethod(arg1 As String, arg2 As String)
                              End Sub
                          End Class",
                         GetBasicResultAt(12, 63),
                         GetBasicResultAt(12, 79));
        }

        [Fact]
        public void VerifyNewOverrideWithInheritance()
        {
            VerifyCSharp(@"public class BaseClass
                           {
                               public void TestMethod(string baseArg1, string baseArg2) { }
                           }

                           public class TestClass : BaseClass
                           {
                               public new void TestMethod(string arg1, string arg2);
                           }");

            VerifyBasic(@"Public Class BaseClass
                              Public Sub TestMethod(baseArg1 As String, baseArg2 As String)
                              End Sub
                          End Class
                          }

                          Public Class TestClass
                              Inherits BaseClass

                              Public Shadows Sub TestMethod(arg1 As String, arg2 As String)
                              End Sub
                          End Class");
        }

        [Fact]
        public void VerifyBaseClassNameHasPriority()
        {
            VerifyCSharp(@"public abstract class BaseClass
                           {
                               public abstract void TestMethod(string arg1, string arg2);
                           }

                           public interface ITest
                           {
                               void TestMethod(string interfaceArg1, string interfaceArg2);
                           }

                           public class TestClass : BaseClass, ITest
                           {
                               public override void TestMethod(string arg1, string arg2);
                           }");

            VerifyCSharp(@"public abstract class BaseClass
                           {
                               public abstract void TestMethod(string arg1, string arg2);
                           }

                           public interface ITest
                           {
                               void TestMethod(string interfaceArg1, string interfaceArg2);
                           }

                           public class TestClass : BaseClass, ITest
                           {
                               public override void TestMethod(string interfaceArg1, string interfaceArg2);
                           }",
                         GetCSharpResultAt(13, 71),
                         GetCSharpResultAt(13, 93));

            VerifyBasic(@"Public MustInherit Class BaseClass
                              Public MustOverride Sub TestMethod(arg1 As String, arg2 As String)
                          End Class

                          Public Interface ITest
                              Sub TestMethod(interfaceArg1 As String, interfaceArg2 As String);
                          End Interface

                          Public Class TestClass
                              Inherits BaseClass
                              Implements ITest

                              Public Overrides Sub TestMethod(arg1 As String, arg2 As String) Implements ITest.TestMethod
                              End Sub
                          End Class");

            VerifyBasic(@"Public MustInherit Class BaseClass
                              Public MustOverride Sub TestMethod(arg1 As String, arg2 As String)
                          End Class

                          Public Interface ITest
                              Sub TestMethod(interfaceArg1 As String, interfaceArg2 As String);
                          End Interface

                          Public Class TestClass
                              Inherits BaseClass
                              Implements ITest

                              Public Overrides Sub TestMethod(interfaceArg1 As String, interfaceArg2 As String) Implements ITest.TestMethod
                              End Sub
                          End Class",
                       GetBasicResultAt(13, 63),
                       GetBasicResultAt(13, 88));
        }

        [Fact]
        public void VerifyMultipleClashingInterfacesWithFullMatch()
        {
            VerifyCSharp(@"public interface ITest1
                           {
                               void TestMethod(string arg1, string arg2);
                           }

                           public interface ITest2
                           {
                               void TestMethod(string otherArg1, string otherArg2);
                           }

                           public class TestClass : ITest1, ITest2
                           {
                               public override void TestMethod(string arg1, string arg2);
                           }");

            VerifyBasic(@"Public Interface ITest1
                              Sub TestMethod(arg1 As String, arg2 As String)
                          End Interface

                          Public Interface ITest2
                              Sub TestMethod(otherArg1 As String, otherArg2 As String)
                          End Interface

                          Public Class TestClass
                              Implements ITest1, ITest2

                              Public Sub TestMethod(arg1 As String, arg2 As String) Implements ITest1.TestMethod, ITest2.TestMethod
                              End Sub
                          End Class");
        }

        [Fact]
        public void VerifyMultipleClashingInterfacesWithPartialMatch()
        {
            VerifyCSharp(@"public interface ITest1
                           {
                               public abstract void TestMethod(string arg1, string arg2, string arg3);
                           }

                           public interface ITest2
                           {
                               void TestMethod(string otherArg1, string otherArg2, string otherArg3);
                           }

                           public class TestClass : ITest1, ITest2
                           {
                               public override void TestMethod(string arg1, string arg2, string otherArg3);
                           }",
                         GetCSharpResultAt(13, 97));

            VerifyBasic(@"Public Interface ITest1
                              Sub TestMethod(arg1 As String, arg2 As String, arg3 As String)
                          End Interface

                          Public Interface ITest2
                              Sub TestMethod(otherArg1 As String, otherArg2 As String, otherArg3 As String)
                          End Interface

                          Public Class TestClass
                              Implements ITest1, ITest2

                              Public Sub TestMethod(arg1 As String, arg2 As String, otherArg3 As String) Implements ITest1.TestMethod, ITest2.TestMethod
                              End Sub
                          End Class",
                         GetBasicResultAt(12, 85));
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, ParameterNamesShouldMatchBaseDeclarationAnalyzer.Rule);
        }

        private static DiagnosticResult GetBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, ParameterNamesShouldMatchBaseDeclarationAnalyzer.Rule);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ParameterNamesShouldMatchBaseDeclarationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ParameterNamesShouldMatchBaseDeclarationAnalyzer();
        }
    }
}