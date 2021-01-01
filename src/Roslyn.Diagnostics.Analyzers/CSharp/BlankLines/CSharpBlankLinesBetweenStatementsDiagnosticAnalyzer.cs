// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.CSharp.Analyzers.BlankLines
{
    /// <summary>
    /// Analyzer that finds code of the form:
    /// <code>
    /// if (cond)
    /// {
    /// }
    /// NextStatement();
    /// </code>
    /// 
    /// And requires it to be of the form:
    /// <code>
    /// if (cond)
    /// {
    /// }
    /// 
    /// NextStatement();
    /// </code>
    /// 
    /// Specifically, all blocks followed by another statement must have a blank line between them.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpBlankLinesBetweenStatementsDiagnosticAnalyzer : AbstractBlankLinesBetweenStatementsDiagnosticAnalyzer
    {
        protected override void InitializeWorker(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        }

        private void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            // Don't examine broken blocks.
            var closeBrace = block.CloseBraceToken;
            if (closeBrace.IsMissing)
                return;

            // If the close brace itself doesn't have a newline, then ignore this.  This is a case of series of
            // statements on the same line.
            if (!closeBrace.TrailingTrivia.Any())
                return;

            if (closeBrace.TrailingTrivia.Last().Kind() != SyntaxKind.EndOfLineTrivia)
                return;

            // Grab whatever comes after the close brace.  If it's not the start of a statement, ignore it.
            var nextToken = closeBrace.GetNextToken();
            var nextTokenContainingStatement = nextToken.Parent!.FirstAncestorOrSelf<StatementSyntax>();
            if (nextTokenContainingStatement == null)
                return;

            if (nextToken != nextTokenContainingStatement.GetFirstToken())
                return;

            // There has to be at least a blank line between the end of the block and the start of the next statement.

            foreach (var trivia in nextToken.LeadingTrivia)
            {
                // If there's a blank line between the brace and the next token, we're all set.
                if (trivia.Kind() == SyntaxKind.EndOfLineTrivia)
                    return;

                if (trivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    continue;

                // got something that wasn't whitespace.  Bail out as we don't want to place any restrictions on this code.
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                closeBrace.GetLocation()));
        }
    }
}
