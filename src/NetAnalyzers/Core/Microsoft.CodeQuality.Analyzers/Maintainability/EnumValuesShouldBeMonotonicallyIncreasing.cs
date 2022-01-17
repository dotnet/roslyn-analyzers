// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Numerics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using static MicrosoftCodeQualityAnalyzersResources;

    /// <summary>
    /// CA1510: Flags enum values that are not monotonically increasing.
    /// </summary>
    public abstract class EnumValuesShouldBeMonotonicallyIncreasing : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1510";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(EnumValuesShouldBeMonotonicallyIncreasingTitle)),
            CreateLocalizableResourceString(nameof(EnumValuesShouldBeMonotonicallyIncreasingMessage)),
            DiagnosticCategory.Maintainability,
            RuleLevel.Disabled,
            description: null,
            isPortedFxCopRule: false,
            isDataflowRule: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumSymbol)
            {
                return;
            }

            var members = enumSymbol.GetMembers();
            BigInteger? previous = null;
            IFieldSymbol? previousField = null;

            foreach (var member in members)
            {
                if (member is not IFieldSymbol { IsImplicitlyDeclared: false, HasConstantValue: true } field)
                {
                    continue;
                }

                var syntax = field.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);
                if (ShouldSkipSyntax(syntax))
                {
                    continue;
                }

                var current = ConstantValueToBigInteger(field.ConstantValue);

                // An error scenario for the given field. Just skip it as if we didn't see it.
                if (current is null)
                {
                    continue;
                }

                if (previous is not null && current < previous)
                {
                    context.ReportDiagnostic(field.CreateDiagnostic(Rule, field.Name, previousField!.Name));
                    break;
                }

                previous = current;
                previousField = field;
            }
        }

        private protected abstract bool ShouldSkipSyntax(SyntaxNode syntax);

        private static BigInteger? ConstantValueToBigInteger(object constantValue)
        {
            return constantValue switch
            {
                sbyte @sbyte => new BigInteger(@sbyte),
                byte @byte => new BigInteger(@byte),
                short @short => new BigInteger(@short),
                ushort @ushort => new BigInteger(@ushort),
                int @int => new BigInteger(@int),
                uint @uint => new BigInteger(@uint),
                long @long => new BigInteger(@long),
                ulong @ulong => new BigInteger(@ulong),
                _ => null,
            };
        }
    }
}
