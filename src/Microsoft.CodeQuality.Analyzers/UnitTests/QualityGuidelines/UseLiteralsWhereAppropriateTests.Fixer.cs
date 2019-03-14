// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public class UseLiteralsWhereAppropriateFixerTests : CodeFixTestBase
    {
        [Fact]
        public void CSharp_CodeFixForEmptyString()
        {
            VerifyCSharpFix(@"
class C
{
    public /*leading*/ static /*intermediate*/ readonly /*trailing*/ string f1 = """";
}
",
                @"
class C
{
    public /*leading*/ const /*intermediate*/  /*trailing*/ string f1 = """";
}
");

            VerifyBasicFix(@"
Class C
    Public Shared ReadOnly f1 As String = """"
End Class
",
@"
Class C
    Public Const f1 As String = """"
End Class
");
        }

        [Fact]
        public void CSharp_CodeFixForNonEmptyString()
        {
            VerifyCSharpFix(@"
class C
{
    /*leading*/
    readonly /*intermediate*/ static /*trailing*/ string f1 = ""Nothing"";
}
",
                @"
class C
{
    /*leading*/
    const /*intermediate*/  /*trailing*/ string f1 = ""Nothing"";
}
");

            VerifyBasicFix(@"
Class C
    'leading
    ReadOnly Shared f1 As String = ""Nothing""
End Class
",
@"
Class C
    'leading
    Const f1 As String = ""Nothing""
End Class
");
        }

        [Fact]
        public void CSharp_CodeFixForMultiDeclaration()
        {
            // Fixers are disabled on multiple fields, because it may introduce compile error.

            VerifyCSharpFix(@"
class C
{
    /*leading*/
    readonly /*intermediate*/ static /*trailing*/ string f3, f4 = ""Message is shown only for f4"";
}
",
                @"
class C
{
    /*leading*/
    readonly /*intermediate*/ static /*trailing*/ string f3, f4 = ""Message is shown only for f4"";
}
");
            VerifyBasicFix(@"
Class C
    Shared ReadOnly f3 As String, f4 As String = ""Message is shown only for f4""
End Class
",
@"
Class C
    Shared ReadOnly f3 As String, f4 As String = ""Message is shown only for f4""
End Class
");
        }

        [Fact]
        public void CSharp_CodeFixForInt32()
        {
            VerifyCSharpFix(@"
class C
{
    const int f6 = 3;
    static readonly int f7 = 8 + f6;
}
",
                @"
class C
{
    const int f6 = 3;
    const int f7 = 8 + f6;
}
");

            VerifyBasicFix(@"
Class C
    Const f6 As Integer = 3
    Friend Shared ReadOnly f7 As Integer = 8 + f6
End Class
",
@"
Class C
    Const f6 As Integer = 3
    Friend Const f7 As Integer = 8 + f6
End Class
");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseLiteralsWhereAppropriateAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseLiteralsWhereAppropriateAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicUseLiteralsWhereAppropriateFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpUseLiteralsWhereAppropriateFixer();
        }
    }
}