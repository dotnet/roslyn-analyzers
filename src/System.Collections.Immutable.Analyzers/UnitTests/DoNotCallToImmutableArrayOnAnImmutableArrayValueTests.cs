// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Collections.Immutable.Analyzers.UnitTests
{
    public class DoNotCallToImmutableArrayOnAnImmutableArrayValueTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer();
        }

        #region No Diagnostic Tests

        [Fact]
        public void NoDiagnosticCases()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Collections.Immutable;
using static System.Collections.Immutable.ImmutableArray;

class C
{
    public static ImmutableArray<TSource> ToImmutableArray<TSource>(this IEnumerable<TSource> items)
    {
        return null;
    }

    public void M(IEnumerable<int> p1, List<int> p2, ImmutableArray<int> p3)
    {
        // Allowed
        p1.ToImmutableArray();
        p2.ToImmutableArray();

        // No dataflow
        IEnumerable<int> l1 = p3;
        l1.ToImmutableArray();
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic
Imports System.Collections.Immutable

Class C
	<System.Runtime.CompilerServices.Extension> _
	Public Shared Function ToImmutableArray(Of TSource)(items As IEnumerable(Of TSource)) As ImmutableArray(Of TSource)
		Return Nothing
	End Function

	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As ImmutableArray(Of Integer))
		' Allowed
		p1.ToImmutableArray()
		p2.ToImmutableArray()

		' No dataflow
		Dim l1 As IEnumerable(Of Integer) = p3
		l1.ToImmutableArray()
	End Sub
End Class
");
        }

        #endregion

        #region Diagnostic Tests

        [Fact]
        public void DiagnosticCases()
        {
            VerifyCSharp(@"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    public static ImmutableArray<TSource> ToImmutableArray<TSource>(this IEnumerable<TSource> items)
    {
        return null;
    }

    public void M(IEnumerable<int> p1, List<int> p2, ImmutableArray<int> p3)
    {
        p1.ToImmutableArray().ToImmutableArray();
        p3.ToImmutableArray();
    }
}
",
    // Test0.cs(14,9): warning RS0012: Do not call ToImmutableArray on an ImmutableArray value
    GetCSharpResultAt(14, 9),
    // Test0.cs(15,9): warning RS0012: Do not call ToImmutableArray on an ImmutableArray value
    GetCSharpResultAt(15, 9));

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic
Imports System.Collections.Immutable

Class C
	<System.Runtime.CompilerServices.Extension> _
	Public Shared Function ToImmutableArray(Of TSource)(items As IEnumerable(Of TSource)) As ImmutableArray(Of TSource)
		Return Nothing
	End Function

	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As ImmutableArray(Of Integer))
		p1.ToImmutableArray().ToImmutableArray()
		p3.ToImmutableArray()
	End Sub
End Class
",
    // Test0.vb(13,3): warning RS0012: Do not call ToImmutableArray on an ImmutableArray value
    GetBasicResultAt(13, 3),
    // Test0.vb(14,3): warning RS0012: Do not call ToImmutableArray on an ImmutableArray value
    GetBasicResultAt(14, 3));
        }

        #endregion

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer.RuleId, SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableArrayOnAnImmutableArrayValueMessage);
        }

        private static DiagnosticResult GetBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer.RuleId, SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableArrayOnAnImmutableArrayValueMessage);
        }
    }
}