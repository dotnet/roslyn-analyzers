// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PlatformCompatibilityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PlatformCompatibilityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public partial class PlatformCompatabilityAnalyzerTests
    {
        private const string s_msBuildPlatforms = "build_property._SupportedPlatformList=windows,browser, ios;\nbuild_property.TargetFramework=net5.0";

        [Fact(Skip = "TODO need to be fixed: Test for for wrong arguments, not sure how to report the Compiler error diagnostic")]
        public async Task TestOsPlatformAttributesWithNonStringArgument()
        {
            var csSource = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    [[|SupportedOSPlatform(""Linux"", ""Windows"")|]]
    public void MethodWithTwoArguments()
    {
    }
    [UnsupportedOSPlatform([|new string[]{""Linux"", ""Windows""}|])]
    public void MethodWithArrayArgument()
    {
    }
}
" + MockAttributesCsSource;

            await VerifyAnalyzerAsyncCs(csSource);
        }

        public static IEnumerable<object[]> Create_DifferentTfms()
        {
            yield return new object[] { "build_property.TargetFramework = net472", false };
            yield return new object[] { "build_property.TargetFramework = netcoreapp1.0", false };
            yield return new object[] { "build_property.TargetFramework = dotnet", false };
            yield return new object[] { "build_property.TargetFramework = uap10.0", false };
            yield return new object[] { "build_property.TargetFramework = netstandard2.1", false };
            yield return new object[] { "build_property.TargetFramework = net5", true };
            yield return new object[] { "build_property.TargetFramework = net5.0", true };
            yield return new object[] { "build_property.TargetFramework = net5.0-windows", true };
            yield return new object[] { "build_property.TargetFramework = net5.0-ios14.0", true };
            yield return new object[] { "build_property.TargetFramework = Net99", true };
            yield return new object[] { "build_property.TargetFramework = netcoreapp5", false };
        }

        [Theory]
        [MemberData(nameof(Create_DifferentTfms))]
        public async Task Net5OrHigherTfmWarns_LowerThanNet5NotWarn(string tfm, bool warn)
        {
            var invocation = warn ? "[|Target.WindowsOnlyMethod()|]" : "Target.WindowsOnlyMethod()";
            var source = @"
using System.Runtime.Versioning;

namespace CallerTargetsBelow5_0
{
    class Caller
    {
        public static void TestWindowsOnlyMethod()
        {
            " + invocation + @";
        }
    }

    class Target
    {
        [SupportedOSPlatform(""windows"")]
        public static void WindowsOnlyMethod() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, tfm);
        }

        public static IEnumerable<object[]> Create_DifferentTfmsWithOption()
        {
            yield return new object[] { "build_property.TargetFramework = net472\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=true", true };
            yield return new object[] { "build_property.TargetFramework = net472\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", false };
            yield return new object[] { "build_property.TargetFramework = netcoreapp1.0\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=true", true };
            yield return new object[] { "build_property.TargetFramework = netcoreapp1.0\ndotnet_code_quality.CA1416.enable_platform_analyzer_on_pre_net5_target=false", false };
            yield return new object[] { "build_property.TargetFramework = dotnet\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=true", true };
            yield return new object[] { "build_property.TargetFramework = uap10.0\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", false };
            yield return new object[] { "build_property.TargetFramework = netstandard2.1\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=true", true };
            yield return new object[] { "build_property.TargetFramework = netstandard2.1\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", false };
            yield return new object[] { "build_property.TargetFramework = net5\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", true };
            yield return new object[] { "build_property.TargetFramework = net5.0\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", true };
            yield return new object[] { "build_property.TargetFramework = net5.0-windows\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", true };
            yield return new object[] { "build_property.TargetFramework = net5.0-ios14.0\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", true };
            yield return new object[] { "build_property.TargetFramework = net6.0\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", true };
            yield return new object[] { "build_property.TargetFramework = netcoreapp5\ndotnet_code_quality.enable_platform_analyzer_on_pre_net5_target=false", false };
        }

        [Theory]
        [MemberData(nameof(Create_DifferentTfmsWithOption))]
        public async Task Net5OrHigherTfmWarns_LowerThanNet5WarnsIfEnabled(string tfmAndOption, bool warn)
        {
            var invocation = warn ? "[|Target.WindowsOnlyMethod()|]" : "Target.WindowsOnlyMethod()";
            var source = @"
using System.Runtime.Versioning;

namespace CallerTargetsBelow5_0
{
    class Caller
    {
        public static void TestWindowsOnlyMethod()
        {
            " + invocation + @";
        }
    }

    class Target
    {
        [SupportedOSPlatform(""windows"")]
        public static void WindowsOnlyMethod() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, tfmAndOption);
        }

        [Fact]
        public async Task OsDependentMethodsCalledWarns()
        {
            var csSource = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""Linux"")]
    public void M1()
    {
        [|WindowsOnly()|];
        [|Unsupported()|];
    }
    
    [UnsupportedOSPlatform(""Linux4.1"")]
    public void Unsupported()
    {
    }
    [SupportedOSPlatform(""Windows10.1.1.1"")]
    public void WindowsOnly()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(csSource);

            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    <SupportedOSPlatform(""Linux"")>
    Public Sub M1()
        [|WindowsOnly()|]
        [|Unsupported()|]
    End Sub

    <SupportedOSPlatform(""Windows10.1.1.1"")>
    Public Sub WindowsOnly()
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
        [|UnsupportedOSPlatformIosDash4_1()|];
        [|UnsupportedOSPlatformIosStar4_1()|];
        [|SupportedLinu4_1()|];
        UnsupportedOSPlatformWithNullString();
        UnsupportedWithEmptyString();
        [|NotForWindows()|];
        UnattributedFunction();
    }
    public void UnattributedFunction() { }
    [UnsupportedOSPlatform(""Windows"")]
    public void NotForWindows()
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
    [SupportedOSPlatform(""Ios-4.1"")]
    public void UnsupportedOSPlatformIosDash4_1()
    {
    }
    [SupportedOSPlatform(""Ios*4.1"")]
    public void UnsupportedOSPlatformIosStar4_1()
    {
    }
    [SupportedOSPlatform(null)]
    public void UnsupportedOSPlatformWithNullString()
    {
    }
    [SupportedOSPlatform(""Linu4.1"")]
    public void SupportedLinu4_1()
    {
    }
    [UnsupportedOSPlatform("""")]
    public void UnsupportedWithEmptyString()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
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
    [UnsupportedOSPlatform(""Linux4.1"")]
    public byte UnsupportedProperty { get; }
    [SupportedOSPlatform(""Linux"")]
    public void M1()
    {
        [|WindowsStringProperty|] = ""Hello"";
        string s = [|WindowsStringProperty|];
        M2([|WindowsStringProperty|]);
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

        [Fact, WorkItem(4071, "https://github.com/dotnet/roslyn-analyzers/issues/4071")]
        public async Task OsDependentPropertyGetterSetterCalledWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""windows"")]
    public static bool WindowsOnlyProperty
    {   
        get { return true; }
        set { }
    }
    public static bool WindowsOnlyPropertyGetter
    {
        [SupportedOSPlatform(""windows"")]
        get { return true; }
        set { }
    }

    public static bool WindowsOnlyPropertySetter
    {
        get { return true; }
        [SupportedOSPlatform(""windows"")]
        set { }
    }

    public void M1()
    {
        WindowsOnlyPropertyGetter = true;
        var s = [|WindowsOnlyPropertyGetter|];
        [|WindowsOnlyPropertyGetter|] |= true;
        [|WindowsOnlyPropertySetter|] &= false;
        [|WindowsOnlyPropertySetter|] = false;
        s = WindowsOnlyPropertySetter;
        M2([|WindowsOnlyPropertyGetter|]);
        M2(WindowsOnlyPropertySetter);
        var name = nameof(WindowsOnlyPropertyGetter);
        name = nameof(WindowsOnlyPropertySetter);
        name = nameof([|WindowsOnlyProperty|]);
    }
    public bool M2(bool option)
    {
        return option;
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    <SupportedOSPlatform(""windows"")>
    Public Shared Property WindowsOnlyProperty As Boolean
        Get
            Return True
        End Get
        Set(ByVal value As Boolean)
        End Set
    End Property

    Public Shared Property WindowsOnlyPropertyGetter As Boolean
        <SupportedOSPlatform(""windows"")>
        Get
            Return True
        End Get
        Set(ByVal value As Boolean)
        End Set
    End Property

    Public Shared Property WindowsOnlyPropertySetter As Boolean
        Get
            Return True
        End Get
        <SupportedOSPlatform(""windows"")>
        Set(ByVal value As Boolean)
        End Set
    End Property

    Public Sub M1()
        WindowsOnlyPropertyGetter = True
        Dim s = [|WindowsOnlyPropertyGetter|]
        WindowsOnlyPropertyGetter = [|WindowsOnlyPropertyGetter|] Or True
        [|WindowsOnlyPropertySetter|] = WindowsOnlyPropertySetter And False
        [|WindowsOnlyPropertySetter|] = False
        s = WindowsOnlyPropertySetter
        M2([|WindowsOnlyPropertyGetter|])
        Dim name = NameOf(WindowsOnlyPropertyGetter)
    End Sub

    Public Function M2(ByVal[option] As Boolean) As Boolean
        Return[option]
    End Function
End Class
" + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Theory]
        [MemberData(nameof(Create_AttributeProperty_WithCondtions))]
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
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
        }

        public static IEnumerable<object[]> Create_AttributeProperty_WithCondtions()
        {
            yield return new object[] { "SupportedOSPlatform", "string StringProperty", " == [|StringProperty|]", @"StringProperty|] = ""Hello""", "StringProperty" };
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
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
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
    public int WindowsIntField;
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

        [Fact(Skip = "TODO: enable when tests could consume new TargetPlatform attribute, (preview 8)")]
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
" + MockAttributesCsSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

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
        public async Task LocalFunctionEscapedCallsOsDependentMemberWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        void Test()
        {
            [|M2()|];
        }

        M3(Test);
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }

    public void M3(Action a) { a(); }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task LocalFunctionUnusedCallsOsDependentMemberWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        void Test()
        {
            [|M2()|];
        }
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
        public async Task LambdaEscapedCallsOsDependentMemberWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        Action a = () =>
        {
            [|M2()|];
        };

        M3(a);
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }

    public void M3(Action a) { a(); }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task LambdaUnusedCallsOsDependentMemberWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        Action a = () =>
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
        public async Task EventOfOsDependentTypeAccessedWarns()
        {
            var source = @"
using System;
using System.Runtime.Versioning;
[SupportedOSPlatform(""windows"")]
public class Test
{
    public static event EventHandler WindowsOnlyEvent
    {
        add { }
        remove { }
    }
}
public class C
{
    public static void WindowsEventHandler(object sender, EventArgs e) { }
    public void M1()
    {
        [|Test.WindowsOnlyEvent|] += WindowsEventHandler;
        [|Test.WindowsOnlyEvent|] -= WindowsEventHandler;
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentEventAddRemoveAccessedWarns()
        {
            var source = @"
using System;
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""windows"")]
    public static event EventHandler WindowsOnlyEvent
    {
        add { }
        remove { }
    }

    public static event EventHandler WindowsOnlyEventAdd
    {
        [SupportedOSPlatform(""windows"")]
        add { }
        remove { }
    }

    public static event EventHandler WindowsOnlyEventRemove
    {
        add { }
        [SupportedOSPlatform(""windows"")]
        remove { }
    }

    public static void WindowsEventHandler(object sender, EventArgs e) { }

    public void M1()
    {
        [|WindowsOnlyEvent|] += WindowsEventHandler;
        [|WindowsOnlyEventAdd|] += WindowsEventHandler;
        WindowsOnlyEventRemove += WindowsEventHandler;

        [|WindowsOnlyEvent|] -= WindowsEventHandler;
        WindowsOnlyEventAdd -= WindowsEventHandler;
        [|WindowsOnlyEventRemove|] -= WindowsEventHandler;
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentMethodAssignedToDelegateWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public delegate void Del(); // The attribute not supported on delegates, so no tests for that

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void DelegateMethod()
    {
    }
    public void M1()
    {
        Del handler = [|DelegateMethod|];
        handler();
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact, WorkItem(4168, "https://github.com/dotnet/roslyn-analyzers/issues/4168")]
        public async Task UnsupportedShouldNotSuppressSupportedWithSameVersion()
        {
            var source = @"
using System.Runtime.Versioning;
static class Program
{
    public static void Main()
    {
        [|UnsupportedOnBrowserType.SupportedOnWindowsIosTvos()|]; // 'UnsupportedOnBrowserType.SupportedOnWindowsIosTvos()' is only supported on: 'ios', 'tvos', 'windows', but this call site is reachable on all platforms.
        [|UnsupportedOnBrowserType.SupportedOnTvos4()|];          // 'UnsupportedOnBrowserType.SupportedOnTvos4()' is only supported on: 'tvos' 4.0 and later, but this call site is reachable on all platforms.
        UnsupportedOnBrowserType.SupportedOnBrowser();
    }
}
[UnsupportedOSPlatform(""browser"")]
class UnsupportedOnBrowserType
{
    [SupportedOSPlatform(""windows"")]
    [SupportedOSPlatform(""ios"")]
    [SupportedOSPlatform(""tvos"")] 
    public static void SupportedOnWindowsIosTvos() {}
    [SupportedOSPlatform(""browser2.0"")] 
    public static void SupportedOnBrowser() {}
    [SupportedOSPlatform(""tvos4.0"")]    
    public static void SupportedOnTvos4() {}
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact, WorkItem(4168, "https://github.com/dotnet/roslyn-analyzers/issues/4168")]
        public async Task CallSitesUnsupportedShouldNotSuppressSupportedWithSameVersion()
        {
            var source = @"
using System.Runtime.Versioning;

[UnsupportedOSPlatform(""browser"")]
static class Program
{
    public static void Main()
    {
        [|UnsupportedOnBrowserType.SupportedOnWindowsIosTvos()|]; // 'UnsupportedOnBrowserType.SupportedOnWindowsIosTvos()' is only supported on: 'windows', 'ios', 'tvos', but this call site is reachable on all platforms.
        [|UnsupportedOnBrowserType.SupportedOnTvos4()|];  // 'UnsupportedOnBrowserType.SupportedOnTvos4()' is only supported on: 'tvos' 4.0 and later, but this call site is reachable on all platforms.
        UnsupportedOnBrowserType.SupportedOnBrowser(); // child support ignored (should not extend)
        UnsupportedOnBrowserType.UnsupportedOnBrowser1_0();
    }
}

[SupportedOSPlatform(""browser"")]
static class Program2
{
    public static void Main()
    {
        [|UnsupportedOnBrowserType.SupportedOnWindowsIosTvos()|]; // 'UnsupportedOnBrowserType.SupportedOnWindowsIosTvos()' is only supported on: 'windows', 'ios', 'tvos', but this call site is reachable on 'browser'.
        [|UnsupportedOnBrowserType.SupportedOnTvos4()|];      // 'UnsupportedOnBrowserType.SupportedOnTvos4()' is unsupported on: 'browser', but this call site is reachable on 'browser'.
        [|UnsupportedOnBrowserType.SupportedOnBrowser()|];    // only warns for parent attribute
        [|UnsupportedOnBrowserType.UnsupportedOnBrowser1_0()|];
    }
}

[UnsupportedOSPlatform(""browser"")]
[SupportedOSPlatform(""windows"")]
static class Program3
{
    public static void Main()
    {
        UnsupportedOnBrowserType.SupportedOnWindowsIosTvos(); // No diagnostics expected
        [|UnsupportedOnBrowserType.SupportedOnTvos4()|];  // 'UnsupportedOnBrowserType.SupportedOnTvos4()' is only supported on: 'tvos' 4.0 and later, but this call site is reachable on 'windows'.
        UnsupportedOnBrowserType.SupportedOnBrowser();
        UnsupportedOnBrowserType.UnsupportedOnBrowser1_0();
    }
}

[UnsupportedOSPlatform(""browser"")]
class UnsupportedOnBrowserType
{
    [SupportedOSPlatform(""windows"")]
    [SupportedOSPlatform(""ios"")]
    [SupportedOSPlatform(""tvos"")] 
    public static void SupportedOnWindowsIosTvos() {}
    [SupportedOSPlatform(""browser2.0"")] 
    public static void SupportedOnBrowser() {}
    [UnsupportedOSPlatform(""browser1.0"")] 
    public static void UnsupportedOnBrowser1_0() {}
    [SupportedOSPlatform(""tvos4.0"")]    
    public static void SupportedOnTvos4() {}
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact, WorkItem(4168, "https://github.com/dotnet/roslyn-analyzers/issues/4168")]
        public async Task CallSiteUnsupportedShouldNotSuppressSupportedWithSameVersionAndSameLevel()
        {
            var source = @"
using System.Runtime.Versioning;

[UnsupportedOSPlatform(""browser"")]
static class Program
{
    public static void Main()
    {
        [|UnsupportedOnBrowserSupportedOnWindowType.SupportedOnIosTvos()|]; // One diagnostics expected only for parent
        [|UnsupportedOnBrowserSupportedOnWindowType.SupportedOnTvos4()|];   // 'UnsupportedOnBrowserSupportedOnWindowType.SupportedOnTvos4()' is only supported on: 'windows', but this call site is reachable on all platforms.
    }
}

[SupportedOSPlatform(""browser"")]
static class Program2
{
    public static void Main()
    {
        [|UnsupportedOnBrowserSupportedOnWindowType.SupportedOnIosTvos()|]; // 'UnsupportedOnBrowserSupportedOnWindowType.SupportedOnIosTvos()' is unsupported on: 'browser', but this call site is reachable on 'browser'.
        [|UnsupportedOnBrowserSupportedOnWindowType.SupportedOnTvos4()|]; // Same here 
    }
}

[SupportedOSPlatform(""windows"")]
static class Program3
{
    public static void Main()
    {
        UnsupportedOnBrowserSupportedOnWindowType.SupportedOnIosTvos(); // No diagnostics expected
        UnsupportedOnBrowserSupportedOnWindowType.SupportedOnTvos4();
    }
}

[UnsupportedOSPlatform(""browser"")]
[SupportedOSPlatform(""windows"")]
class UnsupportedOnBrowserSupportedOnWindowType
{
    [SupportedOSPlatform(""ios"")]
    [SupportedOSPlatform(""tvos"")]  // these attributes will be ignored
    public static void SupportedOnIosTvos() {}
    [SupportedOSPlatform(""tvos4.0"")]    // same here
    public static void SupportedOnTvos4() {}
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task CallerSupportsSubsetOfTarget()
        {
            var source = @"
using System.Runtime.Versioning;

namespace CallerSupportsSubsetOfTarget
{
    class Caller
    {
        [SupportedOSPlatform(""windows"")]
        public static void Test()
        {
            Target.SupportedOnWindows();
            [|Target.SupportedOnBrowser()|];
            Target.SupportedOnWindowsAndBrowser();
        }
    }

    class Target
    {
        [SupportedOSPlatform(""windows"")]
        public static void SupportedOnWindows() { }

        [SupportedOSPlatform(""browser"")]
        public static void SupportedOnBrowser() { }
        [SupportedOSPlatform(""browser""), SupportedOSPlatform(""windows"")]
        public static void SupportedOnWindowsAndBrowser() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task CallerUnsupportsNonSubsetOfTargetSupport()
        {
            var source = @"
using System.Runtime.Versioning;

namespace CallerUnsupportsNonSubsetOfTarget
{
    class Caller
    {
        [UnsupportedOSPlatform(""browser"")]
        public static void TestWithBrowserUnsupported()
        {
            [|Target.UnsupportedOnWindowsUntilWindows11()|];
        }
    }
    class Target
    {
        [UnsupportedOSPlatform(""windows""), SupportedOSPlatform(""windows11.0"")]
        public static void UnsupportedOnWindowsUntilWindows11() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
        }

        [Fact]
        public async Task CallerUnsupportsSubsetOfTargetUsupportedFirstThenSupportsNotWarn()
        {
            var source = @"
using System.Runtime.Versioning;

namespace CallerUnsupportsSubsetOfTarget
{
    class Caller
    {
        [UnsupportedOSPlatform(""windows"")]
        public void TestUnsupportedOnWindows()
        {
            // Call site unsupporting Windows, means the call site supports all other platforms 
            // It is calling into code that was NOT supported only on Windows, but eventually added support,
            // as it was only not supported window supporting it later doesn' matter for call site that is 
            // not supporting windows at all, so it shouldn't raise diagnostic
            TargetUnsupportedOnWindows.FunctionSupportedOnWindows1(); // should not warn
            TargetUnsupportedOnWindows.FunctionSupportedOnWindowsSameVersion();
        }
    }
    [UnsupportedOSPlatform(""windows"")]
    class TargetUnsupportedOnWindows
    {
        [SupportedOSPlatform(""windows1.0"")]
        public static void FunctionSupportedOnWindows1() { }
        
        [SupportedOSPlatform(""windows"")]
        public static void FunctionSupportedOnWindowsSameVersion() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
        }

        [Fact]
        public async Task CallerUnsupportsNonSubsetOfTargetUnsupportedFirstSupportsWarns()
        {
            var source = @"
using System.Runtime.Versioning;

namespace CallerUnsupportsNonSubsetOfTarget
{
    class Caller
    {
        [UnsupportedOSPlatform(""browser"")]
        public void TestUnsupportedOnWindows()
        {
            [|TargetUnsupportedOnWindows.FunctionSupportedOnWindows1()|];
            [|TargetUnsupportedOnWindows.FunctionSupportedOnWindowsSameVersion()|];
        }
    }
    [UnsupportedOSPlatform(""windows"")]
    class TargetUnsupportedOnWindows
    {
        [SupportedOSPlatform(""windows1.0"")]
        public static void FunctionSupportedOnWindows1() { }
        
        [SupportedOSPlatform(""windows"")]
        public static void FunctionSupportedOnWindowsSameVersion() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
        }

        [Fact]
        public async Task CallerSupportsSupersetOfTarget_AnotherScenario()
        {
            var source = @"
using System.Runtime.Versioning;

namespace CallerSupportsSubsetOfTarget
{
    class Caller
    {
        [UnsupportedOSPlatform(""browser"")]
        public static void TestWithBrowserUnsupported()
        {
            [|Target.SupportedOnWindows()|];  // 'Target.SupportedOnWindows()' is only supported on: 'windows', but this call site is reachable on all platforms.
            [|Target.SupportedOnBrowser()|];  // 'Target.SupportedOnBrowser()' is only supported on: 'browser', but this call site is unreachable on 'browser'.
            [|Target.SupportedOnWindowsAndBrowser()|]; // 'Target.SupportedOnWindowsAndBrowser()' is only supported on: 'browser', 'windows', but this call site is unreachable on 'browser'.
            [|Target.SupportedOnWindowsAndUnsupportedOnBrowser()|]; // 'Target.SupportedOnWindowsAndUnsupportedOnBrowser()' is only supported on: 'windows', but this call site is reachable on all platforms.

            [|Target.UnsupportedOnWindows()|];   // Target.UnsupportedOnWindows()' is unsupported on: 'windows', but this call site is reachable on all platforms.
            [|Target.UnsupportedOnWindows11()|]; // 'Target.UnsupportedOnWindows11()' is unsupported on: 'windows' from version 11.0 and later, but this call site is reachable on all platforms.
            Target.UnsupportedOnBrowser();
            [|Target.UnsupportedOnWindowsAndBrowser()|];     // 'Target.UnsupportedOnWindowsAndBrowser()' is unsupported on: 'windows', but this call site is reachable on all platforms.
            [|Target.UnsupportedOnWindowsUntilWindows11()|]; // 'Target.UnsupportedOnWindowsUntilWindows11()' is only supported on: 'windows' 11.0 and later, but this call site is reachable on all platforms.
        }
    }

    class Target
    {
        [SupportedOSPlatform(""windows"")]
        public static void SupportedOnWindows() { }

        [SupportedOSPlatform(""browser"")]
        public static void SupportedOnBrowser() { }

        [SupportedOSPlatform(""windows""), SupportedOSPlatform(""browser"")]
        public static void SupportedOnWindowsAndBrowser() { }

        [UnsupportedOSPlatform(""windows"")]
        public static void UnsupportedOnWindows() { }

        [UnsupportedOSPlatform(""windows11.0"")]
        public static void UnsupportedOnWindows11() { }

        [UnsupportedOSPlatform(""browser"")]
        public static void UnsupportedOnBrowser() { }

        [UnsupportedOSPlatform(""windows""), UnsupportedOSPlatform(""browser"")]
        public static void UnsupportedOnWindowsAndBrowser() { }

        [SupportedOSPlatform(""windows""), UnsupportedOSPlatform(""browser"")]
        public static void SupportedOnWindowsAndUnsupportedOnBrowser() { }

        [UnsupportedOSPlatform(""windows""), SupportedOSPlatform(""windows11.0"")]
        public static void UnsupportedOnWindowsUntilWindows11() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
        }

        [Fact]
        public async Task CallerSupportsSupersetOfTarget()
        {
            var source = @"
using System.Runtime.Versioning;

namespace CallerSupportsSubsetOfTarget
{
    class Caller
    {
        [SupportedOSPlatform(""windows""), SupportedOSPlatform(""browser"")]
        public static void TestWithWindowsAndBrowserSupported()
        {
            [|Target.SupportedOnWindows()|];  // 'Target.SupportedOnWindows()' is only supported on: 'windows', but this call site is reachable on 'windows', 'browser'.
            [|Target.SupportedOnBrowser()|];  // 'Target.SupportedOnBrowser()' is only supported on: 'browser', but this call site is reachable on 'windows', 'browser'.
            Target.SupportedOnWindowsAndBrowser();

            [|Target.UnsupportedOnWindows()|];  // 'Target.UnsupportedOnWindows()' is unsupported on: 'windows', but this call site is reachable on 'windows', 'browser'.
            [|Target.UnsupportedOnBrowser()|];
            [|Target.UnsupportedOnWindowsAndBrowser()|]; // 'Target.UnsupportedOnWindowsAndBrowser()' is unsupported on: 'windows', 'browser', but this call site is reachable on 'windows', 'browser'.
        }
        [UnsupportedOSPlatform(""browser"")]
        public static void TestWithBrowserUnsupported()
        {
            [|Target.SupportedOnWindows()|];  // 'Target.SupportedOnWindows()' is only supported on: 'windows', but this call site is reachable on all platforms.
            [|Target.SupportedOnBrowser()|];  // 'Target.SupportedOnBrowser()' is only supported on: 'browser', but this call site is unreachable on 'browser'.
            [|Target.SupportedOnWindowsAndBrowser()|]; // 'Target.SupportedOnWindowsAndBrowser()' is only supported on: 'browser', 'windows', but this call site is unreachable on 'browser'.

            Target.UnsupportedOnWindows();  // if call site has now support of it and MSbuild list not containg the platform name it will not be warned
            Target.UnsupportedOnBrowser(); 
            Target.UnsupportedOnWindowsAndBrowser(); // same here
        }
    }

    class Target
    {
        [SupportedOSPlatform(""windows"")]
        public static void SupportedOnWindows() { }

        [SupportedOSPlatform(""browser"")]
        public static void SupportedOnBrowser() { }

        [SupportedOSPlatform(""windows""), SupportedOSPlatform(""browser"")]
        public static void SupportedOnWindowsAndBrowser() { }

        [UnsupportedOSPlatform(""windows"")]
        public static void UnsupportedOnWindows() { }

        [UnsupportedOSPlatform(""browser"")]
        public static void UnsupportedOnBrowser() { }

        [UnsupportedOSPlatform(""windows""), UnsupportedOSPlatform(""browser"")]
        public static void UnsupportedOnWindowsAndBrowser() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task UnsupportedSamePlatformMustSuppressSupported()
        {
            var source = @"
using System.Runtime.Versioning;

static class Program
{
    public static void Main()
    {
        [|Some.Api1()|]; // 'Some.Api1()' is only supported on: 'ios' 10.0 and later, 'tvos' 4.0 and later, but this call site is reachable on all platforms.
        [|Some.Api2()|]; // 'Some.Api2()' is only supported on: 'ios' 10.0 and later, but this call site is reachable on all platforms.
    }
}

[SupportedOSPlatform(""ios10.0"")]
[SupportedOSPlatform(""tvos4.0"")]
class Some
{
    public static void Api1() {}

    [UnsupportedOSPlatform(""tvos"")] // tvos suppressed, only warn for ios10.0
    public static void Api2() {}
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task PlatformOverrides()
        {
            var source = @"
using System.Runtime.Versioning;

namespace PlatformCompatDemo.Bugs
{
    class Caller
    {
        [SupportedOSPlatform(""windows"")]
        public void TestSupportedOnWindows()
        {
            [|TargetSupportedOnWindows.FunctionUnsupportedOnWindows()|]; // 'TargetSupportedOnWindows.FunctionUnsupportedOnWindows()' is unsupported on: 'windows', but this call site is reachable on 'windows'.
            TargetSupportedOnWindows.FunctionUnsupportedOnBrowser();     // browser unsupport not related so ignored

            [|TargetUnsupportedOnWindows.FunctionSupportedOnWindows()|]; // 'TargetUnsupportedOnWindows.FunctionSupportedOnWindows()' is unsupported on: 'windows', but this call site is reachable on 'windows'
            [|TargetUnsupportedOnWindows.FunctionSupportedOnBrowser()|]; // 'TargetUnsupportedOnWindows.FunctionSupportedOnWindows()' is unsupported on: 'windows', but this call site is reachable on 'windows'                                   
        }

        [UnsupportedOSPlatform(""windows"")]
        public void TestUnsupportedOnWindows()
        {
            TargetSupportedOnWindows.FunctionUnsupportedOnWindows();
            [|TargetSupportedOnWindows.FunctionUnsupportedOnBrowser()|];  // 'TargetSupportedOnWindows.FunctionUnsupportedOnBrowser()' is only supported on: 'windows', but this call site is unreachable on 'windows'.

            [|TargetUnsupportedOnWindows.FunctionSupportedOnBrowser()|];  // 'TargetUnsupportedOnWindows.FunctionSupportedOnBrowser()' is only supported on: 'browser', but this call site is reachable on all platforms.
            TargetUnsupportedOnWindows.FunctionSupportedOnWindows();      // Function supported will be ignored as cannot widen the support
        }                                                                 
    }

    [SupportedOSPlatform(""windows"")]
    class TargetSupportedOnWindows
    {
        [UnsupportedOSPlatform(""windows"")]  // Not  Ignored
        public static void FunctionUnsupportedOnWindows() { }

        [UnsupportedOSPlatform(""browser"")]  // Will be ignored ignored
        public static void FunctionUnsupportedOnBrowser() { }
    }

    [UnsupportedOSPlatform(""windows"")]
    class TargetUnsupportedOnWindows
    {
        [SupportedOSPlatform(""windows"")] // will be ignored
        public static void FunctionSupportedOnWindows() { }

        [SupportedOSPlatform(""browser"")]
        public static void FunctionSupportedOnBrowser() { }
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task ChildUnsupportedMustParentSupportedPlatformMustNotIgnored()
        {
            var source = @"
using System.Runtime.Versioning;
[assembly:SupportedOSPlatform(""browser"")]
namespace PlatformCompatDemo
{
    static class Program
    {
        public static void Main()
        {
            [|CrossPlatformApis.DoesNotWorkOnBrowser()|];
            CrossPlatformApis.NormalFunction();
            var nonBrowser = new NonLinuxApis();
        }
    }

    public class CrossPlatformApis
    {
        [UnsupportedOSPlatform(""browser"")]
        public static void DoesNotWorkOnBrowser() { }
        public static void NormalFunction() { }
    }

    [UnsupportedOSPlatform(""linux"")] // must be ignored
    public class NonLinuxApis { }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task SupportedMustSuppressUnsupportedAssemblyAttribute()
        {
            var source = @"
using System.Runtime.Versioning;
[assembly:UnsupportedOSPlatform(""browser"")]

namespace PlatformCompatDemo
{
    static class Program
    {
        public static void Main()
        {
            [|CrossPlatformApis.WindowsApi()|];
            var nonBrowser = new BrowserApis();
        }
    }

    public class CrossPlatformApis
    {
        [SupportedOSPlatform(""windows"")]
        public static void WindowsApi() { }
    }

    [SupportedOSPlatform(""browser"")]
    public class BrowserApis
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task UsingUnsupportedApiWithinAllowListShouldWarn()
        {
            var source = @"
using System.Runtime.Versioning;
[assembly: SupportedOSPlatform(""windows"")]

static class Program
{
    public static void Main()
    {
        new SomeWindowsSpecific();
        [|SomeWindowsSpecific.NotForWindows()|];
    }
}

class SomeWindowsSpecific
{
    [UnsupportedOSPlatform(""windows"")] // This will not be ignored
    public static void NotForWindows() { }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task UsingVersionedApiFromAllowListAssemblyNotIgnored()
        {
            var source = @"
using System.Runtime.Versioning;
[assembly: SupportedOSPlatform(""windows"")]

static class Program
{
    public static void Main()
    {
        [|Some.Windows10SpecificApi()|];
        WindowsSpecificApi();
    }
    public static void WindowsSpecificApi()
    {
    }
}

[SupportedOSPlatform(""windows10.0"")] // This attribute will not be ignored
static class Some
{
    public static void Windows10SpecificApi()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task ReintroducingHigherApiSupport_Warn()
        {
            var source = @"
using System.Runtime.Versioning;
[assembly: SupportedOSPlatform(""windows10.0"")]

static class Program
{
    public static void Main()
    {
        [|Some.WindowsSpecificApi11()|];
        Some.WindowsSpecificApi1();
    }
}

static class Some
{
    [SupportedOSPlatform(""windows11.0"")]
    public static void WindowsSpecificApi11()
    {
    }

    [SupportedOSPlatform(""windows1.0"")]
    public static void WindowsSpecificApi1()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact(Skip = "TODO: enable when tests could consume new attribute applied to runtime assemblies")]
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
" + MockAttributesCsSource;
            await VerifyCS.VerifyAnalyzerAsync(source, VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.OnlySupportedOsRule).
                WithSpan(10, 21, 10, 29).WithArguments("M2", "Windows", "10.1.2.3"));
        }

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
            var invokeMethod = warn ? "[|OsDependentClass.M2()|]" : "OsDependentClass.M2()";
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""" + suppressingVersion + @""")]
    public void M1()
    {
        " + invokeMethod + @";
    }
}

[SupportedOSPlatform(""" + dependentVersion + @""")]
public class OsDependentClass
{
    public static void M2()
    {
    }
}
" + MockAttributesCsSource;

            await VerifyAnalyzerAsyncCs(source);
        }

        public static IEnumerable<object[]> UnsupportedAttributeTestData()
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
        [MemberData(nameof(UnsupportedAttributeTestData))]
        public async Task MethodOfOsDependentClassSuppressedWithUnsupportedAttribute(string dependentVersion, string suppressingVersion, bool warn)
        {
            var invokeConstructor = warn ? "[|new OsDependentClass()|]" : "new OsDependentClass()";
            var source = @"
 using System.Runtime.Versioning;
 
[UnsupportedOSPlatform(""" + suppressingVersion + @""")]
public class Test
{
    public void M1()
    {
        OsDependentClass odc = " + invokeConstructor + @";
    }
}

[UnsupportedOSPlatform(""" + dependentVersion + @""")]
public class OsDependentClass { }
" + MockAttributesCsSource;

            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms);
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
        [|C.StaticClass.LinuxMethod()|];  // 'C.StaticClass.LinuxMethod()' is unsupported on: 'linux', but this call site is reachable on 'Linux'.
        [|C.StaticClass.LinuxVersionedMethod()|]; // 'C.StaticClass.LinuxVersionedMethod()' is unsupported on: 'linux' from version 4.8 and later, but this call site is reachable on 'Linux'.
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

            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]

        public async Task OptionallySupportedListDifferentCallsitesTest()
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
        obj.DoesNotWorkOnWindows();  // 'C.DoesNotWorkOnWindows()' is supported on: 'windows' from version 10.0.1903 and later, but this call site is reachable on 'windows' 10.0.2000 and before.
    }

    [UnsupportedOSPlatform(""windows"")]
    [SupportedOSPlatform(""windows10.1"")]
    [UnsupportedOSPlatform(""windows10.0.2003"")]
    public void SameSupportForWindowsNotWarn()
    {
        obj.DoesNotWorkOnWindows();
    }
    
    public void AllSupportedWarnForAll()
    {
        obj.DoesNotWorkOnWindows();  // 'C.DoesNotWorkOnWindows()' is supported on: 'windows' from version 10.0.1903 to 10.0.2004, but this call site is reachable on all platforms.
    }

    [SupportedOSPlatform(""windows10.0.2000"")]
    public void SupportedFromWindows10_0_2000()
    {
        // Should warn for [UnsupportedOSPlatform]
        obj.DoesNotWorkOnWindows();  // 'C.DoesNotWorkOnWindows()' is unsupported on: 'windows' from version 10.0.2004 and later, but this call site is reachable on 'windows' 10.0.2000 and later.
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
    [UnsupportedOSPlatform(""windows10.0.2004"")]
    public void DoesNotWorkOnWindows()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source, s_msBuildPlatforms,
                VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.SupportedOsRule).WithSpan(17, 9, 17, 35).
                WithArguments("C.DoesNotWorkOnWindows()", "'windows' from version 10.0.1903 and later, but this call site is reachable on 'windows' 10.0.2000 and before."),
                VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.SupportedOsRule).WithSpan(30, 9, 30, 35).
                WithArguments("C.DoesNotWorkOnWindows()", "'windows' from version 10.0.1903 to 10.0.2004, but this call site is reachable on all platforms."),
                VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.UnsupportedOsRule).WithSpan(37, 9, 37, 35).
                WithArguments("C.DoesNotWorkOnWindows()", "'windows' from version 10.0.2004 and later, but this call site is reachable on 'windows' 10.0.2000 and later."));
        }

        [Fact]

        public async Task SupportedOnlyOnWindowsListWithDifferentCallsitesTest()
        {
            var source = @"
 using System.Runtime.Versioning;
 
public class Test
{
    C obj = new C();
    [SupportedOSPlatform(""Linux"")]
    public void DiffferentOsWarnsForAll()
    {
        obj.WindowsOnlyMethod();  // 'C.WindowsOnlyMethod()' is only supported on: 'windows' 10.0.2004 and before, but this call site is reachable on 'Linux'.
    }

    [SupportedOSPlatform(""windows"")]
    [UnsupportedOSPlatform(""windows10.0.2003"")]
    public void SameSupportForWindowsNotWarn()
    {
        obj.WindowsOnlyMethod();
    }
    
    public void AllSupportedWarnForAll()
    {
        obj.WindowsOnlyMethod();  // 'C.WindowsOnlyMethod()' is only supported on: 'windows' 10.0.2004 and before, but this call site is reachable on all platforms.
    }

    [SupportedOSPlatform(""windows10.0.2000"")]
    public void SupportedFromWindows10_0_2000()
    {
        // Warns for [UnsupportedOSPlatform]
        obj.WindowsOnlyMethod();  // 'C.WindowsOnlyMethod()' is unsupported on: 'windows' from version 10.0.2004 and later, but this call site is reachable on 'windows' 10.0.2000 and later.
    }
    
    [SupportedOSPlatform(""windows10.0.1904"")]
    [UnsupportedOSPlatform(""windows10.0.1909"")]
    public void SupportedWindowsFrom10_0_1904_To10_0_1909_NotWarn()
    {
        // Should not warn
        obj.WindowsOnlyMethod();
    }
}

public class C
{
    [SupportedOSPlatform(""windows"")]
    [UnsupportedOSPlatform(""windows10.0.2004"")]
    public void WindowsOnlyMethod()
    {
    }
}
" + MockAttributesCsSource;
            await VerifyAnalyzerAsyncCs(source,
                VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.OnlySupportedOsRule).WithSpan(10, 9, 10, 32).
                WithArguments("C.WindowsOnlyMethod()", "'windows' 10.0.2004 and before, but this call site is reachable on 'Linux'."),
                VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.OnlySupportedOsRule).WithSpan(22, 9, 22, 32).
                WithArguments("C.WindowsOnlyMethod()", "'windows' 10.0.2004 and before, but this call site is reachable on all platforms."),
                VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.UnsupportedOsRule).WithSpan(29, 9, 29, 32).
                WithArguments("C.WindowsOnlyMethod()", "'windows' from version 10.0.2004 and later, but this call site is reachable on 'windows' 10.0.2000 and later."));
        }

        [Fact]
        public async Task CallSiteSupportedUnupportedNoMsBuildOptions()
        {
            var source = @"
 using System.Runtime.Versioning;

namespace PlatformCompatDemo.SupportedUnupported 
{
    public class Test
    {
        public static void Supported()
        {
            var supported = new TypeWithoutAttributes();
            [|supported.FunctionSupportedOnWindows()|];
            [|supported.FunctionSupportedOnWindows10()|];
            [|supported.FunctionSupportedOnWindows10AndBrowser()|];

            var supportedOnWindows = [|new TypeSupportedOnWindows()|];
            [|supportedOnWindows.FunctionSupportedOnBrowser()|]; // browser support ignored
            [|supportedOnWindows.FunctionSupportedOnWindows11AndBrowser()|];

            var supportedOnBrowser = [|new TypeSupportedOnBrowser()|];
            [|supportedOnBrowser.FunctionSupportedOnWindows()|];

            var supportedOnWindows10 = [|new TypeSupportedOnWindows10()|];
            [|supportedOnWindows10.FunctionSupportedOnBrowser()|]; // child function support will be ignored

            var supportedOnWindowsAndBrowser = [|new TypeSupportedOnWindowsAndBrowser()|];
            [|supportedOnWindowsAndBrowser.FunctionSupportedOnWindows11()|];
        }

        public static void Unsupported()
        {
            var unsupported = new TypeWithoutAttributes();
            unsupported.FunctionUnsupportedOnWindows();
            unsupported.FunctionUnsupportedOnBrowser();
            unsupported.FunctionUnsupportedOnWindows10();
            unsupported.FunctionUnsupportedOnWindowsAndBrowser();
            unsupported.FunctionUnsupportedOnWindows10AndBrowser();

            var unsupportedOnWindows = new TypeUnsupportedOnWindows();
            unsupportedOnWindows.FunctionUnsupportedOnBrowser();
            unsupportedOnWindows.FunctionUnsupportedOnWindows11();
            unsupportedOnWindows.FunctionUnsupportedOnWindows11AndBrowser();

            var unsupportedOnBrowser = new TypeUnsupportedOnBrowser();
            unsupportedOnBrowser.FunctionUnsupportedOnWindows();
            unsupportedOnBrowser.FunctionUnsupportedOnWindows10();

            var unsupportedOnWindows10 = new TypeUnsupportedOnWindows10();
            unsupportedOnWindows10.FunctionUnsupportedOnBrowser();
            unsupportedOnWindows10.FunctionUnsupportedOnWindows11();
            unsupportedOnWindows10.FunctionUnsupportedOnWindows11AndBrowser();

            var unsupportedOnWindowsAndBrowser = new TypeUnsupportedOnWindowsAndBrowser();
            unsupportedOnWindowsAndBrowser.FunctionUnsupportedOnWindows11();

            var unsupportedOnWindows10AndBrowser = new TypeUnsupportedOnWindows10AndBrowser();
            unsupportedOnWindows10AndBrowser.FunctionUnsupportedOnWindows11();
        }

        public static void UnsupportedCombinations() // no any diagnostics as it is deny list
        {
            var withoutAttributes = new TypeWithoutAttributes();
            withoutAttributes.FunctionUnsupportedOnWindowsSupportedOnWindows11();
            withoutAttributes.FunctionUnsupportedOnWindowsSupportedOnWindows11UnsupportedOnWindows12();
            withoutAttributes.FunctionUnsupportedOnWindowsSupportedOnWindows11UnsupportedOnWindows12SupportedOnWindows13();

            var unsupportedOnWindows = new TypeUnsupportedOnWindows();
            unsupportedOnWindows.FunctionSupportedOnWindows11();
            unsupportedOnWindows.FunctionSupportedOnWindows11UnsupportedOnWindows12();
            unsupportedOnWindows.FunctionSupportedOnWindows11UnsupportedOnWindows12SupportedOnWindows13();

            var unsupportedOnBrowser = new TypeUnsupportedOnBrowser();
            unsupportedOnBrowser.FunctionSupportedOnBrowser();

            var unsupportedOnWindowsSupportedOnWindows11 = new TypeUnsupportedOnWindowsSupportedOnWindows11();
            unsupportedOnWindowsSupportedOnWindows11.FunctionUnsupportedOnWindows12();
            unsupportedOnWindowsSupportedOnWindows11.FunctionUnsupportedOnWindows12SupportedOnWindows13();

            var unsupportedOnWindowsSupportedOnWindows11UnsupportedOnWindows12 = new TypeUnsupportedOnWindowsSupportedOnWindows11UnsupportedOnWindows12();
            unsupportedOnWindowsSupportedOnWindows11UnsupportedOnWindows12.FunctionSupportedOnWindows13();
        }
    }
}
" + TargetTypesForTest + MockAttributesCsSource;

            await VerifyAnalyzerAsyncCs(source);
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

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode, params DiagnosticResult[] expectedDiagnostics)
            => await VerifyAnalyzerAsyncCs(sourceCode, "build_property.TargetFramework = net5", expectedDiagnostics);

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode, string editorconfigText, params DiagnosticResult[] expectedDiagnostics)
        {
            var test = PopulateTestCs(sourceCode, expectedDiagnostics);
            test.TestState.AdditionalFiles.Add((".editorconfig", editorconfigText));
            await test.RunAsync();
        }

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode, string editorconfigText)
        {
            var test = PopulateTestCs(sourceCode);
            test.TestState.AdditionalFiles.Add((".editorconfig", editorconfigText));
            await test.RunAsync();
        }

        private static async Task VerifyAnalyzerAsyncVb(string sourceCode, params DiagnosticResult[] expectedDiagnostics)
            => await VerifyAnalyzerAsyncVb(sourceCode, "build_property.TargetFramework = net5", expectedDiagnostics);

        private static async Task VerifyAnalyzerAsyncVb(string sourceCode, string editorconfigText, params DiagnosticResult[] expectedDiagnostics)
        {
            var test = PopulateTestVb(sourceCode, expectedDiagnostics);
            test.TestState.AdditionalFiles.Add((".editorconfig", editorconfigText));
            await test.RunAsync();
        }

        private static VerifyVB.Test PopulateTestVb(string sourceCode, params DiagnosticResult[] expected)
        {
            var test = new VerifyVB.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
                TestState = { },
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
End Namespace
";
    }
}
