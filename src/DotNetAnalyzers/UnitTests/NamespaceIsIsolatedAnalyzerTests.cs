// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace DotNetAnalyzers.IsolateNamespaceAnalyzer.UnitTests
{
    public sealed class NamespaceIsIsolatedAnalyzerTests
    {
        [Fact]
        public async Task CSharp_Instantiate_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        void M() => new IsolatedClass();
    }
}

class Outside
{
    void M() => new Isolated.IsolatedClass();
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 30, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_LocalVarDeclaration_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        void M() { IsolatedClass i; }
    }
}

class Outside
{
    void M() { Isolated.IsolatedClass i; }
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 25, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareField_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        IsolatedClass F;
    }
}

class Outside
{
    public Isolated.IsolatedClass F;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 21, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareField_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        System.Collections.Generic.List<IsolatedClass> F;
    }
}

class Outside
{
    public System.Collections.Generic.List<Isolated.IsolatedClass> F;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 53, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareField_IsolatedType_NestedTypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        System.Collections.Generic.List<System.Action<IsolatedClass>> F;
    }
}

class Outside
{
    public System.Collections.Generic.List<System.Action<Isolated.IsolatedClass>> F;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 67, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareProperty_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        IsolatedClass P { get; }
    }
}

class Outside
{
    public Isolated.IsolatedClass P { get; }
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 21, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareProperty_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        System.Collections.Generic.List<IsolatedClass> P { get; }
    }
}

class Outside
{
    public System.Collections.Generic.List<Isolated.IsolatedClass> P { get; }
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 53, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareEvent_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    delegate void IsolatedDelegate();

    class Inside
    {
        event IsolatedDelegate E { add { } remove { } }
    }
}

class Outside
{
    public event Isolated.IsolatedDelegate E { add { } remove { } }
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 27, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedDelegate", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareEvent_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        event System.Action<IsolatedClass> E { add { } remove { } }
    }
}

class Outside
{
    public event System.Action<Isolated.IsolatedClass> E { add { } remove { } }
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 41, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareEventField_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    delegate void IsolatedDelegate();

    class Inside
    {
        event IsolatedDelegate E;
    }
}

class Outside
{
    public event Isolated.IsolatedDelegate E;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 27, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedDelegate", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareEventField_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        event System.Action<IsolatedClass> E;
    }
}

class Outside
{
    public event System.Action<Isolated.IsolatedClass> E;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 41, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareParameter_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        void M(IsolatedClass i) {}
    }
}

class Outside
{
    public void M(Isolated.IsolatedClass i) {}
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 28, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareParameter_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        void M(System.Action<IsolatedClass> i) {}
    }
}

class Outside
{
    public void M(System.Action<Isolated.IsolatedClass> i) {}
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 42, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DerivedClass_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside : IsolatedClass {}
}

class Outside : Isolated.IsolatedClass {}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(11, 26, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DerivedClass_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside : System.Collections.Generic.List<IsolatedClass> {}
}

class Outside : System.Collections.Generic.List<Isolated.IsolatedClass> {}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(11, 58, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareType_Class_TypeConstraint_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside<T> where T : IsolatedClass {}
}

class Outside<T> where T : Isolated.IsolatedClass {}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(11, 37, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_DeclareType_Struct_TypeConstraint_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    struct Inside<T> where T : IsolatedClass {}
}

struct Outside<T> where T : Isolated.IsolatedClass {}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(11, 38, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticMethodInvocation_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static void M() {}
    }

    class Inside
    {
        void M() => IsolatedClass.M();
    }
}

class Outside
{
    void M() => Isolated.IsolatedClass.M();
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 26, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticMethodInvocation_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        object M() => System.Array.Empty<IsolatedClass>();
    }
}

class Outside
{
    object M() => System.Array.Empty<Isolated.IsolatedClass>();
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 47, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticMethodInvocation_IsolatedType_StaticUsing()
        {
            var staticUsing = "using static Isolated.IsolatedClass;\n";
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static void M() {}
    }

    class Inside
    {
        void N() => M();
    }
}

