// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task CallerAllowsMainThreadUse()
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
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethod()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodWhenCallerCapturesContext()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency(CapturesContext = true)]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task CorrectUseOfContextCapturingAsynchronousMethod()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [NoMainThreadDependency(CapturesContext = true)]
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency(CapturesContext = true)]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task IncorrectUseOfContextCapturingAsynchronousMethod()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [NoMainThreadDependency(CapturesContext = true)]
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task MissingConfigureAwaitCapturesContextInAsynchronousMethod()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [NoMainThreadDependency]
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task ConfigureAwaitTrueCapturesContextInAsynchronousMethod()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [NoMainThreadDependency]
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync().ConfigureAwait(true)|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task MissingConfigureAwaitDoesNotCaptureContextIfAlreadyCompleted()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [NoMainThreadDependency(AlwaysCompleted = true)]
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync(IInterface obj) {
        await obj.MethodAsync();
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task CorrectUseOfPerInstanceAsynchronousMethod()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [NoMainThreadDependency(PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync([NoMainThreadDependency] IInterface obj) {
        await obj.MethodAsync().ConfigureAwait(false);
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task IncorrectUseOfPerInstanceAsynchronousMethod()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    [NoMainThreadDependency(PerInstance = true)]
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync(IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task IncorrectUseOfUnrestrictedAsynchronousMethodThroughRestrictedInstance()
        {
            var code = @"
using System.Threading.Tasks;
using Roslyn.Utilities;

interface IInterface {
    Task MethodAsync();
}

class Class {
    [NoMainThreadDependency]
    async Task OperationAsync([NoMainThreadDependency] IInterface obj) {
        [|await obj.MethodAsync()|];
    }
}
" + NoMainThreadDependencyAttribute.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
