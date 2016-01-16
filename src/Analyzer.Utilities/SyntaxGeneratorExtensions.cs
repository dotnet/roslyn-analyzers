using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Analyzer.Utilities
{
    public static class SyntaxGeneratorExtensions
    {
        public static SyntaxNode OperatorDeclaration(
            this SyntaxGenerator generator,
            string language,
            SyntaxNode returnType,
            string operatorName,
            SyntaxNode[] parameters,
            SyntaxNode statement)
        {
            switch (language)
            {
                case LanguageNames.CSharp:
                    return generator.CSharpOperatorDeclaration(returnType, operatorName, parameters, statement);

                case LanguageNames.VisualBasic:
                    return generator.BasicOperatorDeclaration(returnType, operatorName, parameters, statement);

                default:
                    throw new ArgumentException($"Invalid language name: {language}", nameof(language));
            }
        }
    }
}
