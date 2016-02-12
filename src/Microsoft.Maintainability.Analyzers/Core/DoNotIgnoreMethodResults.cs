// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.Maintainability.Analyzers
{
    /// <summary>
    /// CA1806: Do not ignore method results
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotIgnoreMethodResultsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1806";

        private static readonly ImmutableHashSet<string> s_stringMethodNames = ImmutableHashSet.CreateRange(
            new string[] {
                "ToUpper",
                "ToLower",
                "Trim",
                "TrimEnd",
                "TrimStart",
                "ToUpperInvariant",
                "ToLowerInvariant",
                "Clone",
                "Format",
                "Concat",
                "Copy",
                "Insert",
                "Join",
                "Normalize",
                "Remove",
                "Replace",
                "Split",
                "PadLeft",
                "PadRight",
                "Substring",
            });

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageObjectCreation = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageObjectCreation), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageStringCreation = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageStringCreation), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageHResultOrErrorCode = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageHResultOrErrorCode), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTryParse = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageTryParse), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             "{0}",     // Use a placeholder message format as we need to display different messages based on the violation.
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182273.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterOperationBlockStartAction(osContext =>
            {
                var method = osContext.OwningSymbol as IMethodSymbol;
                if (method == null)
                {
                    return;
                }

                osContext.RegisterOperationAction(opContext =>
                {
                    IExpression expression = ((IExpressionStatement)opContext.Operation).Expression;
                    string messageFormat = null;
                    string targetMethodName = null;
                    switch (expression.Kind)
                    {
                        case OperationKind.ObjectCreationExpression:
                            IMethodSymbol ctor = ((IObjectCreationExpression)expression).Constructor;
                            if (ctor != null)
                            {
                                messageFormat = MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageObjectCreation;
                                targetMethodName = ctor.ContainingType.Name;
                            }
                            break;

                        case OperationKind.InvocationExpression:
                            IInvocationExpression invocationExpression = ((IInvocationExpression)expression);
                            IMethodSymbol targetMethod = invocationExpression.TargetMethod;
                            if (targetMethod == null)
                            {
                                break;
                            }

                            if (IsStringCreatingMethod(targetMethod))
                            {
                                messageFormat = MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageStringCreation;
                            }
                            else if (IsTryParseMethod(targetMethod))
                            {
                                messageFormat = MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageTryParse;
                            }
                            else if (IsHResultOrErrorCodeReturningMethod(targetMethod))
                            {
                                messageFormat = MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageHResultOrErrorCode;
                            }

                            targetMethodName = targetMethod.Name;
                            break;
                    }

                    if (messageFormat != null)
                    {
                        string message = string.Format(messageFormat, method.Name, targetMethodName);
                        Diagnostic diagnostic = Diagnostic.Create(Rule, expression.Syntax.GetLocation(), message);
                        opContext.ReportDiagnostic(diagnostic);
                    }
                }, OperationKind.ExpressionStatement);
            });
        }

        private static bool IsStringCreatingMethod(IMethodSymbol method)
        {
            return method.ContainingType.SpecialType == SpecialType.System_String &&
                s_stringMethodNames.Contains(method.Name);
        }

        private static bool IsTryParseMethod(IMethodSymbol method)
        {
            return method.Name.StartsWith("TryParse", StringComparison.OrdinalIgnoreCase) &&
                method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                method.Parameters.Length >= 2 &&
                method.Parameters[1].RefKind != RefKind.None;
        }

        private static bool IsHResultOrErrorCodeReturningMethod(IMethodSymbol method)
        {
            // Tune this method to match the FxCop behavior once https://github.com/dotnet/roslyn/issues/7282 is addressed.
            return method.GetDllImportData() != null &&
                (method.ReturnType.SpecialType == SpecialType.System_Int32 ||
                method.ReturnType.SpecialType == SpecialType.System_UInt32);
        }
    }
}