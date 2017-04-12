// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    /// <summary>
    /// Contains those unit tests for the IdentifiersShouldNotMatchKeywords analyzer that
    /// pertain to the NamespaceRule, which applies to the names of type namespaces.
    /// </summary>
    /// <remarks>
    /// FxCop does not report a violation unless the namespace contains a publicly visible
    /// class, and we follow that implementation.
    /// </remarks>
    public partial class IdentifiersShouldNotMatchKeywordsTests
    {
        [Fact]
        public void CSharpDiagnosticForKeywordNamedNamespaceContainingPublicClass()
        {
            VerifyCSharp(@"
namespace @namespace
{
    public class C {}
}
",
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "namespace", "namespace"));
        }

        [Fact]
        public void BasicDiagnosticForKeywordNamedNamespaceContainingPublicClass()
        {
            VerifyBasic(@"
Namespace [Namespace]
    Public Class C
    End Class
End Namespace
",
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "Namespace", "Namespace"));
        }

        [Fact]
        public void CSharpNoDiagnosticForNonKeywordNamedNamespaceContainingPublicClass()
        {
            VerifyCSharp(@"
namespace namespace2
{
    public class C {}
}
");
        }

        [Fact]
        public void BasicNoDiagnosticForNonKeywordNamedNamespaceContainingPublicClass()
        {
            VerifyBasic(@"
Namespace Namespace2
    Public Class C
    End Class
End Namespace
");
        }

        [Fact]
        public void CSharpNoDiagnosticForKeywordNamedNamespaceContainingInternalClass()
        {
            VerifyCSharp(@"
namespace @namespace
{
    internal class C {}
}
");
        }

        [Fact]
        public void BasicNoDiagnosticForKeywordNamedNamespaceContainingInternalClass()
        {
            VerifyBasic(@"
Namespace [Namespace]
    Friend Class C
    End Class
End Namespace
");
        }

        [Fact]
        public void CSharpDiagnosticForKeywordNamedMultiComponentNamespaceContainingPublicClass()
        {
            VerifyCSharp(@"
namespace N1.@namespace.N2.@for.N3
{
    public class C {}
}
",
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "N1.namespace.N2.for.N3", "namespace"),
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "N1.namespace.N2.for.N3", "for"));
        }

        [Fact]
        public void BasicDiagnosticForKeywordNamedMultiComponentNamespaceContainingPublicClass()
        {
            VerifyBasic(@"
Namespace N1.[Namespace].N2.[For].N3
    Public Class C
    End Class
End Namespace
",
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "N1.Namespace.N2.For.N3", "Namespace"),
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "N1.Namespace.N2.For.N3", "For"));
        }

        [Fact]
        public void CSharpNoDiagnosticForPublicClassInGlobalNamespace()
        {
            VerifyCSharp(@"
public class C {}
");
        }

        [Fact]
        public void BasicNoDiagnosticForPublicClassInGlobalNamespace()
        {
            VerifyBasic(@"
Public Class C
End Class
");
        }

        [Fact]
        public void CSharpNoDiagnosticForRepeatedOccurrencesOfSameKeywordNamedNamespace()
        {
            VerifyCSharp(@"
namespace @namespace
{
    public class C {}
}

namespace @namespace
{
    public class D {}
}",
                // Diagnostic for only one of the two occurrences.
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "namespace", "namespace"));
        }

        [Fact]
        public void BasicNoDiagnosticForRepeatedOccurrencesOfSameKeywordNamedNamespace()
        {
            VerifyBasic(@"
Namespace [Namespace]
    Public Class C
    End Class
End Namespace

Namespace [Namespace]
    Public Class D
    End Class
End Namespace
",
                // Diagnostic for only one of the two occurrences.
                GetResultNoLocation(IdentifiersShouldNotMatchKeywordsAnalyzer.NamespaceRule, "Namespace", "Namespace"));
        }

        private DiagnosticResult GetResultNoLocation(DiagnosticDescriptor rule, params object[] messageArguments)
        {
            return new DiagnosticResult
            {
                Locations = Array.Empty<DiagnosticResultLocation>(),
                Id = rule.Id,
                Severity = rule.DefaultSeverity,
                Message = string.Format(rule.MessageFormat.ToString(), messageArguments)
            };
        }
    }
}
