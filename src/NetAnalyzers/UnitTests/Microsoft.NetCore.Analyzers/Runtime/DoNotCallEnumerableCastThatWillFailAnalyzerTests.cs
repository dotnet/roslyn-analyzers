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
            var ofTypeRule = DoNotCallEnumerableCastThatWillFailAnalyzer.OfTypeRule;

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
    // /0/Test0.cs(9,17): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyCS.Diagnostic(ofTypeRule).WithSpan(9, 17, 9, 46).WithArguments("int", "string")
    );
        }

        [Fact]
        public async Task DiagnosticCasesVB()
        {
            var castRule = DoNotCallEnumerableCastThatWillFailAnalyzer.CastRule;

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Linq

Interface IApple
End Interface

Public Class Fruit
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
		Dim a = (New Integer(){}).Cast(Of Object)
        Dim b = (New Object(){}).Cast(Of String)
        Dim c = (New Object(){}).Cast(Of Integer)
        Dim d = (New Integer(){}).Cast(Of String)
        Dim e = (New String(){}).Cast(Of Integer)
        
        Dim f = (New Object(){}).Cast(Of Fruit)
        Dim g = (New Object(){}).Cast(Of Orange)
        Dim h = (New Object(){}).Cast(Of IApple)
        Dim i = (New Object(){}).Cast(Of Apple)
        Dim z1 = (New Object(){}).Cast(Of Salad)

        Dim j = (New Fruit(){}).Cast(Of Fruit)
        Dim k = (New Fruit(){}).Cast(Of Orange)
        Dim l = (New Fruit(){}).Cast(Of IApple)
        Dim m = (New Fruit(){}).Cast(Of Apple)
        Dim z2 = (New Fruit(){}).Cast(Of Salad)

        Dim n = (New Orange(){}).Cast(Of Fruit)
        Dim o = (New Orange(){}).Cast(Of Orange)
        Dim p = (New Orange(){}).Cast(Of IApple)
        Dim q = (New Orange(){}).Cast(Of Apple)
        Dim z3 = (New Orange(){}).Cast(Of Salad)
        
        Dim r = (New IApple(){}).Cast(Of Fruit)
        Dim s = (New IApple(){}).Cast(Of Orange)
        Dim t = (New IApple(){}).Cast(Of IApple)
        Dim u = (New IApple(){}).Cast(Of Apple)
        Dim z4 = (New IApple(){}).Cast(Of Salad)
        
        Dim v = (New Apple(){}).Cast(Of Fruit)
        Dim w = (New Apple(){}).Cast(Of Orange)
        Dim x = (New Apple(){}).Cast(Of IApple)
        Dim y = (New Apple(){}).Cast(Of Apple)
        Dim z5 = (New Apple(){}).Cast(Of Salad)
	End Sub
End Module
",
    // /0/Test0.vb(26, 17): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(26, 17, 26, 50).WithArguments("Object", "Integer"),
    // /0/Test0.vb(27,17): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(27, 17, 27, 50).WithArguments("Integer", "String"),
    // /0/Test0.vb(28,17): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(28, 17, 28, 50).WithArguments("String", "Integer"),
    // /0/Test0.vb(40,18): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(40, 18, 40, 48).WithArguments("Fruit", "Salad"),
    // /0/Test0.vb(45,17): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(45, 17, 45, 48).WithArguments("Orange", "Apple"),
    // /0/Test0.vb(46,18): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(46, 18, 46, 49).WithArguments("Orange", "Salad"),
    // /0/Test0.vb(52,18): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(52, 18, 52, 49).WithArguments("IApple", "Salad"),
    // /0/Test0.vb(55,17): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(55, 17, 55, 48).WithArguments("Apple", "Orange"),
    // /0/Test0.vb(58,18): info CA9999: If the source sequence contains any elements, the sequence returned by Cast will throw InvalidCastException at runtime when enumerated.
    VerifyVB.Diagnostic(castRule).WithSpan(58, 18, 58, 48).WithArguments("Apple", "Salad")
   );
        }
    }
}
