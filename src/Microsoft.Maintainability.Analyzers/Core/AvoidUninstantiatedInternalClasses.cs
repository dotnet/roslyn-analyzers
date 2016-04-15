// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.Maintainability.Analyzers
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
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182265.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(startContext =>
            {
                var instantiatedTypes = new List<INamedTypeSymbol>();
                var internalTypes = new List<INamedTypeSymbol>();

                startContext.RegisterOperationAction(context =>
                {
                    IObjectCreationExpression expr = (IObjectCreationExpression)context.Operation;
                    var namedType = expr.Type as INamedTypeSymbol;
                    if (namedType != null)
                    {
                        instantiatedTypes.Add(namedType);
                    }
                }, OperationKind.ObjectCreationExpression);

                startContext.RegisterSymbolAction(context =>
                {
                    INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
                    if (type.GetResultantVisibility() != SymbolVisibility.Public && !IsOkToBeUnused(type, context.Compilation))
                    {
                        internalTypes.Add(type);
                    }
                }, SymbolKind.NamedType);

                startContext.RegisterCompilationEndAction(context =>
                {
                    IEnumerable<INamedTypeSymbol> uninstantiatedInternalTypes = internalTypes
                        .Except(instantiatedTypes)
                        .Where(type => !HasInstantiatedNestedType(type, instantiatedTypes));

                    foreach (INamedTypeSymbol type in uninstantiatedInternalTypes)
                    {
                        context.ReportDiagnostic(type.CreateDiagnostic(Rule, type.FormatMemberName()));
                    }
                });
            });
        }

        private bool HasInstantiatedNestedType(INamedTypeSymbol type, List<INamedTypeSymbol> instantiatedTypes)
        {
            // We don't care whether a private nested type is instantiated, because if it
            // is, it can only have happened within the type itself.
            IEnumerable<INamedTypeSymbol> nestedTypes = type.GetTypeMembers()
                .Where(member => member.DeclaredAccessibility != Accessibility.Private);

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

        public static bool IsOkToBeUnused(INamedTypeSymbol type, Compilation compilation)
        {
            if (type.TypeKind != TypeKind.Class || type.IsAbstract)
            {
                return true;
            }

            // Attributes are not instantiated in IL but are created by reflection.
            INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName("System.Attribute");
            if (type.Inherits(attributeSymbol))
            {
                return true;
            }

            // The type containing the assembly's entry point is OK.
            if (ContainsEntryPoint(type, compilation))
            {
                return true;
            }

            // MEF exported classes are instantiated by MEF, by reflection.
            if (IsMefExported(type, compilation))
            {
                return true;
            }

            // Types implementing the (deprecated) IConfigurationSectionHandler interface
            // are OK because they are instantiated by the configuration system.
            INamedTypeSymbol iConfigurationSectionHandlerSymbol = compilation.GetTypeByMetadataName("System.Configuration.IConfigurationSectionHandler");
            if (type.Inherits(iConfigurationSectionHandlerSymbol))
            {
                return true;
            }

            // Likewise for types derived from ConfigurationSection.
            INamedTypeSymbol configurationSection = compilation.GetTypeByMetadataName("System.Configuration.ConfigurationSection");
            if (type.Inherits(configurationSection))
            {
                return true;
            }

            // SafeHandles can be created from within the type itself by native code.
            INamedTypeSymbol safeHandle = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.SafeHandle");
            if (type.Inherits(safeHandle))
            {
                return true;
            }

            INamedTypeSymbol traceListener = compilation.GetTypeByMetadataName("System.Diagnostics.TraceListener");
            if (type.Inherits(traceListener))
            {
                return true;
            }

            if (type.IsStaticHolderType())
            {
                return true;
            }

            return false;
        }

        public static bool IsMefExported(INamedTypeSymbol type, Compilation compilation)
        {
            INamedTypeSymbol mef1ExportAttributeSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.Composition.ExportAttribute");
            INamedTypeSymbol mef2ExportAttributeSymbol = compilation.GetTypeByMetadataName("System.Composition.ExportAttribute");

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