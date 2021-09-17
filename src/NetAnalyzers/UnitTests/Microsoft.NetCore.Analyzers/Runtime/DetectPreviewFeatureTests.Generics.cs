﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
        public async Task TestNonPreviewMethodWithGenericPreviewParameter()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        public bool GenericMethod<T>()
        {
            return true;
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            {|#0:program.GenericMethod<Foo>()|};
        }
    }

    [RequiresPreviewFeatures]
    public class Foo
    {
    }

}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestGenericMethodWithPreviewClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        public bool GenericMethod<T>()
            where T : {|#0:Foo|}
        {
            return true;
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
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "GenericMethod", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestGenericMethodWithNullablePreviewClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
#nullable enable
        public bool GenericMethod<T>()
            where T : {|#0:Foo?|}
        {
            return true;
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
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "GenericMethod", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestGenericClassWithNullablePreviewClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

#nullable enable
    class Program<T>
        where T : {|#0:Foo?|}
    {
        static void Main(string[] args)
        {
        }
    }
#nullable disable

    [RequiresPreviewFeatures]
    public class Foo
    {
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "Program", "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestGenericMethodInsidePreviewClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    [RequiresPreviewFeatures]
    class Program
    {
        public bool GenericMethod<T>()
            where T : Foo
        {
            return true;
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
            await test.RunAsync();
        }

        [Fact]
        public async Task TestTwoLevelGenericMethodInsidePreviewClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    [RequiresPreviewFeatures]
    class Program
    {
        class NestedClass
        {
            public bool GenericMethod<T>()
                where T : Foo
            {
                return true;
            }
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
            await test.RunAsync();
        }

        [Fact]
        public async Task TestGenericClassWithoutPreviewInterface()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            A<Foo> aFooInstance = {|#0:new A<Foo>()|};
        }
    }

class A<T> where T : IFoo, new()
{
    public A()
    {
        new T().Bar();
    }
}

[RequiresPreviewFeatures]
class Foo : IFoo
{
    public void Bar() { }
}

interface IFoo
{
    void Bar();
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "Foo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestGenericClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            A<Foo> aFooInstance = new A<Foo>();
        }
    }

class A<T> where T : {|#1:IFoo|}, new()
{
    public A()
    {
        IFoo foo = new T();
        {|#0:foo.Bar()|};
    }
}

class Foo : {|#2:IFoo|}
{
    public void {|#3:Bar|}() { }
}

[RequiresPreviewFeatures]
interface IFoo
{
    void Bar();
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "Bar"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(1).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "A", "IFoo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewInterfaceRule).WithLocation(2).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_implementPreviewInterfaceMessage, "Foo", "IFoo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewMethodRule).WithLocation(3).WithArguments("Bar", "IFoo.Bar"));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestClassImplementsGenericInterface()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{
class A : {|#0:IFoo<PreviewClass>|}
{
    static void Main(string[] args)
    {
    }
}

[RequiresPreviewFeatures]
interface IFoo<T>
{
}

[RequiresPreviewFeatures]
class PreviewClass
{
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewInterfaceRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_implementPreviewInterfaceMessage, "A", "IFoo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestClassExtendsGenericPreviewClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{
class A : {|#0:PreviewClass<int>|}
{
    static void Main(string[] args)
    {
    }
}

[RequiresPreviewFeatures]
class PreviewClass<T>
{
}
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.DerivesFromPreviewClassRule).WithLocation(0).WithArguments("A", "PreviewClass"));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestGenericClassWithPreviewDependency()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
using Library;
namespace Preview_Feature_Scratch
{

    class Program
    {
        static void Main(string[] args)
        {
            A<Foo> aFooInstance = new A<Foo>();
        }
    }
class A<T> where T : {|#1:IFoo|}, new()
{
    public A()
    {
        IFoo foo = new T();
        {|#0:foo.Bar()|};
    }
}
}";
            string csDependencyCode = @"
using System.Runtime.Versioning; using System;
namespace Library
{
public class Foo : {|#2:IFoo|}
{
    public void {|#3:Bar|}() { }
}

[RequiresPreviewFeatures]
public interface IFoo
{
    void Bar();
}
}";

            var test = SetupDependencyAndTestCSWithOneSourceFile(csInput, csDependencyCode);

            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "Bar"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(1).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "A", "IFoo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewInterfaceRule).WithLocation(2).WithArguments(string.Format((string)DetectPreviewFeatureAnalyzer.s_implementPreviewInterfaceMessage, "Foo", "IFoo"), string.Format((string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewMethodRule).WithLocation(3).WithArguments("Bar", "IFoo.Bar"));
            await test.RunAsync();
        }
    }
}
