// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class EnumStorageShouldBeInt32Tests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new EnumStorageShouldBeInt32Analyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnumStorageShouldBeInt32Analyzer();
        }

        #region CSharpUnitTests

        [Fact]
        public void CSharp_CA1028_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
namespace Test
{
    public enum TestEnum1 //no violation - because underlying type is Int32
    {
        Value1 = 1,
        Value2 = 2
    }
    public static class OuterClass
    {
        [Flags]
        public enum TestEnum2 : long //no violation - because underlying type is Int64 and has Flag attributes
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = Value1 | Value2
        }
        private enum TestEnum3 : byte //no violation - because accessibility is private 
        {
            Value1 = 1,
            Value2 = 2
        }
        internal class innerClass
        {
            public enum TestEnum4 : long //no violation - because resultant accessibility is private 
            {
                Value1 = 1,
                Value2 = 2
            }
        }
        public static void Main()
        {
        }
    }
}
 ");
        }

        [Fact]
        public void CSharp_CA1028_Diagnostic1()
        {
            VerifyCSharp(@"
using System;
namespace Test
{
    public enum TestEnum1 : long // violation - because underlying type is Int64 and has no Flags attribute
    {
        Value1 = 1,
        Value2 = 2
    }
}
",
    GetCA1028CSharpResultAt(line: 5, column: 17, enumIdentifier: "TestEnum1", underlyingType: "long"));
        }

        [Fact]
        public void CSharp_CA1028_Diagnostic2()
        {
            VerifyCSharp(@"
using System;
namespace Test
{
    public enum TestEnum2 : sbyte // violation - because underlying type is not Int32
    {
        Value1 = 1,
        Value2 = 2
    }
}
",
    GetCA1028CSharpResultAt(line: 5, column: 17, enumIdentifier: "TestEnum2", underlyingType: "sbyte"));
        }

        [Fact]
        public void CSharp_CA1028_Diagnostic3()
        {
            VerifyCSharp(@"
using System;
namespace Test
{
    public enum TestEnum3 : ushort // violation - because underlying type is not Int32
    {
        Value1 = 1,
        Value2 = 2
    }
}
",
    GetCA1028CSharpResultAt(line: 5, column: 17, enumIdentifier: "TestEnum3", underlyingType: "ushort"));
        }
        #endregion

        #region BasicUnitTests

        [Fact]
        public void Basic_CA1028_NoDiagnostic()
        {
            VerifyBasic(@"
Imports System
Public Module Module1
    Public Enum TestEnum1 'no violation - because underlying type is Int32
        Value1 = 1
        Value2 = 2
    End Enum
    Public Class OuterClass
        <Flags()>
        Public Enum TestEnum2 As Long 'no violation - because underlying type is Int64 and has Flag attributes
            Value1 = 1
            Value2 = 2
            Value3 = Value1 Or Value2
        End Enum
        Private Enum TestEnum3 As Byte 'no violation - because accessibility Is private 
            Value1 = 1
            Value2 = 2
        End Enum
        Private Class innerClass
            Public Enum TestEnum4 As Long 'no violation - because resultant accessibility Is private 
                Value1 = 1
                Value2 = 2
            End Enum
        End Class
    End Class
    Sub Main()
    End Sub
End Module
 ");
        }

        [Fact]
        public void Basic_CA1028_Diagnostic1()
        {
            VerifyBasic(@"
Imports System
Public Module Module1
    Public Enum TestEnum1 As Long 'violation - because underlying type is Int64 and has no Flags attribute
        Value1 = 1
        Value2 = 2
    End Enum
    Sub Main()
    End Sub
End Module
",
    GetCA1028BasicResultAt(line: 4, column: 17, enumIdentifier: "TestEnum1", underlyingType: "Long"));
        }

        [Fact]
        public void Basic_CA1028_Diagnostic2()
        {
            VerifyBasic(@"
Imports System
Public Module Module1
    Public Enum TestEnum2 As Byte 'violation - because underlying type is not Int32
        Value1 = 1
        Value2 = 2
    End Enum
    Sub Main()
    End Sub
End Module
",
    GetCA1028BasicResultAt(line: 4, column: 17, enumIdentifier: "TestEnum2", underlyingType: "Byte"));
        }

        [Fact]
        public void Basic_CA1028_Diagnostic3()
        {
            VerifyBasic(@"
Imports System
Public Module Module1
    Public Enum TestEnum3 As UShort 'violation - because underlying type is not Int32
        Value1 = 1
        Value2 = 2
    End Enum
    Sub Main()
    End Sub
End Module
",
    GetCA1028BasicResultAt(line: 4, column: 17, enumIdentifier: "TestEnum3", underlyingType: "UShort"));
        }
        #endregion

        #region Helpers

        private static DiagnosticResult GetCA1028CSharpResultAt(int line, int column, string enumIdentifier, string underlyingType)
        {
            // Format - Make the underlying type of {0} System.Int32 instead of {1}.
            var message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumStorageShouldBeInt32Message, enumIdentifier, underlyingType);
            return GetCSharpResultAt(line, column, EnumStorageShouldBeInt32Analyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1028BasicResultAt(int line, int column, string enumIdentifier, string underlyingType)
        {
            // Format - Make the underlying type of {0} System.Int32 instead of {1}.
            var message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumStorageShouldBeInt32Message, enumIdentifier, underlyingType);
            return GetBasicResultAt(line, column, EnumStorageShouldBeInt32Analyzer.RuleId, message);
        }
        #endregion

    }
}
