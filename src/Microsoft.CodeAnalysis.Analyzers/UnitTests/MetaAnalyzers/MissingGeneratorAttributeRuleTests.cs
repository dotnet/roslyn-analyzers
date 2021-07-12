// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading.Tasks;
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

        [Fact]
        public async Task TestSimpleClass_CSharp()
        {
            var code = @"
using Microsoft.CodeAnalysis;

public class [|CustomGenerator|] : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}
    public void Execute(GeneratorExecutionContext context) {}
}";

            var fixedCode = @"
using Microsoft.CodeAnalysis;

[Generator]
public class CustomGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}
    public void Execute(GeneratorExecutionContext context) {}
}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    ReferenceAssemblies = ReferenceAssemblies
                },
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
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    ReferenceAssemblies = ReferenceAssemblies
                },
            }.RunAsync();
        }

        [Fact]
        public async Task TestSimpleClass_VisualBasic()
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

            var fixedCode = @"
Imports Microsoft.CodeAnalysis

<Generator>
Public Class CustomGenerator 
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute

    End Sub
End Class";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    ReferenceAssemblies = ReferenceAssemblies
                },
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
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    ReferenceAssemblies = ReferenceAssemblies
                },
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
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    ReferenceAssemblies = ReferenceAssemblies
                },
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
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    ReferenceAssemblies = ReferenceAssemblies
                },
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
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                }
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
                TestState =
                {
                    Sources = { code },
                    ReferenceAssemblies = ReferenceAssemblies
                }
            }.RunAsync();
        }
    }
}