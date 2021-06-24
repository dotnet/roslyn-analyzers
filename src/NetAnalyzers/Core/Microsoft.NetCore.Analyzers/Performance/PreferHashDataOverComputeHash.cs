// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferHashDataOverComputeHashAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA1848 = nameof(CA1848);
        internal const string TargetHashTypeDiagnosticPropertyKey = nameof(TargetHashTypeDiagnosticPropertyKey);
        internal const string HashDataMethodName = "HashData";
        private const string ComputeHashMethodName = nameof(System.Security.Cryptography.HashAlgorithm.ComputeHash);
        private const string CreateMethodName = nameof(System.Security.Cryptography.SHA256.Create);

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableStringDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor StringRule = DiagnosticDescriptorHelper.Create(
            CA1848,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            description: s_localizableStringDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(StringRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;

            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyHashAlgorithm, out var hashAlgoBaseType) ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographySHA256, out var sha256Type))
            {
                return;
            }

            var byteType = context.Compilation.GetSpecialType(SpecialType.System_Byte);
            if (byteType is null)
            {
                return;
            }

            var byteArrayParameter = ParameterInfo.GetParameterInfo(byteType, isArray: true, arrayRank: 1);

            // method introduced in .NET 5.0
            var hashDataMethodType = sha256Type.GetMembers(HashDataMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter);
            if (hashDataMethodType is null)
            {
                return;
            }

            var computeHashMethodBaseType = hashAlgoBaseType.GetMembers(ComputeHashMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter);
            if (computeHashMethodBaseType is null)
            {
                return;
            }

            context.RegisterOperationBlockStartAction(OnOperationBlockStart);
            return;

            void OnOperationBlockStart(OperationBlockStartAnalysisContext context)
            {
                // Patterns we are looking for:
                // Pattern #1                                               Pattern #2
                // var obj = #HashCreation#;              or                #HashCreation#.CompuateHash(buffer);
                // ...
                // obj.CompuateHash(buffer); //x1

                // The core search logic is split into 3 groups
                // 1. Scan all invocations:
                //  a. if it is a Hash Create static method, store its local symbol + declaration operation and type symbols
                //  b. if it is HashAlgorithm.ComputeHash
                //      1. if its instance is a local reference, store its local reference + invocation
                //      2. if its instance is the creation of a hash instance, we found pattern #2. Report diagnostic.
                // 2. Find all HashAlgorithm object creation (new) and store its local symbol + declaration operation and type symbols
                // 3. Find all HashAlgorithm local references and store them

                // At OperationBlockEnd:
                // 1. Count all local references and create a set with only symbols that have a single local reference
                // 2. Iterate all ComputeHash invocation, only report (pattern #1) the invocation whose
                //  a. local reference appears once (exist in the set)
                //  b. local reference was created in the block
                //  c. hashAlgorithm type has a static HashData method

                // Reporting of Diagnostic
                // The main span reported is at the ComputeHash method
                //
                // Additional locations:
                // Pattern #1                                                Pattern #2
                // 1. buffer arg span                                        1. buffer arg span
                // 2. span where the hash instance was created

                var computeHashVariableMap = PooledConcurrentDictionary<ILocalReferenceOperation, IInvocationOperation>.GetInstance();
                var createdSymbolMap = PooledConcurrentDictionary<ILocalSymbol, DeclarationTuple>.GetInstance();
                var localReferenceMap = PooledConcurrentDictionary<ILocalReferenceOperation, ILocalSymbol>.GetInstance();

                context.RegisterOperationAction(CaptureHashLocalReferenceOperation, OperationKind.LocalReference);
                context.RegisterOperationAction(CaptureCreateOrComputeHashInvocationOperation, OperationKind.Invocation);
                context.RegisterOperationAction(CaptureHashObjectCreationOperation, OperationKind.ObjectCreation);
                context.RegisterOperationBlockEndAction(OnOperationBlockEnd);
                return;

                void CaptureHashLocalReferenceOperation(OperationAnalysisContext context)
                {
                    var localReferenceOperation = (ILocalReferenceOperation)context.Operation;
                    if (localReferenceOperation.Local.Type.Inherits(hashAlgoBaseType))
                    {
                        localReferenceMap.TryAdd(localReferenceOperation, localReferenceOperation.Local);
                    }
                }

                void CaptureCreateOrComputeHashInvocationOperation(OperationAnalysisContext context)
                {
                    var invocationOperation = (IInvocationOperation)context.Operation;
                    if (IsHashCreateMethod(invocationOperation.TargetMethod))
                    {
                        CaptureHashCreateInvocationOperation(invocationOperation);
                    }
                    else if (invocationOperation.TargetMethod.Equals(computeHashMethodBaseType, SymbolEqualityComparer.Default))
                    {
                        CaptureOrReportComputeHashInvocationOperation(context, invocationOperation);
                    }
                }

                void CaptureHashObjectCreationOperation(OperationAnalysisContext context)
                {
                    var objectCreationOperation = (IObjectCreationOperation)context.Operation;
                    if (!objectCreationOperation.Type.Inherits(hashAlgoBaseType))
                    {
                        return;
                    }
                    if (TryGetVariableInitializerOperation(objectCreationOperation.Parent, out var variableInitializerOperation))
                    {
                        CaptureVariableDeclaratorOperation(objectCreationOperation.Type, variableInitializerOperation);
                    }
                }

                void CaptureHashCreateInvocationOperation(IInvocationOperation hashCreateInvocation)
                {
                    if (!TryGetVariableInitializerOperation(hashCreateInvocation.Parent, out var variableInitializerOperation))
                    {
                        return;
                    }

                    var ownerType = hashCreateInvocation.TargetMethod.ContainingType;
                    CaptureVariableDeclaratorOperation(ownerType, variableInitializerOperation);
                }

                void CaptureVariableDeclaratorOperation(ITypeSymbol createdType, IVariableInitializerOperation variableInitializerOperation)
                {
                    switch (variableInitializerOperation.Parent)
                    {
                        case IVariableDeclaratorOperation declaratorOperation:
                            createdSymbolMap.TryAdd(declaratorOperation.Symbol, new DeclarationTuple(declaratorOperation, createdType));
                            break;
                        case IVariableDeclarationOperation declarationOperation when declarationOperation.Declarators.Length == 1:
                            {
                                var declaratorOperationAlt = declarationOperation.Declarators[0];
                                createdSymbolMap.TryAdd(declaratorOperationAlt.Symbol, new DeclarationTuple(declaratorOperationAlt, createdType));
                                break;
                            }
                    }
                }

                void CaptureOrReportComputeHashInvocationOperation(OperationAnalysisContext context, IInvocationOperation computeHashInvocation)
                {
                    switch (computeHashInvocation.Instance)
                    {
                        case ILocalReferenceOperation localReferenceOperation:
                            computeHashVariableMap.TryAdd(localReferenceOperation, computeHashInvocation);
                            break;
                        case IInvocationOperation chainedInvocationOperation when IsHashCreateMethod(chainedInvocationOperation.TargetMethod):
                            ReportChainedComputeHashInvocationOperation(context, computeHashInvocation, chainedInvocationOperation.TargetMethod.ContainingType, byteArrayParameter);
                            break;
                        case IObjectCreationOperation chainObjectCreationOperation when chainObjectCreationOperation.Type.Inherits(hashAlgoBaseType):
                            ReportChainedComputeHashInvocationOperation(context, computeHashInvocation, chainObjectCreationOperation.Type, byteArrayParameter);
                            break;
                    }
                }

                void OnOperationBlockEnd(OperationBlockAnalysisContext context)
                {
                    var singleLocalRefElements = localReferenceMap
                        .GroupBy(local => local.Value)
                        .Where(local => local.HasExactly(1))
                        .Select(local => local.First().Key);

                    foreach (var localRef in singleLocalRefElements)
                    {
                        if (!createdSymbolMap.TryGetValue(localRef.Local, out var declarationTuple) ||
                            !computeHashVariableMap.TryGetValue(localRef, out var computeHashMethod) ||
                            !TryGetHashDataMethod(declarationTuple.OriginalType, byteArrayParameter, out var staticHashMethod))
                        {
                            continue;
                        }

                        var codefixerLocations = GetFixerLocations(declarationTuple.DeclaratorOperation, computeHashMethod);
                        var diagnostics = CreateDiagnostics(computeHashMethod, staticHashMethod.ContainingType, codefixerLocations);
                        context.ReportDiagnostic(diagnostics);
                    }

                    computeHashVariableMap.Free(context.CancellationToken);
                    createdSymbolMap.Free(context.CancellationToken);
                    localReferenceMap.Free(context.CancellationToken);
                }

                bool IsHashCreateMethod(IMethodSymbol methodSymbol)
                {
                    return methodSymbol.ContainingType.Inherits(hashAlgoBaseType) &&
                        methodSymbol.ReturnType.Inherits(hashAlgoBaseType) &&
                        methodSymbol.Name.Equals(CreateMethodName, StringComparison.Ordinal);
                }
            }
        }

        private static bool TryGetVariableInitializerOperation(IOperation symbol, [NotNullWhen(true)] out IVariableInitializerOperation? variableInitializerOperation)
        {
            switch (symbol)
            {
                case IVariableInitializerOperation op:
                    variableInitializerOperation = op;
                    return true;
                case IConversionOperation { Parent: IVariableInitializerOperation variableInitializerOperationAlt }:
                    variableInitializerOperation = variableInitializerOperationAlt;
                    return true;
                default:
                    variableInitializerOperation = null;
                    return false;
            };
        }

        private static void ReportChainedComputeHashInvocationOperation(OperationAnalysisContext context, IInvocationOperation computeHashMethod, ITypeSymbol originalHashType, ParameterInfo byteArrayParameter)
        {
            if (!TryGetHashDataMethod(originalHashType, byteArrayParameter, out var staticHashMethod))
            {
                return;
            }

            var codefixerLocations = GetChainedFixerLocations(computeHashMethod);
            var diagnostics = CreateDiagnostics(computeHashMethod, staticHashMethod.ContainingType, codefixerLocations);

            context.ReportDiagnostic(diagnostics);
        }

        private static bool TryGetHashDataMethod(ITypeSymbol originalHashType, ParameterInfo byteArrayParameter, [NotNullWhen(true)] out IMethodSymbol? staticHashMethod)
        {
            var currInstanceType = originalHashType;
            do
            {
                staticHashMethod = currInstanceType.GetMembers(HashDataMethodName).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter);

                if (staticHashMethod is not null)
                {
                    return true;
                }

                currInstanceType = currInstanceType.BaseType;
            } while (currInstanceType is { });
            return false;
        }

        private static Diagnostic CreateDiagnostics(IInvocationOperation computeHashMethod, INamedTypeSymbol staticHashMethodType, ImmutableArray<Location> fixerLocations)
        {
            var dictBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
            dictBuilder.Add(TargetHashTypeDiagnosticPropertyKey, staticHashMethodType.Name);

            return computeHashMethod.CreateDiagnostic(StringRule,
                fixerLocations,
                dictBuilder.ToImmutable(),
                staticHashMethodType.ToDisplayString());
        }

        private static ImmutableArray<Location> GetFixerLocations(IVariableDeclaratorOperation declaratorOperation, IInvocationOperation computeHashInvocationOperation)
        {
            var bufferArgLocation = computeHashInvocationOperation.Arguments[0].Value.Syntax.GetLocation();
            var lineDeclaration = declaratorOperation.GetAncestor<IVariableDeclarationGroupOperation>(OperationKind.VariableDeclarationGroup);

            Location nodeToRemove;
            if (lineDeclaration?.Declarations.Length == 1 && lineDeclaration.Declarations[0].Declarators.Length == 1)
            {
                nodeToRemove = lineDeclaration.Syntax.GetLocation();
            }
            else if (lineDeclaration?.Declarations.Length > 1 && lineDeclaration.Language.Equals("Visual Basic"))
            {
                nodeToRemove = declaratorOperation.Syntax.Parent.GetLocation();
            }
            else
            {
                nodeToRemove = declaratorOperation.Syntax.GetLocation();
            }

            return ImmutableArray.Create(bufferArgLocation, nodeToRemove);
        }

        private static ImmutableArray<Location> GetChainedFixerLocations(IInvocationOperation computeHashInvocationOperation)
        {
            var bufferArgLocation = computeHashInvocationOperation.Arguments[0].Value.Syntax.GetLocation();

            return ImmutableArray.Create(bufferArgLocation);
        }

#pragma warning disable CA1815 // Override equals and operator equals on value types
        private readonly struct DeclarationTuple
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            public IVariableDeclaratorOperation DeclaratorOperation { get; }

            public ITypeSymbol OriginalType { get; }

            public DeclarationTuple(IVariableDeclaratorOperation declaratorOperation, ITypeSymbol type)
            {
                DeclaratorOperation = declaratorOperation;
                OriginalType = type;
            }
        }
    }
}
