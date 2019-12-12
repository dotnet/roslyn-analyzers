﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseInsecureSettingsForJsonNet,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseInsecureSettingsForJsonNet,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseInsecureSettingsForJsonNetTests
    {
        private static readonly DiagnosticDescriptor DefinitelyRule = DoNotUseInsecureSettingsForJsonNet.DefinitelyInsecureSettings;
        private static readonly DiagnosticDescriptor MaybeRule = DoNotUseInsecureSettingsForJsonNet.MaybeInsecureSettings;

        [Fact]
        public async Task DocSample1_CSharp_Violation()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

public class BookRecord
{
    public string Title { get; set; }
    public string Author { get; set; }
    public object Location { get; set; }
}

public abstract class Location
{
    public string StoreId { get; set; }
}

public class AisleLocation : Location
{
    public char Aisle { get; set; }
    public byte Shelf { get; set; }
}

public class WarehouseLocation : Location
{
    public string Bay { get; set; }
    public byte Shelf { get; set; }
}

public class ExampleClass
{
    public BookRecord DeserializeBookRecord(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.Auto;
        return JsonConvert.DeserializeObject<BookRecord>(s, settings);    // CA2327 violation
    }
}
",
            GetCSharpResultAt(34, 16, DefinitelyRule));
        }

        [Fact]
        public async Task DocSample1_VB_Violation()
        {
            await VerifyBasicWithJsonNetAsync(@"
Imports Newtonsoft.Json

Public Class BookRecord
    Public Property Title As String
    Public Property Location As Location
End Class

Public MustInherit Class Location
    Public Property StoreId As String
End Class

Public Class AisleLocation
    Inherits Location

    Public Property Aisle As Char
    Public Property Shelf As Byte
End Class

Public Class WarehouseLocation
    Inherits Location

    Public Property Bay As String
    Public Property Shelf As Byte
End Class

Public Class ExampleClass
    Public Function DeserializeBookRecord(s As String) As BookRecord
        Dim settings As JsonSerializerSettings = New JsonSerializerSettings()
        settings.TypeNameHandling = TypeNameHandling.Auto
        Return JsonConvert.DeserializeObject(Of BookRecord)(s, settings)    ' CA2327 violation
    End Function
End Class
",
                GetBasicResultAt(31, 16, DefinitelyRule));
        }

        [Fact]
        public async Task DocSample1_CSharp_Solution()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class BookRecordSerializationBinder : ISerializationBinder
{
    // To maintain backwards compatibility with serialized data before using an ISerializationBinder.
    private static readonly DefaultSerializationBinder Binder = new DefaultSerializationBinder();

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        Binder.BindToName(serializedType, out assemblyName, out typeName);
    }

    public Type BindToType(string assemblyName, string typeName)
    {
        // If the type isn't expected, then stop deserialization.
        if (typeName != ""BookRecord"" && typeName != ""AisleLocation"" && typeName != ""WarehouseLocation"")
        {
            return null;
        }

        return Binder.BindToType(assemblyName, typeName);
    }
}

public class BookRecord
{
    public string Title { get; set; }
    public object Location { get; set; }
}

public abstract class Location
{
    public string StoreId { get; set; }
}

public class AisleLocation : Location
{
    public char Aisle { get; set; }
    public byte Shelf { get; set; }
}

public class WarehouseLocation : Location
{
    public string Bay { get; set; }
    public byte Shelf { get; set; }
}

