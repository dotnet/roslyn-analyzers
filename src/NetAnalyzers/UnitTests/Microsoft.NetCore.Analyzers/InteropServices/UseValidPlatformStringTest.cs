// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.UseValidPlatformString,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.UseValidPlatformString,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public class UseValidPlatformStringTest
    {
        private const string s_msBuildPlatforms = "build_property._SupportedPlatformList=CustomPlatform";

        [Fact]
        public async Task ValidPlatformStringUsedNotWarn()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""Browser"")]
    public void SupportedBrowser() { }

    [SupportedOSPlatform(""iOS"")]
    public string SupportedIos => string.Empty;

    [UnsupportedOSPlatform(""linux"")]
    public void UnsupportedLinux() { }

    [SupportedOSPlatform(""Android"")]
    public event EventHandler AndroidOnlyEvent;

    [UnsupportedOSPlatform(""macos"")]
    public void UnsupportedMacOS() { }

    [SupportedOSPlatform(""TVOS"")]
    public Test() { }

    [SupportedOSPlatform(""freeBsd2.0.1.2"")]
    public void SupportedFreeBsd() { }

    [SupportedOSPlatform(""watchOS3.2.1"")]
    public void SupporteWatchOS() { }

    [UnsupportedOSPlatform(""windows7.0"")]
    public static void SupportedWindows() { }
}";
            await VerifyAnalyzerAsyncCs(csSource);
        }

        [Fact]
        public async Task ValidPlatformStringUsedNotWarnVB()
        {
            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    <SupportedOSPlatform(""Browser"")>
    Public Sub SupportedBrowser()
    End Sub

    <SupportedOSPlatform(""iOS"")>
    Public Sub SupportedIos()
    End Sub

    <UnsupportedOSPlatform(""linux"")>
    Public Sub UnsupportedLinux()
    End Sub

    <SupportedOSPlatform(""Android"")>
    Public Sub SupportedAndroid()
    End Sub

    <UnsupportedOSPlatform(""macos"")>
    Public Sub UnsupportedMacOS()
    End Sub

    <SupportedOSPlatform(""tvos"")>
    Public Sub SupportedTvOS()
    End Sub

    <SupportedOSPlatform(""freeBsd2.0.1.2"")>
    Public Sub SupportedFreeBsd()
    End Sub

    <SupportedOSPlatform(""watchOS3.2.1"")>
    Public Sub SupporteWatchOS()
    End Sub

    <UnsupportedOSPlatform(""WINDOWS7.0"")>
    Public Shared Sub SupportedWindows()
    End Sub
End Class";
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task InvalidPlatformNameOrVersionWarns()
        {
            var csSource = @"
using System.Runtime.Versioning;

public class Test
{
    [[|UnsupportedOSPlatform(""window7.0"")|]] // The platform 'window' is not a known platform name
    public void InvlaidPlatform() { }

    [[|SupportedOSPlatform(""watch7.0"")|]] // The platform 'watch' is not a known platform name
    public void InvlaidPlatform2() { }

    [[|UnsupportedOSPlatform(""windows7"")|]] // The '7' is not a valid version. Please provide an input having two to four numbers separated with a dot.
    public static void InvalidVersion() { }

    [[|SupportedOSPlatform(""watchOs7.1.0.2.3"")|]] // The '7.1.0.2.3' is not a valid version. Please provide an input having two to four numbers separated with a dot.
    public static void InvalidVersion2() { }

    [[|UnsupportedOSPlatform(""browser1.0."")|]] // The '1.0.' is not a valid version. Please provide an input having two to four numbers separated with a dot.
    public static void InvalidVersion3() { }
}";
            await VerifyAnalyzerAsyncCs(csSource);
        }

        [Fact]
        public async Task InvalidPlatformNameOrVersionWarnsVB()
        {
            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    <[|UnsupportedOSPlatform(""window7.0"")|]>
    Public Sub InvlaidPlatform()
    End Sub

    <[|SupportedOSPlatform(""watch7.0"")|]>
    Public Sub InvlaidPlatform2()
    End Sub

    <[|UnsupportedOSPlatform(""windows7"")|]>
    Public Shared Sub InvalidVersion()
    End Sub

    <[|SupportedOSPlatform(""watchOs7.1.0.2.3"")|]>
    Public Shared Sub InvalidVersion2()
    End Sub

    <[|UnsupportedOSPlatform(""browser1.0."")|]>
    Public Shared Sub InvalidVersion3()
    End Sub
End Class";
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task InvalidPlatformNameOrVersionDifferentSymbolsWarns()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;

[assembly:[|UnsupportedOSPlatform(""blazor"")|]]
namespace ns
{
    [[|UnsupportedOSPlatform(""razor"")|]]
    public class Test
    {
        [[|UnsupportedOSPlatform(""window7.0"")|]] // The platform 'window' is not a known platform name
        public string InvlaidPlatform => string.Empty;

        [[|SupportedOSPlatform(""watch7.0"")|]] // The platform 'watch' is not a known platform name
        public Test() { }

        [[|UnsupportedOSPlatform(""windows7"")|]] // The '7' is not a valid version. Please provide an input having two to four numbers separated with a dot.
        public event EventHandler Windows7Event;

        [[|SupportedOSPlatform(""watchOs7.1.0.2.3"")|]] // The '7.1.0.2.3' is not a valid version. Please provide an input having two to four numbers separated with a dot.
        private static int s_field = 0;
    }
}";
            await VerifyAnalyzerAsyncCs(csSource);
        }

        [Fact]
        public async Task NotWarnForCustomPlatformAddedToMsBuild()
        {
            var csSource = @"
using System.Runtime.Versioning;

public class Test
{
    [SupportedOSPlatform(""customPlatform"")]
    public void SupportedCustomPlatform() { }

    [SupportedOSPlatform(""customplatform1.0"")]
    public void SupportedCustomPlatform1() { }

    [UnsupportedOSPlatform(""customPlatform2.0"")]
    public void UnsupportedCustomPlatform2() { }

    [UnsupportedOSPlatform(""customplatform"")]
    public void UnsupportedCustomPlatform() { }

    [[|UnsupportedOSPlatform(""custom1.0"")|]] // The platform 'custom' is not a known platform name
    public void InvlaidPlatform() { }
}";
            await VerifyAnalyzerAsyncCs(csSource, s_msBuildPlatforms);
        }

        [Fact]
        public async Task NotWarnForCustomPlatformAddedToMsBuildVB()
        {
            var vbSource = @"
Imports System.Runtime.Versioning

Public Class Test
    <SupportedOSPlatform(""customPlatform"")>
    Public Sub SupportedCustomPlatform()
    End Sub

    <SupportedOSPlatform(""customplatform1.0"")>
    Public Sub SupportedCustomPlatform1()
    End Sub

    <UnsupportedOSPlatform(""customPlatform2.0"")>
    Public Sub UnsupportedCustomPlatform2()
    End Sub

    <UnsupportedOSPlatform(""customplatform"")>
    Public Sub UnsupportedCustomPlatform()
    End Sub

    <[|UnsupportedOSPlatform(""custom1.0"")|]>
    Public Sub InvlaidPlatform()
    End Sub
End Class";
            await VerifyAnalyzerAsyncVb(vbSource, s_msBuildPlatforms);
        }

        [Fact]
        public async Task IsOsPlatformWithInvalidPlatformWarns()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;
using System.Diagnostics;

public class Test
{
    public void M1()
    {
        if (OperatingSystem.IsOSPlatform([|""x""|])) // The platform 'x' is not a known platform name
            PlatformSpecificAPI();

        if (OperatingSystem.IsOSPlatformVersionAtLeast([|""x""|], 1)) // The platform 'x' is not a known platform name
            PlatformSpecificAPI();

        if (!OperatingSystem.IsOSPlatformVersionAtLeast([|""window""|], 8, 100)) // The platform 'window' is not a known platform name
            PlatformSpecificAPI();

        if (!OperatingSystem.IsOSPlatformVersionAtLeast([|""xPlatform""|], 1, 2, 3, 4)) // The platform 'xPlatform' is not a known platform name
            PlatformSpecificAPI();

        if (OperatingSystem.IsOSPlatform(""windows"")) // No warn
            PlatformSpecificAPI();

        if (OperatingSystem.IsOSPlatformVersionAtLeast(""ios"", 12, 100, 0, 1)) // No warn
            PlatformSpecificAPI();

        if (OperatingSystem.IsOSPlatform([|""windows8.0""|])) // Version part not allowed: The platform 'windows8.0' is not a known platform name
            PlatformSpecificAPI();

        Debug.Assert(OperatingSystem.IsOSPlatformVersionAtLeast([|""windows8.0""|], 1, 2, 3, 4));
        Debug.Assert(OperatingSystem.IsOSPlatform([|""invalid""|]));
    }

    public void PlatformSpecificAPI() { }
}";
            await VerifyAnalyzerAsyncCs(csSource);
        }

        [Fact]
        public async Task IsOsPlatformWithInvalidPlatformWarnsVB()
        {
            var vbSource = @"
Imports System
Imports System.Runtime.Versioning

Public Class Test
    Public Sub M1()
        If OperatingSystem.IsOSPlatform([|""x""|]) Then PlatformSpecificAPI()
        If OperatingSystem.IsOSPlatformVersionAtLeast([|""x""|], 1) Then PlatformSpecificAPI()
        If Not OperatingSystem.IsOSPlatformVersionAtLeast([|""window""|], 8, 100) Then PlatformSpecificAPI()
        If OperatingSystem.IsOSPlatform(""windows"") Then PlatformSpecificAPI()
        If OperatingSystem.IsOSPlatformVersionAtLeast(""ios"", 12, 100, 0, 1) Then PlatformSpecificAPI()
        If OperatingSystem.IsOSPlatform([|""windows8.0""|]) Then PlatformSpecificAPI()
    End Sub

    Public Sub PlatformSpecificAPI()
    End Sub
End Class";
            await VerifyAnalyzerAsyncVb(vbSource);
        }

        [Fact]
        public async Task MoreInvalidPlatformStringsWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [[|SupportedOSPlatform(""Ios-4.1"")|]]
    public void UnsupportedOSPlatformIosDash4_1() { }

    [[|SupportedOSPlatform(""Ios*4.1"")|]]
    public void UnsupportedOSPlatformIosStar4_1() { }

    [[|SupportedOSPlatform(null)|]]
    public void UnsupportedOSPlatformWithNullString() { }

    [[|SupportedOSPlatform(""Linux_4.1"")|]]
    public void SupportedLinux_41() { }

    [[|UnsupportedOSPlatform("""")|]]
    public void UnsupportedWithEmptyString() { }
}";
            await VerifyAnalyzerAsyncCs(source);
        }

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode)
        {
            var test = PopulateTestCs(sourceCode);
            await test.RunAsync();
        }

        private static async Task VerifyAnalyzerAsyncCs(string sourceCode, string editorconfigText)
        {
            var test = PopulateTestCs(sourceCode);
            test.TestState.AdditionalFiles.Add((".editorconfig", editorconfigText));
            await test.RunAsync();
        }

        private static VerifyCS.Test PopulateTestCs(string sourceCode)
        {
            return new VerifyCS.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
            };
        }

        private static async Task VerifyAnalyzerAsyncVb(string sourceCode)
        {
            var task = PopulateTestVb(sourceCode);
            await task.RunAsync();
        }

        private static async Task VerifyAnalyzerAsyncVb(string sourceCode, string editorconfigText)
        {
            var test = PopulateTestVb(sourceCode);
            test.TestState.AdditionalFiles.Add((".editorconfig", editorconfigText));
            await test.RunAsync();
        }

        private static VerifyVB.Test PopulateTestVb(string sourceCode)
        {
            return new VerifyVB.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
            };
        }
    }
}