// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotUseReferenceEqualsWithValueTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotUseReferenceEqualsWithValueTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotUseReferenceEqualsWithValueTypesTests
    {
        [Fact]
        public async Task ReferenceTypesAreOK()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private static bool TestMethod(string test)
        {
            return ReferenceEquals(test, string.Empty);
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Shared Function TestMethod(test as String)
            Return ReferenceEquals(string.Empty, test)
        End Function
    End Class
End Namespace");
        }

        [Fact]
        public async Task LeftArgumentFailsForValueType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private static bool TestMethod(string test)
        {
            return ReferenceEquals(IntPtr.Zero, test);
        }
    }
}",
                GetCSharpResultAt(10, 36, "objA", "System.IntPtr"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Shared Function TestMethod(test as String)
            Return ReferenceEquals(IntPtr.Zero, test)
        End Function
    End Class
End Namespace",
                GetVisualBasicResultAt(7, 36, "objA", "System.IntPtr"));
        }

        [Fact]
        public async Task RightArgumentFailsForValueType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private static bool TestMethod(string test)
        {
            return object.ReferenceEquals(test, 4);
        }
    }
}",
                GetCSharpResultAt(10, 49, "objB", "int"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Shared Function TestMethod(test as String)
            Return Object.ReferenceEquals(test, 4)
        End Function
    End Class
End Namespace",
                GetVisualBasicResultAt(7, 49, "objB", "Integer"));
        }

        [Fact]
        public async Task NoErrorForUnconstrainedGeneric()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private static bool TestMethod<T>(T test, object other)
        {
            return ReferenceEquals(test, other);
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Shared Function TestMethod(Of T)(test as T, other as Object)
            Return ReferenceEquals(test, other)
        End Function
    End Class
End Namespace");
        }

        [Fact]
        public async Task NoErrorForInterfaceConstrainedGeneric()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private static bool TestMethod<T>(T test, object other)
            where T : IDisposable
        {
            return ReferenceEquals(test, other);
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Shared Function TestMethod(Of T As IDisposable)(test as T, other as Object)
            Return ReferenceEquals(test, other)
        End Function
    End Class
End Namespace");
        }

        [Fact]
        public async Task ErrorForValueTypeConstrainedGeneric()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private static bool TestMethod<T>(T test, object other)
            where T : struct
        {
            return ReferenceEquals(test, other);
        }
    }
}",
                GetCSharpResultAt(11, 36, "objA", "T"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Shared Function TestMethod(Of T As Structure)(test as T, other as Object)
            Return ReferenceEquals(test, other)
        End Function
    End Class
End Namespace",
                GetVisualBasicResultAt(7, 36, "objA", "T"));
        }

        [Fact]
        public async Task TwoValueTypesProducesTwoErrors()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace TestNamespace
{
    class TestClass
    {
        private static bool TestMethod<TLeft, TRight>(TLeft test, TRight other)
            where TLeft : struct
            where TRight : struct
        {
            return ReferenceEquals(
                test,
                other);
        }
    }
}",
                GetCSharpResultAt(13, 17, "objA", "TLeft"),
                GetCSharpResultAt(14, 17, "objB", "TRight"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    Class TestClass
        Private Shared Function TestMethod(Of TLeft As Structure, TRight As Structure)(test as TLeft, other as TRight)
            Return ReferenceEquals(test, other)
        End Function
    End Class
End Namespace",
                GetVisualBasicResultAt(7, 36, "objA", "TLeft"),
                GetVisualBasicResultAt(7, 42, "objB", "TRight"));
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, string paramName, string typeName)
            => VerifyCS.Diagnostic(DoNotUseReferenceEqualsWithValueTypesAnalyzer.Rule)
                .WithLocation(line, column)
                .WithArguments(paramName, typeName);

        private DiagnosticResult GetVisualBasicResultAt(int line, int column, string paramName, string callee)
            => VerifyVB.Diagnostic(DoNotUseReferenceEqualsWithValueTypesAnalyzer.Rule)
                .WithLocation(line, column)
                .WithArguments(paramName, callee);
    }
}
