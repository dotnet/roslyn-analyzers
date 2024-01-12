// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Roslyn.Diagnostics.CSharp.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotCapturePrimaryConstructorParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RS0103";
        private static readonly LocalizableString Title = "Do not capture primary constructor parameters";
        private static readonly LocalizableString MessageFormat = "Primary constructor parameter '{0}' should not be implicitly captured";
        private static readonly LocalizableString Description = "Primary constructor parameters should not be implicitly captured. Manually assign them to fields at the start of the type.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: false, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(AnalyzeOperation, OperationKind.ParameterReference);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            var operation = (IParameterReferenceOperation)context.Operation;

            if (operation.Parameter.ContainingSymbol == context.ContainingSymbol)
            {
                // We're in the primary constructor itself, so no capture.
                // Or, this isn't a primary constructor parameter at all.
                return;
            }

            IOperation rootOperation = operation;
            for (; rootOperation.Parent != null; rootOperation = rootOperation.Parent)
            {
            }

            if (rootOperation is IPropertyInitializerOperation or IFieldInitializerOperation)
            {
                // This is an explicit capture into member state. That's fine.
                return;
            }

            // This must be a capture. Error
            context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Parameter.Name));
        }
    }
}
