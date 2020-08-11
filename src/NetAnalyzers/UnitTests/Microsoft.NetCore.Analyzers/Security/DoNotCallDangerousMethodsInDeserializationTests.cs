﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotCallDangerousMethodsInDeserialization,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotCallDangerousMethodsInDeserialization,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotCallDangerousMethodsInDeserializationTests
    {
        [Fact]
        public async Task TestOnDeserializingDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserializing()]
    internal void OnDeserializingMethod(StreamingContext context)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}",
            GetCSharpResultAt(12, 19, "TestClass", "OnDeserializingMethod", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserializing()>
        Sub OnDeserializingMethod(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 13, "TestClass", "OnDeserializingMethod", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnDeserializedDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}",
            GetCSharpResultAt(12, 19, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserialized()>
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 13, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnMultiAttributesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserialized()]
    [OnSerialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}",
            GetCSharpResultAt(13, 19, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserialized()>
        <OnSerialized()>
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 13, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnDeserializedMediateInvocationDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        var obj = new TestClass();
        obj.TestMethod();
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }

    private void TestMethod()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}",
            GetCSharpResultAt(12, 19, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserialized()>
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim obj As New TestClass()
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub

        Sub TestMethod()
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 13, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnDeserializedMultiMediateInvocationsDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        var obj = new TestClass();
        var count = 2;
        obj.TestMethod(count);
    }
    
    private void TestMethod(int count)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);

        if(count != 0)
        {
            var obj = new TestClass();
            obj.TestMethod(--count);
        }
    }
}",
            GetCSharpResultAt(12, 19, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserialized()>
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim obj As New TestClass()
            obj.TestMethod(2)
        End Sub

        Sub TestMethod(ByVal count As Integer)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)

            If count <> 0
                Dim obj As New TestClass()
                count = count - 1
                obj.TestMethod(count)
            End If
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 13, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnDeserializationImplicitlyDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;
    
    public void OnDeserialization(Object sender)
    {
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(path, bytes);
    }
}",
            GetCSharpResultAt(13, 17, "TestClass", "OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnDeserializationWriteAllBytesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(path, bytes);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserializationExplictlyImplemented(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserializationExplictlyImplemented", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnDeserializationWriteAllLinesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        var strings = new string[]{""111"", ""222""};
        File.WriteAllLines(path, strings, Encoding.ASCII);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllLines"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Text

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            Dim strings(9) As String
            File.WriteAllLines(path, strings, Encoding.ASCII)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "WriteAllLines"));
        }

        [Fact]
        public async Task TestOnDeserializationWriteAllTextDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        var contents = ""This is the contents."";
        File.WriteAllText(path, contents);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllText"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            Dim contents As String
            contents = ""This is the contents.""
            File.WriteAllText(path, contents)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "WriteAllText"));
        }

        [Fact]
        public async Task TestOnDeserializationCopyDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var sourceFileName = ""source file"";
        var destFileName = ""dest file"";
        File.Copy(sourceFileName, destFileName);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Copy"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim sourceFileName As String
            sourceFileName = ""source file""
            Dim destFileName As String
            destFileName = ""dest file""
            File.Copy(sourceFileName, destFileName)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "Copy"));
        }

        [Fact]
        public async Task TestOnDeserializationMoveDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var sourceFileName = ""source file"";
        var destFileName = ""dest file"";
        File.Move(sourceFileName, destFileName);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Move"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim sourceFileName As String
            sourceFileName = ""source file""
            Dim destFileName As String
            destFileName = ""dest file""
            Dim bytes(9) As Byte
            File.Move(sourceFileName, destFileName)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "Move"));
        }

        [Fact]
        public async Task TestOnDeserializationAppendAllLinesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        var strings = new string[]{""111"", ""222""};
        File.AppendAllLines(path, strings);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendAllLines"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            Dim strings(9) As String
            File.AppendAllLines(""C:\\"", strings)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "AppendAllLines"));
        }

        [Fact]
        public async Task TestOnDeserializationAppendAllTextDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        var contents = ""This is the contents."";
        File.AppendAllText(path, contents);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendAllText"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            Dim contents As String
            File.AppendAllText(path, contents)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "AppendAllText"));
        }

        [Fact]
        public async Task TestOnDeserializationAppendTextDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        File.AppendText(path);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendText"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            File.AppendText(path)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "AppendText"));
        }

        [Fact]
        public async Task TestOnDeserializationDeleteDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        File.Delete(path);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            File.Delete(path)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public async Task TestOnDeserializationDeleteOfDirectoryDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        Directory.Delete(path);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            Directory.Delete(path)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public async Task TestOnDeserializationDeleteOfFileInfoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        new FileInfo(""fileName"").Delete();
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim fileInfo As New FileInfo(""fileName"")
            fileInfo.Delete()
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public async Task TestOnDeserializationDeleteOfDirectoryInfoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        new DirectoryInfo(""path"").Delete();
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim directoryInfo As new DirectoryInfo(""path"")
            directoryInfo.Delete()
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(12, 20, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public async Task TestOnDeserializationDeleteOfLogStoreDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.IO.Log;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace System.IO.Log
{
    public sealed class LogStore : IDisposable
    {
        public static void Delete (string path)
        {
        }
        
        public void Dispose ()
        {
        }
    }
}

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var path = ""C:\\"";
        LogStore.Delete(path);
    }
}",
            GetCSharpResultAt(28, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.IO.Log
Imports System.Runtime.Serialization

Namespace System.IO.Log
    Public NotInheritable Class LogStore
        Implements IDisposable
        Public Shared Sub Delete (path As String)
        End Sub
        
        Public Sub Dispose () Implements IDisposable.Dispose
        End Sub
    End Class
End Namespace

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim path As String
            path = ""C:\\""
            LogStore.Delete(path)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(24, 20, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public async Task TestOnDeserializationGetLoadedModulesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var assem = typeof(TestClass).Assembly;
        var modules = assem.GetLoadedModules();
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "GetLoadedModules"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim assem As Assembly = GetType(TestClass).Assembly
            assem.GetLoadedModules()
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "GetLoadedModules"));
        }

        [Fact]
        public async Task TestOnDeserializationLoadDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var fullName = ""sysglobl, Version = 4.0.0.0, Culture = neutral, "" +
                       ""PublicKeyToken=b03f5f7f11d50a3a, processor architecture=MSIL"";
        var an = new AssemblyName(fullName);
        var assem = Assembly.Load(an);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Load"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim fullName As String
            fullName = ""sysglobl, Version = 4.0.0.0, Culture = neutral, _
                       PublicKeyToken=b03f5f7f11d50a3a, processor architecture=MSIL""
            Dim an As new AssemblyName(fullName)
            Assembly.Load(an)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "Load"));
        }

        [Fact]
        public async Task TestOnDeserializationLoadFileDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var fileName = ""C:\\test.txt"";
        var assem = Assembly.LoadFile(fileName);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadFile"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim fileName As String
            fileName = ""C:\\test.txt""
            Assembly.LoadFile(fileName)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "LoadFile"));
        }

        [Fact]
        public async Task TestOnDeserializationLoadFromDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var assemblyName = ""assembly file"";
        var assem = Assembly.LoadFrom(assemblyName);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadFrom"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim assemblyName As String
            assemblyName = ""assembly file""
            Assembly.LoadFrom(assemblyName)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "LoadFrom"));
        }

        [Fact]
        public async Task TestOnDeserializationLoadModuleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        Assembly assem = typeof(TestClass).Assembly;
        var moduleName = ""module name"";
        var rawModule = new byte[] {0x20, 0x20, 0x20};
        var module = assem.LoadModule(moduleName, rawModule);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadModule"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim assem As Assembly = GetType(TestClass).Assembly
            Dim moduleName As String
            moduleName = ""module name""
            Dim rawModule(9) As Byte
            assem.LoadModule(moduleName, rawModule)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "LoadModule"));
        }

        [Fact]
        public async Task TestOnDeserializationLoadWithPartialNameDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var partialName = ""partial name"";
        var assem = Assembly.LoadWithPartialName(partialName);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadWithPartialName"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim partialName As String
            partialName = ""partial name""
            Assembly.LoadWithPartialName(partialName)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "LoadWithPartialName"));
        }

        [Fact]
        public async Task TestOnDeserializationReflectionOnlyLoadDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var rawAssembly = new byte[] {0x20, 0x20, 0x20};
        var assem = Assembly.ReflectionOnlyLoad(rawAssembly);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "ReflectionOnlyLoad"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim rawAssembly(9) As Byte
            Assembly.ReflectionOnlyLoad(rawAssembly)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "ReflectionOnlyLoad"));
        }

        [Fact]
        public async Task TestOnDeserializationReflectionOnlyLoadFromDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var assemblyName = ""assembly file"";
        var assem = Assembly.ReflectionOnlyLoadFrom(assemblyName);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "ReflectionOnlyLoadFrom"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim assemblyName As String
            assemblyName = ""assembly file""
            Assembly.ReflectionOnlyLoadFrom(assemblyName)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "ReflectionOnlyLoadFrom"));
        }

        [Fact]
        public async Task TestOnDeserializationUnsafeLoadFromDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        var assemblyName = ""assembly file"";
        var assem = Assembly.UnsafeLoadFrom(assemblyName);
    }
}",
            GetCSharpResultAt(13, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "UnsafeLoadFrom"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Public Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim assemblyName As String
            assemblyName = ""assembly file""
            Assembly.UnsafeLoadFrom(assemblyName)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(13, 20, "TestClass", "OnDeserialization", "UnsafeLoadFrom"));
        }

        [Fact]
        public async Task TestUsingGenericwithTypeSpecifiedDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestGenericClass<T>
{
    private T memberInGeneric;

    public void TestGenericMethod()
    {
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(path, bytes);
    }
}

