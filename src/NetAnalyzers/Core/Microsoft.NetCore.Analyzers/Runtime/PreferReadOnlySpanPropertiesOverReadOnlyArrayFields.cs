// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    //  Visual Basic doesn't allow ref struct APIs
#pragma warning disable RS1004 // Recommend adding language support to diagnostic analyzer
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1004 // Recommend adding language support to diagnostic analyzer
    public sealed class PreferReadOnlySpanPropertiesOverReadOnlyArrayFields : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1850";
        internal const string FixerDataPropertyName = nameof(FixerDataPropertyName);

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
            //  Bail if we're missing required symbols.
            if (!Cache.TryCreateCache(context.Compilation, out var cache))
                return;
            var fieldReferenceVisitor = new FieldReferenceVisitor(cache);

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
            context.RegisterOperationAction(AnalyzeOperation, OperationKind.FieldReference, OperationKind.FieldInitializer);
            context.RegisterCompilationEndAction(OnCompilationEnd);

            return;

            //  Local functions

            //  We start by finding all field symbols for static readonly fields that are
            //  arrays of an allowed element type.
            void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                RoslynDebug.Assert(cache is not null, $"'{nameof(cache)}' was null.");

                var field = (IFieldSymbol)context.Symbol;
                if (field.IsStatic && field.IsReadOnly && field.Type is IArrayTypeSymbol arrayType && cache.IsSupportedArrayElementType(arrayType.ElementType))
                {
                    cache.Candidates.Add(field);
                }
            }

            //  Candidate elimination round.
            void AnalyzeOperation(OperationAnalysisContext context)
            {
                RoslynDebug.Assert(cache is not null, $"'{nameof(cache)}' was null.");
                RoslynDebug.Assert(fieldReferenceVisitor is not null, $"'{nameof(fieldReferenceVisitor)}' was null.");

                switch (context.Operation)
                {
                    case IFieldReferenceOperation fieldReference:
                        if (fieldReference.GetValueUsageInfo(fieldReference.SemanticModel.GetEnclosingSymbol(fieldReference.Syntax.SpanStart, context.CancellationToken)) is
                            ValueUsageInfo.ReadableWritableReference or ValueUsageInfo.WritableReference)
                        {
                            //  Eliminate candidates that are assigned to ref or out variables.
                            cache.Candidates.Remove(fieldReference.Field);
                        }
                        else
                        {
                            //  Eliminate candidates that are used in ways that prohibit conversion to ReadOnlySpan (see
                            //  visitor classes for details).
                            fieldReference.Parent.Accept(fieldReferenceVisitor, new VisitContext(fieldReference, fieldReference.Field, context.CancellationToken));
                        }
                        break;
                    case IFieldInitializerOperation fieldInitializer:
                        //  Eliminate candidates that don't have an array initializer with all-constant elements.
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

            //  Report diagnostics on all field symbols that survived the candidate elimination round.
            void OnCompilationEnd(CompilationAnalysisContext context)
            {
                var asSpanInvocationLookup = cache.GetSavedOperationsLookup();
                foreach (var field in cache.Candidates)
                {
                    //  Save the locations of all operations that need to be fixed by the fixer.
                    var savedLocations = asSpanInvocationLookup[field].Select(x => new SavedSpanLocation(x.Syntax.Span, x.Syntax.SyntaxTree.FilePath));
                    string propertyValue = SavedSpanLocation.Serialize(savedLocations);
                    var properties = ImmutableDictionary<string, string?>.Empty.Add(FixerDataPropertyName, propertyValue);
                    var messageArgument = ((IArrayTypeSymbol)field.Type).ElementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                    var diagnostic = field.CreateDiagnostic(Rule, properties, messageArgument);
                    context.ReportDiagnostic(diagnostic);
                }
                cache.Dispose();
            }
        }

        //  Not compared for equality
#pragma warning disable CA1815
        private readonly struct VisitContext
#pragma warning restore CA1815
        {
            public VisitContext(IOperation operation, IFieldSymbol field, CancellationToken cancellationToken)
            {
                Operation = operation;
                Field = field;
                CancellationToken = cancellationToken;
            }

            public IOperation Operation { get; }
            public IFieldSymbol Field { get; }
            public CancellationToken CancellationToken { get; }

            public VisitContext WithOperation(IOperation newOperation) => new(newOperation, Field, CancellationToken);
        }

        //  Not compared for equality
#pragma warning disable CA1815
        /// <summary>
        /// Represents a saved span in source code that that needs to be fixed by the fixer.
        /// Currently used to save field references that were passed to any 'AsSpan' overload.
        /// </summary>
        internal readonly struct SavedSpanLocation : IEquatable<SavedSpanLocation>
#pragma warning restore CA1815
        {
            private const int SpanStartCaptureGroup = 1;
            private const int SpanLengthCaptureGroup = 2;
            private const int SourceFilePathCaptureGroup = 3;
            private const string FieldSeparator = "|";
            private const string SavedSpanSeparator = "\n";
            private static readonly Regex s_parseRegex = new($"([0-9]+){Regex.Escape(FieldSeparator)}([0-9]+){Regex.Escape(FieldSeparator)}(.*)$");

            public SavedSpanLocation(TextSpan span, string sourceFilePath)
            {
                Span = span;
                SourceFilePath = sourceFilePath;
            }

            public TextSpan Span { get; }
            public string SourceFilePath { get; }

            public override string ToString()
            {
                return $@"{Span.Start}{FieldSeparator}{Span.Length}{FieldSeparator}{SourceFilePath}";
            }

            public static SavedSpanLocation Parse(string text)
            {
                var match = s_parseRegex.Match(text);
                RoslynDebug.Assert(match.Success, "Invalid saved data format");

                return new SavedSpanLocation(
                    new TextSpan(
                        int.Parse(match.Groups[SpanStartCaptureGroup].Value, CultureInfo.InvariantCulture),
                        int.Parse(match.Groups[SpanLengthCaptureGroup].Value, CultureInfo.InvariantCulture)),
                    match.Groups[SourceFilePathCaptureGroup].Value);
            }

            public static string Serialize(IEnumerable<SavedSpanLocation> savedSpans)
            {
                return string.Join(SavedSpanSeparator, savedSpans.Select(x => x.ToString()));
            }

            public static ImmutableArray<SavedSpanLocation> Deserialize(string text)
            {
                var lines = text.Split(new[] { SavedSpanSeparator }, StringSplitOptions.RemoveEmptyEntries);
                var builder = ImmutableArray.CreateBuilder<SavedSpanLocation>(lines.Length);
                builder.Count = lines.Length;
                for (int i = 0; i < lines.Length; ++i)
                    builder[i] = Parse(lines[i]);

                return builder.MoveToImmutable();
            }

            public static bool Equals(SavedSpanLocation left, SavedSpanLocation right) => (left.Span, left.SourceFilePath) == (right.Span, right.SourceFilePath);
            public static bool operator ==(SavedSpanLocation left, SavedSpanLocation right) => Equals(left, right);
            public static bool operator !=(SavedSpanLocation left, SavedSpanLocation right) => !Equals(left, right);
            public bool Equals(SavedSpanLocation other) => Equals(this, other);
            public override bool Equals(object obj) => obj is SavedSpanLocation other && Equals(this, other);
            public override int GetHashCode() => (Span, SourceFilePath).GetHashCode();
        }

        /// <summary>
        /// Visits the parents of <see cref="IFieldReferenceOperation"/>s and eliminates candidates that
        /// are used in ways that prohibit conversion to <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        private sealed class FieldReferenceVisitor : OperationVisitor<VisitContext, Unit>
        {
            private readonly Cache _cache;
            private readonly ArrayElementReferenceVisitor _arrayElementReferenceVisitor;

            public FieldReferenceVisitor(Cache cache)
            {
                _cache = cache;
                _arrayElementReferenceVisitor = new ArrayElementReferenceVisitor(cache);
            }

            public override Unit VisitArrayElementReference(IArrayElementReferenceOperation operation, VisitContext argument)
            {
                if (operation.GetValueUsageInfo(operation.SemanticModel.GetEnclosingSymbol(operation.Syntax.SpanStart, argument.CancellationToken)) is
                    ValueUsageInfo.ReadableWritableReference or ValueUsageInfo.WritableReference)
                {
                    //  Eliminate candidates who's elements are assigned to ref or out variables.
                    _cache.Candidates.Remove(argument.Field);
                }
                else
                {
                    //  Eliminate candidates who's elements are used in ways that prohibit conversion to ReadOnlySpan
                    operation.Parent.Accept(_arrayElementReferenceVisitor, argument.WithOperation(operation));
                }
                return base.VisitArrayElementReference(operation, argument);
            }

            public override Unit VisitInvocation(IInvocationOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitInvocation(operation, argument);
            }

            public override Unit VisitPropertyReference(IPropertyReferenceOperation operation, VisitContext argument)
            {
                if (!operation.Property.Equals(_cache.ArrayLengthProperty, SymbolEqualityComparer.Default))
                    _cache.Candidates.Remove(argument.Field);
                return base.VisitPropertyReference(operation, argument);
            }

            public override Unit VisitArgument(IArgumentOperation operation, VisitContext argument)
            {
                var targetMethod = GetTargetMethod(operation.Parent);
                if (_cache.IsAsSpanMethod(targetMethod) &&
                    operation.Parent.Parent is IConversionOperation conversion &&
                    conversion.Type.OriginalDefinition.Equals(_cache.ReadOnlySpanType, SymbolEqualityComparer.Default))
                {
                    _cache.AddSavedOperation(argument.Field, operation);
                }
                else
                {
                    _cache.Candidates.Remove(argument.Field);
                }

                return base.VisitArgument(operation, argument);
            }

            public override Unit VisitConversion(IConversionOperation operation, VisitContext argument)
            {
                if (!operation.Type.OriginalDefinition.Equals(_cache.ReadOnlySpanType, SymbolEqualityComparer.Default))
                    _cache.Candidates.Remove(argument.Field);
                return base.VisitConversion(operation, argument);
            }

            public override Unit VisitSimpleAssignment(ISimpleAssignmentOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitSimpleAssignment(operation, argument);
            }

            public override Unit VisitCoalesceAssignment(ICoalesceAssignmentOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitCoalesceAssignment(operation, argument);
            }

            public override Unit VisitVariableInitializer(IVariableInitializerOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitVariableInitializer(operation, argument);
            }

            public override Unit VisitTuple(ITupleOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitTuple(operation, argument);
            }

            public override Unit VisitReturn(IReturnOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitReturn(operation, argument);
            }

            public override Unit VisitArrayInitializer(IArrayInitializerOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitArrayInitializer(operation, argument);
            }

            private static IMethodSymbol? GetTargetMethod(IOperation invocationOrObjectCreation)
            {
                IMethodSymbol? result = invocationOrObjectCreation switch
                {
                    IInvocationOperation invocation => invocation.TargetMethod,
                    IObjectCreationOperation objectCreation => objectCreation.Constructor,
                    _ => null
                };
                return result;
            }
        }

        /// <summary>
        /// Visits the parents of <see cref="IArrayElementReferenceOperation"/>s and eliminates candidates
        /// who's elements are used in ways that prohibit conversion to <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        private sealed class ArrayElementReferenceVisitor : OperationVisitor<VisitContext, Unit>
        {
            private readonly Cache _cache;

            public ArrayElementReferenceVisitor(Cache cache)
            {
                _cache = cache;
            }

            public override Unit VisitSimpleAssignment(ISimpleAssignmentOperation operation, VisitContext argument)
            {
                if (operation.Target.Equals(argument.Operation))
                    _cache.Candidates.Remove(argument.Field);
                return base.VisitSimpleAssignment(operation, argument);
            }

            public override Unit VisitCompoundAssignment(ICompoundAssignmentOperation operation, VisitContext argument)
            {
                if (operation.Target.Equals(argument.Operation))
                    _cache.Candidates.Remove(argument.Field);
                return base.VisitCompoundAssignment(operation, argument);
            }

            public override Unit VisitTuple(ITupleOperation operation, VisitContext argument)
            {
                if (operation.Parent is IDeconstructionAssignmentOperation deconstruction && deconstruction.Target.Equals(operation))
                {
                    _cache.Candidates.Remove(argument.Field);
                }
                return base.VisitTuple(operation, argument);
            }

            public override Unit VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, VisitContext argument)
            {
                _cache.Candidates.Remove(argument.Field);
                return base.VisitIncrementOrDecrement(operation, argument);
            }
        }

        private sealed class Cache : IDisposable
        {
            private readonly ImmutableHashSet<ITypeSymbol> _supportedArrayElementTypes;
            private readonly ImmutableHashSet<IMethodSymbol> _asSpanMethods;
            private readonly PooledConcurrentSet<(IFieldSymbol Field, IOperation Operation)> _savedOperations;

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
                _savedOperations = PooledConcurrentSet<(IFieldSymbol, IOperation)>.GetInstance();
                return;

                //  Local functions

                static ImmutableHashSet<ITypeSymbol> GetSupportedArrayElementTypes(Compilation compilation)
                {
                    var builder = ImmutableHashSet.CreateBuilder<ITypeSymbol>(SymbolEqualityComparer.Default);

                    builder.AddIfNotNull(compilation.GetSpecialType(SpecialType.System_Boolean));
                    builder.AddIfNotNull(compilation.GetSpecialType(SpecialType.System_Byte));
                    builder.AddIfNotNull(compilation.GetSpecialType(SpecialType.System_SByte));

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

            public bool IsAsSpanMethod(IMethodSymbol? method) => method is not null && _asSpanMethods.Contains(method.OriginalDefinition);

            /// <summary>
            /// Add <see cref="IOperation"/>s that need to be fixed by fixer. Currently this is used
            /// for invocations of any 'AsSpan' method on a field reference.
            /// </summary>
            /// <param name="field">The field the operation is associated with</param>
            /// <param name="operation">The field reference operation that needs to be fixed.</param>
            public void AddSavedOperation(IFieldSymbol field, IOperation operation) => _savedOperations.Add((field, operation));

            public ILookup<IFieldSymbol, IOperation> GetSavedOperationsLookup() => _savedOperations.ToLookup(
                t => t.Field,
                t => t.Operation);

            public void Dispose()
            {
                Candidates.Dispose();
                _savedOperations.Dispose();
            }
        }
    }
}
