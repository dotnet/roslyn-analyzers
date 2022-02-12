// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDetectPreviewFeatureAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicDetectPreviewFeatureAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public partial class DetectPreviewFeatureUnitTests
    {
        [Fact]
        public async Task TestGenericPreviewParametersToPreviewMethod()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
using System.Collections.Generic;
namespace Preview_Feature_Scratch
{

    class Program
    {
        public Dictionary<int, {|#1:Foo|}> Getter(Dictionary<int, {|#0:Foo|}> foo)
        {
            return foo;
        }

#nullable enable
        public Dictionary<int, {|#2:Foo|}?> GetterNullable(Dictionary<int, {|#3:Foo|}?> foo)
        {
            return foo;
        }

        public Dictionary<int, {|#4:Foo|}?[]> GetterNullableArray(Dictionary<int, {|#5:Foo|}?[]> foo)
        {
            return foo;
        }

#nullable disable

        static void Main(string[] args)
        {
        }
    }

    [RequiresPreviewFeatures]
    public class Foo
    {
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(0).WithArguments("foo", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(1).WithArguments("Getter", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(2).WithArguments("GetterNullable", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(3).WithArguments("foo", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(4).WithArguments("GetterNullableArray", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(5).WithArguments("foo", "Foo", DefaultURL));
            await test.RunAsync();

            var vbInput = @" 
        Imports System
        Imports System.Runtime.Versioning
        Imports System.Collections.Generic
        Module Preview_Feature_Scratch
            Public Class Program
                Public Function Getter(foo As Dictionary(Of Int32, {|#0:Foo|})) As Dictionary(Of Int32, {|#1:Foo|})
                    Return foo
                End Function
            End Class

            <RequiresPreviewFeatures>
            Public Class Foo
            End Class

        End Module
            ";

            var testVb = TestVB(vbInput);
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(0).WithArguments("foo", "Foo", DefaultURL));
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(1).WithArguments("Getter", "Foo", DefaultURL));
            await testVb.RunAsync();
        }

        [Fact]
        public async Task TestNestedGenericPreviewParametersToPreviewMethod()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
using System.Collections.Generic;
namespace Preview_Feature_Scratch
{

    class Program
    {
        public List<List<List<{|#1:Foo|}>>> Getter(List<List<List<{|#0:Foo|}>>> foo)
        {
            return foo;
        }

        static void Main(string[] args)
        {
        }
    }

    [RequiresPreviewFeatures]
    public class Foo
    {
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(0).WithArguments("foo", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(1).WithArguments("Getter", "Foo", DefaultURL));
            await test.RunAsync();

            var vbInput = @" 
Imports System.Runtime.Versioning
Imports System
Imports System.Collections.Generic

Namespace Preview_Feature_Scratch
    Class Program
        Public Function Getter(ByVal foo As List(Of List(Of List(Of {|#0:Foo|})))) As List(Of List(Of List(Of {|#1:Foo|})))
            Return foo
        End Function

        Private Shared Sub Main(ByVal args As String())
        End Sub
    End Class

    <RequiresPreviewFeatures>
    Public Class Foo
    End Class
End Namespace
";
            var testVb = TestVB(vbInput);
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(UsesPreviewTypeParameterRule).WithLocation(0).WithArguments("foo", "Foo", DefaultURL));
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(1).WithArguments("Getter", "Foo", DefaultURL));
            await testVb.RunAsync();
        }

        [Fact]
        public async Task TestPreviewParametersToPreviewMethod()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    [RequiresPreviewFeatures]
    class Program
    {
        public Foo Getter(Foo foo)
        {
            return foo;
        }

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Getter(new Foo());
        }
    }

    [RequiresPreviewFeatures]
    public class Foo
    {
    }
}";

            var test = TestCS(csInput);
            await test.RunAsync();

            var vbInput = @" 
Imports System.Runtime.Versioning
Imports System

Namespace Preview_Feature_Scratch
    <RequiresPreviewFeatures>
    Class Program
        Public Function Getter(ByVal foo As Foo) As Foo
            Return foo
        End Function

        Private Shared Sub Main(ByVal args As String())
            Dim prog As Program = New Program()
            prog.Getter(New Foo())
        End Sub
    End Class

    <RequiresPreviewFeatures>
    Public Class Foo
    End Class
End Namespace
";

            var vbTest = TestVB(vbInput);
            await vbTest.RunAsync();
        }

        [Fact]
        public async Task TestPreviewParametersToMethodsWithCustomMessageAndUrl()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        public {|#2:Foo|} Getter({|#0:Foo|} foo)
        {
            return foo;
        }

#nullable enable
        public {|#4:Foo|}? GetterNullable({|#3:Foo|}? foo)
        {
            return foo;
        }
#nullable disable

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Getter({|#1:new Foo()|});
        }
    }

    [RequiresPreviewFeatures(""Lib is in preview."", Url = ""https://aka.ms/aspnet/kestrel/http3reqs"")]
    public class Foo
    {
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodUsesPreviewTypeAsParameterRuleWithCustomMessage).WithLocation(0).WithArguments("Getter", "Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRuleWithCustomMessage).WithLocation(1).WithArguments("Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRuleWithCustomMessage).WithLocation(2).WithArguments("Getter", "Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodUsesPreviewTypeAsParameterRuleWithCustomMessage).WithLocation(3).WithArguments("GetterNullable", "Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRuleWithCustomMessage).WithLocation(4).WithArguments("GetterNullable", "Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            await test.RunAsync();

            var vbInput = @" 
Imports System.Runtime.Versioning
Imports System

Namespace Preview_Feature_Scratch
    Class Program
        Public Function Getter(ByVal foo As {|#0:Foo|}) As {|#2:Foo|}
            Return foo
        End Function

        Private Shared Sub Main(ByVal args As String())
            Dim prog As Program = New Program()
            prog.Getter({|#1:New Foo()|})
        End Sub
    End Class

    <RequiresPreviewFeatures(""Lib is in preview."", Url:=""https://aka.ms/aspnet/kestrel/http3reqs"")>
    Public Class Foo
    End Class
End Namespace
";
            var testVb = TestVB(vbInput);
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(MethodUsesPreviewTypeAsParameterRuleWithCustomMessage).WithLocation(0).WithArguments("Getter", "Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(GeneralPreviewFeatureAttributeRuleWithCustomMessage).WithLocation(1).WithArguments("Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(MethodReturnsPreviewTypeRuleWithCustomMessage).WithLocation(2).WithArguments("Getter", "Foo", "https://aka.ms/aspnet/kestrel/http3reqs", "Lib is in preview."));
            await testVb.RunAsync();
        }

        [Fact]
        public async Task TestPreviewParametersToMethods()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        public {|#2:Foo|} Getter({|#0:Foo|} foo)
        {
            return foo;
        }

#nullable enable
        public {|#4:Foo|}? GetterNullable({|#3:Foo|}? foo)
        {
            return foo;
        }
#nullable disable

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Getter({|#1:new Foo()|});
        }
    }

    [RequiresPreviewFeatures]
    public class Foo
    {
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodUsesPreviewTypeAsParameterRule).WithLocation(0).WithArguments("Getter", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments("Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(2).WithArguments("Getter", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodUsesPreviewTypeAsParameterRule).WithLocation(3).WithArguments("GetterNullable", "Foo", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(4).WithArguments("GetterNullable", "Foo", DefaultURL));
            await test.RunAsync();

            var vbInput = @" 
        Imports System
        Imports System.Runtime.Versioning
        Module Preview_Feature_Scratch
            Public Class Program
                Public Function Getter(foo As {|#0:Foo|}) As {|#2:Foo|}
                    Return foo
                End Function
            End Class

            <RequiresPreviewFeatures>
            Public Structure Foo
            End Structure

        End Module
            ";

            var testVb = TestVB(vbInput);
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(MethodUsesPreviewTypeAsParameterRule).WithLocation(0).WithArguments("Getter", "Foo", DefaultURL));
            testVb.ExpectedDiagnostics.Add(VerifyVB.Diagnostic(MethodReturnsPreviewTypeRule).WithLocation(2).WithArguments("Getter", "Foo", DefaultURL));
            await testVb.RunAsync();
        }

        [Fact]
        public async Task TestUnmarkedPreviewMethodCallingPreviewMethod()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
        [RequiresPreviewFeatures]
        public class Program
        {
            public bool CallSite()
            {
                return UnmarkedPreviewClass.SomeStaticMethod();
            }
        }

        public class UnmarkedPreviewClass
        {
                [RequiresPreviewFeatures]
                public static bool SomeStaticMethod()
                {
                    return false;
                }
        }
        }
        ";

            var test = TestCS(csInput);
            await test.RunAsync();
        }

        [Fact]
        public async Task TestSyntaxNodeNameComparison()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
            [RequiresPreviewFeatures]
            public class T { }

            public class C
            {
                public void M1<T>(Preview_Feature_Scratch.{|#0:T|} t) // Doesn't use the type parameter. The location detection logic for syntax node doesn't work here.
                {
                }

                public void M2<T>(T t) // Uses the type parameter.
                {
                }
            }
        }
        ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(MethodUsesPreviewTypeAsParameterRule).WithLocation(0).WithArguments("M1", "T", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestPreviewMethodCallingPreviewMethod()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
        public class Program
        {
            [RequiresPreviewFeatures]
            public virtual void PreviewMethod()  { }

            [RequiresPreviewFeatures]
            void CallSite()
            {
                PreviewMethod();
            }
        }
        }
        ";

            var test = TestCS(csInput);
            await test.RunAsync();
        }

        [Fact]
        public async Task TestMethodInvocation_Simple()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            public class Program
            {
                [RequiresPreviewFeatures]
                public virtual void PreviewMethod()
                {

                }

                static void Main(string[] args)
                {
                    var prog = new Program();
                    {|#0:prog.PreviewMethod()|};
                }
            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments("PreviewMethod", DefaultURL));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestMethodInvocation_DeclareDerivedMethod()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            public class Program
            {
                [RequiresPreviewFeatures]
                public virtual void PreviewMethod()
                {

                }

                static void Main(string[] args)
                {
                }
            }

            public class Derived : Program
            {
                public Derived() : base()
                {
                }

                public override void {|#0:PreviewMethod|}()
                {
                    {|#1:base.PreviewMethod()|};
                }
            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(OverridesPreviewMethodRule).WithLocation(0).WithArguments("PreviewMethod", "Program.PreviewMethod", DefaultURL));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments("PreviewMethod", DefaultURL));
            await test.RunAsync();
        }
    }
}
