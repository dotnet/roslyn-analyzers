// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpForwardCancellationTokenToInvocationsAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpForwardCancellationTokenToInvocationsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicForwardCancellationTokenToInvocationsAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicForwardCancellationTokenToInvocationsFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ForwardCancellationTokenToInvocationsTests
    {
        #region No Diagnostic - C#

        [Fact]
        public Task CS_NoDiagnostic_NoParentToken_AsyncNoToken()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M()
    {
        await MethodAsync();
    }
    Task MethodAsync() => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_NoParentToken_SyncNoToken()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
class C
{
    void M()
    {
        MyMethod();
    }
    void MyMethod() {}
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_NoParentToken_TokenDefault()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M()
    {
        await MethodAsync();
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_NoToken()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync();
    }
    Task MethodAsync() => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_OverloadArgumentsDontMatch()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(5, ""Hello world"");
    }
    Task MethodAsync(int i, string s) => Task.CompletedTask;
    Task MethodAsync(int i, CancellationToken ct) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_AlreadyPassingToken()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_PassingTokenFromSource()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        await MethodAsync(cts.Token);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_PassingExplicitDefault()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(default);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_PassingExplicitDefaultCancellationToken()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(default(CancellationToken));
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_PassingExplicitCancellationTokenNone()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(CancellationToken.None);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_OverloadTokenNotLastParameter()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(int x, CancellationToken ct, string s) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_OverloadWithMultipleTokens()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c1, CancellationToken ct2) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_OverloadWithMultipleTokensSeparated()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(int x, CancellationToken c1, string s, CancellationToken ct2) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_NamedTokenUnordered()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(s: ""Hello world"", c: CancellationToken.None, x: 5);
    }
    Task MethodAsync(int x, string s, CancellationToken c) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_Overload_NamedTokenUnordered()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(s: ""Hello world"", c: CancellationToken.None, x: 5);
    }
    Task MethodAsync(int x, string s) => Task.CompletedTask;
    Task MethodAsync(int x, string s, CancellationToken c) => Task.CompletedTask;
}
            ");
        }

        [Fact]
        public Task CS_NoDiagnostic_ExtensionMethodTakesToken()
        {
            // The extension method is in another class
            string originalCode = @"
using System;
using System.Threading;
public static class Extensions
{
    public static void MyMethod(this MyClass mc, CancellationToken c)
    {
    }
}
class C
{
    public void M(CancellationToken ct)
    {
        MyClass mc = new MyClass();
        mc.MyMethod();
    }
}
public class MyClass
{
    public void MyMethod()
    {
    }
}
            ";
            return VerifyCS.VerifyAnalyzerAsync(originalCode);
        }

        #endregion

        #region Diagnostics with no fix = C#

        [Fact]
        public Task CS_AnalyzerOnlyDiagnostic_OverloadWithNamedParametersUnordered()
        {
            // This is a special case that will get a diagnostic but will not get a fix
            // because the fixer does not currently have a way to know the overload's ct parameter name
            // If the ct argument got added at the end without a name, compilation would fail with:
            // CA8323: Named argument 'z' is used out-of-position but is followed by an unnamed argument
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    Task M(CancellationToken ct)
    {
        return [|MethodAsync|](z: ""Hello world"", x: 5, y: true);
    }
    Task MethodAsync(int x, bool y = default, string z = """") => Task.CompletedTask;
    Task MethodAsync(int x, bool y = default, string z = """", CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyAnalyzerAsync(originalCode);
        }

        [Fact]
        public Task CS_AnalyzerOnlyDiagnostic_CancellationTokenSource()
        {
            /*
            CancellationTokenSource has 3 different overloads that take CancellationToken arguments.
            When no ct is passed, because the overload that takes one instance is not setting a default value, then the analyzer considers it the `params`.

            public class CancellationTokenSource : IDisposable
            {
                public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token);
                public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2);
                public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens);
            }

            In C#, the invocation for a static method includes the type and the dot
            */
            string originalCode = @"
using System.Threading;
class C
{
    void M(CancellationToken ct)
    {
        CancellationTokenSource cts = [|CancellationTokenSource.CreateLinkedTokenSource|]();
    }
}
            ";
            return VerifyCS.VerifyAnalyzerAsync(originalCode);
        }

        #endregion

        #region Diagnostics with fix = C#

        [Fact]
        public Task CS_Diagnostic_Class_TokenDefault()
        {
            string originalCode = @"
using System.Threading;
class C
{
    void M(CancellationToken ct)
    {
        [|MyMethod|]();
    }
    int MyMethod(CancellationToken c = default) => 1;
}
            ";
            string fixedCode = @"
using System.Threading;
class C
{
    void M(CancellationToken ct)
    {
        MyMethod(ct);
    }
    int MyMethod(CancellationToken c = default) => 1;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Class_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]().ConfigureAwait(false);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct).ConfigureAwait(false);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_NoAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        [|MethodAsync|]();
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        MethodAsync(ct);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_SaveTask()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Task t = [|MethodAsync|]();
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Task t = MethodAsync(ct);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_ClassStaticMethod_TokenDefault()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]();
    }
    static Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    static Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_ClassStaticMethod_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]().ConfigureAwait(false);
    }
    static Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct).ConfigureAwait(false);
    }
    static Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OtherClass_TokenDefault()
        {
            string originalCode = @"
using System.Threading;
class C
{
    void M(CancellationToken ct)
    {
        O o = new O();
        [|o.MyMethod|]();
    }
}
class O
{
    public int MyMethod(CancellationToken c = default) => 1;
}
            ";
            string fixedCode = @"
using System.Threading;
class C
{
    void M(CancellationToken ct)
    {
        O o = new O();
        o.MyMethod(ct);
    }
}
class O
{
    public int MyMethod(CancellationToken c = default) => 1;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OtherClass_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        O o = new O();
        await [|o.MethodAsync|]();
    }
}
class O
{
    public Task MethodAsync() => Task.CompletedTask;
    public Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        O o = new O();
        await o.MethodAsync(ct);
    }
}
class O
{
    public Task MethodAsync() => Task.CompletedTask;
    public Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OtherClassStaticMethod_TokenDefault()
        {
            // The invocation for a static method includes the type and the dot
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|O.MethodAsync|]();
    }
}
class O
{
    public static Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await O.MethodAsync(ct);
    }
}
class O
{
    public static Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OtherClassStaticMethod_TokenDefault_WithConfigureAwait()
        {
            // The invocation for a static method includes the type and the dot
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|O.MethodAsync|]();
    }
}
class O
{
    static public Task MethodAsync() => Task.CompletedTask;
    static public Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await O.MethodAsync(ct);
    }
}
class O
{
    static public Task MethodAsync() => Task.CompletedTask;
    static public Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Struct_TokenDefault()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
struct S
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
struct S
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Struct_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
struct S
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]().ConfigureAwait(false);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
struct S
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct).ConfigureAwait(false);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OverloadToken()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OverloadToken_WithConfigureAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OverloadTokenDefault()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OverloadTokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]().ConfigureAwait(false);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct).ConfigureAwait(false);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OverloadsArgumentsMatch()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|](5, ""Hello world"");
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
    Task MethodAsync(int x, string s) => Task.CompletedTask;
    Task MethodAsync(int x, string s, CancellationToken ct) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(5, ""Hello world"", ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
    Task MethodAsync(int x, string s) => Task.CompletedTask;
    Task MethodAsync(int x, string s, CancellationToken ct) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_OverloadsArgumentsMatch_WithConfigureAwait()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|](5, ""Hello world"").ConfigureAwait(true);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
    Task MethodAsync(int x, string s) => Task.CompletedTask;
    Task MethodAsync(int x, string s, CancellationToken ct) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(5, ""Hello world"", ct).ConfigureAwait(true);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
    Task MethodAsync(int x, string s) => Task.CompletedTask;
    Task MethodAsync(int x, string s, CancellationToken ct) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_ActionDelegateAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Action<CancellationToken> a = async (CancellationToken token) => await [|MethodAsync|]();
        a(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Action<CancellationToken> a = async (CancellationToken token) => await MethodAsync(token);
        a(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_ActionDelegateNoAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Action<CancellationToken> a = (CancellationToken c) => [|MethodAsync|]();
        a(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Action<CancellationToken> a = (CancellationToken c) => MethodAsync(c);
        a(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_ActionDelegateAwait_WithConfigureAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Action<CancellationToken> a = async (CancellationToken token) => await [|MethodAsync|]().ConfigureAwait(false);
        a(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Action<CancellationToken> a = async (CancellationToken token) => await MethodAsync(token).ConfigureAwait(false);
        a(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_FuncDelegateAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Func<CancellationToken, Task<bool>> f = async (CancellationToken token) =>
        {
            await [|MethodAsync|]();
            return true;
        };
        f(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Func<CancellationToken, Task<bool>> f = async (CancellationToken token) =>
        {
            await MethodAsync(token);
            return true;
        };
        f(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_FuncDelegateAwait_WithConfigureAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Func<CancellationToken, Task<bool>> f = async (CancellationToken token) =>
        {
            await [|MethodAsync|]().ConfigureAwait(true);
            return true;
        };
        f(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        Func<CancellationToken, Task<bool>> f = async (CancellationToken token) =>
        {
            await MethodAsync(token).ConfigureAwait(true);
            return true;
        };
        f(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_FuncDelegateAwaitOutside()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        Func<CancellationToken, Task> f = (CancellationToken c) => [|MethodAsync|]();
        await f(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        Func<CancellationToken, Task> f = (CancellationToken c) => MethodAsync(c);
        await f(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_NestedFunctionAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        async void LocalMethod(CancellationToken token)
        {
            await [|MethodAsync|]();
        }
        LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        async void LocalMethod(CancellationToken token)
        {
            await MethodAsync(token);
        }
        LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_NestedFunctionNoAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        void LocalMethod(CancellationToken token)
        {
            [|MethodAsync|]();
        }
        LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        void LocalMethod(CancellationToken token)
        {
            MethodAsync(token);
        }
        LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_NestedFunctionAwaitOutside()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        Task LocalMethod(CancellationToken token)
        {
            return [|MethodAsync|]();
        }
        await LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        Task LocalMethod(CancellationToken token)
        {
            return MethodAsync(token);
        }
        await LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_NestedFunctionAwait_WithConfigureAwait()
        {
            string originalCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        async void LocalMethod(CancellationToken token)
        {
            await [|MethodAsync|]().ConfigureAwait(false);
        }
        LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
class C
{
    void M(CancellationToken ct)
    {
        async void LocalMethod(CancellationToken token)
        {
            await MethodAsync(token).ConfigureAwait(false);
        }
        LocalMethod(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_AliasTokenInDefault()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync(TokenAlias c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync(TokenAlias c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_AliasTokenInOverload()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(TokenAlias c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(TokenAlias c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_AliasTokenInMethodParameter()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Overload_AliasTokenInMethodParameter()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_AliasTokenInDefaultAndMethodParameter()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync(TokenAlias c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync(TokenAlias c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Overload_AliasTokenInOverloadAndMethodParameter()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await [|MethodAsync|]();
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await MethodAsync(ct);
    }
    Task MethodAsync() => Task.CompletedTask;
    Task MethodAsync(CancellationToken c) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_WithAllDefaultParametersImplicit()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    Task M(CancellationToken ct)
    {
        return [|MethodAsync|]();
    }
    Task MethodAsync(int x = 0, bool y = false, CancellationToken c = default)
    {
        return Task.CompletedTask;
    }
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    Task M(CancellationToken ct)
    {
        return MethodAsync(c: ct);
    }
    Task MethodAsync(int x = 0, bool y = false, CancellationToken c = default)
    {
        return Task.CompletedTask;
    }
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_WithSomeDefaultParameters()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|](5);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(5, c: ct);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_WithNamedParameters()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|](x: 5);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(x: 5, c: ct);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_WithAncestorAliasAndNamedParameters()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await [|MethodAsync|](x: 5);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(TokenAlias ct)
    {
        await MethodAsync(x: 5, c: ct);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_WithMethodArgumentAliasAndNamedParameters()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync|](x: 5);
    }
    Task MethodAsync(int x, bool y = default, TokenAlias c = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using TokenAlias = System.Threading.CancellationToken;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(x: 5, c: ct);
    }
    Task MethodAsync(int x, bool y = default, TokenAlias c = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_Default_WithNamedParametersUnordered()
        {
            string originalCode = @"
using System.Threading;
class C
{
    int M(CancellationToken ct)
    {
        return [|MyMethod|](z: ""Hello world"", x: 5, y: true);
    }
    int MyMethod(int x, bool y = default, string z = """", CancellationToken c = default) => 1;
}
            ";
            // Notice the parameters do NOT get reordered to their official position
            string fixedCode = @"
using System.Threading;
class C
{
    int M(CancellationToken ct)
    {
        return MyMethod(z: ""Hello world"", x: 5, y: true, c: ct);
    }
    int MyMethod(int x, bool y = default, string z = """", CancellationToken c = default) => 1;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_WithLock()
        {
            string originalCode = @"
using System.Threading;
class C
{
    private readonly object lockingObject = new object();
    int M (CancellationToken ct)
    {
        int x;
        lock (lockingObject)
        {
            x = [|MyMethod|](5);
        }
        return x;
    }
    int MyMethod(int x, CancellationToken c = default) => 1;
}
            ";
            string fixedCode = @"
using System.Threading;
class C
{
    private readonly object lockingObject = new object();
    int M (CancellationToken ct)
    {
        int x;
        lock (lockingObject)
        {
            x = MyMethod(5, ct);
        }
        return x;
    }
    int MyMethod(int x, CancellationToken c = default) => 1;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_DereferencePossibleNullReference()
        {
            string originalCode = @"
#nullable enable
using System.Threading;
class C
{
    O? PossiblyNull()
    {
        return null;
    }
    void M(CancellationToken ct)
    {
        O? o = PossiblyNull();
        o?.[|MyMethod|]();
    }
}
class O
{
    public int MyMethod(CancellationToken c = default) => 1;
}
            ";
            string fixedCode = @"
#nullable enable
using System.Threading;
class C
{
    O? PossiblyNull()
    {
        return null;
    }
    void M(CancellationToken ct)
    {
        O? o = PossiblyNull();
        o?.MyMethod(ct);
    }
}
class O
{
    public int MyMethod(CancellationToken c = default) => 1;
}
            ";
            // Nullability is available in C# 8.0+
            return CSharp8VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task CS_Diagnostic_LambdaAndExtensionMethod()
        {
            // In C#, the invocation for a static method includes the type and the dot
            string originalCode = @"
using System;
using System.Threading;
public static class Extensions
{
    public static void Extension(this bool b, Action<int> action)
    {
    }
    public static void MyMethod(this int i, CancellationToken c = default)
    {
    }
}
class C
{
    public void M(CancellationToken ct)
    {
        bool b = false;
        b.Extension((j) =>
        {
            Console.WriteLine(""Hello world"");
            [|j.MyMethod|]();
        });
    }
}
            ";
            string fixedCode = @"
using System;
using System.Threading;
public static class Extensions
{
    public static void Extension(this bool b, Action<int> action)
    {
    }
    public static void MyMethod(this int i, CancellationToken c = default)
    {
    }
}
class C
{
    public void M(CancellationToken ct)
    {
        bool b = false;
        b.Extension((j) =>
        {
            Console.WriteLine(""Hello world"");
            j.MyMethod(ct);
        });
    }
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        #endregion

        #region No Diagnostic - VB

        [Fact]
        public Task VB_NoDiagnostic_NoParentToken_AsyncNoToken()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M()
        Await MethodAsync()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_NoParentToken_SyncNoToken()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Class C
    Private Sub M()
        MyMethod()
    End Sub
    Private Sub MyMethod()
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_NoParentToken_TokenDefault()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M()
        Await MethodAsync()
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_NoToken()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_OverloadArgumentsDontMatch()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(5, ""Hello, world"")
    End Sub
    Private Function MethodAsync(ByVal i As Integer, ByVal s As String) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal i As Integer, ByVal ct As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_AlreadyPassingToken()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_PassingTokenFromSource()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim cts As CancellationTokenSource = New CancellationTokenSource()
        Await MethodAsync(cts.Token)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        // There is no default keyword in VB, must use Nothing instead.
        // The following test method covers the two cases for: `default` and `default(CancellationToken)`
        [Fact]
        public Task VB_NoDiagnostic_PassingExplicitNothing()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(Nothing)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_PassingExplicitCancellationTokenNone()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(CancellationToken.None)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_OverloadTokenNotLastParameter()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal ct As CancellationToken, ByVal s As String) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_OverloadWithMultipleTokens()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c1 As CancellationToken, ByVal ct2 As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_OverloadWithMultipleTokensSeparated()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal c1 As CancellationToken, ByVal s As String, ByVal ct2 As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }


        [Fact]
        public Task VB_NoDiagnostic_NamedTokenUnordered()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(s:=""Hello, world"", c:=CancellationToken.None, x:=5)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String, ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_Overload_NamedTokenUnordered()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(s:=""Hello, world"", c:=CancellationToken.None, x:=5)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String, ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_NoDiagnostic_LambdaAndExtensionMethod()
        {
            // The extension method is in another class
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Runtime.CompilerServices
Module Extensions
    <Extension()>
    Sub MyMethod(ByVal mc As [MyClass], ByVal c As CancellationToken)
    End Sub
End Module
Class C
    Public Sub M(ByVal ct As CancellationToken)
        Dim mc As [MyClass] = New [MyClass]()
        c.MyMethod()
    End Sub
End Class
Public Class [MyClass]
    Public Sub MyMethod()
    End Sub
End Class
            ";
            return VerifyVB.VerifyAnalyzerAsync(originalCode);
        }

        #endregion

        #region Diagnostics with no fix = VB

        [Fact]
        public Task VB_AnalyzerOnlyDiagnostic_OverloadWithNamedParametersUnordered()
        {
            // This is a special case that will get a diagnostic but will not get a fix
            // because the fixer does not currently have a way to know the overload's ct parameter name
            // VB arguments get reordered in their official parameter order, so we could add the ct argument at the end
            // and VB would compile successfully (CA8323 would not be thrown), but that would require separate VB
            // handling in the fixer, so instead, the C# and VB behavior will remain the same
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Function M(ByVal ct As CancellationToken) As Task
        Return [|MethodAsync|](z:=""Hello world"", x:=5, y:=true)
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional z As String = """") As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional z As String = """", ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyAnalyzerAsync(originalCode);
        }

        [Fact]
        public Task VB_AnalyzerOnlyDiagnostic_CancellationTokenSource()
        {
            /*
            CancellationTokenSource has 3 different overloads that take CancellationToken arguments.
            When no ct is passed, because the overload that takes one instance is not setting a default value, then the analyzer considers it the `params`.
            No fix provided.

            public class CancellationTokenSource : IDisposable
            {
                public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token);
                public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2);
                public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens);
            }

            Note: Unlinke C#, in VB the invocation for a static method does not include the type and the dot.
            */
            string originalCode = @"
Imports System.Threading
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim cts As CancellationTokenSource = CancellationTokenSource.[|CreateLinkedTokenSource|]()
    End Sub
End Class
            ";
            return VerifyVB.VerifyAnalyzerAsync(originalCode);
        }

        #endregion

        #region Diagnostics with fix = VB

        [Fact]
        public Task VB_Diagnostic_Class_TokenDefault()
        {
            string originalCode = @"
Imports System.Threading
Class C
    Private Sub M(ByVal ct As CancellationToken)
        [|MyMethod|]()
    End Sub
    Private Function MyMethod(ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Class C
    Private Sub M(ByVal ct As CancellationToken)
        MyMethod(ct)
    End Sub
    Private Function MyMethod(ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Class_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]().ConfigureAwait(False)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct).ConfigureAwait(False)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_NoAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        [|MethodAsync|]()
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        MethodAsync(ct)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_SaveTask()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks

Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim t As Task = [|MethodAsync|]()
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks

Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim t As Task = MethodAsync(ct)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_ClassStaticMethod_TokenDefault()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]()
    End Sub
    Private Shared Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct)
    End Sub
    Private Shared Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_ClassStaticMethod_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]().ConfigureAwait(False)
    End Sub
    Private Shared Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct).ConfigureAwait(False)
    End Sub
    Private Shared Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OtherClass_TokenDefault()
        {
            string originalCode = @"
Imports System.Threading
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim o As O = New O()
        o.[|MyMethod|]()
    End Sub
End Class
Class O
    Public Function MyMethod(ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim o As O = New O()
        o.MyMethod(ct)
    End Sub
End Class
Class O
    Public Function MyMethod(ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OtherClass_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim o As O = New O()
        Await o.[|MethodAsync|]()
    End Sub
End Class
Class O
    Public Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Public Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim o As O = New O()
        Await o.MethodAsync(ct)
    End Sub
End Class
Class O
    Public Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Public Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OtherClassStaticMethod_TokenDefault()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await O.[|MethodAsync|]()
    End Sub
End Class
Class O
    Public Shared Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await O.MethodAsync(ct)
    End Sub
End Class
Class O
    Public Shared Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OtherClassStaticMethod_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim o As O = New O()
        Await o.[|MethodAsync|]()
    End Sub
End Class
Class O
    Public Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Public Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim o As O = New O()
        Await o.MethodAsync(ct)
    End Sub
End Class
Class O
    Public Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Public Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Struct_TokenDefault()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Structure S
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Structure
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Structure S
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Structure
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Struct_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Structure S
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]().ConfigureAwait(False)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Structure
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Structure S
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct).ConfigureAwait(False)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Structure
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OverloadToken()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OverloadToken_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OverloadTokenDefault()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OverloadTokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]().ConfigureAwait(False)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct).ConfigureAwait(False)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OverloadsArgumentsMatch()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|](5, ""Hello, world"")
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String, ByVal ct As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(5, ""Hello, world"", ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String, ByVal ct As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_OverloadsArgumentsMatch_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|](5, ""Hello, world"").ConfigureAwait(True)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String, ByVal ct As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(5, ""Hello, world"", ct).ConfigureAwait(True)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String) As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal x As Integer, ByVal s As String, ByVal ct As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_ActionDelegateAwait()
        {
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim a As Action(Of CancellationToken) = Async Sub(ByVal token As CancellationToken) Await [|MethodAsync|]()
        a(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim a As Action(Of CancellationToken) = Async Sub(ByVal token As CancellationToken) Await MethodAsync(token)
        a(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_ActionDelegateNoAwait()
        {
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim a As Action(Of CancellationToken) = Sub(ByVal c As CancellationToken) [|MethodAsync|]()
        a(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim a As Action(Of CancellationToken) = Sub(ByVal c As CancellationToken) MethodAsync(c)
        a(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_ActionDelegateAwait_WithConfigureAwait()
        {
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim a As Action(Of CancellationToken) = Async Sub(ByVal token As CancellationToken) Await [|MethodAsync|]().ConfigureAwait(False)
        a(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim a As Action(Of CancellationToken) = Async Sub(ByVal token As CancellationToken) Await MethodAsync(token).ConfigureAwait(False)
        a(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_FuncDelegateAwait()
        {
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Task(Of Boolean)) = Async Function(ByVal token As CancellationToken)
                                                                    Await [|MethodAsync|]()
                                                                    Return True
                                                                End Function
        f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Task(Of Boolean)) = Async Function(ByVal token As CancellationToken)
                                                                    Await MethodAsync(token)
                                                                    Return True
                                                                End Function
        f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_FuncDelegateNoAwait()
        {
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Boolean) = Function(ByVal token As CancellationToken)
                                                           [|MethodAsync|]()
                                                           Return True
                                                        End Function
        f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Boolean) = Function(ByVal token As CancellationToken)
                                                           MethodAsync(token)
                                                           Return True
                                                        End Function
        f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_FuncDelegateAwaitOutside()
        {
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Task) = Function(ByVal c As CancellationToken) [|MethodAsync|]()
        Await f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Task) = Function(ByVal c As CancellationToken) MethodAsync(c)
        Await f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_FuncDelegateAwait_WithConfigureAwait()
        {
            string originalCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Task(Of Boolean)) = Async Function(ByVal token As CancellationToken)
                                                                    Await [|MethodAsync|]().ConfigureAwait(True)
                                                                    Return True
                                                                End Function
        f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Sub M(ByVal ct As CancellationToken)
        Dim f As Func(Of CancellationToken, Task(Of Boolean)) = Async Function(ByVal token As CancellationToken)
                                                                    Await MethodAsync(token).ConfigureAwait(True)
                                                                    Return True
                                                                End Function
        f(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        // Nested functions not available in VB:
        // VB_Diagnostic_NestedFunctionAwait
        // VB_Diagnostic_NestedFunctionNoAwait
        // VB_Diagnostic_NestedFunctionAwaitOutside
        // VB_Diagnostic_NestedFunctionAwait_WithConfigureAwait

        [Fact]
        public Task VB_Diagnostic_AliasTokenInOverload()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As TokenAlias) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As TokenAlias) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_AliasTokenInMethodParameter()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Overload_AliasTokenInMethodParameter()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_AliasTokenInDefaultAndMethodParameter()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync(ByVal Optional c As TokenAlias = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync(ByVal Optional c As TokenAlias = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Overload_AliasTokenInOverloadAndMethodParameter()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await [|MethodAsync|]()
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await MethodAsync(ct)
    End Sub
    Private Function MethodAsync() As Task
        Return Task.CompletedTask
    End Function
    Private Function MethodAsync(ByVal c As CancellationToken) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_WithAllDefaultParametersImplicit()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Function M(ByVal ct As CancellationToken) As Task
        Return [|MethodAsync|]()
    End Function
    Private Function MethodAsync(ByVal Optional x As Integer = 0, ByVal Optional y As Boolean = False, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Function M(ByVal ct As CancellationToken) As Task
        Return MethodAsync(c:=ct)
    End Function
    Private Function MethodAsync(ByVal Optional x As Integer = 0, ByVal Optional y As Boolean = False, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_WithSomeDefaultParameters()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|](5)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(5, c:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_WithNamedParameters()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|](x:=5)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(x:=5, c:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_WithAncestorAliasAndNamedParameters()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await [|MethodAsync|](x:=5)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As TokenAlias)
        Await MethodAsync(x:=5, c:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_WithMethodArgumentAliasAndNamedParameters()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync|](x:=5)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As TokenAlias = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports TokenAlias = System.Threading.CancellationToken
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(x:=5, c:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional c As TokenAlias = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_Default_WithNamedParametersUnordered()
        {
            string originalCode = @"
Imports System.Threading
Class C
    Private Function M(ByVal ct As CancellationToken) As Integer
        Return [|MyMethod|](z:=""Hello world"", x:=5, y:=true)
    End Function
    Private Function MyMethod(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional z As String = """", ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            // Notice the parameters get reordered to their official position
            string fixedCode = @"
Imports System.Threading
Class C
    Private Function M(ByVal ct As CancellationToken) As Integer
        Return MyMethod(x:=5, y:=true, z:=""Hello world"", c:=ct)
    End Function
    Private Function MyMethod(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional z As String = """", ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_WithLock()
        {
            string originalCode = @"
Imports System.Threading
Class C
    Private ReadOnly lockingObject As Object = New Object()
    Private Function M(ByVal ct As CancellationToken) As Integer
        Dim x As Integer
        SyncLock lockingObject
            x = [|MyMethod|](5)
        End SyncLock
        Return x
    End Function
    Private Function MyMethod(ByVal x As Integer, ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Class C
    Private ReadOnly lockingObject As Object = New Object()
    Private Function M(ByVal ct As CancellationToken) As Integer
        Dim x As Integer
        SyncLock lockingObject
            x = MyMethod(5, ct)
        End SyncLock
        Return x
    End Function
    Private Function MyMethod(ByVal x As Integer, ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        [Fact]
        public Task VB_Diagnostic_DereferencePossibleNullReference()
        {
            string originalCode = @"
Imports System.Threading
Class C
    Private Function PossiblyNull() As O?
        Return Nothing
    End Function
    Private Sub M(ByVal ct As CancellationToken)
        Dim o As O? = PossiblyNull()
        o?.[|MyMethod|]()
    End Sub
End Class
Structure O
    Public Function MyMethod(ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Structure
            ";
            string fixedCode = @"
Imports System.Threading
Class C
    Private Function PossiblyNull() As O?
        Return Nothing
    End Function
    Private Sub M(ByVal ct As CancellationToken)
        Dim o As O? = PossiblyNull()
        o?.MyMethod(ct)
    End Sub
End Class
Structure O
    Public Function MyMethod(ByVal Optional c As CancellationToken = Nothing) As Integer
        Return 1
    End Function
End Structure
            ";
            // Nullability is available in C# 8.0+
            return VB16VerifyCodeFixAsync(originalCode, fixedCode);
        }

        #endregion

        #region Helpers

        private static async Task VB16VerifyCodeFixAsync(string originalCode, string fixedCode)
        {
            var test = new VerifyVB.Test
            {
                TestCode = originalCode,
                LanguageVersion = CodeAnalysis.VisualBasic.LanguageVersion.VisualBasic16,
                FixedCode = fixedCode
            };

            test.ExpectedDiagnostics.AddRange(DiagnosticResult.EmptyDiagnosticResults);
            await test.RunAsync();
        }

        private static async Task CSharp8VerifyCodeFixAsync(string originalCode, string fixedCode)
        {
            var test = new VerifyCS.Test
            {
                TestCode = originalCode,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp8,
                FixedCode = fixedCode
            };

            test.ExpectedDiagnostics.AddRange(DiagnosticResult.EmptyDiagnosticResults);
            await test.RunAsync();
        }

        #endregion
    }
}