//  Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<MetaCompilation.Analyzers.MetaCompilationAnalyzer, MetaCompilation.Analyzers.MetaCompilationCodeFixProvider>;

namespace MetaCompilation.Analyzers.UnitTests
{
    public class UnitTest
    {
        private const string s_messagePrefix = "T: ";

        #region default no diagnostics tests
        // no diagnostics
        [Fact]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        // no diagnostics
        [Fact]
        public async Task TestMethod2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            var ifKeyword = ifStatement.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();

                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var openParen = ifStatement.OpenParenToken;
            var startDiagnosticSpan = ifKeyword.SpanStart;
            var endDiagnosticSpan = openParen.SpanStart;
            var diagnosticSpan = TextSpan.FromBounds(startDiagnosticSpan, endDiagnosticSpan);
            var diagnosticLocation = Location.Create(ifStatement.SyntaxTree, diagnosticSpan);
            var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, Rule.MessageFormat);
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            var expected = new DiagnosticResult(MetaCompilationAnalyzer.GoToCodeFix, DiagnosticSeverity.Info)
                .WithMessage(s_messagePrefix + "Congratulations! You have written an analyzer! If you would like to explore a code fix for your diagnostic, open up CodeFixProvider.cs and take a look! To see your analyzer in action, press F5. A new instance of Visual Studio will open up, in which you can open a new C# console app and write test if-statements.")
                .WithLocation(15, 18);

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // no diagnostics
        [Fact]
        public async Task TestMethod3()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = context.Node as IfStatementSyntax;
            var ifKeyword = ifStatement.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();

                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var openParen = ifStatement.OpenParenToken;
            var startDiagnosticSpan = ifKeyword.SpanStart;
            var endDiagnosticSpan = openParen.SpanStart;
            var diagnosticSpan = TextSpan.FromBounds(startDiagnosticSpan, endDiagnosticSpan);
            var diagnosticLocation = Location.Create(ifStatement.SyntaxTree, diagnosticSpan);
            var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, Rule.MessageFormat);
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            var expected = new DiagnosticResult(MetaCompilationAnalyzer.GoToCodeFix, DiagnosticSeverity.Info)
                .WithMessage(s_messagePrefix + "Congratulations! You have written an analyzer! If you would like to explore a code fix for your diagnostic, open up CodeFixProvider.cs and take a look! To see your analyzer in action, press F5. A new instance of Visual Studio will open up, in which you can open a new C# console app and write test if-statements.")
                .WithLocation(15, 18);

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // no diagnostics
        [Fact]
        public async Task SyntaxKindCheckAlternate()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = context.Node as IfStatementSyntax;
            var ifKeyword = ifStatement.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();

                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var openParen = ifStatement.OpenParenToken;
            var startDiagnosticSpan = ifKeyword.SpanStart;
            var endDiagnosticSpan = openParen.SpanStart;
            var diagnosticSpan = TextSpan.FromBounds(startDiagnosticSpan, endDiagnosticSpan);
            var diagnosticLocation = Location.Create(ifStatement.SyntaxTree, diagnosticSpan);
            var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, Rule.MessageFormat);
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            var expected = new DiagnosticResult(MetaCompilationAnalyzer.GoToCodeFix, DiagnosticSeverity.Info)
                .WithMessage(s_messagePrefix + "Congratulations! You have written an analyzer! If you would like to explore a code fix for your diagnostic, open up CodeFixProvider.cs and take a look! To see your analyzer in action, press F5. A new instance of Visual Studio will open up, in which you can open a new C# console app and write test if-statements.")
                .WithLocation(15, 18);

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
        #endregion

        #region MissingId

        // no id, nothing else after
        [Fact]
        public async Task MissingId1()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        public const string spacingRuleId = ""IfSpacing001"";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(15,22): error MetaAnalyzer001: T: 'SyntaxNodeAnalyzer' should have a diagnostic id (a public, constant string uniquely identifying each diagnostic)
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingId).WithSpan(15, 22, 15, 40).WithArguments("SyntaxNodeAnalyzer"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(18,29): error MetaAnalyzer019: T: The analyzer should have at least one DiagnosticDescriptor rule
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRule).WithSpan(18, 29, 18, 42),
                    },
                },
                CodeActionEquivalenceKey = "Give the diagnostic a unique string ID distinguishing it from other diagnostics",
            }.RunAsync();
        }

        //  no id, rules exists
        [Fact]
        public async Task MissingId2()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
            internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                id: SpacingRuleId, // make the id specific
                title: ""If statement must have a space between the 'if' keyword and the boolean expression"", 
                messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
                category: ""Syntax"",
                defaultSeverity.Warning,
                isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        public const string spacingRuleId = ""IfSpacing001"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                id: SpacingRuleId, // make the id specific
                title: ""If statement must have a space between the 'if' keyword and the boolean expression"", 
                messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
                category: ""Syntax"",
                defaultSeverity.Warning,
                isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(15,22): error MetaAnalyzer001: T: 'SyntaxNodeAnalyzer' should have a diagnostic id (a public, constant string uniquely identifying each diagnostic)
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingId).WithSpan(15, 22, 15, 40).WithArguments("SyntaxNodeAnalyzer"),
                        // Test0.cs(18,21): error CS0103: The name 'SpacingRuleId' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(18, 21, 18, 34).WithArguments("SpacingRuleId"),
                        // Test0.cs(22,17): error CS0103: The name 'defaultSeverity' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(22, 17, 22, 32).WithArguments("defaultSeverity")
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(20,21): error CS0103: The name 'SpacingRuleId' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(20, 21, 20, 34).WithArguments("SpacingRuleId"),
                        // Test0.cs(20,21): error MetaAnalyzer015: T: This diagnostic id should be the constant string declared above
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingIdDeclaration).WithSpan(20, 21, 20, 34),
                        // Test0.cs(24,17): error CS0103: The name 'defaultSeverity' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(24, 17, 24, 32).WithArguments("defaultSeverity")
                    },
                },
                CodeActionEquivalenceKey = "Give the diagnostic a unique string ID distinguishing it from other diagnostics",
            }.RunAsync();
        }

        [Fact]
        public async Task MissingId3()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
            public string practice = ""IfSpacing"";
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        public const string spacingRuleId = ""IfSpacing001"";
        public string practice = ""IfSpacing"";
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(15,22): error MetaAnalyzer001: T: 'SyntaxNodeAnalyzer' should have a diagnostic id (a public, constant string uniquely identifying each diagnostic)
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingId).WithSpan(15, 22, 15, 40).WithArguments("SyntaxNodeAnalyzer"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(18,29): error MetaAnalyzer019: T: The analyzer should have at least one DiagnosticDescriptor rule
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRule).WithSpan(18, 29, 18, 42),
                    },
                },
                CodeActionEquivalenceKey = "Give the diagnostic a unique string ID distinguishing it from other diagnostics",
            }.RunAsync();
        }

        [Fact]
        public async Task MissingId4()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
            private const string practice = ""IfSpacing"";
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixTest = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        public const string spacingRuleId = ""IfSpacing001"";
        private const string practice = ""IfSpacing"";
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(15,22): error MetaAnalyzer001: T: 'SyntaxNodeAnalyzer' should have a diagnostic id (a public, constant string uniquely identifying each diagnostic)
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingId).WithSpan(15, 22, 15, 40).WithArguments("SyntaxNodeAnalyzer"),
                    },
                },
                FixedState =
                {
                    Sources = { fixTest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(18,29): error MetaAnalyzer019: T: The analyzer should have at least one DiagnosticDescriptor rule
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRule).WithSpan(18, 29, 18, 42),
                    },
                },
                CodeActionEquivalenceKey = "Give the diagnostic a unique string ID distinguishing it from other diagnostics",
            }.RunAsync();
        }

        [Fact]
        public async Task MissingId5()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
            string practice = ""IfSpacing"";
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        public const string spacingRuleId = ""IfSpacing001"";
        string practice = ""IfSpacing"";
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(15,22): error MetaAnalyzer001: T: 'SyntaxNodeAnalyzer' should have a diagnostic id (a public, constant string uniquely identifying each diagnostic)
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingId).WithSpan(15, 22, 15, 40).WithArguments("SyntaxNodeAnalyzer"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(18,29): error MetaAnalyzer019: T: The analyzer should have at least one DiagnosticDescriptor rule
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRule).WithSpan(18, 29, 18, 42),
                    },
                },
                CodeActionEquivalenceKey = "Give the diagnostic a unique string ID distinguishing it from other diagnostics",
            }.RunAsync();
        }

        [Fact]
        public async Task MissingId6()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
            public const int practice = 7;
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        public const string spacingRuleId = ""IfSpacing001"";
        public const int practice = 7;
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(15,22): error MetaAnalyzer001: T: 'SyntaxNodeAnalyzer' should have a diagnostic id (a public, constant string uniquely identifying each diagnostic)
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingId).WithSpan(15, 22, 15, 40).WithArguments("SyntaxNodeAnalyzer"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(18,29): error MetaAnalyzer019: T: The analyzer should have at least one DiagnosticDescriptor rule
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRule).WithSpan(18, 29, 18, 42),
                    },
                },
                CodeActionEquivalenceKey = "Give the diagnostic a unique string ID distinguishing it from other diagnostics",
            }.RunAsync();
        }
        #endregion

        #region MissingInit

        [Fact]
        public async Task MissingInit1()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }
        }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(16,22): error CS0534: 'SyntaxNodeAnalyzer' does not implement inherited abstract member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0534").WithSpan(16, 22, 16, 40).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(16,22): error MetaAnalyzer002: T: 'SyntaxNodeAnalyzer' is missing the required inherited Initialize method, needed to register analysis actions
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingInit).WithSpan(16, 22, 16, 40).WithArguments("SyntaxNodeAnalyzer"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(37,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(37, 13, 37, 49),
                    },
                },
                CodeActionEquivalenceKey = "Insert the missing Initialize method",
            }.RunAsync();
        }

        // slight misspelling
        [Fact]
        public async Task MissingInit2()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

       namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }
            public override void initialize(AnalysisContext context)
            {
                throw new NotImplementedException();
            }
        }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

       namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }
            public override void initialize(AnalysisContext context)
            {
                throw new NotImplementedException();
            }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(16,22): error CS0534: 'SyntaxNodeAnalyzer' does not implement inherited abstract member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0534").WithSpan(16, 22, 16, 40).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(16,22): error MetaAnalyzer002: T: 'SyntaxNodeAnalyzer' is missing the required inherited Initialize method, needed to register analysis actions
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingInit).WithSpan(16, 22, 16, 40).WithArguments("SyntaxNodeAnalyzer"),
                        // Test0.cs(34,34): error CS0115: 'SyntaxNodeAnalyzer.initialize(AnalysisContext)': no suitable method found to override
                        DiagnosticResult.CompilerError("CS0115").WithSpan(34, 34, 34, 44).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer.initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,34): error CS0115: 'SyntaxNodeAnalyzer.initialize(AnalysisContext)': no suitable method found to override
                        DiagnosticResult.CompilerError("CS0115").WithSpan(34, 34, 34, 44).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer.initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(41,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(41, 13, 41, 49),
                    },
                },
                CodeActionEquivalenceKey = "Insert the missing Initialize method",
            }.RunAsync();
        }

        //  everything except the initialize method
        [Fact]
        public async Task MissingInit3()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            var ifKeyword = ifStatement.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                var trailingTrivia = ifKeyword.TrailingTrivia.Last();

                if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                {
                    if (trailingTrivia.ToString() == "" "")
                    {
                        return;
                    }
                }
            }

            var openParen = ifStatement.OpenParenToken;
            var startDiagnosticSpan = ifKeyword.SpanStart;
            var endDiagnosticSpan = openParen.SpanStart;
            var diagnosticSpan = TextSpan.FromBounds(startDiagnosticSpan, endDiagnosticSpan);
            var diagnosticLocation = Location.Create(ifStatement.SyntaxTree, diagnosticSpan);
            var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, Rule.MessageFormat);
            context.ReportDiagnostic(diagnostic);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            var ifKeyword = ifStatement.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                var trailingTrivia = ifKeyword.TrailingTrivia.Last();

                if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                {
                    if (trailingTrivia.ToString() == "" "")
                    {
                        return;
                    }
                }
            }

            var openParen = ifStatement.OpenParenToken;
            var startDiagnosticSpan = ifKeyword.SpanStart;
            var endDiagnosticSpan = openParen.SpanStart;
            var diagnosticSpan = TextSpan.FromBounds(startDiagnosticSpan, endDiagnosticSpan);
            var diagnosticLocation = Location.Create(ifStatement.SyntaxTree, diagnosticSpan);
            var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, Rule.MessageFormat);
            context.ReportDiagnostic(diagnostic);
        }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(15,18): error CS0534: 'SyntaxNodeAnalyzer' does not implement inherited abstract member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0534").WithSpan(15, 18, 15, 36).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(15,18): error MetaAnalyzer002: T: 'SyntaxNodeAnalyzer' is missing the required inherited Initialize method, needed to register analysis actions
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingInit).WithSpan(15, 18, 15, 36).WithArguments("SyntaxNodeAnalyzer"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(64,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(64, 13, 64, 49),
                    },
                },
                CodeActionEquivalenceKey = "Insert the missing Initialize method",
            }.RunAsync();
        }
        #endregion

        #region MissingRegisterStatement

        [Fact]
        public async Task MissingRegister1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}";
            var fixTest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // Calls the method (first argument) to perform analysis whenever a SyntaxNode of kind IfStatement is found
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        // This is the method that is registered within Initialize and is called when an IfStatement SyntaxNode is found
        // First, this method analyzes the Syntax Tree. Then, it reports a diagnostic if an error is found
        // In this tutorial, this method will walk through the Syntax Tree seen in IfSyntaxTree.jpg and determine if the if-statement being analyzed has the correct spacing
        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer003: T: A syntax node action should be registered within the 'Initialize' method
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRegisterStatement).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixTest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(45,13): error MetaAnalyzer021: T: This statement should extract the if-statement being analyzed by casting context.Node to IfStatementSyntax
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IfStatementIncorrect).WithSpan(45, 13, 45, 49).WithArguments("context"),
                    },
                },
                CodeActionEquivalenceKey = "Register an action to analyze code when changes occur",
            }.RunAsync();
        }

        //  register statement in comments
        [Fact]
        public async Task MissingRegister2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            //  context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // Calls the method (first argument) to perform analysis whenever a SyntaxNode of kind IfStatement is found
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        // This is the method that is registered within Initialize and is called when an IfStatement SyntaxNode is found
        // First, this method analyzes the Syntax Tree. Then, it reports a diagnostic if an error is found
        // In this tutorial, this method will walk through the Syntax Tree seen in IfSyntaxTree.jpg and determine if the if-statement being analyzed has the correct spacing
        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer003: T: A syntax node action should be registered within the 'Initialize' method
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRegisterStatement).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(45,13): error MetaAnalyzer021: T: This statement should extract the if-statement being analyzed by casting context.Node to IfStatementSyntax
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IfStatementIncorrect).WithSpan(45, 13, 45, 49).WithArguments("context"),
                    },
                },
                CodeActionEquivalenceKey = "Register an action to analyze code when changes occur",
            }.RunAsync();
        }

        [Fact]
        public async Task MissingRegister3()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
        }
        private void Method2(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";
            var fixTest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // Calls the method (first argument) to perform analysis whenever a SyntaxNode of kind IfStatement is found
            context.RegisterSyntaxNodeAction(Method2, SyntaxKind.IfStatement);
        }

        private void Method2(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer003: T: A syntax node action should be registered within the 'Initialize' method
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRegisterStatement).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixTest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(42,13): error MetaAnalyzer021: T: This statement should extract the if-statement being analyzed by casting context.Node to IfStatementSyntax
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IfStatementIncorrect).WithSpan(42, 13, 42, 49).WithArguments("context"),
                    },
                },
                CodeActionEquivalenceKey = "Register an action to analyze code when changes occur",
            }.RunAsync();
        }
        #endregion

        #region TooManyInitStatements

        public const string s_tooManyInitStatementsMessage = s_messagePrefix + "For this tutorial, the 'Initialize' method should only register one action";

        //  statement below, incorrect method name
        [Fact]
        public async Task MultipleInit1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(Practice, SyntaxKind.IfStatement);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer004: T: For this tutorial, the 'Initialize' method should only register one action
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.TooManyInitStatements).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(37,46): error CS0103: The name 'Practice' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(37, 46, 37, 54).WithArguments("Practice"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(36,46): error MetaAnalyzer044: T: The method 'AnalyzeIfStatement' that was registered to perform the analysis is missing
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingAnalysisMethod).WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                CodeActionEquivalenceKey = "Remove multiple registered actions from the Initialize method",
            }.RunAsync();
        }

        //  statement below, incorrect syntax kind
        [Fact]
        public async Task MultipleInit2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer004: T: For this tutorial, the 'Initialize' method should only register one action
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.TooManyInitStatements).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(37,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(37, 46, 37, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(36,46): error MetaAnalyzer044: T: The method 'AnalyzeIfStatement' that was registered to perform the analysis is missing
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingAnalysisMethod).WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                CodeActionEquivalenceKey = "Remove multiple registered actions from the Initialize method",
            }.RunAsync();
        }

        //  incorrect statement above
        [Fact]
        public async Task MultipleInit3()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer004: T: For this tutorial, the 'Initialize' method should only register one action
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.TooManyInitStatements).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(37,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(37, 46, 37, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(36,46): error MetaAnalyzer044: T: The method 'AnalyzeIfStatement' that was registered to perform the analysis is missing
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingAnalysisMethod).WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                CodeActionEquivalenceKey = "Remove multiple registered actions from the Initialize method",
            }.RunAsync();
        }

        // multiple incorrect statements below
        [Fact]
        public async Task MultipleInit4()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
            context.RegisterSyntaxNodeAction(Practice, SyntaxKind.IfStatement);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer004: T: For this tutorial, the 'Initialize' method should only register one action
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.TooManyInitStatements).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(37,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(37, 46, 37, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(38,46): error CS0103: The name 'Practice' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(38, 46, 38, 54).WithArguments("Practice"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(36,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(36,46): error MetaAnalyzer044: T: The method 'AnalyzeIfStatement' that was registered to perform the analysis is missing
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingAnalysisMethod).WithSpan(36, 46, 36, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                CodeActionEquivalenceKey = "Remove multiple registered actions from the Initialize method",
            }.RunAsync();
        }

        //  multiple incorrect statements above
        [Fact]
        public async Task MultipleInit5()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Practice, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Practice, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,30): error MetaAnalyzer004: T: For this tutorial, the 'Initialize' method should only register one action
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.TooManyInitStatements).WithSpan(34, 30, 34, 40).WithArguments("Initialize"),
                        // Test0.cs(36,46): error CS0103: The name 'Practice' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 54).WithArguments("Practice"),
                        // Test0.cs(37,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(37, 46, 37, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(38,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(38, 46, 38, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(36,46): error CS0103: The name 'Practice' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(36, 46, 36, 54).WithArguments("Practice"),
                        // Test0.cs(36,46): error MetaAnalyzer044: T: The method 'Practice' that was registered to perform the analysis is missing
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingAnalysisMethod).WithSpan(36, 46, 36, 54).WithArguments("Practice"),
                    },
                },
                CodeActionEquivalenceKey = "Remove multiple registered actions from the Initialize method",
            }.RunAsync();
        }

        //  no correct statements, multiple incorrect
        [Fact]
        public async Task MultipleInit6()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Practice, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Practice, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(31,30): error MetaAnalyzer004: T: For this tutorial, the 'Initialize' method should only register one action
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.TooManyInitStatements).WithSpan(31, 30, 31, 40).WithArguments("Initialize"),
                        // Test0.cs(33,46): error CS0103: The name 'Practice' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 46, 33, 54).WithArguments("Practice"),
                        // Test0.cs(33,56): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 56, 33, 66).WithArguments("SyntaxKind"),
                        // Test0.cs(34,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 46, 34, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(34,66): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 66, 34, 76).WithArguments("SyntaxKind"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,46): error CS0103: The name 'Practice' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 46, 33, 54).WithArguments("Practice"),
                        // Test0.cs(33,56): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 56, 33, 66).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove multiple registered actions from the Initialize method",
            }.RunAsync();
        }
        #endregion

        #region InvalidStatement

        //  invalid throw statement
        [Fact]
        public async Task InvalidStatement1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           throw new NotImplementedException();
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,12): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(33, 12, 33, 48),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(31,30): error MetaAnalyzer003: T: A syntax node action should be registered within the 'Initialize' method
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRegisterStatement).WithSpan(31, 30, 31, 40).WithArguments("Initialize"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid break statement
        [Fact]
        public async Task InvalidStatement2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
           break;
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                        // Test0.cs(34,12): error CS0139: No enclosing loop out of which to break or continue
                        DiagnosticResult.CompilerError("CS0139").WithSpan(34, 12, 34, 18),
                        // Test0.cs(34,12): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 12, 34, 18),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        // invalid check statement
        [Fact]
        public async Task InvalidStatement3()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
           checked { var num = num + 1; }
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                        // Test0.cs(34,12): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 12, 34, 42),
                        // Test0.cs(34,32): error CS0841: Cannot use local variable 'num' before it is declared
                        DiagnosticResult.CompilerError("CS0841").WithSpan(34, 32, 34, 35).WithArguments("num"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid continue statement
        [Fact]
        public async Task InvalidStatement4()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
           continue;
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                        // Test0.cs(34,12): error CS0139: No enclosing loop out of which to break or continue
                        DiagnosticResult.CompilerError("CS0139").WithSpan(34, 12, 34, 21),
                        // Test0.cs(34,12): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 12, 34, 21),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  do while statement
        [Fact]
        public async Task InvalidStatement5()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
           do { var i = 1; } while (i > 3);
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                        // Test0.cs(34,12): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 12, 34, 44),
                        // Test0.cs(34,37): error CS0103: The name 'i' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 37, 34, 38).WithArguments("i"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid random expression statement
        [Fact]
        public async Task InvalidStatement6()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
           context.GetHashCode();
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                        // Test0.cs(34,12): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 12, 34, 34),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid foreach statement
        [Fact]
        public async Task InvalidStatement7()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
           foreach() { break; }
        }
    }
}";
            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
           context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                        // Test0.cs(34,12): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 12, 34, 32),
                        // Test0.cs(34,20): error CS0230: Type and identifier are both required in a foreach statement
                        DiagnosticResult.CompilerError("CS0230").WithSpan(34, 20, 34, 21),
                        // Test0.cs(34,20): error CS1515: 'in' expected
                        DiagnosticResult.CompilerError("CS1515").WithSpan(34, 20, 34, 21),
                        // Test0.cs(34,20): error CS1525: Invalid expression term ')'
                        DiagnosticResult.CompilerError("CS1525").WithSpan(34, 20, 34, 21).WithArguments(")"),
                        // Test0.cs(34,20): error CS1525: Invalid expression term ')'
                        DiagnosticResult.CompilerError("CS1525").WithSpan(34, 20, 34, 21).WithArguments(")")
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,45): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 45, 33, 63).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,65): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 65, 33, 75).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid for statement
        [Fact]
        public async Task InvalidStatement8()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                for(int i = 1; i < 3; i++) { i++; }
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 52),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid if statement
        [Fact]
        public async Task InvalidStatement9()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                if (i < 3) { i++; }
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 36),
                        // Test0.cs(34,21): error CS0103: The name 'i' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 21, 34, 22).WithArguments("i"),
                        // Test0.cs(34,30): error CS0103: The name 'i' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 30, 34, 31).WithArguments("i"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid labeled statement
        [Fact]
        public async Task InvalidStatement10()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                context: return context;
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 41),
                        // Test0.cs(34,26): error CS0127: Since 'SyntaxNodeAnalyzerAnalyzer.Initialize(AnalysisContext)' returns void, a return keyword must not be followed by an object expression
                        DiagnosticResult.CompilerError("CS0127").WithSpan(34, 26, 34, 32).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzerAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid local declaration statement
        [Fact]
        public async Task InvalidStatement11()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                int i;
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 23),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid lock statement
        [Fact]
        public async Task InvalidStatement12()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                lock () {}
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 27),
                        // Test0.cs(34,23): error CS1525: Invalid expression term ')'
                        DiagnosticResult.CompilerError("CS1525").WithSpan(34, 23, 34, 24).WithArguments(")"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  invalid return statement
        [Fact]
        public async Task InvalidStatement13()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                return;
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 24),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  multiple invalid statements
        [Fact]
        public async Task InvalidStatement14()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                int one = 1;
                int two = 2;
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                int two = 2;
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 29),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(34, 17, 34, 29),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  one invalid statement, no valid statements
        [Fact]
        public async Task InvalidStatement15()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                int one = 1;
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(33, 17, 33, 29),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(31,34): error MetaAnalyzer003: T: A syntax node action should be registered within the 'Initialize' method
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingRegisterStatement).WithSpan(31, 34, 31, 44).WithArguments("Initialize"),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  multiple invalid statements, no valid statements
        [Fact]
        public async Task InvalidStatement16()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                int one = 1;
                int two = 2;
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                int two = 2;
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(33, 17, 33, 29),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(33, 17, 33, 29),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }

        //  multiple valid statements, one invalid statement
        [Fact]
        public async Task InvalidStatement17()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
                int one = 1;
                int two = 2;
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
                int two = 2;
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 50, 34, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(34,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 70, 34, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(35,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(35, 17, 35, 29),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(33,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 50, 33, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(33,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(33, 70, 33, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(34,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 50, 34, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(34,70): error CS0103: The name 'SyntaxKind' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 70, 34, 80).WithArguments("SyntaxKind"),
                        // Test0.cs(35,17): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(35, 17, 35, 29),
                    },
                },
                CodeActionEquivalenceKey = "Remove invalid statements from the Initialize method",
            }.RunAsync();
        }
        #endregion

        #region IncorrectKind

        [Fact]
        public async Task IncorrectKind()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfKeyword);
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
            // Calls the method (first argument) to perform analysis whenever a SyntaxNode of kind IfStatement is found
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,50): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(34, 50, 34, 68).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(34,70): error MetaAnalyzer051: T: This tutorial only allows registering for SyntaxKind.IfStatement
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectKind).WithSpan(34, 70, 34, 90),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(35,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(35, 46, 35, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(35,46): error MetaAnalyzer044: T: The method 'AnalyzeIfStatement' that was registered to perform the analysis is missing
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingAnalysisMethod).WithSpan(35, 46, 35, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                CodeActionEquivalenceKey = "Analyze the correct SyntaxKind",
            }.RunAsync();
        }
        #endregion

        #region IncorrectArguments

        [Fact]
        public async Task IncorrectArguments1()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction();
            }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

            public override void Initialize(AnalysisContext context)
            {
            // Calls the method (first argument) to perform analysis whenever a SyntaxNode of kind IfStatement is found
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,17): error MetaAnalyzer053: T: The method RegisterSyntaxNodeAction requires 2 arguments: a method and a SyntaxKind
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectArguments).WithSpan(34, 17, 34, 49),
                        // Test0.cs(34,25): error CS1501: No overload for method 'RegisterSyntaxNodeAction' takes 0 arguments
                        DiagnosticResult.CompilerError("CS1501").WithSpan(34, 25, 34, 49).WithArguments("RegisterSyntaxNodeAction", "0"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(35,46): error CS0103: The name 'AnalyzeIfStatement' does not exist in the current context
                        DiagnosticResult.CompilerError("CS0103").WithSpan(35, 46, 35, 64).WithArguments("AnalyzeIfStatement"),
                        // Test0.cs(35,46): error MetaAnalyzer044: T: The method 'AnalyzeIfStatement' that was registered to perform the analysis is missing
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.MissingAnalysisMethod).WithSpan(35, 46, 35, 64).WithArguments("AnalyzeIfStatement"),
                    },
                },
                CodeActionEquivalenceKey = "Add the correct arguments to the Initialize method",
            }.RunAsync();
        }

        [Fact]
        public async Task IncorrectArguments2()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction();
        }

        private void Method2(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // Calls the method (first argument) to perform analysis whenever a SyntaxNode of kind IfStatement is found
            context.RegisterSyntaxNodeAction(Method2, SyntaxKind.IfStatement);
        }

        private void Method2(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,13): error MetaAnalyzer053: T: The method RegisterSyntaxNodeAction requires 2 arguments: a method and a SyntaxKind
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectArguments).WithSpan(34, 13, 34, 45),
                        // Test0.cs(34,21): error CS1501: No overload for method 'RegisterSyntaxNodeAction' takes 0 arguments
                        DiagnosticResult.CompilerError("CS1501").WithSpan(34, 21, 34, 45).WithArguments("RegisterSyntaxNodeAction", "0"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(40,13): error MetaAnalyzer021: T: This statement should extract the if-statement being analyzed by casting context.Node to IfStatementSyntax
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IfStatementIncorrect).WithSpan(40, 13, 40, 49).WithArguments("context"),
                    },
                },
                CodeActionEquivalenceKey = "Add the correct arguments to the Initialize method",
            }.RunAsync();
        }

        [Fact]
        public async Task IncorrectArguments3()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(SyntaxKind.IfStatement);
        }

        private void Method2(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";
            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // Calls the method (first argument) to perform analysis whenever a SyntaxNode of kind IfStatement is found
            context.RegisterSyntaxNodeAction(Method2, SyntaxKind.IfStatement);
        }

        private void Method2(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
        }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(34,13): error MetaAnalyzer053: T: The method RegisterSyntaxNodeAction requires 2 arguments: a method and a SyntaxKind
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectArguments).WithSpan(34, 13, 34, 45),
                        // Test0.cs(34,21): error CS0411: The type arguments for method 'AnalysisContext.RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext>, params TLanguageKindEnum[])' cannot be inferred from the usage. Try specifying the type arguments explicitly.
                        DiagnosticResult.CompilerError("CS0411").WithSpan(34, 21, 34, 45).WithArguments("Microsoft.CodeAnalysis.Diagnostics.AnalysisContext.RegisterSyntaxNodeAction<TLanguageKindEnum>(System.Action<Microsoft.CodeAnalysis.Diagnostics.SyntaxNodeAnalysisContext>, params TLanguageKindEnum[])"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(40,13): error MetaAnalyzer021: T: This statement should extract the if-statement being analyzed by casting context.Node to IfStatementSyntax
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IfStatementIncorrect).WithSpan(40, 13, 40, 49).WithArguments("context"),
                    },
                },
                CodeActionEquivalenceKey = "Add the correct arguments to the Initialize method",
            }.RunAsync();
        }
        #endregion

        #region IncorrectInitSig

        //  more than one parameter
        [Fact]
        public async Task InitSig1()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context, int i)
        {
            throw new NotImplementedException();
        }
    }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(16,22): error CS0534: 'SyntaxNodeAnalyzer' does not implement inherited abstract member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0534").WithSpan(16, 22, 16, 40).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(35,30): error CS0115: 'SyntaxNodeAnalyzer.Initialize(AnalysisContext, int)': no suitable method found to override
                        DiagnosticResult.CompilerError("CS0115").WithSpan(35, 30, 35, 40).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext, int)"),
                        // Test0.cs(35,30): error MetaAnalyzer005: T: The 'Initialize' method should return void, have the 'override' modifier, and have a single parameter of type 'AnalysisContext'
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectInitSig).WithSpan(35, 30, 35, 40).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(37,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(37, 13, 37, 49),
                    },
                },
                CodeActionEquivalenceKey = "Implement the correct signature for the Initialize method",
            }.RunAsync();
        }

        //  Wrong type for first parameter
        [Fact]
        public async Task InitSig2()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(int context)
        {
            throw new NotImplementedException();
        }
    }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(16,22): error CS0534: 'SyntaxNodeAnalyzer' does not implement inherited abstract member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0534").WithSpan(16, 22, 16, 40).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(35,30): error CS0115: 'SyntaxNodeAnalyzer.Initialize(int)': no suitable method found to override
                        DiagnosticResult.CompilerError("CS0115").WithSpan(35, 30, 35, 40).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer.Initialize(int)"),
                        // Test0.cs(35,30): error MetaAnalyzer005: T: The 'Initialize' method should return void, have the 'override' modifier, and have a single parameter of type 'AnalysisContext'
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectInitSig).WithSpan(35, 30, 35, 40).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(37,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(37, 13, 37, 49),
                    },
                },
                CodeActionEquivalenceKey = "Implement the correct signature for the Initialize method",
            }.RunAsync();
        }

        //  accessibility is not public
        [Fact]
        public async Task InitSig3()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        private override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(35,31): error CS0507: 'SyntaxNodeAnalyzer.Initialize(AnalysisContext)': cannot change access modifiers when overriding 'public' inherited member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0507").WithSpan(35, 31, 35, 41).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)", "public", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(35,31): error CS0621: 'SyntaxNodeAnalyzer.Initialize(AnalysisContext)': virtual or abstract members cannot be private
                        DiagnosticResult.CompilerError("CS0621").WithSpan(35, 31, 35, 41).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(35,31): error MetaAnalyzer005: T: The 'Initialize' method should return void, have the 'override' modifier, and have a single parameter of type 'AnalysisContext'
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectInitSig).WithSpan(35, 31, 35, 41).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(37,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(37, 13, 37, 49),
                    },
                },
                CodeActionEquivalenceKey = "Implement the correct signature for the Initialize method",
            }.RunAsync();
        }

        //  initialize method is not overriden
        [Fact]
        public async Task InitSig4()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(16,22): error CS0534: 'SyntaxNodeAnalyzer' does not implement inherited abstract member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0534").WithSpan(16, 22, 16, 40).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)"),
                        // Test0.cs(35,21): error MetaAnalyzer005: T: The 'Initialize' method should return void, have the 'override' modifier, and have a single parameter of type 'AnalysisContext'
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectInitSig).WithSpan(35, 21, 35, 31).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(37,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(37, 13, 37, 49),
                    },
                },
                CodeActionEquivalenceKey = "Implement the correct signature for the Initialize method",
            }.RunAsync();
        }

        //  initialize method does not return void
        [Fact]
        public async Task InitSig5()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override int Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";
            var fixtest = @"using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzer : DiagnosticAnalyzer
        {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
    }";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { test },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(35,29): error CS0508: 'SyntaxNodeAnalyzer.Initialize(AnalysisContext)': return type must be 'void' to match overridden member 'DiagnosticAnalyzer.Initialize(AnalysisContext)'
                        DiagnosticResult.CompilerError("CS0508").WithSpan(35, 29, 35, 39).WithArguments("SyntaxNodeAnalyzer.SyntaxNodeAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)", "Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)", "void"),
                        // Test0.cs(35,29): error MetaAnalyzer005: T: The 'Initialize' method should return void, have the 'override' modifier, and have a single parameter of type 'AnalysisContext'
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.IncorrectInitSig).WithSpan(35, 29, 35, 39).WithArguments("Initialize"),
                    },
                },
                FixedState =
                {
                    Sources = { fixtest },
                    ExpectedDiagnostics =
                    {
                        // Test0.cs(37,13): error MetaAnalyzer006: T: The Initialize method only registers actions, therefore any other statement placed in Initialize is incorrect
                        VerifyCS.Diagnostic(MetaCompilationAnalyzer.InvalidStatement).WithSpan(37, 13, 37, 49),
                    },
                },
                CodeActionEquivalenceKey = "Implement the correct signature for the Initialize method",
            }.RunAsync();
        }
        #endregion