public class ExampleClass
{
    public BookRecord DeserializeBookRecord(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.Auto;
        settings.SerializationBinder = new BookRecordSerializationBinder();
        return JsonConvert.DeserializeObject<BookRecord>(s, settings);
    }
}
");
        }

        [Fact]
        public async Task DocSample1_VB_Solution()
        {
            await VerifyBasicWithJsonNetAsync(@"
Imports System
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Public Class BookRecordSerializationBinder
    Implements ISerializationBinder

    ' To maintain backwards compatibility with serialized data before using an ISerializationBinder.
    Private Shared ReadOnly Property Binder As New DefaultSerializationBinder()

    Public Sub BindToName(serializedType As Type, ByRef assemblyName As String, ByRef typeName As String) Implements ISerializationBinder.BindToName
        Binder.BindToName(serializedType, assemblyName, typeName)
    End Sub

    Public Function BindToType(assemblyName As String, typeName As String) As Type Implements ISerializationBinder.BindToType
        ' If the type isn't expected, then stop deserialization.
        If typeName <> ""BookRecord"" AndAlso typeName <> ""AisleLocation"" AndAlso typeName <> ""WarehouseLocation"" Then
            Return Nothing
        End If

        Return Binder.BindToType(assemblyName, typeName)
    End Function
End Class

Public Class BookRecord
    Public Property Title As String
    Public Property Location As Location
End Class

Public MustInherit Class Location
    Public Property StoreId As String
End Class

Public Class AisleLocation
    Inherits Location

    Public Property Aisle As Char
    Public Property Shelf As Byte
End Class

Public Class WarehouseLocation
    Inherits Location

    Public Property Bay As String
    Public Property Shelf As Byte
End Class

Public Class ExampleClass
    Public Function DeserializeBookRecord(s As String) As BookRecord
        Dim settings As JsonSerializerSettings = New JsonSerializerSettings()
        settings.TypeNameHandling = TypeNameHandling.Auto
        settings.SerializationBinder = New BookRecordSerializationBinder()
        Return JsonConvert.DeserializeObject(Of BookRecord)(s, settings)
    End Function
End Class
");
        }

        [Fact]
        public async Task DocSample2_CSharp_Violation()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class BookRecordSerializationBinder : ISerializationBinder
{
    // To maintain backwards compatibility with serialized data before using an ISerializationBinder.
    private static readonly DefaultSerializationBinder Binder = new DefaultSerializationBinder();

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        Binder.BindToName(serializedType, out assemblyName, out typeName);
    }

    public Type BindToType(string assemblyName, string typeName)
    {
        // If the type isn't expected, then stop deserialization.
        if (typeName != ""BookRecord"" && typeName != ""AisleLocation"" && typeName != ""WarehouseLocation"")
        {
            return null;
        }

        return Binder.BindToType(assemblyName, typeName);
    }
}

public class BookRecord
{
    public string Title { get; set; }
    public object Location { get; set; }
}

public abstract class Location
{
    public string StoreId { get; set; }
}

public class AisleLocation : Location
{
    public char Aisle { get; set; }
    public byte Shelf { get; set; }
}

public class WarehouseLocation : Location
{
    public string Bay { get; set; }
    public byte Shelf { get; set; }
}

public static class Binders
{
    public static ISerializationBinder BookRecord = new BookRecordSerializationBinder();
}

public class ExampleClass
{
    public BookRecord DeserializeBookRecord(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.Auto;
        settings.SerializationBinder = Binders.BookRecord;
        return JsonConvert.DeserializeObject<BookRecord>(s, settings);    // CA2328 -- settings might be null
    }
}
",
                GetCSharpResultAt(63, 16, MaybeRule));
        }

        [Fact]
        public async Task DocSample2_VB_Violation()
        {
            await VerifyBasicWithJsonNetAsync(@"
Imports System
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Public Class BookRecordSerializationBinder
    Implements ISerializationBinder

    ' To maintain backwards compatibility with serialized data before using an ISerializationBinder.
    Private Shared ReadOnly Property Binder As New DefaultSerializationBinder()

    Public Sub BindToName(serializedType As Type, ByRef assemblyName As String, ByRef typeName As String) Implements ISerializationBinder.BindToName
        Binder.BindToName(serializedType, assemblyName, typeName)
    End Sub

    Public Function BindToType(assemblyName As String, typeName As String) As Type Implements ISerializationBinder.BindToType
        ' If the type isn't expected, then stop deserialization.
        If typeName <> ""BookRecord"" AndAlso typeName <> ""AisleLocation"" AndAlso typeName <> ""WarehouseLocation"" Then
            Return Nothing
        End If

        Return Binder.BindToType(assemblyName, typeName)
    End Function
End Class

Public Class BookRecord
    Public Property Title As String
    Public Property Location As Location
End Class

Public MustInherit Class Location
    Public Property StoreId As String
End Class

Public Class AisleLocation
    Inherits Location

    Public Property Aisle As Char
    Public Property Shelf As Byte
End Class

Public Class WarehouseLocation
    Inherits Location

    Public Property Bay As String
    Public Property Shelf As Byte
End Class

Public Class Binders
    Public Shared Property BookRecord As ISerializationBinder = New BookRecordSerializationBinder()
End Class

Public Class ExampleClass
    Public Function DeserializeBookRecord(s As String) As BookRecord
        Dim settings As JsonSerializerSettings = New JsonSerializerSettings()
        settings.TypeNameHandling = TypeNameHandling.Auto
        settings.SerializationBinder = Binders.BookRecord
        Return JsonConvert.DeserializeObject(Of BookRecord)(s, settings)   ' CA2328 -- settings might be Nothing
    End Function
End Class
",
                GetBasicResultAt(58, 16, MaybeRule));
        }

        [Fact]
        public async Task DocSample2_CSharp_Solution()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class BookRecordSerializationBinder : ISerializationBinder
{
    // To maintain backwards compatibility with serialized data before using an ISerializationBinder.
    private static readonly DefaultSerializationBinder Binder = new DefaultSerializationBinder();

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        Binder.BindToName(serializedType, out assemblyName, out typeName);
    }

    public Type BindToType(string assemblyName, string typeName)
    {
        // If the type isn't expected, then stop deserialization.
        if (typeName != ""BookRecord"" && typeName != ""AisleLocation"" && typeName != ""WarehouseLocation"")
        {
            return null;
        }

        return Binder.BindToType(assemblyName, typeName);
    }
}

