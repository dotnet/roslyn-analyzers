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

            if (model.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparer)
                is not INamedTypeSymbol stringComparerType)
            {
                return;
            }

            string methodName = invocation.TargetMethod.Name;

            // Only fix the one-parameter overloads of these methods: Contains(string), StartsWith(string)
            if ((methodName.Equals(RCISCAnalyzer.StringContainsMethodName, System.StringComparison.Ordinal) ||
                 methodName.Equals(RCISCAnalyzer.StringStartsWithMethodName, System.StringComparison.Ordinal)) && invocation.Arguments.Length != 1)
            {
                return;
            }
            // Only fix the one, two or three parameter overloads of IndexOf: IndexOf(string), IndexOf(string, int), IndexOf(string, int, int)
            else if (methodName.Equals(RCISCAnalyzer.StringIndexOfMethodName, System.StringComparison.Ordinal) && invocation.Arguments.Length > 3)
            {
                return;
            }
            // No fixer for CompareTo, only diagnostic
            else if (methodName.Equals(RCISCAnalyzer.StringCompareToMethodName, System.StringComparison.Ordinal))
            {
                return;
            }

            // Ignore parenthesized operations
            IOperation? instanceGenericOperation = invocation.Instance;
            while (instanceGenericOperation is not null and IParenthesizedOperation parenthesizedOperation)
            {
                instanceGenericOperation = parenthesizedOperation.Operand;
            }

            // There should be a child invocation on the left side (instance) of the large invocation
            // If the large invocation is "a.ToLower().Contains(b)"
            // Its instance is "a.ToLower()", the child invocation
            if (instanceGenericOperation is not IInvocationOperation instanceOperation)
            {
                return;
            }

            string caseChangingApproachName;
            switch (instanceOperation.TargetMethod.Name)
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

            Task<Document> createChangedDocument(CancellationToken _) => FixInvocationAsync(doc, root,
                invocation, instanceOperation,
                stringComparisonType, stringComparerType,
                methodName, caseChangingApproachName);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: MicrosoftNetCoreAnalyzersResources.RecommendCaseInsensitiveStringComparisonTitle,
                    createChangedDocument,
                    equivalenceKey: MicrosoftNetCoreAnalyzersResources.RecommendCaseInsensitiveStringComparisonTitle + methodName + caseChangingApproachName),
                context.Diagnostics);
        }

        private static Task<Document> FixInvocationAsync(Document doc, SyntaxNode root,
            IInvocationOperation invocation, IInvocationOperation instanceOperation,
            INamedTypeSymbol stringComparisonType, INamedTypeSymbol stringComparerType,
            string diagnosableMethodName, string caseChangingApproachName)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            Debug.Assert(diagnosableMethodName is
                RCISCAnalyzer.StringContainsMethodName or
                RCISCAnalyzer.StringIndexOfMethodName or
                RCISCAnalyzer.StringStartsWithMethodName);
            SyntaxNode newInvocation = GetNewInvocationForContainsIndexOfAndStartsWith(generator,
                    invocation, instanceOperation, stringComparisonType, caseChangingApproachName);

            SyntaxNode newRoot = generator.ReplaceNode(root, invocation.Syntax, newInvocation);
            return Task.FromResult(doc.WithSyntaxRoot(newRoot));
        }

        private static SyntaxNode GetNewInvocationForContainsIndexOfAndStartsWith(SyntaxGenerator generator,
            IInvocationOperation invocation, IInvocationOperation instanceOperation,
            INamedTypeSymbol stringComparisonType, string caseChangingApproachName)
        {
            // For the Diagnosable methods Contains(string) and StartsWith(string)
            // If we have this code ('a' and 'b' are string instances):
            //     a.CaseChanging().Diagnosable(b);
            // We want to convert it to:
            //     a.Diagnosable(b, StringComparison.DesiredCultureDesiredCase);

            // For IndexOf we have 3 options:
            //    a.CaseChanging().IndexOf(b)                          => a.IndexOf(b, StringComparison.Desired)
            //    a.CaseChanging().IndexOf(b, startIndex: n)           => a.IndexOf(b, startIndex: n, StringComparison.Desired)
            //    a.CaseChanging().IndexOf(b, startIndex: n, count: m) => a.IndexOf(b, startIndex: n, count: m, StringComparison.Desired)

            // Retrieve "a" and replace the invoked CaseChanging method with the Diagnosable method
            SyntaxNode stringMemberAccessExpression = generator.MemberAccessExpression(instanceOperation.Instance.Syntax, invocation.TargetMethod.Name);

            // Already verified in RegisterCodeFixesAsync, this is merely defensive
            Debug.Assert(invocation.Arguments.Length <= 3);

            List<SyntaxNode> newArguments = new();
            foreach (IArgumentOperation argument in invocation.Arguments)
            {
                newArguments.Add(argument.Syntax);
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
            return generator.InvocationExpression(stringMemberAccessExpression, newArguments).WithTriviaFrom(invocation.Syntax);
        }
    }
}