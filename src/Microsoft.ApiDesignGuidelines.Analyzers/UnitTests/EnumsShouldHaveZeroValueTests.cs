﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Diagnostics.Test.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class CA1008Tests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new EnumsShouldHaveZeroValueAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnumsShouldHaveZeroValueAnalyzer();
        }

        [WorkItem(836193, "DevDiv")]
        [Fact]
        public void CSharp_EnumsShouldZeroValueFlagsRename()
        {
            // In enum '{0}', change the name of '{1}' to 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E", "A");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E2", "A2");
            string expectedMessage3 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E3", "A3");
            string expectedMessage4 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E4", "A4");

            var code = @"
[System.Flags]
private enum E
{
    A = 0,
    B = 3
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
    B4 = (uint)2  // Not a constant
}

[System.Flags]
public enum NoZeroValuedField
{
    A5 = 1,
    B5 = 2
}";
            VerifyCSharp(code,
                GetCSharpResultAt(5, 5, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetCSharpResultAt(12, 5, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2),
                GetCSharpResultAt(19, 5, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage3),
                GetCSharpResultAt(26, 5, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage4));
        }

        [Fact]
        public void CSharp_EnumsShouldZeroValueFlagsMultipleZero()
        {
            // Remove all members that have the value zero from {0} except for one member that is named 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsMultipleZeros, "E");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsMultipleZeros, "E2");

            var code = @"// Some comment
[System.Flags]
private enum E
{
    None = 0,
    A = 0
}
// Some comment
[System.Flags]
internal enum E2
{
    None = 0,
    A = None
}";
            VerifyCSharp(code,
                GetCSharpResultAt(3, 14, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetCSharpResultAt(10, 15, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2));
        }

        [Fact]
        public void CSharp_EnumsShouldZeroValueFlagsMultipleZeroWithScope()
        {
            // Remove all members that have the value zero from {0} except for one member that is named 'None'.
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsMultipleZeros, "E2");

            var code = @"// Some comment
[System.Flags]
private enum E
{
    None = 0,
    A = 0
}
[|// Some comment
[System.Flags]
internal enum E2
{
    None = 0,
    A = None
}|]";
            VerifyCSharp(code,
                GetCSharpResultAt(10, 15, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage));
        }

        [Fact]
        public void CSharp_EnumsShouldZeroValueNotFlagsNoZeroValue()
        {
            // Add a member to {0} that has a value of zero with a suggested name of 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageNotFlagsNoZeroValue, "E");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageNotFlagsNoZeroValue, "E2");

            var code = @"
private enum E
{
    A = 1
}

private enum E2
{
    None = 1,
    A = 2
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
            VerifyCSharp(code,
                GetCSharpResultAt(2, 14, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetCSharpResultAt(7, 14, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2));
        }

        [Fact]
        public void CSharp_EnumsShouldZeroValueNotFlagsNoZeroValueWithScope()
        {
            // Add a member to {0} that has a value of zero with a suggested name of 'None'.
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageNotFlagsNoZeroValue, "E2");

            var code = @"
class C
{
    private enum E
    {
        A = 1
    }

    [|private enum E2
    {
        None = 1,
        A = 2
    }

    internal enum E3
    {
        None = 0,
        A = 1
    }|]

    internal enum E4
    {
        None = 0,
        A = 0
    }
}
";
            VerifyCSharp(code,
                GetCSharpResultAt(9, 18, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage));
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueFlagsRename()
        {
            // In enum '{0}', change the name of '{1}' to 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E", "A");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E2", "A2");
            string expectedMessage3 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E3", "A3");

            var code = @"
Public Class C
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
            VerifyBasic(code,
                GetBasicResultAt(5, 9, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetBasicResultAt(12, 5, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2),
                GetBasicResultAt(18, 5, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage3));
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueFlagsRenameScope()
        {
            // In enum '{0}', change the name of '{1}' to 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E2", "A2");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E3", "A3");

            var code = @"
<System.Flags>
Private Enum E
	A = 0
	B = 1
End Enum

[|<System.Flags>
Public Enum E2
	A2 = 0
	B2 = 1
End Enum

<System.Flags>
Public Enum E3
	A3 = CUShort(0)
	B3 = CUShort(1)
End Enum|]

<System.Flags>
Public Enum NoZeroValuedField
    A5 = 1
    B5 = 2
End Enum
";
            VerifyBasic(code,
                GetBasicResultAt(10, 2, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetBasicResultAt(16, 2, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2));
        }

        [WorkItem(836193, "DevDiv")]
        [Fact]
        public void VisualBasic_EnumsShouldZeroValueFlagsRename_AttributeListHasTrivia()
        {
            // In enum '{0}', change the name of '{1}' to 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E", "A");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E2", "A2");
            string expectedMessage3 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsRename, "E3", "A3");

            var code = @"
<System.Flags> _
Private Enum E
	A = 0
	B = 1
End Enum

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
            VerifyBasic(code,
                GetBasicResultAt(4, 2, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetBasicResultAt(10, 2, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2),
                GetBasicResultAt(16, 2, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage3));
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueFlagsMultipleZero()
        {
            // Remove all members that have the value zero from {0} except for one member that is named 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsMultipleZeros, "E");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsMultipleZeros, "E2");
            string expectedMessage3 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageFlagsMultipleZeros, "E3");

            var code = @"
<System.Flags>
Private Enum E
	None = 0
	A = 0
End Enum

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

            VerifyBasic(code,
                GetBasicResultAt(3, 14, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetBasicResultAt(9, 13, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2),
                GetBasicResultAt(15, 13, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage3));
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueNotFlagsNoZeroValue()
        {
            // Add a member to {0} that has a value of zero with a suggested name of 'None'.
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageNotFlagsNoZeroValue, "E");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageNotFlagsNoZeroValue, "E2");

            var code = @"
Private Enum E
	A = 1
End Enum

Private Enum E2
	None = 1
	A = 2
End Enum

Friend Enum E3
    None = 0
    A = 1
End Enum

Friend Enum E4
    None = 0
    A = 0
End Enum
";

            VerifyBasic(code,
                GetBasicResultAt(2, 14, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage1),
                GetBasicResultAt(6, 14, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage2));
        }

        [Fact]
        public void VisualBasic_EnumsShouldZeroValueNotFlagsNoZeroValueWithScope()
        {
            // Add a member to {0} that has a value of zero with a suggested name of 'None'.
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumsShouldHaveZeroValueMessageNotFlagsNoZeroValue, "E2");

            var code = @"
Private Enum E
	A = 1
End Enum

[|Private Enum E2
	None = 1
	A = 2
End Enum

Friend Enum E3
    None = 0
    A = 1
End Enum|]

Friend Enum E4
    None = 0
    A = 0
End Enum
";

            VerifyBasic(code,
                GetBasicResultAt(6, 14, EnumsShouldHaveZeroValueAnalyzer.RuleId, expectedMessage));
        }
    }
}
