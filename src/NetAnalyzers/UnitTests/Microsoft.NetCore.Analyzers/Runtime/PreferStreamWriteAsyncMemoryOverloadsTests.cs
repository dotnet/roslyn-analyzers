// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferStreamWriteAsyncMemoryOverloadsTest : PreferStreamAsyncMemoryOverloadsTestBase
    {
        #region C# - No diagnostic - Analyzer

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_Write()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    void M()
    {
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            s.Write(buffer, 0, buffer.Length);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_WriteAsync_ByteMemory()
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
            byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
            Memory<byte> memory = new Memory<byte>(buffer);
            await s.WriteAsync(memory, new CancellationToken()).ConfigureAwait(false);
       }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_WriteAsync_AsMemory()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            await s.WriteAsync(buffer.AsMemory(), new CancellationToken()).ConfigureAwait(false);
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
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            Task t = s.WriteAsync(buffer, 0, buffer.Length);
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
        return s.WriteAsync(buffer, 0, buffer.Length);
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
        s.WriteAsync(buffer, 0, 1);
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
        s.WriteAsync(GetBuffer(), 0, 1);
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
    public Task M(FileStream s, byte[] buffer) => s.WriteAsync(buffer, 0, buffer.Length);
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
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            await s.WriteAsync(buffer, 0, buffer.Length).ContinueWith(c => {}).ConfigureAwait(false);
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
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            await s.WriteAsync(buffer, 0, buffer.Length).ContinueWith(c => {}).ContinueWith(c => {}).ConfigureAwait(false);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_AutoCastedToReadOnlyMemory()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            await s.WriteAsync(buffer);
        }
    }
}
            ");
        }

        [Fact]
        public Task CS_Analyzer_NoDiagnostic_AutoCastedToReadOnlyMemory_CancellationToken()
        {
            return CSharpVerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Threading;
class C
{
    public async void M()
    {
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            await s.WriteAsync(buffer, new CancellationToken());
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
        byte[] buffer = { 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 };
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {
            await s.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
            ");
        }

        #endregion

        #region VB - No diagnostic - Analyzer

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_Write()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Private Sub M()
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            s.Write(buffer, 0, buffer.Length)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_WriteAsync_AsMemory()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            Await s.WriteAsync(buffer.AsMemory(), New CancellationToken()).ConfigureAwait(False)
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
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            Dim t As Task = s.WriteAsync(buffer, 0, buffer.Length)
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
Class C
    Public Function M(ByVal s As FileStream, ByVal buffer As Byte()) As Task
        Return s.WriteAsync(buffer, 0, buffer.Length)
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
        s.WriteAsync(buffer, 0, 1)
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
        s.WriteAsync(GetBuffer(), 0, 1)
    End Sub
End Class
            ");
        }

        // The method VB_Analyzer_NoDiagnostic_NoAwait_ExpressionBodyMethod() is
        // skipped because VB does not support expression bodies for methods.

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_ContinueWith_ConfigureAwait()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading
Class C
    Public Async Sub M()
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""file.txt"", FileMode.Create)
            Await s.WriteAsync(buffer, 0, buffer.Length).ContinueWith(Sub(c)
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
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""file.txt"", FileMode.Create)
            Await s.WriteAsync(buffer, 0, buffer.Length).ContinueWith(Sub(c)
                                                                       End Sub).ContinueWith(Sub(c)
                                                                                             End Sub).ConfigureAwait(False)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_AutoCastedToReadOnlyMemory()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading

Class C
    Public Async Sub M()
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            Await s.WriteAsync(buffer)
        End Using
    End Sub
End Class
            ");
        }

        [Fact]
        public Task VB_Analyzer_NoDiagnostic_AutoCastedToReadOnlyMemory_CancellationToken()
        {
            return VisualBasicVerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Threading

Class C
    Public Async Sub M()
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            Await s.WriteAsync(buffer, New CancellationToken())
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
        Dim buffer As Byte() = {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}
        Using s As FileStream = New FileStream(""path.txt"", FileMode.Create)
            Await s.WriteAsync(buffer, 0, buffer.Length)
        End Using
    End Sub
End Class
            ");
        }

        #endregion

        #region C# - Diagnostic - Analyzer

        private const string _sourceWithPredeclaredByteArrayCSharp = @"
