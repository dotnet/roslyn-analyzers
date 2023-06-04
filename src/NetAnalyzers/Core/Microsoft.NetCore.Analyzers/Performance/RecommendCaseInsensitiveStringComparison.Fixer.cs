// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
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
    using RCISCAnalyzer = RecommendCaseInsensitiveStringComparisonAnalyzer;

    /// <summary>
    /// CA1862: Prefer the StringComparison method overloads to perform case-insensitive string comparisons.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = RCISCAnalyzer.RuleId), Shared]
    public sealed class RecommendCaseInsensitiveStringComparisonFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RCISCAnalyzer.RuleId);
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            CancellationToken ct = context.CancellationToken;

            Document doc = context.Document;

            SyntaxNode root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

            if (root.FindNode(context.Span, getInnermostNodeForTie: true) is not SyntaxNode node)
            {
                return;
            }

            SemanticModel model = await doc.GetSemanticModelAsync(ct).ConfigureAwait(false);

            if (model.GetOperation(node, ct) is not IInvocationOperation invocation)
            {
                return;
            }

            if (model.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison)
                is not INamedTypeSymbol stringComparisonType)
            {
                return;
            }

            bool isInverted = false;

            // Ignore parenthesized operations
            IOperation? instanceOffendingOperation = invocation.Instance;
            while (instanceOffendingOperation is not null and IParenthesizedOperation parenthesizedOperation)
            {
                instanceOffendingOperation = parenthesizedOperation.Operand;
            }

            IInvocationOperation removableInvocation;

            // There should be a child invocation on the left side (instance) of the large invocation
            // If the large invocation is "a.ToLower().Contains(b)"
            // Its instance is "a.ToLower()", the child invocation
            if (instanceOffendingOperation is IInvocationOperation instanceInvocation &&
                instanceInvocation.TargetMethod.Name is
                    RCISCAnalyzer.StringToLowerMethodName or RCISCAnalyzer.StringToUpperMethodName or RCISCAnalyzer.StringToLowerInvariantMethodName or RCISCAnalyzer.StringToUpperInvariantMethodName)
            {
                removableInvocation = instanceInvocation;
            }
            // The other option is that the offending operation is the first argument
            // a.Contains(b.ToLower(), ...)
            else
            {
                IOperation? argumentOffendingOperation = invocation.Arguments.FirstOrDefault();
                while (argumentOffendingOperation is not null and IParenthesizedOperation parenthesizedOperation)
                {
                    argumentOffendingOperation = parenthesizedOperation.Operand;
                }

                if (argumentOffendingOperation is IArgumentOperation argumentOperation &&
                    argumentOperation.Children.FirstOrDefault() is IInvocationOperation argumentInvocation)
                {
                    removableInvocation = argumentInvocation;
                    isInverted = true;
                }
                else
                {
                    return;
                }
            }

            string caseChangingApproachName;
            switch (removableInvocation.TargetMethod.Name)
            {
                case RCISCAnalyzer.StringToLowerMethodName or RCISCAnalyzer.StringToUpperMethodName:
                    caseChangingApproachName = RCISCAnalyzer.StringComparisonCurrentCultureIgnoreCaseName;
                    break;
                case RCISCAnalyzer.StringToLowerInvariantMethodName or RCISCAnalyzer.StringToUpperInvariantMethodName:
                    caseChangingApproachName = RCISCAnalyzer.StringComparisonInvariantCultureIgnoreCaseName;
                    break;
                default:
                    return;
            }

            Task<Document> createChangedDocument(CancellationToken _) => FixInvocationAsync(doc, root, isInverted,
                invocation, removableInvocation, stringComparisonType, invocation.TargetMethod.Name, caseChangingApproachName);

            string title = string.Format(MicrosoftNetCoreAnalyzersResources.RecommendCaseInsensitiveStringComparerStringComparisonCodeFixTitle, invocation.TargetMethod.Name, caseChangingApproachName);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument,
                    equivalenceKey: nameof(MicrosoftNetCoreAnalyzersResources.RecommendCaseInsensitiveStringComparerStringComparisonCodeFixTitle)),
                context.Diagnostics);
        }

        private static Task<Document> FixInvocationAsync(Document doc, SyntaxNode root, bool isInverted,
            IInvocationOperation mainInvocation, IInvocationOperation removableInvocation,
            INamedTypeSymbol stringComparisonType, string diagnosableMethodName, string caseChangingApproachName)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            Debug.Assert(diagnosableMethodName is
                RCISCAnalyzer.StringContainsMethodName or
                RCISCAnalyzer.StringIndexOfMethodName or
                RCISCAnalyzer.StringStartsWithMethodName);

            SyntaxNode newInvocation = GetNewInvocation(generator, isInverted, mainInvocation, removableInvocation, stringComparisonType, caseChangingApproachName);

            SyntaxNode newRoot = generator.ReplaceNode(root, mainInvocation.Syntax, newInvocation);
            return Task.FromResult(doc.WithSyntaxRoot(newRoot));
        }

        private static SyntaxNode GetNewInvocation(SyntaxGenerator generator, bool isInverted,
            IInvocationOperation mainInvocation, IInvocationOperation removableInvocation,
            INamedTypeSymbol stringComparisonType, string caseChangingApproachName)
        {
            // For the Diagnosable methods Contains(string) and StartsWith(string)
            // If we have this code ('a' and 'b' are string instances):
            //     A) a.CaseChanging().Diagnosable(b);
            //     B) a.Diagnosable(b.CaseChanging());
            // We want to convert any of them to:
            //     a.Diagnosable(b, StringComparison.DesiredCultureDesiredCase);

            // For IndexOf we have 3 options:
            //    A.1) a.CaseChanging().IndexOf(b)
            //    A.2) a.IndexOf(b.CaseChanging())
            //    B.1) a.CaseChanging().IndexOf(b, startIndex: n)
            //    B.2) a.IndexOf(b.CaseChanging(), startIndex: n)
            //    C.1) a.CaseChanging().IndexOf(b, startIndex: n, count: m)
            //    C.2) a.IndexOf(b.CaseChanging(), startIndex: n, count: m)
            // We want to convert them to (respectively):
            //    A) a.IndexOf(b, StringComparison.Desired)
            //    B) a.IndexOf(b, startIndex: n, StringComparison.Desired)
            //    C) a.IndexOf(b, startIndex: n, count: m, StringComparison.Desired)

            // Retrieve "a" and replace the invoked CaseChanging method with the Diagnosable method

            // Already verified in RegisterCodeFixesAsync, this is merely defensive
            Debug.Assert(mainInvocation.Arguments.Length <= 3);

            SyntaxNode stringMemberAccessExpression;
            List<SyntaxNode> newArguments = new();
            int argIndex;
            if (!isInverted)
            {
                stringMemberAccessExpression = generator.MemberAccessExpression(removableInvocation.Instance.Syntax, mainInvocation.TargetMethod.Name);
                argIndex = 0;
            }
            else
            {
                stringMemberAccessExpression = generator.MemberAccessExpression(mainInvocation.Instance.Syntax, mainInvocation.TargetMethod.Name);
                newArguments.Add(removableInvocation.Instance.Syntax); // Trim the case changing method
                argIndex = 1; // Skips the first
            }

            for (; argIndex < mainInvocation.Arguments.Length; argIndex++)
            {
                newArguments.Add(mainInvocation.Arguments[argIndex].Syntax);
            }

            // Retrieve "StringComparison.DesiredCultureDesiredCase"
            SyntaxNode stringComparisonEnumValueAccess = generator.MemberAccessExpression(
                generator.TypeExpressionForStaticMemberAccess(stringComparisonType),
                generator.IdentifierName(caseChangingApproachName)
            );

            // Convert the above member access into an argument node, then append it to the argument list: "b, StringComparison.DesiredCultureDesiredCase"
            SyntaxNode stringComparisonArgument = generator.Argument(stringComparisonEnumValueAccess);
            newArguments.Add(stringComparisonArgument);

            // Generate the suggestion: "a.Diagnosable(b, StringComparison.DesiredCultureDesiredCase)"
            return generator.InvocationExpression(stringMemberAccessExpression, newArguments).WithTriviaFrom(mainInvocation.Syntax);
        }
    }
}