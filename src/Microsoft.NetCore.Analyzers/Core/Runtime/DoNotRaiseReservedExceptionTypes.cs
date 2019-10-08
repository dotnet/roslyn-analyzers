// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2201: Do not raise reserved exception types
    /// 
    /// Too generic:
    ///     System.Exception
    ///     System.ApplicationException
    ///     System.SystemException 
    ///     
    /// Reserved:
    ///     System.OutOfMemoryException
    ///     System.IndexOutOfRangeException
    ///     System.ExecutionEngineException
    ///     System.NullReferenceException
    ///     System.StackOverflowException
    ///     System.Runtime.InteropServices.ExternalException
    ///     System.Runtime.InteropServices.COMException
    ///     System.Runtime.InteropServices.SEHException
    ///     System.AccessViolationException
    ///     
    /// </summary>
    public abstract class DoNotRaiseReservedExceptionTypesAnalyzer<TLanguageKindEnum, TObjectCreationExpressionSyntax> : DiagnosticAnalyzer
        where TLanguageKindEnum : struct
        where TObjectCreationExpressionSyntax : SyntaxNode
    {
        internal const string RuleId = "CA2201";

        private static readonly ImmutableArray<string> s_tooGenericExceptions = ImmutableArray.Create("System.Exception",
                                                                                                      "System.ApplicationException",
                                                                                                      "System.SystemException");

        private static readonly ImmutableArray<string> s_reservedExceptions = ImmutableArray.Create("System.OutOfMemoryException",
                                                                                                    "System.IndexOutOfRangeException",
                                                                                                    "System.ExecutionEngineException",
                                                                                                    "System.NullReferenceException",
                                                                                                    "System.StackOverflowException",
                                                                                                    "System.Runtime.InteropServices.ExternalException",
                                                                                                    "System.Runtime.InteropServices.COMException",
                                                                                                    "System.Runtime.InteropServices.SEHException",
                                                                                                    "System.AccessViolationException");

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotRaiseReservedExceptionTypesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTooGeneric = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotRaiseReservedExceptionTypesMessageTooGeneric), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageReserved = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotRaiseReservedExceptionTypesMessageReserved), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotRaiseReservedExceptionTypesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor TooGenericRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTooGeneric,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2201-do-not-raise-reserved-exception-types",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor ReservedRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageReserved,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2201-do-not-raise-reserved-exception-types",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        private static readonly SymbolDisplayFormat s_symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TooGenericRule, ReservedRule);

        public abstract TLanguageKindEnum ObjectCreationExpressionKind { get; }

        public abstract SyntaxNode GetTypeSyntaxNode(TObjectCreationExpressionSyntax node);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    ImmutableHashSet<INamedTypeSymbol> tooGenericExceptionSymbols = CreateSymbolSet(compilationStartContext.Compilation, s_tooGenericExceptions);
                    ImmutableHashSet<INamedTypeSymbol> reservedExceptionSymbols = CreateSymbolSet(compilationStartContext.Compilation, s_reservedExceptions); ;

                    if (tooGenericExceptionSymbols.Count == 0 && reservedExceptionSymbols.Count == 0)
                    {
                        return;
                    }

                    compilationStartContext.RegisterSyntaxNodeAction(
                    syntaxNodeContext =>
                    {
                        Analyze(syntaxNodeContext, tooGenericExceptionSymbols, reservedExceptionSymbols);
                    },
                    ObjectCreationExpressionKind);
                });
        }

        private static ImmutableHashSet<INamedTypeSymbol> CreateSymbolSet(Compilation compilation, IEnumerable<string> exceptionNames)
        {
            HashSet<INamedTypeSymbol> set = null;
            foreach (string exp in exceptionNames)
            {
                INamedTypeSymbol symbol = compilation.GetTypeByMetadataName(exp);
                if (symbol == null)
                {
                    continue;
                }
                if (set == null)
                {
                    set = new HashSet<INamedTypeSymbol>();
                }
                set.Add(symbol);
            }

            return set != null ? set.ToImmutableHashSet() : ImmutableHashSet<INamedTypeSymbol>.Empty;
        }

        private void Analyze(
            SyntaxNodeAnalysisContext context,
            ImmutableHashSet<INamedTypeSymbol> tooGenericExceptionSymbols,
            ImmutableHashSet<INamedTypeSymbol> reservedExceptionSymbols)
        {
            var objectCreationNode = (TObjectCreationExpressionSyntax)context.Node;
            SyntaxNode targetType = GetTypeSyntaxNode(objectCreationNode);

            // GetSymbolInfo().Symbol might return an error type symbol 
            if (!(context.SemanticModel.GetSymbolInfo(targetType).Symbol is INamedTypeSymbol typeSymbol))
            {
                return;
            }

            if (tooGenericExceptionSymbols.Contains(typeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(TooGenericRule, targetType.GetLocation(), typeSymbol.ToDisplayString(s_symbolDisplayFormat)));
            }
            else if (reservedExceptionSymbols.Contains(typeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(ReservedRule, targetType.GetLocation(), typeSymbol.ToDisplayString(s_symbolDisplayFormat)));
            }
        }
    }
}