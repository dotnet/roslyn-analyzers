using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Analyzers.MetaAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpImmutableObjectMethodAnalyzer : DiagnosticAnalyzer
    {
        // Each analyzer needs a public id to identify each DiagnosticDescriptor and subsequently fix diagnostics in CodeFixProvider.cs
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocationTitle), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocationMessage), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocationDescription), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));

        public static DiagnosticDescriptor DoNotIgnoreReturnValueDiagnosticRule = new DiagnosticDescriptor(
            DiagnosticIds.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocation,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.AnalyzerCorrectness,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: s_localizableDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(DoNotIgnoreReturnValueDiagnosticRule);
            }
        }

        private static readonly string s_solutionFullName = @"Microsoft.CodeAnalysis.Solution";
        private static readonly string s_projectFullName = @"Microsoft.CodeAnalysis.Project";
        private static readonly string s_documentFullName = @"Microsoft.CodeAnalysis.Document";
        private static readonly string s_syntaxNodeFullName = @"Microsoft.CodeAnalysis.SyntaxNode";

        // A list of known immutable object names
        private static readonly List<string> s_immutableObjectNames = new List<string>()
        {
            s_solutionFullName,
            s_projectFullName,
            s_documentFullName,
            s_syntaxNodeFullName,
        };

        private static readonly string s_Add = "Add";
        private static readonly string s_Remove = "Remove";
        private static readonly string s_Replace = "Replace";
        private static readonly string s_With = "With";

        private static readonly List<string> s_immutableMethodNames = new List<string>()
        {
            s_Add,
            s_Remove,
            s_Replace,
            s_With,
        };

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCodeBlockAction(AnalyzeCodeBlockForIgnoredReturnValues);
        }

        public void AnalyzeCodeBlockForIgnoredReturnValues(CodeBlockAnalysisContext context)
        {
            var model = context.SemanticModel;
            var syntaxNode = context.CodeBlock;

            //Find invocations that ignore the return value
            //We're looking for invocations that are not within an equals value clause or a return statement.
            var candidateInvocations = syntaxNode.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(n => n.Parent is StatementSyntax && !(n.Parent is ReturnStatementSyntax));

            foreach (var candidateInvocation in candidateInvocations)
            {
                //If we can't find the method symbol, quit
                var methodSymbol = model.GetSymbolInfo(candidateInvocation).Symbol as IMethodSymbol;
                if (methodSymbol == null)
                    continue;

                //If the method doesn't start with something like "With" or "Replace", quit
                string methodName = methodSymbol.Name;
                if (!s_immutableMethodNames.Any(n => methodName.StartsWith(n)))
                    continue;

                //If we're not in one of the known immutable types, quit
                var parentName = methodSymbol.ReceiverType.ToString();
                var baseTypesAndSelf = methodSymbol.ReceiverType.GetBaseTypes().Select(n => n.ToString()).ToList();
                baseTypesAndSelf.Add(parentName);

                if (!baseTypesAndSelf.Any(n => s_immutableObjectNames.Contains(n)))
                    continue;

                var location = candidateInvocation.GetLocation();
                var diagnostic = Diagnostic.Create(DoNotIgnoreReturnValueDiagnosticRule, location, methodSymbol.ContainingType.Name, methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
