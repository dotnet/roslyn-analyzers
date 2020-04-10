// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// Test for single character strings passed in to String.Append
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferConstCharOverConstUnitStringAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1831";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableMessage,
                                                                                      DiagnosticCategory.Performance,
                                                                                      RuleLevel.IdeSuggestion,
                                                                                      s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(AnalyzeSymbol, OperationKind.Invocation);
        }

        private static void AnalyzeSymbol(OperationAnalysisContext context)
        {
            IInvocationOperation invocationOperation = (IInvocationOperation)context.Operation;
            if (invocationOperation.Arguments.Length < 1)
            {
                return;
            }

            // Check that the object is a StringBuilder
            if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextStringBuilder, out INamedTypeSymbol? stringBuilderType))
            {
                return;
            }

            IMethodSymbol appendStringMethod = stringBuilderType
                .GetMembers("Append")
                .OfType<IMethodSymbol>()
                .WhereAsArray(s =>
                    s.Parameters.Length == 1 &&
                    s.Parameters[0].Type.SpecialType == SpecialType.System_String)
                .FirstOrDefault();
            if (appendStringMethod is null)
            {
                return;
            }

            if (!invocationOperation.TargetMethod.Equals(appendStringMethod))
            {
                return;
            }

            ImmutableArray<IArgumentOperation> arguments = invocationOperation.Arguments;
            IArgumentOperation firstArgument = arguments[0];

            // We don't handle class fields. Only local declarations
            if (!(firstArgument.Value is ILocalReferenceOperation argumentValue))
            {
                return;
            }

            ILocalSymbol localArgumentDeclaration = argumentValue.Local;

            // If there are multiple declarations, bail
            var semanticModel = context.Operation.SemanticModel;
            var cancellationToken = context.CancellationToken;
            SyntaxReference declaringSyntaxReference = localArgumentDeclaration.DeclaringSyntaxReferences.First();
            var variableDeclarator = semanticModel.GetOperationWalkingUpParentChain(declaringSyntaxReference.GetSyntax(cancellationToken), cancellationToken);
            if (variableDeclarator is IVariableDeclaratorOperation variableDeclaratorOperation)
            {
                IVariableDeclarationOperation variableDeclarationOperation = (IVariableDeclarationOperation)variableDeclaratorOperation.Parent;
                if (variableDeclarationOperation == null)
                {
                    return;
                }

                IVariableDeclarationGroupOperation variableGroupDeclarationOperation = (IVariableDeclarationGroupOperation)variableDeclarationOperation.Parent;
                if (variableGroupDeclarationOperation.Declarations.Length != 1)
                {
                    return;
                }

                if (variableDeclarationOperation.Declarators.Length != 1)
                {
                    return;
                }
            }

            // Ok, single variable declaration
            if (localArgumentDeclaration.HasConstantValue && localArgumentDeclaration.ConstantValue is string unitString && unitString.Length == 1)
            {
                context.ReportDiagnostic(localArgumentDeclaration.CreateDiagnostic(Rule));
            }
        }
    }
}
