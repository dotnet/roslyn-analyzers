// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{                          
    /// <summary>
    /// CA1032: Implement standard exception constructors
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpImplementStandardExceptionConstructorsAnalyzer : ImplementStandardExceptionConstructorsAnalyzer
    {
        protected override string GetParameterLessConstructorMessage(ISymbol symbol)
        {
            return $"public {symbol.Name}()";
        }

        protected override string GetConstructorWithStringTypeParameter(ISymbol symbol)
        {
            return $"public {symbol.Name}(string message)";
        }

        protected override string GetConstructorWithStringAndExceptionTypeParameter(ISymbol symbol)
        {
            return $"public {symbol.Name}(string message, Exception innerException)";
        }
        
    }
}