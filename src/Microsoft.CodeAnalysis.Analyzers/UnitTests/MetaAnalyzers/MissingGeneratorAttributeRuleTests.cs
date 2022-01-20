﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.SourceGeneratorAttributeAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.SourceGeneratorAttributeAnalyzerFix>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.SourceGeneratorAttributeAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.SourceGeneratorAttributeAnalyzerFix>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests.MetaAnalyzers
{
    public class MissingGeneratorAttributeRuleTests
    {
        private static readonly ReferenceAssemblies ReferenceAssemblies =
            ReferenceAssemblies.Default.AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.CodeAnalysis", "3.10.0")));

        [Theory]
        [InlineData("[Generator]", 0, SourceGeneratorAttributeAnalyzerFix.CSharpEquivalenceKey)]
        [InlineData("[Generator(LanguageNames.VisualBasic)]", 1, SourceGeneratorAttributeAnalyzerFix.VisualBasicEquivalenceKey)]
        [InlineData("[Generator(LanguageNames.CSharp, LanguageNames.VisualBasic)]", 2, SourceGeneratorAttributeAnalyzerFix.CSharpVisualBasicEquivalenceKey)]
        public async Task TestSimpleClass_CSharp(string attr, int index, string equivalenceKey)
        {
            var code = @"
using Microsoft.CodeAnalysis;

public class [|CustomGenerator|] : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}
    public void Execute(GeneratorExecutionContext context) {}
}";

            var fixedCode = @$"
using Microsoft.CodeAnalysis;

{attr}
public class CustomGenerator : ISourceGenerator
{{
    public void Initialize(GeneratorInitializationContext context) {{}}
    public void Execute(GeneratorExecutionContext context) {{}}
}}";
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestCode = code,
                FixedCode = fixedCode,
                CodeActionIndex = index,
                CodeActionEquivalenceKey = equivalenceKey
            }.RunAsync();
        }

        [Fact]
        public async Task TestSimpleClass_FullyQualified_CSharp()
        {
            var code = @"
public class [|CustomGenerator|] : Microsoft.CodeAnalysis.ISourceGenerator
{
    public void Initialize(Microsoft.CodeAnalysis.GeneratorInitializationContext context) {}
    public void Execute(Microsoft.CodeAnalysis.GeneratorExecutionContext context) {}
}";

            var fixedCode = @"
[Microsoft.CodeAnalysis.Generator]
public class CustomGenerator : Microsoft.CodeAnalysis.ISourceGenerator
{
    public void Initialize(Microsoft.CodeAnalysis.GeneratorInitializationContext context) {}
    public void Execute(Microsoft.CodeAnalysis.GeneratorExecutionContext context) {}
}";
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestState =
                {
                    Sources = { code },
                },
                FixedState =
                {
                    Sources = { fixedCode },
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("<Generator>", 0, SourceGeneratorAttributeAnalyzerFix.CSharpEquivalenceKey)]
        [InlineData("<Generator(LanguageNames.VisualBasic)>", 1, SourceGeneratorAttributeAnalyzerFix.VisualBasicEquivalenceKey)]
        [InlineData("<Generator(LanguageNames.CSharp, LanguageNames.VisualBasic)>", 2, SourceGeneratorAttributeAnalyzerFix.CSharpVisualBasicEquivalenceKey)]
        public async Task TestSimpleClass_VisualBasic(string attr, int index, string equivalenceKey)
        {
            var code = @"
Imports Microsoft.CodeAnalysis 

Public Class [|CustomGenerator|] 
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute

    End Sub
End Class";

            var fixedCode = @$"
Imports Microsoft.CodeAnalysis

{attr}
Public Class CustomGenerator 
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute

    End Sub
End Class";

            await new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestCode = code,
                FixedCode = fixedCode,
                CodeActionIndex = index,
                CodeActionEquivalenceKey = equivalenceKey
            }.RunAsync();
        }

        [Fact]
        public async Task TestSimpleClass_FullyQualified_VisualBasic()
        {
            var code = @"
Public Class [|CustomGenerator|] 
    Implements Microsoft.CodeAnalysis.ISourceGenerator

    Public Sub Initialize(context As Microsoft.CodeAnalysis.GeneratorInitializationContext) Implements Microsoft.CodeAnalysis.ISourceGenerator.Initialize
    End Sub

    Sub Execute(context As Microsoft.CodeAnalysis.GeneratorExecutionContext) Implements Microsoft.CodeAnalysis.ISourceGenerator.Execute
    End Sub
End Class";

            var fixedCode = @"
<Microsoft.CodeAnalysis.Generator>
Public Class CustomGenerator 
    Implements Microsoft.CodeAnalysis.ISourceGenerator

    Public Sub Initialize(context As Microsoft.CodeAnalysis.GeneratorInitializationContext) Implements Microsoft.CodeAnalysis.ISourceGenerator.Initialize
    End Sub

    Sub Execute(context As Microsoft.CodeAnalysis.GeneratorExecutionContext) Implements Microsoft.CodeAnalysis.ISourceGenerator.Execute
    End Sub
End Class";

            await new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestCode = code,
                FixedCode = fixedCode,
            }.RunAsync();
        }

        [Fact]
        public async Task TestHierarchy_CSharp()
        {
            var code = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

public abstract class CustomGeneratorBase : ISourceGenerator
{
    public abstract void Initialize(GeneratorInitializationContext context);
    public abstract void Execute(GeneratorExecutionContext context);
}

public class [|CustomGenerator|] : CustomGeneratorBase
{
    public override void Initialize(GeneratorInitializationContext context) {}
    public override void Execute(GeneratorExecutionContext context) {}
}";

            var fixedCode = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

public abstract class CustomGeneratorBase : ISourceGenerator
{
    public abstract void Initialize(GeneratorInitializationContext context);
    public abstract void Execute(GeneratorExecutionContext context);
}

[Generator]
public class CustomGenerator : CustomGeneratorBase
{
    public override void Initialize(GeneratorInitializationContext context) {}
    public override void Execute(GeneratorExecutionContext context) {}
}";
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestCode = code,
                FixedCode = fixedCode,
            }.RunAsync();
        }

        [Fact]
        public async Task TestHierarchy_VisualBasic()
        {
            var code = @"
Imports Microsoft.CodeAnalysis 

Public MustInherit Class CustomGeneratorBase
    Implements ISourceGenerator

    Public MustOverride Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize
    Public MustOverride Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute
End Class

Public Class [|CustomGenerator|]
    Inherits CustomGeneratorBase

    Public Overrides Sub Initialize(context As GeneratorInitializationContext)
    End Sub

    Public Overrides Sub Execute(context As GeneratorExecutionContext)
    End Sub
End Class";

            var fixedCode = @"
Imports Microsoft.CodeAnalysis 

Public MustInherit Class CustomGeneratorBase
    Implements ISourceGenerator

    Public MustOverride Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize
    Public MustOverride Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute
End Class

<Generator>
Public Class CustomGenerator
    Inherits CustomGeneratorBase

    Public Overrides Sub Initialize(context As GeneratorInitializationContext)
    End Sub

    Public Overrides Sub Execute(context As GeneratorExecutionContext)
    End Sub
End Class";

            await new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestCode = code,
                FixedCode = fixedCode,
            }.RunAsync();
        }

        [Fact]
        public async Task TestHierarchy_InheritedAttribute_CSharp()
        {
            var code = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[Generator]
public abstract class CustomGeneratorBase : ISourceGenerator
{
    public abstract void Initialize(GeneratorInitializationContext context);
    public abstract void Execute(GeneratorExecutionContext context);
}

public class CustomGenerator : CustomGeneratorBase
{
    public override void Initialize(GeneratorInitializationContext context) {}
    public override void Execute(GeneratorExecutionContext context) {}
}";
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestHierarchy_InheritedAttribute_VisualBasic()
        {
            var code = @"
Imports Microsoft.CodeAnalysis 

<Generator>
Public MustInherit Class CustomGeneratorBase
    Implements ISourceGenerator

    Public MustOverride Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize
    Public MustOverride Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute
End Class

Public Class CustomGenerator
    Inherits CustomGeneratorBase

    Public Overrides Sub Initialize(context As GeneratorInitializationContext)
    End Sub

    Public Overrides Sub Execute(context As GeneratorExecutionContext)
    End Sub
End Class";

            await new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestCode = code,
            }.RunAsync();
        }

        [Theory]
        [InlineData("[Generator]", 0, SourceGeneratorAttributeAnalyzerFix.CSharpEquivalenceKey)]
        [InlineData("[Generator(LanguageNames.VisualBasic)]", 1, SourceGeneratorAttributeAnalyzerFix.VisualBasicEquivalenceKey)]
        [InlineData("[Generator(LanguageNames.CSharp, LanguageNames.VisualBasic)]", 2, SourceGeneratorAttributeAnalyzerFix.CSharpVisualBasicEquivalenceKey)]
        public async Task TestFixAllCSharp(string attr, int index, string equivalenceKey)
        {
            var code1 = @"
using Microsoft.CodeAnalysis;

public class [|CustomGenerator1|] : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}
    public void Execute(GeneratorExecutionContext context) {}
}";

            var code2 = @"
