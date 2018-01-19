// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// CA1806: Do not ignore method results
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotIgnoreMethodResultsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1806";

        private static readonly ImmutableHashSet<string> s_stringMethodNames = ImmutableHashSet.CreateRange(
            new[] {
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
        private static readonly LocalizableString s_localizableMessagePureMethod = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessagePureMethod), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTryParse = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageTryParse), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor ObjectCreationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageObjectCreation,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182273.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor StringCreationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageStringCreation,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182273.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor HResultOrErrorCodeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageHResultOrErrorCode,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182273.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor PureMethodRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessagePureMethod,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182273.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);


        internal static DiagnosticDescriptor TryParseRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTryParse,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182273.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ObjectCreationRule, StringCreationRule, HResultOrErrorCodeRule, TryParseRule, PureMethodRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol expectedExceptionType = WellKnownTypes.ExpectedException(compilationContext.Compilation);

                compilationContext.RegisterOperationBlockStartAction(osContext =>
                {
                    var method = osContext.OwningSymbol as IMethodSymbol;
                    if (method == null)
                    {
                        return;
                    }

                    osContext.RegisterOperationAction(opContext =>
                    {
                        if (ShouldSkipAnalyzing(opContext, expectedExceptionType))
                        {
                            return;
                        }

                        IOperation expression = ((IExpressionStatementOperation)opContext.Operation).Operation;
                        DiagnosticDescriptor rule = null;
                        string targetMethodName = null;
                        switch (expression.Kind)
                        {
                            case OperationKind.ObjectCreation:
                                IMethodSymbol ctor = ((IObjectCreationOperation)expression).Constructor;
                                if (ctor != null)
                                {
                                    rule = ObjectCreationRule;
                                    targetMethodName = ctor.ContainingType.Name;
                                }
                                break;

                            case OperationKind.Invocation:
                                IInvocationOperation invocationExpression = ((IInvocationOperation)expression);
                                IMethodSymbol targetMethod = invocationExpression.TargetMethod;
                                if (targetMethod == null)
                                {
                                    break;
                                }

                                if (IsStringCreatingMethod(targetMethod))
                                {
                                    rule = StringCreationRule;
                                }
                                else if (IsTryParseMethod(targetMethod))
                                {
                                    rule = TryParseRule;
                                }
                                else if (IsHResultOrErrorCodeReturningMethod(targetMethod))
                                {
                                    rule = HResultOrErrorCodeRule;
                                }
                                else if (IsPureMethod(targetMethod, opContext.Compilation))
                                {
                                    rule = PureMethodRule;
                                }

                                targetMethodName = targetMethod.Name;
                                break;
                        }

                        if (rule != null)
                        {
                            Diagnostic diagnostic = Diagnostic.Create(rule, expression.Syntax.GetLocation(), method.Name, targetMethodName);
                            opContext.ReportDiagnostic(diagnostic);
                        }
                    }, OperationKind.ExpressionStatement);
                });
            });
        }

        private static bool ShouldSkipAnalyzing(OperationAnalysisContext operationContext, INamedTypeSymbol expectedExceptionType)
        {
            bool IsThrowsArgument(IParameterSymbol parameterSymbol, string argumentName, ImmutableHashSet<string> methodNames, string containingSymbol)
            {
                return parameterSymbol.Name == argumentName &&
                       parameterSymbol.ContainingSymbol is IMethodSymbol methodSymbol &&
                       methodNames.Contains(methodSymbol.Name) &&
                       methodSymbol.ContainingSymbol.ToDisplayString() == containingSymbol;
            }

            bool IsNUnitThrowsArgument(IParameterSymbol parameterSymbol)
            {
                var methodNames = ImmutableHashSet.Create(new[]
                {
                    "Throws",
                    "Catch",
                    "DoesNotThrow",
                    "ThrowsAsync",
                    "CatchAsync",
                    "DoesNotThrowAsync"
                });

                return IsThrowsArgument(parameterSymbol, "code", methodNames, "NUnit.Framework.Assert");
            }

            bool IsXunitThrowsArgument(IParameterSymbol parameterSymbol)
            {
                var methodNames = ImmutableHashSet.Create(new[]
                {
                    "Throws",
                    "ThrowsAsync",
                    "ThrowsAny",
                    "ThrowsAnyAsync",
                });

                return IsThrowsArgument(parameterSymbol, "testCode", methodNames, "Xunit.Assert");
            }

            // We skip analysis for the last statement in a lambda passed to Assert.Throws/ThrowsAsync (xUnit and NUnit), or the last
            // statement in a method annotated with [ExpectedException] (MSTest)

            // Note: We do not attempt to account for a synchronously-running ThrowsAsync with something like return Task.CompletedTask;
            // as the last line.

            // We only skip analysis if we're in a method
            if (operationContext.ContainingSymbol.Kind != SymbolKind.Method)
            {
                return false;
            }

            // Get the enclosing block. If that block's parent isn't null (MSTest case) or an IAnonymousFunctionOperation (xUnit/NUnit), then
            // we bail immediately
            if (!(operationContext.Operation.Parent is IBlockOperation enclosingBlock))
            {
                return false;
            }

            if (enclosingBlock.Parent != null && enclosingBlock.Parent.Kind != OperationKind.AnonymousFunction)
            {
                return false;
            }

            // Only analyze the last non-implicit statement in the function
            bool foundBlock = false;
            foreach (var statement in enclosingBlock.Operations)
            {
                if (statement == operationContext.Operation)
                {
                    foundBlock = true;
                    continue;
                }
                else if (foundBlock)
                {
                    if (!statement.IsImplicit)
                    {
                        return false;
                    }
                }
                else
                {
                    continue;
                }
            }

            // If the parent is Null, we're in the MSTest case. Otherwise, we're in the xUnit/NUnit case.
            if (enclosingBlock.Parent == null)
            {
                if (expectedExceptionType == null)
                {
                    return false;
                }

                IMethodSymbol methodSymbol = (IMethodSymbol)operationContext.ContainingSymbol;

                return methodSymbol.GetAttributes().Any(attr => attr.AttributeClass == expectedExceptionType);
            }
            else
            {
                // Look for an enclosing IArgumentOperation
                IOperation parentArgument = enclosingBlock;
                do
                {
                    parentArgument = parentArgument.Parent;
                } while (parentArgument != null && parentArgument.Kind != OperationKind.Argument);

                if (parentArgument == null)
                {
                    return false;
                }

                IArgumentOperation argumentOperation = (IArgumentOperation)parentArgument;
                return IsNUnitThrowsArgument(argumentOperation.Parameter) || IsXunitThrowsArgument(argumentOperation.Parameter);
            }
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

        private static bool IsPureMethod(IMethodSymbol method, Compilation compilation)
        {
            return method.GetAttributes().Any(attr => attr.AttributeClass.Equals(WellKnownTypes.PureAttribute(compilation)));
        }
    }
}