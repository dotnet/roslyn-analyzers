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
    <Serializable()>
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
    <Serializable()>
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
    <Serializable()>
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
    <Serializable()>
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
        public void TestOnDeserializationDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

[Serializable()]
public class TestClass
{
    private string member;

    internal void OnDeserialization(StreamingContext context)
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
            GetCSharpResultAt(22, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "WriteAllBytes"),
            GetCSharpResultAt(23, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "WriteAllLines"),
            GetCSharpResultAt(24, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "WriteAllText"),
            GetCSharpResultAt(25, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Copy"),
            GetCSharpResultAt(26, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Move"),
            GetCSharpResultAt(27, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "AppendAllLines"),
            GetCSharpResultAt(28, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "AppendAllText"),
            GetCSharpResultAt(29, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "AppendText"),
            GetCSharpResultAt(30, 9, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Delete"),
            GetCSharpResultAt(42, 23, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "GetLoadedModules"),
            GetCSharpResultAt(44, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "Load"),
            GetCSharpResultAt(45, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadFile"),
            GetCSharpResultAt(46, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadFrom"),
            GetCSharpResultAt(47, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadModule"),
            GetCSharpResultAt(48, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "LoadWithPartialName"),
            GetCSharpResultAt(49, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "ReflectionOnlyLoad"),
            GetCSharpResultAt(50, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "ReflectionOnlyLoadFrom"),
            GetCSharpResultAt(51, 22, DoNotCallDangerousMethodsInDeserialization.Rule, "TestClass", "OnDeserialization", "UnsafeLoadFrom"));
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
    <Serializable()>
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
    <Serializable()>
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
public class TestClass
{
    private string member;

    internal void OnDeserialization(StreamingContext context)
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
    <Serializable()>
    Class TestClass
        Private member As String
        
        Sub OnDeserialization(ByVal context As StreamingContext)
            Dim obj As New TestClass()
            obj.TestMethod()
        End Sub

        Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void TestOnDeserializationWithoutParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable()]
public class TestClass
{
    private string member;

    internal void OnDeserialization()
    {
        byte[] bytes = new byte[] {0x20, 0x20, 0x20};
        File.WriteAllBytes(""C:\\"", bytes);
    }
}");

            VerifyBasic(@"
Imports System
Imports System.Runtime.Serialization

Namespace TestNamespace
    <Serializable()>
    Class TestClass
        Private member As String
        
        Sub OnDeserialization()
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

public class TestClass
{
    private string member;

    internal void OnDeserialization(StreamingContext context)
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
        
        Sub OnDeserialization(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
        End Sub
    End Class
End Namespace");
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
    <Serializable()>
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
    <Serializable()>
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
    <Serializable()>
    Class TestClass
        Private member As String
        
        Sub OnDeserializedMethod(ByVal context As StreamingContext)
            Dim bytes(9) As Byte
            File.WriteAllBytes(""C:\\"", bytes)
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
