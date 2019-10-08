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
    public class CodeMayHaveMainThreadDependencyReturnOperationTests
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
    Task OperationAsync(IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(obj As IInterface) As Task
        Return obj.MethodAsync()
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
    Task OperationAsync(IInterface obj) {
        [|return obj.MethodAsync();|]
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
    Task OperationAsync(IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Return obj.MethodAsync()|]
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Return obj.MethodAsync()
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
    Task OperationAsync(IInterface obj) {
        [|return obj.MethodAsync();|]
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
    Task OperationAsync(IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.Context)> Task
        [|Return obj.MethodAsync()|]
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.Context)> Task
        Return obj.MethodAsync()
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
    Task OperationAsync(IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.Context)> Task
        Return obj.MethodAsync()
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
    Task OperationAsync(IInterface obj) {
        [|return obj.MethodAsync();|]
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
    Task OperationAsync(IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Return obj.MethodAsync()|]
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(Context)> Task
        Return obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task PassThroughDoesNotCaptureContextIfAlreadyCompleted_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None, AlwaysCompleted = true)]
    Task MethodAsync();
}

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    Task OperationAsync(IInterface obj) {
        return obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task PassThroughDoesNotCaptureContextIfAlreadyCompleted_VisualBasic()
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Return obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task PassThroughDoesNotCaptureContextIfAlreadyCompletedTaskFromResult_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.None)]
    [return: ThreadDependency(ContextDependency.None)]
    Task<int> OperationAsync() {
        return Task.FromResult(0);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task PassThroughDoesNotCaptureContextIfAlreadyCompletedTaskFromResult_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Function OperationAsync() As <ThreadDependency(ContextDependency.None)> Task(Of Integer)
        Return Task.FromResult(0)
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
    Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Return obj.MethodAsync()
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
    Task OperationAsync(IInterface obj) {
        return [|obj|].MethodAsync();
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
    Task OperationAsync([ThreadDependency(ContextDependency.None, Verified = false)] IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Return [|obj|].MethodAsync()
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
    Function OperationAsync(<ThreadDependency(None, Verified:=False)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Return obj.MethodAsync()
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
    Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        [|return obj.MethodAsync();|]
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
    Task OperationAsync([ThreadDependency(ContextDependency.None)] IInterface obj) {
        return obj.MethodAsync();
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
    Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Return obj.MethodAsync()|]
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
    Function OperationAsync(<ThreadDependency(ContextDependency.None)> obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Return obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectPassThroughOfUnrestrictedAsynchronousMethod_CSharp()
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
    Task OperationAsync(IInterface obj) {
        [|return obj.MethodAsync();|]
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
    Task OperationAsync(IInterface obj) {
        return obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task IncorrectPassThroughOfUnrestrictedAsynchronousMethod_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Interface IInterface
    Function MethodAsync() As Task
End Interface

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        [|Return obj.MethodAsync()|]
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
    Function OperationAsync(obj As IInterface) As <ThreadDependency(ContextDependency.None)> Task
        Return obj.MethodAsync()
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }
    }
}
