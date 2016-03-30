// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OperatorOverloadsHaveNamedAlternatesTests : DiagnosticAnalyzerTestBase
    {
        #region Boilerplate

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new OperatorOverloadsHaveNamedAlternatesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OperatorOverloadsHaveNamedAlternatesAnalyzer();
        }

        private static DiagnosticResult GetCA2225CSharpDefaultResultAt(int line, int column, string alternateName, string operatorName)
        {
            // Provide a method named '{0}' as a friendly alternate for operator {1}.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesMessageDefault, alternateName, operatorName);
            return GetCSharpResultAt(line, column, OperatorOverloadsHaveNamedAlternatesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA2225CSharpPropertyResultAt(int line, int column, string alternateName, string operatorName)
        {
            // Provide a property named '{0}' as a friendly alternate for operator {1}.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesMessageProperty, alternateName, operatorName);
            return GetCSharpResultAt(line, column, OperatorOverloadsHaveNamedAlternatesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA2225CSharpMultipleResultAt(int line, int column, string alternateName1, string alternateName2, string operatorName)
        {
            // Provide a method named '{0}' or '{1}' as an alternate for operator {2}
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesMessageMultiple, alternateName1, alternateName2, operatorName);
            return GetCSharpResultAt(line, column, OperatorOverloadsHaveNamedAlternatesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA2225CSharpVisibilityResultAt(int line, int column, string alternateName, string operatorName)
        {
            // Mark {0} as public because it is a friendly alternate for operator {1}.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesMessageVisibility, alternateName, operatorName);
            return GetCSharpResultAt(line, column, OperatorOverloadsHaveNamedAlternatesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA2225BasicDefaultResultAt(int line, int column, string alternateName, string operatorName)
        {
            // Provide a method named '{0}' as a friendly alternate for operator {1}.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OperatorOverloadsHaveNamedAlternatesMessageDefault, alternateName, operatorName);
            return GetBasicResultAt(line, column, OperatorOverloadsHaveNamedAlternatesAnalyzer.RuleId, message);
        }

        #endregion

        #region C# tests

        [Fact]
        public void HasAlternateMethod_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static C operator +(C left, C right) { return new C(); }
    public static C Add(C left, C right) { return new C(); }
}
");
        }

        [Fact]
        public void HasMultipleAlternatePrimary_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static C operator %(C left, C right) { return new C(); }
    public static C Mod(C left, C right) { return new C(); }
}
");
        }

        [Fact]
        public void HasMultipleAlternateSecondary_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static C operator %(C left, C right) { return new C(); }
    public static C Remainder(C left, C right) { return new C(); }
}
");
        }

        [Fact]
        public void HasAppropriateConversionAlternate_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static implicit operator int(C item) { return 0; }
    public int ToInt32() { return 0; }
}
");
        }

        [Fact]
        public void MissingAlternateMethod_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static C operator +(C left, C right) { return new C(); }
}
",
            GetCA2225CSharpDefaultResultAt(4, 30, "Add", "op_Addition"));
        }

        [Fact]
        public void MissingAlternateProperty_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static bool operator true(C item) { return true; }
    public static bool operator false(C item) { return false; }
}
",
            GetCA2225CSharpPropertyResultAt(4, 33, "IsTrue", "op_True"));
        }

        [Fact]
        public void MissingMultipleAlternates_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static C operator %(C left, C right) { return new C(); }
}
",
            GetCA2225CSharpMultipleResultAt(4, 30, "Mod", "Remainder", "op_Modulus"));
        }

        [Fact]
        public void ImproperAlternateMethodVisibility_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static C operator +(C left, C right) { return new C(); }
    protected static C Add(C left, C right) { return new C(); }
}
",
                GetCA2225CSharpVisibilityResultAt(5, 24, "Add", "op_Addition"));
        }

        [Fact]
        public void ImproperAlternatePropertyVisibility_CSharp()
        {
            VerifyCSharp(@"
class C
{
    public static bool operator true(C item) { return true; }
    public static bool operator false(C item) { return false; }
    private bool IsTrue => true;
}
",
            GetCA2225CSharpVisibilityResultAt(6, 18, "IsTrue", "op_True"));
        }

        [Fact]
        public void StructHasAlternateMethod_CSharp()
        {
            VerifyCSharp(@"
struct C
{
    public static C operator +(C left, C right) { return new C(); }
    public static C Add(C left, C right) { return new C(); }
}
");
        }

        #endregion

        //
        // Since the analyzer is symbol-based, only a few VB tests are added as a sanity check
        //

        #region VB tests

        [Fact]
        public void HasAlternateMethod_VisualBasic()
        {
            VerifyBasic(@"
Class C
    Public Shared Operator +(left As C, right As C) As C
        Return New C()
    End Operator
    Public Shared Function Add(left As C, right As C) As C
        Return New C()
    End Function
End Class
");
        }

        [Fact]
        public void MissingAlternateMethod_VisualBasic()
        {
            VerifyBasic(@"
Class C
    Public Shared Operator +(left As C, right As C) As C
        Return New C()
    End Operator
End Class
",
            GetCA2225BasicDefaultResultAt(3, 28, "Add", "op_Addition"));
        }

        [Fact]
        public void StructHasAlternateMethod_VisualBasic()
        {
            VerifyBasic(@"
Structure C
    Public Shared Operator +(left As C, right As C) As C
        Return New C()
    End Operator
    Public Shared Function Add(left As C, right As C) As C
        Return New C()
    End Function
End Structure
");
        }

        #endregion
    }
}