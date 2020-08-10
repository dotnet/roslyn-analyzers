// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public partial class PlatformCompatabilityAnalyzerTests
    {
        [Fact]
        public async Task GuardedWith_IsOSPlatform_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }
    }

    [SupportedOSPlatform(""Windows"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;

            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCalled_SimpleIf_NotWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 1, 2, 3))
            M2();
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning
Imports System

Public Class Test
    Public Sub M1()
        [|M2()|]
        If OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 1, 2, 3) Then M2()
    End Sub

    <SupportedOSPlatform(""Windows10.1.2.3"")>
    Public Sub M2()
    End Sub
End Class
" + MockAttributesVbSource + MockRuntimeApiSourceVb;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task GuardedCall_MultipleSimpleIfTests()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 1, 2, 3))
            M2();
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 10, 1, 2, 3))
            [|M2()|];
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
            M2();
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 8, 1, 2, 3))
            [|M2()|];        
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedWith_IsOSPlatformVersionAtLeast_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;

            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedWith_IsOSPlatformEarlierThan_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    [SupportedOSPlatform(""Windows"")]
    void M1()
    {
        if(OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(11, 0, 19222))
        {
            [|M2()|];
            M3();
        }
        else
        {
            [|M2()|];
            [|M3()|];
        }
    }

    [SupportedOSPlatform(""MacOs12.2.3"")]
    void M2()
    {
    }

    [SupportedOSPlatform(""Windows"")]
    [ObsoletedInOSPlatform(""Windows10.0"")]
    void M3 ()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        /*[Fact] TODO fix VB test
        public async Task GuardedWith_StringOverload_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    [SupportedOSPlatform(""Windows"")]
    void M1()
    {
        if(OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(10, 0, 19222))
        {
            [|M2()|];
            M3();
        }
        else
        {
            [|M2()|];
            [|M3()|];
        }

        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Android"",12,1))
        {
            [|M3()|];
            M2();
        }
        else
        {
            [|M2()|];
            [|M3()|];
        }
    }

    [SupportedOSPlatform(""Android10.2.3"")]
    void M2()
    {
    }
    [SupportedOSPlatform(""Windows"")]
    [ObsoletedInOSPlatform(""Windows10.1.2.3"")]
    void M3 ()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning
Imports System

Class Test
    Private Sub M1()
        If OperatingSystemHelper.IsOSPlatformEarlierThan(""Windows10.1"") Then
            [|M2()|]
            M3()
        Else
            [|M2()|]
            [|M3()|]
        End If

        If OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows10.1.3"") Then
            [|M3()|]
            M2()
        Else
            [|M2()|]
            [|M3()|]
        End If
    End Sub

    <SupportedOSPlatform(""Windows10.1.2.3"")>
    Private Sub M2()
    End Sub

    <ObsoletedInOSPlatform(""Windows10.1.2.3"")>
    Private Sub M3()
    End Sub
End Class
" + MockAttributesVbSource + MockRuntimeApiSourceVb;
            await VerifyAnalyzerAsyncVb(vbSource);
        }*/

        [Fact]
        public async Task OsDependentEnumValue_GuardedCall_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test2
{
    public void M1()
    {
        PlatformEnum val = [|PlatformEnum.Windows10|];
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10))
        {
            M2(PlatformEnum.Windows10);
        }
        else
        {
            M2([|PlatformEnum.Windows10|]);
        }
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
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentProperty_GuardedCall_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    [UnsupportedOSPlatform(""Windows8.1"")]
    public string RemovedProperty { get; set;}
    
    public void M1()
    {
        if(OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(8, 0, 19222)) //OperatingSystemHelper.IsOSPlatformEarlierThan 
        {
            RemovedProperty = ""Hello"";
            string s = RemovedProperty;
            M2(RemovedProperty);
        }
        else
        {
            [|RemovedProperty|] = ""Hello"";
            string s = [|RemovedProperty|];
            M2([|RemovedProperty|]);
        }
    }

    public string M2(string option)
    {
        return option;
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task OsDependentConstructorOfClass_GuardedCall_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
        {
            C instance = new C();
            instance.M2();
        }
        else
        {   
            C instance2 = [|new C()|];
            instance2.M2();
        }
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
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task ConstructorAndMethodOfOsDependentClass_GuardedCall_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
        {
            OsDependentClass odc = new OsDependentClass();
            odc.Method2();
        }
        else
        {
            OsDependentClass odc2 = [|new OsDependentClass()|];
            [|odc2.Method2()|];
        }
    }
}
[SupportedOSPlatform(""Windows10.1.2.3"")]
public class OsDependentClass
{
    public void Method2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning
Imports System

Public Class Test
    Public Sub M1()
        If OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) Then
            Dim odc As OsDependentClass = New OsDependentClass()
            odc.M2()
        Else
            Dim odc2 As OsDependentClass = [|New OsDependentClass()|]
            [|odc2.M2()|]
        End If
    End Sub
