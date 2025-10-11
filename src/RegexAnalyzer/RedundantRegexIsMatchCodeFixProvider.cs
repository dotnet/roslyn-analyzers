using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RegexAnalyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantRegexIsMatchCodeFixProvider)), Shared]
public class RedundantRegexIsMatchCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantRegexIsMatchAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the IsMatch invocation
        var isMatchInvocation = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();
        if (isMatchInvocation == null)
            return;

        // Find the containing if statement
        var ifStatement = isMatchInvocation.AncestorsAndSelf().OfType<IfStatementSyntax>().FirstOrDefault();
        if (ifStatement == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with Regex.Match pattern matching",
                createChangedDocument: c => RefactorToPatternMatchingAsync(context.Document, ifStatement, isMatchInvocation, c),
                equivalenceKey: nameof(RedundantRegexIsMatchCodeFixProvider)),
            diagnostic);
    }

    private async Task<Document> RefactorToPatternMatchingAsync(Document document, IfStatementSyntax ifStatement, InvocationExpressionSyntax isMatchInvocation, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Find the Regex.Match invocation in the body
        var matchInfo = FindMatchInvocationAndDeclaration(ifStatement.Statement, semanticModel, isMatchInvocation);
        if (matchInfo == null)
            return document;

        var (matchInvocation, variableName, declarationStatement) = matchInfo.Value;

        // Create the new pattern matching expression: Regex.Match(...) is { Success: true } m
        var newCondition = CreatePatternMatchingCondition(matchInvocation, variableName);

        // Create the new if statement body (without the Match declaration)
        var newBody = RemoveMatchDeclaration(ifStatement.Statement, declarationStatement);

        // Create the new if statement
        var newIfStatement = ifStatement
            .WithCondition(newCondition)
            .WithStatement(newBody);

        var newRoot = root.ReplaceNode(ifStatement, newIfStatement);
        return document.WithSyntaxRoot(newRoot);
    }

    private (InvocationExpressionSyntax, string, StatementSyntax)? FindMatchInvocationAndDeclaration(StatementSyntax statement, SemanticModel semanticModel, InvocationExpressionSyntax isMatchInvocation)
    {
        var statements = statement is BlockSyntax block ? block.Statements : SyntaxFactory.SingletonList(statement);

        foreach (var stmt in statements)
        {
            // Check for variable declarations: Match m = Regex.Match(...)
            if (stmt is LocalDeclarationStatementSyntax localDecl)
            {
                foreach (var variable in localDecl.Declaration.Variables)
                {
                    if (variable.Initializer?.Value is InvocationExpressionSyntax invocation &&
                        IsMatchingRegexMatchInvocation(invocation, isMatchInvocation, semanticModel))
                    {
                        return (invocation, variable.Identifier.Text, stmt);
                    }
                }
            }

            // Check for expression statements: m = Regex.Match(...)
            if (stmt is ExpressionStatementSyntax exprStmt &&
                exprStmt.Expression is AssignmentExpressionSyntax assignment &&
                assignment.Right is InvocationExpressionSyntax assignmentInvocation &&
                IsMatchingRegexMatchInvocation(assignmentInvocation, isMatchInvocation, semanticModel))
            {
                var variableName = assignment.Left.ToString();
                return (assignmentInvocation, variableName, stmt);
            }
        }

        return null;
    }

    private bool IsMatchingRegexMatchInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax isMatchInvocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        if (memberAccess.Name.Identifier.Text != "Match")
            return false;

        var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return false;

        if (methodSymbol.ContainingType?.ToString() != "System.Text.RegularExpressions.Regex")
            return false;

        // Check if arguments match
        if (invocation.ArgumentList.Arguments.Count != isMatchInvocation.ArgumentList.Arguments.Count)
            return false;

        for (int i = 0; i < isMatchInvocation.ArgumentList.Arguments.Count; i++)
        {
            if (isMatchInvocation.ArgumentList.Arguments[i].Expression.ToString() != invocation.ArgumentList.Arguments[i].Expression.ToString())
                return false;
        }

        return true;
    }

    private ExpressionSyntax CreatePatternMatchingCondition(InvocationExpressionSyntax matchInvocation, string variableName)
    {
        // Create: Regex.Match(...) is { Success: true } m
        // Use single space trivia to keep everything on one line
        var space = SyntaxFactory.Space;
        
        var successProperty = SyntaxFactory.Subpattern(
            SyntaxFactory.NameColon(
                SyntaxFactory.IdentifierName("Success").WithTrailingTrivia(space)),
            SyntaxFactory.ConstantPattern(
                SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));

        var propertyPatternClause = SyntaxFactory.PropertyPatternClause(
            SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(space),
            SyntaxFactory.SeparatedList(new[] { successProperty }),
            SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(space));

        var recursivePattern = SyntaxFactory.RecursivePattern()
            .WithPropertyPatternClause(propertyPatternClause)
            .WithDesignation(
                SyntaxFactory.SingleVariableDesignation(
                    SyntaxFactory.Identifier(variableName)));

        return SyntaxFactory.IsPatternExpression(
            matchInvocation,
            SyntaxFactory.Token(SyntaxKind.IsKeyword).WithLeadingTrivia(space).WithTrailingTrivia(space),
            recursivePattern);
    }

    private StatementSyntax RemoveMatchDeclaration(StatementSyntax statement, StatementSyntax declarationToRemove)
    {
        if (statement is BlockSyntax block)
        {
            var newStatements = block.Statements.Where(s => s != declarationToRemove);
            
            // If we removed all statements, keep an empty block for the if statement
            if (!newStatements.Any())
            {
                return SyntaxFactory.Block();
            }
            
            return block.WithStatements(SyntaxFactory.List(newStatements));
        }

        // If it's a single statement that we need to remove, return an empty block
        if (statement == declarationToRemove)
        {
            return SyntaxFactory.Block();
        }

        return statement;
    }
}
