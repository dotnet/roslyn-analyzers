// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PerformanceSensitive.Analyzers
{
    internal abstract class AbstractAllocationAnalyzer<TLanguageKindEnum>
        : DiagnosticAnalyzer
        where TLanguageKindEnum : struct
    {
        protected abstract ImmutableArray<TLanguageKindEnum> Expressions { get; }

        protected abstract void AnalyzeNode(SyntaxNodeAnalysisContext context, in PerformanceSensitiveInfo info);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var attributeSymbol = compilation.GetTypeByMetadataName(AllocationRules.PerformanceSensitiveAttributeName);

                // Bail if PerformanceSensitiveAttribute is not delcared in the compilation.
                if (attributeSymbol == null)
                {
                    return;
                }

                compilationStartContext.RegisterCodeBlockStartAction<TLanguageKindEnum>(blockStartContext =>
                {
                    var checker = new AttributeChecker(attributeSymbol);
                    RegisterSyntaxAnalysis(blockStartContext, checker);
                });
            });
        }

        private void RegisterSyntaxAnalysis(CodeBlockStartAnalysisContext<TLanguageKindEnum> codeBlockStartAnalysisContext, AttributeChecker performanceSensitiveAttributeChecker)
        {
            if (AllocationRules.IsIgnoredFile(codeBlockStartAnalysisContext.CodeBlock.SyntaxTree.FilePath))
            {
                return;
            }

            var owningSymbol = codeBlockStartAnalysisContext.OwningSymbol;

            if (owningSymbol.GetAttributes().Any(AllocationRules.IsIgnoredAttribute))
            {
                return;
            }

            if (!performanceSensitiveAttributeChecker.TryGetContainsPerformanceSensitiveInfo(owningSymbol, out var info))
            {
                return;
            }

            codeBlockStartAnalysisContext.RegisterSyntaxNodeAction(
                syntaxNodeContext =>
                {
                    AnalyzeNode(syntaxNodeContext, in info);
                },
                Expressions);
        }

        protected sealed class AttributeChecker
        {
            private INamedTypeSymbol PerfSensitiveAttributeSymbol { get; }

            public AttributeChecker(INamedTypeSymbol perfSensitiveAttributeSymbol)
            {
                PerfSensitiveAttributeSymbol = perfSensitiveAttributeSymbol;
            }

            public bool TryGetContainsPerformanceSensitiveInfo(ISymbol symbol, out PerformanceSensitiveInfo info)
            {
                var attributes = symbol.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeClass.Equals(PerfSensitiveAttributeSymbol))
                    {
                        info = CreatePerformanceSensitiveInfo(attribute);
                        return true;
                    }
                }

                info = default;
                return false;
            }

            private static PerformanceSensitiveInfo CreatePerformanceSensitiveInfo(AttributeData data)
            {
                var allowCaptures = true;
                var allowGenericEnumeration = true;
                var allowLocks = true;

                foreach (var namedArgument in data.NamedArguments)
                {
                    switch (namedArgument.Key)
                    {
                        case "AllowCaptures":
                            allowCaptures = (bool)namedArgument.Value.Value;
                            break;
                        case "AllowGenericEnumeration":
                            allowGenericEnumeration = (bool)namedArgument.Value.Value;
                            break;
                        case "AllowLocks":
                            allowLocks = (bool)namedArgument.Value.Value;
                            break;
                    }
                }

                return new PerformanceSensitiveInfo(allowCaptures, allowGenericEnumeration, allowLocks);
            }
        }

#pragma warning disable CA1815 // Override equals and operator equals on value types. This type is never used for comparison
        protected readonly struct PerformanceSensitiveInfo
#pragma warning restore CA1815
        {
            public bool AllowCaptures { get; }
            public bool AllowGenericEnumeration { get; }
            public bool AllowLocks { get; }

            public PerformanceSensitiveInfo(
                bool allowCaptures = true,
                bool allowGenericEnumeration = true,
                bool allowLocks = true)
            {
                AllowCaptures = allowCaptures;
                AllowGenericEnumeration = allowGenericEnumeration;
                AllowLocks = allowLocks;
            }
        }
    }
}
