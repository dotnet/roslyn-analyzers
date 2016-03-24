// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class NormalizeStringsToUppercaseTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new NormalizeStringsToUppercaseAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NormalizeStringsToUppercaseAnalyzer();
        }

        #region No Diagnostic Tests

        [Fact]
        public void NoDiagnostic_ToUpperCases()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

public class UnexpectedWarningAttribute : Attribute
{
    public UnexpectedWarningAttribute(string ruleName, string ruleCategory) { }
}

public class NormalizeStringsTesterClass
{
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationOneA()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.InvariantCulture));
    }
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationOneB()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.CurrentCulture));
    }
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationOneC()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.CurrentUICulture));
    }
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationOneD()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.InstalledUICulture));
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Globalization

Public Class UnexpectedWarningAttribute
    Inherits Attribute
    Public Sub New(ruleName As String, ruleCategory As String)
    End Sub
End Class

Public Class NormalizeStringsTesterClass
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationOneA()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.InvariantCulture))
    End Sub
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationOneB()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.CurrentCulture))
    End Sub
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationOneC()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.CurrentUICulture))
    End Sub
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationOneD()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToUpper(CultureInfo.InstalledUICulture))
    End Sub
End Class
");
        }

        [Fact]
        public void NoDiagnostic_ToLowerCases()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

public class UnexpectedWarningAttribute : Attribute
{
    public UnexpectedWarningAttribute(string ruleName, string ruleCategory) { }
}

public class NormalizeStringsTesterClass
{
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationTwoA()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToLower());
    }
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationTwoB()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToLower(CultureInfo.CurrentCulture));
    }
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationTwoC()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToLower(CultureInfo.CurrentUICulture));
    }
    [UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
    public void BadNormalizationTwoD()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToLower(CultureInfo.InstalledUICulture));
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Globalization

Public Class UnexpectedWarningAttribute
    Inherits Attribute
    Public Sub New(ruleName As String, ruleCategory As String)
    End Sub
End Class

Public Class NormalizeStringsTesterClass
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationTwoB()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToLower())
    End Sub
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationTwoB()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToLower(CultureInfo.CurrentCulture))
    End Sub
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationTwoC()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToLower(CultureInfo.CurrentUICulture))
    End Sub
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationTwoD()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToLower(CultureInfo.InstalledUICulture))
    End Sub
End Class
");
        }

        [Fact]
        public void NoDiagnostic_ToUpperInvariantCases()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

public class UnexpectedWarningAttribute : Attribute
{
    public UnexpectedWarningAttribute(string ruleName, string ruleCategory) { }
}

public class NormalizeStringsTesterClass
{
    public void BadNormalizationThree()
    {
        Console.WriteLine(this);
        Console.WriteLine(""FOO"".ToUpperInvariant());
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Globalization

Public Class UnexpectedWarningAttribute
    Inherits Attribute
    Public Sub New(ruleName As String, ruleCategory As String)
    End Sub
End Class

Public Class NormalizeStringsTesterClass
    <UnexpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
    Public Sub BadNormalizationThree()
        Console.WriteLine(Me)
        Console.WriteLine(""FOO"".ToUpperInvariant())
    End Sub
End Class
");
        }

        #endregion

        #region Diagnostic Tests

        [Fact]
        public void Diagnostic_ToLowerCases()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

public class ExpectedWarningAttribute : Attribute
{
    public ExpectedWarningAttribute(string ruleName, string ruleCategory) { }
}

namespace Microsoft.FxCop.Tests.Globalization.Bad.CSharp.NormalizeStringsToUppercaseTests
{
    public class NormalizeStringsTesterClass
    {
        [ExpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
        public void BadNormalizationOne()
        {
            Console.WriteLine(this);
            Console.WriteLine(""FOO"".ToLower(CultureInfo.InvariantCulture));
        }
    }
}
",
            GetCSharpDefaultResultAt(18, 31, "BadNormalizationOne", "ToLower", "ToUpperInvariant"));

            VerifyBasic(@"
Imports System
Imports System.Globalization

Public Class ExpectedWarningAttribute
    Inherits Attribute
    Public Sub New(ruleName As String, ruleCategory As String)
    End Sub
End Class

Namespace Microsoft.FxCop.Tests.Globalization.Bad.CSharp.NormalizeStringsToUppercaseTests
    Public Class NormalizeStringsTesterClass
        <ExpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
        Public Sub BadNormalizationOne()
            Console.WriteLine(Me)
            Console.WriteLine(""FOO"".ToLower(CultureInfo.InvariantCulture))
        End Sub
    End Class
End Namespace
",
            GetBasicDefaultResultAt(16, 31, "BadNormalizationOne", "ToLower", "ToUpperInvariant"));
        }

        [Fact]
        public void Diagnostic_ToLowerInvariantCases()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

public class ExpectedWarningAttribute : Attribute
{
    public ExpectedWarningAttribute(string ruleName, string ruleCategory) { }
}

namespace Microsoft.FxCop.Tests.Globalization.Bad.CSharp.NormalizeStringsToUppercaseTests
{
    public class NormalizeStringsTesterClass
    {
        [ExpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")]
        public void BadNormalizationTwo()
        {
            Console.WriteLine(this);
            Console.WriteLine(""FOO"".ToLowerInvariant());
        }
    }
}
",
            GetCSharpDefaultResultAt(18, 31, "BadNormalizationTwo", "ToLowerInvariant", "ToUpperInvariant"));

            VerifyBasic(@"
Imports System
Imports System.Globalization

Public Class ExpectedWarningAttribute
    Inherits Attribute
    Public Sub New(ruleName As String, ruleCategory As String)
    End Sub
End Class

Namespace Microsoft.FxCop.Tests.Globalization.Bad.CSharp.NormalizeStringsToUppercaseTests
    Public Class NormalizeStringsTesterClass
        <ExpectedWarning(""NormalizeStringsToUppercase"", ""GlobalizationRules"")>
        Public Sub BadNormalizationTwo()
            Console.WriteLine(Me)
            Console.WriteLine(""FOO"".ToLowerInvariant())
        End Sub
    End Class
End Namespace
",
            GetBasicDefaultResultAt(16, 31, "BadNormalizationTwo", "ToLowerInvariant", "ToUpperInvariant"));
        }

        #endregion

        #region Helpers

        private static DiagnosticResult GetCSharpDefaultResultAt(int line, int column, string containingMethod, string invokedMethod, string suggestedMethod)
        {
            // In method '{0}', replace the call to '{1}' with '{2}'.
            string message = string.Format(NormalizeStringsToUppercaseAnalyzer.ToUpperRule.MessageFormat.ToString(CultureInfo.CurrentUICulture), containingMethod, invokedMethod, suggestedMethod);
            return GetCSharpResultAt(line, column, NormalizeStringsToUppercaseAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicDefaultResultAt(int line, int column, string containingMethod, string invokedMethod, string suggestedMethod)
        {
            // In method '{0}', replace the call to '{1}' with '{2}'.
            string message = string.Format(NormalizeStringsToUppercaseAnalyzer.ToUpperRule.MessageFormat.ToString(CultureInfo.CurrentUICulture), containingMethod, invokedMethod, suggestedMethod);
            return GetBasicResultAt(line, column, NormalizeStringsToUppercaseAnalyzer.RuleId, message);
        }

        #endregion
    }
}