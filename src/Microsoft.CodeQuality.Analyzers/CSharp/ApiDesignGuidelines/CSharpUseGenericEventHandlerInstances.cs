﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpUseGenericEventHandlerInstancesAnalyzer : UseGenericEventHandlerInstancesAnalyzer
    {
        protected override SymbolAnalyzer GetAnalyzer(
            Compilation compilation,
            INamedTypeSymbol eventHandler,
            INamedTypeSymbol genericEventHandler,
            INamedTypeSymbol eventArgs,
            INamedTypeSymbol comSourceInterfacesAttribute)
        {
            return new CSharpSymbolAnalyzer(compilation, eventHandler, genericEventHandler, eventArgs, comSourceInterfacesAttribute);
        }

        private sealed class CSharpSymbolAnalyzer : SymbolAnalyzer
        {
            public CSharpSymbolAnalyzer(
                Compilation compilation,
                INamedTypeSymbol eventHandler,
                INamedTypeSymbol genericEventHandler,
                INamedTypeSymbol eventArgs,
                INamedTypeSymbol comSourceInterfacesAttribute)
                : base(compilation, eventHandler, genericEventHandler, eventArgs, comSourceInterfacesAttribute)
            {
            }

            protected override bool IsViolatingEventHandler(INamedTypeSymbol type)
            {
                return !IsValidLibraryEventHandlerInstance(type);
            }

            protected override bool IsAssignableTo(Compilation compilation, ITypeSymbol fromSymbol, ITypeSymbol toSymbol)
            {
                return
                    fromSymbol != null &&
                    toSymbol != null &&
                    ((CSharpCompilation)compilation).ClassifyConversion(fromSymbol, toSymbol).IsImplicit;
            }
        }
    }
}
