// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
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
        [Fact]
        public async Task ObsoletedMethodsCalledWarns()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;
using Mock;

public class Test
{
    [SupportedOSPlatform(""Linux"")]
    public void M1()
    {
        ObsoletedOnWindows10(); // Shold not warn as only accessible on Linux
        {|#0:ObsoletedOnLinux4()|}; // This call site is reachable on: 'Linux'. 'Test.ObsoletedOnLinux()' is obsoleted on: 'Linux' 4.1 and later.
        {|CA1422:ObsoletedOnLinux4AndWindows10()|}; // This call site is reachable on: 'Linux'. 'Test.ObsoletedOnLinux4AndWindows10()' is obsoleted on: 'Linux' 4.1 and later.
    }
    
    [ObsoletedInOSPlatform(""Linux4.1"")]
    public void ObsoletedOnLinux4() { }

    [ObsoletedInOSPlatform(""Windows10.1.1.1"")]
    public void ObsoletedOnWindows10() { }

    [ObsoletedInOSPlatform(""Linux4.1"", ""Use Linux4Supported"")]
    [ObsoletedInOSPlatform(""Windows10.1.1.1"",""Use Windows10Supported"")]
    public void ObsoletedOnLinux4AndWindows10() { }
}" + MockObsoletedAttributeCS;
            await VerifyAnalyzerCSAsync(csSource, VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.ObsoletedCsReachable).WithLocation(0)
                .WithArguments("Test.ObsoletedOnLinux4()", "'Linux' 4.1 and later", "'Linux'"));


            var vbSource = @"
Imports System
Imports System.Runtime.Versioning
Imports Mock
Public Class Test
    <SupportedOSPlatform(""Linux"")>
    Public Sub M1()
        ObsoletedOnWindows10()
        {|#0:ObsoletedOnLinux()|} ' This call site is reachable on: 'Linux'. 'Public Sub ObsoletedOnLinux()' is obsoleted on: 'Linux' 4.1 and later.
    End Sub

    <ObsoletedInOSPlatform(""Windows10.1.1.1"")>
    Public Sub ObsoletedOnWindows10()
    End Sub
    
    <ObsoletedInOSPlatform(""Linux4.1"")>
    Public Sub ObsoletedOnLinux()
    End Sub
End Class
" + MockObsoletedAttributeVB;
            await VerifyAnalyzerVBAsync(vbSource, VerifyVB.Diagnostic(PlatformCompatibilityAnalyzer.ObsoletedCsReachable).WithLocation(0)
                .WithArguments("Public Sub ObsoletedOnLinux()", "'Linux' 4.1 and later", "'Linux'"));
        }

        [Fact]
        public async Task ObsoletedWithMessageUrlCalledWarns()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;
using Mock;