End Class

<SupportedOSPlatform(""Windows10.1.2.3"")>
Public Class OsDependentClass
    Public Sub M2()
    End Sub
End Class
" + MockRuntimeApiSourceVb + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task LocalFunctionCallsOsDependentMember_GuardedCall_SimpleIfElse()
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
            if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2, 1))
            {
                M2();
            }
            else
            {
                [|M2()|];
            }
        }
        Test();
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        /*[Fact] // TODO: Need to be fixed
        public async Task LambdaCallsOsDependentMember_GuardedCall_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;
using System;

public class Test
{
    public void M1()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2, 1))
        {
            void Test() => M2();
            Test();
        }
        else
        {
            void Test() => [|M2()|];
            Test();
        }

        Action action = () =>
        {
            if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2, 1))
            {
                M2();
            }
            else
            {
                [|M2()|];
            }
        };
        action.Invoke();
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }*/

        [Fact]
        public async Task OsDependentEventAccessed_GuardedCall_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public delegate void Del();

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public event Del SampleEvent;

    public void M1()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            SampleEvent += M3;
        }
        else
        {
            [|SampleEvent|] += M4;
        }
    }

    public void M2()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            SampleEvent?.Invoke();
        }
        else
        {
            [|SampleEvent|]?.Invoke();
        }
    }

    public void M3()
    {
    }
    public void M4()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        /*[Fact] // TODO: need to be fixed
        public async Task OsDependentMethodAssignedToDelegate_GuardedCall_SimpleIfElse()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public delegate void Del();

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void DelegateMethod()
    {
    }
    public void M1()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            Del handler = DelegateMethod;
            handler();
        }
        else
        {
            Del handler = [|DelegateMethod|];
            handler();
        }
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }*/

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfElseTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        [|M2()|];

        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            M2();
        }
        else if(OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(8, 0, 19222))
        {
            [|M2()|];
        }
        else if(OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222))
        {
            [|M2()|];
        }
        else if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 12))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseTestWithNegation()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(!OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 1, 2, 3))
            [|M2()|];
        else
            M2();
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfElseTestWithNegation()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(!OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 1, 2, 3))
            [|M2()|];
        else if(OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222))
            M2();
        else
            M2();
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfTestWithNegationAndReturn()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(!OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 1, 2, 3))
            return;
        M2();
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning
Imports System

Public Class Test
    Public Sub M1()
        [|M2()|]
        If Not OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 1, 2, 3) Then Return
        M2()
    End Sub

    <SupportedOSPlatform(""Windows10.1.2.3"")>
    Public Sub M2()
    End Sub
End Class
" + MockAttributesVbSource + MockRuntimeApiSourceVb;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfTestWithLogicalAnd()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) &&
           (OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)))
        {
            M2();
        }

        if((OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)) &&
           OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 12))
        {
            M2();
        }

        if((OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)) &&
           OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 12))
        {
            [|M2()|];
        }

        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) && 1 == 1)
        {
            M2();
        }

        [|M2()|];
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseTestWithLogicalAnd()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];

        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) &&
           (OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }

        if((OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)) &&
           OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 12))
        {
            [|M2()|];
        }
        else
        {
            [|M2()|];
        }
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfTestWithLogicalOr()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) ||
           (OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)))
        {
            [|M2()|];
        }

        if((OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)) || 
            OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
        {
            [|M2()|];
        }

        if(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 12) || 
            OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
        {
            [|M2()|];
        }

        [|M2()|];
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseTestWithLogicalOr()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];

        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) ||
           (OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)))
        {
            [|M2()|];
        }
        else
        {
            [|M2()|];
        }

        if((OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222)) || 
            OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
        {
            [|M2()|];
        }
        else
        {
            [|M2()|];
        }

        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) ||
           OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            [|M2()|]; // Even it is not meaningful check i think it is a bug, it shouldn't warn
        }
        else
        {
            [|M2()|];
        }
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfElseTestWithLogicalOr()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

