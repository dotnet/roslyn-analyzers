// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.EnableConcurrentExecutionAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.EnableConcurrentExecutionFix>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.EnableConcurrentExecutionAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.EnableConcurrentExecutionFix>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests.MetaAnalyzers
{
    public class EnableConcurrentExecutionAnalyzerTests
    {
        [Fact]
        public async Task TestSimpleCase_CSharp()
        {
            var code = @"
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Analyzer : DiagnosticAnalyzer {
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw null;
    public override void Initialize(AnalysisContext [|context|])
    {
        context.RegisterCompilationAction(ctx => { });
    }
}
";
            var fixedCode = @"
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Analyzer : DiagnosticAnalyzer {
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw null;
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(ctx => { });
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task TestSimpleCase_VisualBasic()
        {
            var code = @"
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Analyzer
    Inherits DiagnosticAnalyzer

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Throw New System.Exception
        End Get
    End Property

    Public Overrides Sub Initialize([|context|] As AnalysisContext)
        context.RegisterCompilationAction(
            Function(ctx)
            End Function)
    End Sub
End Class
";
            var fixedCode = @"
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Analyzer
    Inherits DiagnosticAnalyzer

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Throw New System.Exception
        End Get
    End Property

    Public Overrides Sub Initialize([|context|] As AnalysisContext)
        context.EnableConcurrentExecution()
        context.RegisterCompilationAction(
            Function(ctx)
            End Function)
    End Sub
End Class
";

            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task RenamedMethod_CSharp()
        {
            var code = @"
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Analyzer : DiagnosticAnalyzer {
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw null;
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
    }

    public void NotInitialize(AnalysisContext context)
    {
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task RenamedMethod_VisualBasic()
        {
            var code = @"
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Analyzer
    Inherits DiagnosticAnalyzer

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Throw New System.Exception
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.EnableConcurrentExecution()
    End Sub

    Public Sub NotInitialize(context As AnalysisContext)
    End Sub
End Class
";

            await VerifyVB.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(4102, "https://github.com/dotnet/roslyn-analyzers/issues/4102")]
        public async Task TestEmptyMethod_CSharp()
        {
            var code = @"
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
class Analyzer : DiagnosticAnalyzer {
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw null;
    public override void Initialize(AnalysisContext context)
    {
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(4102, "https://github.com/dotnet/roslyn-analyzers/issues/4102")]
        public async Task TestEmptyMethod_VisualBasic()
        {
            var code = @"
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Class Analyzer
    Inherits DiagnosticAnalyzer
    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Throw New System.Exception
        End Get
    End Property
    Public Overrides Sub Initialize(context As AnalysisContext)
    End Sub
End Class
";

            await VerifyVB.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(4102, "https://github.com/dotnet/roslyn-analyzers/issues/4102")]
        public async Task TestEmptyMethodWithComment_CSharp()
        {
            var code = @"
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
class Analyzer : DiagnosticAnalyzer {
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw null;
    public override void Initialize(AnalysisContext context)
    {
        // Method is empty on purpose
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(4102, "https://github.com/dotnet/roslyn-analyzers/issues/4102")]
        public async Task TestEmptyMethodWithComment_VisualBasic()
        {
            var code = @"
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Class Analyzer
    Inherits DiagnosticAnalyzer
    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Throw New System.Exception
        End Get
    End Property
    Public Overrides Sub Initialize(context As AnalysisContext)
        ' Method is empty on purpose
    End Sub
End Class
";

            await VerifyVB.VerifyAnalyzerAsync(code);
        }
    }
}
