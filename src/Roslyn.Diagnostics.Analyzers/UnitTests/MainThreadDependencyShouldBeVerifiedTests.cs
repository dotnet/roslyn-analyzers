// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.MainThreadDependencyShouldBeVerified,
    Roslyn.Diagnostics.CSharp.Analyzers.CSharpMainThreadDependencyShouldBeVerifiedCodeFix>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.MainThreadDependencyShouldBeVerified,
    Roslyn.Diagnostics.VisualBasic.Analyzers.VisualBasicMainThreadDependencyShouldBeVerifiedCodeFix>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class MainThreadDependencyShouldBeVerifiedTests
    {
        [Fact]
        public async Task SimpleCase_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [[|ThreadDependency(ContextDependency.None, Verified = false)|]]
    Task OperationAsync() => Task.CompletedTask;
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.None)]
    Task OperationAsync() => Task.CompletedTask;
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task SimpleCase_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <[|ThreadDependency(ContextDependency.None, Verified:=False)|]>
    Function OperationAsync() As Task
        Return Task.CompletedTask
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.None)>
    Function OperationAsync() As Task
        Return Task.CompletedTask
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task CapturesContext_CSharp()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [[|ThreadDependency(ContextDependency.Context, Verified = false)|]]
    Task OperationAsync() => Task.CompletedTask;
}
" + NoMainThreadDependencyAttribute.CSharp;
            var fixedCode = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

class Class {
    [ThreadDependency(ContextDependency.Context)]
    Task OperationAsync() => Task.CompletedTask;
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task CapturesContext_VisualBasic()
        {
            var code = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <[|ThreadDependency(ContextDependency.Context, Verified:=False)|]>
    Function OperationAsync() As Task
        Return Task.CompletedTask
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;
            var fixedCode = @"
Imports System.Threading.Tasks
Imports Roslyn.Utilities

Class [Class]
    <ThreadDependency(ContextDependency.Context)>
    Function OperationAsync() As Task
        Return Task.CompletedTask
    End Function
End Class
" + NoMainThreadDependencyAttribute.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }
    }
}
