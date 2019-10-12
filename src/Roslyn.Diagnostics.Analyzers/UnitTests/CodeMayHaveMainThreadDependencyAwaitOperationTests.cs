// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync().ConfigureAwait(false)|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [return: ThreadDependency(ContextDependency.None, Verified = false)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
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
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Await obj.MethodAsync().ConfigureAwait(false)|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As <ThreadDependency(None, Verified:=False)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodWithReturn_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task<int> MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task<int> OperationAsync(IInterface obj) {
        return [|await obj.MethodAsync().ConfigureAwait(false)|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [return: ThreadDependency(ContextDependency.None, Verified = false)]
    Task<int> MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task<int> OperationAsync(IInterface obj) {
        return await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodWithReturn_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As Task(Of Integer)
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task(Of Integer)
        Return [|Await obj.MethodAsync().ConfigureAwait(false)|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As <ThreadDependency(None, Verified:=False)> Task(Of Integer)
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task(Of Integer)
        Return Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
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
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [return: ThreadDependency(ContextDependency.Context, Verified = false)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
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
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.Context)> Task
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As <ThreadDependency(Context, Verified:=False)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.Context)> Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task PerInstanceFieldMustBeExplicit_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    IInterface field;

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    async Task OperationAsync() {
        await [|field|].MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None, PerInstance = true, Verified = false)]
    IInterface field;

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    async Task OperationAsync() {
        await field.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task PerInstanceFieldMustBeExplicit_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    Dim field As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
        Await [|field|].MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(None, PerInstance:=True, Verified:=False)>
    Dim field As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
        Await field.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceField_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    IInterface field;

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await [|field|].MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None, Verified = false)]
    IInterface field;

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await field.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceField_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    Dim field As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
        Await [|field|].MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(None, Verified:=False)>
    Dim field As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
        Await field.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceProperty_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    IInterface Property { get; }

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await [|Property|].MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None, Verified = false)]
    IInterface Property { get; }

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await Property.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceProperty_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    ReadOnly Property p As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
        Await [|p|].MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(None, Verified:=False)>
    ReadOnly Property p As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
        Await p.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task CorrectUseOfContextCapturingAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
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
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.Context)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.Context)> Task
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
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfContextCapturingAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.Context)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.Context)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(Context)> Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task MissingConfigureAwaitCapturesContextInAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task MissingConfigureAwaitCapturesContextInAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(Context)> Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task ConfigureAwaitTrueCapturesContextInAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync().ConfigureAwait(true)|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(true);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task ConfigureAwaitTrueCapturesContextInAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Await obj.MethodAsync().ConfigureAwait(True)|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(Context)> Task
        Await obj.MethodAsync().ConfigureAwait(True)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompleted_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.Context, AlwaysCompleted = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
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
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, AlwaysCompleted:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromResult_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task<int> OperationAsync() {
        return await Task.FromResult(0);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromResult_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task(Of Integer)
        Return Await Task.FromResult(0)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromCanceled_CSharp()
        {
            var code = @"
using System.Threading;
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await Task.FromCanceled(CancellationToken.None);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromCanceled_VisualBasic()
        {
            var code = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
        Await Task.FromCanceled(CancellationToken.None)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromCanceledT_CSharp()
        {
            var code = @"
using System.Threading;
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task<int> OperationAsync() {
        return await Task.FromCanceled<int>(CancellationToken.None);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromCanceledT_VisualBasic()
        {
            var code = @"
Imports System.Threading
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task(Of Integer)
        Return Await Task.FromCanceled(Of Integer)(CancellationToken.None)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromException_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await Task.FromException(null);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromException_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
        Await Task.FromException(Nothing)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromExceptionT_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task<int> OperationAsync() {
        return await Task.FromException<int>(null);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompletedTaskFromExceptionT_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task(Of Integer)
        Return Await Task.FromException(Of Integer)(Nothing)
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
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
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
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Await obj.MethodAsync().ConfigureAwait(False)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CorrectUseOfPerInstanceFieldAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    IInterface obj;

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CorrectUseOfPerInstanceFieldAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Dim obj As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
        Await obj.MethodAsync().ConfigureAwait(False)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CorrectUseOfPerInstancePropertyAsynchronousMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    IInterface obj { get; }

    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync() {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task CorrectUseOfPerInstancePropertyAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    ReadOnly Property obj As IInterface

    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task
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
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync(IInterface obj) {
        await [|obj|].MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync([ThreadDependency(ContextDependency.None, Verified = false)] IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Await [|obj|].MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    <ThreadDependency(ContextDependency.None)>
    Function MethodAsync() As <ThreadDependency(ContextDependency.None, PerInstance:=True)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(<ThreadDependency(None, Verified:=False)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
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
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        [|await obj.MethodAsync().ConfigureAwait(false)|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [return: ThreadDependency(ContextDependency.None, PerInstance = true, Verified = false)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    async Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
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
    Async Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Await obj.MethodAsync().ConfigureAwait(false)|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As <ThreadDependency(None, PerInstance:=True, Verified:=False)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Await obj.MethodAsync().ConfigureAwait(false)
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectAwaitPossibleYieldInAlwaysCompletedAsyncMethod_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, AlwaysCompleted = true)]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [return: ThreadDependency(ContextDependency.None, AlwaysCompleted = true, Verified = false)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, AlwaysCompleted = true)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectAwaitPossibleYieldInAlwaysCompletedAsyncMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None, AlwaysCompleted:=True)> Task
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As <ThreadDependency(None, AlwaysCompleted:=True, Verified:=False)> Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Async Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None, AlwaysCompleted:=True)> Task
        Await obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }
    }
}
