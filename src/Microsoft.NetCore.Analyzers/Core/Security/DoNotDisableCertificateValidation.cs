// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    class DoNotDisableCertificateValidation : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5358";
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
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            
            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    INamedTypeSymbol systemNetSecurityRemoteCertificateValidationCallbackTypeSymbol =
                        compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(WellKnownTypes.SystemNetSecurityRemoteCertificateValidationCallback);
                    
                    if (systemNetSecurityRemoteCertificateValidationCallbackTypeSymbol == null)
                    {
                        return;
                    }
                    
                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            IDelegateCreationOperation delegateCreationOperation =
                                (IDelegateCreationOperation)operationAnalysisContext.Operation;
                            
                            if (delegateCreationOperation.Type == systemNetSecurityRemoteCertificateValidationCallbackTypeSymbol)
                            {
                                bool flag = true;
                                var kind = delegateCreationOperation.Target.Kind;
                                
                                switch (kind)
                                {
                                    case OperationKind.AnonymousFunction:
                                        flag = dealWithAnonmousFunction(delegateCreationOperation.Target);
                                        break;

                                    case OperationKind.MethodReference:
                                        IMethodReferenceOperation methodReferenceOperation = (IMethodReferenceOperation)delegateCreationOperation.Target;
                                        flag = dealWithMethodReference(methodReferenceOperation);
                                        break;
                                }

                                if (!flag)
                                    operationAnalysisContext.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Rule,
                                            delegateCreationOperation.Syntax.GetLocation(),
                                            kind.ToString()));


                            }
                        },
                        OperationKind.DelegateCreation);
                });
        }

        bool dealWithAnonmousFunction(IOperation operation)
        {
            bool flag = false;
            var descendants = operation.Descendants();

            foreach (var tmp in descendants)
            {
                if (tmp.Kind == OperationKind.Return)
                {
                    IReturnOperation a=(IReturnOperation)tmp;
                    bool returnedValue = (bool)a.ReturnedValue.ConstantValue.Value;
                    if (returnedValue == false)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            return flag;
        }

        bool dealWithMethodReference(IMethodReferenceOperation methodReferenceOperation)
        {
            bool flag = false;
            MethodDeclarationSyntax methodDeclarationSyntax = AnalysisGetStatements(methodReferenceOperation.Method) as MethodDeclarationSyntax;
            var returnStatementSyntaxs = methodDeclarationSyntax.DescendantNodes().OfType<ReturnStatementSyntax>();

            foreach (var returnStatementSyntax in returnStatementSyntaxs)
            {
                if (returnStatementSyntax.Expression.ToString() == "false")
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        // Returns a list containing the method declaration, and the statements within the method, returns an empty list if failed
        private MethodDeclarationSyntax AnalysisGetStatements(IMethodSymbol analysisMethodSymbol)
        {
            MethodDeclarationSyntax result = null;

            if (analysisMethodSymbol == null)
            {
                return result;
            }

            var methodDeclaration = analysisMethodSymbol.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax;
            if (methodDeclaration == null)
            {
                return result;
            }

            return methodDeclaration;
        }
    }
}