using System;
using System.IO;
using System.Threading;
class C
{{
    public async void M()
    {{
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {{
            byte[] buffer = {{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }};
            await {0};
        }}
    }}
}}
            ";

        [Theory]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    false, 57)]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    true, 57)]
        [InlineData("buffer, 0, buffer.Length, new CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), new CancellationToken()",
                    false, 82)]
        [InlineData("buffer, 0, buffer.Length, new CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), new CancellationToken()",
                    true, 82)]
        public Task CS_Analyzer_Diagnostic_VarByteArray(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            return CSharpVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayCSharp, originalArgs, fixedArgs, withConfigureAwait, 12, 19, 12, endColumn);
        }

        [Theory]
        [InlineData("new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }, 0, 8",
                    "(new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }).AsMemory(0, 8)",
                    false, 99)]
        [InlineData("new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }, 0, 8",
                    "(new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }).AsMemory(0, 8)",
                    true, 99)]
        [InlineData("new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }, 0, 8, new CancellationToken()",
                    "(new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }).AsMemory(0, 8), new CancellationToken()",
                    false, 124)]
        [InlineData("new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }, 0, 8, new CancellationToken()",
                    "(new byte[]{ 0xBA, 0x5E, 0xBA, 0x11, 0xF0, 0x07, 0xBA, 0x11 }).AsMemory(0, 8), new CancellationToken()",
                    true, 124)]
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
        using (FileStream s = new FileStream(""path.txt"", FileMode.Create))
        {{
            await {0};
        }}
    }}
}}
            ";
            return CSharpVerifyCodeFixAsync(source, originalArgs, fixedArgs, withConfigureAwait, 11, 19, 11, endColumn);
        }

        [Theory]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    false, 57)]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    true, 57)]
        [InlineData("buffer, 0, buffer.Length, new CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), new CancellationToken()",
                    false, 82)]
        [InlineData("buffer, 0, buffer.Length, new CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), new CancellationToken()",
                    true, 82)]
        public Task CS_Analyzer_Diagnostic_AsStream(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            return CSharpVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayCSharp, originalArgs, fixedArgs, withConfigureAwait, 12, 19, 12, endColumn);
        }

        public static IEnumerable<object[]> NamedArgumentsTestData()
        {
            // Normal argument order is: (byte[] buffer, int offset, int count)
            yield return new object[] { "buffer: buffer, offset: 0, count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length)" };
            yield return new object[] { "buffer: buffer, count: buffer.Length, offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0)" };
            yield return new object[] { "count: buffer.Length, offset: 0, buffer: buffer",
                                        "buffer.AsMemory(length: buffer.Length, start: 0)" };
            yield return new object[] { "count: buffer.Length, buffer: buffer, offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0)" };
            yield return new object[] { "offset: 0, buffer: buffer, count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length)" };
            yield return new object[] { "offset: 0, count: buffer.Length, buffer: buffer",
                                        "buffer.AsMemory(start: 0, length: buffer.Length)" };
        }

        public static IEnumerable<object[]> NamedArgumentsWithCancellationTokenTestData()
        {
            // Normal argument order is: (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            // Cancellation token as fourth argument
            yield return new object[] { "buffer: buffer, offset: 0, count: buffer.Length, cancellationToken: new CancellationToken()",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "buffer: buffer, count: buffer.Length, offset: 0, cancellationToken: new CancellationToken()",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "count: buffer.Length, offset: 0, buffer: buffer, cancellationToken: new CancellationToken()",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "count: buffer.Length, buffer: buffer, offset: 0, cancellationToken: new CancellationToken()",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "offset: 0, buffer: buffer, count: buffer.Length, cancellationToken: new CancellationToken()",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "offset: 0, count: buffer.Length, buffer: buffer, cancellationToken: new CancellationToken()",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            // Cancellation token as third argument
            yield return new object[] { "buffer: buffer, offset: 0, cancellationToken: new CancellationToken(), count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "buffer: buffer, count: buffer.Length, cancellationToken: new CancellationToken(), offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "count: buffer.Length, offset: 0, cancellationToken: new CancellationToken(), buffer: buffer",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "count: buffer.Length, buffer: buffer, cancellationToken: new CancellationToken(), offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "offset: 0, buffer: buffer, cancellationToken: new CancellationToken(), count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "offset: 0, count: buffer.Length, cancellationToken: new CancellationToken(), buffer: buffer",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            // Cancellation token as second argument
            yield return new object[] { "buffer: buffer, cancellationToken: new CancellationToken(), offset: 0, count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "buffer: buffer, cancellationToken: new CancellationToken(), count: buffer.Length, offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "count: buffer.Length, cancellationToken: new CancellationToken(), offset: 0, buffer: buffer",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "count: buffer.Length, cancellationToken: new CancellationToken(), buffer: buffer, offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "offset: 0, cancellationToken: new CancellationToken(), buffer: buffer, count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "offset: 0, cancellationToken: new CancellationToken(), count: buffer.Length, buffer: buffer",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            // Cancellation token as first argument
            yield return new object[] { "cancellationToken: new CancellationToken(), buffer: buffer, offset: 0, count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "cancellationToken: new CancellationToken(), buffer: buffer, count: buffer.Length, offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "cancellationToken: new CancellationToken(), count: buffer.Length, offset: 0, buffer: buffer",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "cancellationToken: new CancellationToken(), count: buffer.Length, buffer: buffer, offset: 0",
                                        "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()" };
            yield return new object[] { "cancellationToken: new CancellationToken(), offset: 0, buffer: buffer, count: buffer.Length",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
            yield return new object[] { "cancellationToken: new CancellationToken(), offset: 0, count: buffer.Length, buffer: buffer",
                                        "buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: new CancellationToken()" };
        }

        [Fact]
        public Task Test()
        {
            string originalArgs = "count: buffer.Length, cancellationToken: new CancellationToken(), buffer: buffer, offset: 0";
            string fixedArgs = "buffer.AsMemory(length: buffer.Length, start: 0), cancellationToken: new CancellationToken()";

            return CSharpVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayCSharp, originalArgs, fixedArgs, withConfigureAwait: false, 12, 19, 12, 80);
        }

        [Theory]
        [MemberData(nameof(NamedArgumentsTestData))]
        public Task CS_Analyzer_Diagnostic_NamedArguments(string originalArgs, string fixedArgs)
        {
            return CSharpVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayCSharp, originalArgs, fixedArgs, withConfigureAwait: false, 12, 19, 12, 80);
        }

        [Theory]
        [MemberData(nameof(NamedArgumentsTestData))]
        public Task CS_Analyzer_Diagnostic_NamedArguments_WithConfigureAwait(string originalArgs, string fixedArgs)
        {
            return CSharpVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayCSharp, originalArgs, fixedArgs, withConfigureAwait: true, 12, 19, 12, 80);
        }

        [Theory]
        [MemberData(nameof(NamedArgumentsWithCancellationTokenTestData))]
        public Task CS_Analyzer_Diagnostic_NamedArguments_CancellationToken(string originalArgs, string fixedArgs)
        {
            return CSharpVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayCSharp, originalArgs, fixedArgs, withConfigureAwait: false, 12, 19, 12, 124);
        }

        [Theory]
        [MemberData(nameof(NamedArgumentsWithCancellationTokenTestData))]
        public Task CS_Analyzer_Diagnostic_NamedArguments_CancellationToken_WithConfigureAwait(string originalArgs, string fixedArgs)
        {
            return CSharpVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayCSharp, originalArgs, fixedArgs, withConfigureAwait: true, 12, 19, 12, 124);
        }

        #endregion

        #region VB - Diagnostic - Analyzer

        private const string _sourceWithPredeclaredByteArrayVisualBasic = @"
