// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Desktop.Analyzers.Common;

namespace Desktop.Analyzers
{ 
    public abstract class CA3076DiagnosticAnalyzer<TLanguageKindEnum> : DiagnosticAnalyzer where TLanguageKindEnum : struct
    {
        internal const string RuleId = "CA3076";

        /*
         * these 3 FxCop rules are removed since they only trigger on secure code:
         *   XslCompiledTransformTransformWrongOverload
         *   XslCompiledTransformTransformInsecureXmlResolver
         *   XslCompiledTransformLoadWrongOverload 
         */

        //TODO: create new strings
        internal static DiagnosticDescriptor RuleXslCompiledTransformLoadInsecureInputSettings = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XslCompiledTransformLoadInsecureInputDiagnosis)),
                                                                                                                         DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureXsltScriptProcessingDescription)));

        internal static DiagnosticDescriptor RuleXslCompiledTransformLoadInsecureConstructedSettings = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XslCompiledTransformLoadInsecureConstructedDiagnosis)),
                                                                                                                          DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureXsltScriptProcessingDescription)));

        private static readonly ImmutableArray<DiagnosticDescriptor> supportDiagnostics = ImmutableArray.Create(RuleXslCompiledTransformLoadInsecureInputSettings,
                                                                                                                RuleXslCompiledTransformLoadInsecureConstructedSettings);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return CA3076DiagnosticAnalyzer<TLanguageKindEnum>.supportDiagnostics;
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
                                            DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureXsltScriptProcessing)),
                                            messageFormat,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
                                            description: description,
                                            helpLinkUri: helpLink,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        } 

        protected abstract Analyzer GetAnalyzer(CodeBlockStartAnalysisContext<TLanguageKindEnum> context, CompilationSecurityTypes types);

        protected class Analyzer
        {
            private readonly CompilationSecurityTypes xmlTypes;
            private readonly SyntaxNodeHelper syntaxNodeHelper;

            private readonly Dictionary<ISymbol, XsltSettingsEnvironment> xsltSettingsEnvironments = new Dictionary<ISymbol, XsltSettingsEnvironment>();

            public Analyzer(CompilationSecurityTypes xmlTypes, SyntaxNodeHelper helper)
            {
                this.xmlTypes = xmlTypes;
                this.syntaxNodeHelper = helper;
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
                IMethodSymbol methodSymbol = syntaxNodeHelper.GetCalleeMethodSymbol(node, model);

                if (SecurityDiagnosticHelpers.IsXslCompiledTransformLoad(methodSymbol, this.xmlTypes))
                {
                    bool isSecureResolver;
                    bool isSecureSettings;
                    bool isSetInBlock;

                    int xmlResolverIndex = SecurityDiagnosticHelpers.HasXmlResolverParameter(methodSymbol, this.xmlTypes);
                    int xsltSettingsIndex = SecurityDiagnosticHelpers.HasXsltSettingsParameter(methodSymbol, this.xmlTypes);

                    // Overloads with no XmlResolver and XstlSettings specified are secure since they all have folowing behavior:
                    //  1. An XmlUrlResolver with no user credentials is used to process any xsl:import or xsl:include elements.
                    //  2. The document() function is disabled.
                    //  3. Embedded scripts are not supported.
                    if (xmlResolverIndex >= 0 &&
                        xsltSettingsIndex >= 0)
                    {
                        IEnumerable<SyntaxNode> argumentExpressionNodes = this.syntaxNodeHelper.GetInvocationArgumentExpressionNodes(node);
                        SyntaxNode resolverNode = argumentExpressionNodes.ElementAt(xmlResolverIndex);

                        isSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(resolverNode, model) ||
                                           SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(resolverNode).Type, this.xmlTypes);


                        SyntaxNode settingsNode = argumentExpressionNodes.ElementAt(xsltSettingsIndex);
                        ISymbol settingsSymbol = SyntaxNodeHelper.GetSymbol(settingsNode, model);
                        XsltSettingsEnvironment env = null;

                        // 1. pass null or XsltSettings.Default as XsltSetting : secure
                        if (settingsSymbol == null || SecurityDiagnosticHelpers.IsXsltSettingsDefaultProperty(settingsSymbol, this.xmlTypes))
                        {
                            isSetInBlock = true;
                            isSecureSettings = true;
                        }
                        // 2. XsltSettings.TrustedXslt : insecure
                        else if (SecurityDiagnosticHelpers.IsXsltSettingsTrustedXsltProperty(settingsSymbol, this.xmlTypes))
                        {
                            isSetInBlock = true;
                            isSecureSettings = false;
                        }
                        // 3. check xsltSettingsEnvironments, if IsScriptDisabled && IsDocumentFunctionDisabled then secure, else insecure
                        else if (this.xsltSettingsEnvironments.TryGetValue(settingsSymbol, out env))
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
                            var rule = isSetInBlock ? 
                                            RuleXslCompiledTransformLoadInsecureConstructedSettings :
                                            RuleXslCompiledTransformLoadInsecureInputSettings;

                            context.ReportDiagnostic(Diagnostic.Create(rule,
                                                                      node.GetLocation())); 
                        }
                    }
                }
            }


            private void AnalyzeNodeForXsltSettings(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                SyntaxNode lhs = this.syntaxNodeHelper.GetAssignmentLeftNode(node);
                SyntaxNode rhs = this.syntaxNodeHelper.GetAssignmentRightNode(node);

                if (lhs == null || rhs == null)
                {
                    return;
                }

                ISymbol lhsSymbol = SyntaxNodeHelper.GetSymbol(lhs, model);
                if (lhsSymbol == null)
                {
                    return;
                }

                IMethodSymbol rhsMethodSymbol = syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                IPropertySymbol rhsPropertySymbol = SyntaxNodeHelper.GetCalleePropertySymbol(rhs, model);

                if (SecurityDiagnosticHelpers.IsXsltSettingsCtor(rhsMethodSymbol, this.xmlTypes))
                {

                    XsltSettingsEnvironment env = new XsltSettingsEnvironment();
                    this.xsltSettingsEnvironments[lhsSymbol] = env;

                    env.XsltSettingsSymbol = lhsSymbol;
                    env.XsltSettingsDefinitionSymbol = rhsMethodSymbol;
                    env.XsltSettingsDefinition = node;
                    env.EnclosingConstructSymbol = this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                    //default both properties are disbled
                    env.IsDocumentFunctionDisabled = true;
                    env.IsScriptDisabled = true;

                    // XsltSettings Constructor (Boolean, Boolean)
                    if (rhsMethodSymbol.Parameters.Any())
                    {
                        IEnumerable<SyntaxNode> argumentExpressionNodes = this.syntaxNodeHelper.GetObjectCreationArgumentExpressionNodes(rhs);
                        env.IsDocumentFunctionDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argumentExpressionNodes.ElementAt(0), model);
                        env.IsScriptDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argumentExpressionNodes.ElementAt(1), model);
                    }

                    foreach (SyntaxNode arg in this.syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        var argLhs = this.syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        var argRhs = this.syntaxNodeHelper.GetAssignmentRightNode(arg);

                        var argLhsSymbol = SyntaxNodeHelper.GetSymbol(argLhs, model);

                        // anything other than a constant false is treated as true
                        if (SecurityDiagnosticHelpers.IsXsltSettingsEnableDocumentFunctionProperty(argLhsSymbol, this.xmlTypes))
                        {
                            env.IsDocumentFunctionDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argRhs, model);
                        }
                        else if (SecurityDiagnosticHelpers.IsXsltSettingsEnableScriptProperty(argLhsSymbol, this.xmlTypes))
                        {
                            env.IsScriptDisabled = SyntaxNodeHelper.NodeHasConstantValueBoolFalse(argRhs, model);
                        }
                    }
                }
                else if (SecurityDiagnosticHelpers.IsXsltSettingsDefaultProperty(rhsPropertySymbol, this.xmlTypes))
                {

                    XsltSettingsEnvironment env = new XsltSettingsEnvironment();
                    this.xsltSettingsEnvironments[lhsSymbol] = env;

                    env.XsltSettingsSymbol = lhsSymbol;
                    env.XsltSettingsDefinitionSymbol = rhsPropertySymbol;
                    env.XsltSettingsDefinition = node;
                    env.EnclosingConstructSymbol = this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                    env.IsDocumentFunctionDisabled = true;
                    env.IsScriptDisabled = true;
                }
                else if (SecurityDiagnosticHelpers.IsXsltSettingsTrustedXsltProperty(rhsPropertySymbol, this.xmlTypes))
                {
                    XsltSettingsEnvironment env = new XsltSettingsEnvironment();
                    this.xsltSettingsEnvironments[lhsSymbol] = env;

                    env.XsltSettingsSymbol = lhsSymbol;
                    env.XsltSettingsDefinitionSymbol = rhsPropertySymbol;
                    env.XsltSettingsDefinition = node;
                    env.EnclosingConstructSymbol = this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                }
                else
                {
                    bool isXlstSettingsEnableDocumentFunctionProperty = SecurityDiagnosticHelpers.IsXsltSettingsEnableDocumentFunctionProperty(lhsSymbol, this.xmlTypes);
                    bool isXlstSettingsEnableScriptProperty = SecurityDiagnosticHelpers.IsXsltSettingsEnableScriptProperty(lhsSymbol, this.xmlTypes);


                    if (isXlstSettingsEnableDocumentFunctionProperty ||
                        isXlstSettingsEnableScriptProperty)
                    {
                        SyntaxNode lhsExpressionNode = this.syntaxNodeHelper.GetMemberAccessExpressionNode(lhs);
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
                        if (!this.xsltSettingsEnvironments.TryGetValue(lhsExpressionSymbol, out env))
                        {
                            env = new XsltSettingsEnvironment();
                            env.XsltSettingsSymbol = lhsExpressionSymbol;
                            this.xsltSettingsEnvironments[lhsExpressionSymbol] = env;
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
