﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace CA2002.Test
{
    using VerifyCS = CSharpCodeFixVerifier<DoNotLockOnObjectsWithWeakIdentityAnalyzerCSharp, EmptyCodeFixProvider>;
    using VerifyVB = VisualBasicCodeFixVerifier<DoNotLockOnObjectsWithWeakIdentityAnalyzerVisualBasic, EmptyCodeFixProvider>;

    public class DoNotLockOnObjectsWithWeakIdentityTests
    {
        [Fact]
        public async Task CA2002TestLockOnStrongType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
            using System;
            public class SomeClass {
                public void Test() {
                    object o = new object();
                    lock (o) {
                        Console.WriteLine();
                    }
                }
            }
");
            await VerifyVB.VerifyAnalyzerAsync(@"
            Imports System
            Public Class SomeClass
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
            public class SomeClass
            {
                public void Test()
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
                    lock (typeof(SomeClass)) { }
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
            GetCSharpResultAt(11, 27, "System.OutOfMemoryException"),
            GetCSharpResultAt(13, 27, "System.StackOverflowException"),
            GetCSharpResultAt(15, 27, "System.ExecutionEngineException"),
            GetCSharpResultAt(16, 27, "System.Threading.Thread"),
            GetCSharpResultAt(17, 27, "System.Type"),
            GetCSharpResultAt(19, 27, "System.Reflection.MemberInfo"),
            GetCSharpResultAt(21, 27, "System.Reflection.ConstructorInfo"),
            GetCSharpResultAt(23, 27, "System.Reflection.ParameterInfo"),
            GetCSharpResultAt(25, 27, "int[]"),
            GetCSharpResultAt(28, 27, "this"));

            await VerifyVB.VerifyAnalyzerAsync(@"
            Imports System
            Public Class SomeClass
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
                    SyncLock GetType(SomeClass)
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
            GetBasicResultAt(11, 30, "System.OutOfMemoryException"),
            GetBasicResultAt(14, 30, "System.StackOverflowException"),
            GetBasicResultAt(17, 30, "System.ExecutionEngineException"),
            GetBasicResultAt(19, 30, "System.Threading.Thread"),
            GetBasicResultAt(21, 30, "System.Type"),
            GetBasicResultAt(24, 30, "System.Reflection.MemberInfo"),
            GetBasicResultAt(27, 30, "System.Reflection.ConstructorInfo"),
            GetBasicResultAt(30, 30, "System.Reflection.ParameterInfo"),
            GetBasicResultAt(33, 30, "Integer()"),
            GetBasicResultAt(38, 30, "Me"));
        }

        [Fact]
        public async Task CA2002TestLockOnWeakIdentitiesWithScope()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
            using System;
            public class SomeClass
            {
                public void Test()
                {
                    string s1 = """";
                    lock ([|s1|]) { }
                    lock ([|""Hello""|]) { }
                    var o1 = new OutOfMemoryException();
                    lock ([|o1|]) { }
                    var o2 = new StackOverflowException();
                    lock ([|o2|]) { }
                    var o3 = new ExecutionEngineException();
                    lock ([|o3|]) { }
                    lock ([|System.Threading.Thread.CurrentThread|]) { }
                    lock ([|typeof(SomeClass)|]) { }
                    System.Reflection.MemberInfo mi = null;
                    lock ([|mi|]) { }
                    System.Reflection.ConstructorInfo ci = null;
                    lock ([|ci|]) { }
                    System.Reflection.ParameterInfo pi = null;
                    lock ([|pi|]) { }
                    int[] values = { 1, 2, 3 };
                    lock ([|values|]) { }
                    System.Reflection.MemberInfo[] values1 = null;
                    lock (values1) { }
                }
            }");

            await VerifyVB.VerifyAnalyzerAsync(@"
            Imports System
            Public Class SomeClass
                Public Sub Test()
                    Dim s1 As String = """"
                    SyncLock [|s1|]
                    End SyncLock
                    SyncLock [|(""Hello"")|]
                    End SyncLock
                    Dim o1 = New OutOfMemoryException()
                    SyncLock [|o1|]
                    End SyncLock
                    Dim o2 = New StackOverflowException()
                    SyncLock [|o2|]
                    End SyncLock
                    Dim o3 = New ExecutionEngineException()
                    SyncLock [|o3|]
                    End SyncLock
                    SyncLock [|System.Threading.Thread.CurrentThread|]
                    End SyncLock
                    SyncLock [|GetType (SomeClass)|]
                    End SyncLock
                    Dim mi As System.Reflection.MemberInfo = Nothing
                    SyncLock [|mi|]
                    End SyncLock
                    Dim ci As System.Reflection.ConstructorInfo = Nothing
                    SyncLock [|ci|]
                    End SyncLock
                    Dim pi As System.Reflection.ParameterInfo = Nothing
                    SyncLock [|pi|]
                    End SyncLock
                    Dim values As Integer() = { 1, 2, 3}
                    SyncLock [|values|]
                    End SyncLock
                    Dim values1 As System.Reflection.MemberInfo() = Nothing
                    SyncLock values1
                    End SyncLock
                End Sub
            End Class");
        }

        [Fact]
        public async Task CA2002_MonitorEnter_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
public class C
{
    public void SomeMethod()
    {
        Monitor.Enter(this);
        Monitor.Enter(""test1"");
        bool b = true;
        Monitor.Enter(this, ref b);
        Monitor.Enter(""test1"", ref b);
    }
}",
                GetCSharpResultAt(7, 23, "C"),
                GetCSharpResultAt(8, 23, "C"),
                GetCSharpResultAt(10, 23, "C"),
                GetCSharpResultAt(11, 23, "C"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Public Class C
    Public Sub SomeMethod()
        Monitor.Enter(Me)
        Monitor.Enter(""test1"")
        Dim b As Boolean = True
        Monitor.Enter(Me, b)
        Monitor.Enter(""test1"", b)
    End Sub
End Class
",
                GetBasicResultAt(5, 23, "C"),
                GetBasicResultAt(6, 23, "C"),
                GetBasicResultAt(8, 23, "C"),
                GetBasicResultAt(9, 23, "C"));
        }

        [Fact]
        public async Task CA2002_MonitorTryEnter_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Threading;
public class C
{
    public void SomeMethod()
    {
        Monitor.TryEnter(this);
        Monitor.TryEnter(""test1"");
        Monitor.TryEnter(this, 42);
        Monitor.TryEnter(""test1"", 42);
        Monitor.TryEnter(this, TimeSpan.FromMilliseconds(42));
        Monitor.TryEnter(""test1"", TimeSpan.FromMilliseconds(42));
        bool b = true;
        Monitor.TryEnter(this, ref b);
        Monitor.TryEnter(""test1"", ref b);
        Monitor.TryEnter(this, 42, ref b);
        Monitor.TryEnter(""test1"", 42, ref b);
    }
}",
                GetCSharpResultAt(8, 26, "C"),
                GetCSharpResultAt(9, 26, "C"),
                GetCSharpResultAt(10, 26, "C"),
                GetCSharpResultAt(11, 26, "C"),
                GetCSharpResultAt(12, 26, "C"),
                GetCSharpResultAt(13, 26, "C"),
                GetCSharpResultAt(15, 26, "C"),
                GetCSharpResultAt(16, 26, "C"),
                GetCSharpResultAt(17, 26, "C"),
                GetCSharpResultAt(18, 26, "C"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Threading
Public Class C
    Public Sub SomeMethod()
        Monitor.TryEnter(Me)
        Monitor.TryEnter(""test1"")
        Monitor.TryEnter(Me, 42)
        Monitor.TryEnter(""test1"", 42)
        Monitor.TryEnter(Me, TimeSpan.FromMilliseconds(42))
        Monitor.TryEnter(""test1"", TimeSpan.FromMilliseconds(42))
        Dim b As Boolean = True
        Monitor.TryEnter(Me, b)
        Monitor.TryEnter(""test1"", b)
        Monitor.TryEnter(Me, 42, b)
        Monitor.TryEnter(""test1"", 42, b)
    End Sub
End Class
",
                GetBasicResultAt(6, 26, "C"),
                GetBasicResultAt(7, 26, "C"),
                GetBasicResultAt(8, 26, "C"),
                GetBasicResultAt(9, 26, "C"),
                GetBasicResultAt(10, 26, "C"),
                GetBasicResultAt(11, 26, "C"),
                GetBasicResultAt(13, 26, "C"),
                GetBasicResultAt(14, 26, "C"),
                GetBasicResultAt(15, 26, "C"),
                GetBasicResultAt(16, 26, "C"));
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