public class Test
{
    public void CrossPlatform()
    {
        {|#0:ObsoletedWithMessageAndUrl()|}; // This call site is reachable on all platforms. 'Test.ObsoletedWithMessageAndUrl()' is obsoleted on: 'Windows' 10.1.1.1 and later, Use other method instead, http://www/look.for.more.info.
        {|#1:ObsoletedWithMessage()|}; // This call site is reachable on all platforms. 'Test.ObsoletedWithMessage()' is obsoleted on: 'Windows' 10.1.1.1 and later, Use other method instead.
        ObsoletedOnAndroid(); // Cross platform and android not int he MSBuild list so not warn
    }
    
    [ObsoletedInOSPlatform(""android21.0"")]
    public void ObsoletedOnAndroid() { }

    [ObsoletedInOSPlatform(""Windows10.1.1.1"", ""Use other method instead"", Url = ""http://www/look.for.more.info"")]
    public void ObsoletedWithMessageAndUrl() { }
    [ObsoletedInOSPlatform(""Windows10.1.1.1"", ""Use other method instead"")]
    public void ObsoletedWithMessage() { }
}" + MockObsoletedAttributeCS;
            await VerifyAnalyzerCSAsync(csSource, s_msBuildPlatforms, VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.ObsoletedCsAllPlatforms).WithLocation(0).
                    WithArguments("Test.ObsoletedWithMessageAndUrl()", "'Windows' 10.1.1.1 and later, Use other method instead http://www/look.for.more.info"),
                  VerifyCS.Diagnostic(PlatformCompatibilityAnalyzer.ObsoletedCsAllPlatforms).WithLocation(1).
                    WithArguments("Test.ObsoletedWithMessage()", "'Windows' 10.1.1.1 and later, Use other method instead"));
        }

        [Fact]
        public async Task ObsoletedWithinDifferentCallsite()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;
using Mock;

public class Test
{
    [SupportedOSPlatform(""IOS"")]
    public void CallsiteReachableOnIOS()
    {
        ObsoletedOnWindows(); // Unreachable on windows, not warn
        ObsoletedWithMessage();
        {|CA1422:ObsoletedOnIOS14()|}; // This call site is reachable on: 'IOS', 'maccatalyst'. 'Test.ObsoletedOnIOS14()' is obsoleted on: 'ios' 14.0 and later, 'maccatalyst' 14.0 and later.
    }

    [Mock.UnsupportedOSPlatform(""ios13.0"")]
    [System.Runtime.Versioning.UnsupportedOSPlatform(""Windows10.1.0"")]
    public void SuppressedByCallsiteUnsupported()
    {
        ObsoletedOnWindows(); // Not supported on windows with matching version, not warn
        ObsoletedWithMessage(); // Same as above
        ObsoletedOnIOS14();
    }

    [Mock.UnsupportedOSPlatform(""ios15.0"")]
    [System.Runtime.Versioning.UnsupportedOSPlatform(""Windows11.1.0"")]
    public void NotSuppressedByCallsiteUnsupported() // obsoleted before unsupported version
    {
        {|CA1422:ObsoletedOnWindows()|}; // This call site is reachable on: 'Windows' 11.1.0 and before. 'Test.ObsoletedOnWindows()' is obsoleted on: 'Windows' 10.1.1.1 and later.
        {|CA1422:ObsoletedWithMessage()|}; // This call site is reachable on: 'Windows' 11.1.0 and before. 'Test.ObsoletedWithMessage()' is obsoleted on: 'Windows' 10.1.1.1 and later, Use other method instead.
        {|CA1422:ObsoletedOnIOS14()|}; // This call site is reachable on: 'ios' 15.0 and before, 'maccatalyst' 15.0 and before. 'Test.ObsoletedOnIOS14()' is obsoleted on: 'ios' 14.0 and later, 'maccatalyst' 14.0 and later.
    }

    [ObsoletedInOSPlatform(""ios13.0"")]
    [ObsoletedInOSPlatform(""Windows10.1.0"")]
    public void SuppressedByCallsiteObsoleted()
    {
        ObsoletedOnWindows(); // All calls Obsoleted within version range, not warn
        ObsoletedWithMessage();
        ObsoletedOnIOS14();
    }

    [ObsoletedInOSPlatform(""ios16.0"")]
    [ObsoletedInOSPlatform(""Windows11.1.0"")]
    public void NotSuppressedByCallsiteObsoleted()
    {
        {|CA1422:ObsoletedOnWindows()|}; //This call site is reachable on all platforms. 'Test.ObsoletedOnWindows()' is obsoleted on: 'Windows' 10.1.1.1 and later.
        {|CA1422:ObsoletedWithMessage()|}; // This call site is reachable on all platforms. 'Test.ObsoletedWithMessage()' is obsoleted on: 'Windows' 10.1.1.1 and later, Use other method instead.
        {|CA1422:ObsoletedOnIOS14()|}; // This call site is reachable on all platforms. 'Test.ObsoletedOnIOS14()' is obsoleted on: 'ios' 14.0 and later, 'maccatalyst' 14.0 and later.
    }
    
    [ObsoletedInOSPlatform(""ios14.0"")]
    public void ObsoletedOnIOS14() { }

    [ObsoletedInOSPlatform(""Windows10.1.1.1"")]
    public void ObsoletedOnWindows() { }
    [ObsoletedInOSPlatform(""Windows10.1.1.1"", ""Use other method instead"")]
    public void ObsoletedWithMessage() { }
}" + MockObsoletedAttributeCS;
            await VerifyAnalyzerCSAsync(csSource, s_msBuildPlatforms);
        }