[Serializable()]
public class TestClass : IDeserializationCallback
{
    private TestGenericClass<int> member;

    void IDeserializationCallback.OnDeserialization(Object sender)
    {
        member.TestGenericMethod();
    }
}",
            GetCSharpResultAt(26, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestUsingInterfaceDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

interface TestInterface
{
    void TestInterfaceMethod();
}

[Serializable()]
public class TestInterfaceImplement : TestInterface
{
    public void TestInterfaceMethod()
    {
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(path, bytes);
    }
}

[Serializable()]
public class TestClass : IDeserializationCallback
{
    private TestInterfaceImplement member;

    void IDeserializationCallback.OnDeserialization(Object sender)
    {
        member.TestInterfaceMethod();
    }
}",
            GetCSharpResultAt(29, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestStaticDelegateFieldDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

public delegate void TestDelegate();

[Serializable()]
public class TestAnotherClass
{
    public static TestDelegate staticDelegateField = () =>
    {
        var path = ""C:\\"";
        var bytes = new byte[] { 0x20, 0x20, 0x20 };
        File.WriteAllBytes(path, bytes);
    };
}

[Serializable()]
public class TestClass : IDeserializationCallback
{
    void IDeserializationCallback.OnDeserialization(Object sender)
    {
        TestAnotherClass.staticDelegateField();
    }
}",
            GetCSharpResultAt(24, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestDelegateFieldDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

public delegate void TestDelegate();

[Serializable()]
public class TestAnotherClass
{
    public TestDelegate delegateField;
}

[Serializable()]
public class TestClass : IDeserializationCallback 
{
    void IDeserializationCallback.OnDeserialization(Object sender) 
    {
        TestAnotherClass testAnotherClass = new TestAnotherClass();
        testAnotherClass.delegateField = () =>
        {
            var path = ""C:\\"";
            var bytes = new byte[] { 0x20, 0x20, 0x20 };
            File.WriteAllBytes(path, bytes);
        };
        testAnotherClass.delegateField();
    }
}",
            GetCSharpResultAt(19, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestUsingAbstractClassDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

public abstract class TestAbstractClass
{
    public abstract void TestAbstractMethod();
}

[Serializable()]
public class TestDerivedClass : TestAbstractClass
{
    public override void TestAbstractMethod()
    {
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(path, bytes);
    }
}

[Serializable()]
public class TestClass : IDeserializationCallback
{
    private TestDerivedClass member;

    void IDeserializationCallback.OnDeserialization(Object sender)
    {
        member.TestAbstractMethod();
    }
}",
            GetCSharpResultAt(29, 35, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestFinalizeDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    ~TestClass()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}",
            GetCSharpResultAt(11, 6, "TestClass", "Finalize", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        Protected Overrides Sub Finalize()
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(11, 33, "TestClass", "Finalize", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestDisposeDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass : IDisposable
{
    private string member;
    bool disposed = false;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            byte[] bytes = new byte[] { 0x20, 0x20, 0x20 };
            File.WriteAllBytes(""C:\\"", bytes);
        }

        disposed = true;
    }

    ~TestClass()
    {
        Dispose(false);
    }
}",
            GetCSharpResultAt(13, 17, "TestClass", "Dispose", "WriteAllBytes"),
            GetCSharpResultAt(35, 6, "TestClass", "Finalize", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDisposable
        Private member As String
        Protected disposed As Boolean = False

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    Dim bytes(9) As Byte
                    File.WriteAllBytes(""C:\\"", bytes)
                End If
            End If
            Me.disposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(23, 20, "TestClass", "Dispose", "WriteAllBytes"),
            GetBasicResultAt(28, 33, "TestClass", "Finalize", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestFinalizeWhenSubClassWithSerializableDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    ~TestClass()
    {
    }
}

[Serializable()]
public class SubTestClass : TestClass
{
    private string member;

    ~SubTestClass()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}",
            GetCSharpResultAt(21, 6, "SubTestClass", "Finalize", "WriteAllBytes"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        Protected Overrides Sub Finalize()
        End Sub
    End Class

    <Serializable()> _
    Class SubTestClass 
        Inherits TestClass
        Private member As String
        
        Protected Overrides Sub Finalize()
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(20, 33, "SubTestClass", "Finalize", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestOnDeserializingNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserializing()]
    internal void OnDeserializingMethod(StreamingContext context)
    {
        var obj = new TestClass();
        obj.TestMethod();
    }
    
    private void TestMethod()
    {
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserializing()>
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim obj As New TestClass()
            obj.TestMethod()
        End Sub

        Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestOnDeserializedNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        var obj = new TestClass();
        obj.TestMethod();
    }
    
    private void TestMethod()
    {
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserialized()>
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim obj As New TestClass()
            obj.TestMethod()
        End Sub

        Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestOnDeserializationNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass : IDeserializationCallback
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender)
    {
        var obj = new TestClass();
        obj.TestMethod();
    }
    
    private void TestMethod()
    {
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDeserializationCallback
        Private member As String
        
        Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim obj As New TestClass()
            obj.TestMethod()
        End Sub

        Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestOnDeserializingWithoutSerializableNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

public class TestClass
{
    private string member;

    [OnDeserializing()]
    internal void OnDeserializingMethod(StreamingContext context)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    Class TestClass
        Private member As String
        
        <OnDeserializing()>
        Sub OnDeserializingMethod(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestOnDeserializationWithoutSerializableNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

public class TestClass : IDeserializationCallback
{
    private string member;

    void IDeserializationCallback.OnDeserialization(Object sender)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    Class TestClass 
        Implements IDeserializationCallback
        Private member As String
        
        Sub OnDeserialization(ByVal sender As Object) Implements IDeserializationCallback.OnDeserialization
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestOnDeserializationWithoutIDeserializationCallbackNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    public void OnDeserialization(Object sender)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}");
        }

        [Fact]
        public async Task TestOnDeserializedWithEmptyMethodBodyNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        <OnDeserialized()>
        Sub OnDeserialized(ByVal context As StreamingContext)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestWithoutOnDeserializingAttributesNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    internal void OnDeserializingMethod(StreamingContext context)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        Sub OnDeserializingMethod(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestOnSerializedNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnSerialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestFinalizeNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    ~TestClass()
    {
        var obj = new TestClass();
        obj.TestMethod();
    }
    
    private void TestMethod()
    {
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        Sub Finalize()
            Dim obj As New TestClass()
            obj.TestMethod()
        End Sub

        Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestFinalizeWhenSubClassWithoutSerializableNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    ~TestClass()
    {
    }
}

public class SubTestClass : TestClass
{
    private string member;

    ~SubTestClass()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private member As String
        
        Protected Overrides Sub Finalize()
        End Sub
    End Class

    Class SubTestClass 
        Inherits TestClass
        Private member As String
        
        Protected Overrides Sub Finalize()
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestDisposeNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass : IDisposable
{
    private string member;
    bool disposed = false;

    public void Dispose()
    {
        var obj = new TestClass();
        obj.TestMethod();
        Dispose(true);
        GC.SuppressFinalize(this);           
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return; 
        }
      
        if (disposing) 
        {
            var obj = new TestClass();
            obj.TestMethod();
        }
      
        disposed = true;
    }

    private void TestMethod()
    {
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Implements IDisposable
        Private member As String
        Protected disposed As Boolean = False
        
        Sub Dispose() Implements IDisposable.Dispose
            Dim obj As New TestClass()
            obj.TestMethod()
            Dispose(True)  
            GC.SuppressFinalize(Me) 
        End Sub

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    Dim obj As New TestClass()
                    obj.TestMethod()
                End If
            End If
            Me.disposed = True
        End Sub

        Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestDisposeWithoutSerializableNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

public class TestClass : IDisposable
{
    private string member;
    bool disposed = false;

    public void Dispose()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            byte[] bytes = new byte[] {0x20, 0x20, 0x20};
            File.WriteAllBytes(""C:\\"", bytes);
        }

        disposed = true;
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    Class TestClass
        Implements IDisposable
        Private member As String
        Protected disposed As Boolean = False
        
        Sub Dispose() Implements IDisposable.Dispose
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    Dim bytes(9) As Byte
                    File.WriteAllBytes(""C:\\"", bytes)
                End If
            End If
            Me.disposed = True
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestDisposeNotImplementIDisposableNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

public class TestClass
{
    private string member;
    bool disposed = false;

    public void Dispose()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            byte[] bytes = new byte[] {0x20, 0x20, 0x20};
            File.WriteAllBytes(""C:\\"", bytes);
        }

        disposed = true;
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.IO
Imports System.Runtime.Serialization

Namespace TestNamespace
    Class TestClass
        Private member As String
        Protected disposed As Boolean = False
        
        Sub Dispose()
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    Dim bytes(9) As Byte
                    File.WriteAllBytes(""C:\\"", bytes)
                End If
            End If
            Me.disposed = True
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public async Task TestUsingGenericwithTypeSpecifiedNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestGenericClass<T>
{
    private T memberInGeneric;

    public void TestGenericMethod()
    {
    }
}

[Serializable()]
public class TestClass : IDisposable
{
    private TestGenericClass<int> member;
    bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);           
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return; 
        }
      
        if (disposing) 
        {
        }
      
        disposed = true;
    }

    private void TestMethod()
    {
        member.TestGenericMethod();
    }
}");
        }

        [Fact]
        public async Task TestUsingInterfaceNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

interface TestInterface
{
    void TestInterfaceMethod();
}

[Serializable()]
public class TestInterfaceImplement : TestInterface
{
    public void TestInterfaceMethod()
    {
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(path, bytes);
    }
}

[Serializable()]
public class TestClass : IDisposable
{
    private TestInterface member;
    bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
        }

        disposed = true;
    }

    private void TestMethod()
    {
        member.TestInterfaceMethod();
    }
}");
        }

        [Fact]
        public async Task TestUsingAbstractClassNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

public abstract class TestAbstractClass
{
    public abstract void TestAbstractMethod();
}

[Serializable()]
public class TestDerivedClass : TestAbstractClass
{
    public override void TestAbstractMethod()
    {
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(path, bytes);
    }
}

[Serializable()]
public class TestClass : IDisposable
{
    private TestAbstractClass member;
    bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
        }

        disposed = true;
    }

    private void TestMethod()
    {
        member.TestAbstractMethod();
    }
}");
        }

        [Fact]
        public async Task TestLocalFunctionDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserializing()]
    internal void OnDeserializingMethod(StreamingContext context)
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        ALocalFunction();

        void ALocalFunction()
        {
            File.WriteAllBytes(""C:\\"", bytes);
        }
    }
}",
            GetCSharpResultAt(12, 19, "TestClass", "OnDeserializingMethod", "WriteAllBytes"));
        }

        [Fact]
        public async Task TestLocalFunctionNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    [OnDeserializing()]
    internal void OnDeserializingMethod(StreamingContext context)
    {
        ALocalFunction();

        void ALocalFunction()
        {
            object o = new Object();
        }
    }
}");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);

        private static DiagnosticResult GetBasicResultAt(int line, int column, params string[] arguments)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
