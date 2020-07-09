// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PlatformCompatabilityAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public class PlatformCompatabilityAnalyzerTests
    {
        [Fact]
        public async Task OsDependentMethodCalledWithoutSuppressionWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public void M1()
    {
        [|M2()|];
    }
    [MinimumOSPlatform(""Windows10.1.1.1"")]
    public void M2()
    {
    }
}
" + MockPlatformApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task OsDependentMethodCalledFromIntanceWithoutSuppressionWarns()
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
    [MinimumOSPlatform(""Windows10.1.1.1"")]
    public void M2()
    {
    }
}
" + MockPlatformApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task OsDependentMethodCalledFromOtherNsIntanceWarns()
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
        [MinimumOSPlatform(""Windows10.1.1.1"")]
        public void M2()
        {
        }
    }
}
" + MockPlatformApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task MethodOfOsDependentClassCalledWithoutSuppressionWarns()
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    public void M1()
    {
        OsDependentClass odc = new OsDependentClass();
        [|odc.M2()|];
    }
}
[MinimumOSPlatform(""Windows10.1.2.3"")]
public class OsDependentClass
{
    public void M2()
    {
    }
}
" + MockPlatformApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        /*        [Fact] TODO find out how to pass 2 sources
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
            [assembly:MinimumOSPlatform(""Windows10.1.2.3"")]
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

        [Theory]
        [InlineData("10.1.2.3", "10.1.2.3", false)]
        [InlineData("10.1.2.3", "10.1.3.3", false)]
        [InlineData("10.1.2.3", "10.1.3.1", false)]
        [InlineData("10.1.2.3", "11.1.2.3", false)]
        [InlineData("10.1.2.3", "10.2.2.0", false)]
        [InlineData("10.1.2.3", "10.1.1.3", true)]
        [InlineData("10.1.2.3", "10.1.1.4", true)]
        [InlineData("10.1.2.3", "10.0.1.9", true)]
        [InlineData("10.1.2.3", "8.2.3.3", true)]
        public async Task MethodOfOsDependentClassSuppressedWithAddedAttribute(string dependentVersion, string suppressingVersion, bool warn)
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [MinimumOSPlatform(""Windows" + suppressingVersion + @""")]
    public void M1()
    {
        OsDependentClass odc = new OsDependentClass();
        odc.M2();
    }
}

