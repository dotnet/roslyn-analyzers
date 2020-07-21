// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PlatformCompatabilityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public partial class PlatformCompatabilityAnalyzerTests
    {
        [Fact]
        public async Task GuardedCalled_SimpleIf_NotWarns()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 1, 2, 3))
            M2();
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_MultipleSimpleIfTests()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 1, 2, 3))
            M2();
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 10, 1, 2, 3))
            [|M2()|];
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2))
            M2();
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 8, 1, 2, 3))
            [|M2()|];        
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedWith_IsOSPlatformOrLater_SimpleIfElseTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 11))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedWith_IsOSPlatformEarlierThan_SimpleIfElseTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 10))
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

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }

    [ObsoletedInOSPlatform(""Windows10.1.2.3"")]
    void M3 ()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedWith_StringOverload_SimpleIfElseTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(""Windows10.1""))
        {
            [|M2()|];
            M3();
        }
        else
        {
            [|M2()|];
            [|M3()|];
        }

        if(RuntimeInformationHelper.IsOSPlatformOrLater(""Windows10.1.3""))
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

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }

    [ObsoletedInOSPlatform(""Windows10.1.2.3"")]
    void M3 ()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task OsDependentEnumValue_GuardedWith_SimpleIfElseTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test2
{
    public void M1()
    {
        PlatformEnum val = [|PlatformEnum.Windows10|];
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10))
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
    [MinimumOSPlatform(""Windows10.0"")]
    Windows10,
    [MinimumOSPlatform(""Linux4.8"")]
    Linux48,
    NoPlatform
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfElseTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        [|M2()|];

        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 11))
        {
            M2();
        }
        else if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Linux, 11))
        {
            [|M2()|];
        }
        else if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12))
        {
            [|M2()|];
        }
        else if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 12))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseTestWithNegation()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(!RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 1, 2, 3))
            [|M2()|];
        else
            M2();
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfElseTestWithNegation()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(!RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 1, 2, 3))
            [|M2()|];
        else if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Linux, 1, 1))
            M2();
        else
            M2();
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfTestWithNegationAndReturn()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];
        if(!RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 1, 2, 3))
            return;
        M2();
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfTestWithLogicalAnd()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2) &&
           RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12))
        {
            M2();
        }

        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12) &&
           RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 12))
        {
            M2();
        }

        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12) &&
           RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 12))
        {
            [|M2()|];
        }

        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2) && 1 == 1)
        {
            M2();
        }

        [|M2()|];
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseTestWithLogicalAnd()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];

        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2) &&
           RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }

        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12) &&
           RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 12))
        {
            [|M2()|];
        }
        else
        {
            [|M2()|];
        }
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfTestWithLogicalOr()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2) ||
           RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12))
        {
            [|M2()|];
        }

        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12) || 
            RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2))
        {
            [|M2()|];
        }

        if(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 12) || 
            RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2))
        {
            [|M2()|];
        }

        [|M2()|];
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseTestWithLogicalOr()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];

        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2) ||
           RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12))
        {
            [|M2()|];
        }
        else
        {
            [|M2()|];
        }

        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12) || 
            RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2))
        {
            [|M2()|];
        }
        else
        {
            [|M2()|];
        }

        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2) ||
           RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 11))
        {
            [|M2()|]; // Even it is not meaningful check i think it is a bug, it shouldn't warn
        }
        else
        {
            [|M2()|];
        }
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfElseTestWithLogicalOr()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

