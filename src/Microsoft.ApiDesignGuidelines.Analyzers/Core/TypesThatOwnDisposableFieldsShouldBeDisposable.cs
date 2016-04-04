// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1001: Types that own disposable fields should be disposable
    /// </summary>
    public abstract class TypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer<TTypeDeclarationSyntax> : DiagnosticAnalyzer
            where TTypeDeclarationSyntax : SyntaxNode
    {
        internal const string RuleId = "CA1001";
        internal const string Dispose = "Dispose";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.TypesThatOwnDisposableFieldsShouldBeDisposableTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources)),
                                                                         new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.TypesThatOwnDisposableFieldsShouldBeDisposableMessageNonBreaking), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources)),
                                                                         DiagnosticCategory.Design,
                                                                         DiagnosticSeverity.Warning,
                                                                         isEnabledByDefault: true,
                                                                         description: new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.TypesThatOwnDisposableFieldsShouldBeDisposableDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources)),
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182172.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol disposableType = WellKnownTypes.IDisposable(compilationContext.Compilation);

                if (disposableType == null)
                {
                    return;
                }

                DisposableFieldAnalyzer analyzer = GetAnalyzer(disposableType);
                compilationContext.RegisterSymbolAction(context =>
                {
                    analyzer.AnalyzeSymbol(context);
                },
                SymbolKind.NamedType);
            });
        }

        protected abstract DisposableFieldAnalyzer GetAnalyzer(INamedTypeSymbol disposableType);

        protected abstract class DisposableFieldAnalyzer
        {
            private readonly INamedTypeSymbol _disposableTypeSymbol;

            public DisposableFieldAnalyzer(INamedTypeSymbol disposableTypeSymbol)
            {
                _disposableTypeSymbol = disposableTypeSymbol;
            }

            public void AnalyzeSymbol(SymbolAnalysisContext symbolContext)
            {
                INamedTypeSymbol namedType = (INamedTypeSymbol)symbolContext.Symbol;
                if (!namedType.AllInterfaces.Contains(_disposableTypeSymbol))
                {
                    IEnumerable<IFieldSymbol> disposableFields = from member in namedType.GetMembers()
                                                                 where member.Kind == SymbolKind.Field && !member.IsStatic
                                                                 let field = member as IFieldSymbol
                                                                 where field.Type != null && field.Type.AllInterfaces.Contains(_disposableTypeSymbol)
                                                                 select field;

                    if (disposableFields.Any())
                    {
                        var disposableFieldsHashSet = new HashSet<ISymbol>(disposableFields);
                        IEnumerable<TTypeDeclarationSyntax> classDecls = GetClassDeclarationNodes(namedType, symbolContext.CancellationToken);
                        foreach (TTypeDeclarationSyntax classDecl in classDecls)
                        {
                            SemanticModel model = symbolContext.Compilation.GetSemanticModel(classDecl.SyntaxTree);
                            IEnumerable<SyntaxNode> syntaxNodes = classDecl.DescendantNodes(n => !(n is TTypeDeclarationSyntax) || ReferenceEquals(n, classDecl))
                                .Where(n => IsDisposableFieldCreation(n,
                                                                    model,
                                                                    disposableFieldsHashSet,
                                                                    symbolContext.CancellationToken));
                            if (syntaxNodes.Any())
                            {
                                symbolContext.ReportDiagnostic(namedType.CreateDiagnostic(Rule, namedType.Name));
                                return;
                            }
                        }
                    }
                }
            }

            private IEnumerable<TTypeDeclarationSyntax> GetClassDeclarationNodes(INamedTypeSymbol namedType, CancellationToken cancellationToken)
            {
                foreach (SyntaxNode syntax in namedType.DeclaringSyntaxReferences.Select(s => s.GetSyntax(cancellationToken)))
                {
                    if (syntax != null)
                    {
                        TTypeDeclarationSyntax classDecl = syntax.FirstAncestorOrSelf<TTypeDeclarationSyntax>(ascendOutOfTrivia: false);
                        if (classDecl != null)
                        {
                            yield return classDecl;
                        }
                    }
                }
            }

            protected abstract bool IsDisposableFieldCreation(SyntaxNode node, SemanticModel model, HashSet<ISymbol> disposableFields, CancellationToken cancellationToken);
        }
    }
}
