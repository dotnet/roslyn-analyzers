// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotAddFinalizerToMemoryManagerTDerivedTypes,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotAddFinilizerToMemoryManagerTDerivedTypesTests
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
        private void TestMethod(string test)
        {
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
                GetWarningResultAt(7, 17));
        }
        private DiagnosticResult GetWarningResultAt(int line, int column)
           => VerifyCS.Diagnostic(DoNotAddFinalizerToMemoryManagerTDerivedTypes.FinilizerRule)
               .WithLocation(line, column);
    }
}
