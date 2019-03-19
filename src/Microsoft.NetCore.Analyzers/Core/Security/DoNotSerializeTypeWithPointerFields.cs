// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotSerializeTypeWithPointerFields : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5367";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotSerializeTypesWithPointerFields),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotSerializeTypesWithPointerFieldsMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotSerializeTypesWithPointerFieldsDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                s_Message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: false,
                description: s_Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var compilation = compilationStartAnalysisContext.Compilation;
                    var serializableAttributeTypeSymbol = WellKnownTypes.SerializableAttribute(compilation);

                    if (serializableAttributeTypeSymbol == null)
                    {
                        return;
                    }

                    var nonSerializedAttribute = WellKnownTypes.NonSerializedAttribute(compilation);
                    var visitedType = new ConcurrentDictionary<ITypeSymbol, bool>();
                    var pointerFields = new ConcurrentDictionary<IFieldSymbol, bool>();

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            LookForSerializationWithPointerFields((ITypeSymbol)symbolAnalysisContext.Symbol);
                        }, SymbolKind.NamedType);

                    compilationStartAnalysisContext.RegisterCompilationEndAction(
                        (CompilationAnalysisContext compilationAnalysisContext) =>
                        {
                            foreach (var pointerField in pointerFields.Keys)
                            {
                                compilationAnalysisContext.ReportDiagnostic(
                                    pointerField.CreateDiagnostic(
                                        Rule,
                                        pointerField.Name));
                            }
                        });


                    /// <summary>
                    /// Look for serialization of a type with pointer type fields directly and indirectly.
                    /// </summary>
                    /// <param name="typeSymbol">The symbol of the type to be analyzed</param>
                    void LookForSerializationWithPointerFields(ITypeSymbol typeSymbol)
                    {
                        if (typeSymbol.IsInSource() &&
                            typeSymbol.HasAttribute(serializableAttributeTypeSymbol) &&
                            visitedType.TryAdd(typeSymbol, true))
                        {
                            var fields = typeSymbol.GetMembers().OfType<IFieldSymbol>().Where(s => nonSerializedAttribute != null &&
                                                                                                !s.HasAttribute(nonSerializedAttribute) &&
                                                                                                !s.IsStatic);

                            foreach (var field in fields)
                            {
                                var fieldType = field.Type;

                                if (fieldType is IPointerTypeSymbol pointerTypeField &&
                                    (pointerTypeField.PointedAtType.TypeKind == TypeKind.Struct ||
                                    pointerTypeField.PointedAtType.TypeKind == TypeKind.Pointer))
                                {
                                    pointerFields.TryAdd(field, true);
                                }
                                else
                                {
                                    LookForSerializationWithPointerFields(fieldType);
                                }
                            }
                        }
                    }
                });
        }
    }
}
