// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Desktop.Analyzers.Common;
using Analyzer.Utilities;

namespace Desktop.Analyzers
{ 
    public abstract class DoNotUseInsecureXSLTScriptExecutionAnalyzer<TLanguageKindEnum> : DiagnosticAnalyzer where TLanguageKindEnum : struct
    {
        internal const string RuleId = "CA3076";
        private const string HelpLink = "http://aka.ms/CA3076";
        internal static DiagnosticDescriptor RuleDoNotUseInsecureXSLTScriptExecution = CreateDiagnosticDescriptor(SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureDTDProcessingGenericMessage)),
                                                                                                                SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureXSLTScriptExecutionDescription)),
                                                                                                                 HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> supportDiagnostics = ImmutableArray.Create(RuleDoNotUseInsecureXSLTScriptExecution);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return DoNotUseInsecureXSLTScriptExecutionAnalyzer<TLanguageKindEnum>.supportDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext analysisContext)
        {

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    var compilation = context.Compilation;
                    var xmlTypes = new CompilationSecurityTypes(compilation);
                    if (xmlTypes.XslCompiledTransform != null)
                    {
                        context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                            (c) =>
                            {
                                GetAnalyzer(c, xmlTypes);
                            });
                    }
                });
        }


        private static DiagnosticDescriptor CreateDiagnosticDescriptor(LocalizableResourceString messageFormat, LocalizableResourceString description, string helpLink = null)
        {
            return new DiagnosticDescriptor(RuleId,
                                            SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureXsltScriptProcessingMessage)),
                                            messageFormat,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
                                            description: description,
                                            helpLinkUri: helpLink,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        } 

        protected abstract Analyzer GetAnalyzer(CodeBlockStartAnalysisContext<TLanguageKindEnum> context, CompilationSecurityTypes types);

        protected sealed class Analyzer
        {
            private readonly CompilationSecurityTypes _xmlTypes;
            private readonly SyntaxNodeHelper _syntaxNodeHelper;

            private readonly Dictionary<ISymbol, XsltSettingsEnvironment> _xsltSettingsEnvironments = new Dictionary<ISymbol, XsltSettingsEnvironment>();

            public Analyzer(CompilationSecurityTypes xmlTypes, SyntaxNodeHelper helper)
            {
                this._xmlTypes = xmlTypes;
                this._syntaxNodeHelper = helper;
            }

            public void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
                AnalyzeNodeForXsltSettings(context);
                AnalyzeNodeForXslCompiledTransformLoad(context);
            }

            private void AnalyzeNodeForXslCompiledTransformLoad(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;
                IMethodSymbol methodSymbol = _syntaxNodeHelper.GetCalleeMethodSymbol(node, model);

                if (SecurityDiagnosticHelpers.IsXslCompiledTransformLoad(methodSymbol, this._xmlTypes))
                {
                    bool isSecureResolver;
                    bool isSecureSettings;
                    bool isSetInBlock;

                    int xmlResolverIndex = SecurityDiagnosticHelpers.HasXmlResolverParameter(methodSymbol, this._xmlTypes);
                    int xsltSettingsIndex = SecurityDiagnosticHelpers.HasXsltSettingsParameter(methodSymbol, this._xmlTypes);

                    // Overloads with no XmlResolver and XstlSettings specified are secure since they all have folowing behavior:
                    //  1. An XmlUrlResolver with no user credentials is used to process any xsl:import or xsl:include elements.
                    //  2. The document() function is disabled.
                    //  3. Embedded scripts are not supported.
                    if (xmlResolverIndex >= 0 &&
                        xsltSettingsIndex >= 0)
                    {
                        IEnumerable<SyntaxNode> argumentExpressionNodes = this._syntaxNodeHelper.GetInvocationArgumentExpressionNodes(node);
                        SyntaxNode resolverNode = argumentExpressionNodes.ElementAt(xmlResolverIndex);

                        isSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(resolverNode, model) ||
                                           SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(resolverNode).Type, this._xmlTypes);


                        SyntaxNode settingsNode = argumentExpressionNodes.ElementAt(xsltSettingsIndex);
                        ISymbol settingsSymbol = SyntaxNodeHelper.GetSymbol(settingsNode, model);
                        XsltSettingsEnvironment env = null;

                        // 1. pass null or XsltSettings.Default as XsltSetting : secure
                        if (settingsSymbol == null || SecurityDiagnosticHelpers.IsXsltSettingsDefaultProperty(settingsSymbol, this._xmlTypes))
                        {
                            isSetInBlock = true;
                            isSecureSettings = true;
                        }
                        // 2. XsltSettings.TrustedXslt : insecure
                        else if (SecurityDiagnosticHelpers.IsXsltSettingsTrustedXsltProperty(settingsSymbol, this._xmlTypes))
                        {
                            isSetInBlock = true;
                            isSecureSettings = false;
                        }
                        // 3. check xsltSettingsEnvironments, if IsScriptDisabled && IsDocumentFunctionDisabled then secure, else insecure
                        else if (this._xsltSettingsEnvironments.TryGetValue(settingsSymbol, out env))
                        {
                            isSetInBlock = false;
                            isSecureSettings = env.IsDocumentFunctionDisabled && env.IsScriptDisabled;
                        }
                        //4. symbol for settings is not found => passed in without any change => assume insecure
                        else
                        {
                            isSetInBlock = true;
                            isSecureSettings = false;
                        }

                        if (!isSecureSettings && !isSecureResolver)
                        {
                            var message = SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                isSetInBlock ? nameof(DesktopAnalyzersResources.XslCompiledTransformLoadInsecureConstructedMessage) :
                                    nameof(DesktopAnalyzersResources.XslCompiledTransformLoadInsecureInputMessage),
                                SecurityDiagnosticHelpers.GetNonEmptyParentName(node, model)
                            );

                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    RuleDoNotUseInsecureXSLTScriptExecution,
                                    node.GetLocation(),
                                    message
                                )
                            ); 
                        }
                    }
                }
            }
            
            private void AnalyzeNodeForXsltSettings(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                SyntaxNode lhs = this._syntaxNodeHelper.GetAssignmentLeftNode(node);
                SyntaxNode rhs = this._syntaxNodeHelper.GetAssignmentRightNode(node);

                if (lhs == null || rhs == null)
                {
                    return;
                }

                ISymbol lhsSymbol = SyntaxNodeHelper.GetSymbol(lhs, model);
                if (lhsSymbol == null)
                {
                    return;
                }

                IMethodSymbol rhsMethodSymbol = _syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                IPropertySymbol rhsPropertySymbol = SyntaxNodeHelper.GetCalleePropertySymbol(rhs, model);

                if (SecurityDiagnosticHelpers.IsXsltSettingsCtor(rhsMethodSymbol, this._xmlTypes))
                {

                    XsltSettingsEnvironment env = new XsltSettingsEnvironment();
                    this._xsltSettingsEnvironments[lhsSymbol] = env;

                    env.XsltSettingsSymbol = lhsSymbol;
                    env.XsltSettingsDefinitionSymbol = rhsMethodSymbol;
                    env.XsltSettingsDefinition = node;
                    env.EnclosingConstructSymbol = this._syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                    //default both properties are disbled
                    env.IsDocumentFunctionDisabled = true;
                    env.IsScriptDisabled = true;

                    // XsltSettings Constructor (Boolean, Boolean)
                    if (rhsMethodSymbol.Parameters.Any())
                    {
                        IEnumerable<SyntaxNode> argumentExpressionNodes = this._syntaxNodeHelper.GetObjectCreationArgumentExpressionNodes(rhs);
                        env.IsDocumentFunctionDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argumentExpressionNodes.ElementAt(0), model);
                        env.IsScriptDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argumentExpressionNodes.ElementAt(1), model);
                    }

                    foreach (SyntaxNode arg in this._syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        var argLhs = this._syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        var argRhs = this._syntaxNodeHelper.GetAssignmentRightNode(arg);

                        var argLhsSymbol = SyntaxNodeHelper.GetSymbol(argLhs, model);

                        // anything other than a constant false is treated as true
                        if (SecurityDiagnosticHelpers.IsXsltSettingsEnableDocumentFunctionProperty(argLhsSymbol, this._xmlTypes))
                        {
                            env.IsDocumentFunctionDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argRhs, model);
                        }
                        else if (SecurityDiagnosticHelpers.IsXsltSettingsEnableScriptProperty(argLhsSymbol, this._xmlTypes))
                        {
                            env.IsScriptDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argRhs, model);
                        }
                    }
                }
                else if (SecurityDiagnosticHelpers.IsXsltSettingsDefaultProperty(rhsPropertySymbol, this._xmlTypes))
                {

                    XsltSettingsEnvironment env = new XsltSettingsEnvironment();
                    this._xsltSettingsEnvironments[lhsSymbol] = env;

                    env.XsltSettingsSymbol = lhsSymbol;
                    env.XsltSettingsDefinitionSymbol = rhsPropertySymbol;
                    env.XsltSettingsDefinition = node;
                    env.EnclosingConstructSymbol = this._syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                    env.IsDocumentFunctionDisabled = true;
                    env.IsScriptDisabled = true;
                }
                else if (SecurityDiagnosticHelpers.IsXsltSettingsTrustedXsltProperty(rhsPropertySymbol, this._xmlTypes))
                {
                    XsltSettingsEnvironment env = new XsltSettingsEnvironment();
                    this._xsltSettingsEnvironments[lhsSymbol] = env;

                    env.XsltSettingsSymbol = lhsSymbol;
                    env.XsltSettingsDefinitionSymbol = rhsPropertySymbol;
                    env.XsltSettingsDefinition = node;
                    env.EnclosingConstructSymbol = this._syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                }
                else
                {
                    bool isXlstSettingsEnableDocumentFunctionProperty = SecurityDiagnosticHelpers.IsXsltSettingsEnableDocumentFunctionProperty(lhsSymbol, this._xmlTypes);
                    bool isXlstSettingsEnableScriptProperty = SecurityDiagnosticHelpers.IsXsltSettingsEnableScriptProperty(lhsSymbol, this._xmlTypes);


                    if (isXlstSettingsEnableDocumentFunctionProperty ||
                        isXlstSettingsEnableScriptProperty)
                    {
                        SyntaxNode lhsExpressionNode = this._syntaxNodeHelper.GetMemberAccessExpressionNode(lhs);
                        if (lhsExpressionNode == null)
                        {
                            return;
                        }

                        ISymbol lhsExpressionSymbol = SyntaxNodeHelper.GetSymbol(lhsExpressionNode, model);
                        if (lhsExpressionSymbol == null)
                        {
                            return;
                        }

                        XsltSettingsEnvironment env = null;
                        if (!this._xsltSettingsEnvironments.TryGetValue(lhsExpressionSymbol, out env))
                        {
                            env = new XsltSettingsEnvironment();
                            env.XsltSettingsSymbol = lhsExpressionSymbol;
                            this._xsltSettingsEnvironments[lhsExpressionSymbol] = env;
                        }

                        var rhsType = model.GetTypeInfo(rhs).Type;

                        if (isXlstSettingsEnableDocumentFunctionProperty)
                        {
                            env.IsDocumentFunctionDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(rhs, model);
                        }
                        else if (isXlstSettingsEnableScriptProperty)
                        {
                            env.IsScriptDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(rhs, model);
                        }
                    }
                }
            }

            private class XsltSettingsEnvironment
            {
                internal ISymbol XsltSettingsSymbol;
                internal ISymbol XsltSettingsDefinitionSymbol;
                internal SyntaxNode XsltSettingsDefinition;
                internal ISymbol EnclosingConstructSymbol;
                internal bool IsScriptDisabled;
                internal bool IsDocumentFunctionDisabled;
            }
        }
    }
}
