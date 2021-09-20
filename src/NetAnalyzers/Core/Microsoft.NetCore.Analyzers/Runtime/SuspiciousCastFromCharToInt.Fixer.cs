// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using RequiredSymbols = Microsoft.NetCore.Analyzers.Runtime.SuspiciousCastFromCharToIntAnalyzer.RequiredSymbols;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class SuspiciousCastFromCharToIntFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(SuspiciousCastFromCharToIntAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var doc = context.Document;
            var token = context.CancellationToken;
            var root = await doc.GetSyntaxRootAsync(token).ConfigureAwait(false);
            var model = await doc.GetSemanticModelAsync(token).ConfigureAwait(false);

            //  If there were unnecessary parentheses on the argument expression, then we won't get the reported IArgumentOperation
            //  from the call to SemanticModel.GetOperation().
            //  This is because the Syntax property of an IArgumentOperation does NOT include the unnecessary parentheses.
            //  In other words, if we have an IArgumentOperation 'argument', then the following two expressions are equivalent:
            //      argument.SemanticModel.GetOperation(argument.Syntax) 
            //  is equivalent to
            //      argument.Value.WalkDownConversion()
            var reportedOperation = model.GetOperation(root.FindNode(context.Span), token);
            var argumentOperation = reportedOperation as IArgumentOperation ?? reportedOperation?.WalkUpConversion()?.Parent as IArgumentOperation;
            if (argumentOperation is null)
                return;

            var targetMethod = argumentOperation.Parent switch
            {
                IInvocationOperation invocation => invocation.TargetMethod,
                IObjectCreationOperation objectCreation => objectCreation.Constructor,
                _ => default
            };

            if (targetMethod is null)
                return;

            var cache = CreateFixerCache(model.Compilation);

            if (!cache.TryGetValue(targetMethod, out var fixer))
                return;

            var codeAction = CodeAction.Create(
                MicrosoftNetCoreAnalyzersResources.SuspiciousCastFromCharToIntCodeFixTitle,
                token => fixer.GetChangedDocumentAsync(new FixCharCastContext(context, argumentOperation.Parent, this, token)),
                nameof(MicrosoftNetCoreAnalyzersResources.SuspiciousCastFromCharToIntCodeFixTitle));
            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        internal abstract SyntaxNode GetMemberAccessExpressionSyntax(SyntaxNode invocationExpressionSyntax);

        internal abstract SyntaxNode GetDefaultValueExpression(SyntaxNode parameterSyntax);

        //  Property bag
