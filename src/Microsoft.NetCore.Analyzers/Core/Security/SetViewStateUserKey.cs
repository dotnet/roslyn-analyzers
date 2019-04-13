﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SetViewStateUserKey : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5368";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.SetViewStateUserKey),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.SetViewStateUserKeyMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.SetViewStateUserKeyDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                s_Message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: false,    // https://github.com/dotnet/roslyn-analyzers/issues/2258
                description: s_Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var compilation = compilationStartAnalysisContext.Compilation;
                var pageTypeSymbol = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemWebUIPage);

                if (pageTypeSymbol == null)
                {
                    return;
                }

                var eventArgsTypeSymbol = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemEventArgs);

                if (eventArgsTypeSymbol == null)
                {
                    return;
                }

                var objectTypeSymbol = WellKnownTypes.Object(compilation);

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    var classSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;
                    var baseClassSymbol = classSymbol.BaseType;

                    if (pageTypeSymbol.Equals(baseClassSymbol))
                    {
                        var methods = classSymbol.GetMembers().OfType<IMethodSymbol>();
                        var setViewStateUserKeyInOnInit = SetViewStateUserKeyCorrectly(methods.FirstOrDefault(s => s.Name == "OnInit" &&
                                                                                                                    s.Parameters.Length == 1 &&
                                                                                                                    s.Parameters[0].Type.Equals(eventArgsTypeSymbol) &&
                                                                                                                    s.IsProtected() &&
                                                                                                                    !s.IsStatic));
                        var setViewStateUserKeyInPage_Init = SetViewStateUserKeyCorrectly(methods.FirstOrDefault(s => s.Name == "Page_Init" &&
                                                                                                                        s.Parameters.Length == 2 &&
                                                                                                                        objectTypeSymbol != null &&
                                                                                                                        s.Parameters[0].Type.Equals(objectTypeSymbol) &&
                                                                                                                        s.Parameters[1].Type.Equals(eventArgsTypeSymbol) &&
                                                                                                                        s.ReturnType.SpecialType == SpecialType.System_Void));

                        if (setViewStateUserKeyInOnInit || setViewStateUserKeyInPage_Init)
                        {
                            return;
                        }

                        symbolAnalysisContext.ReportDiagnostic(
                                    classSymbol.CreateDiagnostic(
                                        Rule,
                                        classSymbol.Name));
                    }
                }, SymbolKind.NamedType);

                bool SetViewStateUserKeyCorrectly(IMethodSymbol methodSymbol)
                {
                    return methodSymbol?.GetTopmostOperationBlock(compilation)
                                        .Descendants()
                                        .Where(s => s is ISimpleAssignmentOperation simpleAssignmentOperation &&
                                                    simpleAssignmentOperation.Target is IPropertyReferenceOperation propertyReferenceOperation &&
                                                    propertyReferenceOperation.Property.Name == "ViewStateUserKey" &&
                                                    propertyReferenceOperation.Property.Type.SpecialType == SpecialType.System_String &&
                                                    propertyReferenceOperation.Instance is IInstanceReferenceOperation instanceReferenceOperation &&
                                                    instanceReferenceOperation.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
                                        .Count() > 0;
                }
            });
        }
    }
}
