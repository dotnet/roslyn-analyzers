// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1024: Use properties where appropriate
    /// 
    /// Cause:
    /// A public or protected method has a name that starts with Get, takes no parameters, and returns a value that is not an array.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UsePropertiesWhereAppropriateAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1024";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UsePropertiesWhereAppropriateTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UsePropertiesWhereAppropriateMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UsePropertiesWhereAppropriateDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Design,
                                                                         DiagnosticSeverity.Warning,
                                                                         isEnabledByDefault: false,
                                                                         description: s_localizableDescription,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182181.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);
        private const string GetHashCodeName = "GetHashCode";
        private const string GetEnumeratorName = "GetEnumerator";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterOperationBlockStartAction(context =>
            {
                var methodSymbol = context.OwningSymbol as IMethodSymbol;

                if (methodSymbol == null ||
                    methodSymbol.ReturnsVoid ||
                    methodSymbol.ReturnType.Kind == SymbolKind.ArrayType ||
                    methodSymbol.Parameters.Length > 0 ||
                    !(methodSymbol.DeclaredAccessibility == Accessibility.Public || methodSymbol.DeclaredAccessibility == Accessibility.Protected) ||
                    methodSymbol.IsAccessorMethod() ||
                    !IsPropertyLikeName(methodSymbol.Name))
                {
                    return;
                }

                // A few additional checks to reduce the noise for this diagnostic:
                // Ensure that the method is non-generic, non-virtual/override, has no overloads and doesn't have special names: 'GetHashCode' or 'GetEnumerator'.
                // Also avoid generating this diagnostic if the method body has any invocation expressions.
                if (methodSymbol.IsGenericMethod ||
                    methodSymbol.IsVirtual ||
                    methodSymbol.IsOverride ||
                    methodSymbol.ContainingType.GetMembers(methodSymbol.Name).Length > 1 ||
                    methodSymbol.Name == GetHashCodeName ||
                    methodSymbol.Name == GetEnumeratorName)
                {
                    return;
                }

                bool hasInvocations = false;
                context.RegisterOperationAction(operationContext =>
                {
                    hasInvocations = true;
                }, OperationKind.InvocationExpression);

                context.RegisterOperationBlockEndAction(endContext =>
                {
                    if (!hasInvocations)
                    {
                        endContext.ReportDiagnostic(endContext.OwningSymbol.CreateDiagnostic(Rule));
                    }
                });
            });
        }

        private static bool IsPropertyLikeName(string methodName)
        {
            return methodName.Length > 3 &&
                methodName.StartsWith("Get", StringComparison.OrdinalIgnoreCase) &&
                !char.IsLower(methodName[3]);
        }
    }
}
