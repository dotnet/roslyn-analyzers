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
        internal const string RuleId = "CA1834";
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
            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Check that the object is a StringBuilder
                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextStringBuilder, out INamedTypeSymbol? stringBuilderType))
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

                compilationContext.RegisterOperationAction(operationContext =>
                {
                    var invocationOperation = (IInvocationOperation)operationContext.Operation;
                    if (invocationOperation.Arguments.Length < 1)
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
                    if (!IsConstVar(firstArgument))
                    {
                        return;
                    }

                    if (firstArgument.Value is ILocalReferenceOperation argumentValue)
                    {
                        operationContext.ReportDiagnostic(argumentValue.CreateDiagnostic(Rule));
                    }
                    else if (firstArgument.Value is ILiteralOperation literalArgumentValue)
                    {
                        operationContext.ReportDiagnostic(literalArgumentValue.CreateDiagnostic(Rule));
                    }
                }
                , OperationKind.Invocation);
            });
        }

        private static bool IsConstVar(IArgumentOperation argument)
        {
            if (argument.Value.ConstantValue.HasValue && argument.Value.ConstantValue.Value is string unitString && unitString.Length == 1)
            {
                return true;
            }
            return false;
        }

    }
}