        [Fact]
        public async Task UnsuportedWithMessageCalledWarnsAsync()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;
using Mock;

public class Test
{
    public void CrossPlatform()
    {
        [|UnsupportedIosBrowserWatchOS()|]; // This call site is reachable on all platforms. 'Test.UnsupportedIosBrowserWatchOS()' is unsupported on: 'Browser'. Use BrowserSupported() method instead,
                                                           // 'ios' 13.0 and later. Use Test.IOsSupported() method instead, 'maccatalyst' 13.0 and later. Use Test.IOsSupported() method instead.
        UnsupportedAndroid(); //  Cross platform and android not int he MSBuild list so not warn
    }

    [SupportedOSPlatform(""Android"")]
    [SupportedOSPlatform(""browser"")]
    public void ReachableOnAndroidAndBrowser()
    {
        [|UnsupportedIosBrowserWatchOS()|]; // This call site is reachable on: 'Android', 'browser'. 'Test.UnsupportedIosBrowserWatchOS()' is unsupported on: 'Browser', Use BrowserSupported() method instead.
        [|UnsupportedAndroid()|]; // This call site is reachable on: 'Android' all versions, 'browser'. 'Test.UnsupportedAndroid()' is unsupported on: 'Android' 21.0 and later, Use other method instead.
    }

    [Mock.UnsupportedOSPlatform(""Android21.0"", ""Use other method instead"")]
    public void UnsupportedAndroid() { }

    [Mock.UnsupportedOSPlatform(""ios13.0"", ""Use Test.IOsSupported() method instead"")]
    [Mock.UnsupportedOSPlatform(""Browser"", ""Use BrowserSupported() method instead"")]
    [Mock.UnsupportedOSPlatform(""Watchos"", ""Use WitchSupported() method instead"")]
    public void UnsupportedIosBrowserWatchOS() { }
}" + MockObsoletedAttributeCS;
            await VerifyAnalyzerCSAsync(csSource, s_msBuildPlatforms);
        }

        [Fact]
        public async Task ObsoletedWarningsGuardedWithOperatingSystemAPIs()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;
using Mock;

public class Test
{
    public void CrossPlatform()
    {
        if (OperatingSystem.IsMacOS())
        {
            {|CA1422:ObsoletedOnMacOS()|}; // This call site is reachable on: 'macOS/OSX'. 'Test.ObsoletedOnMacOS()' is obsoleted on: 'macOS/OSX'.
            ObsoletedOnAndroid21(); // Call site only reachable on MacOS, no warning 
            ObsoletedOnLinux4AndWindows10(); // Same, no warning
        }
        else
        {
            ObsoletedOnMacOS();
            ObsoletedOnAndroid21(); // Android is not in MSBuild support list, no warning
            {|CA1422:ObsoletedOnLinux4AndWindows10()|}; // This call site is reachable on all platforms. 'Test.ObsoletedOnLinux4AndWindows10()' is obsoleted on: 'Linux' 4.1 and later, Use Linux4Supported, 'Windows' 10.1.1.1 and later, Use Windows10Supported.
        }

        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            {|CA1422:ObsoletedOnMacOS()|}; // This call site is reachable on: 'macOS/OSX' 11.0 and later. 'Test.ObsoletedOnMacOS()' is obsoleted on: 'macOS/OSX' all versions.
        }
        else
        {
            {|CA1422:ObsoletedOnMacOS()|}; // It could be macos with less version, so warns: This call site is reachable on all platforms. 'Test.ObsoletedOnMacOS()' is obsoleted on: 'macOS/OSX'.
        }
    }
    
    [ObsoletedInOSPlatform(""Android21.0"")]
    public void ObsoletedOnAndroid21() { }

    [ObsoletedInOSPlatform(""MacOS"")]
    public void ObsoletedOnMacOS() { }

    [ObsoletedInOSPlatform(""Linux4.1"", ""Use Linux4Supported"")]
    [ObsoletedInOSPlatform(""Windows10.1.1.1"",""Use Windows10Supported"")]
    public void ObsoletedOnLinux4AndWindows10() { }
}" + MockObsoletedAttributeCS;
            await VerifyAnalyzerCSAsync(csSource, s_msBuildPlatforms);
        }

        [Fact]
        public async Task ObsoletedWarningsGuardedWithOperatingSystemAPIsDifferentCallsite()
        {
            var csSource = @"
using System;
using System.Runtime.Versioning;
using Mock;

public class Test
{
    [SupportedOSPlatform(""Android"")]
    public void AndroidOnly()
    {
        if (OperatingSystem.IsMacOS())
        {
            ObsoletedOnMacOS(); // The method is not reachable on MacOS, so no warning
            ObsoletedOnAndroid21(); // Conditional only reachable on MacOS, no warning 
            ObsoletedOnLinux4AndWindows10();
        }
        else
        {
            ObsoletedOnMacOS(); // Method only for android, no warning
            {|CA1422:ObsoletedOnAndroid21()|}; // This call site is reachable on: 'Android'. 'Test.ObsoletedOnAndroid21()' is obsoleted on: 'Android' 21.0 and later.
            ObsoletedOnLinux4AndWindows10(); // Method only for android
        }
    }

    [SupportedOSPlatform(""Android"")]
    [SupportedOSPlatform(""MacOS"")]
    public void AndroidMacOSOnly()
    {
        if (OperatingSystem.IsMacOS())
        {
            {|CA1422:ObsoletedOnMacOS()|}; //  This call site is reachable on: 'Android', 'macOS/OSX'. 'Test.ObsoletedOnMacOS()' is obsoleted on: 'macOS/OSX'.
            ObsoletedOnAndroid21();
            ObsoletedOnLinux4AndWindows10();
        }
        else
        {
            ObsoletedOnMacOS();
            {|CA1422:ObsoletedOnAndroid21()|}; // This call site is reachable on: 'Android', 'macOS/OSX'. 'Test.ObsoletedOnAndroid21()' is obsoleted on: 'Android' 21.0 and later.
            ObsoletedOnLinux4AndWindows10();
        }
    }
    
    [ObsoletedInOSPlatform(""Android21.0"")]
    public void ObsoletedOnAndroid21() { }

    [ObsoletedInOSPlatform(""MacOS"")]
    public void ObsoletedOnMacOS() { }

    [ObsoletedInOSPlatform(""Linux4.1"", ""Use Linux4Supported"")]
    [ObsoletedInOSPlatform(""Windows10.1.1.1"",""Use Windows10Supported"")]
    public void ObsoletedOnLinux4AndWindows10() { }
}" + MockObsoletedAttributeCS;
            await VerifyAnalyzerCSAsync(csSource, s_msBuildPlatforms);
        }

        private readonly string MockObsoletedAttributeCS = @"