public class Test
{
    public void M1()
    {
        [|M2()|];

        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2) ||
           RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 5, 1))
        {
            [|M2()|];
        }
        else if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 9))
        {
            [|M2()|];
        }
        else
            [|M2()|];

        if(RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Windows, 12) || 
            RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2))
        {
            [|M2()|];
        }
        else if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 11))
        {
            M2();
        }
        else
        {
            [|M2()|];
        }
    }
    [MinimumOSPlatform(""Windows10.1.2.3"")]
    public void M2()
    {
    }
}
" + MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseIfTestWithLogicalOrAnd()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        [|M2()|];

        if((RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 1) ||
            RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 1)) &&
            (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 12) ||
            RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 2)))
        {
            [|M2()|]; 
        }
        else if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 13) ||
                 RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 3) ||
                 RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 4))
        {
            [|M2()|];        
        }
        else
        {
            [|M2()|];
        }
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}"
+ MockAttributesSource + MockRuntimeApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedWith_ControlFlowAndMultipleChecks()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 8))
        {
            [|M2()|];

            if (RuntimeInformationHelper.IsOSPlatformEarlierThan(OSPlatform.Linux, 2, 0))
            {
                [|M2()|];
            }
            else if (!RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2, 1))
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

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedWith_DebugAssertAnalysisTest()
        {
            var source = @"
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        [|M2()|];

        Debug.Assert(RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 10, 2));

        M2();
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedWith_ResultSavedInLocal()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        var x1 = RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 11);
        var x2 = RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Linux, 1);

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

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedWith_VersionSavedInLocal()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        var v11 = 11;
        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, v11))
        {
            M2();
        }
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task PlatformSavedInLocal_NotYetSupported() // TODO do we want to support it?
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        var platform = OSPlatform.Windows;
        if (RuntimeInformationHelper.IsOSPlatformOrLater(platform, 11))
        {
            [|M2()|];
        }
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UnrelatedConditionCheckDoesNotInvalidateState()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1(bool flag1, bool flag2)
    {
        [|M2()|];

        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 11))
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

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        /*[Fact] //TODO: Not working anymore, fix this
        public async Task InterproceduralAnalysisTest()
        {
            var source = @"
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

class Test
{
    void M1()
    {
        {|#0:M2()|};

        if (IsWindows11OrLater())
        {
            M2();    
        }

        {|#1:M2()|}; 
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }

    bool IsWindows11OrLater()
    {
        return RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows,10,2,3,4);
    }
}" + MockAttributesSource + MockRuntimeApiSource;

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (".editorconfig", "dotnet_code_quality.interprocedural_analysis_kind = ContextSensitive") }
                }
            };

            test.ExpectedDiagnostics.AddRange(new[]
            {
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.MinimumOsRule).WithLocation(0).WithArguments("M2", "Windows", "10.1.2.3"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.MinimumOsRule).WithLocation(1).WithArguments("M2", "Windows", "10.1.2.3"),
            });

            await test.RunAsync();
        }*/

        private readonly string MockRuntimeApiSource = @"
namespace System.Runtime.InteropServices
{
    public static class RuntimeInformationHelper
    {
#pragma warning disable CA1801, IDE0060 // Review unused parameters
        public static bool IsOSPlatformOrLater(OSPlatform osPlatform, int major)
        {
            return true;
        }
        public static bool IsOSPlatformOrLater(OSPlatform osPlatform, int major, int minor)
        {
            return true;
        }
        public static bool IsOSPlatformOrLater(OSPlatform osPlatform, int major, int minor, int build)
        {
            return true;
        }
        public static bool IsOSPlatformOrLater(OSPlatform osPlatform, int major, int minor, int build, int revision)
        {
            return true;
        }
        public static bool IsOSPlatformEarlierThan(OSPlatform osPlatform, int major)
        {
            return false;
        }
        public static bool IsOSPlatformEarlierThan(OSPlatform osPlatform, int major, int minor)
        {
            return false;
        }
        public static bool IsOSPlatformEarlierThan(OSPlatform osPlatform, int major, int minor, int build)
        {
            return false;
        }
        public static bool IsOSPlatformEarlierThan(OSPlatform osPlatform, int major, int minor, int build, int revision)
        {
            return false;
        }
        public static bool IsOSPlatformOrLater(string platformName) => true;
        public static bool IsOSPlatformEarlierThan(string platformName) => true;
#pragma warning restore CA1801, IDE0060 // Review unused parameters
    }
}";
    }
}
