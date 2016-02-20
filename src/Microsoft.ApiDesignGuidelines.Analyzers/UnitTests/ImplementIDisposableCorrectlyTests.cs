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

        #region CSharp DisposeSignature Unit Tests

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
            GetCA1063CSharpDisposeSignatureResultAt(6, 22, "C", "System.IDisposable.Dispose"));
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

        #region VB DisposeSignature Unit Test

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
        public void Basic_CA1063_DisposeSignature_Diagnostic_DisposeNotPublic()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Implements IDisposable

    Private Sub D() Implements IDisposable.Dispose
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
            GetCA1063BasicDisposeSignatureResultAt(7, 17, "C", "D"));
        }

        #endregion

        #region Helpers

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
        #endregion
    }
}