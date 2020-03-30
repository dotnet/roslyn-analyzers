// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotDefineFinalizersToTypesDerivedFromMemoryManager,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotDefineFinalizersToTypesDerivedFromMemoryManager,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotDefineFinalizersToTypesDerivedFromMemoryManagerTests
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

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task ClassHavingFinalizerButNotDerivedFromMemoryManagerOK()
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

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Sub TestMethod()
        End Sub

        Protected Overrides Sub Finalize()
            TestMethod()
        End Sub
    End Class
End Namespace");
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

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Buffers

Namespace TestNamespace
    MustInherit Class TestClass(Of T)
        Inherits MemoryManager(Of T)
    End Class
End Namespace
");
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

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Buffers

Namespace TestNamespace
    MustInherit Class TestClass(Of T)
        Inherits MemoryManager(Of T)

        Public Overrides Function Pin(ByVal Optional elementIndex As Integer = 0) As MemoryHandle
            Throw New NotImplementedException()
        End Function

        Public Overrides Sub Unpin()
        End Sub

        Protected Overrides Sub Finalize()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        End Sub
    End Class
End Namespace",
                GetVisualBasicWarningResultAt(16, 33));
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

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Buffers

Namespace TestNamespace
        MustInherit Class Deeper(Of T)
        Inherits Middle(Of T)

        Protected Overrides Sub Finalize()
        End Sub
    End Class

    MustInherit Class Middle(Of T)
        Inherits MemoryManager(Of T)

        Public Overrides Function Pin(ByVal Optional elementIndex As Integer = 0) As MemoryHandle
            Throw New NotImplementedException()
        End Function

        Public Overrides Sub Unpin()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        End Sub
    End Class
End Namespace",
                GetVisualBasicWarningResultAt(9, 33));
        }

        private DiagnosticResult GetWarningResultAt(int line, int column)
            => VerifyCS.Diagnostic(DoNotDefineFinalizersToTypesDerivedFromMemoryManager.Rule)
                .WithLocation(line, column);

        private DiagnosticResult GetVisualBasicWarningResultAt(int line, int column)
            => VerifyVB.Diagnostic(DoNotDefineFinalizersToTypesDerivedFromMemoryManager.Rule)
                .WithLocation(line, column);
    }
}
