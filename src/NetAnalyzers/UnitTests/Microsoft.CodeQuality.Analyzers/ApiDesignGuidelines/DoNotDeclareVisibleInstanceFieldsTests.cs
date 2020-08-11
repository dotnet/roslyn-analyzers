// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDeclareVisibleInstanceFieldsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDeclareVisibleInstanceFieldsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotDeclareVisibleInstanceFieldsTests
    {
        [Fact]
        public async Task CSharp_PublicVariable_PublicContainingType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public string field; 
}", GetCSharpResultAt(4, 19));
        }

        [Fact]
        public async Task VisualBasic_PublicVariable_PublicContainingType()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Public field As System.String
End Class", GetBasicResultAt(3, 12));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task CSharp_PublicVariable_InternalContainingType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal class A
{
    public string field; 

    public class B
    {
        public string field; 
    }
}");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task VisualBasic_PublicVariable_InternalContainingType()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Friend Class A
    Public field As System.String

    Public Class B
        Public field As System.String
    End Class
End Class
");
        }

        [Fact]
        public async Task CSharp_DefaultVisibility()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    string field; 
}");
        }

        [Fact]
        public async Task VisualBasic_DefaultVisibility()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Dim field As System.String
End Class");
        }

        [Fact]
        public async Task CSharp_PublicStaticVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public static string field; 
}");
        }

        [Fact]
        public async Task VisualBasic_PublicStaticVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Public Shared field as System.String
End Class");
        }

        [Fact]
        public async Task CSharp_PublicStaticReadonlyVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public static readonly string field; 
}");
        }

        [Fact]
        public async Task VisualBasic_PublicStaticReadonlyVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Public Shared ReadOnly field as System.String
End Class");
        }

        [Fact]
        public async Task CSharp_PublicConstVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public const string field = ""X"";
}");
        }

        [Fact]
        public async Task VisualBasic_PublicConstVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Public Const field as System.String = ""X""
End Class");
        }

        [Fact]
        public async Task CSharp_ProtectedVariable_PublicContainingType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    protected string field;
}", GetCSharpResultAt(4, 22));
        }

        [Fact]
        public async Task VisualBasic_ProtectedVariable_PublicContainingType()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Protected field As System.String
End Class", GetBasicResultAt(3, 15));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task CSharp_ProtectedVariable_InternalContainingType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        internal class A
        {
            protected string field; 

            public class B
            {
                protected string field; 
            }
        }");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task VisualBasic_ProtectedVariable_InternalContainingType()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Friend Class A
            Protected field As System.String

            Public Class B
                Protected field As System.String
            End Class
        End Class
        ");
        }

        [Fact]
        public async Task CSharp_ProtectedStaticVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        public class A
        {
            protected static string field; 
        }");
        }

        [Fact]
        public async Task VisualBasic_ProtectedStaticVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Public Class A
            Protected Shared field as System.String
        End Class");
        }

        [Fact]
        public async Task CSharp_ProtectedStaticReadonlyVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        public class A
        {
            protected static readonly string field; 
        }");
        }

        [Fact]
        public async Task VisualBasic_ProtectedStaticReadonlyVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Public Class A
            Protected Shared ReadOnly field as System.String
        End Class");
        }

        [Fact]
        public async Task CSharp_ProtectedConstVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        public class A
        {
            protected const string field = ""X"";
        }");
        }

        [Fact]
        public async Task VisualBasic_ProtectedConstVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Public Class A
            Protected Const field as System.String = ""X""
        End Class");
        }

        [Fact]
        public async Task CSharp_ProtectedInternalVariable_PublicContainingType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    protected internal string field;
}", GetCSharpResultAt(4, 31));
        }

        [Fact]
        public async Task VisualBasic_ProtectedFriendVariable_PublicContainingType()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Protected Friend field As System.String
End Class", GetBasicResultAt(3, 22));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task CSharp_ProtectedInternalVariable_InternalContainingType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        internal class A
        {
            protected internal string field; 

            public class B
            {
                protected internal string field; 
            }
        }");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task VisualBasic_ProtectedFriendVariable_InternalContainingType()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Friend Class A
            Protected Friend field As System.String

            Public Class B
                Protected Friend field As System.String
            End Class
        End Class
        ");
        }

        [Fact]
        public async Task CSharp_ProtectedInternalStaticVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        public class A
        {
            protected internal static string field; 
        }");
        }

        [Fact]
        public async Task VisualBasic_ProtectedFriendStaticVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Public Class A
            Protected Friend Shared field as System.String
        End Class");
        }

        [Fact]
        public async Task CSharp_ProtectedInternalStaticReadonlyVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        public class A
        {
            protected internal static readonly string field; 
        }");
        }

        [Fact]
        public async Task VisualBasic_ProtectedFriendStaticReadonlyVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Public Class A
            Protected Friend Shared ReadOnly field as System.String
        End Class");
        }

        [Fact]
        public async Task CSharp_ProtectedInternalConstVariable()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
        public class A
        {
            protected internal const string field = ""X"";
        }");
        }

        [Fact]
        public async Task VisualBasic_ProtectedFriendConstVariable()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
        Public Class A
            Protected Friend Const field as System.String = ""X""
        End Class");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
            => VerifyCS.Diagnostic().WithLocation(line, column);

        private static DiagnosticResult GetBasicResultAt(int line, int column)
            => VerifyVB.Diagnostic().WithLocation(line, column);
    }
}