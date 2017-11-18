using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeQuality.Analyzers.Maintainability;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{

    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpUseNameofInPlaceOfStringFixer : UseNameOfInPlaceOfStringFixer
    {
        internal override SyntaxNode GetNameOfExpression(string stringText)
        {
            return InvocationExpression(
                expression: IdentifierName("nameof"),
                argumentList: ArgumentList(
                    arguments: SingletonSeparatedList(Argument(IdentifierName(stringText)))));
        }
    }
}
