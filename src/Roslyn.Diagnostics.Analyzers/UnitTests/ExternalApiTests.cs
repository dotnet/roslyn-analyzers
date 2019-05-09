// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.ExternalApiAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.ExternalApiAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class ExternalApiTests
    {
        [Fact]
        public async Task TestPublicFieldInInternalType_CSharp()
        {
            var source = @"
using Roslyn.Utilities;

[ExternalApi]
class Class {
  public InternalType Field;
}

[ExternalApi]
struct InternalType { }
" + ExternalApi.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task TestPublicFieldInInternalType_VisualBasic()
        {
            var source = @"
Imports Roslyn.Utilities

<ExternalApi>
Friend Class [Class]
  Public Field As InternalType
End Class 

<ExternalApi>
Friend Structure InternalType
End Structure
" + ExternalApi.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task TestPublicFieldInInternalTypeNotAccessible_CSharp()
        {
            var source = @"
using Roslyn.Utilities;

[ExternalApi]
class Class {
  public InternalType [|Field|];
}

struct InternalType { }
" + ExternalApi.CSharp;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task TestPublicFieldInInternalTypeNotAccessible_VisualBasic()
        {
            var source = @"
Imports Roslyn.Utilities

<ExternalApi>
Friend Class [Class]
  Public [|Field|] As InternalType
End Class 

Friend Structure InternalType
End Structure
" + ExternalApi.VisualBasic;

            await VerifyVB.VerifyAnalyzerAsync(source);
        }
    }
}
