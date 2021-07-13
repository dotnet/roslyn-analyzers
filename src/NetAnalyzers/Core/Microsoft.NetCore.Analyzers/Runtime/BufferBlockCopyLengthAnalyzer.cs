// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// Check for the intended use of .Length on arrays passed into Buffer.BlockCopy
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class BufferBlockCopyLengthAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2250";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.BufferBlockCopyLengthTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.BufferBlockCopyLengthMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.BufferBlockCopyDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableMessage,
                                                                                      DiagnosticCategory.Usage,
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
                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBuffer, out INamedTypeSymbol? bufferType))
                {
                    return;
                }

                IMethodSymbol blockCopyMethod = bufferType
                   .GetMembers("BlockCopy")
                   .OfType<IMethodSymbol>()
                   .FirstOrDefault();

                if (blockCopyMethod is null)
                {
                    return;
                }

                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemArray, out INamedTypeSymbol? arrayType))
                {
                    return;
                }

                IPropertySymbol arrayLengthProperty = arrayType
                    .GetMembers("Length")
                    .OfType<IPropertySymbol>()
                    .FirstOrDefault();

                if (arrayLengthProperty is null)
                {
                    return;
                }

                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemByte, out INamedTypeSymbol? byteType))
                {
                    return;
                }

                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSByte, out INamedTypeSymbol? sByteType))
                {
                    return;
                }

                compilationContext.RegisterOperationAction(context =>
                {
                    var invocationOperation = (IInvocationOperation)context.Operation;
                    if (invocationOperation.Arguments.IsEmpty)
                    {
                        return;
                    }

                    ImmutableArray<IArgumentOperation> arguments = invocationOperation.Arguments;
                    if (arguments.Length != 5)
                    {
                        return;
                    }

                    if (!invocationOperation.TargetMethod.Equals(blockCopyMethod))
                    {
                        return;
                    }

                    IArgumentOperation sourceArgument = arguments[0];
                    IArgumentOperation destinationArgument = arguments[2];
                    IArgumentOperation lastArgument = arguments[4];

                    bool CheckArrayLengthLocalReference(IArgumentOperation targetArgument, IPropertyReferenceOperation lengthPropertyArgument)
                    {
                        if (targetArgument.Value is IConversionOperation targetArgValue)
                        {
                            if (lengthPropertyArgument.Instance.GetReferencedMemberOrLocalOrParameter() == targetArgValue.Operand.GetReferencedMemberOrLocalOrParameter())
                            {
                                IArrayTypeSymbol lastArgumentArrayTypeSymbol = (IArrayTypeSymbol)lengthPropertyArgument.Instance.Type;
                                if (!lastArgumentArrayTypeSymbol.ElementType.Equals(byteType) && !lastArgumentArrayTypeSymbol.ElementType.Equals(sByteType))
                                {
                                    return true;
                                }
                            }
                        }
                        return false;
                    }

                    bool CheckLengthPropertyOnByteOrSByteArrays(IPropertyReferenceOperation lastArgument)
                    {
                        if (lastArgument.Property.Equals(arrayLengthProperty))
                        {
                            return CheckArrayLengthLocalReference(sourceArgument, lastArgument) || CheckArrayLengthLocalReference(destinationArgument, lastArgument);
                        }

                        return false;
                    }

                    if (lastArgument.Value is IPropertyReferenceOperation lastArgumentValue && CheckLengthPropertyOnByteOrSByteArrays(lastArgumentValue))
                    {
                        context.ReportDiagnostic(lastArgument.Value.CreateDiagnostic(Rule));
                    }
                    else
                    {
                        if (lastArgument.Value is not ILocalReferenceOperation localReferenceOperation)
                        {
                            return;
                        }

                        SemanticModel semanticModel = lastArgument.SemanticModel;
                        CancellationToken cancellationToken = context.CancellationToken;

                        if (semanticModel is null)
                        {
                            return;
                        }

                        ILocalSymbol localArgumentDeclaration = localReferenceOperation.Local;
                        if (localArgumentDeclaration is null)
                        {
                            return;
                        }

                        SyntaxReference declaringSyntaxReference = localArgumentDeclaration.DeclaringSyntaxReferences.FirstOrDefault();
                        if (declaringSyntaxReference is null)
                        {
                            return;
                        }

                        if (semanticModel.GetOperationWalkingUpParentChain(declaringSyntaxReference.GetSyntax(cancellationToken), cancellationToken) is not IVariableDeclaratorOperation variableDeclaratorOperation)
                        {
                            return;
                        }

                        IVariableInitializerOperation variableInitializer = variableDeclaratorOperation.Initializer;

                        if (variableInitializer is not null && variableInitializer.Value is IPropertyReferenceOperation variableInitializerPropertyReference && CheckLengthPropertyOnByteOrSByteArrays(variableInitializerPropertyReference))
                        {
                            context.ReportDiagnostic(lastArgument.Value.CreateDiagnostic(Rule));
                        }
                        else if (variableDeclaratorOperation.Parent is IVariableDeclarationOperation variableDeclarationOperation && variableDeclarationOperation.Initializer is not null && variableDeclarationOperation.Initializer.Value is IPropertyReferenceOperation variableInitializerPropertyReferenceVB && CheckLengthPropertyOnByteOrSByteArrays(variableInitializerPropertyReferenceVB))
                        {
                            context.ReportDiagnostic(lastArgument.Value.CreateDiagnostic(Rule));
                        }
                    }
                },
                OperationKind.Invocation);
            });
        }
    }
}

