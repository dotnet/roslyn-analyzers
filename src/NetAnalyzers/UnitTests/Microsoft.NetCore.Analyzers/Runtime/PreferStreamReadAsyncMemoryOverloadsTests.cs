﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

#pragma warning disable CA1305 // Specify IFormatProvider in string.Format

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferStreamReadAsyncMemoryOverloadsTest : PreferStreamAsyncMemoryOverloadsTestBase
    {
        #region C# - No diagnostic

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_Read()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System.IO;
class C
{
    public void M()
    {
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            s.Read(buffer, 0, (int)s.Length);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_ReadAsync_ByteMemory()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            byte[] buffer = new byte[s.Length];
            Memory<byte> memory = new Memory<byte>(buffer);
            await s.ReadAsync(memory, new CancellationToken()).ConfigureAwait(false);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_ReadAsync_AsMemory()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            await s.ReadAsync(buffer.AsMemory(), new CancellationToken());
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_NoAwait_SaveAsTask()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
class C
{
    public void M()
    {
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            Task t = s.ReadAsync(buffer, 0, (int)s.Length);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_FileStream_NoAwait_ReturnMethod()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
class C
{
    public Task M(FileStream s, byte[] buffer)
    {
        return s.ReadAsync(buffer, 0, (int)s.Length);
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_Stream_NoAwait_VoidMethod()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
class C
{
    public void M(Stream s, byte[] buffer)
    {
        s.ReadAsync(buffer, 0, 1);
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_Stream_NoAwait_VoidMethod_InvokeGetBufferMethod()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
class C
{
    public byte[] GetBuffer()
    {
        return new byte[] { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
    }
    public void M(Stream s)
    {
        s.ReadAsync(GetBuffer(), 0, 1);
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_NoAwait_ExpressionBodyMethod()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
class C
{
    public Task M(FileStream s, byte[] buffer) => s.ReadAsync(buffer, 0, (int)s.Length);
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_ContinueWith_ConfigureAwait()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            await s.ReadAsync(buffer, 0, (int)s.Length).ContinueWith(c => {}).ConfigureAwait(false);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_ContinueWith_ContinueWith_ConfigureAwait()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            await s.ReadAsync(buffer, 0, (int)s.Length).ContinueWith(c => {}).ContinueWith(c => {}).ConfigureAwait(false);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_AutoCastedToMemory()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        using (FileStream s = File.Open(""path.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            await s.ReadAsync(buffer);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_AutoCastedToMemory_CancellationToken()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        using (FileStream s = File.Open(""path.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            await s.ReadAsync(buffer, new CancellationToken());
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_AwaitInvocationOutsideStreamInvocation()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
class C
{
    public async void M()
    {
        using (FileStream s = new FileStream(""file.txt"", FileMode.Create))
        {
            byte[] buffer = new byte[s.Length];
            await PrintTotalBytesWrittenAsync(s.ReadAsync(buffer, 0, buffer.Length)).ConfigureAwait(false);
        }
    }

    private static async Task PrintTotalBytesWrittenAsync(Task<int> readAsyncTask)
    {
        Console.WriteLine(await readAsyncTask.ConfigureAwait(false));
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_UnsupportedVersion()
        {
            return CSharpVerifyAnalyzerForUnsupportedVersionAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {
            byte[] buffer = new byte[s.Length];
            await s.ReadAsync(buffer, 0, (int)s.Length);
        }
    }
}
            ");
        }

        #endregion

        #region VB - No diagnostic

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_Read()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System.IO
Class C
    Public Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) { }
            s.Read(buffer, 0, s.Length)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_ReadAsync_ByteMemory()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            Dim buffer As Byte() = New Byte(s.Length - 1) {}
            Dim memory As Memory(Of Byte) = New Memory(Of Byte)(buffer)
            Await s.ReadAsync(memory, New CancellationToken()).ConfigureAwait(False)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_ReadAsync_AsMemory()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) { }
            Await s.ReadAsync(buffer.AsMemory(), New CancellationToken())
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_NoAwait_SaveAsTask()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Public Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) { }
            Dim t As Task = s.ReadAsync(buffer, 0, CInt(s.Length))
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_FileStream_NoAwait_ReturnMethod()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Friend Class C
    Public Function M(ByVal s As FileStream, ByVal buffer As Byte()) As Task
        Return s.ReadAsync(buffer, 0, s.Length)
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_Stream_NoAwait_VoidMethod()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Public Sub M(ByVal s As Stream, ByVal buffer As Byte())
        s.ReadAsync(buffer, 0, 1)
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_Stream_NoAwait_VoidMethod_InvokeGetBufferMethod()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Public Function GetBuffer() As Byte()
        Return New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
    End Function
    Public Sub M(ByVal s As Stream)
        s.ReadAsync(GetBuffer(), 0, 1)
    End Sub
End Class
            ");
        }

        // The method VB_Analyzer_NoDiagnostic_NoAwait_ExpressionBodyMethod()
        // is skipped because VB does not support expression bodies for methods

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_ContinueWith_ConfigureAwait()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) {}
            Await s.ReadAsync(buffer, 0, s.Length).ContinueWith(Sub(c)
                                                                        End Sub).ConfigureAwait(False)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_ContinueWith_ContinueWith_ConfigureAwait()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) {}
            Await s.ReadAsync(buffer, 0, s.Length).ContinueWith(Sub(c)
                                                                        End Sub).ContinueWith(Sub(c)
                                                                                              End Sub).ConfigureAwait(False)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_AutoCastedToMemory()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = File.Open(""path.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) {}
            Await s.ReadAsync(buffer)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_AutoCastedToMemory_CancellationToken()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = File.Open(""path.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) {}
            Await s.ReadAsync(buffer, New CancellationToken())
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_AwaitInvocationOutsideStreamInvocation()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Class C
    Public Async Sub M()
        Using s As FileStream = New FileStream(""file.txt"", FileMode.Create)
            Dim buffer As Byte() = New Byte(s.Length - 1) { }
            Await PrintTotalBytesWrittenAsync(s.ReadAsync(buffer, 0, buffer.Length)).ConfigureAwait(False)
        End Using
    End Sub
    Private Shared Async Function PrintTotalBytesWrittenAsync(ByVal readAsyncTask As Task(Of Integer)) As Task
        Console.WriteLine(Await readAsyncTask.ConfigureAwait(False))
    End Function
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_UnsupportedVersion()
        {
            return VisualBasicVerifyAnalyzerForUnsupportedVersionAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) { }
            Await s.ReadAsync(buffer, 0, s.Length)
        End Using
    End Sub
End Class
            ");
        }

        #endregion

        #region C# - Diagnostic

        private const string _sourceCSharp = @"
using System;
using System.IO;
using System.Threading;
class C
{{
    public async void M()
    {{
        using (FileStream s = File.Open(""path.txt"", FileMode.Open))
        {{
            {0}
            await s.ReadAsync({1}){2};
        }}
    }}
}}
            ";

        public static IEnumerable<object[]> CSharpInlineByteArrayTestData()
        {
            yield return new object[] { "new byte[s.Length], 0, (int)s.Length",
                                        "(new byte[s.Length]).AsMemory(0, (int)s.Length)" };
            yield return new object[] { "new byte[s.Length], 0, (int)s.Length, new CancellationToken()",
                                        "(new byte[s.Length]).AsMemory(0, (int)s.Length), new CancellationToken()" };
        }

        [Theory]
        [MemberData(nameof(CSharpUnnamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsWithCancellationTokenTestData))]
        public Task CS_Fixer_Diagnostic_ArgumentNaming(string originalArgs, string fixedArgs) =>
            CSharpVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: false, isEmptyConfigureAwait: false);

        [Theory]
        [MemberData(nameof(CSharpUnnamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsWithCancellationTokenTestData))]
        public Task CS_Fixer_Diagnostic_ArgumentNaming_WithConfigureAwait(string originalArgs, string fixedArgs) =>
            CSharpVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: false, isEmptyConfigureAwait: true);

        [Theory]
        [MemberData(nameof(CSharpInlineByteArrayTestData))]
        public Task CS_Fixer_Diagnostic_InlineByteArray(string originalArgs, string fixedArgs) =>
            CSharpVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: true, isEmptyConfigureAwait: false);

        [Theory]
        [MemberData(nameof(CSharpInlineByteArrayTestData))]
        public Task CS_Fixer_Diagnostic_InlineByteArray_WithConfigureAwait(string originalArgs, string fixedArgs) =>
            CSharpVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: true, isEmptyConfigureAwait: true);

        [Theory]
        [MemberData(nameof(CSharpUnnamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsWithCancellationTokenTestData))]
        public Task CS_Fixer_Diagnostic_AwaitInvocationPassedAsArgument(string originalArgs, string fixedArgs) =>
            CS_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_Internal(originalArgs, fixedArgs, isEmptyConfigureAwait: true);

        [Theory]
        [MemberData(nameof(CSharpUnnamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsTestData))]
        [MemberData(nameof(CSharpNamedArgumentsWithCancellationTokenTestData))]
        public Task CS_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_WithConfigureAwait(string originalArgs, string fixedArgs) =>
            CS_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_Internal(originalArgs, fixedArgs, isEmptyConfigureAwait: false);

        private Task CS_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_Internal(string originalArgs, string fixedArgs, bool isEmptyConfigureAwait)
        {
            string originalSource = @"
using System;
using System.IO;
using System.Threading;
class C
{{
    public async void M()
    {{
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {{
            byte[] buffer = new byte[s.Length];
            PrintTotalBytesWritten(await s.ReadAsync({0}){1});
        }}
    }}

    private void PrintTotalBytesWritten(int bytesWritten) => Console.WriteLine(bytesWritten);
}}
            ";

            int columnsBeforeStreamInvocation = 42;
            int columnsBeforeArguments = columnsBeforeStreamInvocation + " s.ReadAsync(".Length;

            return CSharpVerifyExpectedCodeFixDiagnosticsAsync(
                string.Format(originalSource, originalArgs, GetConfigureAwaitCSharp(isEmptyConfigureAwait)),
                string.Format(originalSource, fixedArgs, GetConfigureAwaitCSharp(isEmptyConfigureAwait)),
                GetCSharpResult(12, columnsBeforeStreamInvocation, 12, columnsBeforeArguments + originalArgs.Length));
        }

        #endregion

        #region VB - Diagnostic

        private const string _sourceVisualBasic = @"
Imports System
Imports System.IO
Imports System.Threading
Public Module C
    Public Async Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            {0}
            Await s.ReadAsync({1}){2}
        End Using
    End Sub
End Module
            ";

        public static IEnumerable<object[]> VisualBasicInlineByteArrayTestData()
        {
            yield return new object[] { @"New Byte(s.Length - 1) {}, 0, s.Length",
                                        @"(New Byte(s.Length - 1) {}).AsMemory(0, s.Length)" };
            yield return new object[] { @"New Byte(s.Length - 1) {}, 0, s.Length, New CancellationToken()",
                                        @"(New Byte(s.Length - 1) {}).AsMemory(0, s.Length), New CancellationToken()" };
        }

        [Theory]
        [MemberData(nameof(VisualBasicUnnamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsWithCancellationTokenTestData))]
        public Task VB_Fixer_Diagnostic_ArgumentNaming(string originalArgs, string fixedArgs) =>
            VisualBasicVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: false, isEmptyConfigureAwait: true);

        [Theory]
        [MemberData(nameof(VisualBasicUnnamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsWithCancellationTokenTestData))]
        public Task VB_Fixer_Diagnostic_ArgumentNaming_WithConfigureAwait(string originalArgs, string fixedArgs) =>
            VisualBasicVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: false, isEmptyConfigureAwait: false);

        [Theory]
        [MemberData(nameof(VisualBasicInlineByteArrayTestData))]
        public Task VB_Fixer_Diagnostic_InlineByteArray(string originalArgs, string fixedArgs) =>
            VisualBasicVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: true, isEmptyConfigureAwait: true);

        [Theory]
        [MemberData(nameof(VisualBasicInlineByteArrayTestData))]
        public Task VB_Fixer_Diagnostic_InlineByteArray_WithConfigureAwait(string originalArgs, string fixedArgs) =>
            VisualBasicVerifyCodeFixAsync(originalArgs, fixedArgs, isEmptyByteDeclaration: true, isEmptyConfigureAwait: false);

        [Theory]
        [MemberData(nameof(VisualBasicUnnamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsWithCancellationTokenTestData))]
        public Task VB_Fixer_Diagnostic_AwaitInvocationPassedAsArgument(string originalArgs, string fixedArgs) =>
            VB_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_Internal(originalArgs, fixedArgs, isEmptyConfigureAwait: true);

        [Theory]
        [MemberData(nameof(VisualBasicUnnamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsTestData))]
        [MemberData(nameof(VisualBasicNamedArgumentsWithCancellationTokenTestData))]
        public Task VB_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_WithConfigureAwait(string originalArgs, string fixedArgs) =>
            VB_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_Internal(originalArgs, fixedArgs, isEmptyConfigureAwait: false);

        private Task VB_Fixer_Diagnostic_AwaitInvocationPassedAsArgument_Internal(string originalArgs, string fixedArgs, bool isEmptyConfigureAwait)
        {
            string originalSource = @"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            Dim buffer As Byte() = New Byte(s.Length - 1) {{ }}
            PrintTotalBytesWritten(Await s.ReadAsync({0}){1})
        End Using
    End Sub

    Private Sub PrintTotalBytesWritten(ByVal bytesWritten As Integer)
        Console.WriteLine(bytesWritten)
    End Sub
End Class
            ";

            int columnsBeforeStreamInvocation = 42;
            int columnsBeforeArguments = columnsBeforeStreamInvocation + " s.ReadAsync(".Length;

            return VisualBasicVerifyExpectedCodeFixDiagnosticsAsync(
                string.Format(originalSource, originalArgs, GetConfigureAwaitVisualBasic(isEmptyConfigureAwait)),
                string.Format(originalSource, fixedArgs, GetConfigureAwaitVisualBasic(isEmptyConfigureAwait)),
                GetVisualBasicResult(9, columnsBeforeStreamInvocation, 9, columnsBeforeArguments + originalArgs.Length));
        }

        #endregion

        #region Helpers

        private const int _columnBeforeStreamInvocation = 19;
        private readonly int _columnsBeforeArguments = _columnBeforeStreamInvocation + " s.ReadAsync(".Length;

        private Task CSharpVerifyCodeFixAsync(string originalArgs, string fixedArgs, bool isEmptyByteDeclaration, bool isEmptyConfigureAwait) =>
            CSharpVerifyExpectedCodeFixDiagnosticsAsync(
                string.Format(_sourceCSharp, GetByteArrayWithoutDataCSharp(isEmptyByteDeclaration), originalArgs, GetConfigureAwaitCSharp(isEmptyConfigureAwait)),
                string.Format(_sourceCSharp, GetByteArrayWithoutDataCSharp(isEmptyByteDeclaration), fixedArgs, GetConfigureAwaitCSharp(isEmptyConfigureAwait)),
                GetCSharpResult(12, _columnBeforeStreamInvocation, 12, _columnsBeforeArguments + originalArgs.Length));

        private Task VisualBasicVerifyCodeFixAsync(string originalArgs, string fixedArgs, bool isEmptyByteDeclaration, bool isEmptyConfigureAwait) =>
            VisualBasicVerifyExpectedCodeFixDiagnosticsAsync(
                string.Format(_sourceVisualBasic, GetByteArrayWithoutDataVisualBasic(isEmptyByteDeclaration), originalArgs, GetConfigureAwaitVisualBasic(isEmptyConfigureAwait)),
                string.Format(_sourceVisualBasic, GetByteArrayWithoutDataVisualBasic(isEmptyByteDeclaration), fixedArgs, GetConfigureAwaitVisualBasic(isEmptyConfigureAwait)),
                GetVisualBasicResult(9, _columnBeforeStreamInvocation, 9, _columnsBeforeArguments + originalArgs.Length));

        // Returns a C# diagnostic result using the specified rule, lines, columns and preferred method signature for the ReadAsync method.
        private DiagnosticResult GetCSharpResult(int startLine, int startColumn, int endLine, int endColumn)
            => GetCSResultForRule(startLine, startColumn, endLine, endColumn,
                PreferStreamAsyncMemoryOverloads.PreferStreamReadAsyncMemoryOverloadsRule,
                "ReadAsync", "System.IO.Stream.ReadAsync(System.Memory<byte>, System.Threading.CancellationToken)");

        // Returns a VB diagnostic result using the specified rule, lines, columns and preferred method signature for the ReadAsync method.
        private DiagnosticResult GetVisualBasicResult(int startLine, int startColumn, int endLine, int endColumn)
            => GetVBResultForRule(startLine, startColumn, endLine, endColumn,
                PreferStreamAsyncMemoryOverloads.PreferStreamReadAsyncMemoryOverloadsRule,
                "ReadAsync", "System.IO.Stream.ReadAsync(System.Memory(Of Byte), System.Threading.CancellationToken)");

        #endregion
    }
}