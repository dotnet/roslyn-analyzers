// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class ImplementIDisposableCorrectlyTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicImplementIDisposableCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpImplementIDisposableCorrectlyAnalyzer();
        }

        #region CSharp Unit Tests

        [Fact]
        public void CSharp_CA1063_DisposeSignature_NoDiagnostic_GoodDisposablePattern()
        {
            VerifyCSharp(@"
using System;

public class C : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}
");
        }

        [Fact]
        public void CSharp_CA1063_DisposeSignature_NoDiagnostic_NotImplementingDisposable()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}
");
        }

        #endregion

        #region CSharp IDisposableReimplementation Unit Tests

        [Fact]
        public void CSharp_CA1063_IDisposableReimplementation_Diagnostic_ReimplementingIDisposable()
        {
            VerifyCSharp(@"
using System;

public class B : IDisposable
{
    public virtual void Dispose()
    {
    }
}

[|public class C : B, IDisposable
{
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}|]
",
            GetCA1063CSharpIDisposableReimplementationResultAt(11, 14, "C"),
            GetCA1063CSharpDisposeSignatureResultAt(13, 26, "C", "Dispose"),
            GetCA1063CSharpDisposeOverrideResultAt(13, 26, "C", "Dispose"));
        }

        [Fact]
        public void CSharp_CA1063_IDisposableReimplementation_Diagnostic_ImplementingInterfaceInheritedFromIDisposable()
        {
            VerifyCSharp(@"
using System;

public interface ITest : IDisposable
{
    void int Test { get; set; }
}

public class B : IDisposable
{
    public void Dispose()
    {
    }
}

[|public class C : B, ITest
{
    public int Test { get; set; }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}|]
",
            GetCA1063CSharpIDisposableReimplementationResultAt(16, 14, "C"));
        }

        [Fact]
        public void CSharp_CA1063_IDisposableReimplementation_NoDiagnostic_ImplementingInheritedInterfaceWithNoDisposeReimplementation()
        {
            VerifyCSharp(@"
using System;

public interface ITest : IDisposable
{
    void int Test { get; set; }
}

public class B : IDisposable
{
    public void Dispose()
    {
    }
}

[|public class C : B, ITest
{
    public int Test { get; set; }
}|]
");
        }

        #endregion

        #region CSharp DisposeSignature Unit Tests

        [Fact]
        public void CSharp_CA1063_DisposeSignature_Diagnostic_DisposeNotPublic()
        {
            VerifyCSharp(@"
using System;

public class C : IDisposable
{
    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}  
",
            GetCA1063CSharpDisposeSignatureResultAt(6, 22, "C", "System.IDisposable.Dispose"),
            GetCA1063CSharpRenameDisposeResultAt(6, 22, "C", "System.IDisposable.Dispose"));
        }

        [Fact]
        public void CSharp_CA1063_DisposeSignature_Diagnostic_DisposeIsVirtual()
        {
            VerifyCSharp(@"
using System;

public class C : IDisposable
{
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}  
",
            GetCA1063CSharpDisposeSignatureResultAt(6, 25, "C", "Dispose"));
        }

        [Fact]
        public void CSharp_CA1063_DisposeSignature_Diagnostic_DisposeIsOverriden()
        {
            VerifyCSharp(@"
using System;

public class B
{
    public virtual void Dispose()
    {
    }
}

public class C : B, IDisposable
{
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}  
",
            GetCA1063CSharpDisposeSignatureResultAt(13, 26, "C", "Dispose"));
        }

        [Fact]
        public void CSharp_CA1063_DisposeSignature_NoDiagnostic_DisposeIsOverridenAndSealed()
        {
            VerifyCSharp(@"
using System;

public class B
{
    public virtual void Dispose()
    {
    }
}

public class C : B, IDisposable
{
    public sealed override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}  
");
        }

        #endregion

        #region CSharp DisposeOverride Unit Tests

        [Fact]
        public void CSharp_CA1063_DisposeOverride_Diagnostic_SimpleDisposeOverride()
        {
            VerifyCSharp(@"
using System;

public class B : IDisposable
{
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}

[|public class C : B
{
    public override void Dispose()
    {
    }
}|]
",
            GetCA1063CSharpDisposeOverrideResultAt(24, 26, "C", "Dispose"));
        }

        [Fact]
        public void CSharp_CA1063_DisposeOverride_Diagnostic_DoubleDisposeOverride()
        {
            VerifyCSharp(@"
using System;

public class A : IDisposable
{
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~C()
    {
        Dispose(false);
    }

    protected virtual Dispose(bool disposing)
    {
    }
}

public class B : A
{
    public override void Dispose()
    {
    }
}

[|public class C : B
{
    public override void Dispose()
    {
        Dispose(true);
    }
}|]
",
            GetCA1063CSharpDisposeOverrideResultAt(31, 26, "C", "Dispose"));
        }

        #endregion

        #region VB Unit Tests

        [Fact]
        public void Basic_CA1063_DisposeSignature_NoDiagnostic_GoodDisposablePattern()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
");
        }

        [Fact]
        public void Basic_CA1063_DisposeSignature_NoDiagnostic_NotImplementingDisposable()
        {
            VerifyBasic(@"
Imports System

Public Class C

    Public Sub Dispose()
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
");
        }

        #endregion

        #region VB IDisposableReimplementation Unit Tests

        [Fact]
        public void Basic_CA1063_IDisposableReimplementation_Diagnostic_ReimplementingIDisposable()
        {
            VerifyBasic(@"
Imports System

Public Class B
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

[|Public Class C
    Inherits B
    Implements IDisposable

    Public Overrides Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Overloads Sub Dispose(disposing As Boolean)
    End Sub

End Class|]
",
            GetCA1063BasicIDisposableReimplementationResultAt(11, 14, "C"),
            GetCA1063BasicDisposeSignatureResultAt(15, 26, "C", "Dispose"),
            GetCA1063BasicDisposeOverrideResultAt(15, 26, "C", "Dispose"));
        }

        [Fact]
        public void Basic_CA1063_IDisposableReimplementation_Diagnostic_ImplementingInterfaceInheritedFromIDisposable()
        {
            VerifyBasic(@"
Imports System

Public Interface ITest
    Inherits IDisposable

    Property Test As Integer
End Interface

Public Class B
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

[|Public Class C
    Inherits B
    Implements ITest

    Public Property Test As Integer

    Public Shadows Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Overloads Sub Dispose(disposing As Boolean)
    End Sub

End Class|]
",
            GetCA1063BasicIDisposableReimplementationResultAt(17, 14, "C"));
        }

        [Fact]
        public void Basic_CA1063_IDisposableReimplementation_Diagnostic_ImplementingInheritedInterfaceWithNoDisposeReimplementation()
        {
            VerifyBasic(@"
Imports System

Public Interface ITest
    Inherits IDisposable

    Property Test As Integer
End Interface

Public Class B
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

[|Public NotInheritable Class C
    Inherits B
    Implements ITest

    Public Property Test As Integer

End Class|]
");
        }

        #endregion

        #region VB DisposeSignature Unit Tests

        [Fact]
        public void Basic_CA1063_DisposeSignature_Diagnostic_DisposeProtected()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Implements IDisposable

    Protected Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
",
            GetCA1063BasicDisposeSignatureResultAt(7, 19, "C", "Dispose"));
        }

        [Fact]
        public void Basic_CA1063_DisposeSignature_Diagnostic_DisposePrivate()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Implements IDisposable

    Private Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
",
            GetCA1063BasicDisposeSignatureResultAt(7, 17, "C", "Dispose"));
        }

        [Fact]
        public void Basic_CA1063_DisposeSignature_Diagnostic_DisposeIsVirtual()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
