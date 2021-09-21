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
            if (!Cache.TryCreateCache(context.Compilation, out var cache))
                return;
            var fieldReferenceVisitor = new FieldReferenceVisitor(cache);

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
            context.RegisterOperationAction(AnalyzeOperation, OperationKind.FieldReference, OperationKind.FieldInitializer);
            context.RegisterCompilationEndAction(OnCompilationEnd);

            return;

            //  Local functions

            void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                RoslynDebug.Assert(cache is not null, $"'{nameof(cache)}' was null.");

                var field = (IFieldSymbol)context.Symbol;
                if (field.IsStatic && field.IsReadOnly && field.Type is IArrayTypeSymbol arrayType && cache.IsSupportedArrayElementType(arrayType.ElementType))
                {
                    cache.Candidates.Add(field);
                }
            }

            void AnalyzeOperation(OperationAnalysisContext context)
            {
                RoslynDebug.Assert(cache is not null, $"'{nameof(cache)}' was null.");
                RoslynDebug.Assert(fieldReferenceVisitor is not null, $"'{nameof(fieldReferenceVisitor)}' was null.");

                switch (context.Operation)
                {
                    case IFieldReferenceOperation fieldReference:
                        fieldReference.Parent.Accept(fieldReferenceVisitor, fieldReference.Field);
                        break;
                    case IFieldInitializerOperation fieldInitializer:
                        if (fieldInitializer.Value is not IArrayCreationOperation arrayCreation ||
                            (fieldInitializer.InitializedFields.Any(x => cache.Candidates.Contains(x)) &&
                            arrayCreation.Initializer.ElementValues.Any(x => !x.ConstantValue.HasValue)))
                        {
                            foreach (var field in fieldInitializer.InitializedFields)
                                cache.Candidates.Remove(field);
                        }
                        break;
                }
            }

            void OnCompilationEnd(CompilationAnalysisContext context)
            {
                foreach (var field in cache.Candidates)
                {
                    context.ReportDiagnostic(field.CreateDiagnostic(Rule));
                }
                cache.Dispose();
            }
        }

        //  Visits the parent of an IFieldReferenceOperation
        private sealed class FieldReferenceVisitor : OperationVisitor<IFieldSymbol, Unit>
        {
            private readonly Cache _cache;
            private readonly ArrayElementVisitor _arrayElementVisitor;

            public FieldReferenceVisitor(Cache cache)
            {
                _cache = cache;
                _arrayElementVisitor = new ArrayElementVisitor(cache);
            }

            public override Unit VisitInvocation(IInvocationOperation operation, IFieldSymbol argument)
            {
                _cache.Candidates.Remove(argument);

                return base.VisitInvocation(operation, argument);
            }

            public override Unit VisitArgument(IArgumentOperation operation, IFieldSymbol argument)
            {
                if (operation.Parent is IInvocationOperation invocation && !_cache.IsAsSpanMethod(invocation.TargetMethod))
                    _cache.Candidates.Remove(argument);

                return base.VisitArgument(operation, argument);
            }

            public override Unit VisitPropertyReference(IPropertyReferenceOperation operation, IFieldSymbol argument)
            {
                if (!operation.Property.Equals(_cache.ArrayLengthProperty, SymbolEqualityComparer.Default))
                    _cache.Candidates.Remove(argument);

                return base.VisitPropertyReference(operation, argument);
            }

            public override Unit VisitConversion(IConversionOperation operation, IFieldSymbol argument)
            {
                if (!operation.Type.OriginalDefinition.Equals(_cache.ReadOnlySpanType, SymbolEqualityComparer.Default))
                    _cache.Candidates.Remove(argument);

                return base.VisitConversion(operation, argument);
            }

            public override Unit VisitArrayElementReference(IArrayElementReferenceOperation operation, IFieldSymbol argument)
            {
                //  Remove candidates who's elements were bound to ref or out variables.
                if (operation.GetValueUsageInfo(operation.SemanticModel.GetEnclosingSymbol(operation.Syntax.SpanStart)) is
                    ValueUsageInfo.ReadableWritableReference or ValueUsageInfo.WritableReference)
                {
                    _cache.Candidates.Remove(argument);
                }
                else
                {
                    operation.Parent.Accept(_arrayElementVisitor, new ArrayElementVisitContext(operation, argument));
                }

                return base.VisitArrayElementReference(operation, argument);
            }
        }

        //  Property bag.
#pragma warning disable CA1815 // Override equals and operator equals on value types
        private readonly struct ArrayElementVisitContext
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            public ArrayElementVisitContext(IArrayElementReferenceOperation operation, IFieldSymbol field)
            {
                Operation = operation;
                Field = field;
            }

            public IArrayElementReferenceOperation Operation { get; }
            public IFieldSymbol Field { get; }
        }

        //  Visits the parent of an IArrayElementReferenceOperation
        private sealed class ArrayElementVisitor : OperationVisitor<ArrayElementVisitContext, Unit>
        {
            private readonly Cache _cache;

            public ArrayElementVisitor(Cache cache)
            {
                _cache = cache;
            }

            public override Unit VisitSimpleAssignment(ISimpleAssignmentOperation operation, ArrayElementVisitContext argument)
            {
                if (operation.Target.Equals(argument.Operation))
                    _cache.Candidates.Remove(argument.Field);

                return base.VisitSimpleAssignment(operation, argument);
            }

            public override Unit VisitCompoundAssignment(ICompoundAssignmentOperation operation, ArrayElementVisitContext argument)
            {
                if (operation.Target.Equals(argument.Operation))
                    _cache.Candidates.Remove(argument.Field);

                return base.VisitCompoundAssignment(operation, argument);
            }

            public override Unit VisitTuple(ITupleOperation operation, ArrayElementVisitContext argument)
            {
                if (operation.Parent is IDeconstructionAssignmentOperation &&
                    operation.Elements.Any(x => x.Equals(argument.Operation)))
                {
                    _cache.Candidates.Remove(argument.Field);
                }

                return base.VisitTuple(operation, argument);
            }

            public override Unit VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, ArrayElementVisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);

                return base.VisitIncrementOrDecrement(operation, argument);
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
