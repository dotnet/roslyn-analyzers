// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotPassMutableValueTypesByValueAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2019";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValueTitle), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValueMessage), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValueDescription), Resx.ResourceManager, typeof(Resx));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        private static readonly HashSet<string> _knownProblematicTypeNames = new()
        {
            WellKnownTypeNames.SystemThreadingSpinLock,
            WellKnownTypeNames.SystemTextJsonUtf8JsonReader
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(context =>
            {
                var parameter = (IParameterSymbol)context.Symbol;

                if (parameter.RefKind is RefKind.Ref or RefKind.Out || !parameter.IsInSource())
                    return;

                if (IsProblematicType(parameter.Type))
                {
                    foreach (var syntaxReference in parameter.DeclaringSyntaxReferences)
                    {
                        var location = Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span);
                        var diagnostic = location.CreateDiagnostic(Rule, parameter.Type.ToDisplayString(SymbolDisplayFormats.QualifiedTypeAndNamespaceSymbolDisplayFormat));
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }, SymbolKind.Parameter);
        }

        private static bool IsProblematicType(ITypeSymbol typeSymbol)
        {
            return _knownProblematicTypeNames.Contains(typeSymbol.ToString());
        }
    }
}