#if false
        #region IfStatementIncorrect

        private const string s_ifStatementIncorrectMessage = s_messagePrefix + "This statement should extract the if-statement being analyzed by casting context.Node to IfStatementSyntax";

        //  No identifier for statement
        [Fact]
        public void IfStatementIncorrect1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var = (IfStatementSyntax)context.Node;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        //  ifStatement not initialized
        [Fact]
        public void IfStatementIncorrect2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        //  no cast
        [Fact]
        public void IfStatementIncorrect3()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = context.Node;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  Wrong cast type
        [Fact]
        public void IfStatementIncorrect()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (MethodDeclarationSyntax)context.Node;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  not a member access expression
        [Fact]
        public void IfStatementIncorrect5()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong object
        [Fact]
        public void IfStatementIncorrect6()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)obj.Node;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  doesn't access node
        [Fact]
        public void IfStatementIncorrect7()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.SemanticModel;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  check that statements below are retained
        [Fact]
        public void IfStatementIncorrect8()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.SemanticModel;
                var ifKeyword = ifStatement.IfKeyword;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementIncorrect,
                Message = s_ifStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
            var ifKeyword = ifStatement.IfKeyword;
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region IfKeywordIncorrect

        private const string s_ifKeywordIncorrectMessage = s_messagePrefix + "This statement should extract the if-keyword SyntaxToken from 'ifStatement'";

        // not initialized
        [Fact]
        public void IfKeywordIncorrect1()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordIncorrect,
                Message = s_ifKeywordIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  no member access expression
        [Fact]
        public void IfKeywordIncorrect2()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordIncorrect,
                Message = s_ifKeywordIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong identifier name
        [Fact]
        public void IfKeyword3()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifState.IfKeyword;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordIncorrect,
                Message = s_ifKeywordIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  doesn't access IfKeyword
        [Fact]
        public void IfKeyword4()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.Condition;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordIncorrect,
                Message = s_ifKeywordIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  no variable declaration 
        [Fact]
        public void IfKeywordIncorrect5()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                ifStatement.IfKeyword;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordIncorrect,
                Message = s_ifKeywordIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statements below are retained
        [Fact]
        public void IfKeywordIncorrect6()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement;
                if (ifKeyword.HasTrailingTrivia)
                {
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordIncorrect,
                Message = s_ifKeywordIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
                {
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region IfStatementMissing

        private const string s_ifStatementMissingMessage = s_messagePrefix + "The first step of the SyntaxNode analysis is to extract the if-statement from 'context' by casting context.Node to IfStatementSyntax";

        //  no statements in analyze method
        [Fact]
        public void IfStatementMissing1()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementMissing,
                Message = s_ifStatementMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 40, 26) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  check comments aren't counted as statements
        [Fact]
        public void IfStatementMissing2()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                // var ifStatement = (IfStatementSyntax)context.Node;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfStatementMissing,
                Message = s_ifStatementMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 40, 26) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
            // The SyntaxNode found by the Initialize method should be cast to the expected type. Here, this type is IfStatementSyntax
            var ifStatement = (IfStatementSyntax)context.Node;
            // var ifStatement = (IfStatementSyntax)context.Node;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region IfKeywordMissing

        private const string s_ifKeywordMissingMessage = s_messagePrefix + "Next, extract the if-keyword SyntaxToken from 'ifStatement'";

        //  no 2nd statement
        [Fact]
        public void IfKeywordMissing1()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordMissing,
                Message = s_ifKeywordMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  second statement is in the comments
        [Fact]
        public void IfKeywordMissing2()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                // var ifKeyword = ifStatement.IfKeyword;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IfKeywordMissing,
                Message = s_ifKeywordMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;

            // This statement navigates down the syntax tree one level to extract the 'if' keyword
            var ifKeyword = ifStatement.IfKeyword;
            // var ifKeyword = ifStatement.IfKeyword;
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TrailingTriviaCheckMissing

        private const string s_trailingTriviaCheckMissingMessage = s_messagePrefix + "Next, begin looking for the space between 'if' and '(' by checking if 'ifKeyword' has trailing trivia";

        //  no 3rd statement
        [Fact]
        public void TrailingCheckMissing1()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckMissing,
                Message = s_trailingTriviaCheckMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;

            // Checks if there is any trailing trivia (eg spaces or comments) associated with the if-keyword
            if (ifKeyword.HasTrailingTrivia)
            {
            }
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  3rd statement commented
        [Fact]
        public void TrailingCheckMissing2()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                /* if (ifKeyword.HasTrailingTrivia)
                {
                }/*
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckMissing,
                Message = s_trailingTriviaCheckMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;

            // Checks if there is any trailing trivia (eg spaces or comments) associated with the if-keyword
            if (ifKeyword.HasTrailingTrivia)
            {
            }                /* if (ifKeyword.HasTrailingTrivia)
                {
                }/*
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TrailingTriviaCheckIncorrect

        private const string s_trailingTriviaCheckIncorrectMessage = s_messagePrefix + "This statement should be an if-statement that checks to see if 'ifKeyword' has trailing trivia";

        //  no if statement
        [Fact]
        public void TrailingCheckIncorrect1()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                (ifKeyword.HasTrailingTrivia)
                {
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckIncorrect,
                Message = s_trailingTriviaCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
            {
            }

            {
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  misslabeled accessor
        [Fact]
        public void TrailingCheckIncorrect2()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifStatement.HasTrailingTrivia)
                {
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckIncorrect,
                Message = s_trailingTriviaCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
            {
            }
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  Doesnt access HasTrailingTrivia
        [Fact]
        public void TrailingCheckIncorrect3()
        {
            var test = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasLeadingTrivia)
                {
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckIncorrect,
                Message = s_trailingTriviaCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Threading;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;
            using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
            {
            }
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  throw statement
        [Fact]
        public void TrailingCheckIncorrec4()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                throw new NotImplementedException();
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckIncorrect,
                Message = s_trailingTriviaCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
            {
            }
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statements below incorrect statement
        [Fact]
        public void TrailingCheckIncorrect5()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasLeadingTrivia)
                {
                }
                var openParen = ifStatement.OpenParenToken;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckIncorrect,
                Message = s_trailingTriviaCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
            {
            }

            var openParen = ifStatement.OpenParenToken;
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statements within if block
        [Fact]
        public void TrailingCheckIncorrect6()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasLeadingTrivia)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.Last();
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckIncorrect,
                Message = s_trailingTriviaCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
            {
                var trailingTrivia = ifKeyword.TrailingTrivia.Last();
            }
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  incorrect statement is next if statement
        [Fact]
        public void TrailingCheckIncorrect7()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                {
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCheckIncorrect,
                Message = s_trailingTriviaCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
            if (ifKeyword.HasTrailingTrivia)
            {
            }
        }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TrailingTriviaVarMissing

        private const string s_trailingTriviaVarMissingMessage = s_messagePrefix + "Next, extract the first trailing trivia of 'ifKeyword' into a variable";

        //  no variable declaration
        [Fact]
        public void TrailingVarMissing1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarMissing,
                Message = s_trailingTriviaVarMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statement below if block
        [Fact]
        public void TrailingVarMissing2()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    }
                }
                var trailing = ifKeyword.TrailingTrivia.First();
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarMissing,
                Message = s_trailingTriviaVarMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
                var trailing = ifKeyword.TrailingTrivia.First();
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  declaration in comments
        [Fact]
        public void TrailingVarMissing3()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        // var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarMissing,
                Message = s_trailingTriviaVarMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    // var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TrailingTriviaVarIncorrect

        private const string s_trailingTriviaVarIncorrectMessage = s_messagePrefix + "This statement should extract the first trailing trivia of 'ifKeyword' into a variable";

        //  not initialized
        [Fact]
        public void TrailingVarIncorrect1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia;
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  no member access expressions
        [Fact]
        public void TrailingVarIncorrect2()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword;
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // only one member access expression
        [Fact]
        public void TrailingVarIncorrect3()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia;
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        //  member access expression order switched
        [Fact]
        public void TrailingVarIncorrect4()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.First().TrailingTrivia;
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  throw statement
        [Fact]
        public void TrailingVarIncorrect5()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong accessor
        [Fact]
        public void TrailingVarIncorrect6()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifStatement.TrailingTrivia.First();
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statements below if block
        [Fact]
        public void TrailingVarIncorrect7()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifStatement.TrailingTrivia.First();
                    }
                }
                var openParen = ifStatement.OpenParenToken;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
                var openParen = ifStatement.OpenParenToken;
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statements within if block
        [Fact]
        public void TrailingVarIncorrect8()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifStatement.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // incorrect statement is the next statement
        [Fact]
        public void TrailingVarIncorrect9()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaVarIncorrect,
                Message = s_trailingTriviaVarIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region InternalAndStaticError

        private const string s_internalAndStaticErrorMessage = s_messagePrefix + "The 'Rule' field should be internal and static";

        [Fact]
        public void InternalAndStatic1() // missing internal modifier
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.InternalAndStaticError,
                Message = s_internalAndStaticErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 19, 37) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void InternalAndStatic2() // missing static modifier
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.InternalAndStaticError,
                Message = s_internalAndStaticErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 19, 39) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void InternalAndStatic3() // missing both modifiers
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

         DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.InternalAndStaticError,
                Message = s_internalAndStaticErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 19, 31) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        #endregion

        #region EnabledByDefault

        private const string s_enabledByDefaultMessage = s_messagePrefix + "The 'isEnabledByDefault' field should be set to true";

        [Fact]
        public void EnabledByDefault1() // isEnabledByDefault set to false
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: false);
        // id: Identifies each rule. Same as the public constant declared above
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.EnabledByDefaultError,
                Message = s_enabledByDefaultMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 33) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // isEnabledByDefault: Determines whether the analyzer is enabled by default or if the user must manually enable it. Generally set to true
        // id: Identifies each rule. Same as the public constant declared above
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void EnabledByDefault2() // isEnabledByDefault set to undeclared variable
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: test);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.EnabledByDefaultError,
                Message = s_enabledByDefaultMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 33) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // isEnabledByDefault: Determines whether the analyzer is enabled by default or if the user must manually enable it. Generally set to true

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void EnabledByDefault3() // isEnabledByDefault set to member access expression
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: DiagnosticSeverity.Error);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.EnabledByDefaultError,
                Message = s_enabledByDefaultMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 25, 33) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // isEnabledByDefault: Determines whether the analyzer is enabled by default or if the user must manually enable it. Generally set to true

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void EnabledByDefault4() // isEnabledByDefault with argument missing
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: );
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.EnabledByDefaultError,
                Message = s_enabledByDefaultMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 23, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // isEnabledByDefault: Determines whether the analyzer is enabled by default or if the user must manually enable it. Generally set to true
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region DefaultSeverityError

        private const string s_defaultSeverityErrorMessage = s_messagePrefix + "The 'defaultSeverity' should be either DiagnosticSeverity.Error or DiagnosticSeverity.Warning";

        [Fact]
        public void DefaultSeverity1() // defaultSeverity set to undeclared variable.
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: test, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DefaultSeverityError,
                Message = s_defaultSeverityErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 25, 30) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtestError = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var fixtestWarning = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtestError, 0);
            VerifyCSharpFix(test, fixtestWarning, 1);
        }

        [Fact]
        public void DefaultSeverity2() // defaultSeverity.Name set to arbitrary string
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.test, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DefaultSeverityError,
                Message = s_defaultSeverityErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 24, 30) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtestError = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var fixtestWarning = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtestError, 0);
            VerifyCSharpFix(test, fixtestWarning, 1);
        }

        [Fact]
        public void DefaultSeverity3() // defaultSeverity with argument missing
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: , // possible options
            isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DefaultSeverityError,
                Message = s_defaultSeverityErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtestError = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Error, // possible options
            isEnabledByDefault: true);
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var fixtestWarning = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // defaultSeverity: Is set to DiagnosticSeverity.[severity] where severity can be Error, Warning, Hidden or Info, but can only be Error or Warning for the purposes of this tutorial
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtestError, 0);
            VerifyCSharpFix(test, fixtestWarning, 1);
        }
        #endregion

        #region IdDeclTypeError

        private const string s_idDeclTypeErrorMessage = s_messagePrefix + "The diagnostic id should be the constant string declared above";
        private const string s_idStringLiteralMessage = s_messagePrefix + "The ID should not be a string literal, because the ID must be accessible from the code fix provider";

        [Fact]
        public void IdDeclType1() // id set to a literal string
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: ""test"", // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IdStringLiteral,
                Message = s_idStringLiteralMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // id: Identifies each rule. Same as the public constant declared above

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IdDeclType2() // id set to a member access expression
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticSeverity.Warning, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IdDeclTypeError,
                Message = s_idDeclTypeErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // id: Identifies each rule. Same as the public constant declared above
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IdDeclType3() // id set to true
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: true, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IdDeclTypeError,
                Message = s_idDeclTypeErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // id: Identifies each rule. Same as the public constant declared above
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IdDeclType4() // id with argument missing
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: , // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IdDeclTypeError,
                Message = s_idDeclTypeErrorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 16) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);
        // id: Identifies each rule. Same as the public constant declared above
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region MissingIdDeclaration

        private const string s_missingIdDeclarationMessage = s_messagePrefix + "This diagnostic id should be the constant string declared above";

        [Fact]
        public void MissingIdDeclaration1() // id set to undeclared variable
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: test, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.MissingIdDeclaration,
                Message = s_missingIdDeclarationMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 20, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        public const string test = ""DescriptiveId"";
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: test, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region OpenParenTests

        private const string s_missingOpenParenMessage = s_messagePrefix + "Moving on to the creation and reporting of the diagnostic, extract the open parenthesis of 'ifState' into a variable to use as the end of the diagnostic span";
        private const string s_incorrectOpenParenMessage = s_messagePrefix + "This statement should extract the open parenthesis of 'ifState' to use as the end of the diagnostic span";

        [Fact]
        public void MissingOpenParen() // no DiagnosticDescriptor field
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.OpenParenMissing,
                Message = s_missingOpenParenMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 45, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            // Extracts the opening parenthesis of the if-statement condition
            var openParen = ifState.OpenParenToken;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectOpenParen()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifState = (IfStatementSyntax)context.Node;
                var ifKeyword = ifState.IfKeyword;

                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                return;
                            }
                        }
                    }
                }

                var test = ifState.Equals;
            }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.OpenParenIncorrect,
                Message = s_incorrectOpenParenMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 60, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifState = (IfStatementSyntax)context.Node;
                var ifKeyword = ifState.IfKeyword;

                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                return;
                            }
                        }
                    }
                }

            // Extracts the opening parenthesis of the if-statement condition
            var openParen = ifState.OpenParenToken;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region MissingSuppDiag

        private const string s_missingSuppDiagMessage = s_messagePrefix + "You are missing the required inherited SupportedDiagnostics property";

        [Fact]
        public void MissingSuppDiag1() // no SupportedDiagnostics property
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.MissingSuppDiag,
                Message = s_missingSuppDiagMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 18) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region IncorrectSigSuppDiag

        private const string s_incorrectSigSuppDiagMessage = s_messagePrefix + "The overriden SupportedDiagnostics property should return an Immutable Array of Diagnostic Descriptors";

        [Fact]
        public void IncorrectSigSuppDiag1() // no public modifier
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectSigSuppDiag,
                Message = s_incorrectSigSuppDiagMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 55) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectSigSuppDiag2() // no override modifier
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectSigSuppDiag,
                Message = s_incorrectSigSuppDiagMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 53) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectSigSuppDiag3() // no modifiers
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectSigSuppDiag,
                Message = s_incorrectSigSuppDiagMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 46) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }
        #endregion

        #region MissingAccessor

        private const string s_missingAccessorMessage = s_messagePrefix + "The 'SupportedDiagnostics' property is missing a get-accessor to return a list of supported diagnostics";

        [Fact]
        public void MissingAccessor1() // empty SupportedDiagnostics property
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {

        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.MissingAccessor,
                Message = s_missingAccessorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 62) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void MissingAccessor2() // SupportedDiagnostics property contains only set accessor
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.MissingAccessor,
                Message = s_missingAccessorMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 62) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TooManyAccessors

        private const string s_tooManyAccessorsMessage = s_messagePrefix + "The 'SupportedDiagnostics' property needs only a single get-accessor";

        [Fact]
        public void TooManyAccessors1() // SupportedDiagnostics property with get and then set accessors
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyAccessors,
                Message = s_tooManyAccessorsMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 32, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void TooManyAccessors2() // SupportedDiagnostics property with set and then get accessors
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            set
            {
                throw new NotImplementedException();
            }
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyAccessors,
                Message = s_tooManyAccessorsMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 32, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void TooManyAccessors3() // SupportedDiagnostics property with two get accessors
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create();
            }
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyAccessors,
                Message = s_tooManyAccessorsMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 32, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region IncorrectAccessorReturn

        private const string s_incorrectAccessorReturnMessage = s_messagePrefix + "The get-accessor should return an ImmutableArray containing all of the DiagnosticDescriptor rules";

        [Fact]
        public void IncorrectAccessorReturn1() // empty get accessor
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {

            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAccessorReturn,
                Message = s_incorrectAccessorReturnMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 62) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectAccessorReturn2() // get accessor throwing NotImplementedException
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAccessorReturn,
                Message = s_incorrectAccessorReturnMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 28, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectAccessorReturn3() // get accessor returning nothing
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAccessorReturn,
                Message = s_incorrectAccessorReturnMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectAccessorReturn4() // get accessor returning true
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return true;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAccessorReturn,
                Message = s_incorrectAccessorReturnMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectAccessorReturn5() // invocation expression form not invoked
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAccessorReturn,
                Message = s_incorrectAccessorReturnMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectAccessorReturn6() // variable declaration form not invoked
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var array = ImmutableArray.Create;
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAccessorReturn,
                Message = s_incorrectAccessorReturnMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                var array = ImmutableArray.Create(Rule);
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }
        #endregion

        #region SuppDiagReturn

        private const string s_suppDiagReturnValueMessage = s_messagePrefix + "The 'SupportedDiagnostics' property's get-accessor should return an ImmutableArray containing all DiagnosticDescriptor rules";

        [Fact]
        public void SuppDiagReturn1() // invocation expression form, incorrect invocation expression
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Equals();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SuppDiagReturnValue,
                Message = s_suppDiagReturnValueMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 24) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void SuppDiagReturn2() // variable declaration form, incorrect invocation expression
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var array = ImmutableArray.Equals();
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SuppDiagReturnValue,
                Message = s_suppDiagReturnValueMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 44) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                var array = ImmutableArray.Create();
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }
        #endregion

        #region SupportedRules
        private const string s_supportedRulesMessage = s_messagePrefix + "The ImmutableArray should contain every DiagnosticDescriptor rule that was created";

        [Fact]
        public void SupportedRules1() // invocation expression form with no arguments
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SupportedRules,
                Message = s_supportedRulesMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 24) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void SupportedRules2() // variable declaration form with no arguments
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var array = ImmutableArray.Create();
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SupportedRules,
                Message = s_supportedRulesMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                var array = ImmutableArray.Create(Rule);
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void SupportedRules3() // invocation expression form with missing rules
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        public const string spacingRuleId2 = ""IfSpacing2"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(
            id: spacingRuleId2,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SupportedRules,
                Message = s_supportedRulesMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 38, 24) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        public const string spacingRuleId2 = ""IfSpacing2"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(
            id: spacingRuleId2,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule, Rule2);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void SupportedRules4() // variable declaration form with missing rules
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        public const string spacingRuleId2 = ""IfSpacing2"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(
            id: spacingRuleId2,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var array = ImmutableArray.Create(Rule);
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SupportedRules,
                Message = s_supportedRulesMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 38, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        public const string spacingRuleId2 = ""IfSpacing2"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(
            id: spacingRuleId2,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                var array = ImmutableArray.Create(Rule, Rule2);
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void SupportedRules5() // invocation expression form, incorrect return type
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(1);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SupportedRules,
                Message = s_supportedRulesMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 24) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void SupportedRules6() // variable declaration form, incorrect return type
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var array = ImmutableArray.Create(1);
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SupportedRules,
                Message = s_supportedRulesMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                // This array contains all the diagnostics that can be shown to the user
                var array = ImmutableArray.Create(Rule);
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void SupportedRules7() // check that "return array;" is supported. ie SupportedRules diagnostic should surface
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var array = ImmutableArray.Create();
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SupportedRules,
                Message = s_supportedRulesMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 30, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        #endregion

        #region StartSpanTests

        private const string s_startSpanMissingMessage = s_messagePrefix + "Next, extract the start of the span of 'ifKeyword' into a variable, to be used as the start of the diagnostic span";
        private const string s_startSpanIncorrectMessage = s_messagePrefix + "This statement should extract the start of the span of 'ifKeyword' into a variable, to be used as the start of the diagnostic span";

        [Fact]
        public void MissingStartSpan()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.StartSpanMissing,
                Message = s_startSpanMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 60, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;

            // Determines the start of the span of the diagnostic that will be reported, ie the start of the squiggle
            var startDiagnosticSpan = ifKeyword.SpanStart;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectStartSpan1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifState.SpanStart;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.StartSpanIncorrect,
                Message = s_startSpanIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 61, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;

            // Determines the start of the span of the diagnostic that will be reported, ie the start of the squiggle
            var startDiagnosticSpan = ifKeyword.SpanStart;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        // Test that Span.Start is also accepted
        [Fact]
        public void IncorrectStartSpan2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            var ifKeyword = ifStatement.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();

                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var openParen = ifStatement.OpenParenToken;
            var startDiagnosticSpan = ifKeyword.Span.Start;
            var endDiagnosticSpan = openParen.SpanStart;
            var diagnosticSpan = TextSpan.FromBounds(startDiagnosticSpan, endDiagnosticSpan);
            var diagnosticLocation = Location.Create(ifStatement.SyntaxTree, diagnosticSpan);
            var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, Rule.MessageFormat);
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.GoToCodeFix,
                Message = s_messagePrefix + "Congratulations! You have written an analyzer! If you would like to explore a code fix for your diagnostic, open up CodeFixProvider.cs and take a look! To see your analyzer in action, press F5. A new instance of Visual Studio will open up, in which you can open a new C# console app and write test if-statements.",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 18) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        #endregion

        #region EndSpanTests

        private const string s_endSpanMissingMessage = s_messagePrefix + "Next, determine the end of the span of the diagnostic that is going to be reported";
        private const string s_endSpanIncorrectMessage = s_messagePrefix + "This statement should extract the start of the span of 'open' into a variable, to be used as the end of the diagnostic span";

        [Fact]
        public void MissingEndSpan()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.EndSpanMissing,
                Message = s_endSpanMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 61, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;

            // Determines the end of the span of the diagnostic that will be reported
            var endDiagnosticSpan = open.SpanStart;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectEndSpan1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            return 1;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.EndSpanIncorrect,
                Message = s_endSpanIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 62, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;

            // Determines the end of the span of the diagnostic that will be reported
            var endDiagnosticSpan = open.SpanStart;
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        // Check that functionality start.span is supported
        [Fact]
        public void IncorrectEndSpan2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            var ifKeyword = ifStatement.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();

                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var openParen = ifStatement.OpenParenToken;
            var startDiagnosticSpan = ifKeyword.SpanStart;
            var endDiagnosticSpan = openParen.Span.Start;
            var diagnosticSpan = TextSpan.FromBounds(startDiagnosticSpan, endDiagnosticSpan);
            var diagnosticLocation = Location.Create(ifStatement.SyntaxTree, diagnosticSpan);
            var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, Rule.MessageFormat);
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.GoToCodeFix,
                Message = s_messagePrefix + "Congratulations! You have written an analyzer! If you would like to explore a code fix for your diagnostic, open up CodeFixProvider.cs and take a look! To see your analyzer in action, press F5. A new instance of Visual Studio will open up, in which you can open a new C# console app and write test if-statements.",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 18) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        #endregion

        #region SpanTests

        private const string s_spanMissingMessage = s_messagePrefix + "Next, using TextSpan.FromBounds, create a variable that is the span of the diagnostic that will be reported";
        private const string s_spanIncorrectMessage = s_messagePrefix + "This statement should use TextSpan.FromBounds, 'start', and 'end' to create the span of the diagnostic that will be reported";

        [Fact]
        public void MissingSpan()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SpanMissing,
                Message = s_spanMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 62, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;

            // The span is the range of integers that define the position of the characters the red squiggle will underline
            var diagnosticSpan = TextSpan.FromBounds(start, end);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectSpan()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            if (true) {}
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.SpanIncorrect,
                Message = s_spanIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 63, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;

            // The span is the range of integers that define the position of the characters the red squiggle will underline
            var diagnosticSpan = TextSpan.FromBounds(start, end);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region LocationTests

        private const string s_locationMissingMessage = s_messagePrefix + "Next, using Location.Create, create a location for the diagnostic";
        private const string s_locationIncorrectMessage = s_messagePrefix + "This statement should use Location.Create, 'ifState', and 'span' to create the location of the diagnostic";

        [Fact]
        public void MissingLocation()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.LocationMissing,
                Message = s_locationMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 63, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);

            // Uses the span created above to create a location for the diagnostic squiggle to appear within the syntax tree passed in as an argument
            var diagnosticLocation = Location.Create(ifState.SyntaxTree, span);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectLocation()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var diagnosticLocation = ""Hello World"";
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.LocationIncorrect,
                Message = s_locationIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 64, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);

            // Uses the span created above to create a location for the diagnostic squiggle to appear within the syntax tree passed in as an argument
            var diagnosticLocation = Location.Create(ifState.SyntaxTree, span);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region DiagnosticTests

        private const string s_diagnosticMissingMessage = s_messagePrefix + "Next, use Diagnostic.Create to create the diagnostic";
        private const string s_diagnosticIncorrectMessage = s_messagePrefix + "This statement should use Diagnostic.Create, 'spacingRule', and 'location' to create the diagnostic that will be reported";

        [Fact]
        public void MissingDiagnostic()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DiagnosticMissing,
                Message = s_diagnosticMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 64, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);

            // Holds the diagnostic and all necessary information to be reported
            var diagnostic = Diagnostic.Create(spacingRule, location);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectDiagnostic()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            diagnostic = Diagnostic.Create(spacingRule, location, spacingRule.MessageFormat);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DiagnosticIncorrect,
                Message = s_diagnosticIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 65, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);

            // Holds the diagnostic and all necessary information to be reported
            var diagnostic = Diagnostic.Create(spacingRule, location);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TrailingTriviaKindCheckMissing

        private const string s_trailingTriviaKindCheckMissingMessage = s_messagePrefix + "Next, check if the kind of 'trailingTrivia' is whitespace trivia";

        [Fact]
        public void TrailingKindMissing1() //  no whitespace check
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckMissing,
                Message = s_trailingTriviaKindCheckMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();

                    // Checks that the single trailing trivia is of kind whitespace (as opposed to a comment for example)
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TrailingTriviaKindCheckIncorrect

        private const string s_trailingTriviaKindCheckIncorrectMessage = s_messagePrefix + "This statement should check to see if the kind of 'trailingTrivia' is whitespace trivia";

        // random variable declaration
        [Fact]
        public void TrailingKindIncorrect1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        var ifCheck = true;
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong variable name
        [Fact]
        public void TrailingKindIncorrect2()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (ifKeyword.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  Doesn't access kind method
        [Fact]
        public void TriviaKindIncorrect3()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  Accesses different method (not kind)
        [Fact]
        public void TrailingKindIncorrect4()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.IsKind() == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // one equals sign
        [Fact]
        public void TrailingKindIncorrect5()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() = SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // wrong member accessor
        [Fact]
        public void TrailingKindIncorrect6()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SymbolKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong accessed
        [Fact]
        public void TrailingKindIncorrect7()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.IfStatement)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  first statement not member access
        [Fact]
        public void TrailingKindIncorrect8()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  second statement not member access
        [Fact]
        public void TrailingKindIncorrect9()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  no condition
        [Fact]
        public void TrailingKindIncorrect10()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if ()
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  incorrect statement is next statement
        [Fact]
        public void TrailingKindIncorrect11()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                    }
                }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statements within if statement
        [Fact]
        public void TrailingKind12()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia == SyntaxKind.WhitespaceTrivia)
                        {
                            var one = 1;
                            one++;
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaKindCheckIncorrect,
                Message = s_trailingTriviaKindCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                        var one = 1;
                            one++;
                    }
                }
                }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region WhitespaceCheckMissing

        private const string s_whitespaceCheckMissingMessage = s_messagePrefix + "Next, check if 'trailingTrivia' is a single whitespace, which is the desired formatting";

        //  no whitespace check
        [Fact]
        public void WhitespaceMissing1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckMissing,
                Message = s_whitespaceCheckMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        // Finally, this statement checks that the trailing trivia is one single space
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region WhitespaceCheckIncorrect

        private const string s_whitespaceCheckIncorrectMessage = s_messagePrefix + "This statement should check to see if 'trailingTrivia' is a single whitespace, which is the desired formatting";

        //  random variable declaration
        [Fact]
        public void WhitespaceIncorrect1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            int one = 1;
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong variable name
        [Fact]
        public void WhitespaceIncorrect2()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (ifKeyword.ToString() == "" "")
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        //  wrong method accessed
        [Fact]
        public void WhitespaceIncorrect3()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.FullSpan == "" "")
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  no member access expression
        [Fact]
        public void WhitespaceIncorrect4()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia == "" "")
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong equals sign
        [Fact]
        public void WhitespaceIncorrect5()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() = "" "")
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  wrong condition
        [Fact]
        public void WhitespaceIncorrect6()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == ""trailingTrivia.ToString()"")
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  empty condition
        [Fact]
        public void WhitespaceIncorrect7()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if ()
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  previous if statement
        [Fact]
        public void WhitespaceIncorrect8()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  next statement
        [Fact]
        public void WhitespaceIncorrect9()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // statements within incorrect if
        [Fact]
        public void WhitespaceIncorrect10()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia == "" "")
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.WhitespaceCheckIncorrect,
                Message = s_whitespaceCheckIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region MissingRule

        private const string s_missingRuleMessage = s_messagePrefix + "The analyzer should have at least one DiagnosticDescriptor rule";

        [Fact]
        public void MissingRule1() // Rule id but no rule
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.MissingRule,
                Message = s_missingRuleMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        // If the analyzer finds an issue, it will report the DiagnosticDescriptor rule
        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id:  ,// The ID here should be the public constant declared above
            title: ""Enter a title for this diagnostic"",
            messageFormat: ""Enter a message to be displayed with this diagnostic"",
            category: ""Enter a category for this diagnostic (e.g. Formatting)"",
            defaultSeverity: default(DiagnosticSeverity),
            isEnabledByDefault: default(bool));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }
        #endregion

        #region ReturnStatementMissing

        private const string s_returnStatementMissingMessage = s_messagePrefix + "Next, since if the code reaches this point the formatting must be correct, return from 'AnalyzeIfStatement'";

        // no return statement
        [Fact]
        public void ReturnMissing1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.ReturnStatementMissing,
                Message = s_returnStatementMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                            // If the analyzer is satisfied that there is only a single whitespace between 'if' and '(', it will return from this method without reporting a diagnostic
                            return;
                        }
                        }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region ReturnStatementIncorrect

        private const string s_returnStatementIncorrectMessage = s_messagePrefix + "This statement should return from 'AnalyzeIfStatement', because reaching this point in the code means that the if-statement being analyzed has the correct spacing";

        //  throw new NotImplementedException statement
        [Fact]
        public void ReturnIncorrect1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.ReturnStatementIncorrect,
                Message = s_returnStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 48, 33) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                            // If the analyzer is satisfied that there is only a single whitespace between 'if' and '(', it will return from this method without reporting a diagnostic
                            return;
                        }
                        }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  return statement not first statement
        [Fact]
        public void ReturnIncorrect2()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                var one = 1;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.ReturnStatementIncorrect,
                Message = s_returnStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 48, 33) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                            // If the analyzer is satisfied that there is only a single whitespace between 'if' and '(', it will return from this method without reporting a diagnostic
                            return;
                            return;
                            }
                        }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        //  next statement
        [Fact]
        public void ReturnIncorrect3()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                var openParen = ifStatement.OpenParenToken;
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.ReturnStatementIncorrect,
                Message = s_returnStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 48, 33) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                            // If the analyzer is satisfied that there is only a single whitespace between 'if' and '(', it will return from this method without reporting a diagnostic
                            return;
                        }
                        }
                    }
                }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        //  statements below
        [Fact]
        public void ReturnIncorrect4()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                var one = 1;
                            }
                        }
                    }
                }
                var openParen = ifStatement.OpenParenToken;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.ReturnStatementIncorrect,
                Message = s_returnStatementIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 48, 33) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                            // If the analyzer is satisfied that there is only a single whitespace between 'if' and '(', it will return from this method without reporting a diagnostic
                            return;
                        }
                        }
                    }
                }
                var openParen = ifStatement.OpenParenToken;
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region AnalysisMethod

        #region MissingAnalysisMethod

        private const string s_missingAnalysisMethodMessage = s_messagePrefix + "The method 'AnalyzeIfStatement' that was registered to perform the analysis is missing";

        [Fact]
        public void MissingAnalysisMethod1() // missing the analysis method called in Initialize
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.MissingAnalysisMethod,
                Message = s_missingAnalysisMethodMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 36, 46) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }
        // This is the method that is registered within Initialize and is called when an IfStatement SyntaxNode is found
        // First, this method analyzes the Syntax Tree. Then, it reports a diagnostic if an error is found
        // In this tutorial, this method will walk through the Syntax Tree seen in IfSyntaxTree.jpg and determine if the if-statement being analyzed has the correct spacing
        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region IncorrectAnalysisAccessibility
        private const string s_incorrectAnalysisAccessibilityMessage = s_messagePrefix + "The 'AnalyzeIfStatement' method should be private";

        [Fact]
        public void IncorrectAnalysisAccessibility1() // analysis method public
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        public void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAnalysisAccessibility,
                Message = s_incorrectAnalysisAccessibilityMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectAnalysisAccessibility2() // analysis method w/o declared accessibility
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

         void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAnalysisAccessibility,
                Message = s_incorrectAnalysisAccessibilityMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 15) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        #endregion

        #region IncorrectAnalysisReturnType
        private const string s_incorrectAnalysisReturnTypeMessage = s_messagePrefix + "The 'AnalyzeIfStatement' method should have a void return type";

        [Fact]
        public void IncorrectAnalysisReturnType1() // analysis method returning incorrect type
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private bool AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAnalysisReturnType,
                Message = s_incorrectAnalysisReturnTypeMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 22) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void IncorrectAnalysisReturnType2() // analysis method without return type explicitly declared
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAnalysisReturnType,
                Message = s_incorrectAnalysisReturnTypeMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 36, 46) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        #endregion

        #region IncorrectAnalysisParameter

        private const string s_incorrectAnalysisParameterMessage = s_messagePrefix + "The 'AnalyzeIfStatement' method should take one parameter of type SyntaxNodeAnalysisContext";

        [Fact]
        public void IncorrectAnalysisParameter1() // analysis method taking incorrect parameter type
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(bool context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAnalysisParameter,
                Message = s_incorrectAnalysisParameterMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 40) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectAnalysisParameter2() // analysis method taking too many parameters
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context, bool boolean)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAnalysisParameter,
                Message = s_incorrectAnalysisParameterMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 40) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncorrectAnalysisParameter3() // analysis method taking no parameters
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement()
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.IncorrectAnalysisParameter,
                Message = s_incorrectAnalysisParameterMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 40) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #endregion

        #region TooManyStatements

        // Trivia check block
        [Fact]
        public void TooManyStatements1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                 return;
                            }
                        }
                    }
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyStatements,
                Message = s_messagePrefix + "This if-block should only have 1 statement(s), which should check the number of trailing trivia on the if-keyword",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // Trivia count block
        [Fact]
        public void TooManyStatements2()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                 return;
                            }
                        }
                        var openParen = ifStatement.OpenParenToken;
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyStatements,
                Message = s_messagePrefix + "This if-block should only have 2 statement(s), which should extract the first trivia of the if-keyword and check its kind",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // Trivia kind check block
        [Fact]
        public void TooManyStatements3()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                 return;
                            }
                            var openParen = ifStatement.OpenParenToken;
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyStatements,
                Message = s_messagePrefix + "This if-block should only have 1 statement(s), which should check if the trivia is a single space",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 44, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // Return block
        [Fact]
        public void TooManyStatements4()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 1)
                    {
                        var trailingTrivia = ifKeyword.TrailingTrivia.First();
                        if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                        {
                            if (trailingTrivia.ToString() == "" "")
                            {
                                return;
                                var openParen = ifStatement.OpenParenToken;
                            }
                        }
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyStatements,
                Message = s_messagePrefix + "This if-block should only have 1 statement(s), which should return from the method",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 29) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // Method declaration (all statements)
        [Fact]
        public void TooManyStatements5()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);
            context.ReportDiagnostic(diagnostic);
            return;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyStatements,
                Message = s_messagePrefix + "This method should only have 10 statement(s), which should walk through the Syntax Tree and check the spacing of the if-statement",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 40, 22) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // Too many get-accessor statements, using one single return line
        [Fact]
        public void TooManyStatements6()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var one = 1;
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyStatements,
                Message = s_messagePrefix + "This get accessor should only have 1 or 2 statement(s), which should create and return an ImmutableArray containing all DiagnosticDescriptors",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 28, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        // Too many get-accessor statements, create array then return
        [Fact]
        public void TooManyStatements7()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""title"",
            messageFormat: ""message"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var array = ImmutableArray.Create(Rule);
                var one = 1;
                return array;
            }
        }

        public override void Initialize(AnalysisContext context)
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TooManyStatements,
                Message = s_messagePrefix + "This get accessor should only have 1 or 2 statement(s), which should create and return an ImmutableArray containing all DiagnosticDescriptors",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 28, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        #endregion

        #region DiagnosticReportMissing

        private const string s_diagnosticReportMissingMessage = s_messagePrefix + "Next, use 'context'.ReportDiagnostic to report the diagnostic that has been created";

        [Fact]
        public void DiagnosticReportMissing1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DiagnosticReportMissing,
                Message = s_diagnosticReportMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 65, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);

            // Sends diagnostic information to the IDE to be shown to the user
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        #endregion

        #region DiagnosticReportIncorrect

        private const string s_diagnosticReportIncorrectMessage = s_messagePrefix + "This statement should use context.ReportDiagnostic to report 'diagnostic'";

        // Incorrect accessor
        [Fact]
        public void DiagnosticReportIncorrect1()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);
            obj.ReportDiagnostic(diagnostic);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DiagnosticReportIncorrect,
                Message = s_diagnosticReportIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 66, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);

            // Sends diagnostic information to the IDE to be shown to the user
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        // Doesn't call ReportDiagnostic
        [Fact]
        public void DiagnosticReportIncorrect2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);
            context.Equals(diagnostic);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DiagnosticReportIncorrect,
                Message = s_diagnosticReportIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 66, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);

            // Sends diagnostic information to the IDE to be shown to the user
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        // Doesn't report diagnostic (does something else)
        [Fact]
        public void DiagnosticReportIncorrect3()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);
            context.ReportDiagnostic(location);
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DiagnosticReportIncorrect,
                Message = s_diagnosticReportIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 66, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);

            // Sends diagnostic information to the IDE to be shown to the user
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        // Simple member access expression as opposed to Invocation expression
        [Fact]
        public void DiagnosticReportIncorrect4()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);
            context.CancellationToken;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.DiagnosticReportIncorrect,
                Message = s_diagnosticReportIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 66, 13) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor spacingRule = new DiagnosticDescriptor(
            id: spacingRuleId,
            title: ""If statement must have a space between 'if' and the boolean expression"",
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"",
            category: ""Syntax"",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(spacingRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifState = (IfStatementSyntax)context.Node;
            var ifKeyword = ifState.IfKeyword;

            if (ifKeyword.HasTrailingTrivia)
            {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                        if (trailingTrivia.ToString() == "" "")
                        {
                            return;
                        }
                    }
                }
            }

            var open = ifState.OpenParenToken;
            var start = ifKeyword.SpanStart;
            var end = open.SpanStart;
            var span = TextSpan.FromBounds(start, end);
            var location = Location.Create(ifState.SyntaxTree, span);
            var diagnostic = Diagnostic.Create(spacingRule, location);

            // Sends diagnostic information to the IDE to be shown to the user
            context.ReportDiagnostic(diagnostic);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        #endregion

        #region TrailingTriviaCountMissing

        private const string s_triviaCountMissingMessage = s_messagePrefix + "Next, check that 'ifKeyword' only has one trailing trivia element";

        [Fact]
        public void TriviaCountMissing1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountMissing,
                Message = s_triviaCountMissingMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 39, 17) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                // Checks that there is only one piece of trailing trivia
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        #endregion

        #region TrailingTriviaCountIncorrect

        private const string s_triviaCountIncorrectMessage = s_messagePrefix + "This statement should check that 'ifKeyword' only has one trailing trivia element";

        // Wrong initial variable accessor
        [Fact]
        public void TriviaCountIncorrect1()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifStatement.TrailingTrivia.Count == 1)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // Doesn't access trailing trivia
        [Fact]
        public void TriviaCountIncorrect2()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.LeadingTrivia.Count == 1)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // Doesn't access the count property
        [Fact]
        public void TriviaCountIncorrect3()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.FullSpan == 1)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // Accesses count method, not property
        [Fact]
        public void TriviaCountIncorrect4()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count() == 1)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        // Doesn't have multiple accessors
        [Fact]
        public void TriviaCountIncorrect5()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.Count == 1)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        // Wrong equals operator
        [Fact]
        public void TriviaIncorrect6()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count = 1)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        // Number 1 is a string
        [Fact]
        public void TriviaCountIncorrect7()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == ""1"")
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        // Not the number 1, but another number
        [Fact]
        public void TriviaCountIncorrect8()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (ifKeyword.TrailingTrivia.Count == 2)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        // Incorrect statement is next statement
        [Fact]
        public void TriviaCountIncorrect9()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    var trailingTrivia = ifKeyword.TrailingTrivia.First();
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        // Incorrect statement is another if-statement
        [Fact]
        public void TriviaCountIncorrect10()
        {
            var test = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                    if (trailingTrivia.Kind() == SyntaxKind.WhitespaceTrivia)
                    {
                    }
                }
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.TrailingTriviaCountIncorrect,
                Message = s_triviaCountIncorrectMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 41, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxNodeAnalyzer
    {
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
        {
            public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If statement must have a space between 'if' and the boolean expression"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(Rule);
                }
            }

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            }

            private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
            {
                var ifStatement = (IfStatementSyntax)context.Node;
                var ifKeyword = ifStatement.IfKeyword;
                if (ifKeyword.HasTrailingTrivia)
                {
                if (ifKeyword.TrailingTrivia.Count == 1)
                {
                }
            }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        #endregion

        #region title, message, category tests

        private const string s_titleMessage = s_messagePrefix + "Please change the title to a string of your choosing";
        private const string s_messageMessage = s_messagePrefix + "Please change the default message to a string of your choosing";
        private const string s_categoryMessage = s_messagePrefix + "Please change the category to a string of your choosing";

        [Fact]
        public void TitleString()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""Enter a title for this diagnostic"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.Title,
                Message = s_titleMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 20) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""If-statement spacing"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }


        [Fact]
        public void MessageString()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""Enter a ttle for this diagnostic"", // allow any title
            messageFormat: ""Enter a message to be displayed with this diagnostic"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.Message,
                Message = s_messageMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 28) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""Enter a ttle for this diagnostic"", // allow any title
            messageFormat: ""The trivia between 'if' and '(' should be a single space"", // allow any message
            category: ""Syntax"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void CategoryString()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""Enter a title fr this diagnostic"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Enter a category for this diagnostic (e.g. Formatting)"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MetaCompilationAnalyzer.Category,
                Message = s_categoryMessage,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 23, 23) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxNodeAnalyzerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyntaxNodeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string spacingRuleId = ""IfSpacing"";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: spacingRuleId, // make the id specific
            title: ""Enter a title fr this diagnostic"", // allow any title
            messageFormat: ""If statements must contain a space between the 'if' keyword and the boolean expression"", // allow any message
            category: ""Formatting"", // make the category specific
            defaultSeverity: DiagnosticSeverity.Warning, // possible options
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext obj)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }
        #endregion
#endif
    }
}
