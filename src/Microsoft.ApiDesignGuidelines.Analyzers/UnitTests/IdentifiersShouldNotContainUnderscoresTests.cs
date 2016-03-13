// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class IdentifiersShouldNotContainUnderscoresTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicIdentifiersShouldNotContainUnderscoresAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpIdentifiersShouldNotContainUnderscoresAnalyzer();
        }

        [Fact]
        public void CSharp_CA1707_ForAssembly()
        {
            VerifyCSharp(@"
public class DoesNotMatter
{
}
",
            testProjectName: "AssemblyNameHasUnderScore_",
            expected: GetCA1707CSharpResultAt(line: 2, column: 1, symbolKind: SymbolKind.Assembly, identifierName: "AssemblyNameHasUnderScore_"));
        }

        [Fact]
        public void CSharp_CA1707_ForAssembly_NoDiagnostics()
        {
            VerifyCSharp(@"
public class DoesNotMatter
{
}
",
            testProjectName: "AssemblyNameHasNoUnderScore");
        }

        [Fact]
        public void CSharp_CA1707_ForNamespace()
        {
            VerifyCSharp(@"
namespace HasUnderScore_
{
    public class DoesNotMatter
    {
    }
}

namespace HasNoUnderScore
{
    public class DoesNotMatter
    {
    }
}",
            GetCA1707CSharpResultAt(line: 2, column: 11, symbolKind: SymbolKind.Namespace, identifierName: "HasUnderScore_"));
        }

        [Fact]
        public void CSharp_CA1707_ForTypes()
        {
            VerifyCSharp(@"
public class UnderScoreInName_
{
}

private class UnderScoreInNameButPrivate_
{
}
",
            GetCA1707CSharpResultAt(line: 2, column: 14, symbolKind: SymbolKind.NamedType, identifierName: "UnderScoreInName_"));
        }

        [Fact]
        public void CSharp_CA1707_ForFields()
        {
            VerifyCSharp(@"
public class DoesNotMatter
{
        public const int ConstField_ = 5;
        public static readonly int StaticReadOnlyField_ = 5;

        // No diagnostics for the below
        private string InstanceField_;
        private static string StaticField_;
        public string _field;
        protected string Another_field;
}

public enum DoesNotMatterEnum
{
    _EnumWithUnderscore,
    _
}",
            GetCA1707CSharpResultAt(line: 4, column: 26, symbolKind: SymbolKind.Member, identifierName: "ConstField_"),
            GetCA1707CSharpResultAt(line: 5, column: 36, symbolKind: SymbolKind.Member, identifierName: "StaticReadOnlyField_"),
            GetCA1707CSharpResultAt(line: 16, column: 5, symbolKind: SymbolKind.Member, identifierName: "_EnumWithUnderscore"),
            GetCA1707CSharpResultAt(line: 17, column: 5, symbolKind: SymbolKind.Member, identifierName: "_"));
        }

        [Fact]
        public void CSharp_CA1707_ForMethods()
        {
            VerifyCSharp(@"
public class DoesNotMatter
{
    public void PublicM1_() { }
    private void PrivateM2_() { } // No diagnostic
    internal void InternalM3_() { } // No diagnostic
    protected void ProtectedM4_() { }
}

public interface I1
{
    void M_();
}

public class ImplementI1 : I1
{
    public void M_() { } // No diagnostic
    public virtual void M2_() { }
}

public class Derives : ImplementI1
{
    public override void M2_() { } // No diagnostic
}


",
            GetCA1707CSharpResultAt(line: 4, column: 17, symbolKind: SymbolKind.Member, identifierName: "PublicM1_"),
            GetCA1707CSharpResultAt(line: 7, column: 20, symbolKind: SymbolKind.Member, identifierName: "ProtectedM4_"),
            GetCA1707CSharpResultAt(line: 12, column: 10, symbolKind: SymbolKind.Member, identifierName: "M_"),
            GetCA1707CSharpResultAt(line: 18, column: 25, symbolKind: SymbolKind.Member, identifierName: "M2_"));
        }

        [Fact]
        public void CSharp_CA1707_ForProperties()
        {
            VerifyCSharp(@"
public class DoesNotMatter
{
    public event EventHandler PublicE1_;
    private event EventHandler PrivateE2_; // No diagnostic
    internal event EventHandler InternalE3_; // No diagnostic
    protected event EventHandler ProtectedE4_;
}

public interface I1
{
    event EventHandler E_;
}

public class ImplementI1 : I1
{
    public event EventHandler E_;// No diagnostic
    public virtual event EventHandler E2_;
}

public class Derives : ImplementI1
{
    public override event EventHandler E2_;
}",
            GetCA1707CSharpResultAt(line: 4, column: 31, symbolKind: SymbolKind.Member, identifierName: "PublicE1_"),
            GetCA1707CSharpResultAt(line: 7, column: 34, symbolKind: SymbolKind.Member, identifierName: "ProtectedE4_"),
            GetCA1707CSharpResultAt(line: 12, column: 24, symbolKind: SymbolKind.Member, identifierName: "E_"),
            GetCA1707CSharpResultAt(line: 18, column: 39, symbolKind: SymbolKind.Member, identifierName: "E2_"));
        }

        #region Helpers

        private static DiagnosticResult GetCA1707CSharpResultAt(int line, int column, SymbolKind symbolKind, string identifierName)
        {
            return GetCA1707CSharpResultAt(line, column, GetApproriateMessage(symbolKind), identifierName);
        }

        private static DiagnosticResult GetCA1707CSharpResultAt(int line, int column, string message, string identifierName)
        {
            return GetCSharpResultAt(line, column, IdentifiersShouldNotContainUnderscoresAnalyzer.RuleId, string.Format(message, identifierName));
        }

        private void VerifyCSharp(string source, string testProjectName, params DiagnosticResult[] expected)
        {
            Verify(source, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), testProjectName, expected);
        }

        private static DiagnosticResult GetCA1707BasicResultAt(int line, int column, string identifierName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesMessage, identifierName);
            return GetBasicResultAt(line, column, IdentifiersShouldNotContainTypeNames.RuleId, message);
        }

        private void VerifyBasic(string source, string testProjectName, params DiagnosticResult[] expected)
        {
            Verify(source, LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), testProjectName, expected);
        }

        private void Verify(string source, string language, DiagnosticAnalyzer analyzer, string testProjectName, DiagnosticResult[] expected)
        {
            var sources = new[] { source };
            var diagnostics = GetSortedDiagnostics(sources.ToFileAndSource(), language, analyzer, true, testProjectName);
            diagnostics.Verify(analyzer, expected);
        }

        private static string GetApproriateMessage(SymbolKind symbolKind)
        {
            switch (symbolKind)
            {
                case SymbolKind.Assembly:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageAssembly;
                case SymbolKind.Namespace:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageNamespace;
                case SymbolKind.NamedType:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageType;
                case SymbolKind.Member:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageMember;
                default:
                    throw new System.Exception("Unknown Symbol Kind");
            }
        }

        private enum SymbolKind
        {
            Assembly,
            Namespace,
            NamedType,
            Member
        }
        #endregion
    }
}