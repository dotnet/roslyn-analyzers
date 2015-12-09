// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.QualityGuidelines.Analyzers.UnitTests
{
    public class SealMethodsThatSatisfyPrivateInterfacesFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicSealMethodsThatSatisfyPrivateInterfacesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpSealMethodsThatSatisfyPrivateInterfacesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicSealMethodsThatSatisfyPrivateInterfacesFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpSealMethodsThatSatisfyPrivateInterfacesFixer();
        }

// Enable these tests when roslyn is available with needed fix.
#if false 
        [Fact]
        public void TestCSharp_OverridenMethodChangedToSealed()
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

public abstract class C : IFace
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

public abstract class C : IFace
{
    public sealed override void M()
    {
    }
}");
        }

        [Fact]
        public void TestBasic_OverridenMethodChangedToSealed()
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
#endif
    }
}