public class Test
{
    public void M1()
    {
        [|M2()|];

        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2) ||
           OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 5, 1))
        {
            [|M2()|];
        }
        else if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 9))
        {
            [|M2()|];
        }
        else
            [|M2()|];

        if(OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222) || 
            OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
        {
            [|M2()|];
        }
        else if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }
    }
    [SupportedOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfTestWithLogicalOrAnd()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        [|M2()|];

        if((OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 1) ||
            OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 1)) &&
            (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 12) ||
            OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 2)))
        {
            [|M2()|]; 
        }
        else if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 13) ||
                 OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 3) ||
                 OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 4))
        {
            [|M2()|];        
        }
        else
        {
            [|M2()|];
        }
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}"
+ MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning
Imports System

Class Test
    Private Sub M1()
        If (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 1) OrElse OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 1)) AndAlso (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 12) OrElse OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 2)) Then
            [|M2()|]
        ElseIf OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 13) OrElse OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 3) OrElse OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 4) Then
            [|M2()|]
        Else
            [|M2()|]
        End If
    End Sub

    <SupportedOSPlatform(""Windows10.1.2.3"")>
    Private Sub M2()
    End Sub
End Class
" + MockRuntimeApiSourceVb + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task GuardedWith_ControlFlowAndMultipleChecks()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 8))
        {
            [|M2()|];

            if (OperatingSystemHelper.IsWindows() && !OperatingSystemHelper.IsWindowsVersionAtLeast(12, 0, 19222))
            {
                [|M2()|];
            }
            else if (!OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2, 1))
            {
                [|M2()|];
            } 
            else
            {
                M2();
            }

            [|M2()|];
        }
        else
        {
            [|M2()|];
        }

        [|M2()|];
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;

            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedWith_DebugAssertAnalysisTest()
        {
            var source = @"
using System.Diagnostics;
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        [|M2()|];

        Debug.Assert(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2));

        M2();
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Diagnostics
Imports System.Runtime.Versioning
Imports System

Class Test
    Private Sub M1()
        [|M2()|]
        Debug.Assert(OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 10, 2))
        M2()
    End Sub

    <SupportedOSPlatform(""Windows10.1.2.3"")>
    Private Sub M2()
    End Sub
End Class
" + MockAttributesVbSource + MockRuntimeApiSourceVb;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task GuardedWith_ResultSavedInLocal()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        var x1 = OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11);
        var x2 = OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Linux"", 1);

        if (x1)
        {
            M2();
        }

        if (x1 || x2)
        {
            [|M2()|];
        }

        if (x2)
            [|M2()|];
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task GuardedWith_VersionSavedInLocal()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        var v11 = 11;
        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", v11))
        {
            M2();
        }
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task PlatformSavedInLocal_NotYetSupported() // TODO do we want to support it?
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        var platform = ""Windows"";
        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(platform, 11))
        {
            [|M2()|];
        }
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);
        }

        [Fact]
        public async Task UnrelatedConditionCheckDoesNotInvalidateState()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1(bool flag1, bool flag2)
    {
        [|M2()|];

        if (OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11))
        {
            M2();

            if (flag1 || flag2)
            {
                M2();
            }
            else
            {
                M2();
            }
            
            M2();
        }

        if (flag1 || flag2)
        {
            [|M2()|];
        }
        else
        {
            [|M2()|];
        }
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesCsSource + MockRuntimeApiSource;
            await VerifyAnalyzerAsyncCs(source);

            var vbSource = @"
Imports System.Runtime.Versioning
Imports System

Class Test
    Private Sub M1(ByVal flag1 As Boolean, ByVal flag2 As Boolean)
        [|M2()|]

        If OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"", 11) Then
            M2()

            If flag1 OrElse flag2 Then
                M2()
            Else
                M2()
            End If

            M2()
        End If

        If flag1 OrElse flag2 Then
            [|M2()|]
        Else
            [|M2()|]
        End If
    End Sub

    <SupportedOSPlatform(""Windows10.1.2.3"")>
    Private Sub M2()
    End Sub
