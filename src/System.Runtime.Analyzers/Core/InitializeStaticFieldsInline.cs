// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1810: Initialize reference type static fields inline
    /// CA2207: Initialize value type static fields inline
    /// </summary>
    public abstract class InitializeStaticFieldsInlineAnalyzer<TLanguageKindEnum> : DiagnosticAnalyzer
        where TLanguageKindEnum : struct
    {
        internal const string CA1810RuleId = "CA1810";
        internal const string CA2207RuleId = "CA2207";

        private static readonly LocalizableString s_CA1810_LocalizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InitializeReferenceTypeStaticFieldsInlineTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_CA2207_LocalizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InitializeValueTypeStaticFieldsInlineTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InitializeStaticFieldsInlineMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_CA1810_LocalizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InitializeReferenceTypeStaticFieldsInlineDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_CA2207_LocalizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InitializeValueTypeStaticFieldsInlineDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor CA1810Rule = new DiagnosticDescriptor(CA1810RuleId,
                                                                             s_CA1810_LocalizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_CA1810_LocalizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182275.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor CA2207Rule = new DiagnosticDescriptor(CA2207RuleId,
                                                                             s_CA2207_LocalizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_CA2207_LocalizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182346.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CA1810Rule, CA2207Rule);

        protected abstract bool InitialiesStaticField(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken);
        protected abstract TLanguageKindEnum AssignmentNodeKind { get; }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCodeBlockStartAction<TLanguageKindEnum>(codeBlockStartContext =>
            {
                var methodSym = codeBlockStartContext.OwningSymbol as IMethodSymbol;
                if (methodSym == null ||
                    !methodSym.IsStatic ||
                    methodSym.MethodKind != MethodKind.SharedConstructor)
                {
                    return;
                }

                var initializesStaticField = false;
                codeBlockStartContext.RegisterSyntaxNodeAction(syntaxContext =>
                {
                    if (!initializesStaticField)
                    {
                        initializesStaticField = InitialiesStaticField(syntaxContext.Node, syntaxContext.SemanticModel, syntaxContext.CancellationToken);
                    }
                }, AssignmentNodeKind);

                codeBlockStartContext.RegisterCodeBlockEndAction(codeBlockEndContext =>
                {
                    if (initializesStaticField)
                    {
                        DiagnosticDescriptor descriptor = methodSym.ContainingType.IsReferenceType ? CA1810Rule : CA2207Rule;
                        Diagnostic diagnostic = Diagnostic.Create(descriptor, methodSym.Locations[0], methodSym.ContainingType.Name);
                        codeBlockEndContext.ReportDiagnostic(diagnostic);
                    }
                });
            });
        }
    }
}