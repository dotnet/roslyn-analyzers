// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseAssignableTypeForForeachVariable,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class UseAssignableTypeForForeachVariableTests
    {
        [Fact]
        public async Task NonGenericIntCollection()
        {
            var test = @"
namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            foreach (string item in new A())
            {
            }
        }
    }

    struct A
    {
        public Enumerator GetEnumerator() =>  new Enumerator();

        public struct Enumerator
        {
            public System.IComparable Current => 42;

            public bool MoveNext() => true;
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task ObjectCollectionList()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<object>();
            foreach (string item in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task ObjectCollectionArrayList()
        {
            var test = @"
using System.Collections;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new ArrayList();
            foreach (string item in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task SameType()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<string>();
            foreach (string item in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task CastBaseToChild()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<A>();
            {|#0:foreach|} (B item in x)
            {
            }
        }
    }

    class A { }
    class B : A { }
}";

            await VerifyCS.VerifyCodeFixAsync(test, GetCSharpResultAt(0).WithArguments("A", "B"), test);
        }

        [Fact]
        public async Task ImplicitConversion()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<int>();
            foreach (long item in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task UserDefinedImplicitConversion()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<A>();
            foreach (B item in x)
            {
            }
        }
    }

    class A { }
    class B 
    { 
        public static implicit operator B(A a) => new B();
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task ExplicitConversion()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<long>();
            {|#0:foreach|} (int item in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, GetCSharpResultAt(0).WithArguments("Int64", "Int32"), test);
        }

        [Fact]
        public async Task UserDefinedExplicitConversion()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<A>();
            {|#0:foreach|} (B item in x)
            {
            }
        }
    }

    class A { }
    class B 
    { 
        public static explicit operator B(A a) => new B();
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, GetCSharpResultAt(0).WithArguments("A", "B"), test);
        }

        [Fact]
        public async Task CastChildToBase()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<B>();
            foreach (A item in x)
            {
            }
        }
    }

    class A { }
    class B : A { }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task InterfaceToClass()
        {
            var test = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<IComparable>();
            {|#0:foreach|} (string s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, GetCSharpResultAt(0).WithArguments("IComparable", "String"), test);
        }

        [Fact]
        public async Task ClassToInterfase()
        {
            var test = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<string>();
            foreach (IComparable s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task GenericTypes_Unrelated()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main<A, B>()
        {
            var x = new List<A>();
            {|#0:foreach|} (B s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, DiagnosticResult.CompilerError("CS0030").WithLocation(0).WithArguments("A", "B"), test);
        }

        [Fact]
        public async Task GenericTypes_Valid_Relationship()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main<A, B>() where A : B
        {
            var x = new List<A>();
            foreach (B s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task GenericTypes_Invalid_Relationship()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main<A, B>() where B : A
        {
            var x = new List<A>();
            {|#0:foreach|} (B s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, GetCSharpResultAt(0).WithArguments("A", "B"), test);
        }

        [Fact]
        public async Task CollectionFromMethodResult_Invalid()
        {
            var test = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            foreach (string item in GenerateSequenceAsync())
            {
            }

            IEnumerable<IComparable> GenerateSequenceAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task CollectionFromMethodResult_Valid()
        {
            var test = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            foreach (IComparable item in GenerateSequenceAsync())
            {
            }

            IEnumerable<IComparable> GenerateSequenceAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task DynamicSameType()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<dynamic>();
            foreach (dynamic s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task DynamicToObject()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<dynamic>();
            foreach (object s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task DynamicToString()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<dynamic>();
            {|#0:foreach|} (string s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, GetCSharpResultAt(0).WithArguments("dynamic", "String"), test);
        }

        [Fact]
        public async Task DynamicToVar()
        {
            var test = @"
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<dynamic>();
            foreach (var s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task TupleToVarTuple()
        {
            var test = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<(int, IComparable)>();
            foreach (var (i, j) in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task TupleToSameTuple()
        {
            var test = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<(int, IComparable)>();
            foreach ((int i,  IComparable j) in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, test);
        }

        [Fact]
        public async Task TupleToChildTuple()
        {
            var test = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {   
        void Main()
        {
            var x = new List<(int, IComparable)>();
            foreach ((int i,  {|#0:int j|}) in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, DiagnosticResult.CompilerError("CS0266").WithLocation(0).WithArguments("System.IComparable", "int"), test);
        }

        private static DiagnosticResult GetCSharpResultAt(int i) =>
            VerifyCS.Diagnostic().WithLocation(i);
    }
}