public class BookRecord
{
    public string Title { get; set; }
    public object Location { get; set; }
}

public abstract class Location
{
    public string StoreId { get; set; }
}

public class AisleLocation : Location
{
    public char Aisle { get; set; }
    public byte Shelf { get; set; }
}

public class WarehouseLocation : Location
{
    public string Bay { get; set; }
    public byte Shelf { get; set; }
}

public static class Binders
{
    public static ISerializationBinder BookRecord = new BookRecordSerializationBinder();
}

public class ExampleClass
{
    public BookRecord DeserializeBookRecord(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.Auto;

        // Ensure that SerializationBinder is non-null before deserializing
        settings.SerializationBinder = Binders.BookRecord ?? throw new Exception(""Expected non-null"");

        return JsonConvert.DeserializeObject<BookRecord>(s, settings);
    }
}
");
        }

        [Fact]
        public async Task DocSample2_VB_Solution()
        {
            await VerifyBasicWithJsonNetAsync(@"
Imports System
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Public Class BookRecordSerializationBinder
    Implements ISerializationBinder

    ' To maintain backwards compatibility with serialized data before using an ISerializationBinder.
    Private Shared ReadOnly Property Binder As New DefaultSerializationBinder()

    Public Sub BindToName(serializedType As Type, ByRef assemblyName As String, ByRef typeName As String) Implements ISerializationBinder.BindToName
        Binder.BindToName(serializedType, assemblyName, typeName)
    End Sub

    Public Function BindToType(assemblyName As String, typeName As String) As Type Implements ISerializationBinder.BindToType
        ' If the type isn't expected, then stop deserialization.
        If typeName <> ""BookRecord"" AndAlso typeName <> ""AisleLocation"" AndAlso typeName <> ""WarehouseLocation"" Then
            Return Nothing
        End If

        Return Binder.BindToType(assemblyName, typeName)
    End Function
End Class

Public Class BookRecord
    Public Property Title As String
    Public Property Location As Location
End Class

Public MustInherit Class Location
    Public Property StoreId As String
End Class

Public Class AisleLocation
    Inherits Location

    Public Property Aisle As Char
    Public Property Shelf As Byte
End Class

Public Class WarehouseLocation
    Inherits Location

    Public Property Bay As String
    Public Property Shelf As Byte
End Class

Public Class Binders
    Public Shared Property BookRecord As ISerializationBinder = New BookRecordSerializationBinder()
End Class

Public Class ExampleClass
    Public Function DeserializeBookRecord(s As String) As BookRecord
        Dim settings As JsonSerializerSettings = New JsonSerializerSettings()
        settings.TypeNameHandling = TypeNameHandling.Auto

        ' Ensure that SerializationBinder is non-null before deserializing
        settings.SerializationBinder = If(Binders.BookRecord, New Exception(""Expected non-null""))

        Return JsonConvert.DeserializeObject(Of BookRecord)(s, settings)
    End Function
End Class
");
        }

        [Fact]
        public async Task Field_Interprocedural_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

class Blah
{
    public class BookRecordSerializationBinder : ISerializationBinder
    {
        // To maintain backwards compatibility with serialized data before using an ISerializationBinder.
        private static readonly DefaultSerializationBinder Binder = new DefaultSerializationBinder();

        public BookRecordSerializationBinder()
        {
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            Binder.BindToName(serializedType, out assemblyName, out typeName);
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            // If the type isn't expected, then stop deserialization.
            if (typeName != ""BookRecord"" && typeName != ""AisleLocation"" && typeName != ""WarehouseLocation"")
            {
                return null;
            }

            return Binder.BindToType(assemblyName, typeName);
        }
    }

    private static ISerializationBinder GetSerializationBinder()
    {
        return new BookRecordSerializationBinder();
    }

    protected static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
    {
        SerializationBinder = GetSerializationBinder(),
    };
}
");
        }

        [Fact]
        public async Task Secure_SometimesInitialization_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public void EnsureInitialized()
    {
        if (this.Settings == null)
        {
            this.Settings = new JsonSerializerSettings();
        }
    }
}
");
        }

        [Fact]
        public async Task Insecure_JsonConvert_DeserializeObject_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    object Method(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        return JsonConvert.DeserializeObject(s, settings);
    }
}",
                GetCSharpResultAt(10, 16, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_JsonConvert_DeserializeAnonymousType_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    T Method<T>(string s, T t)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        return JsonConvert.DeserializeAnonymousType<T>(s, t, settings);
    }
}",
                GetCSharpResultAt(10, 16, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_JsonSerializer_Create_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System.IO;
using Newtonsoft.Json;

class Blah
{
    T Deserialize<T>(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        JsonSerializer serializer = JsonSerializer.Create(settings);
        return (T) serializer.Deserialize(new StringReader(s), typeof(T));
    }
}",
                GetCSharpResultAt(11, 37, DefinitelyRule));
        }

        [Fact]
        public async Task Secure_JsonSerializer_CreateDefault_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System.IO;
