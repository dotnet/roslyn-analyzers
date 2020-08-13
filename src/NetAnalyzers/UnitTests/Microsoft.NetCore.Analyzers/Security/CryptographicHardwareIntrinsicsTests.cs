// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.CryptographicHardwareIntrinsicsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.CryptographicHardwareIntrinsicsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class CryptographicHardwareIntrinsicsTests
    {
        private DiagnosticResult GetCSharpResultAt(int line, int column) =>
            VerifyCS.Diagnostic(CryptographicHardwareIntrinsicsAnalyzer.s_rule).WithLocation(line, column);

        private DiagnosticResult GetBasicResultAt(int line, int column) =>
            VerifyVB.Diagnostic(CryptographicHardwareIntrinsicsAnalyzer.s_rule).WithLocation(line, column);

        [Fact]
        public async Task InvokingDecryptLast_CS_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public class C
{
    public static Vector128<byte> Method()
    {
        return Aes.Decrypt(default, default);
    }
}
", GetCSharpResultAt(9, 20));
        }
    }
}
