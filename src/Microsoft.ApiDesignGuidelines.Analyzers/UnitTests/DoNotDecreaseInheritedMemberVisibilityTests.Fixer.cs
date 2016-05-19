// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotDecreaseInheritedMemberVisibilityFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDecreaseInheritedMemberVisibilityAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDecreaseInheritedMemberVisibilityAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new DoNotDecreaseInheritedMemberVisibilityFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DoNotDecreaseInheritedMemberVisibilityFixer();
        }

        [Fact]
        public void CSharpDecreaseMemberVisibility()
        {
            VerifyCSharpFix(
// Original Code                
@"public class BaseClass
{
    public int MyMethod() { return 5; }
}

public class DerivedClass : BaseClass
{
    private new int MyMethod() { return 11; }
}",

// Fixed Code
@"public class BaseClass
{
    public int MyMethod() { return 5; }
}

public class DerivedClass : BaseClass
{
    public new int MyMethod() { return 11; }
}");

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    public int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    internal new int MyProperty { get; set; }
}",

// Fixed Code
@"public class BaseClass
{
    public int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    internal new int MyProperty { get; set; }
}"
);

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    public int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    public new int MyProperty { get; private set; }
}",

// Fixed Code
@"public class BaseClass
{
    public int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    public new int MyProperty { get; set; }
}");

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    public void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    private void MyMethod() {}
}",

// Fixed Code
@"public class BaseClass
{
    public void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    public void MyMethod() {}
}");

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    protected void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    private void MyMethod() {}
}",

// Fixed Code
@"public class BaseClass
{
    protected void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    protected void MyMethod() {}
}");

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    public int MyProperty { protected get; set; }
}

public class DerivedClass : BaseClass
{
    public new int MyProperty { private get; set; }
}",

// Fixed Code
@"public class BaseClass
{
    public int MyProperty { protected get; set; }
}

public class DerivedClass : BaseClass
{
    public new int MyProperty { protected get; set; }
}");

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    public int MyProperty { get; protected set; }
}

public class DerivedClass : BaseClass
{
    internal new int MyProperty { private get; set; }
}",

// Fixed Code
@"public class BaseClass
{
    public int MyProperty { get; protected set; }
}

public class DerivedClass : BaseClass
{
    internal new int MyProperty { get; set; }
}");

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    protected internal event System.EventHandler MyEvent { add{} remove{} }
    protected internal int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    internal new event System.EventHandler MyEvent { add{} remove{} }
    protected internal int MyProperty { internal get; set; }
}",

// Fixed Code
@"public class BaseClass
{
    protected internal event System.EventHandler MyEvent { add{} remove{} }
    protected internal int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    internal new event System.EventHandler MyEvent { add{} remove{} }
    protected internal int MyProperty { get; set; }
}");
        }

        [Fact]
        public void BasicDecreaseMemberVisibility()
        {
            VerifyBasicFix(
// Original Code
@"Public Class BaseClass
    Public Property MyProperty() As Integer
        Get
            Return m_MyField
        End Get
        Set
            m_MyField = Value
        End Set
    End Property
    Private m_MyField As Integer
End Class

Public Class DerivedClass
    Inherits BaseClass
    Public Shadows Property MyProperty() As Integer
        Get
            Return m_MyField
        End Get
        Private Set
            m_MyField = Value
        End Set
    End Property
    Private Shadows m_MyField As Integer
End Class",

// Fixed Code
@"Public Class BaseClass
    Public Property MyProperty() As Integer
        Get
            Return m_MyField
        End Get
        Set
            m_MyField = Value
        End Set
    End Property
    Private m_MyField As Integer
End Class

Public Class DerivedClass
    Inherits BaseClass
    Public Shadows Property MyProperty() As Integer
        Get
            Return m_MyField
        End Get
        Set
            m_MyField = Value
        End Set
    End Property
    Private Shadows m_MyField As Integer
End Class");

            VerifyBasicFix(
// Original Code
@"Public Class BaseClass
    Public Sub MyMethod()
    End Sub
End Class

Public Class DerivedClass
    Inherits BaseClass

    Private Sub MyMethod()
    End Sub
End Class",

// Fixed Code
@"Public Class BaseClass
    Public Sub MyMethod()
    End Sub
End Class

Public Class DerivedClass
    Inherits BaseClass

    Public Sub MyMethod()
    End Sub
End Class");

            VerifyBasicFix(
// Original Code
@"Public Class BaseClass
	Protected Friend Custom Event MyEvent As System.EventHandler
		AddHandler(ByVal value As System.EventHandler)
		End AddHandler
		RemoveHandler(ByVal value As System.EventHandler)
		End RemoveHandler
	End Event
	Protected Friend Property MyProperty() As Integer
		Get
			Return m_MyField
		End Get
		Set
			m_MyField = Value
		End Set
	End Property
	Private m_MyField As Integer
End Class

Public Class DerivedClass
	Inherits BaseClass
	Friend Shadows Custom Event MyEvent As System.EventHandler
		AddHandler(ByVal value As System.EventHandler)
		End AddHandler
		RemoveHandler(ByVal value As System.EventHandler)
		End RemoveHandler
	End Event
	Protected Friend Property MyProperty() As Integer
		Friend Get
			Return m_MyField
		End Get
		Set
			m_MyField = Value
		End Set
	End Property
	Private m_MyField As Integer
End Class",

// Fixed Code
@"Public Class BaseClass
	Protected Friend Custom Event MyEvent As System.EventHandler
		AddHandler(ByVal value As System.EventHandler)
		End AddHandler
		RemoveHandler(ByVal value As System.EventHandler)
		End RemoveHandler
	End Event
	Protected Friend Property MyProperty() As Integer
		Get
			Return m_MyField
		End Get
		Set
			m_MyField = Value
		End Set
	End Property
	Private m_MyField As Integer
End Class

Public Class DerivedClass
	Inherits BaseClass
	Friend Shadows Custom Event MyEvent As System.EventHandler
		AddHandler(ByVal value As System.EventHandler)
		End AddHandler
		RemoveHandler(ByVal value As System.EventHandler)
		End RemoveHandler
	End Event
	Protected Friend Property MyProperty() As Integer
		Get
			Return m_MyField
		End Get
		Set
			m_MyField = Value
		End Set
	End Property
	Private m_MyField As Integer
End Class");
        }

        [Fact]
        public void MultipleLevelsOfInheritance()
        {
            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    public int MyMethod() { return 5; }
}

public class DerivedClass : BaseClass
{
}

public class DerivedDerivedClass : DerivedClass
{
    private new int MyMethod() { return 11; }
}",

// Fixed Code
@"public class BaseClass
{
    public int MyMethod() { return 5; }
}

public class DerivedClass : BaseClass
{
}

public class DerivedDerivedClass : DerivedClass
{
    public new int MyMethod() { return 11; }
}");

            VerifyCSharpFix(
// Original Code
@"public class BaseClass
{
    public int MyProperty { protected get; set; }
}

public class DerivedClass : BaseClass
{
    protected new int MyProperty { get; private set; }
}

public class DerivedDerivedClass : DerivedClass
{
    internal new int MyProperty { private get; set; }
}",

// Fixed Code
@"public class BaseClass
{
    public int MyProperty { protected get; set; }
}

public class DerivedClass : BaseClass
{
    protected new int MyProperty { get; set; }
}

public class DerivedDerivedClass : DerivedClass
{
    internal new int MyProperty { get; set; }
}");
        }
    }
}