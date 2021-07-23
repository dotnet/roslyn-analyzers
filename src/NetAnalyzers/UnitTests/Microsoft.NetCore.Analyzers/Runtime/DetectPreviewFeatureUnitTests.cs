// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DetectPreviewFeatureAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DetectPreviewFeatureUnitTests
    {
        private static async Task TestCS(string csInput)
        {
            await new VerifyCS.Test
            {
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp10,
                TestState =
                {
                    Sources =
                    {
                        csInput
                    }
                },
                ReferenceAssemblies = AdditionalMetadataReferences.Net60,
            }.RunAsync();
        }

        private static async Task TestCSPreview(string csInput)
        {
            await new VerifyCS.Test
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
            }.RunAsync();
        }

        [Fact]
        public async Task TestPreviewMethodUnaryOperator()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"
    public class Program
    {
        static void Main(string[] args)
        {
            var a = new Fraction();
            var b = {|CA2252:+a|};
        }
    }

    public readonly struct Fraction
    {
        [RequiresPreviewFeatures]
        public static Fraction operator +(Fraction a) => a;
    }
}
";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestCatchPreviewException()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

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
            catch {|CA2252:(DerivedException ex)|}
            {
                throw;
            }
        }
    }
}
";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestArrayOrPreviewTypes()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"
    public class Program
    {
        static void Main(string[] args)
        {
            Lib[] array = {|CA2252:new Lib[] { }|};
        }
    }

    [RequiresPreviewFeatures]
    public class Lib
    {
    }
}
";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestPreviewMethodBinaryOperator()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"
    public class Program
    {
        static void Main(string[] args)
        {
            var a = new Fraction();
            var b = new Fraction();
            b = {|CA2252:b + a|};
        }
    }

    public readonly struct Fraction
    {
        [RequiresPreviewFeatures]
        public static Fraction operator +(Fraction a, Fraction b) => a;
    }
}
";
            await TestCS(csInput);
        }

        [Fact]
        public async Task TestUnmarkedPreviewPropertyCallingPreviewProperty()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"
[RequiresPreviewFeatures]
public class Program
{
    public bool CallSite => UnmarkedPreviewClass.SomeStaticProperty;
}

