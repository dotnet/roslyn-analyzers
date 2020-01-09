// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Test.Utilities;
using Xunit;

namespace MetaCompilation.Analyzers.UnitTests
{
    public abstract class CodeRefactoringVerifier : DiagnosticVerifier
    {
        protected abstract CodeRefactoringProvider GetCodeRefactoringProvider();

        protected async Task VerifyRefactoringAsync(string sourceWithMarkup, string expected, CancellationToken cancellationToken = default)
        {
            MarkupTestFile.GetPositionAndSpan(sourceWithMarkup, out string source, cursorPosition: out _, out TextSpan? textSpan);
            Assert.True(textSpan.HasValue, "Input source is missing span markup '[||]'");

            var document = CreateDocument(source, LanguageNames.CSharp);
            var actions = new List<CodeAction>();
            var context = new CodeRefactoringContext(document, textSpan.Value, (a) => actions.Add(a), cancellationToken);
            var codeRefactoringProvider = GetCodeRefactoringProvider();
            await codeRefactoringProvider.ComputeRefactoringsAsync(context);
            var codeAction = actions.First();
            var operations = await codeAction.GetOperationsAsync(cancellationToken);
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            document = solution.GetDocument(document.Id);

            var newDocumentString = getStringFromDocument(document);
            Assert.Equal(expected, newDocumentString);

            static string getStringFromDocument(Document document)
            {
                var simplifiedDoc = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
                var root = simplifiedDoc.GetSyntaxRootAsync().Result;
                root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
                return root.GetText().ToString();
            }
        }
    }
}
