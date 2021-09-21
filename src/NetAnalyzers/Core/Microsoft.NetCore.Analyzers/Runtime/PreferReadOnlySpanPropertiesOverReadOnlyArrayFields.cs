// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    //  Visual Basic doesn't allow ref struct APIs
#pragma warning disable RS1004 // Recommend adding language support to diagnostic analyzer
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1004 // Recommend adding language support to diagnostic analyzer
    public sealed class PreferReadOnlySpanPropertiesOverReadOnlyArrayFields: DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1850";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            Resx.CreateLocalizableResourceString(nameof(Resx.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_Title)),
            Resx.CreateLocalizableResourceString(nameof(Resx.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_Message)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            Resx.CreateLocalizableResourceString(nameof(Resx.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_Description)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            //  Bail if we don't have all the required symbols.
            if (!Cache.TryCreateCache(context.Compilation, out var cache))
                return;
            var fieldReferenceWalker = new FieldReferenceWalker(cache);

            context.RegisterSymbolAction(context =>
            {
                var field = (IFieldSymbol)context.Symbol;

                if (field.IsStatic && field.IsReadOnly && field.Type is IArrayTypeSymbol arrayType && cache.IsSupportedArrayElementType(arrayType.ElementType))
                {
                    cache.Candidates.Add(field);
                }
            }, SymbolKind.Field);

            context.RegisterOperationAction(AnalyzeFieldReferenceOperation, OperationKind.FieldReference);
            context.RegisterOperationAction(AnalyzeFieldInitializerOperation, OperationKind.FieldInitializer);

            context.RegisterCompilationEndAction(context =>
            {
                foreach (var candidate in cache.Candidates)
                {
                    context.ReportDiagnostic(candidate.CreateDiagnostic(Rule));
                }
                cache.Dispose();
            });

            return;

            //  Local functions

            void AnalyzeFieldReferenceOperation(OperationAnalysisContext context)
            {
                var fieldReference = (IFieldReferenceOperation)context.Operation;
                if (cache.Candidates.Contains(fieldReference.Field))
                    fieldReference.Parent.Accept(fieldReferenceWalker);
            }

            void AnalyzeFieldInitializerOperation(OperationAnalysisContext context)
            {
                //  Eliminate candidates that are not initialized with an array initializer
                //  containing all constant elements.
                var fieldInitializer = (IFieldInitializerOperation)context.Operation;
                if (fieldInitializer.Value is not IArrayCreationOperation arrayCreation ||
                    (fieldInitializer.InitializedFields.Any(x => cache.Candidates.Contains(x)) &&
                    arrayCreation.Initializer.ElementValues.Any(x => !x.ConstantValue.HasValue)))
                {
                    foreach (var field in fieldInitializer.InitializedFields)
                        cache.Candidates.Remove(field);
                }
            }
        }

        private sealed class FieldReferenceWalker : OperationVisitor
        {
            private readonly Cache _cache;
            private readonly ArrayElementWalker _elementWalker;

            public FieldReferenceWalker(Cache cache)
            {
                _cache = cache;
                _elementWalker = new ArrayElementWalker(cache);
            }

            public override void VisitArgument(IArgumentOperation operation)
            {
                if (operation.Parent is IInvocationOperation invocation && !_cache.IsAsSpanMethod(invocation.TargetMethod))
                    _cache.Candidates.Remove(GetField(operation.Value));

                base.VisitArgument(operation);
            }

            public override void VisitConversion(IConversionOperation operation)
            {
                if (!operation.Type.OriginalDefinition.Equals(_cache.ReadOnlySpanType, SymbolEqualityComparer.Default))
                    _cache.Candidates.Remove(GetField(operation.Operand));

                base.VisitConversion(operation);
            }

            public override void VisitArrayElementReference(IArrayElementReferenceOperation operation)
            {
                //  Eliminate candidates if any elements are assigned to ref locals, parameters, or returns.
                if (operation.GetValueUsageInfo(operation.SemanticModel.GetEnclosingSymbol(operation.Syntax.SpanStart)) is
                    ValueUsageInfo.WritableReference or ValueUsageInfo.ReadableWritableReference)
                {
                    _cache.Candidates.Remove(GetField(operation.ArrayReference));
                }
                else
                {
                    operation.Parent.Accept(_elementWalker);
                }

                base.VisitArrayElementReference(operation);
            }

            public override void VisitPropertyReference(IPropertyReferenceOperation operation)
            {
                if (!operation.Property.OriginalDefinition.Equals(_cache.ArrayLengthProperty, SymbolEqualityComparer.Default))
                    _cache.Candidates.Remove(GetField(operation.Instance));

                base.VisitPropertyReference(operation);
            }

            public override void VisitInvocation(IInvocationOperation operation)
            {
                _cache.Candidates.Remove(GetField(operation.Instance));

                base.VisitInvocation(operation);
            }

            public override void VisitFieldInitializer(IFieldInitializerOperation operation)
            {
                if (operation.Value is IArrayCreationOperation arrayCreation)
                {
                    foreach (var element in arrayCreation.Initializer.ElementValues)
                    {
                        if (!element.ConstantValue.HasValue)
                        {
                            foreach (var field in operation.InitializedFields)
                                _cache.Candidates.Remove(field);
                            break;
                        }
                    }
                }
                base.VisitFieldInitializer(operation);
            }

            private static IFieldSymbol GetField(IOperation fieldReference) => ((IFieldReferenceOperation)fieldReference).Field;
            private static bool TryGetField(IOperation? fieldReference, [NotNullWhen(true)] out IFieldSymbol? field)
            {
                return (field = (fieldReference as IFieldReferenceOperation)?.Field) is not null;
            }
        }

        private sealed class ArrayElementWalker : OperationVisitor
        {
            private readonly Cache _cache;

            public ArrayElementWalker(Cache cache)
            {
                _cache = cache;
            }

            public override void VisitSimpleAssignment(ISimpleAssignmentOperation operation)
            {
                if (TryGetField(operation.Target, out var field))
                {
                    _cache.Candidates.Remove(field);
                }

                base.VisitSimpleAssignment(operation);
            }

            public override void VisitCompoundAssignment(ICompoundAssignmentOperation operation)
            {
                if (TryGetField(operation.Target, out var field))
                {
                    _cache.Candidates.Remove(field);
                }

                base.VisitCompoundAssignment(operation);
            }

            public override void VisitTuple(ITupleOperation operation)
            {
                if (operation.Parent is IDeconstructionAssignmentOperation)
                {
                    foreach (var element in operation.Elements)
                    {
                        if (TryGetField(element, out var field))
                            _cache.Candidates.Remove(field);
                    }
                }

                base.VisitTuple(operation);
            }

            public override void VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation)
            {
                _cache.Candidates.Remove(GetField(operation.Target));

                base.VisitIncrementOrDecrement(operation);
            }

            private static bool TryGetField(IOperation elementReference, [NotNullWhen(true)] out IFieldSymbol? field)
            {
                return (field = ((elementReference as IArrayElementReferenceOperation)?.ArrayReference as IFieldReferenceOperation)?.Field) is not null;
            }

            private static IFieldSymbol GetField(IOperation elementReference)
            {
                return ((IFieldReferenceOperation)((IArrayElementReferenceOperation)elementReference).ArrayReference).Field;
            }
        }

        private sealed class Cache : IDisposable
        {
            private readonly ImmutableHashSet<ITypeSymbol> _supportedArrayElementTypes;
            private readonly ImmutableHashSet<IMethodSymbol> _asSpanMethods;
            public INamedTypeSymbol ReadOnlySpanType { get; }
            public IPropertySymbol ArrayLengthProperty { get; }

            public PooledConcurrentSet<IFieldSymbol> Candidates { get; }

            private Cache(Compilation compilation, INamedTypeSymbol readOnlySpanType, IPropertySymbol arrayLengthProperty)
            {
                _supportedArrayElementTypes = GetSupportedArrayElementTypes(compilation);
                ReadOnlySpanType = readOnlySpanType;
                ArrayLengthProperty = arrayLengthProperty;
                Candidates = PooledConcurrentSet<IFieldSymbol>.GetInstance(SymbolEqualityComparer.Default);
                _asSpanMethods = GetAsSpanMethods(compilation, readOnlySpanType);
                return;

                //  Local functions

                static ImmutableHashSet<ITypeSymbol> GetSupportedArrayElementTypes(Compilation compilation)
                {
                    var builder = ImmutableHashSet.CreateBuilder<ITypeSymbol>(SymbolEqualityComparer.Default);

                    builder.AddIfNotNull(compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBoolean));
                    builder.AddIfNotNull(compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemByte));
                    builder.AddIfNotNull(compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSByte));

                    return builder.ToImmutable();
                }

                static ImmutableHashSet<IMethodSymbol> GetAsSpanMethods(Compilation compilation, ITypeSymbol readOnlySpanType)
                {
                    if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemMemoryExtensions, out var memoryExtensionsType))
                        return ImmutableHashSet<IMethodSymbol>.Empty;

                    var spanType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSpan1);
                    var builder = ImmutableHashSet.CreateBuilder<IMethodSymbol>(SymbolEqualityComparer.Default);
                    var asSpanMethods = memoryExtensionsType.GetMembers(nameof(MemoryExtensions.AsSpan)).OfType<IMethodSymbol>()
                        .Where(x =>
                        {
                            return x.IsPublic() &&
                                (x.ReturnType.OriginalDefinition.Equals(readOnlySpanType, SymbolEqualityComparer.Default) ||
                                x.ReturnType.OriginalDefinition.Equals(spanType, SymbolEqualityComparer.Default));
                        });
                    builder.AddRange(asSpanMethods);

                    return builder.ToImmutable();
                }
            }

            public static bool TryCreateCache(Compilation compilation, [NotNullWhen(true)] out Cache? cache)
            {
                var arrayLengthProperty = compilation.GetSpecialType(SpecialType.System_Array).GetMembers(nameof(Array.Length)).OfType<IPropertySymbol>().FirstOrDefault();
                if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1, out var rosType) &&
                    arrayLengthProperty is not null)
                {
                    cache = new Cache(compilation, rosType, arrayLengthProperty);
                    return true;
                }

                cache = null;
                return false;
            }

            public bool IsSupportedArrayElementType(ITypeSymbol type) => _supportedArrayElementTypes.Contains(type);

            public bool IsAsSpanMethod(IMethodSymbol method) => _asSpanMethods.Contains(method.OriginalDefinition);

            public void Dispose() => Candidates.Dispose();
        }
    }
}
