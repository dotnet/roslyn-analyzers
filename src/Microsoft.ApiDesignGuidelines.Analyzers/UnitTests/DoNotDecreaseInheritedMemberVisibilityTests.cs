// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Diagnostics.Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotDecreaseInheritedMemberVisibilityTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDecreaseInheritedMemberVisibilityAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDecreaseInheritedMemberVisibilityAnalyzer();
        }

        [Fact]
        public void DecreaseCSharpMemberVisibility()
        {
            VerifyCSharp(
@"public class BaseClass
{
    public int MyMethod() { return 5; }
}

public class DerivedClass : BaseClass
{
    private new int MyMethod() { return 5; }
}", GetCSharpCA2222RuleNameResultAt(8, 21));

            VerifyCSharp(
@"public class BaseClass
{
    public int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    internal new int MyProperty { get; set; }
}", GetCSharpCA2222RuleNameResultAt(8, 35), GetCSharpCA2222RuleNameResultAt(8, 40));

            VerifyCSharp(
@"public class BaseClass
{
    public int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    public new int MyProperty { get; private set; }
}", GetCSharpCA2222RuleNameResultAt(8, 46));

            VerifyCSharp(
@"public class BaseClass
{
    protected internal void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    internal void MyMethod() {}
}", GetCSharpCA2222RuleNameResultAt(8, 19));
        }

        [Fact]
        public void DecreaseBasicMemberVisibility()
        {
            VerifyBasic(
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
End Class", GetBasicCA2222RuleNameResultAt(19, 17));

            VerifyBasic(
@"Public Class BaseClass
    Public Sub MyMethod()
    End Sub
End Class

Public Class DerivedClass
    Inherits BaseClass
    Private Overloads Sub MyMethod()
    End Sub
End Class", GetBasicCA2222RuleNameResultAt(8, 27));
        }

        [Fact]
        public void IncreaseCSharpMemberVisibility()
        {
            VerifyCSharp(
@"public class BaseClass
{
    internal void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    public new void MyMethod() {}
}");
        }

        [Fact]
        public void DecreaseCSharpMemberVisibilityInOverride()
        {
            VerifyCSharp(
@"public class BaseClass
{
    public virtual void MyMethod() {}

    public virtual int MyProperty { get; set; }
}

public class DerivedClass : BaseClass
{
    internal override void MyMethod() {}

    public override int MyProperty { private get; set; }
}");
        }

        [Fact]
        public void IncreaseBasicMemberVisibility()
        {
            VerifyBasic(
@"Public Class BaseClass
    Private Sub MyMethod()
    End Sub
End Class

Public Class DerivedClass
    Inherits BaseClass
    Public Overloads Sub MyMethod()
    End Sub
End Class");
        }

        [Fact]
        public void SimilarCSharpVisibilities()
        {
            VerifyCSharp(
@"public class BaseClass
{
    public int MyProperty {get;}
}

public class DerivedClass : BaseClass
{
    public new int MyProperty {get;}
}");

            VerifyCSharp(
@"public class BaseClass
{
    internal void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    protected void MyMethod() {}
}");

            VerifyCSharp(
@"public class BaseClass
{
    public void MyMethod() {}
}

public class DerivedClass : BaseClass
{
    protected void MyMethod() {}
}");
        }

        [Fact]
        public void SimilarBasicVisibilities()
        {
            VerifyBasic(
@"Public Class BaseClass
    Friend Sub MyMethod()
    End Sub
End Class

Public Class DerivedClass
    Inherits BaseClass
    Protected Overloads Sub MyMethod()
    End Sub
End Class");

            VerifyBasic(
@"Public Class BaseClass
    Public Sub MyMethod()
    End Sub
End Class

Public Class DerivedClass
    Inherits BaseClass
    Protected Overloads Sub MyMethod()
    End Sub
End Class");
        }

        [Fact]
        public void MultipleLevelsOfInheritance()
        {
            VerifyCSharp(
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
}", GetCSharpCA2222RuleNameResultAt(12, 21));
        }

        [Fact]
        public void SealedTypes()
        {
            VerifyCSharp(
@"public class BaseClass
{
    public int MyProperty { get; set; }
    public void MyMethod() { }
}

public sealed class DerivedClass : BaseClass
{
    private new int MyProperty { get; set; }
    protected new void MyMethod() { }
}");
        }

        [Fact, WorkItem(940, "https://github.com/dotnet/roslyn-analyzers/issues/940")]
        public void MembersInsideTypesThatAreNotPublicallyVisible()
        {
            VerifyCSharp(
@"public class OuterClass
{
    public class NestedBaseClass
    {
        public NestedBaseClass(int x) { }
        public int MyProperty { get; set; }
        public void MyMethod() { }
    }

    public sealed class NestedSealedPublicDerivedClass : NestedBaseClass
    {
        private NestedSealedPublicDerivedClass(int x) : base(x) { }
        private new int MyProperty { get; set; }
        private new void MyMethod() { }
    }

    private sealed class NestedSealedPrivateDerivedClass : NestedBaseClass
    {
        private NestedSealedPrivateDerivedClass(int x) : base(x) { }
        private new int MyProperty { get; set; }
        private new void MyMethod() { }
    }

    private class NestedUnsealedPrivateDerivedClass : NestedBaseClass
    {
        private NestedUnsealedPrivateDerivedClass(int x) : base(x) { }
        private new int MyProperty { get; set; }
        private new void MyMethod() { }
    }
}");
            VerifyBasic(
@"Public Class OuterClass
	Public Class NestedBaseClass
		Public Sub New(x As Integer)
		End Sub
		Public Property MyProperty() As Integer
			Get
				Return m_MyProperty
			End Get
			Set
				m_MyProperty = Value
			End Set
		End Property
		Private m_MyProperty As Integer
		Public Sub MyMethod()
		End Sub
	End Class

	Public NotInheritable Class NestedSealedPublicDerivedClass
		Inherits NestedBaseClass
		Private Sub New(x As Integer)
			MyBase.New(x)
		End Sub
		Private Shadows Property MyProperty() As Integer
			Get
				Return m_MyProperty
			End Get
			Set
				m_MyProperty = Value
			End Set
		End Property
		Private Shadows m_MyProperty As Integer
		Private Shadows Sub MyMethod()
		End Sub
	End Class

	Private NotInheritable Class NestedSealedPrivateDerivedClass
		Inherits NestedBaseClass
		Private Sub New(x As Integer)
			MyBase.New(x)
		End Sub
		Private Shadows Property MyProperty() As Integer
			Get
				Return m_MyProperty
			End Get
			Set
				m_MyProperty = Value
			End Set
		End Property
		Private Shadows m_MyProperty As Integer
		Private Shadows Sub MyMethod()
		End Sub
	End Class

	Private Class NestedUnsealedPrivateDerivedClass
		Inherits NestedBaseClass
		Private Sub New(x As Integer)
			MyBase.New(x)
		End Sub
		Private Shadows Property MyProperty() As Integer
			Get
				Return m_MyProperty
			End Get
			Set
				m_MyProperty = Value
			End Set
		End Property
		Private Shadows m_MyProperty As Integer
		Private Shadows Sub MyMethod()
		End Sub
	End Class
End Class");
        }

        private DiagnosticResult GetCSharpCA2222RuleNameResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, DoNotDecreaseInheritedMemberVisibilityAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDecreaseInheritedMemberVisibilityMessage);
        }

        private DiagnosticResult GetBasicCA2222RuleNameResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, DoNotDecreaseInheritedMemberVisibilityAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDecreaseInheritedMemberVisibilityMessage);
        }
    }
}