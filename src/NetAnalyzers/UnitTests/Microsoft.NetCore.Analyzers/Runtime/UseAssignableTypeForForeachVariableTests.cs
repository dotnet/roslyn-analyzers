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

            await VerifyCS.VerifyAnalyzerAsync(test);
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
            foreach (B item in x)
            {
            }
        }
    }

    class A { }
    class B : A { }
}";

            await VerifyCS.VerifyAnalyzerAsync(test, GetCSharpResultAt(11, 13).WithArguments("A", "B"));
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

            await VerifyCS.VerifyAnalyzerAsync(test);
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

            await VerifyCS.VerifyAnalyzerAsync(test);
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
            foreach (int item in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test, GetCSharpResultAt(11, 13).WithArguments("Int64", "Int32"));
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
            foreach (B item in x)
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

            await VerifyCS.VerifyAnalyzerAsync(test, GetCSharpResultAt(11, 13).WithArguments("A", "B"));
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

            await VerifyCS.VerifyAnalyzerAsync(test);
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
            foreach (string s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test, GetCSharpResultAt(12, 13).WithArguments("IComparable", "String"));
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

            await VerifyCS.VerifyAnalyzerAsync(test);
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
            foreach (B s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test,
                GetCSharpResultAt(11, 13).WithArguments("A", "B"),
                DiagnosticResult.CompilerError("CS0030").WithSpan(11, 13, 11, 20).WithArguments("A", "B"));
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

            await VerifyCS.VerifyAnalyzerAsync(test);
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
            foreach (B s in x)
            {
            }
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test, GetCSharpResultAt(11, 13).WithArguments("A", "B"));
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

            await VerifyCS.VerifyAnalyzerAsync(test, GetCSharpResultAt(11, 13).WithArguments("IComparable", "String"));
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

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);
#pragma warning restore RS0030 // Do not used banned APIs
    }
}