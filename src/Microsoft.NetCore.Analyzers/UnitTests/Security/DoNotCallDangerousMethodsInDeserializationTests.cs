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
            GetCSharpResultAt(15, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializingMethod", "WriteAllBytes"));

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
            GetBasicResultAt(14, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializingMethod", "WriteAllBytes"));
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
            GetCSharpResultAt(15, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

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
            GetBasicResultAt(14, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
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
            GetCSharpResultAt(16, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

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
            GetBasicResultAt(15, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
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
        File.WriteAllText(""C:\\"", ""contents"");
    }
    
    private void TestMethod()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}",
            GetCSharpResultAt(16, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllText"),
            GetCSharpResultAt(22, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

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
            obj.TestMethod()
            File.WriteAllText(""C:\\"", ""contents"")
        End Sub

        Sub TestMethod()
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(15, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllText"),
            GetBasicResultAt(20, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
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
            GetCSharpResultAt(22, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));

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
            GetBasicResultAt(19, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserializedMethod", "WriteAllBytes"));
        }

        [Fact]
        public void TestOnDeserializationDiagnostic()
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
        //File
        var path = ""C:\\"";
        var bytes = new byte[] {0x20, 0x20, 0x20};
        var strings = new string[]{""111"", ""222""};
        var contents = ""This is the contents."";
        var sourceFileName = ""source file"";
        var destFileName = ""dest file"";
        File.WriteAllBytes(path, bytes);
        File.WriteAllLines(path, strings, Encoding.ASCII);
        File.WriteAllText(path, contents);
        File.Copy(sourceFileName, destFileName);
        File.Move(sourceFileName, destFileName);
        File.AppendAllLines(path, strings);
        File.AppendAllText(path, contents);
        File.AppendText(path);
        File.Delete(path);

        //Assembly
        var fileName = ""C:\\test.txt"";
        var assemblyName = ""assembly file"";
        var moduleName = ""module name"";
        var partialName = ""partial name"";
        var fullName = ""sysglobl, Version = 4.0.0.0, Culture = neutral, "" +
                       ""PublicKeyToken=b03f5f7f11d50a3a, processor architecture=MSIL"";
        var rawModule = new byte[] {0x20, 0x20, 0x20};
        var rawAssembly = new byte[] {0x20, 0x20, 0x20};
        var assem = typeof(TestClass).Assembly;
        var modules = assem.GetLoadedModules();
        var an = new AssemblyName(fullName);
        var assem2 = Assembly.Load(an);
        var assem3 = Assembly.LoadFile(fileName);
        var assem4 = Assembly.LoadFrom(assemblyName);
        var module = assem.LoadModule(moduleName, rawModule);
        var assem5 = Assembly.LoadWithPartialName(partialName);
        var assem6 = Assembly.ReflectionOnlyLoad(rawAssembly);
        var assem7 = Assembly.ReflectionOnlyLoadFrom(assemblyName);
        var assem8 = Assembly.UnsafeLoadFrom(assemblyName);
    }
}",
            GetCSharpResultAt(22, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllBytes"),
            GetCSharpResultAt(23, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllLines"),
            GetCSharpResultAt(24, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "WriteAllText"),
            GetCSharpResultAt(25, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Copy"),
            GetCSharpResultAt(26, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Move"),
            GetCSharpResultAt(27, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendAllLines"),
            GetCSharpResultAt(28, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendAllText"),
            GetCSharpResultAt(29, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "AppendText"),
            GetCSharpResultAt(30, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Delete"),
            GetCSharpResultAt(42, 23, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "GetLoadedModules"),
            GetCSharpResultAt(44, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "Load"),
            GetCSharpResultAt(45, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadFile"),
            GetCSharpResultAt(46, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadFrom"),
            GetCSharpResultAt(47, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadModule"),
            GetCSharpResultAt(48, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "LoadWithPartialName"),
            GetCSharpResultAt(49, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "ReflectionOnlyLoad"),
            GetCSharpResultAt(50, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "ReflectionOnlyLoadFrom"),
            GetCSharpResultAt(51, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", "UnsafeLoadFrom"));

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
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(14, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "WriteAllBytes"));
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
            GetCSharpResultAt(14, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Finalize", "WriteAllBytes"));

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
            GetBasicResultAt(13, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Finalize", "WriteAllBytes"));
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
            GetCSharpResultAt(29, 13, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Dispose", "WriteAllBytes"));

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
    End Class
End Namespace",
            GetBasicResultAt(17, 21, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "Dispose", "WriteAllBytes"));
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

    void OnDeserialization(Object sender)
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