[MinimumOSPlatform(""Windows" + dependentVersion + @""")]
public class OsDependentClass
{
    public void M2()
    {
    }
}
" + MockPlatformApiSource;

            if (warn)
                await VerifyCS.VerifyAnalyzerAsync(source, VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.AddedRule).WithSpan(10, 9, 10, 17).WithArguments("M2", "Windows", "10.1.2.3"));
            else
                await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("10.1.2.3", "10.1.2.3", false)]
        [InlineData("10.1.2.3", "10.1.3.3", true)]
        [InlineData("10.1.2.3", "10.1.3.1", true)]
        [InlineData("10.1.2.3", "11.1.2.3", true)]
        [InlineData("10.1.2.3", "10.2.2.0", true)]
        [InlineData("10.1.2.3", "10.1.1.3", false)]
        [InlineData("10.1.2.3", "10.1.1.4", false)]
        [InlineData("10.1.2.3", "10.1.0.1", false)]
        [InlineData("10.1.2.3", "8.2.3.4", false)]
        public async Task MethodOfOsDependentClassSuppressedWithObsoleteAttribute(string dependentVersion, string suppressingVersion, bool warn)
        {
            var source = @"
using System.Runtime.Versioning;

public class Test
{
    [ObsoletedInOSPlatform(""Windows" + suppressingVersion + @""")]
    public void M1()
    {
        OsDependentClass odc = new OsDependentClass();
        odc.M2();
    }
 }
 
[ObsoletedInOSPlatform(""Windows" + dependentVersion + @""")]
public class OsDependentClass
{
     public void M2()
    {
    }
}
" + MockPlatformApiSource;

            if (warn)
                await VerifyCS.VerifyAnalyzerAsync(source, VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.ObsoleteRule).WithSpan(10, 9, 10, 17).WithArguments("M2", "Windows", "10.1.2.3"));
            else
                await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("10.1.2.3", "10.1.2.3", false)]
        [InlineData("10.1.2.3", "10.1.3.3", true)]
        [InlineData("10.1.2.3", "10.1.3.1", true)]
        [InlineData("10.1.2.3", "11.1.2.3", true)]
        [InlineData("10.1.2.3", "10.2.2.0", true)]
        [InlineData("10.1.2.3", "10.1.1.3", false)]
        [InlineData("10.1.2.3", "10.1.1.4", false)]
        [InlineData("10.1.2.3", "10.1.0.1", false)]
        [InlineData("10.1.2.3", "8.2.3.4", false)]
        public async Task MethodOfOsDependentClassSuppressedWithRemovedAttribute(string dependentVersion, string suppressingVersion, bool warn)
        {
            var source = @"
 using System.Runtime.Versioning;
 
public class Test
{
    [RemovedInOSPlatform(""Windows" + suppressingVersion + @""")]
    public void M1()
    {
        OsDependentClass odc = new OsDependentClass();
        odc.M2();
    }
}

[RemovedInOSPlatform(""Windows" + dependentVersion + @""")]
public class OsDependentClass
{
    public void M2()
    {
    }
}
" + MockPlatformApiSource;

            if (warn)
                await VerifyCS.VerifyAnalyzerAsync(source, VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.RemovedRule).WithSpan(10, 9, 10, 17).WithArguments("M2", "Windows", "10.1.2.3"));
            else
                await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("", true)]
        [InlineData("build_property.TargetFramework=net5.0", true)]
        [InlineData("build_property.TargetFramework=net472", true)]
        [InlineData("build_property.TargetFramework=net5.0-linux", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.1.1.1", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.2", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows11", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows10", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows11\nbuild_property.TargetPlatformMinVersion=10;", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.2\nbuild_property.TargetPlatformMinVersion=10.1;", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.1.1.2\nbuild_property.TargetPlatformMinVersion=10.0.0.1;", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.2.1\nbuild_property.TargetPlatformMinVersion=9.1.1.1;", true)]
        public async Task TfmAndTargetPlatformMinVersionWithAddedAttribute(string editorConfigText, bool expectDiagnostic)
        {
            var invocation = expectDiagnostic ? @"[|M2()|]" : "M2()";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
using System.Runtime.Versioning;

public class Test
{{
    public void M1()
    {{
        {invocation};
    }}
    [MinimumOSPlatform(""Windows10.1.1.1"")]
    public void M2()
    {{
    }}
}}
" + MockPlatformApiSource
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }

        [Theory]
        [InlineData("", true)]
        [InlineData("build_property.TargetFramework=net5.0", true)]
        [InlineData("build_property.TargetFramework=net472", false)] // TODO because no version in TFM, version is set to 0.0.0.0
        [InlineData("build_property.TargetFramework=net5.0-linux", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows", false)] // Same here
        [InlineData("build_property.TargetFramework=net5.0-windows10.1.1.1", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.2", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows11", true)]
        [InlineData("build_property.TargetFramework=net5.0-windows10", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows11\nbuild_property.TargetPlatformMinVersion=10;", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.2\nbuild_property.TargetPlatformMinVersion=10.1;", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.1.1.2\nbuild_property.TargetPlatformMinVersion=10.0.0.1;", false)]
        [InlineData("build_property.TargetFramework=net5.0-windows10.2.1\nbuild_property.TargetPlatformMinVersion=9.1.1.1;", false)]
        public async Task TfmAndTargetPlatformMinVersionWithObsoleteAttribute(string editorConfigText, bool expectDiagnostic)
        {
            var invocation = expectDiagnostic ? @"[|M2()|]" : "M2()";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
$@"
using System.Runtime.Versioning;

public class Test
{{
    public void M1()
    {{
        {invocation};
    }}
    [ObsoletedInOSPlatform(""Windows10.1.1.1"")]
    public void M2()
    {{
    }}
}}
" + MockPlatformApiSource
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }

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
" + MockPlatformApiSource;
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
" + MockPlatformApiSource;
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task GuardedCall_SimpleIfElseTest()
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
        else
        {
            [|M2()|];
        }
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockPlatformApiSource;

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

        [|M2()|];
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockPlatformApiSource;

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
" + MockPlatformApiSource;
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
" + MockPlatformApiSource;
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
" + MockPlatformApiSource;
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
        [|M2()|];

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
" + MockPlatformApiSource;
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
" + MockPlatformApiSource;
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
        [|M2()|];

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
" + MockPlatformApiSource;
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
" + MockPlatformApiSource;
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
" + MockPlatformApiSource;
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
+ MockPlatformApiSource;
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
        [|M2()|];

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
}" + MockPlatformApiSource;

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
}" + MockPlatformApiSource;

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
        [|M2()|];

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
}" + MockPlatformApiSource;

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
        [|M2()|];

        var v11 = 11;
        if (RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, v11))
        {
            M2();
        }

        [|M2()|];
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockPlatformApiSource;

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
        [|M2()|];

        var platform = OSPlatform.Windows;
        if (RuntimeInformationHelper.IsOSPlatformOrLater(platform, 11))
        {
            [|M2()|];
        }

        [|M2()|];
    }

    [MinimumOSPlatform(""Windows10.1.2.3"")]
    void M2()
    {
    }
}" + MockPlatformApiSource;

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
}" + MockPlatformApiSource;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("dotnet_code_quality.interprocedural_analysis_kind = ContextSensitive")]
        public async Task InterproceduralAnalysisTest(string editorconfig)
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
        return RuntimeInformationHelper.IsOSPlatformOrLater(OSPlatform.Windows, 11);
    }
}" + MockPlatformApiSource;

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (".editorconfig", editorconfig) }
                }
            };

            test.ExpectedDiagnostics.AddRange(new[]
            {
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.AddedRule).WithLocation(0).WithArguments("M2", "Windows", "10.1.2.3"),
                VerifyCS.Diagnostic(PlatformCompatabilityAnalyzer.AddedRule).WithLocation(1).WithArguments("M2", "Windows", "10.1.2.3")
            });

            await test.RunAsync();
        }

        private readonly string MockPlatformApiSource = @"
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
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    public sealed class MinimumOSPlatformAttribute : OSPlatformAttribute
    {
        public MinimumOSPlatformAttribute(string platformName) : base(platformName)
        { }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Event |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    public sealed class RemovedInOSPlatformAttribute : OSPlatformAttribute
    {
        public RemovedInOSPlatformAttribute(string platformName) : base(platformName)
        { }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Event |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
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

#pragma warning restore CA1801, IDE0060 // Review unused parameters
    }
}";
    }
}
