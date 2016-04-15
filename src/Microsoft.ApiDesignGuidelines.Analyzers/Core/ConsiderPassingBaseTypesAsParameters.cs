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
    /// CA1011: Consider passing base types as parameters. 
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
            // Analyzer Strategy: Track all usages of each parameter, including member accesses 
            // and passing as an argument to another method.
            // Compares the parameter type to the target type (i.e. the containing type for member
            // accesses, or the argument type for method calls), and identify a "most derived type"
            // based on this. If the most derived type is a base type of the parameter's type,  
            // recommend using that type instead.
            
            // Perform analysis on each method block
            context.RegisterOperationBlockStartAction(
                (blockStartContext) =>
                {
                    IMethodSymbol containingMethod = blockStartContext.OwningSymbol as IMethodSymbol;

                    if (containingMethod == null || containingMethod.IsOverride || containingMethod.IsVirtual)
                    {
                        return;
                    }

                    // data structures for tracking derived type
                    var mostDerivedTypeFor = new Dictionary<IParameterSymbol, ITypeSymbol>();
                    var allDerivedTypesFor = new Dictionary<IParameterSymbol, HashSet<ITypeSymbol>>();

                    // 1. Track passing the parameter as an argument to a method call
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

                    // 2. Track member references (fields, properties, etc)
                    //    E.g.  bool CanRead(FileStream s) => s.CanRead;
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

                    // 3. Track member invocations
                    //    E.g. string ReadLine(FileStream s) => s.ReadLine();
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

                    // -- Special cases that shut down analysis --

                    // If ever a parameter is cast to an even more derived type,
                    // then there's probably a good reason why this method is
                    // strongly typed. Also, if it is cast at all, we could lose
                    // track of its flow, so we ignore any parameter that's cast.

                    // (a) No warning for is expressions
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

                    // (b) No warning for explicit casts
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IConversionExpression;
                            IParameterSymbol parameter;
                            if (oper != null
                                && oper.IsExplicit
                                && (parameter = GetParameterSymbol(oper.Operand)) != null)
                            {
                                // prevent diagnostic for this parameter
                                RecordParameterUse(parameter, parameter.Type,
                                    mostDerivedTypeFor, allDerivedTypesFor);
                            }
                        },
                        OperationKind.ConversionExpression);

                    // Since we don't understand flow control, as soon as we see
                    // a parameter assigned to another variable, we will no longer
                    // be certain if its exact type is necessary or not, so
                    // we have to shut down the analysis.

                    // (c) shut down analysis when parameter is assigned in a variable declaration
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

                    // (d) shut down analysis when parameter is assigned in other assignments
                    blockStartContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            var oper = operationContext.Operation as IAssignmentExpression;
                            IParameterSymbol parameter;
                            if (oper != null && (parameter = GetParameterSymbol(oper.Value)) != null)
                            {
                                // prevent diagnostic for this parameter
                                RecordParameterUse(parameter, parameter.Type,
                                    mostDerivedTypeFor, allDerivedTypesFor); 
                            }
                        },
                        OperationKind.AssignmentExpression,
                        OperationKind.CompoundAssignmentExpression);

                    // Finally, check the maps containing most derived types and 
                    // fire diagnostics if a base class could be used instead.

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

                // Check that the most derived type found is not the same as the
                // parameter type, or 'object'
                if (mostDerived != null 
                    && !mostDerived.Equals(paramType)
                    && mostDerived.SpecialType != SpecialType.System_Object)
                {
                    // Verify that indeed the most derived type is assignable to all the derived types. 
                    // Sometimes this can be not true if a concrete class implements two completely 
                    // separate interfaces, like string implements ICloneable and IComparable.
                    bool descendantOfAll = true;
                    HashSet<ITypeSymbol> allDerived;
                    if (allDerivedTypesFor.TryGetValue(parameter, out allDerived) && allDerived != null)
                    {
                        descendantOfAll = allDerived.All(t => IsAssignableTo(mostDerived, t));
                    }

                    if (descendantOfAll)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations.FirstOrDefault(),
                            parameter.Name,
                            parameter.ContainingSymbol?.Name,
                            paramType,
                            mostDerived));
                    }
                }
            }
        }

        /// <summary>
        /// Record the most derived usage type (and all usage types) to use in 
        /// our evaluation at the end of the method. Note that setting usage type
        /// to the parameter's type effectively shuts down the analysis, because
        /// it will always be the most derived type.
        /// </summary>
        private void RecordParameterUse(
            IParameterSymbol parameter,
            ITypeSymbol usageType,
            TMapParameterToType mostDerivedTypeFor,
            TMapParameterToTypeSet allDerivedTypesFor)
        {
            var paramType = parameter.Type;

            if (parameter.IsThis
                || parameter.IsImplicitlyDeclared 
                || paramType.IsValueType
                || paramType.SpecialType != SpecialType.None // ignore all the special types
                || paramType.IsValueType
                )
            {
                return;
            }

            // if the usage type is not compatible with the parameter, shutdown
            // the analysis by making usage type the parameter type
            if (!IsAssignableTo(paramType, usageType))
            {
                usageType = paramType;
            }

            // Record the most derived usage type
            ITypeSymbol mostDerivedTypeSoFar;
            if (!mostDerivedTypeFor.TryGetValue(parameter, out mostDerivedTypeSoFar)
                || IsAssignableTo(usageType, mostDerivedTypeSoFar))
            {
                mostDerivedTypeFor[parameter] = usageType;
            }

            // Save all the usage types, so we can later verify that the most derived type
            // is assignable to them all (i.e. in case we get a class that implements two 
            // unrelated interfaces)
            HashSet<ITypeSymbol> derivedTypes = null;
            if (!allDerivedTypesFor.TryGetValue(parameter, out derivedTypes))
            {
                derivedTypes = new HashSet<ITypeSymbol>();
                allDerivedTypesFor[parameter] = derivedTypes;
            }
            if (!derivedTypes.Contains(usageType))
            {
                derivedTypes.Add(usageType);
            }
        }

        /// <summary>
        /// Utility to determine if the IOperation is a parameter reference. 
        /// Returns null if it is not.
        /// </summary>
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
