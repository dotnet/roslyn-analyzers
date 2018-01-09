﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA2214: Do not call overridable methods in constructors
    /// 
    /// Cause: The constructor of an unsealed type calls a virtual method defined in its class.
    /// 
    /// Description: When a virtual method is called, the actual type that executes the method is not selected 
    /// until run time. When a constructor calls a virtual method, it is possible that the constructor for the 
    /// instance that invokes the method has not executed. 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCallOverridableMethodsInConstructorsAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA2214";
        private static readonly LocalizableString s_localizableMessageAndTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.DoNotCallOverridableMethodsInConstructors), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.DoNotCallOverridableMethodsInConstructorsDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableMessageAndTitle,
                                                                         s_localizableMessageAndTitle,
                                                                         DiagnosticCategory.Usage,
                                                                         DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                         isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                         description: s_localizableDescription,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182331.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX ? ImmutableArray.Create(Rule) : ImmutableArray<DiagnosticDescriptor>.Empty;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol webUiControlType = compilationContext.Compilation.GetTypeByMetadataName("System.Web.UI.Control");
                INamedTypeSymbol componentModelComponentType = compilationContext.Compilation.GetTypeByMetadataName("System.ComponentModel.Component");

                compilationContext.RegisterOperationBlockStartAction(context =>
                {
                    if (ShouldOmitThisDiagnostic(context.OwningSymbol, webUiControlType, componentModelComponentType))
                    {
                        return;
                    }

                    context.RegisterOperationAction(oc => AnalyzeOperation(oc, context.OwningSymbol.ContainingType), OperationKind.Invocation);
                });
            });
        }

        private static void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol containingType)
        {
            var operation = context.Operation as IInvocationOperation;
            IMethodSymbol method = operation.TargetMethod;
            if (method != null &&
                (method.IsAbstract || method.IsVirtual) &&
                method.ContainingType == containingType)
            {
                context.ReportDiagnostic(operation.Syntax.CreateDiagnostic(Rule));
            }
        }

        private static bool ShouldOmitThisDiagnostic(ISymbol symbol, INamedTypeSymbol webUiControlType, INamedTypeSymbol componentModelComponentType)
        {
            // This diagnostic is only relevant in constructors.
            // TODO: should this apply to instance field initializers for VB?
            var m = symbol as IMethodSymbol;
            if (m == null || m.MethodKind != MethodKind.Constructor)
            {
                return true;
            }

            INamedTypeSymbol containingType = m.ContainingType;
            if (containingType == null)
            {
                return true;
            }

            // special case ASP.NET and Components constructors
            if (containingType.Inherits(webUiControlType))
            {
                return true;
            }

            if (containingType.Inherits(componentModelComponentType))
            {
                return true;
            }

            return false;
        }
    }
}