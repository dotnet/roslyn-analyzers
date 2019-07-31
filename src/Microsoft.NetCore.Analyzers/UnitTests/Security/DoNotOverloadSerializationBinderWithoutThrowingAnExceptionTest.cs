using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotOverloadSerializationBinderWithoutThrowingAnExceptionTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestReturningNullFromBindToTypeDiagnostic()
        {
            VerifyCSharp(@"
//from https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.serializationbinder?view=netframework-4.8
//this has been reported
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Security.Permissions;


class App 
{
    [STAThread]
    static void Main() 
    {
        System.Console.WriteLine(""This program wasn't supposed to get through compilation. It overloads a serialization binder but doesn't throw an exception, which has security risks."");
        Serialize();
        Deserialize();
    }

    static void Serialize() 
    {
        // To serialize the objects, you must first open a stream for writing. 
        // Use a file stream here.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Create);

        try 
        {
            // Construct a BinaryFormatter and use it 
            // to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct a Version1Type object and serialize it.
            Version1Type obj = new Version1Type();
            obj.x = 123;
            formatter.Serialize(fs, obj);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to serialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }
    }

   
    static void Deserialize() 
    {
        // Declare the Version2Type reference.
        Version2Type obj = null;

        // Open the file containing the data that you want to deserialize.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Open);
        try 
        {
            // Construct a BinaryFormatter and use it 
            // to deserialize the data from the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct an instance of our the
            // Version1ToVersion2TypeSerialiationBinder type.
            // This Binder type can deserialize a Version1Type  
            // object to a Version2Type object.
            formatter.Binder = new Version1ToVersion2DeserializationBinder();

            obj = (Version2Type) formatter.Deserialize(fs);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to deserialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }

        // To prove that a Version2Type object was deserialized, 
        // display the object's type and fields to the console.
        Console.WriteLine(""Type of object deserialized: "" + obj.GetType());
        Console.WriteLine(""x = {0}, name = {1}"", obj.x, obj.name);
    }
}


[Serializable]
class Version1Type 
{
    public Int32 x;
}


[Serializable]
class Version2Type : ISerializable 
{
    public Int32 x;
    public String name;
   
    // The security attribute demands that code that calls
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) 
    {
        info.AddValue(""x"", x);
        info.AddValue(""name"", name);
    }

    // The security attribute demands that code that calls  
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    private Version2Type(SerializationInfo info, StreamingContext context) 
    {
        x = info.GetInt32(""x"");
        try 
        {
            name = info.GetString(""name"");
        }
        catch (SerializationException) 
        {
            // The ""name"" field was not serialized because Version1Type 
            // did not contain this field.
            // Set this field to a reasonable default value.
            name = ""Reasonable default value"";
        }
    }
}


sealed class Version1ToVersion2DeserializationBinder : SerializationBinder 
{
    public override Type BindToType(string assemblyName, string typeName) 
    {
        Type typeToDeserialize = null;

        // For each assemblyName/typeName that you want to deserialize to
        // a different type, set typeToDeserialize to the desired type.
        String assemVer1 = Assembly.GetExecutingAssembly().FullName;
        String typeVer1 = ""Version1Type"";

        if (assemblyName == assemVer1 && typeName == typeVer1) 
        {
            // To use a type from a different assembly version, 
            // change the version number.
            // To do this, uncomment the following line of code.
            // assemblyName = assemblyName.Replace(""1.0.0.0"", ""2.0.0.0"");

            // To use a different type from the same assembly, 
            // change the type name.
            typeName = ""Version2Type"";
        }

        // The following line of code returns the type.
        typeToDeserialize = Type.GetType(String.Format(""{0}, {1}"", 
            typeName, assemblyName));

        return typeToDeserialize;
    }
}

            ",
            GetCSharpResultAt(135, 26, DoNotOverloadSerializationBinderWithoutThrowingAnException.DoNotOverloadSerializationBinderWithoutThrowingAnExceptionRule));
        }

        [Fact]
        public void TestCatchingNullAndThrowingAnExceptionNoDiagnostic()
        {
            VerifyCSharp(@"
//fixed version of previous case
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Security.Permissions;


class App 
{
    [STAThread]
    static void Main() 
    {
        System.Console.WriteLine(""This program should pass compilation. It overloads a serialization binder and throws an exception."");
        Serialize();
        Deserialize();
    }

    static void Serialize() 
    {
        // To serialize the objects, you must first open a stream for writing. 
        // Use a file stream here.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Create);

        try 
        {
            // Construct a BinaryFormatter and use it 
            // to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct a Version1Type object and serialize it.
            Version1Type obj = new Version1Type();
            obj.x = 123;
            formatter.Serialize(fs, obj);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to serialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }
    }

   
    static void Deserialize() 
    {
        // Declare the Version2Type reference.
        Version2Type obj = null;

        // Open the file containing the data that you want to deserialize.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Open);
        try 
        {
            // Construct a BinaryFormatter and use it 
            // to deserialize the data from the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct an instance of our the
            // Version1ToVersion2TypeSerialiationBinder type.
            // This Binder type can deserialize a Version1Type  
            // object to a Version2Type object.
            formatter.Binder = new Version1ToVersion2DeserializationBinder();

            obj = (Version2Type) formatter.Deserialize(fs);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to deserialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }

        // To prove that a Version2Type object was deserialized, 
        // display the object's type and fields to the console.
        Console.WriteLine(""Type of object deserialized: "" + obj.GetType());
        Console.WriteLine(""x = {0}, name = {1}"", obj.x, obj.name);
    }
}


[Serializable]
class Version1Type 
{
    public Int32 x;
}


[Serializable]
class Version2Type : ISerializable 
{
    public Int32 x;
    public String name;
   
    // The security attribute demands that code that calls
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) 
    {
        info.AddValue(""x"", x);
        info.AddValue(""name"", name);
    }

    // The security attribute demands that code that calls  
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    private Version2Type(SerializationInfo info, StreamingContext context) 
    {
        x = info.GetInt32(""x"");
        try 
        {
            name = info.GetString(""name"");
        }
        catch (SerializationException) 
        {
            // The ""name"" field was not serialized because Version1Type 
            // did not contain this field.
            // Set this field to a reasonable default value.
            name = ""Reasonable default value"";
        }
    }
}


sealed class Version1ToVersion2DeserializationBinder : SerializationBinder 
{
    public override Type BindToType(string assemblyName, string typeName) 
    {
        Type typeToDeserialize = null;

        // For each assemblyName/typeName that you want to deserialize to
        // a different type, set typeToDeserialize to the desired type.
        String assemVer1 = Assembly.GetExecutingAssembly().FullName;
        String typeVer1 = ""Version1Type"";

        if (assemblyName == assemVer1 && typeName == typeVer1) 
        {
            // To use a type from a different assembly version, 
            // change the version number.
            // To do this, uncomment the following line of code.
            // assemblyName = assemblyName.Replace(""1.0.0.0"", ""2.0.0.0"");

            // To use a different type from the same assembly, 
            // change the type name.
            typeName = ""Version2Type"";
        }

        // The following line of code returns the type.
        typeToDeserialize = Type.GetType(String.Format(""{0}, {1}"", 
            typeName, assemblyName));

        if (typeToDeserialize == null) {
            throw new SerializationException();
        }
        
        return typeToDeserialize;
    }
}
            ");
        }

        [Fact]
        public void TestSubfunctionImplementsBindToTypeNoDiagnostic()
        {
            VerifyCSharp(@"
//recursive version of previous case
//not the way you'd really do that, but I want to check things
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Security.Permissions;


class App 
{
    [STAThread]
    static void Main() 
    {
        System.Console.WriteLine(""This program is designed to pass compilation tests. It throws an exception as it should, but does that through some extra complexity."");
        Serialize();
        Deserialize();
    }

    static void Serialize() 
    {
        // To serialize the objects, you must first open a stream for writing. 
        // Use a file stream here.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Create);

        try 
        {
            // Construct a BinaryFormatter and use it 
            // to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct a Version1Type object and serialize it.
            Version1Type obj = new Version1Type();
            obj.x = 123;
            formatter.Serialize(fs, obj);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to serialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }
    }

   
    static void Deserialize() 
    {
        // Declare the Version2Type reference.
        Version2Type obj = null;

        // Open the file containing the data that you want to deserialize.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Open);
        try 
        {
            // Construct a BinaryFormatter and use it 
            // to deserialize the data from the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct an instance of our the
            // Version1ToVersion2TypeSerialiationBinder type.
            // This Binder type can deserialize a Version1Type  
            // object to a Version2Type object.
            formatter.Binder = new Version1ToVersion2DeserializationBinder();

            obj = (Version2Type) formatter.Deserialize(fs);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to deserialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }

        // To prove that a Version2Type object was deserialized, 
        // display the object's type and fields to the console.
        Console.WriteLine(""Type of object deserialized: "" + obj.GetType());
        Console.WriteLine(""x = {0}, name = {1}"", obj.x, obj.name);
    }
}


[Serializable]
class Version1Type 
{
    public Int32 x;
}


[Serializable]
class Version2Type : ISerializable 
{
    public Int32 x;
    public String name;
   
    // The security attribute demands that code that calls
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) 
    {
        info.AddValue(""x"", x);
        info.AddValue(""name"", name);
    }

    // The security attribute demands that code that calls  
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    private Version2Type(SerializationInfo info, StreamingContext context) 
    {
        x = info.GetInt32(""x"");
        try 
        {
            name = info.GetString(""name"");
        }
        catch (SerializationException) 
        {
            // The ""name"" field was not serialized because Version1Type 
            // did not contain this field.
            // Set this field to a reasonable default value.
            name = ""Reasonable default value"";
        }
    }
}


sealed class Version1ToVersion2DeserializationBinder : SerializationBinder 
{
    string[] typenames = new string[2];
    
    public override Type BindToType(string assemblyName, string typeName) 
    {
        String assemVer1 = Assembly.GetExecutingAssembly().FullName;
        typenames[0]=""Version1Type"";
        typenames[1] = ""Version2Type"";
        return pointlessRecursiveFunction(assemVer1, typeName, 0);
    }
    Type pointlessRecursiveFunction(string assemblyName, string typeName, int offset=0) {
        try {
            if (typeName == typenames[offset]) {
                return Type.GetType(String.Format(""{0}, {1}"", 
                typenames[offset+1], assemblyName));
            }
        }
        catch (IndexOutOfRangeException) {
                throw new SerializationException();
        }
        return pointlessRecursiveFunction(typeName, assemblyName, offset+2);
    }
}
            ");
        }

        [Fact]
        public void TestReturningNullFromSubfunctionDiagnostic()
        {
            VerifyCSharp(@"
//version where another function is involved but it's still broken
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Security.Permissions;


class App 
{
    [STAThread]
    static void Main() 
    {
        System.Console.WriteLine(""This program is designed to pass compilation tests. It throws an exception as it should, but does that through some extra complexity."");
        Serialize();
        Deserialize();
    }

    static void Serialize() 
    {
        // To serialize the objects, you must first open a stream for writing. 
        // Use a file stream here.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Create);

        try 
        {
            // Construct a BinaryFormatter and use it 
            // to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct a Version1Type object and serialize it.
            Version1Type obj = new Version1Type();
            obj.x = 123;
            formatter.Serialize(fs, obj);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to serialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }
    }

   
    static void Deserialize() 
    {
        // Declare the Version2Type reference.
        Version2Type obj = null;

        // Open the file containing the data that you want to deserialize.
        FileStream fs = new FileStream(""DataFile.dat"", FileMode.Open);
        try 
        {
            // Construct a BinaryFormatter and use it 
            // to deserialize the data from the stream.
            BinaryFormatter formatter = new BinaryFormatter();

            // Construct an instance of our the
            // Version1ToVersion2TypeSerialiationBinder type.
            // This Binder type can deserialize a Version1Type  
            // object to a Version2Type object.
            formatter.Binder = new Version1ToVersion2DeserializationBinder();

            obj = (Version2Type) formatter.Deserialize(fs);
        }
        catch (SerializationException e) 
        {
            Console.WriteLine(""Failed to deserialize. Reason: "" + e.Message);
            throw;
        }
        finally 
        {
            fs.Close();
        }

        // To prove that a Version2Type object was deserialized, 
        // display the object's type and fields to the console.
        Console.WriteLine(""Type of object deserialized: "" + obj.GetType());
        Console.WriteLine(""x = {0}, name = {1}"", obj.x, obj.name);
    }
}


[Serializable]
class Version1Type 
{
    public Int32 x;
}


[Serializable]
class Version2Type : ISerializable 
{
    public Int32 x;
    public String name;
   
    // The security attribute demands that code that calls
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) 
    {
        info.AddValue(""x"", x);
        info.AddValue(""name"", name);
    }

    // The security attribute demands that code that calls  
    // this method have permission to perform serialization.
    [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
    private Version2Type(SerializationInfo info, StreamingContext context) 
    {
        x = info.GetInt32(""x"");
        try 
        {
            name = info.GetString(""name"");
        }
        catch (SerializationException) 
        {
            // The ""name"" field was not serialized because Version1Type 
            // did not contain this field.
            // Set this field to a reasonable default value.
            name = ""Reasonable default value"";
        }
    }
}


sealed class Version1ToVersion2DeserializationBinder : SerializationBinder 
{
    string[] typenames = new string[2];
    Type pointlessRecursiveFunction(string assemblyName, string typeName, int offset=0) {
        try {
            if (typeName == typenames[offset]) {
                return Type.GetType(String.Format(""{0}, {1}"", 
                typenames[offset+1], assemblyName));
            }
        }
        catch (IndexOutOfRangeException) {
            return null;
        }
        return pointlessRecursiveFunction(typeName, assemblyName, offset+2);
    }
    
    public override Type BindToType(string assemblyName, string typeName) 
    {
        String assemVer1 = Assembly.GetExecutingAssembly().FullName;
        typenames[0]=""Version1Type"";
        typenames[1] = ""Version2Type"";
        return pointlessRecursiveFunction(assemVer1, typeName, 0);
    }
}
            ", GetCSharpResultAt(148, 26, DoNotOverloadSerializationBinderWithoutThrowingAnException.DoNotOverloadSerializationBinderWithoutThrowingAnExceptionRule));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotOverloadSerializationBinderWithoutThrowingAnException();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotOverloadSerializationBinderWithoutThrowingAnException();
        }
    } //end class
} //end namespace
