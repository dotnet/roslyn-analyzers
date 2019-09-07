// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.CSharp.Analyzers.Performance;
using Microsoft.NetCore.VisualBasic.Analyzers.Performance;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class MutableStructsShouldNotBeUsedForReadonlyFieldsFixerTests
    {
        [Fact]
        public async Task CSharpReadonlyFieldsOfKnownMutableTypes_RemovesReadonlyModifier()
        {
            var initial = @"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class C
{
    private [|readonly|] SpinLock _sl = new SpinLock();
    private [|readonly|] GCHandle _gch = new GCHandle();

    private [|readonly|] SpinLock _sl_noinit;
    private [|readonly|] GCHandle _gch_noinit;
}
";

            var expected = @"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class C
{
    private SpinLock _sl = new SpinLock();
    private GCHandle _gch = new GCHandle();

    private SpinLock _sl_noinit;
    private GCHandle _gch_noinit;
}
";
            await CSharpCodeFixVerifier<CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer, CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsFixer>
                .VerifyCodeFixAsync(initial, expected);
        }

        [Fact]
        public async Task CSharpWritableFieldsOfKnownMutableType_NoDiagnosticNoFix()
        {
            var initial = @"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class C
{
    private SpinLock _sl = new SpinLock();
    private GCHandle _gch = new GCHandle();

    private SpinLock _sl_noinit;
    private GCHandle _gch_noinit;
}
";

            var expected = @"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class C
{
    private SpinLock _sl = new SpinLock();
    private GCHandle _gch = new GCHandle();

    private SpinLock _sl_noinit;
    private GCHandle _gch_noinit;
}
";
            await CSharpCodeFixVerifier<CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer, CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsFixer>
                .VerifyCodeFixAsync(initial, expected);
        }

        [Fact]
        public async Task CSharpReadonlyFieldsOfCustomType_NoDiagnosticNoFix()
        {
            var initial = @"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public struct S 
{
}

public class C
{
    private readonly S _s = new S();
}
";

            var expected = @"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public struct S 
{
}

public class C
{
    private readonly S _s = new S();
}
";
            await CSharpCodeFixVerifier<CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer, CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsFixer>
                .VerifyCodeFixAsync(initial, expected);
        }

        [Fact]
        public async Task BasicReadonlyFieldsOfKnownMutableTypes_RemovesReadonlyModifier()
        {
            var initial = @"
Imports System.Threading
Imports System.Runtime.InteropServices

Public Class Class1
    Public [|ReadOnly|] _sl As SpinLock = New SpinLock()
    Public [|ReadOnly|] _gch As GCHandle = New GCHandle()

    Public [|ReadOnly|] _sl_noinit As SpinLock
    Public [|ReadOnly|] _gch_noinit As GCHandle
End Class
";

            var expected = @"
Imports System.Threading
Imports System.Runtime.InteropServices

Public Class Class1
    Public _sl As SpinLock = New SpinLock()
    Public _gch As GCHandle = New GCHandle()

    Public _sl_noinit As SpinLock
    Public _gch_noinit As GCHandle
End Class
";
            await VisualBasicCodeFixVerifier<BasicMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer, BasicMutableStructsShouldNotBeUsedForReadonlyFieldsFixer>
                .VerifyCodeFixAsync(initial, expected);
        }

        [Fact]
        public async Task BasicWritableFieldsOfKnownMutableTypes_NoDiagnosticNoFix()
        {
            var initial = @"
Imports System.Threading
Imports System.Runtime.InteropServices

Public Class Class1
    Public _sl As SpinLock = New SpinLock()
    Public _gch As GCHandle = New GCHandle()

    Public _sl_noinit As SpinLock
    Public _gch_noinit As GCHandle
End Class
";

            var expected = @"
Imports System.Threading
Imports System.Runtime.InteropServices

Public Class Class1
    Public _sl As SpinLock = New SpinLock()
    Public _gch As GCHandle = New GCHandle()

    Public _sl_noinit As SpinLock
    Public _gch_noinit As GCHandle
End Class
";
            await VisualBasicCodeFixVerifier<BasicMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer, BasicMutableStructsShouldNotBeUsedForReadonlyFieldsFixer>
                .VerifyCodeFixAsync(initial, expected);
        }

        [Fact]
        public async Task BasicReadonlyFieldsOfCustomTypes_NoDiagnosticNoFix()
        {
            var initial = @"
Public Structure S
End Structure

Public Class Class1
    Public ReadOnly _sl As S = New S()
    Public ReadOnly _sl_noinit As S
End Class
";

            var expected = @"
Public Structure S
End Structure

Public Class Class1
    Public ReadOnly _sl As S = New S()
    Public ReadOnly _sl_noinit As S
End Class
";
            await VisualBasicCodeFixVerifier<BasicMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer, BasicMutableStructsShouldNotBeUsedForReadonlyFieldsFixer>
                .VerifyCodeFixAsync(initial, expected);
        }
    }
}