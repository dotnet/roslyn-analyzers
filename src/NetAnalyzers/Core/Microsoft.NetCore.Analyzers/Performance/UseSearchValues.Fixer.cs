// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA1870: <inheritdoc cref="UseSearchValuesTitle"/>
    /// </summary>
    public abstract class UseSearchValuesFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(UseSearchValuesAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);
            if (node is null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    UseSearchValuesTitle,
                    cancellationToken => ConvertToSearchValuesAsync(context.Document, node, cancellationToken),
                    equivalenceKey: nameof(UseSearchValuesTitle)),
                context.Diagnostics);
        }

        protected abstract ValueTask<(SyntaxNode TypeDeclaration, INamedTypeSymbol? TypeSymbol)> GetTypeSymbolAsync(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken);

        protected abstract string ReplaceSearchValuesFieldName(string name);

        protected abstract SyntaxNode GetDeclaratorInitializer(SyntaxNode syntax);

        private async Task<Document> ConvertToSearchValuesAsync(Document document, SyntaxNode argumentNode, CancellationToken cancellationToken)
        {
            SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;

            if (semanticModel?.Compilation is not { } compilation ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBuffersSearchValues, out INamedTypeSymbol? searchValues) ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBuffersSearchValues1, out INamedTypeSymbol? searchValuesOfT) ||
                semanticModel.GetOperation(argumentNode, cancellationToken) is not { } argument)
            {
                return document;
            }

            // Walk up the operation tree to find the argument operation.
            while (argument is not IArgumentOperation)
            {
                if (argument.Parent is null)
                {
                    return document;
                }

                argument = argument.Parent;
            }

            var argumentOperation = (IArgumentOperation)argument;

            bool isByte =
                argumentOperation.Parameter?.Type is INamedTypeSymbol spanType &&
                spanType.TypeArguments is [var typeArgument] &&
                typeArgument.SpecialType == SpecialType.System_Byte;

            string defaultSearchValuesFieldName = GetSearchValuesFieldName(argumentOperation.Value, isByte);

            string fieldName = defaultSearchValuesFieldName;

            (var typeDeclaration, var typeSymbol) = await GetTypeSymbolAsync(semanticModel, argumentNode, cancellationToken).ConfigureAwait(false);

            if (typeSymbol is not null)
            {
                IEnumerable<ISymbol> members = GetAllMembers(typeSymbol);
                int memberCount = 1;
                while (members.Any(m => m.Name == fieldName))
                {
                    fieldName = $"{defaultSearchValuesFieldName}{memberCount++}";
                }
            }

            // Allow the user to pick a different name for the method.
            fieldName = ReplaceSearchValuesFieldName(fieldName);

            // private static readonly SearchValues<T> s_myValues = SearchValues.Create(argument);
            var newField = generator.FieldDeclaration(
                fieldName,
                generator.TypeExpression(searchValuesOfT.Construct(compilation.GetSpecialType(isByte ? SpecialType.System_Byte : SpecialType.System_Char))),
                Accessibility.Private,
                DeclarationModifiers.Static.WithIsReadOnly(true),
                generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpressionForStaticMemberAccess(searchValues), "Create"),
                    CreateSearchValuesCreateArgument(argumentOperation.Syntax, argumentOperation.Value, cancellationToken)));

            editor.InsertMembers(typeDeclaration, 0, new[] { newField });

            editor.ReplaceNode(argumentNode, generator.IdentifierName(fieldName));

            return editor.GetChangedDocument();
        }

        private static IEnumerable<ISymbol> GetAllMembers(ITypeSymbol? symbol)
        {
            while (symbol != null)
            {
                foreach (ISymbol member in symbol.GetMembers())
                {
                    yield return member;
                }

                symbol = symbol.BaseType;
            }
        }

        private static string GetSearchValuesFieldName(IOperation argument, bool isByte)
        {
            if (argument is IConversionOperation conversion)
            {
                if (conversion.Operand is ILocalReferenceOperation localReference)
                {
                    return CreateFromExistingName(localReference.Local.Name);
                }

                if (conversion.Operand is IFieldReferenceOperation fieldReference)
                {
                    return CreateFromExistingName(fieldReference.Field.Name);
                }
            }
            else if (argument is IPropertyReferenceOperation propertyReference)
            {
                return CreateFromExistingName(propertyReference.Property.Name);
            }

            return isByte ? "s_myBytes" : "s_myChars";

            static string CreateFromExistingName(string name)
            {
                if (!name.StartsWith("s_", StringComparison.OrdinalIgnoreCase))
                {
                    if (name.Length >= 2 && IsAsciiLetterUpper(name[0]) && !IsAsciiLetterUpper(name[1]))
                    {
                        name = $"{char.ToLowerInvariant(name[0])}{name[1..]}";
                    }

                    name = $"s_{name}";
                }

                return $"{name}SearchValues";

                static bool IsAsciiLetterUpper(char c) => c is >= 'A' and <= 'Z';
            }
        }

        private SyntaxNode CreateSearchValuesCreateArgument(SyntaxNode originalSyntax, IOperation argument, CancellationToken cancellationToken)
        {
            if (argument is IConversionOperation conversion)
            {
                if (conversion.Operand is ILocalReferenceOperation localReference)
                {
                    Debug.Assert(localReference.Local.DeclaringSyntaxReferences.Length == 1);

                    // Local string literal would be out of scope in the field declaration.
                    return GetDeclaratorInitializer(localReference.Local);
                }
                else if (conversion.Operand is IFieldReferenceOperation fieldReference)
                {
                    if (!fieldReference.ConstantValue.HasValue)
                    {
                        // If we were to use the field reference directly, we risk initializing the SearchValues field to an empty
                        // instance depending on field declaration order.
                        return GetDeclaratorInitializer(fieldReference.Field);
                    }
                }
            }
            else if (argument is IPropertyReferenceOperation propertyReference)
            {
                if (!propertyReference.Property.IsStatic)
                {
                    // Can't access an instance property from a field initializer.
                    return GetDeclaratorInitializer(propertyReference.Property);
                }
            }

            // Use the original syntax (e.g. string literal, static property reference ...)
            return originalSyntax;

            SyntaxNode GetDeclaratorInitializer(ISymbol symbol)
            {
                Debug.Assert(symbol.DeclaringSyntaxReferences.Length == 1);

                return this.GetDeclaratorInitializer(symbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken));
            }
        }
    }
}