using Newtonsoft.Json;

class Blah
{
    T Deserialize<T>(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        JsonSerializer serializer = JsonSerializer.Create(settings);
        return (T) serializer.Deserialize(new StringReader(s), typeof(T));
    }
}");
        }

        [Fact]
        public async Task Insecure_JsonConvert_DefaultSettings_Lambda_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    void Method()
    {
        JsonConvert.DefaultSettings = () =>
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return settings;
        };
    }
}",
                GetCSharpResultAt(12, 20, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_JsonConvert_DefaultSettings_Lambda_ImplicitReturn_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    void Method()
    {
        JsonConvert.DefaultSettings = () =>
            new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
            };
    }
}",
                GetCSharpResultAt(9, 13, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_JsonConvert_DefaultSettings_LocalFunction_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    void Method()
    {
        JsonConvert.DefaultSettings = GetSettings;

        JsonSerializerSettings GetSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return settings;
        };
    }
}",
                GetCSharpResultAt(14, 20, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_JsonConvert_DefaultSettings_LocalFunctionWithTryCatch_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    void Method()
    {
        JsonConvert.DefaultSettings = GetSettings;

        JsonSerializerSettings GetSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            try
            {
                settings.TypeNameHandling = TypeNameHandling.Objects;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return settings;
        };
    }

    // 'ex' asserts in AnalysisEntityFactory.EnsureLocation(), when performing interprocedural DFA from Method()
    void HandleException(Exception exParam)
    {
        Console.WriteLine(exParam);
    }
}",
                GetCSharpResultAt(24, 20, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_JsonConvert_DefaultSettings_LocalFunction_CapturedVariables_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    void Method()
    {
        TypeNameHandling tnh = TypeNameHandling.None;
        JsonConvert.DefaultSettings = GetSettings;

        tnh = TypeNameHandling.All;

        JsonSerializerSettings GetSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = tnh;
            return settings;
        };
    }
}",
                GetCSharpResultAt(17, 20, MaybeRule));
        }

        // Ideally, we'd only generate one diagnostic in this case.
        [Fact]
        public async Task Insecure_JsonConvert_DefaultSettings_NestedLocalFunction_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    void Method()
    {
        JsonConvert.DefaultSettings = GetSettings;

        JsonSerializerSettings GetSettings()
        {
            return InnerGetSettings();

            JsonSerializerSettings InnerGetSettings()
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.All;
                return settings;
            }
        };
    }
}",
                GetCSharpResultAt(12, 20, DefinitelyRule),
                GetCSharpResultAt(18, 24, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_FieldInitialization_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
}",
                GetCSharpResultAt(6, 60, DefinitelyRule));
        }

        [Fact]
        public async Task Secure_FieldInitialization_SerializationBinderSet_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static readonly JsonSerializerSettings Settings = 
        new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = new MyISerializationBinder(),
        };
}

