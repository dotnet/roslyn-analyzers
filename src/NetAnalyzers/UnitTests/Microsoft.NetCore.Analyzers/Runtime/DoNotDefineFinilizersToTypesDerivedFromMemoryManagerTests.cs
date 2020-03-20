// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotDefineFinalizersToTypesDerivedFromMemoryManager,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotDefineFinilizersToTypesDerivedFromMemoryManagerTests
    {
        [Fact]
        public async Task ClassNotDerivedFromMemoryManagerOK()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod() { }
    }
}");
        }

        [Fact]
        public async Task ClassHavingFinalizerButNotDerivedFromMemoryManageOK()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private void TestMethod()
        {
        }

        ~TestClass() 
        {
            TestMethod();
        }
    }
}");
        }

        [Fact]
        public async Task ClassDerivedFromMemoryManagerNoFinilizerOK()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Buffers;

namespace TestNamespace
{
    abstract class TestClass<T> : MemoryManager<T>
    {
    }
}");
        }


        [Fact]
        public async Task ClassDerivedFromMemoryManagerWithFinilizerWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Buffers;

 namespace TestNamespace
 {
    class TestClass<T> : MemoryManager<T>
    {
        public override Span<T> GetSpan()
        {
            throw new NotImplementedException();
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override void Unpin() { }

        ~TestClass() { }

        protected override void Dispose(bool disposing) { }
    }
}",
                GetWarningResultAt(21, 10));
        }

        [Fact]
        public async Task ClassIndirectlyDerivedFromMemoryManagerWithFinilizerWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Buffers;

 namespace TestNamespace
 {
    class Deeper<T> : Middle<T>
    {
        ~Deeper() { }
    }

    class Middle<T> : MemoryManager<T>
    {
        public override Span<T> GetSpan()
        {
            throw new NotImplementedException();
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}",
                GetWarningResultAt(9, 10));
        }

        private DiagnosticResult GetWarningResultAt(int line, int column)
           => VerifyCS.Diagnostic(DoNotDefineFinalizersToTypesDerivedFromMemoryManager.Rule)
               .WithLocation(line, column);
    }
}