public class UnmarkedPreviewClass
{
        [RequiresPreviewFeatures]
        public static bool SomeStaticProperty => false;
}
}
";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestUnmarkedPreviewMethodCallingPreviewMethod()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"
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

            await TestCS(csInput);
        }
        [Fact]
        public async Task TestPreviewMethodCallingPreviewMethod()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"
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

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestDerivedClassExtendsUnmarkedClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"
    public partial class UnmarkedPreviewClass
    {
        [RequiresPreviewFeatures]
        public virtual void UnmarkedVirtualMethodInPreviewClass() { }
    }

    public partial class Derived : UnmarkedPreviewClass
    {
        public override void {|CA2252:UnmarkedVirtualMethodInPreviewClass|}()
        {
            throw new NotImplementedException();
        }
    }
}
";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestMethodInvocation_Simple()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    public class Program
    {
        [RequiresPreviewFeatures]
        public virtual void PreviewMethod()
        {

        }

        static void Main(string[] args)
        {
            var prog = new Program();
            {|CA2252:prog.PreviewMethod()|};
        }
    }
}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestMethodInvocation_DeclareDerivedMethod()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

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

        public override void {|CA2252:PreviewMethod|}()
        {
            {|CA2252:base.PreviewMethod()|};
        }
    }
}";

            await TestCS(csInput);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("struct")]
        public async Task TestClassOrStruct(string classOrStruct)
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@$"

    [RequiresPreviewFeatures]
    {classOrStruct} Program
    {{
        static void Main(string[] args)
        {{
            new Program();
        }}
    }}
}}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestAbstractClass()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class {|CA2252:Program|} : AbClass
    {
        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Bar();
            {|CA2252:prog.FooBar()|};
            {|CA2252:prog.BarImplemented()|};
        }

        public override void {|CA2252:Bar|}()
        {
            throw new NotImplementedException();
        }

        [RequiresPreviewFeatures]
        public override void FooBar()
        {
            throw new NotImplementedException();
        }
    }

    [RequiresPreviewFeatures]
    public abstract class AbClass
    {
        [RequiresPreviewFeatures]
        public abstract void Bar();

        [RequiresPreviewFeatures]
        public abstract void FooBar();

        [RequiresPreviewFeatures]
        public void BarImplemented() => throw new NotImplementedException();
    }
}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestUnmarkedPreviewProperty()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program : IProgram
    {
        static void Main(string[] args)
        {
            new Program();
        }

        public bool {|CA2252:MarkedPropertyInInterface|} { get => throw new NotImplementedException(); set => throw new NotImplementedException(); } // [] if not opted in yet
    }

    public interface IProgram
    {
        [RequiresPreviewFeatures]
        bool MarkedPropertyInInterface { get; set; }
    }
}

    ";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestUnmarkedPreviewInterface()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program : IProgram
    {
        static void Main(string[] args)
        {
            new Program();
        }

        public void {|CA2252:MarkedMethodInInterface|}()
        {
            throw new NotImplementedException();
        }        
    }

    public interface IProgram
    {
        [RequiresPreviewFeatures]
        void MarkedMethodInInterface();
    }
}

    ";

            await TestCS(csInput);

        }

        [Fact]
        public async Task TestPreviewLanguageFeatures()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {" +
@"

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
                public static abstract bool {|CA2253:StaticMethod|}();
                public static abstract bool {|CA2253:AProperty|} { {|CA2253:get|}; }
            }
        }

            ";

            await TestCSPreview(csInput);
        }

        [Fact]
        public async Task TestMarkedPreviewInterface()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program : IProgram
    {
        static void Main(string[] args)
        {
            new Program();
        }

        public void {|CA2252:UnmarkedMethodInMarkedInterface|}() { }

    }

    [RequiresPreviewFeatures]
    public interface IProgram
    {
        public void UnmarkedMethodInMarkedInterface() { }
    }
}

    ";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestMarkedEmptyPreviewInterface()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class {|CA2254:Program|} : IProgram
    {
        static void Main(string[] args)
        {
            new Program();
        }
    }

    [RequiresPreviewFeatures]
    public interface IProgram
    {
    }
}

    ";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestInterfaceMethodInvocation()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program : IProgram
    {
        static void Main(string[] args)
        {
            Program progObject = new Program();
            IProgram prog = progObject;
            {|CA2252:prog.Foo()|};
            {|CA2252:prog.FooDelegate()|};
            bool prop = {|CA2252:prog.AProperty|};
            bool anotherProp = {|CA2252:progObject.AnotherInterfaceProperty|};
            Console.WriteLine(""prop.ToString() + anotherProp.ToString()"");
        }

        public IProgram.IProgramDelegate {|CA2252:FooDelegate|}()
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

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestField()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program
    {
        [RequiresPreviewFeatures]
        private bool _field;

        public Program()
        {
            {|CA2252:_field|} = true;
        } 

        static void Main(string[] args)
        {
        }
    }
}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestProperty()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program
    {
        [RequiresPreviewFeatures]
        private bool Foo => true;

        [RequiresPreviewFeatures]
        public virtual bool AProperty => true;

        static void Main(string[] args)
        {
            Program prog = new Program();
            bool foo = {|CA2252:prog.Foo|};

            Derived derived = new Derived();
            bool prop = derived.AProperty;
        }
    }

    class Derived: Program
    {
        public override bool {|CA2252:AProperty|} => true;
    }
}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestDelegate()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program
    {
        [RequiresPreviewFeatures]
        public delegate void Del();

        static void Main(string[] args)
        {
            Del del = {|CA2252:new(() => { })|};
        }
    }
}";

            await TestCS(csInput);

        }

        [Fact]
        public async Task TestEnumValue()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    enum AnEnum
    {
        [RequiresPreviewFeatures]
        Foo,
        Bar
    }

    class Program
    {
        public Program()
        {
        }

        static void Main(string[] args)
        {
            AnEnum fooEnum = {|CA2252:AnEnum.Foo|};
        }
    }
}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestEnumValue_NoDiagnostic()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    enum AnEnum
    {
        Foo,
        [RequiresPreviewFeatures]
        Bar
    }

    class Program
    {
        public Program()
        {
        }

        static void Main(string[] args)
        {
            AnEnum fooEnum = AnEnum.Foo;
        }
    }
}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestEnum()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    [RequiresPreviewFeatures]
    enum AnEnum
    {
        Foo,
        Bar
    }

    class Program
    {
        public Program()
        {
        }

        static void Main(string[] args)
        {
            AnEnum fooEnum = {|CA2252:AnEnum.Foo|};
        }
    }
}";

            await TestCS(csInput);
        }

        [Fact]
        public async Task TestEvent()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{" +
@"

    class Program
    {
        public Program()
        {
        }

        public delegate void SampleEventHandler(object sender, bool e);

        [RequiresPreviewFeatures]
        public static event SampleEventHandler SampleEvent;

        static void Main(string[] args)
        {
            {|CA2252:SampleEvent|}?.Invoke(new Program(), new bool());
        }
    }
}";

            await TestCS(csInput);
        }
    }
}