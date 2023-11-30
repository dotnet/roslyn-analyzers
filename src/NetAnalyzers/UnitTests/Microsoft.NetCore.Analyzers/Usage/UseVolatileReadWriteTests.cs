// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.UnitTests.UseVolatileReadWriteAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Usage.CSharpUseVolatileReadWriteFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.UnitTests.UseVolatileReadWriteAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Usage.BasicUseVolatileReadWriteFixer>;

namespace Microsoft.NetCore.Analyzers.Usage.UnitTests
{
    public sealed class UseVolatileReadWriteTests
    {
        [Theory]
        [InlineData("IntPtr")]
        [InlineData("UIntPtr")]
        [InlineData("byte")]
        [InlineData("double")]
        [InlineData("float")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("ushort")]
        public Task CS_UseVolatileRead(string type)
        {
            var code = $$"""
                         using System;
                         using System.Threading;

                         #nullable enable
                         class Test
                         {
                             void M({{type}} arg)
                             {
                                 {|#0:Thread.VolatileRead(ref arg)|};
                             }
                         }
                         """;
            var fixedCode = $$"""
                              using System;
                              using System.Threading;

                              #nullable enable
                              class Test
                              {
                                  void M({{type}} arg)
                                  {
                                      Volatile.Read(ref arg);
                                  }
                              }
                              """;

            return new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = fixedCode,
                ExpectedDiagnostics = { new DiagnosticResult(UseVolatileReadWriteAnalyzer.ReadDescriptor).WithLocation(0) },
                LanguageVersion = LanguageVersion.CSharp8,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp30
            }.RunAsync();
        }