End Class
" + MockRuntimeApiSourceVb + MockAttributesVbSource;
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        /*[Fact] //TODO: Not working anymore, fix this
        public async Task InterproceduralAnalysisTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System;

class Test
{
    void M1()
    {
        [|M2()|];

        if (IsWindows11OrLater())
        {
            M2();    
        }

        [|M2()|]; 
    }

    [SupportedOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }

    bool IsWindows11OrLater()
    {
        return OperatingSystemHelper.IsOSPlatformVersionAtLeast(""Windows"",10,2,3,4);
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyAnalyzerAsyncCs(source, @"{ ("".editorconfig"", ""dotnet_code_quality.interprocedural_analysis_kind = ContextSensitive"") }");
        }*/

        private readonly string MockRuntimeApiSource = @"
namespace System
{
    public sealed class OperatingSystemHelper
    {
#pragma warning disable CA1801, IDE0060 // Review unused parameters
        public static bool IsOSPlatform(string platform) { return true; }
        public static bool IsOSPlatformVersionAtLeast(string platform, int major, int minor = 0, int build = 0, int revision = 0) { return true; }
        public static bool IsBrowser() { return true; }
        public static bool IsLinux() { return true; }
        public static bool IsFreeBSD() { return true; }
        public static bool IsFreeBSDVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0) { return true; }
        public static bool IsAndroid() { return true; }
        public static bool IsAndroidVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0) { return true; }
        public static bool IsIOS() { return true; }
        public static bool IsIOSVersionAtLeast(int major, int minor = 0, int build = 0) { return true; }
        public static bool IsMacOS() { return true; }
        public static bool IsMacOSVersionAtLeast(int major, int minor = 0, int build = 0) { return true; }
        public static bool IsTvOS() { return true; }
        public static bool IsTvOSVersionAtLeast(int major, int minor = 0, int build = 0) { return true; }
        public static bool IsWatchOS() { return true; }
        public static bool IsWatchOSVersionAtLeast(int major, int minor = 0, int build = 0) { return true; }
        public static bool IsWindows() { return true; }
        public static bool IsWindowsVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0) { return true; }
#pragma warning restore CA1801, IDE0060 // Review unused parameters
    }
}";

        private readonly string MockRuntimeApiSourceVb = @"
Namespace System
    Public NotInheritable Class OperatingSystemHelper
        Public Shared Function IsOSPlatform(ByVal platform As String) As Boolean
            Return True
        End Function

        Public Shared Function IsOSPlatformVersionAtLeast(ByVal platform As String, ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0, ByVal Optional revision As Integer = 0) As Boolean
            Return True
        End Function

        Public Shared Function IsBrowser() As Boolean
            Return True
        End Function

        Public Shared Function IsLinux() As Boolean
            Return True
        End Function

        Public Shared Function IsFreeBSD() As Boolean
            Return True
        End Function

        Public Shared Function IsFreeBSDVersionAtLeast(ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0, ByVal Optional revision As Integer = 0) As Boolean
            Return True
        End Function

        Public Shared Function IsAndroid() As Boolean
            Return True
        End Function

        Public Shared Function IsAndroidVersionAtLeast(ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0, ByVal Optional revision As Integer = 0) As Boolean
            Return True
        End Function

        Public Shared Function IsIOS() As Boolean
            Return True
        End Function

        Public Shared Function IsIOSVersionAtLeast(ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0) As Boolean
            Return True
        End Function

        Public Shared Function IsMacOS() As Boolean
            Return True
        End Function

        Public Shared Function IsMacOSVersionAtLeast(ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0) As Boolean
            Return True
        End Function

        Public Shared Function IsTvOS() As Boolean
            Return True
        End Function

        Public Shared Function IsTvOSVersionAtLeast(ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0) As Boolean
            Return True
        End Function

        Public Shared Function IsWatchOS() As Boolean
            Return True
        End Function

        Public Shared Function IsWatchOSVersionAtLeast(ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0) As Boolean
            Return True
        End Function

        Public Shared Function IsWindows() As Boolean
            Return True
        End Function

        Public Shared Function IsWindowsVersionAtLeast(ByVal major As Integer, ByVal Optional minor As Integer = 0, ByVal Optional build As Integer = 0, ByVal Optional revision As Integer = 0) As Boolean
            Return True
        End Function
    End Class
End Namespace
";
    }
}