#pragma warning disable CA1815 // Override equals and operator equals on value types
        private readonly struct FixCharCastContext
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            public FixCharCastContext(CodeFixContext codeFixContext, IOperation operation, SuspiciousCastFromCharToIntFixer fixer, CancellationToken cancellationToken)
            {
                CodeFixContext = codeFixContext;
                Operation = operation;
                Fixer = fixer;
                CancellationToken = cancellationToken;
            }

            public CodeFixContext CodeFixContext { get; }
            public IOperation Operation { get; }
            public SuspiciousCastFromCharToIntFixer Fixer { get; }
            public CancellationToken CancellationToken { get; }
        }

        private abstract class OperationFixer
        {
            protected OperationFixer(IMethodSymbol invokedMethod)
            {
                InvokedMethod = invokedMethod;
            }

            public IMethodSymbol InvokedMethod { get; }

            public abstract Task<Document> GetChangedDocumentAsync(FixCharCastContext context);
        }

        /// <summary>
        /// Fixes calls to string.Split(char, int, StringSplitOptions) with an implicit char-to-int conversion.
        /// </summary>
        private sealed class StringSplit_CharInt32StringSplitOptions : OperationFixer
        {
            private StringSplit_CharInt32StringSplitOptions(IMethodSymbol splitMethod) : base(splitMethod) { }

            public static StringSplit_CharInt32StringSplitOptions? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringSplitOptions, out var stringSplitOptionsType))
                    return null;

                var splitMethod = symbols.StringType.GetMembers(nameof(string.Split)).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.CharType, symbols.Int32Type, stringSplitOptionsType);

                return splitMethod is not null ? new StringSplit_CharInt32StringSplitOptions(splitMethod) : null;
            }

            public override async Task<Document> GetChangedDocumentAsync(FixCharCastContext context)
            {
                var doc = context.CodeFixContext.Document;
                var token = context.CancellationToken;
                var invocation = (IInvocationOperation)context.Operation;
                var editor = await DocumentEditor.CreateAsync(doc, token).ConfigureAwait(false);

                var charTypeExpressionSyntax = editor.Generator.TypeExpression(invocation.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Char));
                var arguments = invocation.GetArgumentsInParameterOrder();
                var elementNodes = new[]
                {
                    arguments[0].Value.Syntax,
                    arguments[1].Value.Syntax
                };
                var arrayCreationSyntax = editor.Generator.ArrayCreationExpression(charTypeExpressionSyntax, elementNodes);
                var memberAccessSyntax = context.Fixer.GetMemberAccessExpressionSyntax(invocation.Syntax);
                var newInvocationSyntax = editor.Generator.InvocationExpression(memberAccessSyntax, arrayCreationSyntax, arguments[2].Syntax);

                editor.ReplaceNode(invocation.Syntax, newInvocationSyntax);

                return editor.GetChangedDocument();
            }
        }

        /// <summary>
        /// Fixes calls to String.Split(string, int, StringSplitOptions) with an implicit char-to-int conversion.
        /// </summary>
        private sealed class StringSplit_StringInt32StringSplitOptions : OperationFixer
        {
            private StringSplit_StringInt32StringSplitOptions(IMethodSymbol splitMethod) : base(splitMethod) { }

            public static StringSplit_StringInt32StringSplitOptions? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringSplitOptions, out var stringSplitOptionsType))
                    return null;

                var splitMethod = symbols.StringType.GetMembers(nameof(string.Split)).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.StringType, symbols.Int32Type, stringSplitOptionsType);

                return splitMethod is not null ? new StringSplit_StringInt32StringSplitOptions(splitMethod) : null;
            }

            public override async Task<Document> GetChangedDocumentAsync(FixCharCastContext context)
            {
                var editor = await DocumentEditor.CreateAsync(context.CodeFixContext.Document, context.CancellationToken).ConfigureAwait(false);
                var invocation = (IInvocationOperation)context.Operation;

                var stringType = invocation.SemanticModel.Compilation.GetSpecialType(SpecialType.System_String);
                var stringTypeExpressionSyntax = editor.Generator.TypeExpression(stringType);

                var arguments = invocation.GetArgumentsInParameterOrder();
                var elementNodes = new[]
                {
                    arguments[0].Value.Syntax,
                    ConvertCharOperationToString(editor, ((IConversionOperation)arguments[1].Value).Operand)
                };
                var arrayCreationSyntax = editor.Generator.ArrayCreationExpression(stringTypeExpressionSyntax, elementNodes);

                var optionsArgumentSyntax = arguments[2].ArgumentKind is ArgumentKind.DefaultValue ?
                    CreateEnumMemberAccess((INamedTypeSymbol)arguments[2].Parameter.Type, nameof(StringSplitOptions.None)) :
                    arguments[2].Syntax;

                var splitMemberAccessSyntax = context.Fixer.GetMemberAccessExpressionSyntax(context.Operation.Syntax);
                var newInvocationSyntax = editor.Generator.InvocationExpression(splitMemberAccessSyntax, arrayCreationSyntax, optionsArgumentSyntax);

                editor.ReplaceNode(context.Operation.Syntax, newInvocationSyntax);

                return editor.GetChangedDocument();

                //  Local functions

                SyntaxNode CreateEnumMemberAccess(INamedTypeSymbol enumType, string enumMemberName)
                {
                    RoslynDebug.Assert(editor is not null);

                    var enumTypeSyntax = editor.Generator.TypeExpressionForStaticMemberAccess(enumType);
                    return editor.Generator.MemberAccessExpression(enumTypeSyntax, enumMemberName);
                }
            }
        }

        /// <summary>
        /// Fixes calls to new StringBuilder(int) with an implicit char-to-int conversion.
        /// </summary>
        private sealed class StringBuilderCtor_Int32 : OperationFixer
        {
            private StringBuilderCtor_Int32(IMethodSymbol ctor) : base(ctor) { }

            public static StringBuilderCtor_Int32? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextStringBuilder, out var stringBuilderType))
                    return null;

                var ctor = stringBuilderType.GetMembers(WellKnownMemberNames.InstanceConstructorName).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.Int32Type);

                return ctor is not null ? new StringBuilderCtor_Int32(ctor) : null;
            }

            public override async Task<Document> GetChangedDocumentAsync(FixCharCastContext context)
            {
                var editor = await DocumentEditor.CreateAsync(context.CodeFixContext.Document, context.CancellationToken).ConfigureAwait(false);
                var instanceCreation = (IObjectCreationOperation)context.Operation;
                var argument = instanceCreation.Arguments[0];

                var newArgumentSyntax = ConvertCharOperationToString(editor, ((IConversionOperation)argument.Value).Operand);
                var newObjectCreationSyntax = editor.Generator.ObjectCreationExpression(instanceCreation.Constructor.ContainingType, newArgumentSyntax);

                editor.ReplaceNode(instanceCreation.Syntax, newObjectCreationSyntax);

                return editor.GetChangedDocument();
            }
        }

        private static ImmutableDictionary<IMethodSymbol, OperationFixer> CreateFixerCache(Compilation compilation)
        {
            var symbols = new RequiredSymbols(compilation);
            var builder = ImmutableDictionary.CreateBuilder<IMethodSymbol, OperationFixer>();

            AddIfNotNull(StringSplit_CharInt32StringSplitOptions.Create(compilation, symbols));
            AddIfNotNull(StringSplit_StringInt32StringSplitOptions.Create(compilation, symbols));
            AddIfNotNull(StringBuilderCtor_Int32.Create(compilation, symbols));

            return builder.ToImmutable();

            //  Local functions

            void AddIfNotNull(OperationFixer? fixer)
            {
                if (fixer is not null)
                    builder!.Add(fixer.InvokedMethod, fixer);
            }
        }

        private static SyntaxNode ConvertCharOperationToString(DocumentEditor editor, IOperation charOperation)
        {
            if (charOperation is ILiteralOperation charLiteral)
            {
                return editor.Generator.LiteralExpression(charLiteral.ConstantValue.Value.ToString());
            }

            var toStringMemberAccessSyntax = editor.Generator.MemberAccessExpression(charOperation.Syntax, nameof(object.ToString));

            return editor.Generator.InvocationExpression(toStringMemberAccessSyntax);
        }
    }
}
