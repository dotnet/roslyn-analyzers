// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1820: Test for empty strings using string length.
    /// <para>
    /// Comparing strings using the <see cref="string.Length"/> property or the <see cref="string.IsNullOrEmpty"/> method is significantly faster than using <see cref="string.Equals(string)"/>.
    /// This is because Equals executes significantly more MSIL instructions than either IsNullOrEmpty or the number of instructions executed to retrieve the Length property value and compare it to zero.
    /// </para>
    /// </summary>
    public abstract class TestForEmptyStringsUsingStringLengthAnalyzer<TLanguageKindEnum> : DiagnosticAnalyzer
        where TLanguageKindEnum : struct
    {
        internal const string RuleId = "CA1820";
        private const string StringEmptyFieldName = "Empty";

        private static LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForEmptyStringsUsingStringLengthTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForEmptyStringsUsingStringLengthMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForEmptyStringsUsingStringLengthDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        protected static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/library/ms182279.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);


        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKindsOfInterest);

        protected abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
        protected abstract void AnalyzeNode(SyntaxNodeAnalysisContext context);

        protected static bool IsEqualsMethod(string methodName) =>
            string.Equals(methodName, WellKnownMemberNames.ObjectEquals, StringComparison.Ordinal);

        protected static bool IsEqualityOrInequalityOperator(IMethodSymbol methodSymbol) =>
            string.Equals(methodSymbol.Name, WellKnownMemberNames.EqualityOperatorName, StringComparison.Ordinal) ||
            string.Equals(methodSymbol.Name, WellKnownMemberNames.InequalityOperatorName, StringComparison.Ordinal);

        protected static bool IsEmptyString(SyntaxNode expression, SemanticModel model, CancellationToken cancellationToken)
        {
            if (expression == null)
            {
                return false;
            }

            var constantValueOpt = model.GetConstantValue(expression, cancellationToken);
            if (constantValueOpt.HasValue)
            {
                return (constantValueOpt.Value as string)?.Length == 0;
            }

            var symbol = model.GetSymbolInfo(expression, cancellationToken).Symbol as IFieldSymbol;
            return string.Equals(symbol?.Name, StringEmptyFieldName) &&
                symbol.Type?.SpecialType == SpecialType.System_String;
        }
    }
}
