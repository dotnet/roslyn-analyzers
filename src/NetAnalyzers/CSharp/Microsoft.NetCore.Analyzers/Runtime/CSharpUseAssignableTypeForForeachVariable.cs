// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseAssignableTypeForForeachVariable : UseAssignableTypeForForeachVariable
    {
        protected override void AnalyzeLoop(OperationAnalysisContext context)
        {
            if (context.Operation is not IForEachLoopOperation loop || loop.Syntax is not CommonForEachStatementSyntax syntax)
            {
                return;
            }

            ForEachStatementInfo loopInfo = loop.SemanticModel.GetForEachStatementInfo(syntax);
            if (!loopInfo.CurrentConversion.Exists ||
                !loopInfo.CurrentConversion.IsImplicit ||
                !loopInfo.CurrentConversion.IsIdentity)
            {
                return;
            }

            ITypeSymbol collectionElementType = loopInfo.ElementType;
            if (collectionElementType.SpecialType == SpecialType.System_Object)
            {
                return;
            }

            ITypeSymbol? variableType = (loop.LoopControlVariable as IVariableDeclaratorOperation)?.Symbol?.Type;
            if (variableType == null)
            {
                return;
            }

            CommonConversion conversion = context.Compilation.ClassifyCommonConversion(collectionElementType, variableType);
            if (!conversion.Exists || conversion.IsImplicit)
            {
                return;
            }

            context.ReportDiagnostic(syntax.ForEachKeyword.CreateDiagnostic(Rule, collectionElementType.Name, variableType.Name));
        }
    }
}
