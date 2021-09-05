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
    using static MicrosoftNetCoreAnalyzersResources;
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

        internal static readonly DiagnosticDescriptor StringRule = DiagnosticDescriptorHelper.Create(
            CA1849,
            CreateLocalizableResourceString(nameof(PreferHashDataOverComputeHashAnalyzerTitle)),
            CreateLocalizableResourceString(nameof(PreferHashDataOverComputeHashAnalyzerMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            description: CreateLocalizableResourceString(nameof(PreferHashDataOverComputeHashAnalyzerDescription)),
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
            var methodHelper = MethodHelper.Init(context.Compilation);
            if (methodHelper is null)
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

                var dataCollector = new DataCollector();

                context.RegisterOperationAction(CaptureHashLocalReferenceOperation, OperationKind.LocalReference);
                context.RegisterOperationAction(CaptureCreateOrComputeHashInvocationOperation, OperationKind.Invocation);
                context.RegisterOperationAction(CaptureHashObjectCreationOperation, OperationKind.ObjectCreation);
                context.RegisterOperationBlockEndAction(OnOperationBlockEnd);
                return;

                void CaptureHashLocalReferenceOperation(OperationAnalysisContext context)
                {
                    var localReferenceOperation = (ILocalReferenceOperation)context.Operation;
                    if (methodHelper.IsLocalReferenceInheritingHashAlgorithm(localReferenceOperation))
                    {
                        dataCollector.CollectLocalReferenceInheritingHashAlgorithm(localReferenceOperation);
                    }
                }

                void CaptureCreateOrComputeHashInvocationOperation(OperationAnalysisContext context)
                {
                    var invocationOperation = (IInvocationOperation)context.Operation;
                    if (methodHelper.IsHashCreateMethod(invocationOperation))
                    {
                        CaptureHashCreateInvocationOperation(dataCollector, invocationOperation);
                    }
                    else if (methodHelper.IsComputeHashMethod(invocationOperation))
                    {
                        CaptureOrReportComputeHashInvocationOperation(context, methodHelper, dataCollector, invocationOperation);
                    }
                    else if (methodHelper.IsComputeHashSectionMethod(invocationOperation))
                    {
                        CaptureOrReportComputeHashSectionInvocationOperation(context, methodHelper, dataCollector, invocationOperation);
                    }
                    else if (methodHelper.IsTryComputeHashMethod(invocationOperation))
                    {
                        CaptureOrReportTryComputeHashInvocationOperation(context, methodHelper, dataCollector, invocationOperation);
                    }
                    else if (invocationOperation.Instance is ILocalReferenceOperation && methodHelper.IsDisposeMethod(invocationOperation))
                    {
                        dataCollector.CollectDisposeInvocation(invocationOperation);
                    }
                }

                void CaptureHashObjectCreationOperation(OperationAnalysisContext context)
                {
                    var objectCreationOperation = (IObjectCreationOperation)context.Operation;
                    if (!methodHelper.IsObjectCreationInheritingHashAlgorithm(objectCreationOperation))
                    {
                        return;
                    }
                    if (TryGetVariableInitializerOperation(objectCreationOperation.Parent, out var variableInitializerOperation))
                    {
                        CaptureVariableDeclaratorOperation(dataCollector, objectCreationOperation.Type, variableInitializerOperation);
                    }
                }

                void OnOperationBlockEnd(OperationBlockAnalysisContext context)
                {
                    var cancellationToken = context.CancellationToken;
                    var (disposeMap, computeHashOnlySymbolMap) = dataCollector.Compile(cancellationToken);

                    foreach (var (computeHash, type) in dataCollector.ComputeHashMap)
                    {
                        ImmutableArray<Location> codefixerLocations;
                        var localSymbol = ((ILocalReferenceOperation)computeHash.Instance).Local;
                        var isToDeleteHashCreation = false;
                        int hashCreationLocationIndex;

                        if (dataCollector.TryGetDeclarationTuple(localSymbol, out var declarationTuple) &&
                            computeHashOnlySymbolMap.TryGetValue(localSymbol, out var refCount) &&
                            methodHelper.TryGetHashDataMethod(declarationTuple.OriginalType, type, out var hashDataMethodSymbol))
                        {
                            var disposeArray = GetValueOrEmtpty(disposeMap, localSymbol);
                            isToDeleteHashCreation = refCount == 1;
                            codefixerLocations = GetFixerLocations(declarationTuple.DeclaratorOperation, computeHash, type, disposeArray, out hashCreationLocationIndex);
                        }
                        else
                        {
                            continue;
                        }

                        var diagnostics = CreateDiagnostics(computeHash, hashDataMethodSymbol.ContainingType, codefixerLocations, type, isToDeleteHashCreation, hashCreationLocationIndex);
                        context.ReportDiagnostic(diagnostics);
                    }

                    dataCollector.Free(cancellationToken);
                    disposeMap?.Free(cancellationToken);
                    computeHashOnlySymbolMap.Free(cancellationToken);
                }
            }

            static void CaptureHashCreateInvocationOperation(DataCollector dataCollector, IInvocationOperation hashCreateInvocation)
            {
                if (!TryGetVariableInitializerOperation(hashCreateInvocation.Parent, out var variableInitializerOperation))
                {
                    return;
                }

                var ownerType = hashCreateInvocation.TargetMethod.ContainingType;
                CaptureVariableDeclaratorOperation(dataCollector, ownerType, variableInitializerOperation);
            }

            static void CaptureVariableDeclaratorOperation(DataCollector dataCollector, ITypeSymbol createdType, IVariableInitializerOperation variableInitializerOperation)
            {
                switch (variableInitializerOperation.Parent)
                {
                    case IVariableDeclaratorOperation declaratorOperation:
                        dataCollector.CollectVariableDeclaratorOperation(declaratorOperation, createdType);
                        break;
                    case IVariableDeclarationOperation declarationOperation when declarationOperation.Declarators.Length == 1:
                        {
                            var declaratorOperationAlt = declarationOperation.Declarators[0];
                            dataCollector.CollectVariableDeclaratorOperation(declaratorOperationAlt, createdType);
                            break;
                        }
                }
            }

            static void CaptureOrReportComputeHashInvocationOperation(OperationAnalysisContext context, MethodHelper methodHelper, DataCollector dataCollector, IInvocationOperation computeHashInvocation)
            {
                switch (computeHashInvocation.Instance)
                {
                    case ILocalReferenceOperation:
                        dataCollector.CollectComputeHashInvocation(computeHashInvocation);
                        break;
                    case IInvocationOperation chainedInvocationOperation when methodHelper.IsHashCreateMethod(chainedInvocationOperation):
                        ReportChainedComputeHashInvocationOperation(chainedInvocationOperation.TargetMethod.ContainingType);
                        break;
                    case IObjectCreationOperation chainObjectCreationOperation when methodHelper.IsObjectCreationInheritingHashAlgorithm(chainObjectCreationOperation):
                        ReportChainedComputeHashInvocationOperation(chainObjectCreationOperation.Type);
                        break;
                }

                void ReportChainedComputeHashInvocationOperation(ITypeSymbol originalHashType)
                {
                    if (!methodHelper.TryGetHashDataMethodByteArg(originalHashType, out var staticHashMethod))
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

            static void CaptureOrReportComputeHashSectionInvocationOperation(OperationAnalysisContext context, MethodHelper methodHelper, DataCollector dataCollector, IInvocationOperation computeHashInvocation)
            {
                switch (computeHashInvocation.Instance)
                {
                    case ILocalReferenceOperation:
                        dataCollector.CollectComputeHashSectionInvocation(computeHashInvocation);
                        break;
                    case IInvocationOperation chainedInvocationOperation when methodHelper.IsHashCreateMethod(chainedInvocationOperation):
                        ReportChainedComputeHashSectionInvocationOperation(chainedInvocationOperation.TargetMethod.ContainingType);
                        break;
                    case IObjectCreationOperation chainObjectCreationOperation when methodHelper.IsObjectCreationInheritingHashAlgorithm(chainObjectCreationOperation):
                        ReportChainedComputeHashSectionInvocationOperation(chainObjectCreationOperation.Type);
                        break;
                }

                void ReportChainedComputeHashSectionInvocationOperation(ITypeSymbol originalHashType)
                {
                    if (!methodHelper.TryGetHashDataMethodSpanArg(originalHashType, out var staticHashMethod))
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

            static void CaptureOrReportTryComputeHashInvocationOperation(OperationAnalysisContext context, MethodHelper methodHelper, DataCollector dataCollector, IInvocationOperation computeHashInvocation)
            {
                switch (computeHashInvocation.Instance)
                {
                    case ILocalReferenceOperation:
                        dataCollector.CollectTryComputeHashInvocation(computeHashInvocation);
                        break;
                    case IInvocationOperation chainedInvocationOperation when methodHelper.IsHashCreateMethod(chainedInvocationOperation):
                        ReportChainedTryComputeHashInvocationOperation(chainedInvocationOperation.TargetMethod.ContainingType);
                        break;
                    case IObjectCreationOperation chainObjectCreationOperation when methodHelper.IsObjectCreationInheritingHashAlgorithm(chainObjectCreationOperation):
                        ReportChainedTryComputeHashInvocationOperation(chainObjectCreationOperation.Type);
                        break;
                }

                void ReportChainedTryComputeHashInvocationOperation(ITypeSymbol originalHashType)
                {
                    if (!methodHelper.TryGetTryHashDataMethod(originalHashType, out var staticHashMethod))
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

        private static ImmutableArray<IInvocationOperation> GetValueOrEmtpty(PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>>? dictionary, ILocalSymbol key)
        {
            if (dictionary is not null && dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            return ImmutableArray<IInvocationOperation>.Empty;
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

        private sealed class MethodHelper
        {
            private readonly INamedTypeSymbol _hashAlgorithmBaseType;
            private readonly IMethodSymbol _computeHashMethodSymbol;
            private readonly IMethodSymbol _disposeMethodSymbol;
            private readonly IMethodSymbol? _computeHashSectionMethodSymbol;
            private readonly IMethodSymbol? _tryComputeHashMethodSymbol;

            // for finding HashData methods
            private readonly ParameterInfo _byteArrayParameter;
            private readonly ParameterInfo _rosByteParameter;
            private readonly ParameterInfo _spanByteParameter;
            private readonly ParameterInfo _intParameter;

            private static readonly ImmutableHashSet<string> SpecialManagedHashAlgorithms = ImmutableHashSet.CreateRange(new[] {
                nameof(System.Security.Cryptography.SHA1Managed),
                nameof(System.Security.Cryptography.SHA256Managed),
                nameof(System.Security.Cryptography.SHA384Managed),
                nameof(System.Security.Cryptography.SHA512Managed),
                nameof(System.Security.Cryptography.MD5CryptoServiceProvider),
                nameof(System.Security.Cryptography.SHA1CryptoServiceProvider),
                nameof(System.Security.Cryptography.SHA256CryptoServiceProvider),
                nameof(System.Security.Cryptography.SHA384CryptoServiceProvider),
                nameof(System.Security.Cryptography.SHA512CryptoServiceProvider),
            });

            private MethodHelper(INamedTypeSymbol hashAlgorithmBaseType,
                IMethodSymbol computeHashMethodSymbol,
                IMethodSymbol disposeMethodSymbol,
                IMethodSymbol? computeHashSectionMethodSymbol,
                IMethodSymbol? tryComputeHashMethodSymbol,
                ParameterInfo byteArrayParameter,
                ParameterInfo rosByteParameter,
                ParameterInfo spanByteParameter,
                ParameterInfo intParameter)
            {
                _hashAlgorithmBaseType = hashAlgorithmBaseType;
                _computeHashMethodSymbol = computeHashMethodSymbol;
                _disposeMethodSymbol = disposeMethodSymbol;
                _computeHashSectionMethodSymbol = computeHashSectionMethodSymbol;
                _tryComputeHashMethodSymbol = tryComputeHashMethodSymbol;

                _byteArrayParameter = byteArrayParameter;
                _rosByteParameter = rosByteParameter;
                _spanByteParameter = spanByteParameter;
                _intParameter = intParameter;
            }

            public bool IsLocalReferenceInheritingHashAlgorithm(ILocalReferenceOperation localReferenceOperation) => localReferenceOperation.Local.Type.Inherits(_hashAlgorithmBaseType);

            public bool IsObjectCreationInheritingHashAlgorithm(IObjectCreationOperation objectCreationOperation) => objectCreationOperation.Type.Inherits(_hashAlgorithmBaseType);

            public bool IsHashCreateMethod(IInvocationOperation invocationOperation)
            {
                IMethodSymbol methodSymbol = invocationOperation.TargetMethod;
                return methodSymbol.ContainingType.Inherits(_hashAlgorithmBaseType) &&
                    methodSymbol.ReturnType.Inherits(_hashAlgorithmBaseType) &&
                    methodSymbol.Name.Equals(CreateMethodName, StringComparison.Ordinal);
            }

            public bool IsComputeHashMethod(IInvocationOperation invocationOperation) => invocationOperation.TargetMethod.Equals(_computeHashMethodSymbol, SymbolEqualityComparer.Default);

            public bool IsComputeHashSectionMethod(IInvocationOperation invocationOperation) => invocationOperation.TargetMethod.Equals(_computeHashSectionMethodSymbol, SymbolEqualityComparer.Default);

            public bool IsTryComputeHashMethod(IInvocationOperation invocationOperation) => invocationOperation.TargetMethod.Equals(_tryComputeHashMethodSymbol, SymbolEqualityComparer.Default);

            public bool IsDisposeMethod(IInvocationOperation invocationOperation) => invocationOperation.TargetMethod.Equals(_disposeMethodSymbol, SymbolEqualityComparer.Default);

            public bool TryGetHashDataMethod(ITypeSymbol originalHashType, ComputeType computeType, [NotNullWhen(true)] out IMethodSymbol? staticHashMethod)
            {
                staticHashMethod = null;
                return computeType switch
                {
                    ComputeType.ComputeHash => TryGetHashDataMethodByteArg(originalHashType, out staticHashMethod),
                    ComputeType.ComputeHashSection => TryGetHashDataMethodSpanArg(originalHashType, out staticHashMethod),
                    ComputeType.TryComputeHash => TryGetTryHashDataMethod(originalHashType, out staticHashMethod),
                    _ => false,
                };
            }

            public bool TryGetHashDataMethodByteArg(ITypeSymbol originalHashType, [NotNullWhen(true)] out IMethodSymbol? staticHashMethod) => TryGetHashDataMethodOneArg(originalHashType, _byteArrayParameter, out staticHashMethod);

            public bool TryGetHashDataMethodSpanArg(ITypeSymbol originalHashType, [NotNullWhen(true)] out IMethodSymbol? staticHashMethod) => TryGetHashDataMethodOneArg(originalHashType, _rosByteParameter, out staticHashMethod);

            public bool TryGetTryHashDataMethod(ITypeSymbol originalHashType, [NotNullWhen(true)] out IMethodSymbol? staticHashMethod) => TryGetTryHashDataMethod(originalHashType, _rosByteParameter, _spanByteParameter, _intParameter, out staticHashMethod);

            private static bool TryGetHashDataMethodOneArg(ITypeSymbol originalHashType, ParameterInfo argOne, [NotNullWhen(true)] out IMethodSymbol? staticHashMethod)
            {
                if (IsSpecialManagedHashAlgorithms(originalHashType))
                {
                    originalHashType = originalHashType.BaseType;
                }

                staticHashMethod = originalHashType.GetMembers(HashDataMethodName).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterInfos(argOne);

                return staticHashMethod is not null;
            }

            private static bool TryGetTryHashDataMethod(ITypeSymbol originalHashType,
                ParameterInfo sourceArgument,
                ParameterInfo destArgument,
                ParameterInfo intArgument,
                [NotNullWhen(true)] out IMethodSymbol? staticHashMethod)
            {
                if (IsSpecialManagedHashAlgorithms(originalHashType))
                {
                    originalHashType = originalHashType.BaseType;
                }

                staticHashMethod = originalHashType.GetMembers(TryHashDataMethodName).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterInfos(sourceArgument, destArgument, intArgument);

                return staticHashMethod is not null;
            }

            private static bool IsSpecialManagedHashAlgorithms(ITypeSymbol originalHashType)
            {
                if (!SpecialManagedHashAlgorithms.Contains(originalHashType.Name))
                {
                    return false;
                }

                return originalHashType.ContainingNamespace is
                {
                    Name: nameof(System.Security.Cryptography),
                    ContainingNamespace:
                    {
                        Name: nameof(System.Security),
                        ContainingNamespace: { Name: nameof(System) }
                    }
                };
            }

            public static MethodHelper? Init(Compilation compilation)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyHashAlgorithm, out var hashAlgoBaseType) ||
                    !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographySHA256, out var sha256Type) ||
                    !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1, out var rosType) ||
                    !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSpan1, out var spanType))
                {
                    return null;
                }

                var byteType = compilation.GetSpecialType(SpecialType.System_Byte);
                var intType = compilation.GetSpecialType(SpecialType.System_Int32);
                if (byteType.IsErrorType() || intType.IsErrorType())
                {
                    return null;
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
                    return null;
                }

                var disposeHashMethodBaseType = hashAlgoBaseType.GetMembers(DisposeMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos();
                if (disposeHashMethodBaseType is null)
                {
                    return null;
                }

                var computeHashMethodBaseType = hashAlgoBaseType.GetMembers(ComputeHashMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter);
                if (computeHashMethodBaseType is null)
                {
                    return null;
                }

                var computeHashSectionMethodBaseType = hashAlgoBaseType.GetMembers(ComputeHashMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(byteArrayParameter, intParameter, intParameter);
                var tryComputeHashMethodBaseType = hashAlgoBaseType.GetMembers(TryComputeHashMethodName).OfType<IMethodSymbol>().GetFirstOrDefaultMemberWithParameterInfos(rosByteParameter, spanByteParameter, intParameter);

                var methodHelper = new MethodHelper(
                    hashAlgoBaseType,
                    computeHashMethodBaseType,
                    disposeHashMethodBaseType,
                    computeHashSectionMethodBaseType,
                    tryComputeHashMethodBaseType,
                    byteArrayParameter,
                    rosByteParameter,
                    spanByteParameter,
                    intParameter);

                return methodHelper;
            }
        }

        private sealed class DataCollector
        {
            private readonly PooledConcurrentSet<IInvocationOperation> _disposeHashSet = PooledConcurrentSet<IInvocationOperation>.GetInstance();
            private readonly PooledConcurrentDictionary<ILocalSymbol, DeclarationTuple> _createdSymbolMap = PooledConcurrentDictionary<ILocalSymbol, DeclarationTuple>.GetInstance(SymbolEqualityComparer.Default);
            private readonly PooledConcurrentDictionary<ILocalReferenceOperation, ILocalSymbol> _localReferenceMap = PooledConcurrentDictionary<ILocalReferenceOperation, ILocalSymbol>.GetInstance();

            public PooledConcurrentDictionary<IInvocationOperation, ComputeType> ComputeHashMap { get; } = PooledConcurrentDictionary<IInvocationOperation, ComputeType>.GetInstance();

            public void CollectLocalReferenceInheritingHashAlgorithm(ILocalReferenceOperation localReferenceOperation) => _localReferenceMap.TryAdd(localReferenceOperation, localReferenceOperation.Local);

            public void CollectVariableDeclaratorOperation(IVariableDeclaratorOperation declaratorOperation, ITypeSymbol createdType) => _createdSymbolMap.TryAdd(declaratorOperation.Symbol, new DeclarationTuple(declaratorOperation, createdType));

            public void CollectComputeHashInvocation(IInvocationOperation computeHashInvocation) => ComputeHashMap.TryAdd(computeHashInvocation, ComputeType.ComputeHash);

            public void CollectComputeHashSectionInvocation(IInvocationOperation computeHashInvocation) => ComputeHashMap.TryAdd(computeHashInvocation, ComputeType.ComputeHashSection);

            public void CollectTryComputeHashInvocation(IInvocationOperation computeHashInvocation) => ComputeHashMap.TryAdd(computeHashInvocation, ComputeType.TryComputeHash);

            public void CollectDisposeInvocation(IInvocationOperation disposeInvocation) => _disposeHashSet.Add(disposeInvocation);

            public (PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>>? DisposeMap, PooledDictionary<ILocalSymbol, int> ComputeHashOnlyMap) Compile(CancellationToken cancellationToken)
            {
                var disposeMap = CompileDisposeMap(cancellationToken);
                var computeHashOnlyMap = GetComputeHashOnlySymbols(disposeMap, cancellationToken);
                return (disposeMap, computeHashOnlyMap);
            }

            private PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>>? CompileDisposeMap(CancellationToken cancellationToken)
            {
                if (_disposeHashSet.IsEmpty)
                {
                    return null;
                }

                var map = PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>>.GetInstance(SymbolEqualityComparer.Default);
                var workingMap = PooledDictionary<ILocalSymbol, ArrayBuilder<IInvocationOperation>>.GetInstance(SymbolEqualityComparer.Default);

                foreach (var dispose in _disposeHashSet)
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

            private PooledDictionary<ILocalSymbol, int> GetComputeHashOnlySymbols(PooledDictionary<ILocalSymbol, ImmutableArray<IInvocationOperation>>? disposeMap, CancellationToken cancellationToken)
            {
                var hashSet = PooledDictionary<ILocalSymbol, int>.GetInstance(SymbolEqualityComparer.Default);
                if (_localReferenceMap.IsEmpty || ComputeHashMap.IsEmpty)
                {
                    return hashSet;
                }

                // we find the symbol whose local ref count matches the count of computeHash invoked
                var localRefSymbolCountMap = PooledDictionary<ILocalSymbol, int>.GetInstance(SymbolEqualityComparer.Default);
                var computeHashSymbolCountMap = PooledDictionary<ILocalSymbol, int>.GetInstance(SymbolEqualityComparer.Default);

                foreach (var (_, local) in _localReferenceMap)
                {
                    if (!localRefSymbolCountMap.TryGetValue(local, out var count))
                    {
                        count = 0;
                    }
                    localRefSymbolCountMap[local] = count + 1;
                }

                foreach (var (computeHash, _) in ComputeHashMap)
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

            public bool TryGetDeclarationTuple(ILocalSymbol localSymbol, [NotNullWhen(true)] out DeclarationTuple declarationTuple) => _createdSymbolMap.TryGetValue(localSymbol, out declarationTuple);

            public void Free(CancellationToken cancellationToken)
            {
                ComputeHashMap.Free(cancellationToken);
                _disposeHashSet.Free(cancellationToken);
                _createdSymbolMap.Free(cancellationToken);
                _localReferenceMap.Free(cancellationToken);
            }
        }
    }
}
