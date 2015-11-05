// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class InitializeValueTypeStaticFieldsInlineTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicInitializeStaticFieldsInlineAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpInitializeStaticFieldsInlineAnalyzer();
        }

        [Fact]
        public void CSharp_CA1810_NoDiagnostic_EmptyStaticConstructor()
        {
            VerifyCSharp(@"
public class Class1
{
    private readonly static int field = 1;
    static Class1() // Empty
    {
    }
}
");
        }

        [Fact]
        public void CSharp_CA2207_NoDiagnostic_EmptyStaticConstructor()
        {
            VerifyCSharp(@"
public struct Struct1
{
    private readonly static int field = 1;
    static Struct1() // Empty
    {
    }
}
");
        }

        [Fact]
        public void CSharp_CA1810_NoDiagnostic_NoStaticFieldInitializedInStaticConstructor()
        {
            VerifyCSharp(@"
public class Class1
{
    private readonly static int field = 1;
    static Class1() // No static field initalization
    {
        Class1_Method();
        var field2 = 1;
    }
}
");
        }

        [Fact]
        public void CSharp_CA1810_NoDiagnostic_StaticPropertyInStaticConstructor()
        {
            VerifyCSharp(@"
public class Class1
{
    private static int Property { get { return 0; } }

    static Class1() // Static property initalization
    {
        Property = 1;
    }
}
");
        }

        [Fact]
        public void CSharp_CA1810_NoDiagnostic_InitializionInNonStaticConstructor()
        {
            VerifyCSharp(@"
public class Class1
{
    private readonly static int field = 1;
    public Class1() // Non static constructor
    {
        field = 0;
    }

    public static void Class1_Method() // Non constructor
    {
        field = 0;
    }
}
");
        }

        [Fact]
        public void CSharp_CA1810_Diagnostic_InitializationInStaticConstructor()
        {
            VerifyCSharp(@"
public class Class1
{
    private readonly static int field;
    static Class1() // Non static constructor
    {
        field = 0;
    }
}
",
    GetCA1810CSharpDefaultResultAt(5, 12, "Class1"));

        }

        [Fact]
        public void CSharp_CA2207_Diagnostic_InitializationInStaticConstructor()
        {
            VerifyCSharp(@"
public struct Struct1
{
    private readonly static int field;
    static Struct1() // Non static constructor
    {
        field = 0;
    }
}
",
    GetCA2207CSharpDefaultResultAt(5, 12, "Struct1"));

        }

        [Fact]
        public void CSharp_CA1810_Diagnostic_NoDuplicteDiagnostics()
        {
            VerifyCSharp(@"
public class Class1
{
    private readonly static int field, field2;
    static Class1() // Non static constructor
    {
        field = 0;
        field2 = 0;
    }
}
",
    GetCA1810CSharpDefaultResultAt(5, 12, "Class1"));

        }

        [Fact]
        public void CSharp_CA2207_Diagnostic_NoDuplicteDiagnostics()
        {
            VerifyCSharp(@"
public struct Struct1
{
    private readonly static int field, field2;
    static Struct1() // Non static constructor
    {
        field = 0;
        field2 = 0;
    }
}
",
    GetCA2207CSharpDefaultResultAt(5, 12, "Struct1"));

        }

        [Fact]
        public void VisualBasic_CA1810_NoDiagnostic_EmptyStaticConstructor()
        {
            VerifyBasic(@"
Public Class Class1
	Private Shared ReadOnly field As Integer = 1
	Shared Sub New() ' Empty
	End Sub
End Class
");
        }

        [Fact]
        public void VisualBasic_CA2207_NoDiagnostic_EmptyStaticConstructor()
        {
            VerifyBasic(@"
Public Structure Struct1
	Private Shared ReadOnly field As Integer = 1
	Shared Sub New() ' Empty
	End Sub
End Structure
");
        }

        [Fact]
        public void VisualBasic_CA1810_NoDiagnostic_NoStaticFieldInitializedInStaticConstructor()
        {
            VerifyBasic(@"
Public Class Class1
	Private Shared ReadOnly field As Integer = 1
	Shared Sub New() ' No static field initalization
		Class1_Method()
		Dim field2 = 1
	End Sub
End Class
");
        }

        [Fact]
        public void Basic_CA1810_NoDiagnostic_StaticPropertyInStaticConstructor()
        {
            VerifyBasic(@"
Public Class Class1
	Private Shared ReadOnly Property [Property]() As Integer
		Get
			Return 0
		End Get
	End Property

	Shared Sub New()
		' Static property initalization
		[Property] = 1
	End Sub
End Class
");
        }

        [Fact]
        public void Basic_CA1810_NoDiagnostic_InitializionInNonStaticConstructor()
        {
            VerifyBasic (@"
Public Class Class1
	Private Shared ReadOnly field As Integer = 1
	Public Sub New() ' Non static constructor
		field = 0
	End Sub

	Public Shared Sub Class1_Method() ' Non constructor
		field = 0
	End Sub
End Class
");
        }

        [Fact]
        public void Basic_CA1810_Diagnostic_InitializationInStaticConstructor()
        {
            VerifyBasic(@"
Public Class Class1
	Private Shared ReadOnly field As Integer
	Shared Sub New()
		' Non static constructor
		field = 0
	End Sub
End Class
",
    GetCA1810BasicDefaultResultAt(4, 13, "Class1"));

        }

        [Fact]
        public void Basic_CA2207_Diagnostic_InitializationInStaticConstructor()
        {
            VerifyBasic(@"
Public Structure Struct1
	Private Shared ReadOnly field As Integer
	Shared Sub New()
		' Non static constructor
		field = 0
	End Sub
End Structure
",
    GetCA2207BasicDefaultResultAt(4, 13, "Struct1"));

        }

        [Fact]
        public void Basic_CA1810_Diagnostic_NoDuplicteDiagnostics()
        {
            VerifyBasic(@"
Public Class Class1
	Private Shared ReadOnly field As Integer, field2 As Integer
	Shared Sub New()
		' Non static constructor
		field = 0
		field2 = 0
	End Sub
End Class",
    GetCA1810BasicDefaultResultAt(4, 13, "Class1"));

        }

        [Fact]
        public void Basic_CA2207_Diagnostic_NoDuplicteDiagnostics()
        {
            VerifyBasic(@"
Public Structure Struct1
	Private Shared ReadOnly field As Integer, field2 As Integer
	Shared Sub New()
		' Non static constructor
		field = 0
		field2 = 0
	End Sub
End Structure",
    GetCA2207BasicDefaultResultAt(4, 13, "Struct1"));

        }

        #region Helpers

        private static DiagnosticResult GetCA1810CSharpDefaultResultAt(int line, int column, string typeName)
        {
            var message = string.Format(SystemRuntimeAnalyzersResources.InitializeStaticFieldsInlineMessage, typeName);
            return GetCSharpResultAt(line, column, CSharpInitializeStaticFieldsInlineAnalyzer.CA1810RuleId, message);
        }

        private static DiagnosticResult GetCA1810BasicDefaultResultAt(int line, int column, string typeName)
        {
            var message = string.Format(SystemRuntimeAnalyzersResources.InitializeStaticFieldsInlineMessage, typeName);
            return GetBasicResultAt(line, column, BasicInitializeStaticFieldsInlineAnalyzer.CA1810RuleId, message);
        }

        private static DiagnosticResult GetCA2207CSharpDefaultResultAt(int line, int column, string typeName)
        {
            var message = string.Format(SystemRuntimeAnalyzersResources.InitializeStaticFieldsInlineMessage, typeName);
            return GetCSharpResultAt(line, column, CSharpInitializeStaticFieldsInlineAnalyzer.CA2207RuleId, message);
        }

        private static DiagnosticResult GetCA2207BasicDefaultResultAt(int line, int column, string typeName)
        {
            var message = string.Format(SystemRuntimeAnalyzersResources.InitializeStaticFieldsInlineMessage, typeName);
            return GetBasicResultAt(line, column, BasicInitializeStaticFieldsInlineAnalyzer.CA2207RuleId, message);
        }

        #endregion
    }
}