",
            GetCA1063BasicDisposeSignatureResultAt(7, 28, "C", "Dispose"));
        }

        [Fact]
        public void Basic_CA1063_DisposeSignature_Diagnostic_DisposeIsOverriden()
        {
            VerifyBasic(@"
Imports System

Public Class B
    Public Overridable Sub Dispose()
    End Sub
End Class

Public Class C
    Inherits B
    Implements IDisposable

    Public Overrides Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
",
            GetCA1063BasicDisposeSignatureResultAt(13, 26, "C", "Dispose"));
        }

        [Fact]
        public void Basic_CA1063_DisposeSignature_Diagnostic_DisposeIsOverridenAndSealed()
        {
            VerifyBasic(@"
Imports System

Public Class B
    Public Overridable Sub Dispose()
    End Sub
End Class

Public Class C
    Inherits B
    Implements IDisposable

    Public NotOverridable Overrides Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
");
        }

        #endregion

        #region VB RenameDispose Unit Tests

        [Fact]
        public void Basic_CA1063_RenameDispose_Diagnostic_DisposeNamedD()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Implements IDisposable

    Public Sub D() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class
",
            GetCA1063BasicRenameDisposeResultAt(7, 16, "C", "D"));
        }

        #endregion

        #region VB DisposeOverride Unit Tests

        [Fact]
        public void Basic_CA1063_DisposeOverride_Diagnostic_SimpleDisposeOverride()
        {
            VerifyBasic(@"
Imports System

Public Class B
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class

[|Public Class C
    Inherits B

    Public Overrides Sub Dispose()
    End Sub
End Class|]
",
            GetCA1063BasicDisposeOverrideResultAt(25, 26, "C", "Dispose"));
        }

        [Fact]
        public void Basic_CA1063_DisposeOverride_Diagnostic_DoubleDisposeOverride()
        {
            VerifyBasic(@"
Imports System

Public Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class

Public Class B
    Inherits A

    Public Overrides Sub Dispose()
    End Sub
End Class
    
[|Public Class C
    Inherits B

    Public Overrides Sub Dispose()
        Dispose(True)
    End Sub
End Class|]
",
            GetCA1063BasicDisposeOverrideResultAt(32, 26, "C", "Dispose"));
        }

        [Fact]
        public void Basic_CA1063_DisposeOverride_Diagnostic_2DisposeImplementationsOverriden()
        {
            VerifyBasic(@"
Imports System

Public Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
    End Sub

End Class

Public Class B
    Inherits A
    Implements IDisposable

    Public Overridable Sub D() Implements IDisposable.Dispose
    End Sub
End Class
    
[|Public Class C
    Inherits B

    Public Overrides Sub Dispose()
        Dispose(True)
    End Sub

    Public Overrides Sub D()
        Dispose()
    End Sub
End Class|]
",
            GetCA1063BasicDisposeOverrideResultAt(33, 26, "C", "Dispose"),
            GetCA1063BasicDisposeOverrideResultAt(37, 26, "C", "D"));
        }

        #endregion

        #region Helpers

        private static DiagnosticResult GetCA1063CSharpIDisposableReimplementationResultAt(int line, int column, string typeName)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageIDisposableReimplementation, typeName);
            return GetCSharpResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1063BasicIDisposableReimplementationResultAt(int line, int column, string typeName)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageIDisposableReimplementation, typeName);
            return GetBasicResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1063CSharpDisposeSignatureResultAt(int line, int column, string typeName, string disposeMethod)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeSignature, typeName + "." + disposeMethod);
            return GetCSharpResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1063BasicDisposeSignatureResultAt(int line, int column, string typeName, string disposeMethod)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeSignature, typeName + "." + disposeMethod);
            return GetBasicResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1063CSharpRenameDisposeResultAt(int line, int column, string typeName, string disposeMethod)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageRenameDispose, typeName + "." + disposeMethod);
            return GetCSharpResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1063BasicRenameDisposeResultAt(int line, int column, string typeName, string disposeMethod)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageRenameDispose, typeName + "." + disposeMethod);
            return GetBasicResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1063CSharpDisposeOverrideResultAt(int line, int column, string typeName, string method)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeOverride, typeName + "." + method);
            return GetCSharpResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1063BasicDisposeOverrideResultAt(int line, int column, string typeName, string method)
        {
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeOverride, typeName + "." + method);
            return GetBasicResultAt(line, column, ImplementIDisposableCorrectlyAnalyzer.RuleId, message);
        }

        #endregion
    }
}