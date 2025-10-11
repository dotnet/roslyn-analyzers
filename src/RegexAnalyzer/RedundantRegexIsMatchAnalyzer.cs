using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RegexAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RedundantRegexIsMatchAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "REGEX001";
    private const string Title = "Use Regex.Match with pattern matching instead of Regex.IsMatch guard";
    private const string MessageFormat = "Consider using 'Regex.Match' with pattern matching instead of checking 'IsMatch' before calling 'Match'";
    private const string Description = "Using Regex.IsMatch followed by Regex.Match performs redundant matching. Use Regex.Match with pattern matching for better performance.";
    private const string Category = "Performance";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        // Check if the condition is a Regex.IsMatch call
        if (!IsRegexIsMatchInvocation(ifStatement.Condition, context.SemanticModel, out var isMatchArgs))
            return;

        // Check if the body contains a Regex.Match call with the same arguments
        var matchInvocation = FindRegexMatchInvocation(ifStatement.Statement, context.SemanticModel, isMatchArgs);
        if (matchInvocation == null)
            return;

        // Report diagnostic on the IsMatch call
        var diagnostic = Diagnostic.Create(Rule, ifStatement.Condition.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private bool IsRegexIsMatchInvocation(ExpressionSyntax expression, SemanticModel semanticModel, out ImmutableArray<ArgumentSyntax> arguments)
    {
        arguments = ImmutableArray<ArgumentSyntax>.Empty;

        if (expression is not InvocationExpressionSyntax invocation)
            return false;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        if (memberAccess.Name.Identifier.Text != "IsMatch")
            return false;

        var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return false;

        if (methodSymbol.ContainingType?.ToString() != "System.Text.RegularExpressions.Regex")
            return false;

        arguments = invocation.ArgumentList.Arguments.ToImmutableArray();
        return true;
    }

    private InvocationExpressionSyntax? FindRegexMatchInvocation(StatementSyntax statement, SemanticModel semanticModel, ImmutableArray<ArgumentSyntax> expectedArgs)
    {
        // Handle block statements
        if (statement is BlockSyntax block)
        {
            foreach (var stmt in block.Statements)
            {
                var result = FindRegexMatchInStatement(stmt, semanticModel, expectedArgs);
                if (result != null)
                    return result;
            }
        }
        else
        {
            return FindRegexMatchInStatement(statement, semanticModel, expectedArgs);
        }

        return null;
    }

    private InvocationExpressionSyntax? FindRegexMatchInStatement(StatementSyntax statement, SemanticModel semanticModel, ImmutableArray<ArgumentSyntax> expectedArgs)
    {
        // Check for variable declarations: Match m = Regex.Match(...)
        if (statement is LocalDeclarationStatementSyntax localDecl)
        {
            foreach (var variable in localDecl.Declaration.Variables)
            {
                if (variable.Initializer?.Value is InvocationExpressionSyntax invocation &&
                    IsRegexMatchInvocation(invocation, semanticModel, expectedArgs))
                {
                    return invocation;
                }
            }
        }

        // Check for expression statements: var m = Regex.Match(...) or m = Regex.Match(...)
        if (statement is ExpressionStatementSyntax exprStmt)
        {
            if (exprStmt.Expression is AssignmentExpressionSyntax assignment &&
                assignment.Right is InvocationExpressionSyntax invocation &&
                IsRegexMatchInvocation(invocation, semanticModel, expectedArgs))
            {
                return invocation;
            }
        }

        return null;
    }

    private bool IsRegexMatchInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel, ImmutableArray<ArgumentSyntax> expectedArgs)
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
        if (invocation.ArgumentList.Arguments.Count != expectedArgs.Length)
            return false;

        for (int i = 0; i < expectedArgs.Length; i++)
        {
            if (!AreArgumentsEquivalent(expectedArgs[i], invocation.ArgumentList.Arguments[i], semanticModel))
                return false;
        }

        return true;
    }

    private bool AreArgumentsEquivalent(ArgumentSyntax arg1, ArgumentSyntax arg2, SemanticModel semanticModel)
    {
        // Simple syntactic comparison for now
        return arg1.Expression.ToString() == arg2.Expression.ToString();
    }
}
