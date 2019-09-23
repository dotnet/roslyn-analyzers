// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.NetCore.CSharp.Analyzers.Performance;
using Microsoft.NetCore.VisualBasic.Analyzers.Performance;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class MutableStructsShouldNotBeUsedForReadonlyFieldsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer();
        }

        #region Diagnostic Tests

        [Fact]
        public void CSharpReadonlyKnownMutableTypes_DiagnosticFires()
        {
            VerifyCSharp(@"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class C
{
    private readonly SpinLock _sl = new SpinLock();
    private readonly GCHandle _gch = new GCHandle();

    private readonly SpinLock _sl2_noinit;
    private readonly GCHandle _gc_noinit;
}
",
                GetCA1829CSharpResultAt(8, 13, "_sl", "SpinLock"),
                GetCA1829CSharpResultAt(9, 13, "_gch", "GCHandle"),
                GetCA1829CSharpResultAt(11, 13, "_sl2_noinit", "SpinLock"),
                GetCA1829CSharpResultAt(12, 13, "_gc_noinit", "GCHandle"));
        }

        [Fact]
        public void CSharpWritableKnownMutableTypes_DiagnosticIgnored()
        {
            VerifyCSharp(@"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class C
{
    private SpinLock _sl = new SpinLock();
    private GCHandle _gch = new GCHandle();

    private SpinLock _sl2_noinit;
    private GCHandle _gc_noinit;
}
");
        }

        [Fact]
        public void CSharpReadonlyCustomType_DiagnosticIgnored()
        {
            VerifyCSharp(@"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public struct S
{
}

public class C
{
    private readonly S _sl = new S();
    private readonly S _sl2;
}
");
        }

        [Fact]
        public void CSharpCustomType_DiagnosticIgnored()
        {
            VerifyCSharp(@"
using System;
using System.Threading;
using System.Runtime.InteropServices;

public struct S
{
}

public class C
{
    private S _sl = new S();
    private S _sl2;
}
");
        }

        [Fact]
        public void BasicReadonlyKnownMutableTypes_DiagnosticFires()
        {
            VerifyBasic(@"
Imports System.Threading
Imports System.Runtime.InteropServices

Public Class Class1
    Public ReadOnly _sl As SpinLock = New SpinLock()
    Public ReadOnly _gch As GCHandle = New GCHandle()

    Public ReadOnly _sl_noinit As SpinLock
    Public ReadOnly _gch_noinit As GCHandle
End Class
",
                GetCA1829BasicResultAt(6, 12, "_sl", "SpinLock"),
                GetCA1829BasicResultAt(7, 12, "_gch", "GCHandle"),
                GetCA1829BasicResultAt(9, 12, "_sl_noinit", "SpinLock"),
                GetCA1829BasicResultAt(10, 12, "_gch_noinit", "GCHandle"));
        }

        [Fact]
        public void BasicWritableKnownMutableTypes_DiagnosticIgnored()
        {
            VerifyBasic(@"
Imports System.Threading
Imports System.Runtime.InteropServices

Public Class Class1
    Public _sl As SpinLock = New SpinLock()
    Public _gch As GCHandle = New GCHandle()

    Public _sl_noinit As SpinLock
    Public _gch_noinit As GCHandle
End Class
");
        }

        [Fact]
        public void BasicReadonlyCustomType_DiagnosticIgnored()
        {
            VerifyBasic(@"
Imports System

Public Structure S

End Structure

Public Class Class1
    Public ReadOnly _s As S = New S()
    Public ReadOnly _s2 As S
End Class
");
        }

        [Fact]
        public void BasicCustomType_DiagnosticIgnored()
        {
            VerifyBasic(@"
Imports System

Public Structure S

End Structure

Public Class Class1
    Public _s As S = New S()
    Public _s2 As S
End Class
");
        }

        #endregion

        private static readonly string MessageTemplate = MicrosoftNetCoreAnalyzersResources.MutableStructsShouldNotBeUsedForReadonlyFieldsMessage;

        private static DiagnosticResult GetCA1829CSharpResultAt(int line, int column, string fieldName, string fieldType)
        {
            return GetCSharpResultAt(line, column, MutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer.RuleId,
                string.Format(MessageTemplate, fieldName, fieldType));
        }

        private static DiagnosticResult GetCA1829BasicResultAt(int line, int column, string fieldName, string fieldType)
        {
            return GetBasicResultAt(line, column, MutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer.RuleId,
                string.Format(MessageTemplate, fieldName, fieldType));
        }
    }
}