class Outside
{
    void N() => M();
}
";

            await VerifyCSharpAsync(
                staticUsing + AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(20, 17, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_ExtensionMethodInvocation_IsolatedType()
        {
            var source = $@"
using Isolated;

{_isolateNamespaceAttributeDeclaration}

namespace Isolated
{{
    static class IsolatedClass
    {{
        public static void M(this string s) {{}}
    }}

    class Inside
    {{
        void M1() => """".M();
        void M2() => """"?.M();
    }}

    namespace Outside
    {{
        class C
        {{
            void M1() => """".M();
            void M2() => """"?.M();
        }}
    }}
}}

class Outside
{{
    void M1() => """".M();
    void M2() => """"?.M();
}}

{_isolateNamespaceAttributeSource}";

            await VerifyCSharpAsync(
                source,
                GetCSharpResultAt(23, 26, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "Isolated.Outside"),
                GetCSharpResultAt(24, 29, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "Isolated.Outside"),
                GetCSharpResultAt(31, 18, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"),
                GetCSharpResultAt(32, 21, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticFieldAccess_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static int F;
    }

    class Inside
    {
        void M() => IsolatedClass.F++;
    }
}

class Outside
{
    void M() => Isolated.IsolatedClass.F++;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 26, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticFieldAccess_IsolatedType_StaticUsing()
        {
            var staticUsing = "using static Isolated.IsolatedClass;\n";
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static int F;
    }

    class Inside
    {
        void M() => F++;
    }
}

class Outside
{
    void M() => F++;
}
";

            await VerifyCSharpAsync(
                staticUsing + AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(20, 17, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticPropertyAccess_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static int P { get; }
    }

    class Inside
    {
        int M => IsolatedClass.P;
    }
}

class Outside
{
    int M => Isolated.IsolatedClass.P;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 23, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticPropertyAccess_IsolatedType_StaticUsing()
        {
            var staticUsing = "using static Isolated.IsolatedClass;\n";
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static int P { get; }
    }

    class Inside
    {
        int M => P;
    }
}

class Outside
{
    int M => P;
}
";

            await VerifyCSharpAsync(
                staticUsing + AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(20, 14, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticEventAccess_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static event System.Action E;
    }

    class Inside
    {
        void M() => IsolatedClass.E += () => { };
    }
}

class Outside
{
    void M() => Isolated.IsolatedClass.E += () => { };
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 26, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticEventAccess_IsolatedType_StaticUsing()
        {
            var staticUsing = "using static Isolated.IsolatedClass;\n";
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static event System.Action E;
    }

    class Inside
    {
        void M() => E += () => { };
    }
}

class Outside
{
    void M() => E += () => { };
}
";

            await VerifyCSharpAsync(
                staticUsing + AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(20, 17, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_ConstAccess_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public const int C = 1234;
    }

    class Inside
    {
        int M => IsolatedClass.C;
    }
}

class Outside
{
    int M => Isolated.IsolatedClass.C;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 23, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_ConstAccess_IsolatedType_StaticUsing()
        {
            var staticUsing = "using static Isolated.IsolatedClass;\n";
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public const int C = 1234;
    }

    class Inside
    {
        int N => C;
    }
}

class Outside
{
    int N => C;
}
";

            await VerifyCSharpAsync(
                staticUsing + AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(20, 14, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticMethodGroupAccess_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static void M() {}
    }

    class Inside
    {
        System.Action M => IsolatedClass.M;
    }
}

class Outside
{
    System.Action M => Isolated.IsolatedClass.M;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 33, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticMethodGroupAccess_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static void M() {}
    }

    class Inside
    {
        System.Func<object> M => System.Array.Empty<IsolatedClass>;
    }
}

class Outside
{
    System.Func<object> M => System.Array.Empty<Isolated.IsolatedClass>;
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 58, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_StaticMethodGroupAccess_IsolatedType_StaticUsing()
        {
            var staticUsing = "using static Isolated.IsolatedClass;\n";
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static void M() {}
    }

    class Inside
    {
        System.Action MG => M;
    }
}

class Outside
{
    System.Action MG => M;
}
";

            await VerifyCSharpAsync(
                staticUsing + AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(20, 25, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_Typeof_IsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static void M() {}
    }

    class Inside
    {
        System.Type M => typeof(IsolatedClass);
    }
}

class Outside
{
    System.Type M => typeof(Isolated.IsolatedClass);
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 38, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_Typeof_IsolatedType_TypeParameter()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass
    {
        public static void M() {}
    }

    class Inside
    {
        System.Type M => typeof(System.Action<IsolatedClass>);
    }
}

class Outside
{
    System.Type M => typeof(System.Action<Isolated.IsolatedClass>);
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(19, 52, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_Instantiate_IsolatedType_NonGlobalNamespace()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        void M() => new IsolatedClass();
    }
}

namespace Outside
{
    class Outside
    {
        void M() => new Isolated.IsolatedClass();
    }
}
";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(18, 34, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "Outside"));
        }

        [Fact]
        public async Task CSharp_Instantiate_ArrayOfIsolatedType()
        {
            var source = @"
namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        object M() => new IsolatedClass[0];
    }
}

class Outside
{
    object M() => new Isolated.IsolatedClass[0];
}";

            await VerifyCSharpAsync(
                AddIsolateNamespaceAttribute(source),
                GetCSharpResultAt(16, 32, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_MultiLevelIsolatedNamespace()
        {
            var source = @"
[assembly: IsolateNamespace(""Nested.Isolated"")]

namespace Nested.Isolated
{
    class IsolatedClass {}

    class Inside
    {
        void M() => new IsolatedClass();
    }
}

class Outside
{
    void M() => new Nested.Isolated.IsolatedClass();
}
";

            await VerifyCSharpAsync(
                source + _isolateNamespaceAttributeSource,
                GetCSharpResultAt(16, 37, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Nested.Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_IsolationAttributeCanBeInAnyNamespace()
        {
            var source = @"
[assembly: Foo.IsolateNamespace(""Isolated"")]

namespace Isolated
{
    class IsolatedClass {}

    class Inside
    {
        void M() => new IsolatedClass();
    }
}

class Outside
{
    void M() => new Isolated.IsolatedClass();
}

namespace Foo
{
    class IsolateNamespaceAttribute : System.Attribute { public IsolateNamespaceAttribute(string ns) { } }
}
";

            await VerifyCSharpAsync(
                source,
                GetCSharpResultAt(16, 30, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_IsolationWithAllow()
        {
            var source = @"
[assembly: IsolateNamespace(""Isolated1"", AllowFrom = new[] { ""Isolated2"" })]

namespace Isolated1
{
    class IsolatedClass {}

    class Inside
    {
        void M1() => new Isolated1.IsolatedClass();
        void M2() => new Isolated2.IsolatedClass();
    }
}

namespace Isolated2
{
    class IsolatedClass {}

    class Inside
    {
        void M1() => new Isolated1.IsolatedClass();
        void M2() => new Isolated2.IsolatedClass();
    }
}

class Outside
{
    void M1() => new Isolated1.IsolatedClass(); // FAIL
    void M2() => new Isolated2.IsolatedClass();
}";

            await VerifyCSharpAsync(
                source + _isolateNamespaceAttributeSource,
                GetCSharpResultAt(28, 32, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated1.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_IsolationGroups()
        {
            var source = @"
namespace Isolated1
{
    class IsolatedClass {}

    class Inside
    {
        void M1() => new Isolated1.IsolatedClass();
        void M2() => new Isolated2.IsolatedClass();
    }
}

namespace Isolated2
{
    class IsolatedClass {}

    class Inside
    {
        void M1() => new Isolated1.IsolatedClass();
        void M2() => new Isolated2.IsolatedClass();
    }
}

class Outside
{
    void M1() => new Isolated1.IsolatedClass();
    void M2() => new Isolated2.IsolatedClass();
}";

            await VerifyCSharpAsync(
                AddIsolateNamespaceGroupAttribute(source),
                GetCSharpResultAt(28, 32, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated1.IsolatedClass", "<global namespace>"),
                GetCSharpResultAt(29, 32, NamespaceIsIsolatedAnalyzer.TypeIsInIsolatedNamespaceRule, "Isolated2.IsolatedClass", "<global namespace>"));
        }

        [Fact]
        public async Task CSharp_HandlesConditionalAccessExpressions()
        {
            var source = @"
class C
{
    string M(string s) => s?.ToUpper();
}";

            await VerifyCSharpAsync(AddIsolateNamespaceGroupAttribute(source));
        }

        #region Test support

        private const string _isolateNamespaceAttributeDeclaration = @"[assembly: IsolateNamespace(""Isolated"")]";
        private const string _isolateNamespaceAttributeSource = @"class IsolateNamespaceAttribute : System.Attribute
{
    public IsolateNamespaceAttribute(string ns)
    {
        Namespace = ns;
    }

    public string Namespace { get; }
    public string[] AllowFrom { get; set; }
}";

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor descriptor, string v3, string v4)
        {
            return new DiagnosticResult(descriptor)
                .WithLocation(line, column)
                .WithArguments(v3, v4);
        }

        private static string AddIsolateNamespaceAttribute(string source)
        {
            return $@"{_isolateNamespaceAttributeDeclaration}

{source}

{_isolateNamespaceAttributeSource}";
        }

        private static string AddIsolateNamespaceGroupAttribute(string source)
        {
            return $@"[assembly: IsolateNamespaceGroup(""Isolated1"", ""Isolated2"")]

{source}

class IsolateNamespaceGroupAttribute : System.Attribute {{ public IsolateNamespaceGroupAttribute(params string[] ns) {{ }} }}";
        }

        private static async Task VerifyCSharpAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new CSharpCodeFixTest<NamespaceIsIsolatedAnalyzer, EmptyCodeFixProvider, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { source }
                }
            };

            test.Exclusions &= ~AnalysisExclusions.GeneratedCode;
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        #endregion
    }
}
