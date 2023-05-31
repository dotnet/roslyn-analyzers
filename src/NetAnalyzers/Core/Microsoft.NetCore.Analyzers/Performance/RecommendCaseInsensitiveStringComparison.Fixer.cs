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
    /// <summary>
    /// CA1862: Prefer the StringComparison method overloads to perform case-insensitive string comparisons.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = RecommendCaseInsensitiveStringComparisonAnalyzer.RuleId), Shared]
    public sealed class RecommendCaseInsensitiveStringComparisonFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RecommendCaseInsensitiveStringComparisonAnalyzer.RuleId);
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

            // Only diagnose overloads that take one string argument:
            // Contains(string), IndexOf(string), StartsWith(string), CompareTo(string)
            if (invocation.Arguments.Length != 1)
            {
                return;
            }

            // There should be a child invocation on the left side (instance) of the large invocation
            // If the large invocation is "a.ToLower().Contains(b)"
            // Its instance is "a.ToLower()", the child invocation
            if (invocation.Instance is not IInvocationOperation instanceOperation)
            {
                return;
            }

            string caseChangingApproachName;
            switch (instanceOperation.TargetMethod.Name)
            {
                case RecommendCaseInsensitiveStringComparisonAnalyzer.StringToLowerMethodName or RecommendCaseInsensitiveStringComparisonAnalyzer.StringToUpperMethodName:
                    caseChangingApproachName = RecommendCaseInsensitiveStringComparisonAnalyzer.StringComparisonCurrentCultureIgnoreCaseName;
                    break;
                case RecommendCaseInsensitiveStringComparisonAnalyzer.StringToLowerInvariantMethodName or RecommendCaseInsensitiveStringComparisonAnalyzer.StringToUpperInvariantMethodName:
                    caseChangingApproachName = RecommendCaseInsensitiveStringComparisonAnalyzer.StringComparisonInvariantCultureIgnoreCaseName;
                    break;
                default:
                    return;
            }

            Task<Document> createChangedDocument(CancellationToken _) => FixInvocationAsync(doc, root,
                invocation, instanceOperation,
                stringComparisonType, stringComparerType,
                invocation.TargetMethod.Name, caseChangingApproachName);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: MicrosoftNetCoreAnalyzersResources.RecommendCaseInsensitiveStringComparisonTitle,
                    createChangedDocument,
                    equivalenceKey: MicrosoftNetCoreAnalyzersResources.RecommendCaseInsensitiveStringComparisonTitle + invocation.TargetMethod.Name + caseChangingApproachName),
                context.Diagnostics);
        }

        private static Task<Document> FixInvocationAsync(Document doc, SyntaxNode root,
            IInvocationOperation invocation, IInvocationOperation instanceOperation,
            INamedTypeSymbol stringComparisonType, INamedTypeSymbol stringComparerType,
            string diagnosableMethodName, string caseChangingApproachName)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            SyntaxNode newInvocation;
            if (diagnosableMethodName is
                RecommendCaseInsensitiveStringComparisonAnalyzer.StringContainsMethodName or
                RecommendCaseInsensitiveStringComparisonAnalyzer.StringIndexOfMethodName or
                RecommendCaseInsensitiveStringComparisonAnalyzer.StringStartsWithMethodName)
            {
                newInvocation = GetNewInvocationForContainsIndexOfAndStartsWith(generator,
                    invocation, instanceOperation, stringComparisonType, caseChangingApproachName);
            }
            else
            {
                // CompareTo should be the only other possible option
                Debug.Assert(diagnosableMethodName is RecommendCaseInsensitiveStringComparisonAnalyzer.StringCompareToMethodName);
                newInvocation = GetNewInvocationForCompareTo(generator,
                    invocation, instanceOperation, stringComparerType, caseChangingApproachName);
            }

            SyntaxNode newRoot = generator.ReplaceNode(root, invocation.Syntax, newInvocation);
            return Task.FromResult(doc.WithSyntaxRoot(newRoot));
        }

        private static SyntaxNode GetNewInvocationForContainsIndexOfAndStartsWith(SyntaxGenerator generator,
            IInvocationOperation invocation, IInvocationOperation instanceOperation,
            INamedTypeSymbol stringComparisonType, string caseChangingApproachName)
        {
            // For the Diagnosable methods Contains(string), IndexOf(string), StartsWith(string)
            // If we have this code ('a' and 'b' are string instances):
            //     a.CaseChanging().Diagnosable(b);
            // We want to convert it to:
            //     a.Diagnosable(b, StringComparison.DesiredCultureDesiredCase);

            // Retrieve "a" and replace the invoked CaseChanging method with the Diagnosable method
            SyntaxNode stringMemberAccessExpression = generator.MemberAccessExpression(instanceOperation.Instance.Syntax, invocation.TargetMethod.Name);

            List<SyntaxNode> newArguments = new()
            {
                // Retrieve "b"
                invocation.Arguments.First().Syntax
            };

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

        private static SyntaxNode GetNewInvocationForCompareTo(SyntaxGenerator generator,
            IInvocationOperation invocation, IInvocationOperation instanceOperation,
            INamedTypeSymbol stringComparerType, string caseChangingApproachName)
        {
            // For the Diagnosable method CompareTo(string)
            // If we have this code ('a' and 'b' are string instances):
            //     a.ToLower().CompareTo(b)
            // We want to convert it to:
            //     StringComparer.DesiredCultureDesiredCase.Compare(a, b)

            // Create the "StringComparer.DesiredCultureDesiredCase" member access expression
            SyntaxNode stringComparerPropertyInvocation = generator.MemberAccessExpression(
                generator.TypeExpressionForStaticMemberAccess(stringComparerType),
                caseChangingApproachName);

            // Create the ".Compare" expression using the above one
            SyntaxNode compareMethodInvocation = generator.MemberAccessExpression(
                stringComparerPropertyInvocation,
                "Compare");

            List<SyntaxNode> newArguments = new()
            {
                // Retrieve a and b
                generator.Argument(instanceOperation.Instance.Syntax),
                invocation.Arguments.First().Syntax
            };

            // Generate the suggestion: "StringComparer.DesiredCultureDesiredCase.Compare(a, b)"
            return generator.InvocationExpression(compareMethodInvocation, newArguments).WithTriviaFrom(invocation.Syntax);
        }
    }
}