// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.ProvidePublicParameterlessSafeHandleConstructorAnalyzer,
    Microsoft.NetCore.Analyzers.InteropServices.ProvidePublicParameterlessSafeHandleConstructorFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.ProvidePublicParameterlessSafeHandleConstructorAnalyzer,
    Microsoft.NetCore.Analyzers.InteropServices.ProvidePublicParameterlessSafeHandleConstructorFixer>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public class ProvidePublicParameterlessSafeHandleConstructorTests
    {
        [Fact]
        public async Task NonSafeHandleDerivedType_NoDiagnostics_CS()
        {
            string source = @"
class Foo
{
    private Foo()
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task NonSafeHandleDerivedType_NoDiagnostics_VB()
        {
            string source = @"
Class Foo
    Private Sub New()
    End Sub
End Class";
            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task SafeHandleDerivedType_WithParameterlessConstructor_NoDiagnostics_CS()
        {
            string source = @"
using Microsoft.Win32.SafeHandles;

class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public FooHandle() : base(true)
    {
    }

    protected override bool ReleaseHandle() => true;
}";
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task SafeHandleDerivedType_WithParameterlessConstructor_NoDiagnostics_VB()
        {
            string source = @"
Imports Microsoft.Win32.SafeHandles
Public Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid
    Public Sub New()
        MyBase.New(True)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNonPublicParameterlessConstructor_CodeFix_CS()
        {
            string source = @"
using Microsoft.Win32.SafeHandles;

class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private [|FooHandle|]() : base(true)
    {
    }

    protected override bool ReleaseHandle() => true;
}";
            string fixedSource = @"
using Microsoft.Win32.SafeHandles;

class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public FooHandle() : base(true)
    {
    }

    protected override bool ReleaseHandle() => true;
}";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNonPublicParameterlessConstructor_CodeFix_VB()
        {
            string source = @"
Imports Microsoft.Win32.SafeHandles
Public Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid

    Private Sub [|New|]()
        MyBase.New(True)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function

End Class";
            string fixedSource = @"
Imports Microsoft.Win32.SafeHandles
Public Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid

    Public Sub New()
        MyBase.New(True)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function

End Class";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNoParameterlessConstructor_WithAccessibleBaseTypeParameterlessConstructor_CodeFix_CS()
        {
            string source = @"
using System;
using Microsoft.Win32.SafeHandles;

abstract class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    protected FooHandle() : base(true)
    {
    }

    protected override bool ReleaseHandle() => true;
}

class [|BarHandle|] : FooHandle
{
    public BarHandle(IntPtr handle)
    {
        SetHandle(handle);
    }
}";
            string fixedSource = @"
using System;
using Microsoft.Win32.SafeHandles;

abstract class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    protected FooHandle() : base(true)
    {
    }

    protected override bool ReleaseHandle() => true;
}

class BarHandle : FooHandle
{
    public BarHandle(IntPtr handle)
    {
        SetHandle(handle);
    }

    public BarHandle()
    {
    }
}";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNoParameterlessConstructor_WithAccessibleBaseTypeParameterlessConstructor_CodeFix_VB()
        {
            string source = @"
Imports System
Imports Microsoft.Win32.SafeHandles
Public MustInherit Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid

    Protected Sub New()
        MyBase.New(True)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function

End Class
Public Class [|BarHandle|] : Inherits FooHandle

    Public Sub New(handle As IntPtr)
        SetHandle(handle)
    End Sub

End Class";
            string fixedSource = @"
Imports System
Imports Microsoft.Win32.SafeHandles
Public MustInherit Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid

    Protected Sub New()
        MyBase.New(True)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function

End Class
Public Class BarHandle : Inherits FooHandle

    Public Sub New(handle As IntPtr)
        SetHandle(handle)
    End Sub

    Public Sub New()
    End Sub
End Class";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNoParameterlessConstructor_WithNoAccessibleBaseTypeParameterlessConstructor_Diagnostic_CS()
        {
            string source = @"
using System;
using Microsoft.Win32.SafeHandles;

abstract class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private FooHandle() : base(true)
    {
    }

    public FooHandle(IntPtr handle) : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle() => true;
}

class [|BarHandle|] : FooHandle
{
    public BarHandle(IntPtr handle) : base(handle)
    {
    }

    protected override bool ReleaseHandle() => true;
}";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNoParameterlessConstructor_WithNoAccessibleBaseTypeParameterlessConstructor_DeepInheritance_Diagnostic_CS()
        {
            string source = @"
using System;
using Microsoft.Win32.SafeHandles;

abstract class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    protected FooHandle() : base(true)
    {
    }

    public FooHandle(IntPtr handle) : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle() => true;
}

abstract class BarHandle : FooHandle
{
    protected BarHandle(IntPtr handle) : base(handle)
    {
    }
}

class [|BazHandle|] : BarHandle
{
    public BazHandle(IntPtr handle) : base(handle)
    {
    }
}";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task SafeHandleDerived_Abstract_NoPublicConstructor_NoDiagnostic_CS()
        {
            string source = @"
using System;
using Microsoft.Win32.SafeHandles;

abstract class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    protected FooHandle() : base(true)
    {
    }

    protected override bool ReleaseHandle() => true;
}";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task SafeHandleDerived_Abstract_NoPublicConstructor_NoDiagnostic_VB()
        {
            string source = @"
Imports System
Imports Microsoft.Win32.SafeHandles
Public MustInherit Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid

    Protected Sub New()
        MyBase.New(True)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function

End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNoParameterlessConstructor_WithNoAccessibleBaseTypeParameterlessConstructor_Diagnostic_VB()
        {
            string source = @"
Imports System
Imports Microsoft.Win32.SafeHandles
Public MustInherit Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid

    Private Sub New()
        MyBase.New(True)
    End Sub

    Public Sub New(handle As IntPtr)
        MyBase.New(True)
        SetHandle(handle)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function

End Class
Public Class [|BarHandle|] : Inherits FooHandle

    Public Sub New(handle As IntPtr)
        MyBase.New(handle)
    End Sub

End Class";
            await VerifyVB.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNoParameterlessConstructor_WithNoBaseTypeParameterlessConstructor_Diagnostic_CS()
        {

            string source = @"
using System;
using Microsoft.Win32.SafeHandles;

abstract class FooHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public FooHandle(IntPtr handle) : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle() => true;
}

class [|BarHandle|] : FooHandle
{
    public BarHandle(IntPtr handle)
        : base(handle)
    {
    }

    protected override bool ReleaseHandle() => true;
}";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task SafeHandleDerived_WithNoParameterlessConstructor_WithNoBaseTypeParameterlessConstructor_Diagnostic_VB()
        {
            string source = @"
Imports System
Imports Microsoft.Win32.SafeHandles
Public MustInherit Class FooHandle : Inherits SafeHandleZeroOrMinusOneIsInvalid
    Public Sub New(handle As IntPtr)
        MyBase.New(True)
        SetHandle(handle)
    End Sub
    
    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function

End Class
Public Class [|BarHandle|] : Inherits FooHandle

    Public Sub New(handle As IntPtr)
        MyBase.New(handle)
    End Sub

End Class";
            await VerifyVB.VerifyCodeFixAsync(source, source);
        }
    }
}
