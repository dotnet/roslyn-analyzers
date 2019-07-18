// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotUseInsecureSettingsForJsonNetTests : DiagnosticAnalyzerTestBase
    {
        private static readonly DiagnosticDescriptor DefinitelyRule = DoNotUseInsecureSettingsForJsonNet.DefinitelyInsecureSettings;
        private static readonly DiagnosticDescriptor MaybeRule = DoNotUseInsecureSettingsForJsonNet.MaybeInsecureSettings;

        [Fact]
        public void Insecure_JsonConvert_DeserializeObject_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonConvert_DeserializeAnonymousType_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonSerializer_Create_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Secure_JsonSerializer_CreateDefault_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonConvert_DefaultSettings_Lambda_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonConvert_DefaultSettings_Lambda_ImplicitReturn_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonConvert_DefaultSettings_LocalFunction_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonConvert_DefaultSettings_LocalFunctionWithTryCatch_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonConvert_DefaultSettings_LocalFunction_CapturedVariables_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_JsonConvert_DefaultSettings_NestedLocalFunction_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_FieldInitialization_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
}",
                GetCSharpResultAt(6, 60, DefinitelyRule));
        }

        [Fact]
        public void Secure_FieldInitialization_SerializationBinderSet_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
}");
        }

        [Fact]
        public void Secure_FieldInitialization_BinderSet_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
}");
        }

        [Fact]
        public void Insecure_PropertyInitialization_DefinitelyDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using Newtonsoft.Json;

class Blah
{
    public static JsonSerializerSettings Settings { get; } = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
}",
                GetCSharpResultAt(6, 60, DefinitelyRule));
        }

        [Fact]
        public void Insecure_PropertyInitialization_MaybeDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
using System;
using Newtonsoft.Json;

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
                GetCSharpResultAt(13, 60, MaybeRule));
        }

        [Fact]
        public void Insecure_Lazy_Field_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_Instance_Constructor_Initializer_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_Instance_Constructor_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_Instance_Constructor_Interprocedural_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void InsecureButNotInitialized_Instance_Constructor_Interprocedural_LValuesWithMoreThanOneCapturedOperation_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Unknown_PropertyInitialized_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void UnknownThenNull_PropertyInitialized_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void UnknownOrNull_PropertyInitialized_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void InsecureThenNull_PropertyInitialized_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void InsecureThenSecure_PropertyInitialized_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void SecureThenInsecure_FieldInitialized_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void InsecureOrNull_PropertyInitialized_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void InsecureOrSecure_PropertyInitialized_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_Field_Initialized_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_UnusedLocalVariable_NoDiagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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
        public void Insecure_Return_InstanceMethod_Diagnostic()
        {
            this.VerifyCSharpWithJsonNet(@"
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

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureSettingsForJsonNet();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureSettingsForJsonNet();
        }

        private void VerifyCSharpWithJsonNet(string source, params DiagnosticResult[] expected)
        {
            this.VerifyCSharpAcrossTwoAssemblies(NewtonsoftJsonNetApis.CSharp, source, expected);
        }
    }
}
