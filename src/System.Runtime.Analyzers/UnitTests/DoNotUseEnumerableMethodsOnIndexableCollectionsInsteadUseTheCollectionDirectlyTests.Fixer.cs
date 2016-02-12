// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;

namespace System.Runtime.Analyzers.UnitTests
{
    public class DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer();
        }
    }
}