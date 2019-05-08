﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.CodeMayHaveMainThreadDependency,
    Roslyn.Diagnostics.Analyzers.CodeMayHaveMainThreadDependencyCodeFix>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.CodeMayHaveMainThreadDependency,
    Roslyn.Diagnostics.Analyzers.CodeMayHaveMainThreadDependencyCodeFix>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class CodeMayHaveMainThreadDependencyAwaitOperationTests
    {
        [Fact]
        public async Task CallerAllowsMainThreadUse_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CallerAllowsMainThreadUse_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As Task
End Interface

Class [Class]
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None, Verified = false)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(
                code,
                VerifyCS.Diagnostic().WithSpan(12, 9, 12, 54).WithSpan(10, 6, 10, 46),
                fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(None, Verified:=False)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(
                code,
                VerifyVB.Diagnostic().WithSpan(12, 9, 12, 54).WithSpan(10, 6, 10, 46),
                fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodWhenCallerCapturesContext_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.Context, Verified = false)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(
                code,
                VerifyCS.Diagnostic().WithSpan(12, 9, 12, 32).WithSpan(10, 6, 10, 49),
                fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodWhenCallerCapturesContext_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.Context)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(Context, Verified:=False)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.Context)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(
                code,
                VerifyVB.Diagnostic().WithSpan(12, 9, 12, 32).WithSpan(10, 6, 10, 49),
                fixedCode);
        }

        [Fact]
        public async Task CorrectUseOfContextCapturingAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.Context)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CorrectUseOfContextCapturingAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.Context)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.Context)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task IncorrectUseOfContextCapturingAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.Context)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.Context)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(
                code,
                VerifyCS.Diagnostic().WithSpan(13, 9, 13, 32).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfContextCapturingAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.Context)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.Context)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(Context)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(
                code,
                VerifyVB.Diagnostic().WithSpan(13, 9, 13, 32).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task MissingConfigureAwaitCapturesContextInAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(
                code,
                VerifyCS.Diagnostic().WithSpan(13, 9, 13, 32).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task MissingConfigureAwaitCapturesContextInAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(Context)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(
                code,
                VerifyVB.Diagnostic().WithSpan(13, 9, 13, 32).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task ConfigureAwaitTrueCapturesContextInAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(true);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(true);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(
                code,
                VerifyCS.Diagnostic().WithSpan(13, 9, 13, 53).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task ConfigureAwaitTrueCapturesContextInAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(True)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(Context)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(True)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(
                code,
                VerifyVB.Diagnostic().WithSpan(13, 9, 13, 53).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompleted_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.Context, AlwaysCompleted = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompleted_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None, AlwaysCompleted:=True)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CorrectUseOfPerInstanceAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CorrectUseOfPerInstanceAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None, PerInstance:=True)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(False)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync([ThreadDependency(ContextDependency.None, Verified = false)] IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(
                code,
                VerifyCS.Diagnostic().WithSpan(13, 15, 13, 18).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None, PerInstance:=True)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None, PerInstance:=True)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(<ThreadDependency(None, Verified:=False)> obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(
                code,
                VerifyVB.Diagnostic().WithSpan(13, 15, 13, 18).WithSpan(11, 6, 11, 46),
                fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodThroughRestrictedInstance_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None, PerInstance = true, Verified = false)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    async Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(
                code,
                VerifyCS.Diagnostic().WithSpan(12, 9, 12, 54).WithSpan(10, 6, 10, 46),
                fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodThroughRestrictedInstance_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(None, PerInstance:=True, Verified:=False)>
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(
                code,
                VerifyVB.Diagnostic().WithSpan(12, 9, 12, 54).WithSpan(10, 6, 10, 46),
                fixedCode);
        }
    }
}
