// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferStreamReadAsyncMemoryOverloadsTest : PreferStreamAsyncMemoryOverloadsTestBase
    {
        #region C# - No diagnostic - Analyzer

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

        #region VB - No diagnostic - Analyzer

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

        #region C# - Diagnostic - Analyzer

        [Theory]
        [InlineData("buffer, 0, (int)s.Length",
                    "buffer.AsMemory(0, (int)s.Length)",
                    false, 56)]
        [InlineData("buffer, 0, (int)s.Length",
                    "buffer.AsMemory(0, (int)s.Length)",
                    true, 56)]
        [InlineData("buffer, 0, (int)s.Length, new CancellationToken()",
                    "buffer.AsMemory(0, (int)s.Length), new CancellationToken()",
                    false, 81)]
        [InlineData("buffer, 0, (int)s.Length, new CancellationToken()",
                    "buffer.AsMemory(0, (int)s.Length), new CancellationToken()",
                    true, 81)]
        public Task CS_Analyzer_Diagnostic_VarByteArray(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            string source = @"
using System;
using System.IO;
using System.Threading;
class C
{{
    public async void M()
    {{
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {{
            byte[] buffer = new byte[s.Length];
            await {0};
        }}
    }}
}}
            ";
            return CSharpVerifyCodeFixAsync(source, originalArgs, fixedArgs, withConfigureAwait, 12, 19, 12, endColumn);
        }

        [Theory]
        [InlineData("new byte[s.Length], 0, (int)s.Length",
                    "(new byte[s.Length]).AsMemory(0, (int)s.Length)",
                    false, 68)]
        [InlineData("new byte[s.Length], 0, (int)s.Length",
                    "(new byte[s.Length]).AsMemory(0, (int)s.Length)",
                    true, 68)]
        [InlineData("new byte[s.Length], 0, (int)s.Length, new CancellationToken()",
                    "(new byte[s.Length]).AsMemory(0, (int)s.Length), new CancellationToken()",
                    false, 93)]
        [InlineData("new byte[s.Length], 0, (int)s.Length, new CancellationToken()",
                    "(new byte[s.Length]).AsMemory(0, (int)s.Length), new CancellationToken()",
                    true, 93)]
        public Task CS_Analyzer_Diagnostic_InlineByteArray(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            string source = @"
using System;
using System.IO;
using System.Threading;
class C
{{
    public async void M()
    {{
        using (FileStream s = File.Open(""file.txt"", FileMode.Open))
        {{
            await {0};
        }}
    }}
}}
            ";
            return CSharpVerifyCodeFixAsync(source, originalArgs, fixedArgs, withConfigureAwait, 11, 19, 11, endColumn);
        }

        #endregion

        #region VB - Diagnostic - Analyzer

        [Theory]
        [InlineData("buffer, 0, s.Length",
                    "buffer.AsMemory(0, s.Length)",
                    false, 51)]
        [InlineData("buffer, 0, s.Length",
                    "buffer.AsMemory(0, s.Length)",
                    true, 51)]
        [InlineData("buffer, 0, s.Length, New CancellationToken()",
                    "buffer.AsMemory(0, s.Length), New CancellationToken()",
                    false, 76)]
        [InlineData("buffer, 0, s.Length, New CancellationToken()",
                    "buffer.AsMemory(0, s.Length), New CancellationToken()",
                    true, 76)]
        public Task VB_Analyzer_Diagnostic_VarByteArray(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            string source = @"
Imports System
Imports System.IO
Imports System.Threading
Public Module C
    Public Async Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Dim buffer As Byte() = New Byte(s.Length - 1) {{}}
            Await {0}
        End Using
    End Sub
End Module
            ";
            return VisualBasicVerifyCodeFixAsync(source, originalArgs, fixedArgs, withConfigureAwait, 9, 19, 9, endColumn);
        }

        [Theory]
        [InlineData("New Byte(s.Length - 1) {}, 0, s.Length",
                    "(New Byte(s.Length - 1) {}).AsMemory(0, s.Length)",
                    false, 70)]
        [InlineData("New Byte(s.Length - 1) {}, 0, s.Length",
                    "(New Byte(s.Length - 1) {}).AsMemory(0, s.Length)",
                    true, 70)]
        [InlineData("New Byte(s.Length - 1) {}, 0, s.Length, New CancellationToken()",
                    "(New Byte(s.Length - 1) {}).AsMemory(0, s.Length), New CancellationToken()",
                    false, 95)]
        [InlineData("New Byte(s.Length - 1) {}, 0, s.Length, New CancellationToken()",
                    "(New Byte(s.Length - 1) {}).AsMemory(0, s.Length), New CancellationToken()",
                    true, 95)]
        public Task VB_Analyzer_Diagnostic_InlineByteArray(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            string source = @"
Imports System
Imports System.IO
Imports System.Threading
Public Module C
    Public Async Sub M()
        Using s As FileStream = File.Open(""file.txt"", FileMode.Open)
            Await {0}
        End Using
    End Sub
End Module
            ";
            return VisualBasicVerifyCodeFixAsync(source, originalArgs, fixedArgs, withConfigureAwait, 8, 19, 8, endColumn);
        }

        #endregion

        #region Helpers

        private const string AsyncMethodName = "Read";

        // Verifies that the fixer generates the fixes for the specified C# diagnostic result configuration for the ReadAsync method.
        private Task CSharpVerifyCodeFixAsync(string source, string originalArgs, string fixedArgs, bool withConfigureAwait, int startLine, int startColumn, int endLine, int endColumn)
        {
            return CSharpVerifyCodeFixAsync(
                GetSourceCodeForInvocation(source, AsyncMethodName, originalArgs, withConfigureAwait, LanguageNames.CSharp),
                GetSourceCodeForInvocation(source, AsyncMethodName, fixedArgs, withConfigureAwait, LanguageNames.CSharp),
                GetCSResult(startLine, startColumn, endLine, endColumn));
        }

        // Verifies that the fixer generates the fixes for the specified VB diagnostic result configuration for the ReadAsync method.
        private Task VisualBasicVerifyCodeFixAsync(string source, string originalArgs, string fixedArgs, bool withConfigureAwait, int startLine, int startColumn, int endLine, int endColumn)
        {
            return VisualBasicVerifyCodeFixAsync(
                GetSourceCodeForInvocation(source, AsyncMethodName, originalArgs, withConfigureAwait, LanguageNames.VisualBasic),
                GetSourceCodeForInvocation(source, AsyncMethodName, fixedArgs, withConfigureAwait, LanguageNames.VisualBasic),
                GetVBResult(startLine, startColumn, endLine, endColumn));
        }

        // Returns a C# diagnostic result using the specified rule, lines, columns and preferred method signature for the ReadAsync method.
        private static DiagnosticResult GetCSResult(int startLine, int startColumn, int endLine, int endColumn)
            => GetCSResultForRule(startLine, startColumn, endLine, endColumn,
                PreferStreamAsyncMemoryOverloads.PreferStreamReadAsyncMemoryOverloadsRule,
                "ReadAsync", "System.IO.Stream.ReadAsync(System.Memory<byte>, System.Threading.CancellationToken)");

        // Returns a VB diagnostic result using the specified rule, lines, columns and preferred method signature for the ReadAsync method.
        private static DiagnosticResult GetVBResult(int startLine, int startColumn, int endLine, int endColumn)
            => GetVBResultForRule(startLine, startColumn, endLine, endColumn,
                PreferStreamAsyncMemoryOverloads.PreferStreamReadAsyncMemoryOverloadsRule,
                "ReadAsync", "System.IO.Stream.ReadAsync(System.Memory(Of Byte), System.Threading.CancellationToken)");

        #endregion
    }
}