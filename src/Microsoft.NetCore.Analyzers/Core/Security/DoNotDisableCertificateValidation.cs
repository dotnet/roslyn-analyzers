// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    class DoNotDisableCertificateValidation : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5359";
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableCertificateValidation),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableCertificateValidationMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableCertificateValidationDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor Rule =
            CreateDiagnosticDescriptor(DiagnosticId, Title, Message, Description);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(string ruleId, LocalizableString title, LocalizableString message, LocalizableString description, string uri = null)
        {
            return new DiagnosticDescriptor(
                ruleId,
                title,
                message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: false,
                description: description,
                helpLinkUri: uri,
                customTags: WellKnownDiagnosticTags.Telemetry);
        }

        public sealed override void Initialize(AnalysisContext context)
        {
           // context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            
            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var systemNetSecurityRemoteCertificateValidationCallbackTypeSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(
                            WellKnownTypes.SystemNetSecurityRemoteCertificateValidationCallback);
                    if (systemNetSecurityRemoteCertificateValidationCallbackTypeSymbol == null)
                    {
                        return;
                    }
                    
                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            var delegateCreationOperation =
                                (IDelegateCreationOperation)operationAnalysisContext.Operation;
                            if (systemNetSecurityRemoteCertificateValidationCallbackTypeSymbol.Equals(delegateCreationOperation.Type))
                            {
                                var alwaysReturnTrue = true;

                                var kindOfTargetFunction = delegateCreationOperation.Target.Kind;
                                switch (kindOfTargetFunction)
                                {
                                    case OperationKind.AnonymousFunction:
                                        alwaysReturnTrue = AlwaysReturnTrue(delegateCreationOperation.Target.Descendants());
                                        break;

                                    case OperationKind.MethodReference:
                                        var methodReferenceOperation = (IMethodReferenceOperation)delegateCreationOperation.Target;
                                        var methodSymbol = methodReferenceOperation.Method;
                                        var blockOperation = methodSymbol.GetTopmostOperationBlock(compilationStartAnalysisContext.Compilation);
                                        var tmp = ImmutableArray.ToImmutableArray(blockOperation.Descendants()).GetOperations();
                                        alwaysReturnTrue = AlwaysReturnTrue(tmp);
                                        break;
                                }

                                if (alwaysReturnTrue)
                                {
                                    operationAnalysisContext.ReportDiagnostic(
                                        delegateCreationOperation.CreateDiagnostic(
                                            Rule,
                                            kindOfTargetFunction.ToString()));
                                }
                            }
                        },
                        OperationKind.DelegateCreation);
                });
        }

        /// <summary>
        /// Find every IReturnOperation in the method and get the value of return statement to determine if the method always return true.
        /// </summary>
        /// <param name="operation">A method body in the form of a IOperation</param>
        private static bool AlwaysReturnTrue(IEnumerable<IOperation> operations)
        {
            var result = true;
            var countOfReturnStatement = 0;
            
            foreach (var descendant in operations)
            {
                if (descendant.Kind == OperationKind.Return)
                {
                    var returnOperation = (IReturnOperation)descendant;
                    var constantValue = returnOperation.ReturnedValue.ConstantValue;

                    countOfReturnStatement++;

                    // If it invokes another function which is from local or 3rd assembly,
                    // or the value of return statement is false.
                    if (!constantValue.HasValue || constantValue.Value.Equals(false))
                    {
                        result = false;
                        break;
                    }
                }
            }

            if (countOfReturnStatement == 0)
            {
                result = false;
            }

            return result;
        }
    }
}
