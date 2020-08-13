// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
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
            var test = new VerifyCS.Test
            {
                ReferenceAssemblies = new ReferenceAssemblies(
                    "netcoreapp3.1",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "3.1.0"),
                    Path.Combine("ref", "netcoreapp3.1")),
                TestCode = @"
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public class C
{
    public static Vector128<byte> Method()
    {
        return Aes.Decrypt(default, default);
    }
}
"
            };
            test.ExpectedDiagnostics.Add(GetCSharpResultAt(9, 16));
            await test.RunAsync();

        }
    }
}
