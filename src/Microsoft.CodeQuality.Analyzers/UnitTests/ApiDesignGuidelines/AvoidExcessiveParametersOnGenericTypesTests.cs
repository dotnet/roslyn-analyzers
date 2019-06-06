// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidExcessiveParametersOnGenericTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidExcessiveParametersOnGenericTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidExcessiveParametersOnGenericTypesTests
    {
        [Fact]
        public async Task PublicType_NoArguments_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                End Class");
        }


        [Fact]
        public async Task PublicGenericType_OneArgument_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class<T>
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass](Of T)
                End Class");
        }

        [Fact]
        public async Task PublicGenericType_TwoArguments_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class<T1, T2>
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass](Of T1, T2)
                End Class");
        }

        [Fact]
        public async Task PublicGenericType_ThreeArguments_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class [|Class|]<T1, T2, T3>
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [|[MyClass]|](Of T1, T2, T3)
                End Class");
        }


        [Fact]
        public async Task InternalGenericType_ThreeArguments_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                internal class Class<T1, T2, T3>
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Friend Class [MyClass](Of T1, T2, T3)
                End Class");
        }
    }
}