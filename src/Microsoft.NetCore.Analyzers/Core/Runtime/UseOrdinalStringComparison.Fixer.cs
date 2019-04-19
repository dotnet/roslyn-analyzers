// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class UseOrdinalStringComparisonFixerBase : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseOrdinalStringComparisonAnalyzer.RuleId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);

            // We cannot have multiple overlapping diagnostics of this id.
            Diagnostic diagnostic = context.Diagnostics.Single();
            string title = SystemRuntimeAnalyzersResources.UseOrdinalStringComparisonTitle;

            if (IsInArgumentContext(node))
            {
                // StringComparison.CurrentCulture => StringComparison.Ordinal
                // StringComparison.CurrentCultureIgnoreCase => StringComparison.OrdinalIgnoreCase
                context.RegisterCodeFix(new MyCodeAction(title,
                                                         async ct => await FixArgument(context.Document, syntaxGenerator, root, node).ConfigureAwait(false),
                                                         equivalenceKey: title),
                                                    diagnostic);
            }
            else if (IsInIdentifierNameContext(node))
            {
                // string.Equals(a, b) => string.Equals(a, b, StringComparison.Ordinal)
                // string.Compare(a, b) => string.Compare(a, b, StringComparison.Ordinal)
                context.RegisterCodeFix(new MyCodeAction(title,
                                                         async ct => await FixIdentifierName(context.Document, syntaxGenerator, root, node, context.CancellationToken).ConfigureAwait(false),
                                                         equivalenceKey: title),
                                                    diagnostic);
            }
        }

        protected abstract bool IsInArgumentContext(SyntaxNode node);
        protected abstract Task<Document> FixArgument(Document document, SyntaxGenerator generator, SyntaxNode root, SyntaxNode argument);

        protected abstract bool IsInIdentifierNameContext(SyntaxNode node);
        protected abstract Task<Document> FixIdentifierName(Document document, SyntaxGenerator generator, SyntaxNode root, SyntaxNode identifier, CancellationToken cancellationToken);

        internal SyntaxNode CreateEqualsExpression(SyntaxGenerator generator, SemanticModel model, SyntaxNode operand1, SyntaxNode operand2, bool isEquals)
        {
            INamedTypeSymbol stringType = model.Compilation.GetSpecialType(SpecialType.System_String);
            SyntaxNode memberAccess = generator.MemberAccessExpression(
                        generator.TypeExpressionForStaticMemberAccess(stringType),
                        generator.IdentifierName(UseOrdinalStringComparisonAnalyzer.EqualsMethodName));
            SyntaxNode ordinal = CreateOrdinalMemberAccess(generator, model);
            SyntaxNode invocation = generator.InvocationExpression(
                memberAccess,
                operand1,
                operand2.WithoutTrailingTrivia(),
                ordinal)
                .WithAdditionalAnnotations(Formatter.Annotation);
            if (!isEquals)
            {
                invocation = generator.LogicalNotExpression(invocation);
            }

            invocation = invocation.WithTrailingTrivia(operand2.GetTrailingTrivia());

            return invocation;
        }

        internal static SyntaxNode CreateOrdinalMemberAccess(SyntaxGenerator generator, SemanticModel model)
        {
            INamedTypeSymbol stringComparisonType = WellKnownTypes.StringComparison(model.Compilation);
            return generator.MemberAccessExpression(
                generator.TypeExpressionForStaticMemberAccess(stringComparisonType),
                generator.IdentifierName(UseOrdinalStringComparisonAnalyzer.OrdinalText));
        }

        protected static bool CanAddStringComparison(IMethodSymbol methodSymbol, SemanticModel model)
        {
            if (WellKnownTypes.StringComparison(model.Compilation) == null)
            {
                return false;
            }

            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;
            switch (methodSymbol.Name)
            {
                case UseOrdinalStringComparisonAnalyzer.EqualsMethodName:
                    // can fix .Equals() with (string), (string, string)
                    switch (parameters.Length)
                    {
                        case 1:
                            return parameters[0].Type.SpecialType == SpecialType.System_String;
                        case 2:
                            return parameters[0].Type.SpecialType == SpecialType.System_String &&
                                parameters[1].Type.SpecialType == SpecialType.System_String;
                    }

                    break;
                case UseOrdinalStringComparisonAnalyzer.CompareMethodName:
                    // can fix .Compare() with (string, string), (string, int, string, int, int)
                    switch (parameters.Length)
                    {
                        case 2:
                            return parameters[0].Type.SpecialType == SpecialType.System_String &&
                                parameters[1].Type.SpecialType == SpecialType.System_String;
                        case 5:
                            return parameters[0].Type.SpecialType == SpecialType.System_String &&
                                parameters[1].Type.SpecialType == SpecialType.System_Int32 &&
                                parameters[2].Type.SpecialType == SpecialType.System_String &&
                                parameters[3].Type.SpecialType == SpecialType.System_Int32 &&
                                parameters[4].Type.SpecialType == SpecialType.System_Int32;
                    }

                    break;
            }

            return false;
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}
