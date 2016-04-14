// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.QualityGuidelines.Analyzers.UnitTests
{
    public class SealMethodsThatSatisfyPrivateInterfacesFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SealMethodsThatSatisfyPrivateInterfacesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SealMethodsThatSatisfyPrivateInterfacesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new SealMethodsThatSatisfyPrivateInterfacesFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new SealMethodsThatSatisfyPrivateInterfacesFixer();
        }

        [Fact(Skip = "Enable this when roslyn is available with needed fix")]
        public void TestCSharp_OverriddenMethodChangedToSealed()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

public class C : B, IFace
{
    public override void M()
    {
    }
}",

@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

public class C : B, IFace
{
    public sealed override void M()
    {
    }
}");
        }

        [Fact]
        public void TestCSharp_VirtualMethodChangedToNotVirtual()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public class C : IFace
{
    public virtual void M()
    {
    }
}",

@"internal interface IFace
{
    void M();
}

public class C : IFace
{
    public void M()
    {
    }
}");
        }

        [Fact]
        public void TestCSharp_AbstractMethodChangedToNotAbstract()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public abstract class C : IFace
{
    public abstract void M();
}",

@"internal interface IFace
{
    void M();
}

public abstract class C : IFace
{
    public void M()
    {
    }
}");
        }

        [Fact]
        public void TestCSharp_ContainingTypeChangedToSealed()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

public class C : B, IFace
{
    public override void M()
    {
    }
}",

@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

public sealed class C : B, IFace
{
    public override void M()
    {
    }
}", codeFixIndex: 1);
        }

        [Fact]
        public void TestCSharp_ContainingTypeChangedToInternal()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

public class C : B, IFace
{
    public override void M()
    {
    }
}",

@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

internal class C : B, IFace
{
    public override void M()
    {
    }
}", codeFixIndex: 2);
        }

        [Fact]
        public void TestCSharp_AbstractContainingTypeChangedToInternal()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

public abstract class C : B, IFace
{
    public override void M()
    {
    }
}",

@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

internal abstract class C : B, IFace
{
    public override void M()
    {
    }
}", codeFixIndex: 1);  // sealed option is not available because class is abstract
        }

        [Fact]
        public void TestCSharp_ImplicitOverride_ContainingTypeChangedToSealed()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public class B
{
    public virtual void M()
    {
    }
}

public class C : B, IFace
{
}",

@"internal interface IFace
{
    void M();
}

public class B
{
    public virtual void M()
    {
    }
}

public sealed class C : B, IFace
{
}");
        }

        [Fact]
        public void TestCSharp_ImplicitOverride_ContainingTypeChangedToInternal()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public class B
{
    public virtual void M()
    {
    }
}

public class C : B, IFace
{
}",

@"internal interface IFace
{
    void M();
}

public class B
{
    public virtual void M()
    {
    }
}

internal class C : B, IFace
{
}", codeFixIndex: 1);
        }

        [Fact]
        public void TestCSharp_ImplicitOverride_AbstractContainingTypeChangedToInternal()
        {
            VerifyCSharpFix(
@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

public abstract class C : B, IFace
{
}",

@"internal interface IFace
{
    void M();
}

public abstract class B
{
    public abstract void M();
}

internal abstract class C : B, IFace
{
}", codeFixIndex: 0); // sealed option is not available because type is abstract
        }

        [Fact(Skip = "Enable this when roslyn is available with needed fix")]
        public void TestBasic_OverriddenMethodChangedToSealed()
        {
            VerifyBasicFix(
@"Friend Interface IFace
    Sub M()
End Interface

Public MustInherit Class B
    Public MustOverride Sub M()
End Class

Public Class C
    Inherits B
    Implements IFace

    Public Overrides Sub M() Implements IFace.M
    End Sub
End Class",

@"Friend Interface IFace
    Sub M()
End Interface

Public MustInherit Class B
    Public MustOverride Sub M()
End Class

Public Class C
    Inherits B
    Implements IFace

    Public NotOverridable Overrides Sub M() Implements IFace.M
    End Sub
End Class");
        }

        [Fact(Skip = "Enable this when roslyn is available with needed fix")]
        public void TestBasic_VirtualMethodChangedToNotVirtual()
        {
            VerifyBasicFix(
@"Friend Interface IFace
    Sub M()
End Interface

Public Class C
    Implements IFace

    Public Overridable Sub M() Implements IFace.M
    End Sub
End Class",

@"Friend Interface IFace
    Sub M()
End Interface

Public Class C
    Implements IFace

    Public Sub M() Implements IFace.M
    End Sub
End Class");
        }

        [Fact(Skip = "Enable this when roslyn is available with needed fix")]
        public void TestBasic_AbstractMethodChangedToNotAbstract()
        {
            VerifyBasicFix(
@"Friend Interface IFace
    Sub M()
End Interface

Public MustInherit Class C
    Implements IFace

    Public MustOverride Sub M() Implements IFace.M
End Class",

@"Friend Interface IFace
    Sub M()
End Interface

Public MustInherit Class C
    Implements IFace

    Public Sub M() Implements IFace.M
    End Sub
End Class");
        }
    }
}