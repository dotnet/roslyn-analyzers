// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ParameterNamesShouldMatchBaseDeclarationFixerTests : CodeFixTestBase
    {
        [Fact]
        public void VerifyOverrideWithWrongParameterNames()
        {
            VerifyCSharpFix(@"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2);
                              }

                              public class TestClass : BaseClass
                              {
                                  public override void TestMethod(string arg1, string arg2) { }
                              }",
                            @"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2);
                              }

                              public class TestClass : BaseClass
                              {
                                  public override void TestMethod(string baseArg1, string baseArg2) { }
                              }");

            VerifyCSharpFix(@"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2, __arglist);
                              }

                              public class TestClass : BaseClass
                              {
                                  public override void TestMethod(string arg1, string arg2, __arglist) { }
                              }",
                            @"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2, __arglist);
                              }

                              public class TestClass : BaseClass
                              {
                                  public override void TestMethod(string baseArg1, string baseArg2, __arglist) { }
                              }");

            VerifyCSharpFix(@"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                              }

                              public class TestClass : BaseClass
                              {
                                  public override void TestMethod(string arg1, string arg2, params string[] arg3) { }
                              }",
                            @"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                              }

                              public class TestClass : BaseClass
                              {
                                  public override void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3) { }
                              }");

            VerifyBasicFix(@"Public MustInherit Class BaseClass
                                 Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String)
                             End Class

                             Public Class TestClass 
                                 Inherits BaseClass

                                 Public Overrides Sub TestMethod(arg1 as String, arg2 as String)
                                 End Sub
                             End Class",
                           @"Public MustInherit Class BaseClass
                                 Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String)
                             End Class

                             Public Class TestClass 
                                 Inherits BaseClass

                                 Public Overrides Sub TestMethod(baseArg1 as String, baseArg2 as String)
                                 End Sub
                             End Class");

            VerifyBasicFix(@"Public MustInherit Class BaseClass
                                 Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                             End Class

                             Public Class TestClass
                                 Inherits BaseClass

                                 Public Overrides Sub TestMethod(arg1 as String, arg2 as String, ParamArray arg3() As String)
                                 End Sub
                             End Class",
                           @"Public MustInherit Class BaseClass
                                 Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                             End Class

                             Public Class TestClass
                                 Inherits BaseClass

                                 Public Overrides Sub TestMethod(baseArg1 as String, baseArg2 as String, ParamArray baseArg3() As String)
                                 End Sub
                             End Class");
        }

        [Fact]
        public void VerifyInterfaceImplementationWithWrongParameterNames()
        {
            VerifyCSharpFix(@"public interface IBase
                              {
                                  void TestMethod(string baseArg1, string baseArg2);
                              }

                              public class TestClass : IBase
                              {
                                  public void TestMethod(string arg1, string arg2) { }
                              }",
                            @"public interface IBase
                              {
                                  void TestMethod(string baseArg1, string baseArg2);
                              }

                              public class TestClass : IBase
                              {
                                  public void TestMethod(string baseArg1, string baseArg2) { }
                              }");

            VerifyCSharpFix(@"public interface IBase
                              {
                                  void TestMethod(string baseArg1, string baseArg2, __arglist);
                              }

                              public class TestClass : IBase
                              {
                                  public void TestMethod(string arg1, string arg2, __arglist) { }
                              }",
                            @"public interface IBase
                              {
                                  void TestMethod(string baseArg1, string baseArg2, __arglist);
                              }

                              public class TestClass : IBase
                              {
                                  public void TestMethod(string baseArg1, string baseArg2, __arglist) { }
                              }");

            VerifyCSharpFix(@"public interface IBase
                              {
                                  void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                              }

                              public class TestClass : IBase
                              {
                                  public void TestMethod(string arg1, string arg2, params string[] arg3) { }
                              }",
                            @"public interface IBase
                              {
                                  void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                              }

                              public class TestClass : IBase
                              {
                                  public void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3) { }
                              }");

            VerifyBasicFix(@"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub TestMethod(arg1 As String, arg2 As String) Implements IBase.TestMethod
                                 End Sub
                             End Class",
                           @"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub TestMethod(baseArg1 As String, baseArg2 As String) Implements IBase.TestMethod
                                 End Sub
                             End Class");

            VerifyBasicFix(@"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub TestMethod(arg1 As String, arg2 As String, ParamArray arg3() As String) Implements IBase.TestMethod
                                 End Sub
                             End Class",
                           @"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String) Implements IBase.TestMethod
                                 End Sub
                             End Class");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void VerifyExplicitInterfaceImplementationWithWrongParameterNames_NoDiagnostic()
        {
            var source = @"public interface IBase
                              {
                                  void TestMethod(string baseArg1, string baseArg2);
                              }

                              public class TestClass : IBase
                              {
                                  void IBase.TestMethod(string arg1, string arg2) { }
                              }";
            VerifyCSharpFix(source, source);

            source = @"public interface IBase
                        {
                            void TestMethod(string baseArg1, string baseArg2, __arglist);
                        }

                        public class TestClass : IBase
                        {
                            void IBase.TestMethod(string arg1, string arg2, __arglist) { }
                        }";
            VerifyCSharpFix(source, source);

            source = @"public interface IBase
                        {
                            void TestMethod(string baseArg1, string baseArg2, params string[] baseArg3);
                        }

                        public class TestClass : IBase
                        {
                            void IBase.TestMethod(string arg1, string arg2, params string[] arg3) { }
                        }";
            VerifyCSharpFix(source, source);
        }

        [Fact]
        public void VerifyInterfaceImplementationWithDifferentMethodName()
        {
            VerifyBasicFix(@"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub AnotherTestMethod(arg1 As String, arg2 As String) Implements IBase.TestMethod
                                 End Sub
                             End Class",
                           @"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub AnotherTestMethod(baseArg1 As String, baseArg2 As String) Implements IBase.TestMethod
                                 End Sub
                             End Class");

            VerifyBasicFix(@"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub AnotherTestMethod(arg1 As String, arg2 As String, ParamArray arg3() As String) Implements IBase.TestMethod
                                 End Sub
                             End Class",
                           @"Public Interface IBase
                                 Sub TestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String)
                             End Interface

                             Public Class TestClass
                                 Implements IBase

                                 Public Sub AnotherTestMethod(baseArg1 As String, baseArg2 As String, ParamArray baseArg3() As String) Implements IBase.TestMethod
                                 End Sub
                             End Class");
        }

        [Fact]
        public void VerifyOverrideWithInheritanceChain()
        {
            VerifyCSharpFix(@"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2);
                              }

                              public abstract class IntermediateBaseClass : BaseClass
                              {
                              }

                              public class TestClass : IntermediateBaseClass
                              {
                                  public override void TestMethod(string arg1, string arg2) { }
                              }",
                            @"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string baseArg1, string baseArg2);
                              }

                              public abstract class IntermediateBaseClass : BaseClass
                              {
                              }

                              public class TestClass : IntermediateBaseClass
                              {
                                  public override void TestMethod(string baseArg1, string baseArg2) { }
                              }");

            VerifyBasicFix(@"Public MustInherit Class BaseClass
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
                            @"Public MustInherit Class BaseClass
                                 Public MustOverride Sub TestMethod(baseArg1 As String, baseArg2 As String)
                             End Class

                             Public MustInherit Class IntermediateBaseClass
                                 Inherits BaseClass
                             End Class

                             Public Class TestClass
                                 Inherits IntermediateBaseClass

                                 Public Overrides Sub TestMethod(baseArg1 As String, baseArg2 As String)
                                 End Sub
                             End Class");
        }

        [Fact]
        public void VerifyBaseClassNameHasPriority()
        {
            VerifyCSharpFix(@"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string arg1, string arg2);
                              }

                              public interface ITest
                              {
                                  void TestMethod(string interfaceArg1, string interfaceArg2);
                              }

                              public class TestClass : BaseClass, ITest
                              {
                                  public override void TestMethod(string interfaceArg1, string interfaceArg2) { }
                              }",
                            @"public abstract class BaseClass
                              {
                                  public abstract void TestMethod(string arg1, string arg2);
                              }

                              public interface ITest
                              {
                                  void TestMethod(string interfaceArg1, string interfaceArg2);
                              }

                              public class TestClass : BaseClass, ITest
                              {
                                  public override void TestMethod(string arg1, string arg2) { }
                              }");

            VerifyBasicFix(@"Public MustInherit Class BaseClass
                                 Public MustOverride Sub TestMethod(arg1 As String, arg2 As String)
                             End Class
    
                             Public Interface ITest
                                 Sub TestMethod(interfaceArg1 As String, interfaceArg2 As String)
                             End Interface
    
                             Public Class TestClass
                                 Inherits BaseClass
                                 Implements ITest
    
                                 Public Overrides Sub TestMethod(interfaceArg1 As String, interfaceArg2 As String) Implements ITest.TestMethod
                                 End Sub
                             End Class",
                          @"Public MustInherit Class BaseClass
                                 Public MustOverride Sub TestMethod(arg1 As String, arg2 As String)
                             End Class
    
                             Public Interface ITest
                                 Sub TestMethod(interfaceArg1 As String, interfaceArg2 As String)
                             End Interface
    
                             Public Class TestClass
                                 Inherits BaseClass
                                 Implements ITest
    
                                 Public Overrides Sub TestMethod(arg1 As String, arg2 As String) Implements ITest.TestMethod
                                 End Sub
                             End Class");
        }

        [Fact]
        public void VerifyMultipleClashingInterfacesWithPartialMatch()
        {
            VerifyCSharpFix(@"public interface ITest1
                              {
                                  void TestMethod(string arg1, string arg2, string arg3);
                              }

                              public interface ITest2
                              {
                                  void TestMethod(string otherArg1, string otherArg2, string otherArg3);
                              }

                              public class TestClass : ITest1, ITest2
                              {
                                  public void TestMethod(string arg1, string arg2, string otherArg3) { }
                              }",
                            @"public interface ITest1
                              {
                                  void TestMethod(string arg1, string arg2, string arg3);
                              }

                              public interface ITest2
                              {
                                  void TestMethod(string otherArg1, string otherArg2, string otherArg3);
                              }

                              public class TestClass : ITest1, ITest2
                              {
                                  public void TestMethod(string arg1, string arg2, string arg3) { }
                              }");

            VerifyBasicFix(@"Public Interface ITest1
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
                            @"Public Interface ITest1
                                 Sub TestMethod(arg1 As String, arg2 As String, arg3 As String)
                             End Interface
    
                             Public Interface ITest2
                                 Sub TestMethod(otherArg1 As String, otherArg2 As String, otherArg3 As String)
                             End Interface
    
                             Public Class TestClass
                                 Implements ITest1, ITest2
    
                                 Public Sub TestMethod(arg1 As String, arg2 As String, arg3 As String) Implements ITest1.TestMethod, ITest2.TestMethod
                                 End Sub
                             End Class");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ParameterNamesShouldMatchBaseDeclarationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ParameterNamesShouldMatchBaseDeclarationAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new ParameterNamesShouldMatchBaseDeclarationFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ParameterNamesShouldMatchBaseDeclarationFixer();
        }
    }
}