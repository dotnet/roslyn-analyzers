// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PlatformCompatabilityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PlatformCompatabilityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{

    public partial class PlatformCompatabilityAnalyzerTests
    {
        [Fact]
        public async Task OsDependentMethodsCalledWarns()
        {
            var csSource = @"
using System.Runtime.Versioning;

[SupportedOSPlatform(""Linux"")]
public class Test
{
    public void M1()
    {
        [|WindowsOnly()|];
        [|Obsoleted()|];
        [|Unsupported()|];
        [|ObsoletedOverload()|];
    }
    [UnsupportedOSPlatform(""Linux4.1"")]
    public void Unsupported()
    {
    }
    [SupportedOSPlatform(""Windows10.1.1.1"")]
    public void WindowsOnly()
    {
    }
    [ObsoletedInOSPlatform(""Linux4.1"")]
    public void Obsoleted()
    {
    }
    [ObsoletedInOSPlatform(""Linux4.1"", ""Obsolete message"")]
    public void ObsoletedOverload()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(csSource);

            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    Public Sub M1()
        [|WindowsOnly()|]
        [|Obsoleted()|]
        [|ObsoletedOverload()|]
        [|Unsupported()|]
    End Sub

    <SupportedOSPlatform(""Windows10.1.1.1"")>
    Public Sub WindowsOnly()
    End Sub

    <ObsoletedInOSPlatform(""Linux4.1"")>
    Public Sub Obsoleted()
    End Sub

    <ObsoletedInOSPlatform(""Linux4.1"", ""Obsoleted message"")>
    Public Sub ObsoletedOverload()
    End Sub

    <UnsupportedOSPlatform(""Linux4.1"")>
    Public Sub Unsupported()
    End Sub
End Class
" + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task WrongPlatformStringsShouldHandledGracefully()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public void M1()
    {
        Windows10();
        Windows1_2_3_4_5();
        [|ObsoletedLinuxDash4_1()|];
        [|ObsoletedLinuxStar4_1()|];
        [|UnsupportedLinu4_1()|];
        ObsoletedWithNullString();
        UnsupportedWithEmptyString();
        [|WindowsOnly()|];
    }

    [SupportedOSPlatform(""Windows"")]
    public void WindowsOnly()
    {
    }
    [SupportedOSPlatform(""Windows10"")]
    public void Windows10()
    {
    }
    [SupportedOSPlatform(""Windows1.2.3.4.5"")]
    public void Windows1_2_3_4_5()
    {
    }
    [ObsoletedInOSPlatform(""Linux-4.1"")]
    public void ObsoletedLinuxDash4_1()
    {
    }
    [ObsoletedInOSPlatform(""Linux*4.1"")]
    public void ObsoletedLinuxStar4_1()
    {
    }
    [ObsoletedInOSPlatform(null)]
    public void ObsoletedWithNullString()
    {
    }
    [UnsupportedOSPlatform(""Linu4.1"")]
    public void UnsupportedLinu4_1()
    {
    }
    [UnsupportedOSPlatform("""")]
    public void UnsupportedWithEmptyString()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentPropertyCalledWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""Windows10.1.1"")]
    public string WindowsStringProperty { get; set; }
    [ObsoletedInOSPlatform(""ios4.1"")]
    public int ObsoleteIntProperty { get; set; }
    [UnsupportedOSPlatform(""Linux4.1"")]
    public byte UnsupportedProperty { get; }
    public void M1()
    {
        [|WindowsStringProperty|] = ""Hello"";
        string s = [|WindowsStringProperty|];
        M2([|WindowsStringProperty|]);
        [|ObsoleteIntProperty|] = 5;
        M3([|ObsoleteIntProperty|]);
        M3([|UnsupportedProperty|]);
    }
    public string M2(string option)
    {
        return option;
    }
    public int M3(int option)
    {
        return option;
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Theory]
        [MemberData(nameof(Create_AtrrbiuteProperty_WithCondtions))]
        public async Task OsDependentPropertyConditionalCheckWarns(string attribute, string property, string condition, string setter, string getter)
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [" + attribute + @"(""Windows10.1.1"")]
    public " + property + @" { get; set; }

    public void M1()
    {
        [|" + setter + @";
        var s = [|" + getter + @"|];
        bool check = s " + condition + @";
        M2([|" + getter + @"|]);
    }
    public object M2(object option)
    {
        return option;
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        public static IEnumerable<object[]> Create_AtrrbiuteProperty_WithCondtions()
        {
            yield return new object[] { "SupportedOSPlatform", "string StringProperty", " == [|StringProperty|]", @"StringProperty|] = ""Hello""", "StringProperty" };
            yield return new object[] { "ObsoletedInOSPlatform", "int IntProperty", " > [|IntProperty|]", "IntProperty|] = 5", "IntProperty" };
            yield return new object[] { "UnsupportedOSPlatform", "int UnsupportedProperty", " <= [|UnsupportedProperty|]", "UnsupportedProperty|] = 3", "UnsupportedProperty" };
        }

        [Fact]
        public async Task OsDependentEnumValueCalledWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test2
{
    public void M1()
    {
        PlatformEnum val = [|PlatformEnum.Windows10|];
        M2([|PlatformEnum.Windows10|]);
        M2([|PlatformEnum.Linux48|]);
        M2(PlatformEnum.NoPlatform);
    }
    public PlatformEnum M2(PlatformEnum option)
    {
        return option;
    }
}

public enum PlatformEnum
{
    [SupportedOSPlatform(""windows10.0"")]
    Windows10,
    [SupportedOSPlatform(""linux4.8"")]
    Linux48,
    NoPlatform
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentEnumConditionalCheckNotWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test2
{
    public void M1()
    {
        PlatformEnum val = [|PlatformEnum.Windows10|];
        if (val == PlatformEnum.Windows10)
            return;
        M2([|PlatformEnum.Windows10|]);
        M2([|PlatformEnum.Linux48|]);
        M2(PlatformEnum.NoPlatform);
    }
    public PlatformEnum M2(PlatformEnum option)
    {
        return option;
    }
}

public enum PlatformEnum
{
    [SupportedOSPlatform(""Windows10.0"")]
    Windows10,
    [SupportedOSPlatform(""Linux4.8"")]
    Linux48,
    NoPlatform
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test2
    Public Sub M1()
        Dim val As PlatformEnum = [|PlatformEnum.Windows10|]
        If val = [|PlatformEnum.Windows10|] Then Return
        M2([|PlatformEnum.Windows10|])
        M2([|PlatformEnum.Linux48|])
        M2(PlatformEnum.NoPlatform)
    End Sub

    Public Sub M2(ByVal [option] As PlatformEnum)
    End Sub
End Class

Public Enum PlatformEnum
    <SupportedOSPlatform(""Windows10.0"")>
    Windows10
    < SupportedOSPlatform(""Linux4.8"") >
    Linux48
    NoPlatform
End Enum
" + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task OsDependentFieldCalledWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""Windows10.1.1.1"")]
    string WindowsStringField;
    [SupportedOSPlatform(""Windows10.1.1.1"")]
    public int WindowsIntField { get; set; }
    public void M1()
    {
        Test test = new Test();
        [|WindowsStringField|] = ""Hello"";
        string s = [|WindowsStringField|];
        M2([|test.WindowsStringField|]);
        M2([|WindowsStringField|]);
        M3([|WindowsIntField|]);
    }
    public string M2(string option)
    {
        return option;
    }
    public int M3(int option)
    {
        return option;
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        /*[Fact] TODO: enable the test when preview 8 consumed
        public async Task MethodWithTargetPlatrofrmAttributeDoesNotWarn()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public void M1()
    {
        M2();
    }
    [TargetPlatform(""Windows10.1.1.1"")]
    public void M2()
    {
    }
}
" + MockPlatformApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }*/

        [Fact]
        public async Task OsDependentMethodCalledFromInstanceWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    private B field = new B();
    public void M1()
    {
        [|field.M2()|];
    }
}
public class B
{
    [SupportedOSPlatform(""Windows10.1.1.1"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentMethodCalledFromOtherNsInstanceWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using Ns;

public class Test
{
    private B field = new B();
    public void M1()
    {
        [|field.M2()|];
    }
}

namespace Ns
{
    public class B
    {
        [SupportedOSPlatform(""Windows10.1.1.1"")]
        public void M2()
        {
        }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning
Imports Ns

Public Class Test
    Private field As B = New B()

    Public Sub M1()
        [|field.M2()|]
    End Sub
End Class

Namespace Ns
    Public Class B
        <SupportedOSPlatform(""Windows10.1.1.1"")>
        Public Sub M2()
        End Sub
    End Class
End Namespace
" + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task OsDependentConstructorOfClassUsedCalledWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public void M1()
    {
        C instance = [|new C()|];
        instance.M2();
    }
}

public class C
{
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public C()
    {
    }
    public void M2()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task ConstructorAndMethodOfOsDependentClassCalledWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public void M1()
    {
        OsDependentClass odc = [|new OsDependentClass()|];
        [|odc.M2()|];
    }
}
[SupportedOSPlatform(""Windows10.1.2.3"")]
public class OsDependentClass
{
    public void M2()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    Public Sub M1()
        Dim odc As OsDependentClass = [|New OsDependentClass()|]
        [|odc.M2()|]
    End Sub
End Class

<SupportedOSPlatform(""Windows10.1.2.3"")>
Public Class OsDependentClass
    Public Sub M2()
    End Sub
End Class
" + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task LocalFunctionCallsOsDependentMemberWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public void M1()
    {
        void Test()
        {
            [|M2()|];
        }
        Test();
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task LocalGuardedFunctionCallsOsDependentMemberNotWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""Windows10.2"")]
    public void M1()
    {
        void Test()
        {
            M2();
        }
        Test();
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task LambdaCallsOsDependentMemberWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        void Test() => [|M2()|];
        Test();

        Action action = () =>
        {
            [|M2()|];
        };
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task AttributedLambdaCallsOsDependentMemberNotWarn()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class C
{
    [SupportedOSPlatform(""Windows10.13"")]
    public void M1()
    {
        void Test() => M2();
        Test();

        Action action = () =>
        {
            M2();
        };
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentEventAccessedWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public delegate void Del();

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public event Del SampleEvent;

    public void M1()
    {
        [|SampleEvent|] += M3;
        M2();
    }

    public void M2()
    {
        [|SampleEvent|]?.Invoke();
    }

    public void M3()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    Public Delegate Sub Del()
    <SupportedOSPlatform(""Windows10.1.2.3"")>
    Public Event SampleEvent As Del

    Public Sub M1()
        AddHandler [|SampleEvent|], AddressOf M3
        M2()
    End Sub

    Public Sub M2()
        RaiseEvent  [|SampleEvent|]
    End Sub

    Public Sub M3()
    End Sub
End Class"
+ MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task OsDependentMethodAssignedToDelegateWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public delegate void Del(); // The attribute not supported on delegates, so no tets for that

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void DelegateMethod()
    {
    }
    public void M1()
    {
        Del handler = [|DelegateMethod|];
        handler(); // assume it shouldn't warn here
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        /*[Fact] TODO wait until assembly level APIs merged
        public async Task MethodOfOsDependentAssemblyCalledWithoutSuppressionWarns()
        {
            var source = @"
            using System.Runtime.Versioning;
            using ns;
            public class Test
            {
                public void M1()
                {
                    OsDependentClass odc = new OsDependentClass();
                    odc.M2();
                }
            }
            [assembly:SupportedOSPlatform(""Windows10.1.2.3"")]
            namespace ns
            {
                public class OsDependentClass
                {
                    public void M2()
                    {
                    }
                }
            }
" + MockPlatformApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(RuntimePlatformCheckAnalyzer2.Rule).WithSpan(10, 21, 10, 29).WithArguments("M2", "Windows", "10.1.2.3"));
        }*/

        public static IEnumerable<object[]> SupportedOsAttributeTestData()
        {
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.2.3", false };
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.3.3", false };
            yield return new object[] { "WINDOWS10.1.2.3", "Windows10.1.3", false };
            yield return new object[] { "Windows10.1.2.3", "Windows11.0", false };
            yield return new object[] { "Windows10.1.2.3", "windows10.2.2.0", false };
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.1.3", true };
            yield return new object[] { "Windows10.1.2.3", "WINDOWS11.1.1.3", false };
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.1.4", true };
            yield return new object[] { "MACOS10.1.2.3", "macos10.2.2.0", false };
            yield return new object[] { "OSX10.1.2.3", "Osx11.1.1.0", false };
            yield return new object[] { "Osx10.1.2.3", "osx10.2", false };
            yield return new object[] { "Windows10.1.2.3", "Osx11.1.1.4", true };
            yield return new object[] { "Windows10.1.2.3", "Windows10.0.1.9", true };
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.1.4", true };
            yield return new object[] { "Windows10.1.2.3", "Windows8.2.3.3", true };
        }

        [Theory]
        [MemberData(nameof(SupportedOsAttributeTestData))]
        public async Task MethodOfOsDependentClassSuppressedWithSupportedOsAttribute(string dependentVersion, string suppressingVersion, bool warn)
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""" + suppressingVersion + @""")]
    public void M1()
    {
        OsDependentClass odc = new OsDependentClass();
        odc.M2();
    }
}

[SupportedOSPlatform(""" + dependentVersion + @""")]
public class OsDependentClass
{
    public void M2()
    {
    }
}
" + MockAttributesCsSource;

            if (warn)
            {
                await VerifyAnalyzerAsyncCs(source,
                    VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsVersionRule).WithSpan(9, 32, 9, 54).WithArguments(".ctor", "Windows", "10.1.2.3"),
                    VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsVersionRule).WithSpan(10, 9, 10, 17).WithArguments("M2", "Windows", "10.1.2.3"));
            }
            else
            {
                await VerifyAnalyzerAsyncCs(source);
            }
        }

        [Theory]
        [MemberData(nameof(ObsoletedUnsupportedAttributeTestData))]
        public async Task MethodOfOsDependentClassSuppressedWithObsoleteAttribute(string dependentVersion, string suppressingVersion, bool warn)
        {
            var source = @"
using System.Runtime.Versioning;

[SupportedOSPlatform(""Windows"")]
public class Test
{
    [ObsoletedInOSPlatform(""" + suppressingVersion + @""")]
    public void M1()
    {
        OsDependentClass odc = new OsDependentClass();
        odc.M2();
    }
 }
 
[SupportedOSPlatform(""Windows"")]
[ObsoletedInOSPlatform(""" + dependentVersion + @""")]
public class OsDependentClass
{
     public void M2()
    {
    }
}
" + MockAttributesCsSource;

            if (warn)
            {
                await VerifyAnalyzerAsyncCs(source,
                    VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.ObsoleteOsRule).WithLocation(10, 32).WithArguments(".ctor", "Windows", "10.1.2.3"),
                    VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.ObsoleteOsRule).WithLocation(11, 9).WithArguments("M2", "Windows", "10.1.2.3"));
            }
            else
            {
                await VerifyAnalyzerAsyncCs(source);
            }
        }

        public static IEnumerable<object[]> ObsoletedUnsupportedAttributeTestData()
        {
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.2.3", false };
            yield return new object[] { "Windows10.1.2.3", "MacOs10.1.3.3", true };
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.3.1", true };
            yield return new object[] { "Windows10.1.2.3", "Windows11.1", true };
            yield return new object[] { "Windows10.1.2.3", "Windows10.2.2.0", true };
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.1.3", false };
            yield return new object[] { "Windows10.1.2.3", "WINDOWS10.1.1.3", false };
            yield return new object[] { "Windows10.1.2.3", "Windows10.1.1.4", false };
            yield return new object[] { "Windows10.1.2.3", "Osx10.1.1.4", true };
            yield return new object[] { "windows10.1.2.3", "Windows10.1.0.1", false };
            yield return new object[] { "Windows10.1.2.3", "Windows8.2.3.4", false };
        }

        [Theory]
        [MemberData(nameof(ObsoletedUnsupportedAttributeTestData))]
        public async Task MethodOfOsDependentClassSuppressedWithUnsupportedAttribute(string dependentVersion, string suppressingVersion, bool warn)
        {
            var source = @"
 using System.Runtime.Versioning;
 
[SupportedOSPlatform(""Windows"")]
public class Test
{
    [UnsupportedOSPlatform(""" + suppressingVersion + @""")]
    public void M1()
    {
        OsDependentClass odc = new OsDependentClass();
        odc.M2();
    }
}

[UnsupportedOSPlatform(""" + dependentVersion + @""")]
public class OsDependentClass
{
    public void M2()
    {
    }
}
" + MockAttributesCsSource;

            if (warn)
            {
                await VerifyAnalyzerAsyncCs(source,
                    VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(10, 32).WithArguments(".ctor", "Windows", "10.1.2.3"),
                    VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(11, 9).WithArguments("M2", "Windows", "10.1.2.3"));
            }
            else
            {
                await VerifyAnalyzerAsyncCs(source);
            }
        }

        [Fact]
        public async Task UnsupportedNotWarnsForUnrelatedSupportedContext()
        {
            var source = @"
 using System.Runtime.Versioning;
 
[SupportedOSPlatform(""Linux"")]
public class Test
{
    public void M1()
    {
        var obj = new C();
        obj.BrowserMethod();
        C.StaticClass.LinuxMethod();
        C.StaticClass.LinuxVersionedMethod();
    }
}

public class C
{
    [UnsupportedOSPlatform(""browser"")]
    public void BrowserMethod()
    {
    }
    
    [UnsupportedOSPlatform(""linux4.8"")]
    internal static class StaticClass{
        public static void LinuxVersionedMethod()
        {
        }
        [UnsupportedOSPlatform(""linux"")]
        public static void LinuxMethod()
        {
        }
    }
}
" + MockAttributesCsSource;

            await VerifyAnalyzerAsyncCs(source, VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsRule).WithLocation(11, 9)
                .WithMessage("'LinuxMethod' is not supported or has been removed from linux").WithArguments("LinuxMethod", "linux"),
                 VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(12, 9)
                .WithMessage("'LinuxVersionedMethod' is not supported or has been removed since linux 4.8 version").WithArguments("LinuxMethod", "linux"));

        }

        [Fact]

        public async Task MultipleAttrbiutesOptionallySupportedListTest()
        {
            var source = @"
 using System.Runtime.Versioning;
 
public class Test
{
    C obj = new C();
    [SupportedOSPlatform(""Linux"")]
    public void DiffferentOsNotWarn()
    {
        obj.DoesNotWorkOnWindows();
    }

    [SupportedOSPlatform(""windows"")]
    [UnsupportedOSPlatform(""windows10.0.2000"")]
    public void SupporteWindows()
    {
        // Warns for UnsupportedFirst, Supported and Obsoleted
        obj.DoesNotWorkOnWindows(); 
    }

    [UnsupportedOSPlatform(""windows"")]
    [SupportedOSPlatform(""windows10.1"")]
    [ObsoletedInOSPlatform(""windows10.0.1909"")]
    [UnsupportedOSPlatform(""windows10.0.2003"")]
    public void SameSupportForWindowsNotWarn()
    {
        obj.DoesNotWorkOnWindows();
    }
    
    public void AllSupportedWarnForAll()
    {
        obj.DoesNotWorkOnWindows();
    }

    [SupportedOSPlatform(""windows10.0.2000"")]
    public void SupportedFromWindows10_0_2000()
    {
        // Should warn for [ObsoletedInOSPlatform] and [UnsupportedOSPlatform]
        obj.DoesNotWorkOnWindows();
    }

    [SupportedOSPlatform(""windows10.0.1904"")]
    [UnsupportedOSPlatform(""windows10.0.1909"")]
    public void SupportedWindowsFrom10_0_1904_To10_0_1909_NotWarn()
    {
        // Should not warn
        obj.DoesNotWorkOnWindows();
    }
}

public class C
{
    [UnsupportedOSPlatform(""windows"")]
    [SupportedOSPlatform(""windows10.0.1903"")]
    [ObsoletedInOSPlatform(""windows10.0.1909"")]
    [UnsupportedOSPlatform(""windows10.0.2004"")]
    public void DoesNotWorkOnWindows()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source,
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsRule).WithLocation(18, 9).WithMessage("'DoesNotWorkOnWindows' is not supported or has been removed from windows"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsVersionRule).WithLocation(18, 9).WithMessage("'DoesNotWorkOnWindows' requires windows 10.0.1903 version or later"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.ObsoleteOsRule).WithLocation(18, 9).WithMessage("'DoesNotWorkOnWindows' has been deprecated since windows 10.0.1909 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsRule).WithLocation(32, 9).WithMessage("'DoesNotWorkOnWindows' is not supported or has been removed from windows"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsVersionRule).WithLocation(32, 9).WithMessage("'DoesNotWorkOnWindows' requires windows 10.0.1903 version or later"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.ObsoleteOsRule).WithLocation(32, 9).WithMessage("'DoesNotWorkOnWindows' has been deprecated since windows 10.0.1909 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(39, 9).WithMessage("'DoesNotWorkOnWindows' has been deprecated since windows 10.0.1909 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(39, 9).WithMessage("'DoesNotWorkOnWindows' is not supported or has been removed since windows 10.0.2004 version"));
        }

        [Fact]

        public async Task MultipleAttrbiutesSupportedOnlyWindowsListTest()
        {
            var source = @"
 using System.Runtime.Versioning;
 
public class Test
{
    C obj = new C();
    [SupportedOSPlatform(""Linux"")]
    public void DiffferentOsWarnsForAll()
    {
        obj.DoesNotWorkOnWindows();
    }

    [SupportedOSPlatform(""windows"")]
    [UnsupportedOSPlatform(""windows10.0.2000"")]
    public void SupporteWindows()
    {
        // Warns for Obsoleted version
        obj.DoesNotWorkOnWindows(); 
    }

    [UnsupportedOSPlatform(""windows"")]
    [SupportedOSPlatform(""windows10.1"")]
    [ObsoletedInOSPlatform(""windows10.0.1909"")]
    [UnsupportedOSPlatform(""windows10.0.2003"")]
    public void SameSupportForWindowsNotWarn()
    {
        obj.DoesNotWorkOnWindows();
    }
    
    public void AllSupportedWarnForAll()
    {
        obj.DoesNotWorkOnWindows();
    }

    [SupportedOSPlatform(""windows10.0.2000"")]
    public void SupportedFromWindows10_0_2000()
    {
        // Warns for [ObsoletedInOSPlatform] and [UnsupportedOSPlatform]
        obj.DoesNotWorkOnWindows();
    }
    
    [SupportedOSPlatform(""windows10.0.1904"")]
    [UnsupportedOSPlatform(""windows10.0.1909"")]
    public void SupportedWindowsFrom10_0_1904_To10_0_1909_NotWarn()
    {
        // Should not warn
        obj.DoesNotWorkOnWindows();
    }
}

public class C
{
    [SupportedOSPlatform(""windows"")]
    [ObsoletedInOSPlatform(""windows10.0.1909"")]
    [UnsupportedOSPlatform(""windows10.0.2004"")]
    public void DoesNotWorkOnWindows()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source,
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsRule).WithLocation(10, 9).WithMessage("'DoesNotWorkOnWindows' requires windows"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsVersionRule).WithLocation(10, 9).WithMessage("'DoesNotWorkOnWindows' has been deprecated since windows 10.0.1909 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsVersionRule).WithLocation(10, 9).WithMessage("'DoesNotWorkOnWindows' is not supported or has been removed since windows 10.0.2004 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.ObsoleteOsRule).WithLocation(18, 9).WithMessage("'DoesNotWorkOnWindows' has been deprecated since windows 10.0.1909 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RequiredOsRule).WithLocation(32, 9).WithMessage("'DoesNotWorkOnWindows' requires windows"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.ObsoleteOsRule).WithLocation(32, 9).WithMessage("'DoesNotWorkOnWindows' has been deprecated since windows 10.0.1909 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(32, 9).WithMessage("'DoesNotWorkOnWindows' is not supported or has been removed since windows 10.0.2004 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(39, 9).WithMessage("'DoesNotWorkOnWindows' has been deprecated since windows 10.0.1909 version"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.UnsupportedOsVersionRule).WithLocation(39, 9).WithMessage("'DoesNotWorkOnWindows' is not supported or has been removed since windows 10.0.2004 version"));
        }

        private static VerifyCS.Test PopulateTestCs(string sourceCode, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
                TestState = { }
            };
            test.ExpectedDiagnostics.AddRange(expected);
            return test;
        }

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode) => await PopulateTestCs(sourceCode).RunAsync();

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode, params DiagnosticResult[] expectedDiagnostics)
            => await PopulateTestCs(sourceCode, expectedDiagnostics).RunAsync();

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode, string additionalFiles)
        {
            var test = PopulateTestCs(sourceCode);
            test.TestState.AdditionalFiles.Add((".editorconfig", additionalFiles));
            await test.RunAsync();
        }

        private static async Task VerifyAnalyzerAsyncVb(string sourceCode) => await PopulateTestVb(sourceCode).RunAsync();

        private static VerifyVB.Test PopulateTestVb(string sourceCode, params DiagnosticResult[] expected)
        {
            var test = new VerifyVB.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            return test;
        }

        private readonly string MockAttributesCsSource = @"
namespace System.Runtime.Versioning
{
    public abstract class OSPlatformAttribute : Attribute
    {
        private protected OSPlatformAttribute(string platformName)
        {
            PlatformName = platformName;
        }

        public string PlatformName { get; }
    }

    [AttributeUsage(AttributeTargets.Assembly,
                    AllowMultiple = false, Inherited = false)]
    public sealed class TargetPlatformAttribute : OSPlatformAttribute
    {
        public TargetPlatformAttribute(string platformName) : base(platformName)
        { }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Event |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
                    AttributeTargets.Field |
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    public sealed class SupportedOSPlatformAttribute : OSPlatformAttribute
    {
        public SupportedOSPlatformAttribute(string platformName) : base(platformName)
        { }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Event |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
                    AttributeTargets.Field |
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    public sealed class UnsupportedOSPlatformAttribute : OSPlatformAttribute
    {
        public UnsupportedOSPlatformAttribute(string platformName) : base(platformName)
        { }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Event |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
                    AttributeTargets.Field |
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    public sealed class ObsoletedInOSPlatformAttribute: OSPlatformAttribute
    {
        public ObsoletedInOSPlatformAttribute(string platformName) : base(platformName)
        { }
        public ObsoletedInOSPlatformAttribute(string platformName, string message) : base(platformName)
        {
            Message = message;
        }
        public string Message { get; }
        public string Url { get; set; }
    }
}
";

        private readonly string MockAttributesVbSource = @"
Namespace System.Runtime.Versioning
    Public MustInherit Class OSPlatformAttribute
        Inherits Attribute

        Private Protected Sub New(ByVal platformName As String)
            PlatformName = platformName
        End Sub

        Public ReadOnly Property PlatformName As String
    End Class

    <AttributeUsage(AttributeTargets.Assembly, AllowMultiple:=False, Inherited:=False)>
    Public NotInheritable Class TargetPlatformAttribute
        Inherits OSPlatformAttribute

        Public Sub New(ByVal platformName As String)
            MyBase.New(platformName)
        End Sub
    End Class

    <AttributeUsage(AttributeTargets.Assembly Or AttributeTargets.[Class] Or AttributeTargets.Constructor Or AttributeTargets.[Event] Or AttributeTargets.Method Or AttributeTargets.[Module] Or AttributeTargets.[Property] Or AttributeTargets.Field Or AttributeTargets.Struct, AllowMultiple:=True, Inherited:=False)>
    Public NotInheritable Class SupportedOSPlatformAttribute
        Inherits OSPlatformAttribute

        Public Sub New(ByVal platformName As String)
            MyBase.New(platformName)
        End Sub
    End Class

    <AttributeUsage(AttributeTargets.Assembly Or AttributeTargets.[Class] Or AttributeTargets.Constructor Or AttributeTargets.[Event] Or AttributeTargets.Method Or AttributeTargets.[Module] Or AttributeTargets.[Property] Or AttributeTargets.Field Or AttributeTargets.Struct, AllowMultiple:=True, Inherited:=False)>
    Public NotInheritable Class UnsupportedOSPlatformAttribute
        Inherits OSPlatformAttribute

        Public Sub New(ByVal platformName As String)
            MyBase.New(platformName)
        End Sub
    End Class

    <AttributeUsage(AttributeTargets.Assembly Or AttributeTargets.[Class] Or AttributeTargets.Constructor Or AttributeTargets.[Event] Or AttributeTargets.Method Or AttributeTargets.[Module] Or AttributeTargets.[Property] Or AttributeTargets.Field Or AttributeTargets.Struct, AllowMultiple:=True, Inherited:=False)>
    Public NotInheritable Class ObsoletedInOSPlatformAttribute
        Inherits OSPlatformAttribute

        Public Sub New(ByVal platformName As String)
            MyBase.New(platformName)
        End Sub

        Public Sub New(ByVal platformName As String, ByVal message As String)
            MyBase.New(platformName)
            Message = message
        End Sub

        Public ReadOnly Property Message As String
        Public Property Url As String
    End Class
End Namespace
";
    }
}
