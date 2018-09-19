// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Analyzer.Utilities;
    using Analyzer.Utilities.Extensions;
    using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
    using Microsoft.CodeAnalysis.Operations;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ReviewCodeForSqlInjectionVulnerabilities : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3001";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesTitle),
            MicrosoftNetCoreSecurityResources.ResourceManager,
            typeof(MicrosoftNetCoreSecurityResources));

        private static readonly LocalizableString Message = new LocalizableResourceString(
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesMessage),
            MicrosoftNetCoreSecurityResources.ResourceManager,
            typeof(MicrosoftNetCoreSecurityResources));

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            Title,
            Message,
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            //context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol iDbCommandType = WellKnownTypes.IDbCommand(compilationContext.Compilation);
                INamedTypeSymbol iDataAdapterType = WellKnownTypes.IDataAdapter(compilationContext.Compilation);
                IPropertySymbol commandTextProperty = iDbCommandType?.GetProperty("CommandText");

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
                        if (!propertyReference.Property.IsOverrideOrImplementationOfInterfaceMember(commandTextProperty))
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
                var cfg = argumentValue.GetEnclosingControlFlowGraph();
                var taintedDataAnalysisResult = TaintedDataAnalysis.GetOrComputeResult(cfg, operationContext.Compilation, containingMethod);
                //TaintedDataAbstractValue abstractValue = taintedDataAnalysisResult[argumentValue];
                TaintedDataAbstractValue abstractValue = taintedDataAnalysisResult[argumentValue.Kind, argumentValue.Syntax];
                if (abstractValue.Kind != TaintedDataAbstractValueKind.Tainted)
                {
                    return false;
                }

                // Potential SQL injection vulnerability was found where {0} may be tainted by user-controlled data in method {1}.
                operationContext.ReportDiagnostic(
                    syntax.CreateDiagnostic(
                        Rule,
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