Imports System
Imports System.IO
Imports System.Threading
Public Module C
    Public Async Sub M()
        Using s As FileStream = New FileStream(""file.txt"", FileMode.Create)
            Dim buffer As Byte() = {{&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}}
            Await {0}
        End Using
    End Sub
End Module
            ";

        [Theory]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    false, 57)]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    true, 57)]
        [InlineData("buffer, 0, buffer.Length, New CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), New CancellationToken()",
                    false, 82)]
        [InlineData("buffer, 0, buffer.Length, New CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), New CancellationToken()",
                    true, 82)]
        public Task VB_Analyzer_Diagnostic_VarByteArray(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            return VisualBasicVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayVisualBasic, originalArgs, fixedArgs, withConfigureAwait, 9, 19, 9, endColumn);
        }

        [Theory]
        [InlineData("New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}, 0, 8",
                    "(New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}).AsMemory(0, 8)",
                    false, 98)]
        [InlineData("New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}, 0, 8",
                    "(New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}).AsMemory(0, 8)",
                    true, 98)]
        [InlineData("New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}, 0, 8, New CancellationToken()",
                    "(New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}).AsMemory(0, 8), New CancellationToken()",
                    false, 123)]
        [InlineData("New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}, 0, 8, New CancellationToken()",
                    "(New Byte() {&HBA, &H5E, &HBA, &H11, &HF0, &H07, &HBA, &H11}).AsMemory(0, 8), New CancellationToken()",
                    true, 123)]
        public Task VB_Analyzer_Diagnostic_InlineByteArray(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            string source = @"
Imports System
Imports System.IO
Imports System.Threading
Public Module C
    Public Async Sub M()
        Using s As FileStream = New FileStream(""file.txt"", FileMode.Create)
            Await {0}
        End Using
    End Sub
End Module
            ";
            return VisualBasicVerifyCodeFixAsync(source, originalArgs, fixedArgs, withConfigureAwait, 8, 19, 8, endColumn);
        }

        [Theory]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    false, 57)]
        [InlineData("buffer, 0, buffer.Length",
                    "buffer.AsMemory(0, buffer.Length)",
                    true, 57)]
        [InlineData("buffer, 0, buffer.Length, New CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), New CancellationToken()",
                    false, 82)]
        [InlineData("buffer, 0, buffer.Length, New CancellationToken()",
                    "buffer.AsMemory(0, buffer.Length), New CancellationToken()",
                    true, 82)]
        public Task VB_Analyzer_Diagnostic_AsStream(string originalArgs, string fixedArgs, bool withConfigureAwait, int endColumn)
        {
            return VisualBasicVerifyCodeFixAsync(_sourceWithPredeclaredByteArrayVisualBasic, originalArgs, fixedArgs, withConfigureAwait, 9, 19, 9, endColumn);
        }

        #endregion

        #region Helpers

        private const string AsyncMethodName = "Write";

        // Verifies that the fixer generates the fixes for the specified C# diagnostic result configuration for the WriteAsync method.
        private Task CSharpVerifyCodeFixAsync(string source, string originalArgs, string fixedArgs, bool withConfigureAwait, int startLine, int startColumn, int endLine, int endColumn)
        {
            return CSharpVerifyCodeFixAsync(
                GetSourceCodeForInvocation(source, AsyncMethodName, originalArgs, withConfigureAwait, LanguageNames.CSharp),
                GetSourceCodeForInvocation(source, AsyncMethodName, fixedArgs, withConfigureAwait, LanguageNames.CSharp),
                GetCSResult(startLine, startColumn, endLine, endColumn));
        }

        // Verifies that the fixer generates the fixes for the specified VB diagnostic result configuration for the WriteAsync method.
        private Task VisualBasicVerifyCodeFixAsync(string source, string originalArgs, string fixedArgs, bool withConfigureAwait, int startLine, int startColumn, int endLine, int endColumn)
        {
            return VisualBasicVerifyCodeFixAsync(
                GetSourceCodeForInvocation(source, AsyncMethodName, originalArgs, withConfigureAwait, LanguageNames.VisualBasic),
                GetSourceCodeForInvocation(source, AsyncMethodName, fixedArgs, withConfigureAwait, LanguageNames.VisualBasic),
                GetVBResult(startLine, startColumn, endLine, endColumn));
        }

        // Returns a C# diagnostic result using the specified rule, lines, columns and preferred method signature for the WriteAsync method.
        protected static DiagnosticResult GetCSResult(int startLine, int startColumn, int endLine, int endColumn)
            => GetCSResultForRule(startLine, startColumn, endLine, endColumn,
                PreferStreamAsyncMemoryOverloads.PreferStreamWriteAsyncMemoryOverloadsRule,
                "WriteAsync", "System.IO.Stream.WriteAsync(System.ReadOnlyMemory<byte>, System.Threading.CancellationToken)");

        // Returns a VB diagnostic result using the specified rule, lines, columns and preferred method signature for the WriteAsync method.
        protected static DiagnosticResult GetVBResult(int startLine, int startColumn, int endLine, int endColumn)
            => GetVBResultForRule(startLine, startColumn, endLine, endColumn,
                PreferStreamAsyncMemoryOverloads.PreferStreamWriteAsyncMemoryOverloadsRule,
                "WriteAsync", "System.IO.Stream.WriteAsync(System.ReadOnlyMemory(Of Byte), System.Threading.CancellationToken)");

        #endregion
    }
}