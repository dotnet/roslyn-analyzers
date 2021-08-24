// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    public class CSharpDetectPreviewFeatureAnalyzer : DetectPreviewFeatureAnalyzer
    {
        protected override SyntaxNode? GetPreviewInterfaceNodeForTypeImplementingPreviewInterface(ISymbol typeSymbol, ISymbol previewInterfaceSymbol)
        {
            SyntaxNode? ret = null;
            ImmutableArray<SyntaxReference> typeSymbolDeclaringReferences = typeSymbol.DeclaringSyntaxReferences;

            foreach (SyntaxReference? syntaxReference in typeSymbolDeclaringReferences)
            {
                SyntaxNode typeSymbolDefinition = syntaxReference.GetSyntax();
                if (typeSymbolDefinition is ClassDeclarationSyntax classDeclaration)
                {
                    SeparatedSyntaxList<BaseTypeSyntax> baseListTypes = classDeclaration.BaseList.Types;
                    ret = GetPreviewInterfaceNodeForClassOrStructImplementingPreviewInterface(baseListTypes, previewInterfaceSymbol);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                else if (typeSymbolDefinition is StructDeclarationSyntax structDeclaration)
                {
                    SeparatedSyntaxList<BaseTypeSyntax> baseListTypes = structDeclaration.BaseList.Types;
                    ret = GetPreviewInterfaceNodeForClassOrStructImplementingPreviewInterface(baseListTypes, previewInterfaceSymbol);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
            }

            return ret;
        }

        private SyntaxNode? GetPreviewInterfaceNodeForClassOrStructImplementingPreviewInterface(SeparatedSyntaxList<BaseTypeSyntax> baseListTypes, ISymbol previewInterfaceSymbol)
        {
            foreach (BaseTypeSyntax baseTypeSyntax in baseListTypes)
            {
                if (baseTypeSyntax is SimpleBaseTypeSyntax simpleBaseTypeSyntax)
                {
                    TypeSyntax type = simpleBaseTypeSyntax.Type;
                    if (type is IdentifierNameSyntax identifier)
                    {
                        if (identifier.Identifier.ValueText == previewInterfaceSymbol.Name)
                        {
                            return simpleBaseTypeSyntax;
                        }
                    }
                }
            }

            return null;
        }
    }
}
