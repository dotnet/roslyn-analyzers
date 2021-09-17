// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDetectPreviewFeatureAnalyzer,
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

        public Dictionary<int, {|#4:Foo?|}[]> GetterNullableArray(Dictionary<int, {|#5:Foo?|}[]> foo)
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
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "Getter", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodReturnsPreviewTypeRule).WithLocation(1).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodReturnsPreviewTypeMessage, "Getter", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodReturnsPreviewTypeRule).WithLocation(2).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodReturnsPreviewTypeMessage, "GetterNullable", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(3).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "GetterNullable", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodReturnsPreviewTypeRule).WithLocation(4).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodReturnsPreviewTypeMessage, "GetterNullableArray", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(5).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "GetterNullableArray", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
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
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "Getter", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodReturnsPreviewTypeRule).WithLocation(1).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodReturnsPreviewTypeMessage, "Getter", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
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
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodUsesPreviewTypeAsParameterRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodUsesPreviewTypeAsParameterMessage, "Getter", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodReturnsPreviewTypeRule).WithLocation(2).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodReturnsPreviewTypeMessage, "Getter", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodUsesPreviewTypeAsParameterRule).WithLocation(3).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodUsesPreviewTypeAsParameterMessage, "GetterNullable", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodReturnsPreviewTypeRule).WithLocation(4).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodReturnsPreviewTypeMessage, "GetterNullable", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
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
                public void M1<T>(Preview_Feature_Scratch.T {|#0:t|}) // Doesn't use the type parameter. The location detection logic for syntax node doesn't work here.
                {
                }

                public void M2<T>(T t) // Uses the type parameter.
                {
                }
            }
        }
        ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.MethodUsesPreviewTypeAsParameterRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_methodUsesPreviewTypeAsParameterMessage, "M1", "T"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
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
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewMethod"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
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
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.OverridesPreviewMethodRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_overridePreviewMethodMessage, "PreviewMethod", "Program.PreviewMethod"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewMethod"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }
    }
}
