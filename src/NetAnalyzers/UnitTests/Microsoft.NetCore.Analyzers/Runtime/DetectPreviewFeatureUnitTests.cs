// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DetectPreviewFeatureAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DetectPreviewFeatureAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DetectPreviewFeatureUnitTests
    {
        private const string setupPreviewAttribute = @"
    [AttributeUsage(AttributeTargets.Assembly |
                AttributeTargets.Module |
                AttributeTargets.Class |
                AttributeTargets.Struct |
                AttributeTargets.Delegate |
                AttributeTargets.Interface |
                AttributeTargets.Enum |
                AttributeTargets.Constructor |
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Field |
                AttributeTargets.Event, Inherited = false)]
    public sealed class RequiresPreviewFeaturesAttribute : Attribute
    {
        public RequiresPreviewFeaturesAttribute() { }
    }

";

        [Fact]
        public async Task TestMethodInvocation_Simple()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
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
            [|prog.PreviewMethod()|];
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestMethodInvocation_DeclareDerivedMethod()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
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

        public override void PreviewMethod()
        {
            [|base.PreviewMethod()|];
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestMethodInvocation_DerivedNotMarked()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@"

    public class Program
    {
        [RequiresPreviewFeatures]
        public virtual void BaseMarked()
        {

        }

        static void Main(string[] args)
        {
            var derived = new Derived();
            derived.BaseMarked();
        }
    }

    public class Derived : Program
    {
        public Derived() : base()
        {
        }

        public override void BaseMarked()
        {
            [|[|base.BaseMarked()|]|];
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestConstructor()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@"

    class Program
    {
        [RequiresPreviewFeatures]
        public Program()
        {

        }

        static void Main(string[] args)
        {
            [|new Program()|];
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Theory]
        [InlineData("class")]
        [InlineData("struct")]
        public async Task TestClassOrStruct(string classOrStruct)
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@$"

    [RequiresPreviewFeatures]
    {classOrStruct} Program
    {{
        static void Main(string[] args)
        {{
            [|new Program()|];
        }}
    }}
}}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestAbstractClass()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@"

    class Program : AbClass
    {
        static void Main(string[] args)
        {
            Program prog = [|new Program()|];
            [|prog.Bar()|];
            [|[|prog.FooBar()|]|];
            [|prog.BarImplemented()|];
        }

        public override void Bar()
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
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestInterface()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@"

    class Program : IProgram
    {
        static void Main(string[] args)
        {
            [|new Program()|];
        }
    }

    [RequiresPreviewFeatures]
    interface IProgram
    {
    }
}

    ";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestInterfaceMethodInvocation()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@"

    class Program : IProgram
    {
        static void Main(string[] args)
        {
            Program progObject = new Program();
            IProgram prog = progObject;
            [|prog.Foo()|];
            [|prog.FooDelegate()|];
            bool prop = [|prog.AProperty|];
            bool anotherProp = [|progObject.AnotherInterfaceProperty|];
            Console.WriteLine(""prop.ToString() + anotherProp.ToString()"");
        }

        public IProgram.IProgramDelegate FooDelegate()
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
            await new VerifyCS.Test
            {
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestState =
                {
                    Sources =
                    {
                        csInput
                    }
                },
            }.RunAsync();
        }

        [Fact]
        public async Task TestField()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@"

    class Program
    {
        [RequiresPreviewFeatures]
        private bool _field;

        public Program()
        {
            [|_field|] = true;
        } 

        static void Main(string[] args)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestProperty()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
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
            bool foo = [|prog.Foo|];

            Derived derived = new Derived();
            bool prop = [|derived.AProperty|];
        }
    }

    class Derived: Program
    {
        public override bool AProperty => true;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestDelegate()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
@"

    class Program
    {
        [RequiresPreviewFeatures]
        public delegate void Del();

        static void Main(string[] args)
        {
            Del del = [|new(() => { })|];
        }
    }
}";
            await new VerifyCS.Test
            {
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestState =
                {
                    Sources =
                    {
                        csInput
                    }
                },
            }.RunAsync();

        }

        [Fact]
        public async Task TestEnumValue()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
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
            AnEnum fooEnum = [|AnEnum.Foo|];
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);
        }

        [Fact]
        public async Task TestEnumValue_NoDiagnostic()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
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
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestEnum()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
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
            AnEnum fooEnum = [|AnEnum.Foo|];
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }

        [Fact]
        public async Task TestEvent()
        {
            string csInput = @" 
using System;
namespace Preview_Feature_Scratch
{" + setupPreviewAttribute +
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
            [|SampleEvent|]?.Invoke(new Program(), new bool());
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(csInput);

        }
    }
}