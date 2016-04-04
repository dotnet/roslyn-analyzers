// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class EnumStorageShouldBeInt32FixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new EnumStorageShouldBeInt32Analyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnumStorageShouldBeInt32Analyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicEnumStorageShouldBeInt32Fixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpEnumStorageShouldBeInt32Fixer();
        }

        #region CSharpUnitTests
        [Fact]
        public void CSharp_CA1028_TestFixForEnumTypeIsLongWithNoTrivia()
        {
            var code = @"
using System;
namespace Test
{
    public enum TestEnum1: long
    {
        Value1 = 1,
        Value2 = 2
    }
}
";
            var fix = @"
using System;
namespace Test
{
    public enum TestEnum1
    {
        Value1 = 1,
        Value2 = 2
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void CSharp_CA1028_TestFixForEnumTypeIsLongWithTrivia()
        {
            var code = @"
using System;
namespace Test
{
    public enum TestEnum1: long // with trivia
    {
        Value1 = 1,
        Value2 = 2
    }
}
";
            var fix = @"
using System;
namespace Test
{
    public enum TestEnum1 // with trivia
    {
        Value1 = 1,
        Value2 = 2
    }
}
";
            VerifyCSharpFix(code, fix);
        }
        #endregion

        #region BasicUnitTests

        [Fact]
        public void Basic_CA1028_TestFixForEnumTypeIsLongWithNoTrivia()
        {
            var code = @"
Imports System
Public Module Module1
    Public Enum TestEnum1 As Long
        Value1 = 1
        Value2 = 2
    End Enum
End Module
";
            var fix = @"
Imports System
Public Module Module1
    Public Enum TestEnum1 
        Value1 = 1
        Value2 = 2
    End Enum
End Module
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void Basic_CA1028_TestFixForEnumTypeIsLongWithTrivia()
        {
            var code = @"
Imports System
Public Module Module1
    Public Enum TestEnum1 As Long 'with trivia 
        Value1 = 1
        Value2 = 2
    End Enum
End Module
";
            var fix = @"
Imports System
Public Module Module1
    Public Enum TestEnum1  'with trivia 
        Value1 = 1
        Value2 = 2
    End Enum
End Module
";
            VerifyBasicFix(code, fix);
        }

        #endregion
    }
}