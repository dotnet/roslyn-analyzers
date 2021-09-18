// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SuspiciousCastFromCharToIntAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3200";

        private static readonly LocalizableString s_localizableTitle = Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntTitle));
        private static readonly LocalizableString s_localizableMessage = Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntMessage));
        private static readonly LocalizableString s_localizableDescription = Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntDescription));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Correctness,
            RuleLevel.BuildWarning,
            s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var apiCache = GetApiHandlerCache(context.Compilation);

            if (apiCache.IsEmpty)
                return;

            context.RegisterOperationAction(context =>
            {
                var targetMethod = context.Operation switch
                {
                    IInvocationOperation invocation => invocation.TargetMethod,
                    IObjectCreationOperation objectCreation => objectCreation.Constructor,
                    _ => default
                };

                RoslynDebug.Assert(targetMethod is not null, $"{nameof(targetMethod)} must not be null.");

                if (apiCache.TryGetValue(targetMethod, out var handler))
                    handler.AnalyzeInvocationOperation(context);

            }, OperationKind.Invocation, OperationKind.ObjectCreation);
        }

        //  Property bag
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal struct RequiredSymbols
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            private RequiredSymbols(ITypeSymbol charType, ITypeSymbol int32Type, ITypeSymbol stringType)
            {
                CharType = charType;
                Int32Type = int32Type;
                StringType = stringType;
            }

            public static bool TryGetSymbols(Compilation compilation, out RequiredSymbols result)
            {
                var charType = compilation.GetSpecialType(SpecialType.System_Char);
                var int32Type = compilation.GetSpecialType(SpecialType.System_Int32);
                var stringType = compilation.GetSpecialType(SpecialType.System_String);

                if (charType is not null && int32Type is not null && stringType is not null)
                {
                    result = new RequiredSymbols(charType, int32Type, stringType);
                    return true;
                }

                result = default;
                return false;
            }

            public ITypeSymbol CharType { get; }
            public ITypeSymbol Int32Type { get; }
            public ITypeSymbol StringType { get; }
        }

        //  Property bag
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal struct FixCharCastContext
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

        internal abstract class ApiHandler
        {
            protected ApiHandler(IMethodSymbol method)
            {
                Method = method;
            }

            public IMethodSymbol Method { get; }

            public abstract void AnalyzeInvocationOperation(OperationAnalysisContext context);

            public abstract Task<Document> CreateChangedDocumentAsync(FixCharCastContext context);
        }

        private sealed class StringSplitCharInt32StringSplitOptionsHandler : ApiHandler
        {
            private StringSplitCharInt32StringSplitOptionsHandler(IMethodSymbol splitMethod) : base(splitMethod) { }

            public static StringSplitCharInt32StringSplitOptionsHandler? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringSplitOptions, out var stringSplitOptions))
                    return null;

                var splitMethod = symbols.StringType.GetMembers(nameof(string.Split)).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.CharType, symbols.Int32Type, stringSplitOptions);

                return splitMethod is not null ? new StringSplitCharInt32StringSplitOptionsHandler(splitMethod) : null;
            }

            public override void AnalyzeInvocationOperation(OperationAnalysisContext context)
            {
                var invocation = (IInvocationOperation)context.Operation;
                var argument = invocation.Arguments.First(x => x.Parameter.Ordinal == 1);

                if (argument.Value is IConversionOperation conversion && conversion.IsImplicit && conversion.Operand.Type.SpecialType == SpecialType.System_Char)
                {
                    context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                }
            }

            public override async Task<Document> CreateChangedDocumentAsync(FixCharCastContext context)
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

        private sealed class StringSplitStringInt32StringSplitOptionsHandler : ApiHandler
        {
            private StringSplitStringInt32StringSplitOptionsHandler(IMethodSymbol splitMethod) : base(splitMethod) { }

            public static StringSplitStringInt32StringSplitOptionsHandler? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringSplitOptions, out var stringSplitOptionsType))
                    return null;

                var splitMethod = symbols.StringType.GetMembers(nameof(string.Split)).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.StringType, symbols.Int32Type, stringSplitOptionsType);

                return splitMethod is not null ? new StringSplitStringInt32StringSplitOptionsHandler(splitMethod) : null;
            }

            public override void AnalyzeInvocationOperation(OperationAnalysisContext context)
            {
                var invocation = (IInvocationOperation)context.Operation;
                var argument = invocation.Arguments.First(x => x.Parameter.Ordinal == 1);

                if (argument.Value is IConversionOperation conversion && conversion.IsImplicit && conversion.Operand.Type.SpecialType == SpecialType.System_Char)
                {
                    context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                }
            }

            public override async Task<Document> CreateChangedDocumentAsync(FixCharCastContext context)
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

        private sealed class StringBuilderInt32Handler : ApiHandler
        {
            private StringBuilderInt32Handler(IMethodSymbol ctor) : base(ctor) { }

            public static StringBuilderInt32Handler? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextStringBuilder, out var stringBuilderType))
                    return null;

                var ctor = stringBuilderType.GetMembers(WellKnownMemberNames.InstanceConstructorName).Cast<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.Int32Type);

                return ctor is not null ? new StringBuilderInt32Handler(ctor) : null;
            }

            public override void AnalyzeInvocationOperation(OperationAnalysisContext context)
            {
                var objectCreation = (IObjectCreationOperation)context.Operation;
                var argument = objectCreation.Arguments[0];

                if (argument.Value is IConversionOperation conversion && conversion.IsImplicit && conversion.Operand.Type.SpecialType == SpecialType.System_Char)
                {
                    context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                }
            }

            public override async Task<Document> CreateChangedDocumentAsync(FixCharCastContext context)
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

        internal static ImmutableDictionary<IMethodSymbol, ApiHandler> GetApiHandlerCache(Compilation compilation)
        {
            if (!RequiredSymbols.TryGetSymbols(compilation, out var symbols))
                return ImmutableDictionary<IMethodSymbol, ApiHandler>.Empty;

            var builder = ImmutableDictionary.CreateBuilder<IMethodSymbol, ApiHandler>();

            AddIfNotNull(StringSplitCharInt32StringSplitOptionsHandler.Create(compilation, symbols));
            AddIfNotNull(StringSplitStringInt32StringSplitOptionsHandler.Create(compilation, symbols));
            AddIfNotNull(StringBuilderInt32Handler.Create(compilation, symbols));

            return builder.ToImmutable();

            // Local functions

            void AddIfNotNull(ApiHandler? handler)
            {
                if (handler is not null)
                    builder!.Add(handler.Method, handler);
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
