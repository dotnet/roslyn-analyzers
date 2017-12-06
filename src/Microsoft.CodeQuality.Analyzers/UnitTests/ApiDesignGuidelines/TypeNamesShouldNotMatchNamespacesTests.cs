// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class TypeNamesShouldNotMatchNamespacesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new TypeNamesShouldNotMatchNamespacesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TypeNamesShouldNotMatchNamespacesAnalyzer();
        }

        private static DiagnosticResult CSharpDefaultResultAt(int line, int column, string typeName, string namespaceName)
        {
            return GetCSharpResultAt(line, column, TypeNamesShouldNotMatchNamespacesAnalyzer.DefaultRule, typeName, namespaceName);
        }

        private static DiagnosticResult CSharpSystemResultAt(int line, int column, string typeName, string namespaceName)
        {
            return GetCSharpResultAt(line, column, TypeNamesShouldNotMatchNamespacesAnalyzer.SystemRule, typeName, namespaceName);
        }

        private static DiagnosticResult BasicDefaultResultAt(int line, int column, string typeName, string namespaceName)
        {
            return GetBasicResultAt(line, column, TypeNamesShouldNotMatchNamespacesAnalyzer.DefaultRule, typeName, namespaceName);
        }

        private static DiagnosticResult BasicSystemResultAt(int line, int column, string typeName, string namespaceName)
        {
            return GetBasicResultAt(line, column, TypeNamesShouldNotMatchNamespacesAnalyzer.SystemRule, typeName, namespaceName);
        }

        [Fact]
        public void CA1724CSharpValidName()
        {
            VerifyCSharp(@"
public class C
{
}");
        }

        [Fact]
        public void CA1724CSharpInvalidNameMatchingFormsNamespaceInSystemRule()
        {
            VerifyCSharp(@"
public class Forms
{
}",
        CSharpSystemResultAt(2, 14, "Forms", "System.Windows.Forms"));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void CA1724CSharpInvalidNameMatchingFormsNamespaceInSystemRule_Internal_NoDiagnostic()
        {
            VerifyCSharp(@"
internal class Forms
{
}

public class Outer
{
    private class Forms
    {
    }
}

internal class Outer2
{
    public class Forms
    {
    }
}
");
        }

        [Fact]
        public void CA1724CSharpInvalidNameMatchingSdkNamespaceInDefaultRule()
        {
            var source = @"
public class Sdk
{
}
";
            Document document = CreateDocument(source, LanguageNames.CSharp);
            Project project = document.Project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(Xunit.Sdk.AllException).Assembly.Location));
            DiagnosticAnalyzer analyzer = GetCSharpDiagnosticAnalyzer();
            GetSortedDiagnostics(analyzer, project.Documents.Single())
                .Verify(analyzer,
                        CSharpDefaultResultAt(2, 14, "Sdk", "Xunit.Sdk"));
        }

        [Fact]
        public void CA1724CSharpDeterministicDiagnosticOnA()
        {
            VerifyCSharp(@"
namespace A.B
{
}

namespace D
{
    public class A {}
}",
            CSharpDefaultResultAt(8, 18, "A", "A"));
        }

        [Fact]
        public void CA1724VisualBasicValidName()
        {
            VerifyBasic(@"
Public Class C
End Class");
        }

        [Fact]
        public void CA1724VisualBasicInvalidNameMatchingFormsNamespaceInSystemRule()
        {
            VerifyBasic(@"
Public Class Forms
End Class",
        BasicSystemResultAt(2, 14, "Forms", "System.Windows.Forms"));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void CA1724VisualBasicInvalidNameMatchingFormsNamespaceInSystemRule_Internal_NoDiagnostic()
        {
            VerifyBasic(@"
Friend Class Forms
End Class

Public Class Outer
    Private Class Forms
    End Class
End Class

Friend Class Outer2
    Public Class Forms
    End Class
End Class
");
        }

        [Fact]
        public void CA1724VisualBasicInvalidNameMatchingSdkNamespaceInDefaultRule()
        {
            var source = @"
Public Class Sdk
End Class";
            Document document = CreateDocument(source, LanguageNames.VisualBasic);
            Project project = document.Project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(Xunit.Sdk.AllException).Assembly.Location));
            DiagnosticAnalyzer analyzer = GetCSharpDiagnosticAnalyzer();
            GetSortedDiagnostics(analyzer, project.Documents.Single())
                .Verify(analyzer,
                        BasicDefaultResultAt(2, 14, "Sdk", "Xunit.Sdk"));
        }
    }
}
