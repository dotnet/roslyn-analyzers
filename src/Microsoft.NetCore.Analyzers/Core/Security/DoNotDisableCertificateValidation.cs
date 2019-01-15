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
    public sealed class DoNotDisableCertificateValidation : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5359";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableCertificateValidation),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableCertificateValidationMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableCertificateValidationDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                s_Message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: false,
                description: s_Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTags.Telemetry);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

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
                                var alwaysReturnTrue = false;

                                switch (delegateCreationOperation.Target.Kind)
                                {
                                    case OperationKind.AnonymousFunction:
                                        var delegateTargetFunction = (IAnonymousFunctionOperation)delegateCreationOperation.Target;

                                        if (delegateTargetFunction == null)
                                        {
                                            return;
                                        }

                                        if (delegateTargetFunction.Symbol.ReturnType.SpecialType != SpecialType.System_Boolean)
                                        {
                                            return;
                                        }

                                        alwaysReturnTrue = AlwaysReturnTrue(delegateCreationOperation.Target.Descendants());
                                        break;

                                    case OperationKind.MethodReference:
                                        var methodReferenceOperation = (IMethodReferenceOperation)delegateCreationOperation.Target;

                                        if (methodReferenceOperation == null)
                                        {
                                            return;
                                        }

                                        var methodSymbol = methodReferenceOperation.Method;

                                        if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Boolean)
                                        {
                                            return;
                                        }

                                        var blockOperation = methodSymbol.GetTopmostOperationBlock(compilationStartAnalysisContext.Compilation);

                                        if (blockOperation == null)
                                        {
                                            return;
                                        }

                                        // TODO(LINCHE): This is an issue tracked by #2009. We filter extraneous based on IsImplicit.
                                        var targetOperations = GetFilteredOperations(ImmutableArray.ToImmutableArray(blockOperation.Descendants()));
                                        alwaysReturnTrue = AlwaysReturnTrue(targetOperations);
                                        break;
                                }

                                if (alwaysReturnTrue)
                                {
                                    operationAnalysisContext.ReportDiagnostic(
                                        delegateCreationOperation.CreateDiagnostic(
                                            Rule));
                                }
                            }
                        },
                        OperationKind.DelegateCreation);
                });
        }

        private static IEnumerable<IOperation> GetFilteredOperations(ImmutableArray<IOperation> blockOperations)
            => blockOperations.GetOperations().Where(o => !o.IsImplicit);

        /// <summary>
        /// Find every IReturnOperation in the method and get the value of return statement to determine if the method always return true.
        /// </summary>
        /// <param name="operation">A method body in the form of explicit IOperations</param>
        private static bool AlwaysReturnTrue(IEnumerable<IOperation> operations)
        {
            var hasReturnStatement = false;
            
            foreach (var descendant in operations)
            {
                if (descendant.Kind == OperationKind.Return)
                {
                    var returnOperation = (IReturnOperation)descendant;

                    if (returnOperation.ReturnedValue == null)
                    {
                        return false;
                    }

                    var constantValue = returnOperation.ReturnedValue.ConstantValue;

                    hasReturnStatement = true;

                    // Check if the value being returned is a compile time constant 'true'
                    if (!constantValue.HasValue || constantValue.Value.Equals(false))
                    {
                        return false;
                    }
                }
            }

            return hasReturnStatement;
        }
    }
}