using Microsoft.CodeAnalysis;

public class [|CustomGenerator2|] : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}
    public void Execute(GeneratorExecutionContext context) {}
}";

            var fixedCode1 = @$"
using Microsoft.CodeAnalysis;

{attr}
public class CustomGenerator1 : ISourceGenerator
{{
    public void Initialize(GeneratorInitializationContext context) {{}}
    public void Execute(GeneratorExecutionContext context) {{}}
}}";

            var fixedCode2 = @$"
using Microsoft.CodeAnalysis;

{attr}
public class CustomGenerator2 : ISourceGenerator
{{
    public void Initialize(GeneratorInitializationContext context) {{}}
    public void Execute(GeneratorExecutionContext context) {{}}
}}";

            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestState =
                {
                    Sources = {code1, code2 }
                },
                FixedState =
                {
                    Sources = {fixedCode1, fixedCode2}
                },
                CodeActionEquivalenceKey = equivalenceKey,
                CodeActionIndex = index
            }.RunAsync();
        }

        [Theory]
        [InlineData("<Generator>", 0, SourceGeneratorAttributeAnalyzerFix.CSharpEquivalenceKey)]
        [InlineData("<Generator(LanguageNames.VisualBasic)>", 1, SourceGeneratorAttributeAnalyzerFix.VisualBasicEquivalenceKey)]
        [InlineData("<Generator(LanguageNames.CSharp, LanguageNames.VisualBasic)>", 2, SourceGeneratorAttributeAnalyzerFix.CSharpVisualBasicEquivalenceKey)]
        public async Task TestFixAllVisualBasic(string attr, int index, string equivalenceKey)
        {
            var code1 = @"
Imports Microsoft.CodeAnalysis 

Public Class [|CustomGenerator1|]
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute

    End Sub
End Class";

            var code2 = @"
Imports Microsoft.CodeAnalysis 

Public Class [|CustomGenerator2|]
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute

    End Sub
End Class";

            var fixedCode1 = @$"
Imports Microsoft.CodeAnalysis

{attr}
Public Class CustomGenerator1
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute

    End Sub
End Class";

            var fixedCode2 = @$"
Imports Microsoft.CodeAnalysis

{attr}
Public Class CustomGenerator2
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute

    End Sub
End Class";

            await new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestState =
                {
                    Sources = {code1, code2}
                },
                FixedState =
                {
                    Sources = {fixedCode1, fixedCode2}
                },
                CodeActionIndex = index,
                CodeActionEquivalenceKey = equivalenceKey
            }.RunAsync();
        }
    }
}