// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// CA1812: Avoid uninstantiated internal classes
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidUninstantiatedInternalClassesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1812";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUninstantiatedInternalClassesTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUninstantiatedInternalClassesMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUninstantiatedInternalClassesDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182265.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            analysisContext.RegisterCompilationStartAction(startContext =>
            {
                var instantiatedTypes = new ConcurrentBag<INamedTypeSymbol>();
                var internalTypes = new ConcurrentBag<INamedTypeSymbol>();

                var compilation = startContext.Compilation;

                // If the assembly being built by this compilation exposes its internals to
                // any other assembly, don't report any "uninstantiated internal class" errors.
                // If we were to report an error for an internal type that is not instantiated
                // by this assembly, and then it turned out that the friend assembly did
                // instantiate the type, that would be a false positive. We've decided it's
                // better to have false negatives (which would happen if the type were *not*
                // instantiated by any friend assembly, but we didn't report the issue) than
                // to have false positives.
                var internalsVisibleToAttributeSymbol = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
                if (AssemblyExposesInternals(compilation, internalsVisibleToAttributeSymbol))
                {
                    return;
                }

                var systemAttributeSymbol = compilation.GetTypeByMetadataName("System.Attribute");
                var iConfigurationSectionHandlerSymbol = compilation.GetTypeByMetadataName("System.Configuration.IConfigurationSectionHandler");
                var configurationSectionSymbol = compilation.GetTypeByMetadataName("System.Configuration.ConfigurationSection");
                var safeHandleSymbol = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.SafeHandle");
                var traceListenerSymbol = compilation.GetTypeByMetadataName("System.Diagnostics.TraceListener");
                var mef1ExportAttributeSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.Composition.ExportAttribute");
                var mef2ExportAttributeSymbol = compilation.GetTypeByMetadataName("System.Composition.ExportAttribute");

                startContext.RegisterOperationAction(context =>
                {
                    var expr = (IObjectCreationOperation)context.Operation;
                    if (expr.Type is INamedTypeSymbol namedType)
                    {
                        instantiatedTypes.Add(namedType);
                    }
                }, OperationKind.ObjectCreation);

                startContext.RegisterSymbolAction(context =>
                {
                    var type = (INamedTypeSymbol)context.Symbol;
                    if (!type.IsExternallyVisible() &&
                        !IsOkToBeUnused(type, compilation,
                            systemAttributeSymbol,
                            iConfigurationSectionHandlerSymbol,
                            configurationSectionSymbol,
                            safeHandleSymbol,
                            traceListenerSymbol,
                            mef1ExportAttributeSymbol,
                            mef2ExportAttributeSymbol))
                    {
                        internalTypes.Add(type);
                    }

                    // Instantiation from the subtype constructor initializer.
                    if (type.BaseType != null)
                    {
                        instantiatedTypes.Add(type.BaseType);
                    }
                }, SymbolKind.NamedType);

                // If a type is passed a generic argument to another type or a method that specifies that the type must have a constructor,
                // we presume that the method will be constructing the type, and add it to the list of instantiated types.

                void ProcessGenericTypes(IEnumerable<(ITypeParameterSymbol param, ITypeSymbol arg)> generics)
                {
                    foreach (var (typeParam, typeArg) in generics)
                    {
                        if (typeParam.HasConstructorConstraint)
                        {
                            void ProcessNamedTypeParamConstraint(INamedTypeSymbol namedTypeArg)
                            {
                                instantiatedTypes.Add(namedTypeArg);

                                // We need to handle if this type param also has type params that have a generic constraint. Take the following example:
                                // new Factory1<Factory2<InstantiatedType>>();
                                // In this example, Factory1 and Factory2 have type params with constructor constraints. Therefore, we need to add all 3
                                // types to the list of types that have actually been instantiated. However, in the following example:
                                // new List<Factory<InstantiatedType>>();
                                // List does not have a constructor constraint, so we can't reasonably infer anything about its type parameters.
                                if (namedTypeArg.IsGenericType)
                                {
                                    var newGenerics = namedTypeArg.TypeParameters.Zip(namedTypeArg.TypeArguments, (parameter, argument) => (parameter, argument));
                                    ProcessGenericTypes(newGenerics);
                                }
                            };

                            if (typeArg is INamedTypeSymbol namedType)
                            {
                                ProcessNamedTypeParamConstraint(namedType);
                            }
                            else if (typeArg is ITypeParameterSymbol typeParameterArg && !typeParameterArg.ConstraintTypes.IsEmpty)
                            {
                                IEnumerable<INamedTypeSymbol> GetAllNamedTypeConstraints(ITypeParameterSymbol t)
                                {
                                    var directConstraints = t.ConstraintTypes.OfType<INamedTypeSymbol>();
                                    var inheritedConstraints = t.ConstraintTypes.OfType<ITypeParameterSymbol>()
                                        .SelectMany(constraintT => GetAllNamedTypeConstraints(constraintT));
                                    return directConstraints.Concat(inheritedConstraints);
                                };

                                var constraints = GetAllNamedTypeConstraints(typeParameterArg);
                                foreach (INamedTypeSymbol constraint in constraints)
                                {
                                    ProcessNamedTypeParamConstraint(constraint);
                                }
                            }
                        }
                    }
                }

                startContext.RegisterOperationAction(context =>
                {
                    var expr = (IObjectCreationOperation)context.Operation;
                    var constructedClass = (INamedTypeSymbol)expr.Type;

                    if (!constructedClass.IsGenericType || constructedClass.IsUnboundGenericType)
                    {
                        return;
                    }

                    var generics = constructedClass.TypeParameters.Zip(constructedClass.TypeArguments, (parameter, argument) => (parameter, argument));
                    ProcessGenericTypes(generics);
                }, OperationKind.ObjectCreation);

                startContext.RegisterOperationAction(context =>
                {
                    var expr = (IInvocationOperation)context.Operation;
                    var methodType = expr.TargetMethod;

                    if (!methodType.IsGenericMethod)
                    {
                        return;
                    }

                    var generics = methodType.TypeParameters.Zip(methodType.TypeArguments, (parameter, argument) => (parameter, argument));
                    ProcessGenericTypes(generics);
                }, OperationKind.Invocation);

                startContext.RegisterCompilationEndAction(context =>
                {
                    var uninstantiatedInternalTypes = internalTypes
                        .Select(it => it.OriginalDefinition)
                        .Except(instantiatedTypes.Select(it => it.OriginalDefinition))
                        .Where(type => !HasInstantiatedNestedType(type, instantiatedTypes));

                    foreach (var type in uninstantiatedInternalTypes)
                    {
                        context.ReportDiagnostic(type.CreateDiagnostic(Rule, type.FormatMemberName()));
                    }
                });
            });
        }

        private bool AssemblyExposesInternals(
            Compilation compilation,
            INamedTypeSymbol internalsVisibleToAttributeSymbol)
        {
            ISymbol assemblySymbol = compilation.Assembly;
            var attributes = assemblySymbol.GetAttributes();
            return attributes.Any(
                attr => attr.AttributeClass.Equals(internalsVisibleToAttributeSymbol));
        }

        private bool HasInstantiatedNestedType(INamedTypeSymbol type, IEnumerable<INamedTypeSymbol> instantiatedTypes)
        {
            // We don't care whether a private nested type is instantiated, because if it
            // is, it can only have happened within the type itself.
            var nestedTypes = type.GetTypeMembers().Where(member => member.DeclaredAccessibility != Accessibility.Private);

            foreach (var nestedType in nestedTypes)
            {
                if (instantiatedTypes.Contains(nestedType))
                {
                    return true;
                }

                if (HasInstantiatedNestedType(nestedType, instantiatedTypes))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsOkToBeUnused(
            INamedTypeSymbol type,
            Compilation compilation,
            INamedTypeSymbol systemAttributeSymbol,
            INamedTypeSymbol iConfigurationSectionHandlerSymbol,
            INamedTypeSymbol configurationSectionSymbol,
            INamedTypeSymbol safeHandleSymbol,
            INamedTypeSymbol traceListenerSymbol,
            INamedTypeSymbol mef1ExportAttributeSymbol,
            INamedTypeSymbol mef2ExportAttributeSymbol)
        {
            if (type.TypeKind != TypeKind.Class || type.IsAbstract)
            {
                return true;
            }

            // Attributes are not instantiated in IL but are created by reflection.
            if (type.Inherits(systemAttributeSymbol))
            {
                return true;
            }

            // The type containing the assembly's entry point is OK.
            if (ContainsEntryPoint(type, compilation))
            {
                return true;
            }

            // MEF exported classes are instantiated by MEF, by reflection.
            if (IsMefExported(type, mef1ExportAttributeSymbol, mef2ExportAttributeSymbol))
            {
                return true;
            }

            // Types implementing the (deprecated) IConfigurationSectionHandler interface
            // are OK because they are instantiated by the configuration system.
            if (type.Inherits(iConfigurationSectionHandlerSymbol))
            {
                return true;
            }

            // Likewise for types derived from ConfigurationSection.
            if (type.Inherits(configurationSectionSymbol))
            {
                return true;
            }

            // SafeHandles can be created from within the type itself by native code.
            if (type.Inherits(safeHandleSymbol))
            {
                return true;
            }

            if (type.Inherits(traceListenerSymbol))
            {
                return true;
            }

            if (type.IsStaticHolderType())
            {
                return true;
            }

            return false;
        }
        public static bool IsMefExported(
            INamedTypeSymbol type,
            INamedTypeSymbol mef1ExportAttributeSymbol,
            INamedTypeSymbol mef2ExportAttributeSymbol)
        {
            return (mef1ExportAttributeSymbol != null && type.HasAttribute(mef1ExportAttributeSymbol))
                || (mef2ExportAttributeSymbol != null && type.HasAttribute(mef2ExportAttributeSymbol));
        }

        private static bool ContainsEntryPoint(INamedTypeSymbol type, Compilation compilation)
        {
            // If this type doesn't live in an application assembly (.exe), it can't contain
            // the entry point.
            if (compilation.Options.OutputKind != OutputKind.ConsoleApplication &&
                compilation.Options.OutputKind != OutputKind.WindowsApplication &&
                compilation.Options.OutputKind != OutputKind.WindowsRuntimeApplication)
            {
                return false;
            }

            // TODO: Handle the case where Compilation.Options.MainTypeName matches this type.
            // TODO: Test: can't have type parameters.
            // TODO: Main in nested class? If allowed, what name does it have?
            // TODO: Test that parameter is array of int.
            return type.GetMembers("Main")
                .Where(m => m is IMethodSymbol)
                .Cast<IMethodSymbol>()
                .Any(IsEntryPoint);
        }

        private static bool IsEntryPoint(IMethodSymbol method)
        {
            if (!method.IsStatic)
            {
                return false;
            }

            if (method.ReturnType.SpecialType != SpecialType.System_Int32 && !method.ReturnsVoid)
            {
                return false;
            }

            if (method.Parameters.Count() == 0)
            {
                return true;
            }

            if (method.Parameters.Count() > 1)
            {
                return false;
            }

            ITypeSymbol parameterType = method.Parameters.Single().Type;

            return true;
        }
    }
}