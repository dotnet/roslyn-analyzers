// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Exp.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ReviewSqlQueriesForSecurityVulnerabilities : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2100";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewSQLQueriesForSecurityVulnerabilitiesTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageNoNonLiterals = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewSQLQueriesForSecurityVulnerabilitiesMessageNoNonLiterals), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewSQLQueriesForSecurityVulnerabilitiesDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNoNonLiterals,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol iDbCommandType = WellKnownTypes.IDbCommand(compilationContext.Compilation);
                INamedTypeSymbol iDataAdapterType = WellKnownTypes.IDataAdapter(compilationContext.Compilation);
                IPropertySymbol commandTextProperty = iDbCommandType.GetProperty("CommandText");

                if (iDbCommandType == null ||
                    iDataAdapterType == null ||
                    commandTextProperty == null)
                {
                    return;
                }

                compilationContext.RegisterOperationBlockStartAction(operationBlockStartContext =>
                {
                    ISymbol symbol = operationBlockStartContext.OwningSymbol;

                    var isInDbCommandConstructor = false;
                    var isInDataAdapterConstructor = false;

                    if (symbol.Kind != SymbolKind.Method)
                    {
                        return;
                    }

                    var methodSymbol = (IMethodSymbol)symbol;

                    if (methodSymbol.MethodKind == MethodKind.Constructor)
                    {
                        CheckForDbCommandAndDataAdapterImplementation(symbol.ContainingType, iDbCommandType, iDataAdapterType, out isInDbCommandConstructor, out isInDataAdapterConstructor);
                    }

                    operationBlockStartContext.RegisterOperationAction(operationContext =>
                    {
                        var creation = (IObjectCreationOperation)operationContext.Operation;
                        AnalyzeMethodCall(operationContext, creation.Constructor, symbol, creation.Arguments, creation.Syntax, isInDbCommandConstructor, isInDataAdapterConstructor, iDbCommandType, iDataAdapterType);
                    }, OperationKind.ObjectCreation);

                    // If an object calls a constructor in a base class or the same class, this will get called.
                    operationBlockStartContext.RegisterOperationAction(operationContext =>
                    {
                        var invocation = (IInvocationOperation)operationContext.Operation;

                        // We only analyze constructor invocations
                        if (invocation.TargetMethod.MethodKind != MethodKind.Constructor)
                        {
                            return;
                        }

                        // If we're calling another constructor in the same class from this constructor, assume that all parameters are safe and skip analysis. Parameter usage
                        // will be analyzed there
                        if (invocation.TargetMethod.ContainingType == symbol.ContainingType)
                        {
                            return;
                        }

                        AnalyzeMethodCall(operationContext, invocation.TargetMethod, symbol, invocation.Arguments, invocation.Syntax, isInDbCommandConstructor, isInDataAdapterConstructor, iDbCommandType, iDataAdapterType);
                    }, OperationKind.Invocation);

                    operationBlockStartContext.RegisterOperationAction(operationContext =>
                    {
                        var propertyReference = (IPropertyReferenceOperation)operationContext.Operation;

                        // We're only interested in implementations of IDbCommand.CommandText
                        if (!propertyReference.Property.IsImplementationOfInterfaceMember(commandTextProperty))
                        {
                            return;
                        }

                        // Make sure we're in assignment statement
                        if (!(propertyReference.Parent is IAssignmentOperation assignment))
                        {
                            return;
                        }

                        // Only if the property reference is actually the target of the assignment
                        if (assignment.Target != propertyReference)
                        {
                            return;
                        }

                        ReportDiagnosticIfNecessary(operationContext, assignment.Value, assignment.Syntax, propertyReference.Property, symbol);
                    }, OperationKind.PropertyReference);
                });
            });
        }

        private static void AnalyzeMethodCall(OperationAnalysisContext operationContext,
                                       IMethodSymbol constructorSymbol,
                                       ISymbol containingSymbol,
                                       ImmutableArray<IArgumentOperation> arguments,
                                       SyntaxNode invocationSyntax,
                                       bool isInDbCommandConstructor,
                                       bool isInDataAdapterConstructor,
                                       INamedTypeSymbol iDbCommandType,
                                       INamedTypeSymbol iDataAdapterType)
        {
            CheckForDbCommandAndDataAdapterImplementation(constructorSymbol.ContainingType, iDbCommandType, iDataAdapterType,
                                                          out var callingDbCommandConstructor,
                                                          out var callingDataAdapterConstructor);

            if (!callingDataAdapterConstructor && !callingDbCommandConstructor)
            {
                return;
            }

            // All parameters the function takes that are explicit strings are potential vulnerabilities
            var potentials = arguments.WhereAsArray(arg => arg.Parameter.Type.SpecialType == SpecialType.System_String && !arg.Parameter.IsImplicitlyDeclared);
            if (potentials.IsEmpty)
            {
                return;
            }

            var vulnerableArgumentsBuilder = ImmutableArray.CreateBuilder<IArgumentOperation>();

            foreach (var argument in potentials)
            {
                // For the constructor of a IDbCommand-derived class, if there is only one string parameter, then we just
                // assume that it's the command text. If it takes more than one string, then we need to figure out which
                // one is the command string. However, for the constructor of a IDataAdapter, a lot of times the
                // constructor also take in the connection string, so we can't assume it's the command if there is only one
                // string.
                if (callingDataAdapterConstructor || potentials.Length > 1)
                {
                    if (!IsParameterSymbolVulnerable(argument.Parameter))
                    {
                        continue;
                    }
                }

                vulnerableArgumentsBuilder.Add(argument);
            }

            var vulnerableArguments = vulnerableArgumentsBuilder.ToImmutable();

            foreach (var argument in vulnerableArguments)
            {
                if (IsParameterSymbolVulnerable(argument.Parameter) && (isInDbCommandConstructor || isInDataAdapterConstructor))
                {
                    //No warnings, as Constructor parameters in derived classes are assumed to be safe since this rule will check the constructor arguments at their call sites.
                    return;
                }

                if (ReportDiagnosticIfNecessary(operationContext, argument.Value, invocationSyntax, constructorSymbol, containingSymbol))
                {
                    // Only report one warning per invocation
                    return;
                }
            }
        }

        private static bool IsParameterSymbolVulnerable(IParameterSymbol parameter)
        {
            // Parameters might be vulnerable if "cmd" or "command" is in the name
            return parameter != null &&
                   (parameter.Name.IndexOf("cmd", StringComparison.OrdinalIgnoreCase) != -1 ||
                    parameter.Name.IndexOf("command", StringComparison.OrdinalIgnoreCase) != -1);
        }

        private static bool ReportDiagnosticIfNecessary(OperationAnalysisContext operationContext,
                                                 IOperation argumentValue,
                                                 SyntaxNode syntax,
                                                 ISymbol invokedSymbol,
                                                 ISymbol containingMethod)
        {
            if (argumentValue.Type.SpecialType != SpecialType.System_String || !argumentValue.ConstantValue.HasValue)
            {
                // We have a candidate for diagnostic. perform more precise dataflow analysis.
                var topmostBlock = argumentValue.GetTopmostParentBlock();
                if (topmostBlock != null)
                {
                    var cfg = ControlFlowGraph.Create(topmostBlock);
                    var nullAnalysisResult = NullAnalysis.GetOrComputeResult(cfg, containingMethod.ContainingType);
                    var pointsToAnalysisResult = PointsToAnalysis.GetOrComputeResult(cfg, containingMethod.ContainingType, nullAnalysisResult);
                    var stringContentResult = StringContentAnalysis.GetOrComputeResult(cfg, containingMethod.ContainingType, nullAnalysisResult, pointsToAnalysisResult);
                    StringContentAbstractValue value = stringContentResult[argumentValue];
                    if (value.NonLiteralState == StringContainsNonLiteralState.No)
                    {
                        // The value is a constant literal or default/unitialized, so avoid flagging this usage.
                        return false;
                    }
                }

                // Review if the symbol passed to {invocation} in {method/field/constructor/etc} has user input.
                operationContext.ReportDiagnostic(syntax.CreateDiagnostic(Rule,
                                                                          invokedSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                                          containingMethod.Name));
                return true;
            }

            return false;
        }

        private static void CheckForDbCommandAndDataAdapterImplementation(INamedTypeSymbol containingType,
                                                                          INamedTypeSymbol iDbCommandType,
                                                                          INamedTypeSymbol iDataAdapterType,
                                                                          out bool implementsDbCommand,
                                                                          out bool implementsDataCommand)
        {
            implementsDbCommand = false;
            implementsDataCommand = false;
            foreach (var @interface in containingType.AllInterfaces)
            {
                if (@interface == iDbCommandType)
                {
                    implementsDbCommand = true;
                }
                else if (@interface == iDataAdapterType)
                {
                    implementsDataCommand = true;
                }
            }
        }
    }
}
