// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.CodeMayHaveMainThreadDependency,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.CodeMayHaveMainThreadDependency,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class CodeMayHaveMainThreadDependencyTests
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

            await VerifyCS.VerifyAnalyzerAsync(code);
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

            await VerifyVB.VerifyAnalyzerAsync(code);
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
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
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
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(code);
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
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
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
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(code);
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

            await VerifyCS.VerifyAnalyzerAsync(code);
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

            await VerifyVB.VerifyAnalyzerAsync(code);
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
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
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
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(code);
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
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
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
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(code);
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
        [|await obj.MethodAsync().ConfigureAwait(true)|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
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
        [|Await obj.MethodAsync().ConfigureAwait(True)|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(code);
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

            await VerifyCS.VerifyAnalyzerAsync(code);
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

            await VerifyVB.VerifyAnalyzerAsync(code);
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

            await VerifyCS.VerifyAnalyzerAsync(code);
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

            await VerifyVB.VerifyAnalyzerAsync(code);
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
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
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
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(code);
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
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
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
        [|Await obj.MethodAsync()|]
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(code);
        }
    }
}
