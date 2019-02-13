// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class CallSiteImplicitAllocationAnalyzerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void CallSiteImplicitAllocation_Param()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {

        Params();
        Params(1, 2);
        Params(new [] { 1, 2}); // explicit, so no warning
        ParamsWithObjects(new [] { 1, 2}); // explicit, but converted to objects, so stil la warning?!

        // Only 4 args and above use the params overload of String.Format
        var test = String.Format(""Testing {0}, {1}, {2}, {3}"", 1, ""blah"", 2.0m, 'c');
    }

    public void Params(params int[] args)
    {
    }

    public void ParamsWithObjects(params object[] args)
    {
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(10,9): warning HAA0101: This call site is calling into a function with a 'params' parameter. This results in an array allocation even if no parameter is passed in for the params parameter
                        GetCSharpResultAt(10, 9, CallSiteImplicitAllocationAnalyzer.ParamsParameterRule),
                        // Test0.cs(11,9): warning HAA0101: This call site is calling into a function with a 'params' parameter. This results in an array allocation even if no parameter is passed in for the params parameter
                        GetCSharpResultAt(11, 9, CallSiteImplicitAllocationAnalyzer.ParamsParameterRule),
                        // Test0.cs(13,9): warning HAA0101: This call site is calling into a function with a 'params' parameter. This results in an array allocation even if no parameter is passed in for the params parameter
                        GetCSharpResultAt(13, 9, CallSiteImplicitAllocationAnalyzer.ParamsParameterRule),
                        // Test0.cs(16,20): warning HAA0101: This call site is calling into a function with a 'params' parameter. This results in an array allocation even if no parameter is passed in for the params parameter
                        GetCSharpResultAt(16, 20, CallSiteImplicitAllocationAnalyzer.ParamsParameterRule));
        }

        [Fact]
        public void CallSiteImplicitAllocation_NonOverridenMethodOnStruct()
        {
            var sampleProgram = @"
using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var normal = new Normal().GetHashCode();
        var overridden = new OverrideToHashCode().GetHashCode();
    }
}

public struct Normal
{
}

public struct OverrideToHashCode
{
    public override int GetHashCode()
    {
        return -1;
    }
}";


            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(10,22): warning HAA0102: Non-overridden virtual method call on a value type adds a boxing or constrained instruction
                        GetCSharpResultAt(10, 22, CallSiteImplicitAllocationAnalyzer.ValueTypeNonOverridenCallRule));
        }

        [Fact]
        public void CallSiteImplicitAllocation_DoNotReportNonOverriddenMethodCallForStaticCalls()
        {
            var sampleProgram = @"
using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var t = System.Enum.GetUnderlyingType(typeof(System.StringComparison));
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true);
        }

        [Fact]
        public void CallSiteImplicitAllocation_DoNotReportNonOverriddenMethodCallForNonVirtualCalls()
        {
            var sampleProgram = @"
using System.IO;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        FileAttributes attr = FileAttributes.System;
        attr.HasFlag (FileAttributes.Directory);
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CallSiteImplicitAllocationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }
    }
}
