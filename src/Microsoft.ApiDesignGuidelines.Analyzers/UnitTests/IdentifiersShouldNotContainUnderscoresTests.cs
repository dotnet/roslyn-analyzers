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
            expected: GetCA1707CSharpResultAt(line: 2, column: 1, symbolKind: SymbolKind.Assembly, identifierNames: "AssemblyNameHasUnderScore_"));
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
            GetCA1707CSharpResultAt(line: 2, column: 11, symbolKind: SymbolKind.Namespace, identifierNames: "HasUnderScore_"));
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
            GetCA1707CSharpResultAt(line: 2, column: 14, symbolKind: SymbolKind.NamedType, identifierNames: "UnderScoreInName_"));
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
            GetCA1707CSharpResultAt(line: 4, column: 26, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatter.ConstField_"),
            GetCA1707CSharpResultAt(line: 5, column: 36, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatter.StaticReadOnlyField_"),
            GetCA1707CSharpResultAt(line: 16, column: 5, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatterEnum._EnumWithUnderscore"),
            GetCA1707CSharpResultAt(line: 17, column: 5, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatterEnum._"));
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
}",
            GetCA1707CSharpResultAt(line: 4, column: 17, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatter.PublicM1_()"),
            GetCA1707CSharpResultAt(line: 7, column: 20, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatter.ProtectedM4_()"),
            GetCA1707CSharpResultAt(line: 12, column: 10, symbolKind: SymbolKind.Member, identifierNames: "I1.M_()"),
            GetCA1707CSharpResultAt(line: 18, column: 25, symbolKind: SymbolKind.Member, identifierNames: "ImplementI1.M2_()"));
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
    public override event EventHandler E2_; // No diagnostic
}",
            GetCA1707CSharpResultAt(line: 4, column: 31, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatter.PublicE1_"),
            GetCA1707CSharpResultAt(line: 7, column: 34, symbolKind: SymbolKind.Member, identifierNames: "DoesNotMatter.ProtectedE4_"),
            GetCA1707CSharpResultAt(line: 12, column: 24, symbolKind: SymbolKind.Member, identifierNames: "I1.E_"),
            GetCA1707CSharpResultAt(line: 18, column: 39, symbolKind: SymbolKind.Member, identifierNames: "ImplementI1.E2_"));
        }

        [Fact]
        public void CSharp_CA1707_ForDelegates()
        {
            VerifyCSharp(@"
public delegate void Dele(int intPublic_, string stringPublic_);
internal delegate void Dele2(int intInternal_, string stringInternal_); // No diagnostics
public delegate T Del<T>(int t_);
",
            GetCA1707CSharpResultAt(2, 31, SymbolKind.DelegateParameter, "Dele", "intPublic_"),
            GetCA1707CSharpResultAt(2, 50, SymbolKind.DelegateParameter, "Dele", "stringPublic_"),
            GetCA1707CSharpResultAt(4, 30, SymbolKind.DelegateParameter, "Del<T>", "t_"));
        }

        [Fact]
        public void CSharp_CA1707_ForMemberparameters()
        {
            VerifyCSharp(@"
public class DoesNotMatter
{
    public void PublicM1(int int_) { }
    private void PrivateM2(int int_) { } // No diagnostic
    internal void InternalM3(int int_) { } // No diagnostic
    protected void ProtectedM4(int int_) { }
}

public interface I
{
    void M(int int_);
}

public class implementI : I
{
    public void M(int int_)
    {
    }
}

public abstract class Base
{
    public virtual void M1(int int_)
    {
    }

    public abstract void M2(int int_);
}

public class Der : Base
{
    public override void M2(int int_)
    {
        throw new NotImplementedException();
    }

    public override void M1(int int_)
    {
        base.M1(int_);
    }
}",
            GetCA1707CSharpResultAt(4, 30, SymbolKind.MemberParameter, "DoesNotMatter.PublicM1(int)", "int_"),
            GetCA1707CSharpResultAt(7, 36, SymbolKind.MemberParameter, "DoesNotMatter.ProtectedM4(int)", "int_"),
            GetCA1707CSharpResultAt(12, 16, SymbolKind.MemberParameter, "I.M(int)", "int_"),
            GetCA1707CSharpResultAt(24, 32, SymbolKind.MemberParameter, "Base.M1(int)", "int_"),
            GetCA1707CSharpResultAt(28, 33, SymbolKind.MemberParameter, "Base.M2(int)", "int_"));
        }

        [Fact]
        public void CSharp_CA1707_ForTypeTypeParameters()
        {
            VerifyCSharp(@"
public class DoesNotMatter<T_>
{
}

class NoDiag<U_>
{
}",
            GetCA1707CSharpResultAt(2, 28, SymbolKind.TypeTypeParameter, "DoesNotMatter<T_>", "T_"));
        }

        [Fact]
        public void CSharp_CA1707_ForMemberTypeParameters()
        {
            VerifyCSharp(@"
public class DoesNotMatter22
{
    public void PublicM1<T1_>() { }
    private void PrivateM2<U_>() { } // No diagnostic
    internal void InternalM3<W_>() { } // No diagnostic
    protected void ProtectedM4<D_>() { }
}

public interface I
{
    void M<T_>();
}

public class implementI : I
{
    public void M<U_>()
    {
        throw new NotImplementedException();
    }
}

public abstract class Base
{
    public virtual void M1<T_>()
    {
    }

    public abstract void M2<U_>();
}

public class Der : Base
{
    public override void M2<U_>()
    {
        throw new NotImplementedException();
    }

    public override void M1<T_>()
    {
        base.M1<T_>(int1);
    }
}",
            GetCA1707CSharpResultAt(4, 26, SymbolKind.MethodTypeParameter, "DoesNotMatter22.PublicM1<T1_>()", "T1_"),
            GetCA1707CSharpResultAt(7, 32, SymbolKind.MethodTypeParameter, "DoesNotMatter22.ProtectedM4<D_>()", "D_"),
            GetCA1707CSharpResultAt(12, 12, SymbolKind.MethodTypeParameter, "I.M<T_>()", "T_"),
            GetCA1707CSharpResultAt(25, 28, SymbolKind.MethodTypeParameter, "Base.M1<T_>()", "T_"),
            GetCA1707CSharpResultAt(29, 29, SymbolKind.MethodTypeParameter, "Base.M2<U_>()", "U_"));
        }
        #region Helpers

        private static DiagnosticResult GetCA1707CSharpResultAt(int line, int column, SymbolKind symbolKind, params string[] identifierNames)
        {
            return GetCA1707CSharpResultAt(line, column, GetApproriateMessage(symbolKind), identifierNames);
        }

        private static DiagnosticResult GetCA1707CSharpResultAt(int line, int column, string message, params string[] identifierName)
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
                case SymbolKind.DelegateParameter:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageDelegateParameter;
                case SymbolKind.MemberParameter:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageMemberParameter;
                case SymbolKind.TypeTypeParameter:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageTypeTypeParameter;
                case SymbolKind.MethodTypeParameter:
                    return MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageMethodTypeParameter;
                default:
                    throw new System.Exception("Unknown Symbol Kind");
            }
        }

        private enum SymbolKind
        {
            Assembly,
            Namespace,
            NamedType,
            Member,
            DelegateParameter,
            MemberParameter,
            TypeTypeParameter,
            MethodTypeParameter
        }
        #endregion
    }
}