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
        [InlineData("byte", "7, 14, 128, 255")]
        [InlineData("sbyte", "-41, 11, 0")]
        public Task LiteralReadOnlyArrayFields_Diagnostic_CS(string arrayType, string arrayInitializer)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
using System;
public class C
{{
    {{|#0:private static readonly {arrayType}[] _array = new {arrayType}[] {{ {arrayInitializer} }}|}};
}}",
                ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithLocation(0) },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        [Theory]
        [InlineData("byte b = a[2];")]
        [InlineData("byte c = a[1] + a[2];")]
        [InlineData("a[0].ToString();")]
        [InlineData("ConsumeRos(a);")]
        [InlineData("ConsumeRos(a.AsSpan());")]
        [InlineData("ConsumeRos(a.AsSpan(3));")]
        [InlineData("ConsumeRos(a.AsSpan(1, 4));")]
        public Task LiteralReadOnlyArrayField_LegalUses_Diagnostic_CS(string code)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
using System;
public class C
{{
    {{|#0:private static readonly byte[] a = new byte[] {{ 2, 4, 8, 16 }}|}};
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
        public Task NonLiteralInitializer_NoDiagnostic_CS(string initializer)
        {
            var test = new VerifyCS.Test
            {
                TestCode = $@"
using System;
public class C
{{
    private static readonly byte[] a = {initializer};
    private static byte[] GetBytes() => null;
    private static byte GetByte() => 4;
}}",
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        [Theory]
        [InlineData("ConsumeArray(a);")]
        [InlineData("ConsumeSpan(a);")]
        [InlineData("ConsumeEnumerable(a);")]
        [InlineData("a[0] = 12;")]
        [InlineData("a[1] += 12;")]
        [InlineData("a[2] -= 12;")]
        [InlineData("a[3] *= 12;")]
        [InlineData("a[4] /= 12;")]
        [InlineData("ref byte r = ref a[1];")]
        [InlineData("ConsumeByteRef(ref a[3]);")]
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

    public void M()
    {{
        {code}
    }}
}}",
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
            };
            return test.RunAsync();
        }

        private static DiagnosticDescriptor Rule => PreferReadOnlySpanPropertiesOverReadOnlyArrayFields.Rule;
    }
}
