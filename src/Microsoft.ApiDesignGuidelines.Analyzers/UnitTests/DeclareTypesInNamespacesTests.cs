// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DeclareTypesInNamespacesTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string s_ruleId = DeclareTypesInNamespacesAnalyzer.RuleId;
        private static readonly string s_message = MicrosoftApiDesignGuidelinesAnalyzersResources.DeclareTypesInNamespacesMessage;

        [Fact]
        public void OuterTypeInGlobalNamespace_Warns()
        {
            VerifyCSharp(@"
                public class Class
                {
                }",
                GetCSharpExpectedResult(2, 30));

            VerifyBasic(@"
                Public Class MyClass
                End Class",
                GetBasicExpectedResult(2, 30));
        }

        [Fact]
        public void NestedTypeInGlobalNamespace_WarnsOnlyOnce()
        {
            VerifyCSharp(@"
                public class Class
                {
                    public class Nested {}
                }",
                GetCSharpExpectedResult(2, 30));

            VerifyBasic(@"
                Public Class MyClass
                    Public Class Nested
                    End Class
                End Class",
                GetBasicExpectedResult(2, 30));
        }

        [Fact]
        public void InternalClassInGlobalNamespace_DoesNotWarn()
        {
            VerifyCSharp(@"
                internal class Class
                {
                    public class Nested {}
                }");

            VerifyBasic(@"
                Friend Class MyClass
                    Public Class Nested
                    End Class
                End Class");
        }

        [Fact]
        public void PublicClassInNonGlobalNamespace_DoesNotWarn()
        {
            VerifyCSharp(@"
                namespace NS
                {
                    public class Class
                    {
                        public class Nested {}
                    }
                }");

            VerifyBasic(@"
                Namespace NS
                    Public Class MyClass
                        Public Class Nested
                        End Class
                    End Class
                End Namespace");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DeclareTypesInNamespacesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DeclareTypesInNamespacesAnalyzer();
        }

        private static DiagnosticResult GetCSharpExpectedResult(int line, int column)
        {
            return GetCSharpResultAt(line, column, s_ruleId, s_message);
        }

        private static DiagnosticResult GetBasicExpectedResult(int line, int column)
        {
            return GetBasicResultAt(line, column, s_ruleId, s_message);
        }
    }
}