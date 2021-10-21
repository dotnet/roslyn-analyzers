// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotPassMutableValueTypesByValueAnalyzer : DoNotPassMutableValueTypesByValueAnalyzer
    {
        private protected override IEnumerable<Location> GetMethodReturnTypeLocations(IMethodSymbol methodSymbol, CancellationToken token)
        {
            return methodSymbol.DeclaringSyntaxReferences.Select(syntaxReference =>
            {
                var node = (MethodDeclarationSyntax)syntaxReference.GetSyntax(token);
                return node.ReturnType.GetLocation();
            });
        }

        private protected override IEnumerable<Location> GetPropertyReturnTypeLocations(IPropertySymbol propertySymbol, CancellationToken token)
        {
            return propertySymbol.DeclaringSyntaxReferences.Select(syntaxReference =>
            {
                var node = (PropertyDeclarationSyntax)syntaxReference.GetSyntax(token);
                return node.Type.GetLocation();
            });
        }
    }
}
