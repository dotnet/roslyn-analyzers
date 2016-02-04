// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1009: Declare event handlers correctly
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DeclareEventHandlersCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1009";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DeclareEventHandlersCorrectlyTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DeclareEventHandlersCorrectlyMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DeclareEventHandlersCorrectlyDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                 s_localizableTitle,
                                                                 s_localizableMessage,
                                                                 DiagnosticCategory.Design,
                                                                 DiagnosticSeverity.Warning,
                                                                 isEnabledByDefault: false,
                                                                 helpLinkUri: "http://msdn.microsoft.com/library/ms182133.aspx",
                                                                 customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var compilationHandler = new CompilationHandler();
                compilationHandler.Initialize(compilationContext);
            });
        }

        private class CompilationHandler
        {
            private INamedTypeSymbol eventArgsType;

            public void Initialize(CompilationStartAnalysisContext compilationContext)
            {
                this.eventArgsType = compilationContext.Compilation.GetTypeByMetadataName("System.EventArgs");
                if (this.eventArgsType != null)
                {
                    compilationContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Event);
                }
            }

            private void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                var symbol = (IEventSymbol)context.Symbol;
                if (symbol.DeclaredAccessibility == Accessibility.Public || symbol.DeclaredAccessibility == Accessibility.Protected)
                {
                    var eventType = symbol.Type as INamedTypeSymbol;
                    if (eventType != null && eventType.TypeKind == TypeKind.Delegate)
                    {
                        var invokeMethod = eventType.DelegateInvokeMethod;
                        if (invokeMethod != null)
                        {
                            if (!IsDelegateInvokeMethodCorrect(invokeMethod))
                            {
                                context.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
                            }
                        }
                    }
                }
            }

            private bool IsDelegateInvokeMethodCorrect(IMethodSymbol method)
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    return false;
                }

                if (method.Parameters.Length != 2)
                {
                    return false;
                }

                return IsParameter1Correct(method.Parameters[0]) && IsParameter2Correct(method.Parameters[1]);
            }

            private bool IsParameter1Correct(IParameterSymbol parameter)
            {
                if (parameter.RefKind != RefKind.None)
                {
                    return false;
                }

                return parameter.Type.SpecialType == SpecialType.System_Object;
            }

            private bool IsParameter2Correct(IParameterSymbol parameter)
            {
                if (parameter.RefKind != RefKind.None)
                {
                    return false;
                }

                for (var type = parameter.Type as INamedTypeSymbol; type != null; type = type.BaseType)
                {
                    if (type == this.eventArgsType)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