public class MyISerializationBinder : Newtonsoft.Json.Serialization.ISerializationBinder
{
    public Type BindToType(string assemblyName, string typeName) => throw new NotImplementedException();
    public void BindToName(Type serializedType, out string assemblyName, out string typeName) => throw new NotImplementedException();
}");
        }

        [Fact]
        public async Task Secure_FieldInitialization_BinderSet_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public static readonly JsonSerializerSettings Settings = 
        new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Binder = new MyBinder(),
        };
}

public class MyBinder : System.Runtime.Serialization.SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName) => throw new NotImplementedException();
    public override void BindToName(Type serializedType, out string assemblyName, out string typeName) => throw new NotImplementedException();
}");
        }

        [Fact]
        public async Task Insecure_PropertyInitialization_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using Newtonsoft.Json;

class Blah
{
    public static JsonSerializerSettings Settings { get; } = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
}",
                GetCSharpResultAt(6, 60, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_PropertyInitialization_MaybeDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

class Foo
{
    public static Func<ISerializationBinder> GetBinder { get; set; }
}

class Blah
{
    
    public static JsonSerializerSettings Settings { get; } = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = Foo.GetBinder(),
        };
}",
                GetCSharpResultAt(14, 60, MaybeRule));
        }

        [Fact]
        public async Task Insecure_Lazy_Field_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    private static readonly Lazy<JsonSerializerSettings> jsonSerializerSettings =
        new Lazy<JsonSerializerSettings>(() => 
            new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
            });
}",
            GetCSharpResultAt(9, 13, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_Instance_Constructor_Initializer_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; }

    public Blah()
    {
        this.Settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
        };
    }
}",
            GetCSharpResultAt(11, 9, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_Instance_Constructor_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; }

    public Blah()
    {
        this.Settings = new JsonSerializerSettings();
        this.Settings.TypeNameHandling = TypeNameHandling.Objects;
    }
}",
            GetCSharpResultAt(11, 9, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_Instance_Constructor_Interprocedural_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public Blah(bool flag)
    {
        this.Initialize(flag);
    }

    public void Initialize(bool flag)
    {
        if (flag)
        {
            this.Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        }
        else
        {
            this.Settings = new JsonSerializerSettings();
        }
    }
}
",
                GetCSharpResultAt(18, 13, DefinitelyRule));
        }

        [Fact]
        public async Task InsecureButNotInitialized_Instance_Constructor_Interprocedural_LValuesWithMoreThanOneCapturedOperation_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public static Func<JsonSerializerSettings[]> GetSettingsArray;

    public Blah()
    {
    }

    public Blah(bool flag)
    {
        Initialize(GetSettingsArray(), GetSettingsArray(), flag);
    }

    public static void Initialize(JsonSerializerSettings[] a1, JsonSerializerSettings[] a2, bool flag)
    {
        if (flag)
        {
            (a1 ?? a2)[0] = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        }
        else
        {
            (a2 ?? a1)[0] = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None };
        }
    }
}
");
        }

        [Fact]
        public async Task Unknown_PropertyInitialized_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public static Func<JsonSerializerSettings> GetSettings;

    public Blah()
    {
        this.Settings = GetSettings();
    }
}
");
        }

        [Fact]
        public async Task UnknownThenNull_PropertyInitialized_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public static Func<JsonSerializerSettings> GetSettings;

    public Blah()
    {
        this.Settings = GetSettings();
        this.Settings = null;
    }
}
");
        }

        [Fact]
        public async Task UnknownOrNull_PropertyInitialized_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public static Func<JsonSerializerSettings> GetSettings;

    public Blah()
    {
        if (new Random().Next(6) == 4)
            this.Settings = GetSettings();
        else
            this.Settings = null;
    }
}
");
        }

        [Fact]
        public async Task InsecureThenNull_PropertyInitialized_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public Blah()
    {
        this.Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        this.Settings = null;
    }
}
");
        }

        [Fact]
        public async Task InsecureThenSecure_PropertyInitialized_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public Blah()
    {
        this.Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        this.Settings.TypeNameHandling = TypeNameHandling.None;
    }
}
");
        }

        [Fact]
        public async Task SecureThenInsecure_FieldInitialized_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings;

    public Blah()
    {
        this.Settings = new JsonSerializerSettings();
        this.Settings.TypeNameHandling = TypeNameHandling.All;
    }
}
",
                GetCSharpResultAt(11, 9, DefinitelyRule));
        }

        [Fact]
        public async Task InsecureOrNull_PropertyInitialized_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public Blah()
    {
        if (new Random().Next(6) == 4)
            this.Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        else
            this.Settings = null;
    }
}
",
                GetCSharpResultAt(12, 13, DefinitelyRule));
        }

        [Fact]
        public async Task InsecureOrSecure_PropertyInitialized_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings Settings { get; set; }

    public Blah()
    {
        if (new Random().Next(6) == 4)
            this.Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        else
            this.Settings = new JsonSerializerSettings();
    }
}
",
                GetCSharpResultAt(12, 13, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_Field_Initialized_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    private static readonly JsonSerializerSettings Settings;

    static Blah()
    {
        Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
    }
}
",
                GetCSharpResultAt(11, 9, DefinitelyRule));
        }

        [Fact]
        public async Task Insecure_UnusedLocalVariable_NoDiagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public void Initialize(bool flag)
    {
        JsonSerializerSettings settings;
        if (flag)
        {
            settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        }
        else
        {
            settings = new JsonSerializerSettings();
        }
    }
}
");
        }

        [Fact]
        public async Task Insecure_Return_InstanceMethod_Diagnostic()
        {
            await VerifyCSharpWithJsonNetAsync(@"
using System;
using Newtonsoft.Json;

class Blah
{
    public JsonSerializerSettings GetSerializerSettings(bool flag)
    {
        JsonSerializerSettings settings;
        if (flag)
        {
            settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        }
        else
        {
            settings = new JsonSerializerSettings();
        }
        
        return settings;
    }
}",
                GetCSharpResultAt(19, 16, DefinitelyRule));
        }

        [Theory]
        [InlineData("")]
        [InlineData("dotnet_code_quality.excluded_symbol_names = Method")]
        [InlineData(@"dotnet_code_quality.CA2327.excluded_symbol_names = Method
                      dotnet_code_quality.CA2328.excluded_symbol_names = Method")]
        [InlineData("dotnet_code_quality.dataflow.excluded_symbol_names = Method")]
        public async Task EditorConfigConfiguration_ExcludedSymbolNamesOption(string editorConfigText)
        {
            var csharpTest = new VerifyCS.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithNewtonsoftJson,
                TestState =
                {
                    Sources =
                    {
                        @"
using Newtonsoft.Json;

class Blah
{
    object Method(string s)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        return JsonConvert.DeserializeObject(s, settings);
    }
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            };

            if (editorConfigText.Length == 0)
            {
                csharpTest.ExpectedDiagnostics.Add(GetCSharpResultAt(10, 16, DefinitelyRule));
            }

            await csharpTest.RunAsync();
        }

        private async Task VerifyCSharpWithJsonNetAsync(string source, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithNewtonsoftJson,
                TestState =
                {
                    Sources = { source },
                },
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        private async Task VerifyBasicWithJsonNetAsync(string source, params DiagnosticResult[] expected)
        {
            var vbTest = new VerifyVB.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithNewtonsoftJson,
                TestState =
                {
                    Sources = { source },
                },
            };

            vbTest.ExpectedDiagnostics.AddRange(expected);

            await vbTest.RunAsync();
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule)
           => VerifyCS.Diagnostic(rule)
               .WithLocation(line, column);

        private static DiagnosticResult GetBasicResultAt(int line, int column, DiagnosticDescriptor rule)
           => VerifyVB.Diagnostic(rule)
               .WithLocation(line, column);
    }
}
