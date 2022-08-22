// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Usage.CSharpImplementGenericMathInterfacesCorrectly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Usage.UnitTests
{
    public class ImplementGenericMathInterfacesCorrectlyTests
    {
#if DEBUG
        [Fact]
        public async Task IParsableNotImplementedCorrectly()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public readonly struct MyDate : IParsable<{|#0:DateOnly|}> // 'IParsable' interface requires the derived type 'MyDate' used for the 'TSelf' type parameter
{ }
" + MockTypes, VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.CRTPRule).WithLocation(0).WithArguments("IParsable", "MyDate"));
        }

        [Fact]
        public async Task IParsableImplementedCorrectlyNotWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public readonly struct MyDate1 : IParsable<MyDate1>
{ }

public class Test : ISpanParsable<Test>
{ }

public readonly struct MyDate<TSelf> : IParsable<TSelf> where TSelf : IParsable<TSelf>
{ }
" + MockTypes);
        }

        [Fact]
        public async Task ISpanParsableNotImplementedCorrectly()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class Test : ISpanParsable<{|#0:DateOnly|}> // 'ISpanParsable' interface requires the derived type 'Test' used for the 'TSelf' type parameter
{ }
" + MockTypes, VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.CRTPRule).WithLocation(0).WithArguments("ISpanParsable", "Test"));
        }

        [Fact]
        public async Task IAdditionOperatorsNotImplementedCorrectly()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Numerics;

public class Test : IAdditionOperators<Test, MyTest, long>
{ }
public class MyTest : IAdditionOperators<[|Test|], MyTest, long> // 'IAdditionOperators' interface requires the derived type 'MyTest' used for the 'TSelf' type parameter
{ }
" + MockTypes);
        }

        private readonly string MockTypes = @"
namespace System
{
    public interface IParsable<TSelf> where TSelf : IParsable<TSelf>
    { }

    public readonly struct DateOnly : ISpanParsable<DateOnly>
    { }

    public interface ISpanParsable<TSelf> : IParsable<TSelf> where TSelf : ISpanParsable<TSelf>
    { }
}

namespace System.Numerics
{
    public interface IDecrementOperators<TSelf> where TSelf : IDecrementOperators<TSelf>
    { }
    public interface IAdditionOperators<TSelf, TOther, TResult> where TSelf : IAdditionOperators<TSelf, TOther, TResult>
    { }
}";
#endif
    }
}


