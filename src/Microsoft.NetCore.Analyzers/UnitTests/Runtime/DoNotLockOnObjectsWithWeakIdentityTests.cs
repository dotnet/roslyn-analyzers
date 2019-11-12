// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotLockOnObjectsWithWeakIdentityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotLockOnObjectsWithWeakIdentityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotLockOnObjectsWithWeakIdentityTests
    {
        [Fact]
        public async Task CA2002TestLockOnStrongType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
            using System;
            public class foo {
                public async Task Test() {
                    object o = new object();
                    lock (o) {
                        Console.WriteLine();
                    }
                }
            }
");
            await VerifyVB.VerifyAnalyzerAsync(@"
            Imports System
            Public Class foo
                Public Sub Test()
                    Dim o As new Object()
                    SyncLock o
                        Console.WriteLine()
                    End SyncLock
                End Sub
            End Class
");
        }

        [Fact]
        public async Task CA2002TestLockOnWeakIdentities()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
            using System;
            public class foo
            {
                public async Task Test()
                {
                    string s1 = """";
                    lock (s1) { }
                    lock (""Hello"") { }

                    var o1 = new OutOfMemoryException();
                    lock (o1) { }
                    var o2 = new StackOverflowException();
                    lock (o2) { }
                    var o3 = new ExecutionEngineException();
                    lock (o3) { }

                    lock (System.Threading.Thread.CurrentThread) { }

                    lock (typeof(foo)) { }

                    System.Reflection.MemberInfo mi = null;
                    lock (mi) { }

                    System.Reflection.ConstructorInfo ci = null;
                    lock (ci) { }

                    System.Reflection.ParameterInfo pi = null;
                    lock (pi) { }

                    int[] values = { 1, 2, 3 };
                    lock (values) { }

                    System.Reflection.MemberInfo[] values1 = null;
                    lock (values1) { }

                    lock (this) { }
                }
            }
            ",
            GetCSharpResultAt(8, 27, "string"),
            GetCSharpResultAt(9, 27, "string"),
            GetCSharpResultAt(12, 27, "System.OutOfMemoryException"),
            GetCSharpResultAt(14, 27, "System.StackOverflowException"),
            GetCSharpResultAt(16, 27, "System.ExecutionEngineException"),
            GetCSharpResultAt(18, 27, "System.Threading.Thread"),
            GetCSharpResultAt(20, 27, "System.Type"),
            GetCSharpResultAt(23, 27, "System.Reflection.MemberInfo"),
            GetCSharpResultAt(26, 27, "System.Reflection.ConstructorInfo"),
            GetCSharpResultAt(29, 27, "System.Reflection.ParameterInfo"),
            GetCSharpResultAt(32, 27, "int[]"),
            GetCSharpResultAt(37, 27, "this"));

            await VerifyVB.VerifyAnalyzerAsync(@"
            Imports System
            Public Class foo
                Public Sub Test()
                    Dim s1 As String = """"
                    SyncLock s1
                    End SyncLock
                    SyncLock (""Hello"")
                    End SyncLock

                    Dim o1 = New OutOfMemoryException()
                    SyncLock o1
                    End SyncLock
                    Dim o2 = New StackOverflowException()
                    SyncLock o2
                    End SyncLock
                    Dim o3 = New ExecutionEngineException()
                    SyncLock o3
                    End SyncLock

                    SyncLock System.Threading.Thread.CurrentThread
                    End SyncLock

                    SyncLock GetType(foo)
                    End SyncLock

                    Dim mi As System.Reflection.MemberInfo = Nothing
                    SyncLock mi
                    End SyncLock

                    Dim ci As System.Reflection.ConstructorInfo = Nothing
                    SyncLock ci
                    End SyncLock

                    Dim pi As System.Reflection.ParameterInfo = Nothing
                    SyncLock pi
                    End SyncLock

                    Dim values As Integer() = { 1, 2, 3}
                    SyncLock values
                    End SyncLock

                    Dim values1 As System.Reflection.MemberInfo() = Nothing
                    SyncLock values1
                    End SyncLock

                    SyncLock Me
                    End SyncLock
                End Sub
            End Class",
            GetBasicResultAt(6, 30, "String"),
            GetBasicResultAt(8, 30, "String"),
            GetBasicResultAt(12, 30, "System.OutOfMemoryException"),
            GetBasicResultAt(15, 30, "System.StackOverflowException"),
            GetBasicResultAt(18, 30, "System.ExecutionEngineException"),
            GetBasicResultAt(21, 30, "System.Threading.Thread"),
            GetBasicResultAt(24, 30, "System.Type"),
            GetBasicResultAt(28, 30, "System.Reflection.MemberInfo"),
            GetBasicResultAt(32, 30, "System.Reflection.ConstructorInfo"),
            GetBasicResultAt(36, 30, "System.Reflection.ParameterInfo"),
            GetBasicResultAt(40, 30, "Integer()"),
            GetBasicResultAt(47, 30, "Me"));
        }

        [Fact]
        public async Task CA2002TestLockOnWeakIdentitiesWithScope()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
            using System;
            public class foo
            {
                public async Task Test()
                {
                    string s1 = """";
                    lock (s1) { }
                    lock (""Hello"") { }

                    [|var o1 = new OutOfMemoryException();
                    lock (o1) { }
                    var o2 = new StackOverflowException();
                    lock (o2) { }
                    var o3 = new ExecutionEngineException();
                    lock (o3) { }|]

                    lock (System.Threading.Thread.CurrentThread) { }

                    lock (typeof(foo)) { }

                    System.Reflection.MemberInfo mi = null;
                    lock (mi) { }

                    System.Reflection.ConstructorInfo ci = null;
                    lock (ci) { }

                    System.Reflection.ParameterInfo pi = null;
                    lock (pi) { }

                    int[] values = { 1, 2, 3 };
                    lock (values) { }

                    System.Reflection.MemberInfo[] values1 = null;
                    lock (values1) { }
                }
            }
            ",
            GetCSharpResultAt(12, 27, "System.OutOfMemoryException"),
            GetCSharpResultAt(14, 27, "System.StackOverflowException"),
            GetCSharpResultAt(16, 27, "System.ExecutionEngineException"));

            await VerifyVB.VerifyAnalyzerAsync(@"
            Imports System
            Public Class foo
                Public Sub Test()
                    Dim s1 As String = """"
                    SyncLock s1
                    End SyncLock
                    SyncLock (""Hello"")
                    End SyncLock

                    [|Dim o1 = New OutOfMemoryException()
                    SyncLock o1
                    End SyncLock
                    Dim o2 = New StackOverflowException()
                    SyncLock o2
                    End SyncLock
                    Dim o3 = New ExecutionEngineException()
                    SyncLock o3
                    End SyncLock|]

                    SyncLock System.Threading.Thread.CurrentThread
                    End SyncLock

                    SyncLock GetType (foo)
                    End SyncLock

                    Dim mi As System.Reflection.MemberInfo = Nothing
                    SyncLock mi
                    End SyncLock

                    Dim ci As System.Reflection.ConstructorInfo = Nothing
                    SyncLock ci
                    End SyncLock

                    Dim pi As System.Reflection.ParameterInfo = Nothing
                    SyncLock pi
                    End SyncLock

                    Dim values As Integer() = { 1, 2, 3}
                    SyncLock values
                    End SyncLock

                    Dim values1 As System.Reflection.MemberInfo() = Nothing
                    SyncLock values1
                    End SyncLock
                End Sub
            End Class",
            GetBasicResultAt(12, 30, "System.OutOfMemoryException"),
            GetBasicResultAt(15, 30, "System.StackOverflowException"),
            GetBasicResultAt(18, 30, "System.ExecutionEngineException"));
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
           => VerifyCS.Diagnostic()
               .WithLocation(line, column)
               .WithArguments(arguments);

        private static DiagnosticResult GetBasicResultAt(int line, int column, params string[] arguments)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
