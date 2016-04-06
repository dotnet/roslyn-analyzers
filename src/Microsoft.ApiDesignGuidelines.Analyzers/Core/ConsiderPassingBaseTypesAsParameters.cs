// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    using TMapParameterToType = Dictionary<IParameterSymbol, ITypeSymbol>;
    using TMapParameterToTypeSet = Dictionary<IParameterSymbol, HashSet<ITypeSymbol>>;

    /// <summary>
    /// CA1011: Consider passing base types as parameters
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ConsiderPassingBaseTypesAsParametersAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1011";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ConsiderPassingBaseTypesAsParametersTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ConsiderPassingBaseTypesAsParametersMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ConsiderPassingBaseTypesAsParametersDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "http://msdn.microsoft.com/library/ms182126.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // perform analysis on each method block
            context.RegisterOperationBlockStartAction(
                (blockStartContext) =>
                {
                    IMethodSymbol containingMethod = blockStartContext.OwningSymbol as IMethodSymbol;

                    if (containingMethod == null || containingMethod.IsOverride || containingMethod.IsVirtual) // TODO method.IsSpecialName
                    {
                        return;
                    }

                    // data structures for tracking derived type
                    var mostDerivedTypeFor = new Dictionary<IParameterSymbol, ITypeSymbol>();
                    var allDerivedTypesFor = new Dictionary<IParameterSymbol, HashSet<ITypeSymbol>>();

                    // store type of each parameter
                    //containingMethod.Parameters

                    // 1. check the arguments of all method calls for a parameter
                    //    E.g.  string Read1(FileStream s) => Read2(s);
                    //          string Read2(Stream s) => s.Read();
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var argOper = operationContext.Operation as IArgument;
                            IParameterSymbol parameter;
                            if (argOper != null && (parameter = GetParameterSymbol(argOper.Value)) != null)
                            {
                                RecordParameterUse(parameter, argOper.Parameter.Type,
                                    mostDerivedTypeFor, allDerivedTypesFor);
                            }
                        }, OperationKind.Argument);

                    // 2. check for member references (fields, properties, etc)
                    //    E.g.  bool CanRead(FileStream s) => s.CanRead; // no diagnostic because this is abstract
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IMemberReferenceExpression;
                            IParameterSymbol parameter;
                            if (oper != null && (parameter = GetParameterSymbol(oper.Instance)) != null)
                            {
                                RecordParameterUse(parameter, oper.Member.ContainingType,
                                    mostDerivedTypeFor, allDerivedTypesFor);
                            }
                        },
                        OperationKind.FieldReferenceExpression,
                        OperationKind.MethodBindingExpression,
                        OperationKind.PropertyReferenceExpression,
                        OperationKind.EventReferenceExpression);

                    // 3. check for member invocations
                    //    E.g.  string ReadLine(FileStream s) => s.ReadLine();
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IInvocationExpression;
                            IParameterSymbol parameter;
                            if (oper != null && (parameter = GetParameterSymbol(oper.Instance)) != null)
                            {
                                RecordParameterUse(parameter, oper.TargetMethod.ContainingType,
                                    mostDerivedTypeFor, allDerivedTypesFor);
                            }
                        },
                        OperationKind.InvocationExpression);

                    // 4a. no warning
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IIsTypeExpression;
                            IParameterSymbol parameter;
                            if (oper != null && (parameter = GetParameterSymbol(oper.Operand)) != null)
                            {
                                // prevent diagnostic for this parameter
                                RecordParameterUse(parameter, parameter.Type,
                                    mostDerivedTypeFor, allDerivedTypesFor);
                            }
                        },
                        OperationKind.IsTypeExpression);

                    // 4b. shut down analysis for conversions
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IConversionExpression;
                            IParameterSymbol parameter;
                            if (oper != null && (parameter = GetParameterSymbol(oper.Operand)) != null)
                            {
                                // prevent diagnostic for this parameter
                                RecordParameterUse(parameter, parameter.Type,
                                    mostDerivedTypeFor, allDerivedTypesFor);
                            }
                        },
                        OperationKind.ConversionExpression);

                    // 5a. shut down analysis for variable declarations
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IVariableDeclarationStatement;
                            IParameterSymbol parameter;
                            foreach (var var in oper.Variables)
                            {
                                if ((parameter = GetParameterSymbol(var.InitialValue)) != null)
                                {
                                    // prevent diagnostic for this parameter
                                    RecordParameterUse(parameter, parameter.Type,
                                        mostDerivedTypeFor, allDerivedTypesFor);
                                }
                            }
                        },
                        OperationKind.VariableDeclarationStatement);

                    // 5a. shut down analysis for assignments
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IAssignmentExpression;
                            IParameterSymbol parameter;
                            if (oper != null && (parameter = GetParameterSymbol(oper.Value)) != null)
                            {
                                // prevent diagnostic for this parameter
                                RecordParameterUse(parameter, parameter.Type,
                                    mostDerivedTypeFor, allDerivedTypesFor); // TODO iterate through child values
                            }
                        },
                        OperationKind.AssignmentExpression,
                        OperationKind.CompoundAssignmentExpression);

                    // tester
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IBlockStatement;
                            var desc = oper?.Descendants();
                            var names = desc?.Select(v => v.Syntax.ToString());
                            var kinds = desc?.Select(v => v.Kind);
                            if (oper != null) { }
                        },
                        OperationKind.BlockStatement);

                    // check parameter maps and fire diagnostics as appropriate
                    blockStartContext.RegisterOperationBlockEndAction(
                        (blockEndContext) =>
                        {
                            CheckParameters(blockEndContext, mostDerivedTypeFor, allDerivedTypesFor);
                        });
                });
        }


        private void CheckParameters(
            OperationBlockAnalysisContext context,
            TMapParameterToType mostDerivedTypeFor,
            TMapParameterToTypeSet allDerivedTypesFor)
        {
            foreach (var pair in mostDerivedTypeFor)
            {
                IParameterSymbol parameter = pair.Key;
                ITypeSymbol paramType = parameter.Type;
                ITypeSymbol mostDerived = pair.Value;

                if (mostDerived != null && !mostDerived.Equals(paramType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations.FirstOrDefault(), 
                        parameter.Name,
                        parameter.ContainingSymbol?.Name,
                        paramType,
                        mostDerived));
                }
            }
        }

        private void RecordParameterUse(
            IParameterSymbol parameter,
            ITypeSymbol usageType,
            TMapParameterToType mostDerivedTypeFor,
            TMapParameterToTypeSet allDerivedTypesFor)
        {
            var paramType = parameter.Type;

            if (parameter.IsThis
                || parameter.IsImplicitlyDeclared // TODO add is Out parameter
                || paramType.IsValueType
                || paramType.SpecialType != SpecialType.None // ignore all the special types
                || paramType.IsValueType
                )
            {
                return;
            }

            if (!IsAssignableTo(paramType, usageType))
            {
                usageType = paramType;
            }

            ITypeSymbol mostDerivedTypeSoFar;
            if (!mostDerivedTypeFor.TryGetValue(parameter, out mostDerivedTypeSoFar)
                || IsAssignableTo(usageType, mostDerivedTypeSoFar))
            {
                mostDerivedTypeFor[parameter] = usageType;
            }

            // TODO All Derived Types
        }

        static IParameterSymbol GetParameterSymbol(IOperation value)
        {
            if (value?.Kind == OperationKind.ConversionExpression)
            {
                value = ((IConversionExpression)value).Operand;
            }

            return (value as IParameterReferenceExpression)?.Parameter;
        }

        static bool IsAssignableTo(ITypeSymbol derivedType, ITypeSymbol baseType)
        {
            if (derivedType.Equals(baseType))
            {
                return true;
            }

            if (derivedType.TypeKind == TypeKind.Class || derivedType.TypeKind == TypeKind.Structure)
            {
                INamedTypeSymbol derivedBaseType = derivedType.BaseType;
                return derivedBaseType != null && (derivedBaseType.Equals(baseType) || IsAssignableTo(derivedBaseType, baseType));
            }

            else if (derivedType.TypeKind == TypeKind.Interface)
            {
                if (derivedType.Interfaces.Contains(baseType))
                {
                    return true;
                }

                foreach (INamedTypeSymbol baseInterface in derivedType.Interfaces)
                {
                    if (IsAssignableTo(baseInterface, baseType))
                    {
                        return true;
                    }
                }

                return baseType.TypeKind == TypeKind.Class && baseType.SpecialType == SpecialType.System_Object;
            }

            return false;
        }
    }
}