namespace Mock
{
    public abstract class OSPlatformAttribute : Attribute
    {
        private protected OSPlatformAttribute(string platformName)
        {
            PlatformName = platformName;
        }
        public string PlatformName { get; }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Enum |
                    AttributeTargets.Event |
                    AttributeTargets.Field |
                    AttributeTargets.Interface |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    public sealed class UnsupportedOSPlatformAttribute : OSPlatformAttribute
    {
        public UnsupportedOSPlatformAttribute(string platformName) : base(platformName)
        {
        }
        public UnsupportedOSPlatformAttribute(string platformName, string message) : base(platformName)
        {
            Message = message;
        }
        public string Message { get; }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Enum |
                    AttributeTargets.Event |
                    AttributeTargets.Field |
                    AttributeTargets.Interface |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    public sealed class ObsoletedInOSPlatformAttribute : OSPlatformAttribute
    {
        public ObsoletedInOSPlatformAttribute(string platformName) : base(platformName)
        {
        }
        public ObsoletedInOSPlatformAttribute(string platformName, string message) : base(platformName)
        {
            Message = message;
        }
        public string Message { get; }
        public string Url { get; set; }
    }
}";

        private readonly string MockObsoletedAttributeVB = @"
Namespace Mock
    <AttributeUsage(AttributeTargets.Assembly Or
                    AttributeTargets.[Class] Or AttributeTargets.Constructor Or
                    AttributeTargets.[Enum] Or AttributeTargets.[Event] Or
                    AttributeTargets.Field Or AttributeTargets.[Interface] Or
                    AttributeTargets.Method Or AttributeTargets.[Module] Or
                    AttributeTargets.[Property] Or
                    AttributeTargets.Struct,
                    AllowMultiple:=True, Inherited:=False)>
    Public NotInheritable Class ObsoletedInOSPlatformAttribute
        Inherits Attribute

        Public Sub New(ByVal platformName As String)
            PlatformName = platformName
        End Sub

        Public Sub New(ByVal platformName As String, ByVal message As String)
            PlatformName = platformName
            Message = message
        End Sub

        Public ReadOnly Property PlatformName As String
        Public ReadOnly Property Message As String
        Public Property Url As String
    End Class
End Namespace";
    }
}