        [Fact]
        public Task CS_UseVolatileRead_Nullable()
        {
            const string code = """
                                using System;
                                using System.Threading;

                                #nullable enable
                                class Test
                                {
                                    void M(object? arg)
                                    {
                                        {|#0:Thread.VolatileRead(ref arg)|};
                                    }
                                }
                                """;
            const string fixedCode = """
                                     using System;
                                     using System.Threading;

                                     #nullable enable
                                     class Test
                                     {
                                         void M(object? arg)
                                         {
                                             Volatile.Read(ref arg);
                                         }
                                     }
                                     """;

            return new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = fixedCode,
                ExpectedDiagnostics = { new DiagnosticResult(UseVolatileReadWriteAnalyzer.ReadDescriptor).WithLocation(0) },
                LanguageVersion = LanguageVersion.CSharp8
            }.RunAsync();
        }

        [Fact]
        public Task CS_UseVolatileRead_NonNullable()
        {
            const string code = """
                                using System;
                                using System.Threading;

                                class Test
                                {
                                    void M(object arg)
                                    {
                                        {|#0:Thread.VolatileRead(ref arg)|};
                                    }
                                }
                                """;
            const string fixedCode = """
                                     using System;
                                     using System.Threading;

                                     class Test
                                     {
                                         void M(object arg)
                                         {
                                             Volatile.Read(ref arg);
                                         }
                                     }
                                     """;
            var expectedDiagnostic = new DiagnosticResult(UseVolatileReadWriteAnalyzer.ReadDescriptor).WithLocation(0);

            return VerifyCS.VerifyCodeFixAsync(code, expectedDiagnostic, fixedCode);
        }

        [Theory]
        [InlineData("IntPtr")]
        [InlineData("UIntPtr")]
        [InlineData("byte")]
        [InlineData("double")]
        [InlineData("float")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("ushort")]
        public Task CS_UseVolatileWrite(string type)
        {
            var code = $$"""
                         using System;
                         using System.Threading;

                         #nullable enable
                         class Test
                         {
                             void M({{type}} arg, {{type}} value)
                             {
                                 {|#0:Thread.VolatileWrite(ref arg, value)|};
                             }
                         }
                         """;
            var fixedCode = $$"""
                              using System;
                              using System.Threading;

                              #nullable enable
                              class Test
                              {
                                  void M({{type}} arg, {{type}} value)
                                  {
                                      Volatile.Write(ref arg, value);
                                  }
                              }
                              """;

            return new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = fixedCode,
                ExpectedDiagnostics = { new DiagnosticResult(UseVolatileReadWriteAnalyzer.WriteDescriptor).WithLocation(0) },
                LanguageVersion = LanguageVersion.CSharp8
            }.RunAsync();
        }

        [Fact]
        public Task CS_UseVolatileWrite_Nullable()
        {
            const string code = """
                                using System;
                                using System.Threading;

                                #nullable enable
                                class Test
                                {
                                    void M(object? arg, object? value)
                                    {
                                        {|#0:Thread.VolatileWrite(ref arg, value)|};
                                    }
                                }
                                """;
            const string fixedCode = """
                                     using System;
                                     using System.Threading;

                                     #nullable enable
                                     class Test
                                     {
                                         void M(object? arg, object? value)
                                         {
                                             Volatile.Write(ref arg, value);
                                         }
                                     }
                                     """;

            return new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = fixedCode,
                ExpectedDiagnostics = { new DiagnosticResult(UseVolatileReadWriteAnalyzer.WriteDescriptor).WithLocation(0) },
                LanguageVersion = LanguageVersion.CSharp8
            }.RunAsync();
        }

        [Fact]
        public Task CS_UseVolatileWrite_NonNullable()
        {
            const string code = """
                                using System;
                                using System.Threading;

                                class Test
                                {
                                    void M(object arg, object value)
                                    {
                                        {|#0:Thread.VolatileWrite(ref arg, value)|};
                                    }
                                }
                                """;
            const string fixedCode = """
                                     using System;
                                     using System.Threading;

                                     class Test
                                     {
                                         void M(object arg, object value)
                                         {
                                             Volatile.Write(ref arg, value);
                                         }
                                     }
                                     """;
            var expectedDiagnostic = new DiagnosticResult(UseVolatileReadWriteAnalyzer.WriteDescriptor).WithLocation(0);

            return VerifyCS.VerifyCodeFixAsync(code, expectedDiagnostic, fixedCode);
        }

        [Theory]
        [InlineData("IntPtr")]
        [InlineData("UIntPtr")]
        [InlineData("Byte")]
        [InlineData("Double")]
        [InlineData("Single")]
        [InlineData("Integer")]
        [InlineData("Long")]
        [InlineData("Object")]
        [InlineData("Sbyte")]
        [InlineData("Short")]
        [InlineData("UInteger")]
        [InlineData("ULong")]
        [InlineData("UShort")]
        public Task VB_UseVolatileRead(string type)
        {
            var code = $$"""
                         Imports System
                         Imports System.Threading

                         Class Test
                             Sub M(arg As {{type}})
                                 {|#0:Thread.VolatileRead(arg)|}
                             End Sub
                         End Class
                         """;
            var fixedCode = $"""
                             Imports System
                             Imports System.Threading

                             Class Test
                                 Sub M(arg As {type})
                                     Volatile.Read(arg)
                                 End Sub
                             End Class
                             """;

            var expectedDiagnostic = new DiagnosticResult(UseVolatileReadWriteAnalyzer.ReadDescriptor).WithLocation(0);

            return VerifyVB.VerifyCodeFixAsync(code, expectedDiagnostic, fixedCode);
        }

        [Theory]
        [InlineData("IntPtr")]
        [InlineData("UIntPtr")]
        [InlineData("Byte")]
        [InlineData("Double")]
        [InlineData("Single")]
        [InlineData("Integer")]
        [InlineData("Long")]
        [InlineData("Object")]
        [InlineData("Sbyte")]
        [InlineData("Short")]
        [InlineData("UInteger")]
        [InlineData("ULong")]
        [InlineData("UShort")]
        public Task VB_UseVolatileWrite(string type)
        {
            var code = $$"""
                         Imports System
                         Imports System.Threading

                         Class Test
                             Sub M(arg As {{type}}, value As {{type}})
                                 {|#0:Thread.VolatileWrite(arg, value)|}
                             End Sub
                         End Class
                         """;
            var fixedCode = $"""
                             Imports System
                             Imports System.Threading

                             Class Test
                                 Sub M(arg As {type}, value As {type})
                                     Volatile.Write(arg, value)
                                 End Sub
                             End Class
                             """;

            var expectedDiagnostic = new DiagnosticResult(UseVolatileReadWriteAnalyzer.WriteDescriptor).WithLocation(0);

            return VerifyVB.VerifyCodeFixAsync(code, expectedDiagnostic, fixedCode);
        }
    }
}