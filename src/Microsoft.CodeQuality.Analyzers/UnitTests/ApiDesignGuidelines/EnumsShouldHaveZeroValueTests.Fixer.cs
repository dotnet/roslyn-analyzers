// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class CA1008FixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new EnumsShouldHaveZeroValueAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicEnumsShouldHaveZeroValueFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnumsShouldHaveZeroValueAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpEnumsShouldHaveZeroValueFixer();
        }

        [Fact]
        public void CSharp_EnumsShouldZeroValueFlagsRename()
        {
            var code = @"
public class Outer
{
    [System.Flags]
    private enum E
    {
        A = 0,
        B = 3
    }
}

[System.Flags]
public enum E2
{
    A2 = 0,
    B2 = 1
}

[System.Flags]
public enum E3
{
    A3 = (ushort)0,
    B3 = (ushort)1
}

[System.Flags]
public enum E4
{
    A4 = 0,
    B4 = (int)2  // Sample comment
}

[System.Flags]
public enum NoZeroValuedField
{
    A5 = 1,
    B5 = 2
}";

            var expectedFixedCode = @"
public class Outer
{
    [System.Flags]
    private enum E
    {
        None = 0,
        B = 3
    }
}

[System.Flags]
public enum E2
{
    None = 0,
    B2 = 1
}

[System.Flags]
public enum E3
{
    None = (ushort)0,
    B3 = (ushort)1
}

[System.Flags]
public enum E4
{
    None = 0,
    B4 = (int)2  // Sample comment
}

[System.Flags]
public enum NoZeroValuedField
{
    A5 = 1,
    B5 = 2
}";
            VerifyCSharpFix(code, expectedFixedCode);
        }

        [Fact]
        public void CSharp_EnumsShouldZeroValueFlagsMultipleZero()
        {
            var code = @"// Some comment
public class Outer
{
    [System.Flags]
    private enum E
    {
        None = 0,
        A = 0
    }
}
// Some comment
[System.Flags]
internal enum E2
{
    None = 0,
    A = None
}";
            var expectedFixedCode = @"// Some comment
public class Outer
{
    [System.Flags]
    private enum E
    {
        None = 0
    }
}
// Some comment
[System.Flags]
internal enum E2
{
    None = 0
}";
            VerifyCSharpFix(code, expectedFixedCode);
        }

        [Fact]
        public void CSharp_EnumsShouldZeroValueNotFlagsNoZeroValue()
        {
            var code = @"
public class Outer
{
    private enum E
    {
        A = 1
    }

    private enum E2
    {
        None = 1,
        A = 2
    }
}

internal enum E3
{
    None = 0,
    A = 1
}

internal enum E4
{
    None = 0,
    A = 0
}
";

            var expectedFixedCode = @"
public class Outer
{
    private enum E
    {
        None,
        A = 1
    }

    private enum E2
    {
        None,
        A = 2
    }
}

internal enum E3
{
    None = 0,
    A = 1
}

internal enum E4
{
    None = 0,
    A = 0
}
";
            VerifyCSharpFix(code, expectedFixedCode);
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueFlagsRename()
        {
            var code = @"
Class Outer
    <System.Flags>
    Private Enum E
        A = 0
        B = 1
    End Enum
End Class

<System.Flags>
Public Enum E2
    A2 = 0
    B2 = 1
End Enum

<System.Flags>
Public Enum E3
    A3 = CUShort(0)
    B3 = CUShort(1)
End Enum

<System.Flags>
Public Enum NoZeroValuedField
    A5 = 1
    B5 = 2
End Enum
";

            var expectedFixedCode = @"
Class Outer
    <System.Flags>
    Private Enum E
        None = 0
        B = 1
    End Enum
End Class

<System.Flags>
Public Enum E2
    None = 0
    B2 = 1
End Enum

<System.Flags>
Public Enum E3
    None = CUShort(0)
    B3 = CUShort(1)
End Enum

<System.Flags>
Public Enum NoZeroValuedField
    A5 = 1
    B5 = 2
End Enum
";
            VerifyBasicFix(code, expectedFixedCode);
        }

        [WorkItem(836193, "DevDiv")]
        [Fact]
        public void VisualBasic_EnumsShouldZeroValueFlagsRename_AttributeListHasTrivia()
        {
            var code = @"
Class Outer
    <System.Flags> _
    Private Enum E
        A = 0
        B = 1
    End Enum
End Class

<System.Flags> _
Public Enum E2
    A2 = 0
    B2 = 1
End Enum

<System.Flags> _
Public Enum E3
    A3 = CUShort(0)
    B3 = CUShort(1)
End Enum

<System.Flags> _
Public Enum NoZeroValuedField
    A5 = 1
    B5 = 2
End Enum
";

            var expectedFixedCode = @"
Class Outer
    <System.Flags> _
    Private Enum E
        None = 0
        B = 1
    End Enum
End Class

<System.Flags> _
Public Enum E2
    None = 0
    B2 = 1
End Enum

<System.Flags> _
Public Enum E3
    None = CUShort(0)
    B3 = CUShort(1)
End Enum

<System.Flags> _
Public Enum NoZeroValuedField
    A5 = 1
    B5 = 2
End Enum
";
            VerifyBasicFix(code, expectedFixedCode);
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueFlagsMultipleZero()
        {
            var code = @"
Class Outer
    <System.Flags>
    Private Enum E
        None = 0
        A = 0
    End Enum
End Class

<System.Flags>
Friend Enum E2
    None = 0
    A = None
End Enum

<System.Flags>
Public Enum E3
    A3 = 0
    B3 = CUInt(0)  ' Not a constant
End Enum";

            var expectedFixedCode = @"
Class Outer
    <System.Flags>
    Private Enum E
        None = 0
    End Enum
End Class

<System.Flags>
Friend Enum E2
    None = 0
End Enum

<System.Flags>
Public Enum E3
    None
End Enum";

            VerifyBasicFix(code, expectedFixedCode);
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueNotFlagsNoZeroValue()
        {
            var code = @"
Class C
    Private Enum E
        A = 1
    End Enum

    Private Enum E2
        None = 1
        A = 2
    End Enum
End Class

Friend Enum E3
    None = 0
    A = 1
End Enum

Friend Enum E4
    None = 0
    A = 0
End Enum
";

            var expectedFixedCode = @"
Class C
    Private Enum E
        None
        A = 1
    End Enum

    Private Enum E2
        None
        A = 2
    End Enum
End Class

Friend Enum E3
    None = 0
    A = 1
End Enum

Friend Enum E4
    None = 0
    A = 0
End Enum
";
            VerifyBasicFix(code, expectedFixedCode);
        }
    }
}
