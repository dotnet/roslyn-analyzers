// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA1870: <inheritdoc cref="UseSearchValuesTitle"/>
    /// </summary>
    public abstract class UseSearchValuesAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA1870";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            DiagnosticId,
            CreateLocalizableResourceString(nameof(UseSearchValuesTitle)),
            CreateLocalizableResourceString(nameof(UseSearchValuesTitle)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            description: CreateLocalizableResourceString(nameof(UseSearchValuesDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected abstract bool IsConstantByteOrCharArrayVariableDeclaratorSyntax(SyntaxNode syntax, out int length);

        protected abstract bool IsConstantByteOrCharReadOnlySpanPropertyDeclarationSyntax(SyntaxNode syntax, out int length);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBuffersSearchValues, out _) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1, out var readOnlySpanType))
                {
                    return;
                }

                var indexOfAnyMethods = GetIndexOfAnyMethods(context.Compilation);

                context.RegisterOperationAction(context => AnalyzeInvocation(context, indexOfAnyMethods, readOnlySpanType), OperationKind.Invocation);
            });
        }

        private void AnalyzeInvocation(OperationAnalysisContext context, HashSet<IMethodSymbol> indexOfAnyMethodsToDetect, INamedTypeSymbol readOnlySpanType)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (!indexOfAnyMethodsToDetect.Contains(invocation.TargetMethod))
            {
                return;
            }

            Debug.Assert(invocation.Arguments.Length is 1 or 2);
            IArgumentOperation valuesArgument = invocation.Arguments[^1];

            if (AreConstantValuesWorthReplacing(valuesArgument.Value, readOnlySpanType))
            {
                context.ReportDiagnostic(valuesArgument.CreateDiagnostic(Rule));
            }
        }

        private static HashSet<IMethodSymbol> GetIndexOfAnyMethods(Compilation compilation)
        {
            var methods = new HashSet<IMethodSymbol>();

            // Possible improvement: Add support for string {Last}IndexOfAny calls

            // {ReadOnly}Span<T>.{Last}IndexOfAny{Except}(ReadOnlySpan<T>) and ContainsAny{Except}(ReadOnlySpan<T>)
            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemMemoryExtensions, out var memoryExtensionsType) &&
                compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSpan1, out var spanType) &&
                compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1, out var readOnlySpanType))
            {
                foreach (var method in memoryExtensionsType.GetMembers().OfType<IMethodSymbol>())
                {
                    if (method.Parameters.Length != 2 || method.TypeParameters.Length != 1)
                    {
                        continue;
                    }

                    if (method.Name is not ("IndexOfAny" or "IndexOfAnyExcept" or "LastIndexOfAny" or "LastIndexOfAnyExcept" or "ContainsAny" or "ContainsAnyExcept"))
                    {
                        continue;
                    }

                    var firstParameterType = method.Parameters[0].Type.OriginalDefinition;
                    var secondParameterType = method.Parameters[1].Type.OriginalDefinition;

                    if (!SymbolEqualityComparer.Default.Equals(firstParameterType, spanType) &&
                        !SymbolEqualityComparer.Default.Equals(firstParameterType, readOnlySpanType))
                    {
                        continue;
                    }

                    if (!SymbolEqualityComparer.Default.Equals(secondParameterType, readOnlySpanType))
                    {
                        continue;
                    }

                    // All of these methods are generic for any T, but SearchValues only supports byte/char variants.
                    methods.Add(method.Construct(compilation.GetSpecialType(SpecialType.System_Byte)));
                    methods.Add(method.Construct(compilation.GetSpecialType(SpecialType.System_Char)));
                }
            }

            return methods;
        }

        private bool AreConstantValuesWorthReplacing(IOperation argument, INamedTypeSymbol readOnlySpanType)
        {
            // It's not always worth going through SearchValues if there are very few values used
            // such that they will already be using dedicated vectorized paths.
            const int MinLengthWorthReplacing = 6;

            if (argument is IConversionOperation conversion)
            {
                if (conversion.Operand is ILiteralOperation literal)
                {
                    // text.IndexOfAny("abc")
                    return
                        literal.Type is { SpecialType: SpecialType.System_String } &&
                        literal.ConstantValue.HasValue &&
                        literal.ConstantValue.Value is string values &&
                        values.Length >= MinLengthWorthReplacing;
                }
                else if (conversion.Operand is IArrayCreationOperation arrayCreation)
                {
                    // text.IndexOfAny(new[] { 'a', 'b', 'c' })
                    return
                        IsConstantByteOrCharSZArrayCreation(arrayCreation, out int length) &&
                        length >= MinLengthWorthReplacing;
                }
                else if (conversion.Operand is ILocalReferenceOperation localReference)
                {
                    // const string ValuesLocal = "abc";
                    // text.IndexOfAny(ValuesLocal)
                    return
                        localReference.Type is { SpecialType: SpecialType.System_String } &&
                        localReference.ConstantValue.HasValue &&
                        localReference.ConstantValue.Value is string values &&
                        values.Length >= MinLengthWorthReplacing;
                }
                else if (conversion.Operand is IFieldReferenceOperation fieldReference)
                {
                    if (fieldReference.Type is { SpecialType: SpecialType.System_String })
                    {
                        // const string ValuesField = "abc";
                        // text.IndexOfAny(ValuesField)
                        return
                            fieldReference.ConstantValue.HasValue &&
                            fieldReference.ConstantValue.Value is string values &&
                            values.Length >= MinLengthWorthReplacing;
                    }

                    // readonly char[] Values = new char[] { 'a', 'b', 'c' };
                    // text.IndexOfAny(Values)
                    // Possible false-negative: readonly array field is being modified intentionally
                    return
                        fieldReference.Field is { } field &&
                        field.IsReadOnly &&
                        IsByteOrCharSZArray(field.Type) &&
                        field.DeclaringSyntaxReferences is [var declaringSyntaxReference] &&
                        declaringSyntaxReference.GetSyntax() is { } syntax &&
                        IsConstantByteOrCharArrayVariableDeclaratorSyntax(syntax, out int length) &&
                        length >= MinLengthWorthReplacing;
                }
            }
            else if (argument is IUtf8StringOperation utf8String)
            {
                // text.IndexOfAny("abc"u8)
                return utf8String.Value.Length >= MinLengthWorthReplacing;
            }
            else if (argument is IPropertyReferenceOperation propertyReference)
            {
                // ReadOnlySpan<byte> Values => "abc"u8;
                // ReadOnlySpan<byte> Values => new byte[] { (byte)'a', (byte)'b', (byte)'c' };
                // ReadOnlySpan<char> Values => new char[] { 'a', 'b', 'c' };
                // text.IndexOfAny(Values)
                return
                    propertyReference.Member is IPropertySymbol property &&
                    property.IsReadOnly &&
                    IsByteOrCharReadOnlySpan(property.Type, readOnlySpanType) &&
                    property.DeclaringSyntaxReferences is [var declaringSyntaxReference] &&
                    declaringSyntaxReference.GetSyntax() is { } syntax &&
                    IsConstantByteOrCharReadOnlySpanPropertyDeclarationSyntax(syntax, out int length) &&
                    length >= MinLengthWorthReplacing;
            }

            return false;
        }

        private static bool IsByteOrCharSZArray(ITypeSymbol? type) =>
            type is IArrayTypeSymbol array &&
            array.IsSZArray &&
            array.ElementType.SpecialType is SpecialType.System_Byte or SpecialType.System_Char;

        private static bool IsByteOrCharReadOnlySpan(ISymbol symbol, INamedTypeSymbol readOnlySpanType) =>
            symbol is INamedTypeSymbol namedType &&
            SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, readOnlySpanType) &&
            namedType.TypeArguments is [var typeArgument] &&
            typeArgument.SpecialType is SpecialType.System_Byte or SpecialType.System_Char;

        private static bool IsConstantByteOrCharSZArrayCreation(IArrayCreationOperation arrayCreation, out int length)
        {
            length = 0;

            if (IsByteOrCharSZArray(arrayCreation.Type) &&
                arrayCreation.Initializer?.ElementValues is { } elements)
            {
                foreach (var element in elements)
                {
                    var actualElement = (element as IConversionOperation)?.Operand ?? element;

                    if (actualElement is not ILiteralOperation elementLiteral || !elementLiteral.ConstantValue.HasValue)
                    {
                        return false;
                    }
                }

                length = elements.Length;
                return true;
            }

            return false;
        }
    }
}
