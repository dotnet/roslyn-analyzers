// Copyright (c) Microsoft. All Rights Reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotCallDangerousMethodsInDeserializationTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestOnDeserializingDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(12, 19, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializingMethod", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializingMethod", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializedDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(12, 19, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnMultiAttributesDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 19, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializedMediateInvocationDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(12, 19, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializedMultiMediateInvocationsDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(12, 19, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializationImplicitlyDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 17, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializationWriteAllBytesDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializationExplictlyImplemented", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializationWriteAllLinesDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllLines"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "WriteAllLines"));
        }

        [Fact]
        public void TestOnDeserializationWriteAllTextDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllText"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "WriteAllText"));
        }

        [Fact]
        public void TestOnDeserializationCopyDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Copy"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Copy"));
        }

        [Fact]
        public void TestOnDeserializationMoveDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Move"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Move"));
        }

        [Fact]
        public void TestOnDeserializationAppendAllLinesDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendAllLines"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "AppendAllLines"));
        }

        [Fact]
        public void TestOnDeserializationAppendAllTextDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendAllText"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "AppendAllText"));
        }

        [Fact]
        public void TestOnDeserializationAppendTextDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendText"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "AppendText"));
        }

        [Fact]
        public void TestOnDeserializationDeleteDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public void TestOnDeserializationDeleteOfDirectoryDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public void TestOnDeserializationDeleteOfFileInfoDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public void TestOnDeserializationDeleteOfDirectoryInfoDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            VerifyBasic(@"
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
            GetBasicResultAt(12, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public void TestOnDeserializationDeleteOfLogStoreDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(28, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"));

            VerifyBasic(@"
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
            GetBasicResultAt(24, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Delete"));
        }

        [Fact]
        public void TestOnDeserializationGetLoadedModulesDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "GetLoadedModules"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "GetLoadedModules"));
        }

        [Fact]
        public void TestOnDeserializationLoadDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Load"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Load"));
        }

        [Fact]
        public void TestOnDeserializationLoadFileDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadFile"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadFile"));
        }

        [Fact]
        public void TestOnDeserializationLoadFromDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadFrom"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadFrom"));
        }

        [Fact]
        public void TestOnDeserializationLoadModuleDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadModule"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadModule"));
        }

        [Fact]
        public void TestOnDeserializationLoadWithPartialNameDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadWithPartialName"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadWithPartialName"));
        }

        [Fact]
        public void TestOnDeserializationReflectionOnlyLoadDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "ReflectionOnlyLoad"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "ReflectionOnlyLoad"));
        }

        [Fact]
        public void TestOnDeserializationReflectionOnlyLoadFromDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "ReflectionOnlyLoadFrom"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "ReflectionOnlyLoadFrom"));
        }

        [Fact]
        public void TestOnDeserializationUnsafeLoadFromDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "UnsafeLoadFrom"));

            VerifyBasic(@"
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
            GetBasicResultAt(13, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "UnsafeLoadFrom"));
        }

        [Fact]
        public void TestUsingGenericwithTypeSpecifiedDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(26, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public void TestUsingInterfaceDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(29, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public void TestStaticDelegateFieldDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(24, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public void TestDelegateFieldDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(19, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public void TestUsingAbstractClassDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(29, 35, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"));
        }

        [Fact]
        public void TestFinalizeDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(11, 6, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Finalize", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(11, 33, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Finalize", "WriteAllBytes"));
        }

        [Fact]
        public void TestDisposeDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(13, 17, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Dispose", "WriteAllBytes"),
            GetCSharpResultAt(35, 6, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Finalize", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(23, 20, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Dispose", "WriteAllBytes"),
            GetBasicResultAt(28, 33, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Finalize", "WriteAllBytes"));
        }

        [Fact]
        public void TestFinalizeWhenSubClassWithSerializableDiagnostic()
        {
            VerifyCSharp(@"
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
            GetCSharpResultAt(21, 6, DoNotCallDangerousMethodsInDeserialization.Rule, "SubTestClass", "Finalize", "WriteAllBytes"));

            VerifyBasic(@"
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
            GetBasicResultAt(20, 33, DoNotCallDangerousMethodsInDeserialization.Rule, "SubTestClass", "Finalize", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializingNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestOnDeserializedNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestOnDeserializationNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestOnDeserializingWithoutSerializableNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestOnDeserializationWithoutSerializableNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestOnDeserializationWithoutIDeserializationCallbackNoDiagnostic()
        {
            VerifyCSharp(@"
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
        public void TestOnDeserializedWithEmptyMethodBodyNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestWithoutOnDeserializingAttributesNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestOnSerializedNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestFinalizeNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestFinalizeWhenSubClassWithoutSerializableNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestDisposeNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestDisposeWithoutSerializableNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestDisposeNotImplementIDisposableNoDiagnostic()
        {
            VerifyCSharp(@"
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

            VerifyBasic(@"
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
        public void TestUsingGenericwithTypeSpecifiedNoDiagnostic()
        {
            VerifyCSharp(@"
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
        public void TestUsingInterfaceNoDiagnostic()
        {
            VerifyCSharp(@"
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
        public void TestUsingAbstractClassNoDiagnostic()
        {
            VerifyCSharp(@"
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

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCallDangerousMethodsInDeserialization();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCallDangerousMethodsInDeserialization();
        }
    }
}
