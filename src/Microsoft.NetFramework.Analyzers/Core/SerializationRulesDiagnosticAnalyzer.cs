// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetFramework.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SerializationRulesDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        // Implement serialization constructors
        internal const string RuleCA2229Id = "CA2229";

        private static readonly LocalizableString s_localizableTitleCA2229 =
            new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ImplementSerializationConstructorsTitle),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        private static readonly LocalizableString s_localizableDescriptionCA2229 =
            new LocalizableResourceString(
                nameof(MicrosoftNetFrameworkAnalyzersResources.ImplementSerializationConstructorsDescription),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        internal static DiagnosticDescriptor RuleCA2229 = new DiagnosticDescriptor(RuleCA2229Id,
                                                                        s_localizableTitleCA2229,
                                                                        "{0}",
                                                                        DiagnosticCategory.Usage,
                                                                        DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                        isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
                                                                        description: s_localizableDescriptionCA2229,
                                                                        helpLinkUri: "http://msdn.microsoft.com/library/ms182343.aspx",
                                                                        customTags: WellKnownDiagnosticTags.Telemetry);

        // Mark ISerializable types with SerializableAttribute
        internal const string RuleCA2237Id = "CA2237";

        private static readonly LocalizableString s_localizableTitleCA2237 =
            new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.MarkISerializableTypesWithSerializableTitle),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageCA2237 =
            new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.MarkISerializableTypesWithSerializableMessage),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        private static readonly LocalizableString s_localizableDescriptionCA2237 =
            new LocalizableResourceString(
                nameof(MicrosoftNetFrameworkAnalyzersResources.MarkISerializableTypesWithSerializableDescription),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        internal static DiagnosticDescriptor RuleCA2237 = new DiagnosticDescriptor(RuleCA2237Id,
                                                                        s_localizableTitleCA2237,
                                                                        s_localizableMessageCA2237,
                                                                        DiagnosticCategory.Usage,
                                                                        DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                        isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
                                                                        description: s_localizableDescriptionCA2237,
                                                                        helpLinkUri: "http://msdn.microsoft.com/library/ms182350.aspx",
                                                                        customTags: WellKnownDiagnosticTags.Telemetry);

        // Mark all non-serializable fields
        internal const string RuleCA2235Id = "CA2235";

        private static readonly LocalizableString s_localizableTitleCA2235 =
            new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.MarkAllNonSerializableFieldsTitle),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageCA2235 =
            new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.MarkAllNonSerializableFieldsMessage),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        private static readonly LocalizableString s_localizableDescriptionCA2235 =
            new LocalizableResourceString(
                nameof(MicrosoftNetFrameworkAnalyzersResources.MarkAllNonSerializableFieldsDescription),
                MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        internal static DiagnosticDescriptor RuleCA2235 = new DiagnosticDescriptor(RuleCA2235Id,
                                                                        s_localizableTitleCA2235,
                                                                        s_localizableMessageCA2235,
                                                                        DiagnosticCategory.Usage,
                                                                        DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                        isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
                                                                        description: s_localizableDescriptionCA2235,
                                                                        helpLinkUri: "http://msdn.microsoft.com/library/ms182349.aspx",
                                                                        customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleCA2229, RuleCA2235, RuleCA2237);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    INamedTypeSymbol iserializableTypeSymbol = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.ISerializable");
                    if (iserializableTypeSymbol == null)
                    {
                        return;
                    }

                    INamedTypeSymbol serializationInfoTypeSymbol = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.SerializationInfo");
                    if (serializationInfoTypeSymbol == null)
                    {
                        return;
                    }

                    INamedTypeSymbol streamingContextTypeSymbol = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.StreamingContext");
                    if (streamingContextTypeSymbol == null)
                    {
                        return;
                    }

                    INamedTypeSymbol serializableAttributeTypeSymbol = context.Compilation.GetTypeByMetadataName("System.SerializableAttribute");
                    if (serializableAttributeTypeSymbol == null)
                    {
                        return;
                    }

                    INamedTypeSymbol nonSerializedAttributeTypeSymbol = context.Compilation.GetTypeByMetadataName("System.NonSerializedAttribute");
                    if (nonSerializedAttributeTypeSymbol == null)
                    {
                        return;
                    }

                    context.RegisterSymbolAction(new SymbolAnalyzer(iserializableTypeSymbol, serializationInfoTypeSymbol, streamingContextTypeSymbol, serializableAttributeTypeSymbol, nonSerializedAttributeTypeSymbol).AnalyzeSymbol, SymbolKind.NamedType);
                });
        }

        private sealed class SymbolAnalyzer
        {
            private readonly INamedTypeSymbol _iserializableTypeSymbol;
            private readonly INamedTypeSymbol _serializationInfoTypeSymbol;
            private readonly INamedTypeSymbol _streamingContextTypeSymbol;
            private readonly INamedTypeSymbol _serializableAttributeTypeSymbol;
            private readonly INamedTypeSymbol _nonSerializedAttributeTypeSymbol;

            public SymbolAnalyzer(
                INamedTypeSymbol iserializableTypeSymbol,
                INamedTypeSymbol serializationInfoTypeSymbol,
                INamedTypeSymbol streamingContextTypeSymbol,
                INamedTypeSymbol serializableAttributeTypeSymbol,
                INamedTypeSymbol nonSerializedAttributeTypeSymbol)
            {
                _iserializableTypeSymbol = iserializableTypeSymbol;
                _serializationInfoTypeSymbol = serializationInfoTypeSymbol;
                _streamingContextTypeSymbol = streamingContextTypeSymbol;
                _serializableAttributeTypeSymbol = serializableAttributeTypeSymbol;
                _nonSerializedAttributeTypeSymbol = nonSerializedAttributeTypeSymbol;
            }

            public void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
                if (namedTypeSymbol.TypeKind == TypeKind.Delegate || namedTypeSymbol.TypeKind == TypeKind.Interface)
                {
                    return;
                }

                // If the type is public and implements ISerializable
                if (namedTypeSymbol.DeclaredAccessibility == Accessibility.Public && namedTypeSymbol.AllInterfaces.Contains(_iserializableTypeSymbol))
                {
                    if (!IsSerializable(namedTypeSymbol))
                    {
                        // CA2237 : Mark serializable types with the SerializableAttribute
                        if (namedTypeSymbol.BaseType.SpecialType == SpecialType.System_Object ||
                            IsSerializable(namedTypeSymbol.BaseType))
                        {
                            context.ReportDiagnostic(namedTypeSymbol.CreateDiagnostic(RuleCA2237, namedTypeSymbol.Name));
                        }
                    }
                    else
                    {
                        // Look for a serialization constructor.
                        // A serialization constructor takes two params of type SerializationInfo and StreamingContext.
                        IMethodSymbol serializationCtor = namedTypeSymbol.Constructors.Where(c => c.Parameters.Count() == 2 &&
                                                                                        c.Parameters[0].Type ==
                                                                                        _serializationInfoTypeSymbol &&
                                                                                        c.Parameters[1].Type ==
                                                                                        _streamingContextTypeSymbol)
                            .SingleOrDefault();

                        // There is no serialization ctor - issue a diagnostic.
                        if (serializationCtor == null)
                        {
                            context.ReportDiagnostic(namedTypeSymbol.CreateDiagnostic(RuleCA2229,
                                string.Format(MicrosoftNetFrameworkAnalyzersResources.ImplementSerializationConstructorsMessageCreateMagicConstructor,
                                    namedTypeSymbol.Name)));
                        }
                        else
                        {
                            // Check the accessibility
                            // The serialization ctor should be protected if the class is unsealed and private if the class is sealed.
                            if (namedTypeSymbol.IsSealed &&
                                serializationCtor.DeclaredAccessibility != Accessibility.Private)
                            {
                                context.ReportDiagnostic(serializationCtor.CreateDiagnostic(RuleCA2229,
                                    string.Format(
                                        MicrosoftNetFrameworkAnalyzersResources.ImplementSerializationConstructorsMessageMakeSealedMagicConstructorPrivate,
                                        namedTypeSymbol.Name)));
                            }

                            if (!namedTypeSymbol.IsSealed &&
                                serializationCtor.DeclaredAccessibility != Accessibility.Protected)
                            {
                                context.ReportDiagnostic(serializationCtor.CreateDiagnostic(RuleCA2229,
                                    string.Format(
                                        MicrosoftNetFrameworkAnalyzersResources.ImplementSerializationConstructorsMessageMakeUnsealedMagicConstructorFamily,
                                        namedTypeSymbol.Name)));
                            }
                        }
                    }
                }

                // If this is type is marked Serializable check it's fields types' as well
                if (IsSerializable(namedTypeSymbol))
                {
                    System.Collections.Generic.IEnumerable<IFieldSymbol> nonSerializableFields =
                        namedTypeSymbol.GetMembers().OfType<IFieldSymbol>().Where(m => !IsSerializable(m.Type));
                    foreach (IFieldSymbol field in nonSerializableFields)
                    {
                        // Check for [NonSerialized]
                        if (field.GetAttributes().Any(x => x.AttributeClass.Equals(_nonSerializedAttributeTypeSymbol)))
                        {
                            continue;
                        }

                        if (field.IsImplicitlyDeclared && field.AssociatedSymbol != null)
                        {
                            context.ReportDiagnostic(field.AssociatedSymbol.CreateDiagnostic(RuleCA2235,
                                field.AssociatedSymbol.Name, namedTypeSymbol.Name, field.Type));
                        }
                        else
                        {
                            context.ReportDiagnostic(field.CreateDiagnostic(RuleCA2235, field.Name, namedTypeSymbol.Name,
                                field.Type));
                        }
                    }
                }
            }

            private bool IsSerializable(ITypeSymbol type)
            {
                switch (type.TypeKind)
                {
                    case TypeKind.Array:
                        return IsSerializable(((IArrayTypeSymbol)type).ElementType);

                    case TypeKind.Enum:
                        return IsSerializable(((INamedTypeSymbol)type).EnumUnderlyingType);

                    case TypeKind.TypeParameter:
                    case TypeKind.Interface:
                        // The concrete type can't be determined statically,
                        // so we assume true to cut down on noise.
                        return true;

                    case TypeKind.Delegate:
                        // delegates are always serializable, even if
                        // they aren't actually marked [Serializable]
                        return true;

                    default:
                        return IsPrimitiveType(type) ||
                            type.SpecialType == SpecialType.System_String ||
                            type.SpecialType == SpecialType.System_Decimal ||
                            type.GetAttributes()
                                .Any(a => a.AttributeClass.Equals(_serializableAttributeTypeSymbol));
                }
            }

            private static bool IsPrimitiveType(ITypeSymbol type)
            {
                switch (type.SpecialType)
                {
                    case SpecialType.System_Boolean:
                    case SpecialType.System_Byte:
                    case SpecialType.System_Char:
                    case SpecialType.System_Double:
                    case SpecialType.System_Int16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_UInt64:
                    case SpecialType.System_IntPtr:
                    case SpecialType.System_UIntPtr:
                    case SpecialType.System_SByte:
                    case SpecialType.System_Single:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
