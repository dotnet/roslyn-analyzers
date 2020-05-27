// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ForwardCancellationTokenToAsyncMethodsAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpForwardCancellationTokenToAsyncMethodsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ForwardCancellationTokenToAsyncMethodsAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicForwardCancellationTokenToAsyncMethodsFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ForwardCancellationTokenToAsyncMethodsTests
    {
        #region No Diagnostic - C#

        [Fact]
        public Task CS_NoDiagnostic_NoParentToken_NoToken()
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

        #endregion

        #region Diagnostic = C#

        [Fact]
        public Task CS_Diagnostic_Class_TokenDefault()
        {
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|MethodAsync()|];
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
        await MethodAsync(ct);
    }
    Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
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
        await [|MethodAsync()|].ConfigureAwait(false);
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
        [|MethodAsync()|];
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
        Task t = [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|].ConfigureAwait(false);
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
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        O o = new O();
        await [|o.MethodAsync()|];
    }
}
class O
{
    public Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
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
    public Task MethodAsync(CancellationToken c = default) => Task.CompletedTask;
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
        await [|o.MethodAsync()|];
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
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|O.MethodAsync()|];
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
            string originalCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await [|O.MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|].ConfigureAwait(false);
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|].ConfigureAwait(false);
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
        await [|MethodAsync(5, ""Hello world"")|];
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
        await [|MethodAsync(5, ""Hello world"")|].ConfigureAwait(true);
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
        Action<CancellationToken> a = async (CancellationToken token) => await [|MethodAsync()|];
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
        Action<CancellationToken> a = (CancellationToken c) => [|MethodAsync()|];
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
        Action<CancellationToken> a = async (CancellationToken token) => await [|MethodAsync()|].ConfigureAwait(false);
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
            await [|MethodAsync()|];
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
            await [|MethodAsync()|].ConfigureAwait(true);
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
        Func<CancellationToken, Task> f = (CancellationToken c) => [|MethodAsync()|];
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
            await [|MethodAsync()|];
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
            [|MethodAsync()|];
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
            return [|MethodAsync()|];
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
            await [|MethodAsync()|].ConfigureAwait(false);
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        await [|MethodAsync()|];
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
        return [|MethodAsync()|];
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
        await [|MethodAsync(5)|];
    }
    Task MethodAsync(int x, bool y = default, CancellationToken ct = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(5, ct: ct);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken ct = default) => Task.CompletedTask;
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
        await [|MethodAsync(x: 5)|];
    }
    Task MethodAsync(int x, bool y = default, CancellationToken ct = default) => Task.CompletedTask;
}
            ";
            string fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
class C
{
    async void M(CancellationToken ct)
    {
        await MethodAsync(x: 5, ct: ct);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken ct = default) => Task.CompletedTask;
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
        await [|MethodAsync(x: 5)|];
    }
    Task MethodAsync(int x, bool y = default, CancellationToken ct = default) => Task.CompletedTask;
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
        await MethodAsync(x:5, ct: ct);
    }
    Task MethodAsync(int x, bool y = default, CancellationToken ct = default) => Task.CompletedTask;
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
        await [|MethodAsync(x: 5)|];
    }
    Task MethodAsync(int x, bool y = default, TokenAlias ct = default) => Task.CompletedTask;
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
        await MethodAsync(x: 5, ct: ct);
    }
    Task MethodAsync(int x, bool y = default, TokenAlias ct = default) => Task.CompletedTask;
}
            ";
            return VerifyCS.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        #endregion

        #region No Diagnostic - VB

        [Fact]
        public Task VB_NoDiagnostic_NoParentToken_NoToken()
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

        #endregion

        #region Diagnostic = VB

        [Fact]
        public Task VB_Diagnostic_Class_TokenDefault()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync()|]
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
        public Task VB_Diagnostic_Class_TokenDefault_WithConfigureAwait()
        {
            string originalCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync()|].ConfigureAwait(False)
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
        [|MethodAsync()|]
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
        Dim t As Task = [|MethodAsync()|]
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|].ConfigureAwait(False)
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
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Dim o As O = New O()
        Await [|o.MethodAsync()|]
    End Sub
End Class
Class O
    Public Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
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
    Public Function MethodAsync(ByVal Optional c As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
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
        Await [|o.MethodAsync()|]
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
        Await [|O.MethodAsync()|]
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
        Await [|o.MethodAsync()|]
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|].ConfigureAwait(False)
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|].ConfigureAwait(False)
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
        Await [|MethodAsync(5, ""Hello, world"")|]
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
        Await [|MethodAsync(5, ""Hello, world"")|].ConfigureAwait(True)
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
        Dim a As Action(Of CancellationToken) = Async Sub(ByVal token As CancellationToken) Await [|MethodAsync()|]
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
        Dim a As Action(Of CancellationToken) = Sub(ByVal c As CancellationToken) [|MethodAsync()|]
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
        Dim a As Action(Of CancellationToken) = Async Sub(ByVal token As CancellationToken) Await [|MethodAsync()|].ConfigureAwait(False)
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
                                                                    Await [|MethodAsync()|]
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
                                                           [|MethodAsync()|]
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
        Dim f As Func(Of CancellationToken, Task) = Function(ByVal c As CancellationToken) [|MethodAsync()|]
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
                                                                    Await [|MethodAsync()|].ConfigureAwait(True)
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|]
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
        Await [|MethodAsync()|]
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
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync()|]
    End Sub
    Private Function MethodAsync(ByVal Optional x As Integer = 0, ByVal Optional y As Boolean = False, ByVal Optional ct As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(ct:=ct)
    End Sub
    Private Function MethodAsync(ByVal Optional x As Integer = 0, ByVal Optional y As Boolean = False, ByVal Optional ct As CancellationToken = Nothing) As Task
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
        Await [|MethodAsync(5)|]
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(5, ct:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
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
        Await [|MethodAsync(x:=5)|]
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(x:=5, ct:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
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
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await [|MethodAsync(y:=true, x:=5)|]
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            // Notice the parameters get reordered in their official position
            string fixedCode = @"
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Private Async Sub M(ByVal ct As CancellationToken)
        Await MethodAsync(x:=5, y:=true, ct:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
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
        Await [|MethodAsync(x:=5)|]
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
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
        Await MethodAsync(x:=5, ct:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As CancellationToken = Nothing) As Task
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
        Await [|MethodAsync(x:=5)|]
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As TokenAlias = Nothing) As Task
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
        Await MethodAsync(x:=5, ct:=ct)
    End Sub
    Private Function MethodAsync(ByVal x As Integer, ByVal Optional y As Boolean = false, ByVal Optional ct As TokenAlias = Nothing) As Task
        Return Task.CompletedTask
    End Function
End Class
            ";
            return VerifyVB.VerifyCodeFixAsync(originalCode, fixedCode);
        }

        #endregion
    }
}