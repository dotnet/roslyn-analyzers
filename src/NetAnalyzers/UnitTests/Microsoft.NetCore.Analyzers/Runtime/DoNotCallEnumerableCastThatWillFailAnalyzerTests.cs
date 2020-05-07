// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotCallEnumerableCastThatWillFailAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotCallEnumerableCastThatWillFailAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotCallEnumerableCastThatWillFailAnalyzerTests
    {
        [Fact]
        public async Task DiagnosticCasesCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Linq;

class C
{
    public void M()
    {
        var a = (new int[0]).OfType<object>();
        var b = (new int[0]).OfType<string>();
        var c = (new object[0]).OfType<string>();
    }
}
",
                // Test0.cs(18,9): warning CA2009: Do not call ToImmutableCollection on an ImmutableCollection value
                GetCSharpResultAt(8, 17, "OfType"));

        }

        [Fact]
        public async Task DiagnosticCasesVB()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Linq

Interface IApple
End Interface

Class Fruit
End Class

Public Class Orange
    Inherits Fruit
End Class

Class Apple
    Inherits Fruit
    Implements IApple
End Class

NotInheritable Class Salad
End Class

Module M    
	Sub S
		Dim a = (New Integer(){}).OfType(Of Object)
        Dim b = (New Object(){}).OfType(Of String)
        Dim c = (New Object(){}).OfType(Of Integer)
        Dim d = (New Integer(){}).OfType(Of String)
        Dim e = (New String(){}).OfType(Of Integer)
        
        Dim f = (New Object(){}).OfType(Of Fruit)
        Dim g = (New Object(){}).OfType(Of Orange)
        Dim h = (New Object(){}).OfType(Of IApple)
        Dim i = (New Object(){}).OfType(Of Apple)
        Dim z1 = (New Object(){}).OfType(Of Salad)

        Dim j = (New Fruit(){}).OfType(Of Fruit)
        Dim k = (New Fruit(){}).OfType(Of Orange)
        Dim l = (New Fruit(){}).OfType(Of IApple)
        Dim m = (New Fruit(){}).OfType(Of Apple)
        Dim z2 = (New Fruit(){}).OfType(Of Salad)

        Dim n = (New Orange(){}).OfType(Of Fruit)
        Dim o = (New Orange(){}).OfType(Of Orange)
        Dim p = (New Orange(){}).OfType(Of IApple)
        Dim q = (New Orange(){}).OfType(Of Apple)
        Dim z3 = (New Orange(){}).OfType(Of Salad)
        
        Dim r = (New IApple(){}).OfType(Of Fruit)
        Dim s = (New IApple(){}).OfType(Of Orange)
        Dim t = (New IApple(){}).OfType(Of IApple)
        Dim u = (New IApple(){}).OfType(Of Apple)
        Dim z4 = (New IApple(){}).OfType(Of Salad)
        
        Dim v = (New Apple(){}).OfType(Of Fruit)
        Dim w = (New Apple(){}).OfType(Of Orange)
        Dim x = (New Apple(){}).OfType(Of IApple)
        Dim y = (New Apple(){}).OfType(Of Apple)
        Dim z5 = (New Apple(){}).OfType(Of Salad)
	End Sub
End Module
",
    // Test0.cs(18,9): warning CA2009: Do not call ToImmutableCollection on an ImmutableCollection value
    GetBasicResultAt(8, 17, "OfType"));

        }



        private static DiagnosticResult GetCSharpResultAt(int line, int column, string methodName)
        {
            return VerifyCS.Diagnostic(DoNotCallEnumerableCastThatWillFailAnalyzer.Rule).WithLocation(line, column).WithArguments(methodName, methodName);
        }

        private static DiagnosticResult GetBasicResultAt(int line, int column, string methodName)
        {
            return VerifyVB.Diagnostic(DoNotCallEnumerableCastThatWillFailAnalyzer.Rule).WithLocation(line, column).WithArguments(methodName, methodName);
        }
    }
}
