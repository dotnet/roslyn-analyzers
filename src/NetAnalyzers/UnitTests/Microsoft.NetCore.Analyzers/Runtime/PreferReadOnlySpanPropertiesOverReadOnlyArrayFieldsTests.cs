// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields,
    Microsoft.NetCore.Analyzers.Runtime.PreferReadOnlySpanPropertiesOverReadOnlyArrayFieldsFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferReadOnlySpanPropertiesOverReadOnlyArrayFieldsTests
    {
        [Theory]
        [InlineData("bool", "true, true, false, false, true")]
        [InlineData("bool", "false")]
        [InlineData("bool", "ConstBool, true, false")]
        [InlineData("bool", "true, ConstBool, true")]
        [InlineData("bool", "")]
        [InlineData("byte", "7, 14, 128, 255")]
        [InlineData("byte", "8, 16, ConstByte")]
        [InlineData("byte", "")]
        [InlineData("sbyte", "-41, 11, 0")]
        [InlineData("sbyte", "ConstSByte, ConstSByte")]
        [InlineData("sbyte", "")]
        public Task ConstReadOnlyArrayFields_Diagnostic_CS(string arrayType, string arrayInitializer)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
using System;
public class C
{{
    private static readonly {arrayType}[] {{|#0:_array|}} = new {arrayType}[] {{ {arrayInitializer} }};
    private const byte ConstByte = 7;
    private const sbyte ConstSByte = -7;
    private const bool ConstBool = true;
}}",
                ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithLocation(0) },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        [Theory]
        [InlineData("int n = a[2];")]
        [InlineData("int n = a[1] + a[2];")]
        [InlineData("int n = a.Length;")]
        [InlineData("a[0].ToString();")]
        [InlineData("ConsumeRos(a);")]
        [InlineData("ConsumeRos(a.AsSpan());")]
        [InlineData("ConsumeRos(a.AsSpan(3));")]
        [InlineData("ConsumeRos(a.AsSpan(1, 4));")]
        public Task LegalUsage_Diagnostic_CS(string code)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
using System;
public class C
{{
    private static readonly byte[] {{|#0:a|}} = new byte[] {{ 2, 4, 8, 16 }};
    public void ConsumeRos(ReadOnlySpan<byte> span) {{ }}

    public void M()
    {{
        {code}
    }}
}}",
                ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithLocation(0) },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        [Theory]
        [InlineData("new byte[] { GetByte(), 7, 14 }")]
        [InlineData("GetBytes()")]
        [InlineData("new byte[] { 6, 19, GetByte() }")]
        [InlineData("new byte[] { 5, readOnlyByte, 5 }")]
        [InlineData("new byte[] { 4, mutableByte, 4 }")]
        public Task NonConstInitializer_NoDiagnostic_CS(string initializer)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
using System;
public class C
{{
    private static byte[] GetBytes() => null;
    private static byte GetByte() => 4;
    private static readonly byte readOnlyByte = 7;
    private static byte mutableByte = 6;

    private static readonly byte[] a = {initializer};
}}",
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        [Theory]
        [InlineData("ConsumeArray(a);")]
        [InlineData("ConsumeSpan(a);")]
        [InlineData("ConsumeEnumerable(a);")]
        [InlineData("int n = a.Rank;")]
        [InlineData("a.CopyTo(new byte[5], 0);")]
        [InlineData("a[0] = 12;")]
        [InlineData("(a[0], a[1]) = t;")]
        [InlineData("(a[1], b) = t;")]
        [InlineData("(b, a[1]) = t;")]
        [InlineData("a[1] += 12;")]
        [InlineData("a[2] -= 12;")]
        [InlineData("a[3] *= 12;")]
        [InlineData("a[4] /= 12;")]
        [InlineData("a[1]++;")]
        [InlineData("++a[1];")]
        [InlineData("a[1]--;")]
        [InlineData("--a[1];")]
        [InlineData("ref byte r = ref a[1];")]
        [InlineData("ConsumeByteRef(ref a[3]);")]
        [InlineData("ConsumeByteOut(out a[3]);")]
        [InlineData("ConsumeImplicit(a);")]
        [InlineData("ConsumeExplicit((Explicit)a);")]
        public Task IllegalFieldUsage_NoDiagnostic_CS(string code)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
using System;
using System.Collections.Generic;
public class C
{{
    private static readonly byte[] a = new byte[] {{ 1, 2, 3 }};
    private static void ConsumeArray(byte[] bytes) {{ }}
    private static void ConsumeEnumerable(IEnumerable<byte> bytes) {{ }}
    private static void ConsumeSpan(Span<byte> bytes) {{ }}
    private static void ConsumeByteRef(ref byte b) {{ }}
    private static void ConsumeByteOut(out byte b) => b = default;
    private static void ConsumeImplicit(Implicit i) {{ }}
    private static void ConsumeExplicit(Explicit e) {{ }}

    public void M(byte b, (byte x, byte y) t)
    {{
        {code}
    }}
}}

public class Implicit
{{
    public static implicit operator Implicit(byte[] operand) => new Implicit();
}}

public class Explicit
{{
    public static explicit operator Explicit(byte[] operand) => new Explicit();
}}",
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        [Fact]
        public Task ElementReturnByRef_NoDiagnostic_CS()
        {
            var test = new VerifyCS.Test
            {
                TestCode = @"
using System;
public class C
{
    private static readonly byte[] a = new byte[] { 1, 2, 3 };

    public ref byte M()
    {
        return ref a[1];
    }
}",
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        [Theory]
        [InlineData("private readonly byte[] a = new byte[] { 1 };")]
        [InlineData("private static byte[] a = new byte[] { 1 };")]
        [InlineData("private static readonly short[] a = new short[] { 1 };")]
        [InlineData("private static readonly int[] a = new int[] { 1 };")]
        [InlineData("private static readonly string[] a = new string[] { nameof(a) };")]
        public Task IllegalDeclarations_NoDiagnostic_CS(string declaration)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
public class C
{{
    {declaration}
}}",
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        private static DiagnosticDescriptor Rule => PreferReadOnlySpanPropertiesOverReadOnlyArrayFields.Rule;
    }
}
