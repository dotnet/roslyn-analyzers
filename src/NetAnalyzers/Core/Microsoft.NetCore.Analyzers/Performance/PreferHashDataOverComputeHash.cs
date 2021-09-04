// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
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
        internal const string CA1849 = nameof(CA1849);
        internal const string TargetHashTypeDiagnosticPropertyKey = nameof(TargetHashTypeDiagnosticPropertyKey);
        internal const string DeleteHashCreationPropertyKey = nameof(DeleteHashCreationPropertyKey);
        internal const string ComputeTypePropertyKey = nameof(ComputeTypePropertyKey);
        internal const string HashCreationIndexPropertyKey = nameof(HashCreationIndexPropertyKey);
        internal const string HashDataMethodName = "HashData";
        internal const string TryHashDataMethodName = "TryHashData";
        private const string ComputeHashMethodName = nameof(System.Security.Cryptography.HashAlgorithm.ComputeHash);
        private const string DisposeMethodName = nameof(System.Security.Cryptography.HashAlgorithm.Dispose);
        private const string TryComputeHashMethodName = "TryComputeHash";
        private const string CreateMethodName = nameof(System.Security.Cryptography.SHA256.Create);

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableStringDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor StringRule = DiagnosticDescriptorHelper.Create(
            CA1849,
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
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographySHA256, out var sha256Type) ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1, out var rosType) ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSpan1, out var spanType))
            {
                return;
            }

            var byteType = context.Compilation.GetSpecialType(SpecialType.System_Byte);
            var intType = context.Compilation.GetSpecialType(SpecialType.System_Int32);
            if (byteType.IsErrorType() || intType.IsErrorType())
            {
                return;
            }
            var rosByteType = rosType.Construct(byteType);
            var spanByteType = spanType.Construct(byteType);

            var byteArrayParameter = ParameterInfo.GetParameterInfo(byteType, isArray: true, arrayRank: 1);
            var rosByteParameter = ParameterInfo.GetParameterInfo(rosByteType);
            var spanByteParameter = ParameterInfo.GetParameterInfo(spanByteType);
            var intParameter = ParameterInfo.GetParameterInfo(intType);

            // method introduced in .NET 5.0
            var hashDataMethodType = sha256Type.GetMembers(HashDataMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter);
            if (hashDataMethodType is null)
            {
                return;
            }
            var disposeHashMethodBaseType = hashAlgoBaseType.GetMembers(DisposeMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos();
            if (disposeHashMethodBaseType is null)
            {
                return;
            }

            var computeHashMethodBaseType = hashAlgoBaseType.GetMembers(ComputeHashMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter);
            var computeHashSectionMethodBaseType = hashAlgoBaseType.GetMembers(ComputeHashMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter, intParameter, intParameter);
            var tryComputeHashMethodBaseType = hashAlgoBaseType.GetMembers(TryComputeHashMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(rosByteParameter, spanByteParameter, intParameter);
            if (computeHashMethodBaseType is null && computeHashSectionMethodBaseType is null && tryComputeHashMethodBaseType is null)
            {
                return;
            }

            context.RegisterOperationBlockStartAction(OnOperationBlockStart);
            return;

            void OnOperationBlockStart(OperationBlockStartAnalysisContext context)
            {
                // Patterns we are looking for:
                // Pattern #1                                      Pattern #2
                // var obj = #HashCreation#;           or          #HashCreation#.CompuateHash(buffer);  
                // ...
                // obj.CompuateHash(buffer);

                // The core search logic is split into 3 groups
                // 1. Scan all invocations:
                //  a. if it is a Hash Create static method, store its local symbol + declaration operation and type symbols
                //  b. if it is HashAlgorithm.ComputeHash /  HashAlgorithm.TryComputeHash
                //      1. if its instance is a local reference, store its local reference + invocation
                //      2. if its instance is the creation of a hash instance, we found pattern #2. Report diagnostic.
                //  c. if it is HashAlgorithm.Dispose, store the invocation operation
                // 2. Find all HashAlgorithm object creation (new) and store its local symbol + declaration operation and type symbols
                // 3. Find all HashAlgorithm local references and store them

                // At OperationBlockEnd:
                // 1. Compile a symbol to dispose-invocation-array map
                // 1. Create a set of local reference whose symbols appears only once excluding dispose invocation
                // 2. Iterate the invocation, only report the invocation
                //    a. hashAlgorithm type has a static HashData method
                //    b. hashAlgorithm instance was created in the block
                //    c. hashAlgorithm ref count is the same number as the number of computehash invoked

                // Reporting of Diagnostic
                // The main span reported is at the ComputeHash method
                //
                // Properties:
                //  if there is only 1 local reference of a symbol excluding dispose reference, DeleteHashCreationPropertyKey is set
                //
                // Additional locations:
                // ComputeHash:
                // Pattern #1                                      Pattern #2        
                // 1. buffer arg span                              1. buffer arg span
                // 2. span where the hash instance was created
                // 3-N. dispose invocations
                //
                // ComputeHash(a,b,c)/TryComputeHash
                // Pattern #1                                      Pattern #2        
                // 1. buffer arg span                              1. buffer arg span
                // 2. 2nd arg span                                 2. 2nd arg span
                // 3. 3rd arg span                                 3. 3rd arg span
                // 4. span where the hash instance was created
                // 5-N. dispose invocations

                var computeMap = PooledConcurrentDictionary<IInvocationOperation, ComputeType>.GetInstance();
                var disposeHashSet = PooledConcurrentSet<IInvocationOperation>.GetInstance();
                var createdSymbolMap = PooledConcurrentDictionary<ILocalSymbol, DeclarationTuple>.GetInstance(SymbolEqualityComparer.Default);
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
                    else if (invocationOperation.TargetMethod.Equals(computeHashSectionMethodBaseType, SymbolEqualityComparer.Default))
                    {
                        CaptureOrReportComputeHashSectionInvocationOperation(context, invocationOperation);
                    }
                    else if (invocationOperation.TargetMethod.Equals(tryComputeHashMethodBaseType, SymbolEqualityComparer.Default))
                    {
                        CaptureOrReportTryComputeHashInvocationOperation(context, invocationOperation);
                    }
                    else if (invocationOperation.TargetMethod.Equals(tryComputeHashMethodBaseType, SymbolEqualityComparer.Default))
                    {
                        CaptureOrReportTryComputeHashInvocationOperation(context, invocationOperation);
                    }
                    else if (invocationOperation.TargetMethod.Equals(disposeHashMethodBaseType, SymbolEqualityComparer.Default) &&
                        invocationOperation.Instance is ILocalReferenceOperation)
                    {
                        disposeHashSet.Add(invocationOperation);
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
                        case ILocalReferenceOperation:
                            computeMap.TryAdd(computeHashInvocation, ComputeType.ComputeHash);
                            break;
                        case IInvocationOperation chainedInvocationOperation when IsHashCreateMethod(chainedInvocationOperation.TargetMethod):
                            ReportChainedComputeHashInvocationOperation(chainedInvocationOperation.TargetMethod.ContainingType);
                            break;
                        case IObjectCreationOperation chainObjectCreationOperation when chainObjectCreationOperation.Type.Inherits(hashAlgoBaseType):
                            ReportChainedComputeHashInvocationOperation(chainObjectCreationOperation.Type);
                            break;
                    }

                    void ReportChainedComputeHashInvocationOperation(ITypeSymbol originalHashType)
                    {
                        if (!TryGetHashDataMethod(originalHashType, byteArrayParameter, out var staticHashMethod))
                        {
                            return;
                        }

                        var builder = ImmutableArray.CreateBuilder<Location>(1);
                        FillLocationForComputeHash(builder, computeHashInvocation);
                        var codefixerLocations = builder.MoveToImmutable();
                        var diagnostics = CreateDiagnostics(computeHashInvocation, staticHashMethod.ContainingType, codefixerLocations, ComputeType.ComputeHash);

                        context.ReportDiagnostic(diagnostics);
                    }
                }

                void CaptureOrReportComputeHashSectionInvocationOperation(OperationAnalysisContext context, IInvocationOperation computeHashInvocation)
                {
                    switch (computeHashInvocation.Instance)
                    {
                        case ILocalReferenceOperation:
                            computeMap.TryAdd(computeHashInvocation, ComputeType.ComputeHashSection);
                            break;
                        case IInvocationOperation chainedInvocationOperation when IsHashCreateMethod(chainedInvocationOperation.TargetMethod):
                            ReportChainedComputeHashSectionInvocationOperation(chainedInvocationOperation.TargetMethod.ContainingType);
                            break;
                        case IObjectCreationOperation chainObjectCreationOperation when chainObjectCreationOperation.Type.Inherits(hashAlgoBaseType):
                            ReportChainedComputeHashSectionInvocationOperation(chainObjectCreationOperation.Type);
                            break;
                    }

                    void ReportChainedComputeHashSectionInvocationOperation(ITypeSymbol originalHashType)
                    {
                        if (!TryGetHashDataMethod(originalHashType, rosByteParameter, out var staticHashMethod))
                        {
                            return;
                        }
                        var builder = ImmutableArray.CreateBuilder<Location>(3);
                        FillLocationForComputeHash3Args(builder, computeHashInvocation);
                        var codefixerLocations = builder.MoveToImmutable();
                        var diagnostics = CreateDiagnostics(computeHashInvocation, staticHashMethod.ContainingType, codefixerLocations, ComputeType.ComputeHashSection);

                        context.ReportDiagnostic(diagnostics);
                    }
                }

                void CaptureOrReportTryComputeHashInvocationOperation(OperationAnalysisContext context, IInvocationOperation computeHashInvocation)
                {
                    switch (computeHashInvocation.Instance)
                    {
                        case ILocalReferenceOperation:
                            computeMap.TryAdd(computeHashInvocation, ComputeType.TryComputeHash);
                            break;
                        case IInvocationOperation chainedInvocationOperation when IsHashCreateMethod(chainedInvocationOperation.TargetMethod):
                            ReportChainedTryComputeHashInvocationOperation(chainedInvocationOperation.TargetMethod.ContainingType);
                            break;
                        case IObjectCreationOperation chainObjectCreationOperation when chainObjectCreationOperation.Type.Inherits(hashAlgoBaseType):
                            ReportChainedTryComputeHashInvocationOperation(chainObjectCreationOperation.Type);
                            break;
                    }

                    void ReportChainedTryComputeHashInvocationOperation(ITypeSymbol originalHashType)
                    {
                        if (!TryGetTryHashDataMethod(originalHashType, rosByteParameter, spanByteParameter, intParameter, out var staticHashMethod))
                        {
                            return;
                        }

                        var builder = ImmutableArray.CreateBuilder<Location>(3);
                        FillLocationForComputeHash3Args(builder, computeHashInvocation);
                        var codefixerLocations = builder.MoveToImmutable();
                        var diagnostics = CreateDiagnostics(computeHashInvocation, staticHashMethod.ContainingType, codefixerLocations, ComputeType.TryComputeHash);

                        context.ReportDiagnostic(diagnostics);
                    }
                }

                void OnOperationBlockEnd(OperationBlockAnalysisContext context)
                {
                    var cancellationToken = context.CancellationToken;
                    var disposeMap = CompileDisposeMap(disposeHashSet, cancellationToken);
                    var computeHashOnlySymbolMap = GetComputeHashOnlySymbols(localReferenceMap, computeMap, disposeMap, cancellationToken);

                    foreach (var (computeHash, type) in computeMap)
                    {
                        ImmutableArray<Location> codefixerLocations;
                        var localSymbol = ((ILocalReferenceOperation)computeHash.Instance).Local;
                        var isToDeleteHashCreation = false;
                        int hashCreationLocationIndex;

                        if (createdSymbolMap.TryGetValue(localSymbol, out var declarationTuple) &&
                            computeHashOnlySymbolMap.TryGetValue(localSymbol, out var refCount) &&
                            TryGetHashDataMethod(declarationTuple.OriginalType, byteArrayParameter, out var staticHashMethod))
                        {
                            var disposeArray = GetValueOrEmtpty(disposeMap, localSymbol);
                            isToDeleteHashCreation = refCount == 1;
                            codefixerLocations = GetFixerLocations(declarationTuple.DeclaratorOperation, computeHash, type, disposeArray, out hashCreationLocationIndex);
                        }
                        else
                        {
                            continue;
                        }

                        var diagnostics = CreateDiagnostics(computeHash, staticHashMethod.ContainingType, codefixerLocations, type, isToDeleteHashCreation, hashCreationLocationIndex);
                        context.ReportDiagnostic(diagnostics);
                    }

                    computeMap.Free(cancellationToken);
                    disposeHashSet.Free(cancellationToken);
                    createdSymbolMap.Free(cancellationToken);
                    localReferenceMap.Free(cancellationToken);
                    computeHashOnlySymbolMap.Free(cancellationToken);
                }

                bool IsHashCreateMethod(IMethodSymbol methodSymbol)
                {
                    return methodSymbol.ContainingType.Inherits(hashAlgoBaseType) &&
                        methodSymbol.ReturnType.Inherits(hashAlgoBaseType) &&
                        methodSymbol.Name.Equals(CreateMethodName, StringComparison.Ordinal);
                }

                static PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>> CompileDisposeMap(PooledConcurrentSet<IInvocationOperation> disposeSet, CancellationToken cancellationToken)
                {
                    var map = PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>>.GetInstance(SymbolEqualityComparer.Default);
                    if(disposeSet.IsEmpty)
                    {
                        return map;
                    }

                    var workingMap = PooledDictionary<ILocalSymbol, ArrayBuilder<IInvocationOperation>>.GetInstance(SymbolEqualityComparer.Default);

                    foreach (var dispose in disposeSet)
                    {
                        var local = ((ILocalReferenceOperation)dispose.Instance).Local;
                        if (!workingMap.TryGetValue(local, out var arrayBuilder))
                        {
                            arrayBuilder = ArrayBuilder<IInvocationOperation>.GetInstance();
                            workingMap.Add(local, arrayBuilder);
                        }

                        arrayBuilder.Add(dispose);
                    }

                    foreach (var kvp in workingMap)
                    {
                        var local = kvp.Key;
                        var arrayBuilder = kvp.Value;
                        var disposeArray = arrayBuilder.ToImmutableAndFree();

                        map.Add(local, disposeArray);
                    }

                    workingMap.Free(cancellationToken);

                    return map;
                }

                static ImmutableArray<IInvocationOperation> GetValueOrEmtpty(PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>> dictionary, ILocalSymbol key)
                {
                    if(dictionary.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                    return ImmutableArray<IInvocationOperation>.Empty;
                }

                static PooledDictionary<ILocalSymbol, int> GetComputeHashOnlySymbols(
                    PooledConcurrentDictionary<ILocalReferenceOperation, ILocalSymbol> map,
                    PooledConcurrentDictionary<IInvocationOperation, ComputeType> computeHashMap,
                    PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>> disposeMap,
                    CancellationToken cancellationToken)
                {
                    // we find the symbol whose local ref count matches the count of computeHash invoked
                    var hashSet = PooledDictionary<ILocalSymbol, int>.GetInstance(SymbolEqualityComparer.Default);
                    if(map.IsEmpty || computeHashMap.IsEmpty)
                    {
                        return hashSet;
                    }

                    var localRefSymbolCountMap = PooledDictionary<ILocalSymbol, int>.GetInstance(SymbolEqualityComparer.Default);
                    var computeHashSymbolCountMap = PooledDictionary<ILocalSymbol, int>.GetInstance(SymbolEqualityComparer.Default);

                    foreach (var (localRef, local) in map)
                    {
                        if (!localRefSymbolCountMap.TryGetValue(local, out var count))
                        {
                            count = 0;
                        }
                        localRefSymbolCountMap[local] = count + 1;
                    }

                    foreach (var (computeHash, _) in computeHashMap)
                    {
                        var local = ((ILocalReferenceOperation)computeHash.Instance).Local;
                        if (!computeHashSymbolCountMap.TryGetValue(local, out var count))
                        {
                            count = 0;
                        }
                        computeHashSymbolCountMap[local] = count + 1;
                    }

                    foreach (var (local, refCount) in localRefSymbolCountMap)
                    {
                        if (!computeHashSymbolCountMap.TryGetValue(local, out var computeHashCount))
                        {
                            continue;
                        }
                        var disposeArray = GetValueOrEmtpty(disposeMap, local);

                        var refCountWithoutDispose = refCount - disposeArray.Length;
                        if (refCountWithoutDispose == computeHashCount)
                        {
                            hashSet.Add(local, refCountWithoutDispose);
                        }
                    }

                    localRefSymbolCountMap.Free(cancellationToken);
                    computeHashSymbolCountMap.Free(cancellationToken);

                    return hashSet;
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

        private static bool TryGetHashDataMethod(ITypeSymbol originalHashType, ParameterInfo firstArgument, [NotNullWhen(true)] out IMethodSymbol? staticHashMethod)
        {
            var currInstanceType = originalHashType;
            do
            {
                staticHashMethod = currInstanceType.GetMembers(HashDataMethodName).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterInfos(firstArgument);

                if (staticHashMethod is not null)
                {
                    return true;
                }

                currInstanceType = currInstanceType.BaseType;
            } while (currInstanceType is { });
            return false;
        }

        private static bool TryGetTryHashDataMethod(ITypeSymbol originalHashType,
            ParameterInfo sourceArgument,
            ParameterInfo destArgument,
            ParameterInfo intArgument,
            [NotNullWhen(true)] out IMethodSymbol? staticHashMethod)
        {
            var currInstanceType = originalHashType;
            do
            {
                staticHashMethod = currInstanceType.GetMembers(TryHashDataMethodName).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterInfos(sourceArgument, destArgument, intArgument);

                if (staticHashMethod is not null)
                {
                    return true;
                }

                currInstanceType = currInstanceType.BaseType;
            } while (currInstanceType is { });
            return false;
        }

        private static Diagnostic CreateDiagnostics(IInvocationOperation computeHashMethod,
            INamedTypeSymbol staticHashMethodType,
            ImmutableArray<Location> fixerLocations,
            ComputeType computeType)
        {
            var dictBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
            dictBuilder.Add(TargetHashTypeDiagnosticPropertyKey, staticHashMethodType.Name);
            dictBuilder.Add(ComputeTypePropertyKey, computeType.ToString());

            return computeHashMethod.CreateDiagnostic(StringRule,
                fixerLocations,
                dictBuilder.ToImmutable(),
                staticHashMethodType.ToDisplayString());
        }

        private static Diagnostic CreateDiagnostics(IInvocationOperation computeHashMethod,
            INamedTypeSymbol staticHashMethodType,
            ImmutableArray<Location> fixerLocations,
            ComputeType computeType,
            bool isSingleLocalRef,
            int hashCreationLocationIndex)
        {
            var dictBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
            dictBuilder.Add(TargetHashTypeDiagnosticPropertyKey, staticHashMethodType.Name);
            dictBuilder.Add(ComputeTypePropertyKey, computeType.ToString());
            dictBuilder.Add(HashCreationIndexPropertyKey, hashCreationLocationIndex.ToString(CultureInfo.InvariantCulture));
            if (isSingleLocalRef)
            {
                dictBuilder.Add(DeleteHashCreationPropertyKey, DeleteHashCreationPropertyKey);
            }

            return computeHashMethod.CreateDiagnostic(StringRule,
                fixerLocations,
                dictBuilder.ToImmutable(),
                staticHashMethodType.ToDisplayString());
        }

        private static ImmutableArray<Location> GetFixerLocations(
            IVariableDeclaratorOperation declaratorOperation,
            IInvocationOperation computeHashInvocationOperation,
            ComputeType computeType,
            ImmutableArray<IInvocationOperation> disposeArray,
            out int hashCreationLocationIndex)
        {
            ImmutableArray<Location>.Builder builder;
            if (computeType is ComputeType.ComputeHash)
            {
                builder = ImmutableArray.CreateBuilder<Location>(2 + disposeArray.Length);
                FillLocationForComputeHash(builder, computeHashInvocationOperation);
            }
            else
            {
                builder = ImmutableArray.CreateBuilder<Location>(4 + disposeArray.Length);
                FillLocationForComputeHash3Args(builder, computeHashInvocationOperation);
            }
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
            hashCreationLocationIndex = builder.Count;
            builder.Add(nodeToRemove);


            foreach (var disposeInvocation in disposeArray)
            {
                var disposeLocation = disposeInvocation.Syntax.Parent.GetLocation();
                builder.Add(disposeLocation);
            }

            return builder.MoveToImmutable();
        }

        private static void FillLocationForComputeHash(ImmutableArray<Location>.Builder builder, IInvocationOperation computeHashInvocationOperation)
        {
            builder.Add(computeHashInvocationOperation.Arguments[0].Syntax.GetLocation());
        }

        private static void FillLocationForComputeHash3Args(ImmutableArray<Location>.Builder builder, IInvocationOperation computeHashInvocationOperation)
        {
            builder.Add(computeHashInvocationOperation.Arguments[0].Syntax.GetLocation());
            builder.Add(computeHashInvocationOperation.Arguments[1].Syntax.GetLocation());
            builder.Add(computeHashInvocationOperation.Arguments[2].Syntax.GetLocation());
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

        public enum ComputeType
        {
            ComputeHash,
            ComputeHashSection,
            TryComputeHash
        }
    }
}
