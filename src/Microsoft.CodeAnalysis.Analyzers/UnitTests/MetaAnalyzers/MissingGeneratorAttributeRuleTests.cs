// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
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
        private const string SourceGeneratorStub_CSharp = @"
using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratorAttribute : Attribute
    {
    }

    public interface ISourceGenerator
    {
        void Initialize(GeneratorInitializationContext context);
        void Execute(GeneratorExecutionContext context);
    }

    public struct GeneratorInitializationContext
    {
    }

    public readonly struct GeneratorExecutionContext
    {
    }
}";

        private const string SourceGeneratorStub_VisualBasic = @"
Imports System
Imports Microsoft.CodeAnalysis

Namespace Microsoft.CodeAnalysis
    <AttributeUsage(AttributeTargets.Class)>
    public Class GeneratorAttribute
            Inherits Attribute
    End Class

    public interface ISourceGenerator
        Sub Initialize(context As GeneratorInitializationContext)
        Sub Execute(context As GeneratorExecutionContext)
    End Interface

    public Class GeneratorInitializationContext
    End Class

    public Class GeneratorExecutionContext
    End Class
End Namespace";

        [Fact]
        public async Task TestMissing_CSharp()
        {
            var code = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[Generator]
public class CustomGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}

    public void Execute(GeneratorExecutionContext context)
    {
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code , SourceGeneratorStub_CSharp },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestMissing_VisualBasic()
        {
            var code = @"
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
                    Sources = { code , SourceGeneratorStub_VisualBasic },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestSimpleClass_CSharp()
        {
            var code = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

public class [|CustomGenerator|] : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}

    public void Execute(GeneratorExecutionContext context)
    {
    }
}";

            var fixedCode = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[Generator]
public class CustomGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}

    public void Execute(GeneratorExecutionContext context)
    {
    }
}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code , SourceGeneratorStub_CSharp },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                },
                FixedState =
                {
                    Sources = { fixedCode, SourceGeneratorStub_CSharp },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference},
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
                    Sources = { code , SourceGeneratorStub_VisualBasic },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                },
                FixedState =
                {
                    Sources = { fixedCode, SourceGeneratorStub_VisualBasic },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference},
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

    public override void Execute(GeneratorExecutionContext context)
    {
    }
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

    public override void Execute(GeneratorExecutionContext context)
    {
    }
}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code , SourceGeneratorStub_CSharp },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                },
                FixedState =
                {
                    Sources = { fixedCode, SourceGeneratorStub_CSharp },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference},
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
                    Sources = { code , SourceGeneratorStub_VisualBasic },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                },
                FixedState =
                {
                    Sources = { fixedCode, SourceGeneratorStub_VisualBasic },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference},
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

    public override void Execute(GeneratorExecutionContext context)
    {
    }
}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code , SourceGeneratorStub_CSharp },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
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
                    Sources = { code , SourceGeneratorStub_VisualBasic },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                }
            }.RunAsync();
        }
    }
}
