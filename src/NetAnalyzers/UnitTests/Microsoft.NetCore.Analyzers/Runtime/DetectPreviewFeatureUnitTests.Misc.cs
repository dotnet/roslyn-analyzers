// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDetectPreviewFeatureAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicDetectPreviewFeatureAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    using AnalyzerType = DetectPreviewFeatureAnalyzer<SyntaxNode, SyntaxNode, SyntaxNode, SyntaxNode, SyntaxNode>;

    public partial class DetectPreviewFeatureUnitTests
    {
        private const string DefaultURL = "https://aka.ms/dotnet-warnings/preview-features";

        private static readonly DiagnosticDescriptor GeneralPreviewFeatureAttributeRule = AnalyzerType.GeneralPreviewFeatureAttributeRule;
        private static readonly DiagnosticDescriptor GeneralPreviewFeatureAttributeRuleWithCustomMessage = AnalyzerType.GeneralPreviewFeatureAttributeRuleWithCustomMessage;

        private static readonly DiagnosticDescriptor ImplementsPreviewInterfaceRule = AnalyzerType.ImplementsPreviewInterfaceRule;
        private static readonly DiagnosticDescriptor ImplementsPreviewInterfaceRuleWithCustomMessage = AnalyzerType.ImplementsPreviewInterfaceRuleWithCustomMessage;

        private static readonly DiagnosticDescriptor ImplementsPreviewMethodRule = AnalyzerType.ImplementsPreviewMethodRule;
        private static readonly DiagnosticDescriptor ImplementsPreviewMethodRuleWithCustomMessage = AnalyzerType.ImplementsPreviewMethodRuleWithCustomMessage;


        private static readonly DiagnosticDescriptor OverridesPreviewMethodRule = AnalyzerType.OverridesPreviewMethodRule;
        private static readonly DiagnosticDescriptor OverridesPreviewMethodRuleWithCustomMessage = AnalyzerType.OverridesPreviewMethodRuleWithCustomMessage;

        private static readonly DiagnosticDescriptor DerivesFromPreviewClassRule = AnalyzerType.DerivesFromPreviewClassRule;
        private static readonly DiagnosticDescriptor DerivesFromPreviewClassRuleWithCustomMessage = AnalyzerType.DerivesFromPreviewClassRuleWithCustomMessage;


        internal static readonly DiagnosticDescriptor UsesPreviewTypeParameterRule = AnalyzerType.UsesPreviewTypeParameterRule;
        internal static readonly DiagnosticDescriptor UsesPreviewTypeParameterRuleWithCustomMessage = AnalyzerType.UsesPreviewTypeParameterRuleWithCustomMessage;

        internal static readonly DiagnosticDescriptor MethodReturnsPreviewTypeRule = AnalyzerType.MethodReturnsPreviewTypeRule;
        internal static readonly DiagnosticDescriptor MethodReturnsPreviewTypeRuleWithCustomMessage = AnalyzerType.MethodReturnsPreviewTypeRuleWithCustomMessage;

        internal static readonly DiagnosticDescriptor MethodUsesPreviewTypeAsParameterRule = AnalyzerType.MethodUsesPreviewTypeAsParameterRule;
        internal static readonly DiagnosticDescriptor MethodUsesPreviewTypeAsParameterRuleWithCustomMessage = AnalyzerType.MethodUsesPreviewTypeAsParameterRuleWithCustomMessage;

        internal static readonly DiagnosticDescriptor FieldOrEventIsPreviewTypeRule = AnalyzerType.FieldOrEventIsPreviewTypeRule;
        internal static readonly DiagnosticDescriptor FieldOrEventIsPreviewTypeRuleWithCustomMessage = AnalyzerType.FieldOrEventIsPreviewTypeRuleWithCustomMessage;

        internal static readonly DiagnosticDescriptor StaticAbstractIsPreviewFeatureRule = AnalyzerType.StaticAbstractIsPreviewFeatureRule;


        private static VerifyCS.Test TestCS(string csInput)
        {
            return new VerifyCS.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.Net60,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp10,
                TestState =
                {
                    Sources =
                    {
                        csInput
                    },
                },
            };
        }

        private static VerifyVB.Test TestVB(string vbInput)
        {
            return new VerifyVB.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.Net60,
                LanguageVersion = CodeAnalysis.VisualBasic.LanguageVersion.Latest,
                TestState =
                {
                    Sources =
                    {
                        vbInput
                    },
                },
            };
        }

        private static VerifyCS.Test SetupDependencyAndTestCSWithOneSourceFile(string csInput, string csDependencyCode)
        {
            return new VerifyCS.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.Net60,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp10,
                TestState =
                {
                    Sources =
                    {
                        csInput
                    },
                    AdditionalProjects =
                    {
                        ["PreviewAssembly"] =
                        {
                            Sources =
                            {
                                ("/PreviewAssembly/AssemblyInfo.g.cs", csDependencyCode)
                            },
                        },
                    },
                    AdditionalProjectReferences =
                    {
                        "PreviewAssembly",
                    },
                },
            };
        }

        private static VerifyCS.Test TestCSPreview(string csInput)
        {
            return new VerifyCS.Test
            {
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
                TestState =
                {
                    Sources =
                    {
                        csInput
                    }
                },
                ReferenceAssemblies = AdditionalMetadataReferences.Net60,
            };
        }

        [Fact]
        public async Task TestCatchPreviewException()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    [RequiresPreviewFeatures]
    public class DerivedException : Exception
    {

    }

    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(""Foo"");
            }
            catch {|#0:(DerivedException ex)|}
            {
                throw;
            }
        }
    }
}
";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("DerivedException", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestCustomMessageCustomURL()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
            public class Program
            {
                static void Main(string[] args)
                {
                    Lib[] array = {|#0:new Lib[] { }|};
                    Lib lib = {|#1:new Lib()|};
                }
            }

            [RequiresPreviewFeatures(""Lib is in preview."", Url = ""https://aka.ms/aspnet/kestrel/http3reqs"")]
            public class Lib
            {
            }
        }
        ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRuleWithCustomMessage).WithLocation(0).WithArguments("Lib", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRuleWithCustomMessage).WithLocation(1).WithArguments("Lib", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestCustomMessageDefaultURL()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
            public class Program
            {
                static void Main(string[] args)
                {
                    Lib[] array = {|#0:new Lib[] { }|};
                }
            }

            [RequiresPreviewFeatures(""Lib is in preview."")]
            public class Lib
            {
            }
        }
        ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRuleWithCustomMessage).WithLocation(0).WithArguments("Lib", DefaultURL, "Lib is in preview."));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestDefaultMessageCustomURL()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
            public class Program
            {
                static void Main(string[] args)
                {
                    Lib[] array = {|#0:new Lib[] { }|};
                }
            }

            [RequiresPreviewFeatures(Url = ""https://aka.ms/aspnet/kestrel/http3reqs"")]
            public class Lib
            {
            }
        }
        ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("Lib", "https://aka.ms/aspnet/kestrel/http3reqs"));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestArrayOfPreviewTypes()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
            public class Program
            {
                static void Main(string[] args)
                {
                    Lib[] array = {|#0:new Lib[] { }|};
                    Lib anObject = {|#1:new()|};
                }
            }

            [RequiresPreviewFeatures(Url = ""https://aka.ms/aspnet/kestrel/http3reqs"")]
            public class Lib
            {
            }
        }
        ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("Lib", "https://aka.ms/aspnet/kestrel/http3reqs"));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments("Lib", "https://aka.ms/aspnet/kestrel/http3reqs"));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestArrayOfArraysOfPreviewTypes()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
            public class Program
            {
                static void Main(string[] args)
                {
                    Lib[][] array = {|#0:new Lib[][] {}|};
                }
            }

            [RequiresPreviewFeatures]
            public class Lib
            {
            }
        }
        ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("Lib", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestPreviewLanguageFeaturesHeirarchy()
        {
            var csInput = @" 
                using System.Runtime.Versioning; using System;
                namespace Preview_Feature_Scratch
                {

                    [RequiresPreviewFeatures]
                    class Program : IProgram
                    {
                        static void Main(string[] args)
                        {
                            new Program();
                        }

                        public static bool StaticMethod() => throw null;
                        public static bool AProperty => throw null;
                    }

                    [RequiresPreviewFeatures]
                    public interface IProgram
                    {
                        public static abstract bool StaticMethod();
                        public static abstract bool AProperty { get; }
                    }
                }

                    ";

            var test = TestCSPreview(csInput);
            await test.RunAsync();
        }

        [Fact]
        public async Task TestPreviewLanguageFeatures()
        {
            var csInput = @" 
                using System.Runtime.Versioning; using System;
                namespace Preview_Feature_Scratch
                {

                    class Program : IProgram
                    {
                        static void Main(string[] args)
                        {
                            new Program();
                        }

                        public static bool StaticMethod() => throw null;
                        public static bool AProperty => throw null;
                    }

                    public interface IProgram
                    {
                        public static abstract bool {|#0:StaticMethod|}();
                        public static abstract bool {|#1:AProperty|} { {|#2:get|}; }
                    }
                }

                    ";

            var test = TestCSPreview(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(StaticAbstractIsPreviewFeatureRule).WithLocation(0).WithArguments("StaticMethod"));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(StaticAbstractIsPreviewFeatureRule).WithLocation(1).WithArguments("AProperty"));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(StaticAbstractIsPreviewFeatureRule).WithLocation(2).WithArguments("get"));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestInterfaceMethodInvocation()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program : IProgram
            {
                static void Main(string[] args)
                {
                    Program progObject = new Program();
                    IProgram prog = progObject;
                    {|#0:prog.Foo()|};
                    {|#1:prog.FooDelegate()|};
                    bool prop = {|#2:prog.AProperty|};
                    bool anotherProp = {|#3:progObject.AnotherInterfaceProperty|};
                    Console.WriteLine(""prop.ToString() + anotherProp.ToString()"");
                }

                public IProgram.IProgramDelegate {|#4:FooDelegate|}()
                {
                    throw new NotImplementedException();
                }

                [RequiresPreviewFeatures]
                public bool AnotherInterfaceProperty { get; set; }
            }

            public interface IProgram
            {
                [RequiresPreviewFeatures]
                public bool AProperty => true;

                public bool AnotherInterfaceProperty { get; set; }

                public delegate void IProgramDelegate();

                [RequiresPreviewFeatures]
                public void Foo()
                {
                    throw new NotImplementedException();
                }

                [RequiresPreviewFeatures]
                public IProgramDelegate FooDelegate();

            }
        }

            ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments("FooDelegate", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(2).WithArguments("AProperty", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(3).WithArguments("AnotherInterfaceProperty", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(ImplementsPreviewMethodRule).WithLocation(4).WithArguments("FooDelegate", "IProgram.FooDelegate", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestDelegate()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program
            {
                [RequiresPreviewFeatures]
                public delegate void Del();

                static void Main(string[] args)
                {
                    Del del = {|#0:new(() => { })|};
                }
            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("Del", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestTypeOf()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine({|#0:typeof(IFoo)|});
        }
    }
    
    [RequiresPreviewFeatures]
    interface IFoo { }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("IFoo", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestSimpleCustomAttributeOnPreviewClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            A aObject = [|#0:new()|};
        }
    }

[RequiresPreviewFeatures]
[My]
class A
{
}

[RequiresPreviewFeatures]
[AttributeUsage(AttributeTargets.All)]
class MyAttribute : Attribute
{
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("A", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestSimpleCustomAttribute()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            A aObject = new A();
        }
    }

[{|#1:My|}]
class A
{
}

[RequiresPreviewFeatures]
[AttributeUsage(AttributeTargets.All)]
class MyAttribute : Attribute
{
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments("MyAttribute", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestCustomAttribute()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            A aObject = new A();
        }
    }

[{|#0:My|}]
class A
{
}

[My(true)]
class B
{
}

[RequiresPreviewFeatures]
[My]
class C
{
}

[RequiresPreviewFeatures]
[My(Feature = ""This is a feature"")]
class classUsingFeatureAndGuarded
{
}

[My(true, Feature = ""This is a feature"")]
class classUsingFeature
{
}

[My(true, {|#1:PreviewFeature|} = ""This is a feature"")]
class classUsingPreviewFeature
{
}

[RequiresPreviewFeatures]
[My(true, PreviewFeature = ""This is a feature"")]
class classUsingBoolFeatureAndGuarded
{
}

[AttributeUsage(AttributeTargets.All)]
class MyAttribute : Attribute
{
    [RequiresPreviewFeatures]
    public MyAttribute() {}

    public MyAttribute(bool foo) {}

    public string Feature { get; set; }

    [RequiresPreviewFeatures]
    public string PreviewFeature { get; set; }
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("MyAttribute", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments("PreviewFeature", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestDeepNesting()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            NestedClass0.NestedClass1.NestedClass2.NestedClass3 nestedClass3 = {|#0:new()|};
            {|#1:nestedClass3.AMethod()|};
            bool prop = {|#2:nestedClass3.AProperty|};
            prop = {|#3:nestedClass3.AField|};
        }
    }

    [RequiresPreviewFeatures]
    public class NestedClass0
    {
        public class NestedClass1
        {
            public class NestedClass2
            {
                public class NestedClass3
                {
                    public bool AMethod() => false;
                    public bool AProperty => false;
                    public bool AField = true;
                }
            }
        }
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("NestedClass3", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments("AMethod", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(2).WithArguments("AProperty", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(3).WithArguments("AField", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestNestedInvocation()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine({|#0:A.B()|});
        }
    }

class A
{
    [RequiresPreviewFeatures]
    public static bool B() => true;
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("B", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestNestedClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        [RequiresPreviewFeatures]
        class NestedClass
        {

        }

        static void Main(string[] args)
        {
            NestedClass nestedClass = {|#0:new NestedClass()|};
        }
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("NestedClass", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestCallback()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
    @"

    class AFoo<T> where T : {|#2:Foo|}, new()
    {
        public {|#1:Foo|}[] _fooArray;

        public void CallBackMethod(Action<{|#5:Foo|}> action)
        {
            foreach (var foo in _fooArray)
            {
                action(foo);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            AFoo<Foo> anObject = {|#4:new AFoo<Foo>()|};
            anObject.CallBackMethod({|#0:(Foo foo) => { }|});
        }
    }

    [RequiresPreviewFeatures]
    public class Foo
    {
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(FieldOrEventIsPreviewTypeRule).WithLocation(1).WithArguments("_fooArray", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(2).WithArguments("AFoo", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(4).WithArguments("Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(5).WithArguments("CallBackMethod", "Foo", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestVbCaseInsensitiveCsharpSensitive()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program : {|#1:IProgram|}, Iprogram
            {
                static void Main(string[] args)
                {
                    new Program();
                }

                public void {|#0:UnmarkedMethodInMarkedInterface|}() { }

                public void UnmarkedMethodInUnMarkedInterface() { }
            }

            [RequiresPreviewFeatures]
            public interface IProgram
            {
                public void UnmarkedMethodInMarkedInterface() { }
            }

            public interface Iprogram
            {
                public void UnmarkedMethodInUnMarkedInterface() { }
            }
        }
            ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(ImplementsPreviewMethodRule).WithLocation(0).WithArguments("UnmarkedMethodInMarkedInterface", "IProgram.UnmarkedMethodInMarkedInterface", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(ImplementsPreviewInterfaceRule).WithLocation(1).WithArguments("Program", "IProgram", DefaultURL));
            await test.RunAsync();

            var vbInput = @" 
        Imports System
        Imports System.Runtime.Versioning
        Module Preview_Feature_Scratch
            Public Class Program
                Implements {|#1:IProgram|}
                Private Shared Sub Main(ByVal args As String())
                    Dim prog = New Program()
                End Sub

                Public Sub MarkedMethodInInterface() Implements IProgram.{|#0:markedMethodInInterface|}
                    Throw New NotImplementedException()
                End Sub
            End Class

            <RequiresPreviewFeatures>
            Public Interface Iprogram
                Sub MarkedMethodInInterface()
            End Interface
        End Module
            ";

            var testVb = TestVB(vbInput);
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(ImplementsPreviewMethodRule).WithLocation(0).WithArguments("MarkedMethodInInterface", "Iprogram.MarkedMethodInInterface", DefaultURL));
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(ImplementsPreviewInterfaceRule).WithLocation(1).WithArguments("Program", "Iprogram", DefaultURL));
            await testVb.RunAsync();
        }
    }
}