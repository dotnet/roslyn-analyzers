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
    /// <summary>
    /// Secure DTD processing and entity resolution in XML
    /// </summary>
    public abstract class CA3075DiagnosticAnalyzer<TLanguageKindEnum> : DiagnosticAnalyzer where TLanguageKindEnum : struct
    {
        internal const string RuleId = "CA3075";

        // Do not use insecure API:

        // Do not use overloads that enable dtd processing, use one takes XmlReader instead.
        // Matches FxCop warnings 3056,3057,3059, 3060, 3062, 3063, 3064, 3070, 3071, 3072
        internal static DiagnosticDescriptor RuleDoNotUseUnsafeDtdProcessingOverloads = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseDtdProcessingOverloadsDiagnosis)),
                                                                                                                  DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseDtdProcessingOverloadsDescription)));
        // Do not use SetInnerXml that enable dtd processing, replace with call to Load.
        // Matches FxCop warnings 3058
        internal static DiagnosticDescriptor RuleDoNotUseSetInnerXml = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseSetInnerXmlDiagnosis)),
                                                                                                  DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseSetInnerXmlDescription)));

        // Do not use unsafe properties that implicitly use DTD procesing. 
        // Matches FxCop warnings 3065
        internal static DiagnosticDescriptor RuleReviewUnsafeDtdProcessingProperties = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.ReviewDtdProcessingPropertiesDiagnosis)),
                                                                                                                  DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.ReviewDtdProcessingPropertiesDescription)));

        // Secure DTD processing on instances of XmlReader, XmlTextReader and XmlDocument:

        // Do not use overloads of XmlReader.Create() that doesn't accept an XmlReaderSettings parameter.
        // Match FxCop waning CA3053 : XmlReaderCreateWrongOverload
        internal static DiagnosticDescriptor RuleXmlReaderCreateWrongOverload = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlReaderCreateWrongOverloadDiagnosis)),
                                                                                                           DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureDescription)));   

        // If XmlReaderSettings is constructed in the code block with insecure value, do not pass it to XmlReader.Create.
        // Doesn't directly match any FxCop rule, related to:
        //      FxCop CA 3053 : XmlReaderCreateInsecureXmlResolver
        //      FxCop CA 3055
        //      FxCop CA 3069
        internal static DiagnosticDescriptor RuleXmlReaderCreateUsingInsecureConstructedXmlReaderSettings = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureConstructedDiagnosis)),
                                                                                                                                       DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureDescription)));

        // If XmlReaderSettings is NOT constructed onsite (i.e. passed in as parameter or a field), do not pass it to XmlReader.Create if DtdProcessing, 
        // XmlResolver and MaxCharactersFromEntities properties are not set to secure values.
        // Doesn't directly match any FxCop rule, related to:
        //      FxCop CA 3053 : XmlReaderCreateInsecureXmlResolver
        //      FxCop CA 3055
        //      FxCop CA 3069
        internal static DiagnosticDescriptor RuleXmlReaderCreateUsingInsecureInputXmlReaderSettings = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureInputDiagnosis)),
                                                                                                                                 DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureDescription))); 

        // Do not create an XmlDocument without setting its XmlResolver property.
        // This DOES NOT trigger when an XmlDocument.XmlResolver is set to any value
        // Partially match FxCop warning CA3053 : XmlDocumentWithNoSecureResolver
        internal static DiagnosticDescriptor RuleXmlDocumentConstructedWithNoSecureResolver = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverDiagnosis)),
                                                                                                              DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlDocumentUseSecureResolverDescription)));

        // Do not explicitly set XmlDocument.XmlResolver property to an insecure value (anything thing other than null or a instance of XmlSecureResolver).
        // This DOES NOT trigger if the XmlDocument instance is created but its XmlResolver property is never explicitly set
        // Partially match FxCop CA3053 CA3053 : XmlDocumentWithNoSecureResolver
        internal static DiagnosticDescriptor RuleXmlDocumentSetInsecureResolver = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverDiagnosis)),
                                                                                                              DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlDocumentUseSecureResolverDescription)));

        // if XmlTextReader is constructed in the method/class as field, must set entity and resource resolution related properties to secure values
        // (using object initialier list in case of field), or use XmlReader.Create() instead
        // i.e. DtdProcessing == Dtdprocessing.Prohibit/Ignore && XmlResolver == XmlSecureResolver/null (no warning if so)    
        // Doesn't directly match any FxCop rule, related to:
        //      FxCop CA 3054
        //      FxCop CA 3069
        internal static DiagnosticDescriptor RuleXmlTextReaderConstructedWithNoSecureResolution = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionDiagnosis)),
                                                                                                                             DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlTextReaderInsecureResolutionDescription)));

        // if XmlTextReader instance is passed in as parameter (or is a field), Do not set entity or resource resolution related properties to insecure values
        // i.e. DtdProcessing == Dtdprocessing.Parse || XmlResolver != XmlSecureResolver/null     
        // Doesn't directly match any FxCop rule, related to:
        //      FxCop CA 3054
        //      FxCop CA 3069
        internal static DiagnosticDescriptor RuleXmlTextReaderSetInsecureResolution = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlTextReaderSetInsecureResolutionDiagnosis)),
                                                                                                                 DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlTextReaderInsecureResolutionDescription)));

        private static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics =ImmutableArray.Create(RuleDoNotUseUnsafeDtdProcessingOverloads,
                                                                                                                 RuleDoNotUseSetInnerXml,
                                                                                                                 RuleReviewUnsafeDtdProcessingProperties,
                                                                                                                 RuleXmlReaderCreateWrongOverload,
                                                                                                                 RuleXmlDocumentConstructedWithNoSecureResolver,
                                                                                                                 RuleXmlDocumentSetInsecureResolver,
                                                                                                                 RuleXmlTextReaderConstructedWithNoSecureResolution,
                                                                                                                 RuleXmlTextReaderSetInsecureResolution,
                                                                                                                 RuleXmlReaderCreateUsingInsecureConstructedXmlReaderSettings,
                                                                                                                 RuleXmlReaderCreateUsingInsecureInputXmlReaderSettings);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return CA3075DiagnosticAnalyzer<TLanguageKindEnum>.supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext analysisContext)
        {

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    var compilation = context.Compilation;
                    var xmlTypes = new CompilationSecurityTypes(compilation);
                    if (ReferencesAnyTargetType(xmlTypes))
                    {
                        Version version = DiagnosticHelpers.GetDotNetFrameworkVersion(compilation);
                        // bail if we are not analyzing project targeting .NET Framework
                        // TODO: should we throw an exception to notify user?
                        if (version != null)
                        {
                            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                                (c) =>
                                {
                                    RegisterAnalyzer(c, xmlTypes, version);
                                });
                        }
                    }
                });
        }

        private static bool ReferencesAnyTargetType(CompilationSecurityTypes types)
        {
            return types.XmlDocument != null
                || types.XmlNode != null
                || types.XmlReader != null
                || types.XmlTextReader != null
                || types.XPathDocument != null
                || types.XmlSchema != null
                || types.DataSet != null
                || types.DataTable != null
                || types.DataViewManager != null
                || types.XmlSerializer != null;
        }

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(LocalizableResourceString messageFormat, LocalizableResourceString description, string helpLink = null)
        {
            return new DiagnosticDescriptor(RuleId,
                                            DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureXmlDtdProcessing)),
                                            messageFormat,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
                                            description: description,
                                            helpLinkUri: helpLink,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }    

        protected abstract void RegisterAnalyzer(CodeBlockStartAnalysisContext<TLanguageKindEnum> context, CompilationSecurityTypes types, Version targetFrameworkVersion);

        protected abstract class Analyzer
        {
            // .NET frameworks >= 4.5.2 have secure default settings
            private static readonly Version minSecureFxVersion = new Version(4, 5, 2);

            private readonly CompilationSecurityTypes xmlTypes;
            private readonly SyntaxNodeHelper syntaxNodeHelper;
            private readonly bool isFrameworkSecure;

            private readonly Dictionary<ISymbol, XmlDocumentEnvironment> xmlDocumentEnvironments = new Dictionary<ISymbol, XmlDocumentEnvironment>();
            private readonly Dictionary<ISymbol, XmlTextReaderEnvironment> xmlTextReaderEnvironments = new Dictionary<ISymbol, XmlTextReaderEnvironment>();
            private readonly Dictionary<ISymbol, XmlReaderSettingsEnvironment> xmlReaderSettingsEnvironments = new Dictionary<ISymbol, XmlReaderSettingsEnvironment>();

            public Analyzer(CompilationSecurityTypes xmlTypes, SyntaxNodeHelper helper, Version targetFrameworkVersion)
            {
                this.xmlTypes = xmlTypes;
                this.syntaxNodeHelper = helper;
                this.isFrameworkSecure = targetFrameworkVersion == null ? false : targetFrameworkVersion >= Analyzer.minSecureFxVersion;
            }
            public void AnalyzeCodeBlockEnd(CodeBlockAnalysisContext context)
            {
                foreach (var p in this.xmlDocumentEnvironments)
                {
                    var env = p.Value;
                    if (!(env.IsXmlResolverSet | env.IsSecureResolver))
                    {
                        Diagnostic diag = Diagnostic.Create(RuleXmlDocumentConstructedWithNoSecureResolver,
                                                            env.XmlDocumentDefinition.GetLocation(),
                                                            env.EnclosingConstructSymbol.ToDisplayString());
                        context.ReportDiagnostic(diag);
                    }
                }


                foreach (var p in this.xmlTextReaderEnvironments)
                {
                    var env = p.Value;
                    if (!(env.IsXmlResolverSet | env.IsSecureResolver) ||
                        !(env.IsDtdProcessingSet | env.IsDtdProcessingDisabled))
                    {
                        Diagnostic diag = Diagnostic.Create(RuleXmlTextReaderConstructedWithNoSecureResolution,
                                                            env.XmlTextReaderDefinition.GetLocation());
                        context.ReportDiagnostic(diag);
                    }
                }
            }

            public void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
                AnalyzeNodeForXmlDocument(context);
                AnalyzeNodeForXmlTextReader(context);
                AnalyzeNodeForXmlReaderSettings(context);
                AnalyzeNodeForDtdProcessingOverloads(context);
                AnalyzeNodeForDtdProcessingProperties(context);
            }

            private void AnalyzeNodeForDtdProcessingOverloads(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                IMethodSymbol method = syntaxNodeHelper.GetCalleeMethodSymbol(node, model);
                if (method == null)
                {
                    return;
                }

                CompilationSecurityTypes xmlTypes = this.xmlTypes;
                if (method.MatchMethodDerived(xmlTypes.XmlDocument, SecurityMemberNames.Load) ||                                    //FxCop CA3056
                    method.MatchMethodDerived(xmlTypes.XmlDocument, SecurityMemberNames.LoadXml) ||                                 //FxCop CA3057
                    method.MatchMethodDerived(xmlTypes.XPathDocument, WellKnownMemberNames.InstanceConstructorName) ||         //FxCop CA3059
                    method.MatchMethodDerived(xmlTypes.XmlSchema, SecurityMemberNames.Read) ||                                      //FxCop CA3060
                    method.MatchMethodDerived(xmlTypes.DataSet, SecurityMemberNames.ReadXml) ||                                     //FxCop CA3063
                    method.MatchMethodDerived(xmlTypes.DataSet, SecurityMemberNames.ReadXmlSchema) ||                               //FxCop CA3064
                    method.MatchMethodDerived(xmlTypes.XmlSerializer, SecurityMemberNames.Deserialize) ||                           //FxCop CA3070
                    method.MatchMethodDerived(xmlTypes.DataTable, SecurityMemberNames.ReadXml) ||                                   //FxCop CA3071
                    method.MatchMethodDerived(xmlTypes.DataTable, SecurityMemberNames.ReadXmlSchema))                               //FxCop CA3072
                {
                    if (SecurityDiagnosticHelpers.HasXmlReaderParameter(method, xmlTypes) < 0)
                    {
                        DiagnosticDescriptor rule = RuleDoNotUseUnsafeDtdProcessingOverloads;
                        context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation(), method.Name));
                    }
                }
                // We assume the design of derived type are secure, per Rule CA9003
                else if (method.MatchMethod(xmlTypes.XmlDocument, WellKnownMemberNames.InstanceConstructorName))
                {
                    if (IsObjectConstructionForTemporaryObject(node))   // REVIEW: may be hard to check
                    {
                        bool isXmlDocumentSecureResolver = false;

                        foreach (SyntaxNode arg in this.syntaxNodeHelper.GetObjectInitializerExpressionNodes(node))
                        {
                            var argLhs = this.syntaxNodeHelper.GetAssignmentLeftNode(arg);
                            var argRhs = this.syntaxNodeHelper.GetAssignmentRightNode(arg);

                            if (SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverProperty(SyntaxNodeHelper.GetSymbol(argLhs, model), xmlTypes))
                            {
                                if (!(SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                    SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes)))
                                {
                                    // if XmlResolver property is explicitly set to an insecure value in initializer list, 
                                    // a warning would be generated when handling assignment of XmlDocument.XmlResolver 
                                    // AnalyzeNodeForXmlDocument method, so we ignore it here.
                                    return;
                                }
                                else
                                {
                                    isXmlDocumentSecureResolver = true;
                                }
                            }
                        }
                        if (!isXmlDocumentSecureResolver)
                        {
                            Diagnostic diag = Diagnostic.Create(RuleXmlDocumentConstructedWithNoSecureResolver,
                                                                node.GetLocation(),
                                                                this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).ToDisplayString());
                            context.ReportDiagnostic(diag);
                        }
                    }
                }
                // We assume the design of derived type are secure, per Rule CA9003                    
                else if (method.MatchMethod(xmlTypes.XmlTextReader, WellKnownMemberNames.InstanceConstructorName))
                {
                    if (IsObjectConstructionForTemporaryObject(node))   // REVIEW: may be hard to check
                    {
                        bool isXmlTextReaderSecureResolver, isXmlTextReaderDtdProcessingDisabled;
                        isXmlTextReaderSecureResolver = isXmlTextReaderDtdProcessingDisabled = false;

                        foreach (SyntaxNode arg in this.syntaxNodeHelper.GetObjectInitializerExpressionNodes(node))
                        {
                            var argLhs = this.syntaxNodeHelper.GetAssignmentLeftNode(arg);
                            var argRhs = this.syntaxNodeHelper.GetAssignmentRightNode(arg);
                            var symArgLhs = SyntaxNodeHelper.GetSymbol(argLhs, model);
                            if (SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverProperty(symArgLhs, xmlTypes))
                            {
                                if (!(SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                    SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes)))
                                {
                                    // Generate a warning whenever the XmlTextReader.XmlResolver property is set to an insecure value
                                    Diagnostic diag = Diagnostic.Create(RuleXmlTextReaderSetInsecureResolution,
                                                                        node.GetLocation(),
                                                                        this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).ToDisplayString());
                                    context.ReportDiagnostic(diag);
                                    return;
                                }
                                else
                                {
                                    isXmlTextReaderSecureResolver = true;
                                }
                            }
                            else if (SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingProperty(symArgLhs, xmlTypes))
                            {
                                if (SyntaxNodeHelper.GetSymbol(argRhs, model).MatchField(xmlTypes.DtdProcessing, SecurityMemberNames.Parse))
                                {
                                    // Generate a warning whenever the XmlTextReader.DtdProcessing property is set to DtdProcessing.Parse
                                    Diagnostic diag = Diagnostic.Create(RuleXmlTextReaderSetInsecureResolution,
                                                                        node.GetLocation(),
                                                                        this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).ToDisplayString());
                                    context.ReportDiagnostic(diag);
                                    return;
                                }
                                else
                                {
                                    isXmlTextReaderDtdProcessingDisabled = true;
                                }
                            }
                        }
                        if (!isXmlTextReaderSecureResolver || !isXmlTextReaderDtdProcessingDisabled)
                        {
                            Diagnostic diag = Diagnostic.Create(RuleXmlTextReaderConstructedWithNoSecureResolution,
                                                                node.GetLocation());
                            context.ReportDiagnostic(diag);
                        }
                    }
                }
                else if (method.MatchMethodDerived(xmlTypes.XmlReader, SecurityMemberNames.Create))
                {
                    int xmlReaderSettingsIndex = SecurityDiagnosticHelpers.HasXmlReaderSettingsParameter(method, xmlTypes);
                    if (xmlReaderSettingsIndex < 0)     //FxCop CA3053:XmlReaderCreateWrongOverload
                    {
                        DiagnosticDescriptor rule = RuleXmlReaderCreateWrongOverload;
                        context.ReportDiagnostic(Diagnostic.Create(rule,
                                                                   node.GetLocation()));
                    }
                    else
                    {
                        SyntaxNode settingsNode = this.syntaxNodeHelper.GetInvocationArgumentExpressionNodes(node).ElementAt(xmlReaderSettingsIndex);
                        ISymbol settingsSymbol = SyntaxNodeHelper.GetSymbol(settingsNode, model);
                        XmlReaderSettingsEnvironment env = null;
                        if (!this.xmlReaderSettingsEnvironments.TryGetValue(settingsSymbol, out env))
                        {
                            // symbol for settings is not found => passed in without any change => assume insecure
                            Diagnostic diag = Diagnostic.Create(RuleXmlReaderCreateUsingInsecureInputXmlReaderSettings,
                                                     node.GetLocation());
                            context.ReportDiagnostic(diag);
                        }
                        else if (!env.IsDtdProcessingDisabled && !(env.IsSecureResolver & env.IsMaxCharactersFromEntitiesLimited))
                        {
                            Diagnostic diag;
                            if (env.IsConstructedInCodeBlock)
                            {
                                diag = Diagnostic.Create(RuleXmlReaderCreateUsingInsecureConstructedXmlReaderSettings,
                                                            node.GetLocation());
                            }
                            else
                            {
                                diag = Diagnostic.Create(RuleXmlReaderCreateUsingInsecureInputXmlReaderSettings,
                                                            node.GetLocation());
                            }
                            context.ReportDiagnostic(diag);
                        }
                    }
                }
            }

            private void AnalyzeNodeForDtdProcessingProperties(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel semanticModel = context.SemanticModel;

                var lhs = syntaxNodeHelper.GetAssignmentLeftNode(node);
                if (lhs == null)
                {
                    return;
                }

                IPropertySymbol property = SyntaxNodeHelper.GetCalleePropertySymbol(lhs, semanticModel);
                if (property == null)
                {
                    return;
                }

                if (property.MatchPropertyDerived(this.xmlTypes.XmlDocument, SecurityMemberNames.InnerXml))                                       //FxCop CA3058
                {
                    DiagnosticDescriptor rule = RuleDoNotUseSetInnerXml;
                    context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation()));
                }
                else if (property.MatchPropertyDerived(this.xmlTypes.DataViewManager, SecurityMemberNames.DataViewSettingCollectionString))   //FxCop CA3065
                {
                    DiagnosticDescriptor rule = RuleReviewUnsafeDtdProcessingProperties;
                    context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation()));
                }
            }

            private void AnalyzeNodeForXmlDocument(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                node = this.syntaxNodeHelper.GetVariableDeclaratorOfAFieldDeclarationNode(node) ?? node;

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

                CompilationSecurityTypes xmlTypes = this.xmlTypes;
                IMethodSymbol rhsMethodSymbol = syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                if (SecurityDiagnosticHelpers.IsXmlDocumentCtorDerived(rhsMethodSymbol, xmlTypes))
                {
                    XmlDocumentEnvironment env = new XmlDocumentEnvironment();

                    if (rhsMethodSymbol.ContainingType != xmlTypes.XmlDocument)
                    {
                        env.IsSecureResolver = true;
                    }

                    foreach (SyntaxNode arg in this.syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        var argLhs = this.syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        var argRhs = this.syntaxNodeHelper.GetAssignmentRightNode(arg);

                        if (SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverPropertyDerived(SyntaxNodeHelper.GetSymbol(argLhs, model), xmlTypes))
                        {
                            env.IsXmlResolverSet = true;
                            if (SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes))
                            {
                                env.IsSecureResolver = true;
                            }
                            break;
                        }
                    }
                    // if XmlResolver property is explicitly set to an insecure value in initializer list, 
                    // a warning would be generated when handling assignment of XmlDocument.XmlResolver in the 
                    // else-if clause below, so we ignore it here.
                    // Only keep track of XmlDocument constructed here when if is not explicitly set to insecure value. 
                    if (!env.IsXmlResolverSet | env.IsSecureResolver)
                    {
                        env.XmlDocumentDefinition = node;
                        env.EnclosingConstructSymbol = this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                        this.xmlDocumentEnvironments[lhsSymbol] = env;
                    }
                }
                else if (SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverPropertyDerived(lhsSymbol, xmlTypes))
                {
                    SyntaxNode lhsExpressionNode = this.syntaxNodeHelper.GetMemberAccessExpressionNode(lhs) ?? lhs;
                    if (lhsExpressionNode == null)
                    {
                        return;
                    }

                    ISymbol lhsExpressionSymbol = SyntaxNodeHelper.GetSymbol(lhsExpressionNode, model);
                    if (lhsExpressionSymbol == null)
                    {
                        return;
                    }

                    XmlDocumentEnvironment env = null;
                    this.xmlDocumentEnvironments.TryGetValue(lhsExpressionSymbol, out env);

                    var rhsType = model.GetTypeInfo(rhs).Type;
                    // if XmlDocument was constructed in the same code block with default values.
                    if (env != null)
                    {
                        env.IsXmlResolverSet = true;
                    }

                    if (SyntaxNodeHelper.NodeHasConstantValueNull(rhs, model) ||
                        SecurityDiagnosticHelpers.IsXmlSecureResolverType(rhsType, xmlTypes))
                    {
                        if (env != null)
                        {
                            env.IsSecureResolver = true;
                        }
                    }
                    else
                    {
                        // Generate a warning whenever the XmlResolver property is set to an insecure value
                        Diagnostic diag = Diagnostic.Create(RuleXmlDocumentSetInsecureResolver,
                                                            node.GetLocation(),
                                                            this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).ToDisplayString());
                        context.ReportDiagnostic(diag);
                    }
                }
            }


            //Note: False negative if integer is used to set DtdProcessing instead of enumeration
            private void AnalyzeNodeForXmlTextReader(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                node = this.syntaxNodeHelper.GetVariableDeclaratorOfAFieldDeclarationNode(node) ?? node;

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

                CompilationSecurityTypes xmlTypes = this.xmlTypes;
                IMethodSymbol rhsMethodSymbol = syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                if (SecurityDiagnosticHelpers.IsXmlTextReaderCtorDerived(rhsMethodSymbol, xmlTypes))
                {
                    XmlTextReaderEnvironment env = null;
                    if (!this.xmlTextReaderEnvironments.TryGetValue(lhsSymbol, out env))
                    {
                        env = new XmlTextReaderEnvironment(this.isFrameworkSecure);
                    }

                    if (rhsMethodSymbol.ContainingType != xmlTypes.XmlTextReader)
                    {
                        env.IsDtdProcessingDisabled = true;
                        env.IsSecureResolver = true;
                    }

                    foreach (SyntaxNode arg in this.syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        var argLhs = this.syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        var argRhs = this.syntaxNodeHelper.GetAssignmentRightNode(arg);

                        if (SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverPropertyDerived(SyntaxNodeHelper.GetSymbol(argLhs, model), xmlTypes))
                        {
                            env.IsXmlResolverSet = true;
                            env.IsSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes);
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingPropertyDerived(SyntaxNodeHelper.GetSymbol(argLhs, model), xmlTypes))
                        {
                            env.IsDtdProcessingSet = true;
                            env.IsDtdProcessingDisabled = !SyntaxNodeHelper.GetSymbol(argRhs, model).MatchField(xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
                        }
                    }
                    // if the XmlResolver or Dtdprocessing property is explicitly set when created, and is to an insecure value, generate a warning
                    if ((env.IsXmlResolverSet & !env.IsSecureResolver) ||
                        (env.IsDtdProcessingSet & !env.IsDtdProcessingDisabled))
                    {
                        Diagnostic diag = Diagnostic.Create(RuleXmlTextReaderSetInsecureResolution,
                                                            node.GetLocation(),
                                                            this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).ToDisplayString());
                        context.ReportDiagnostic(diag);
                    }
                    // if the XmlResolver or Dtdprocessing property is not explicitly set when constructed for XmlTextReader type, add env to the dictionary.
                    else if (!(env.IsDtdProcessingSet & env.IsXmlResolverSet) && (rhsMethodSymbol.ContainingType == xmlTypes.XmlTextReader))
                    {
                        env.XmlTextReaderDefinition = node;
                        env.EnclosingConstructSymbol = this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                        this.xmlTextReaderEnvironments[lhsSymbol] = env;
                    }
                }
                else if (lhsSymbol.Kind == SymbolKind.Property)
                {
                    bool isXmlTextReaderXmlResolverProperty = SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverPropertyDerived(lhsSymbol, xmlTypes);
                    bool isXmlTextReaderDtdProcessingProperty = !isXmlTextReaderXmlResolverProperty &&
                                                              SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingPropertyDerived(lhsSymbol, xmlTypes);

                    if (isXmlTextReaderXmlResolverProperty | isXmlTextReaderDtdProcessingProperty)
                    {
                        // unlike XmlDocument, we already generate a warning for this scenario:
                        //      var doc = new XmlTextReader(path){XmlResolver = new XmlUrlResolver()};
                        // therefore we only need to check property setting in the form of:
                        //      xmlTextReaderObject.XmlResolver = new XmlUrlResolver();
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

                        XmlTextReaderEnvironment env = null;
                        this.xmlTextReaderEnvironments.TryGetValue(lhsExpressionSymbol, out env);

                        var rhsType = model.GetTypeInfo(rhs).Type;

                        // if the XmlTextReader object was constructed with default values
                        if (env != null)
                        {
                            if (isXmlTextReaderXmlResolverProperty)
                            {
                                env.IsXmlResolverSet = true;
                            }
                            else
                            {
                                env.IsDtdProcessingSet = true;
                            }
                        }

                        if (isXmlTextReaderXmlResolverProperty &&
                            (SyntaxNodeHelper.NodeHasConstantValueNull(rhs, model) ||
                             SecurityDiagnosticHelpers.IsXmlSecureResolverType(rhsType, xmlTypes)))
                        {
                            if (env != null)
                            {
                                env.IsSecureResolver = true;
                            }
                        }
                        else if (isXmlTextReaderDtdProcessingProperty &&
                                 !SyntaxNodeHelper.GetSymbol(rhs, model).MatchField(xmlTypes.DtdProcessing, SecurityMemberNames.Parse))
                        {
                            if (env != null)
                            {
                                env.IsDtdProcessingDisabled = true;
                            }
                        }
                        else
                        {
                            // Generate a warning whenever the XmlResolver or DtdProcessing property is set to an insecure value
                            Diagnostic diag = Diagnostic.Create(RuleXmlTextReaderSetInsecureResolution,
                                                                node.GetLocation(),
                                                                this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).ToDisplayString());
                            context.ReportDiagnostic(diag);
                        }
                    }
                }
            }

            //Note: False negative if integer is used to set DtdProcessing instead of enumeration
            private void AnalyzeNodeForXmlReaderSettings(SyntaxNodeAnalysisContext context)
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

                CompilationSecurityTypes xmlTypes = this.xmlTypes;
                IMethodSymbol rhsMethodSymbol = syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                if (SecurityDiagnosticHelpers.IsXmlReaderSettingsCtor(rhsMethodSymbol, xmlTypes))
                {
                    XmlReaderSettingsEnvironment env = new XmlReaderSettingsEnvironment(this.isFrameworkSecure);
                    this.xmlReaderSettingsEnvironments[lhsSymbol] = env;

                    env.XmlReaderSettingsDefinition = node;
                    env.EnclosingConstructSymbol = this.syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);

                    foreach (SyntaxNode arg in this.syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        var argLhs = this.syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        var argRhs = this.syntaxNodeHelper.GetAssignmentRightNode(arg);

                        var argLhsSymbol = SyntaxNodeHelper.GetSymbol(argLhs, model);

                        if (SecurityDiagnosticHelpers.IsXmlReaderSettingsXmlResolverProperty(argLhsSymbol, xmlTypes))
                        {
                            env.IsSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes);
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsDtdProcessingProperty(argLhsSymbol, xmlTypes))
                        {
                            // since the default is always Prohibit, we only need update if it is set to Parse
                            if (SyntaxNodeHelper.GetSymbol(argRhs, model).MatchField(xmlTypes.DtdProcessing, SecurityMemberNames.Parse))
                            {
                                env.IsDtdProcessingDisabled = false;
                            }
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsMaxCharactersFromEntitiesProperty(argLhsSymbol, xmlTypes))
                        {
                            env.IsMaxCharactersFromEntitiesLimited = !SyntaxNodeHelper.NodeHasConstantValueIntZero(argRhs, model);
                        }
                    }
                }
                else if (lhsSymbol.Kind == SymbolKind.Property)
                {
                    bool isXmlReaderSettingsXmlResolverProperty = SecurityDiagnosticHelpers.IsXmlReaderSettingsXmlResolverProperty(lhsSymbol, xmlTypes);
                    bool isXmlReaderSettingsDtdProcessingProperty = !isXmlReaderSettingsXmlResolverProperty &&
                                                                  SecurityDiagnosticHelpers.IsXmlReaderSettingsDtdProcessingProperty(lhsSymbol, xmlTypes);
                    bool isXmlReaderSettingsMaxCharactersFromEntitiesProperty =
                        !(isXmlReaderSettingsXmlResolverProperty | isXmlReaderSettingsDtdProcessingProperty) &&
                        SecurityDiagnosticHelpers.IsXmlReaderSettingsMaxCharactersFromEntitiesProperty(lhsSymbol, xmlTypes);

                    if (isXmlReaderSettingsXmlResolverProperty |
                        isXmlReaderSettingsDtdProcessingProperty |
                        isXmlReaderSettingsMaxCharactersFromEntitiesProperty)
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

                        XmlReaderSettingsEnvironment env = null;
                        if (!this.xmlReaderSettingsEnvironments.TryGetValue(lhsExpressionSymbol, out env))
                        {
                            // env.IsConstructedInCodeBlock is false
                            env = new XmlReaderSettingsEnvironment();
                            this.xmlReaderSettingsEnvironments[lhsExpressionSymbol] = env;
                        }

                        var rhsType = model.GetTypeInfo(rhs).Type;

                        if (isXmlReaderSettingsXmlResolverProperty)
                        {
                            env.IsSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(rhs, model) ||
                                                   SecurityDiagnosticHelpers.IsXmlSecureResolverType(rhsType, xmlTypes);
                        }
                        else if (isXmlReaderSettingsDtdProcessingProperty)
                        {
                            env.IsDtdProcessingDisabled = !SyntaxNodeHelper.GetSymbol(rhs, model).MatchField(xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
                        }
                        else
                        {
                            env.IsMaxCharactersFromEntitiesLimited = !SyntaxNodeHelper.NodeHasConstantValueIntZero(rhs, model);
                        }
                    }
                }
            }

            protected abstract bool IsObjectConstructionForTemporaryObject(SyntaxNode node);

            private class XmlDocumentEnvironment
            {
                internal SyntaxNode XmlDocumentDefinition;
                internal ISymbol EnclosingConstructSymbol;
                internal bool IsXmlResolverSet;
                internal bool IsSecureResolver;
            }

            private class XmlTextReaderEnvironment
            {
                internal SyntaxNode XmlTextReaderDefinition;
                internal ISymbol EnclosingConstructSymbol;
                internal bool IsDtdProcessingSet;
                internal bool IsDtdProcessingDisabled;
                internal bool IsXmlResolverSet;
                internal bool IsSecureResolver;
                internal XmlTextReaderEnvironment(bool isTargetFrameworkSecure)
                {
                    // for .NET framework >= 4.5.2, the default value for XmlResolver property is null
                    if (isTargetFrameworkSecure)
                    {
                        this.IsSecureResolver = true;
                    }
                }
            }

            private class XmlReaderSettingsEnvironment
            {
                internal SyntaxNode XmlReaderSettingsDefinition;  // REVIEW: remove?
                internal ISymbol EnclosingConstructSymbol;        // REVIEW: remove?
                internal bool IsDtdProcessingDisabled;
                internal bool IsMaxCharactersFromEntitiesLimited;
                internal bool IsSecureResolver;
                internal bool IsConstructedInCodeBlock;

                // this constructor is used for keep track of XmlReaderSettings not created in the code block
                internal XmlReaderSettingsEnvironment() { }

                // this constructor is used for keep track of XmlReaderSettings craeted in the code block
                internal XmlReaderSettingsEnvironment(bool isTargetFrameworkSecure)
                {
                    this.IsConstructedInCodeBlock = true;
                    this.IsDtdProcessingDisabled = true;
                    // for .NET framework >= 4.5.2, the default value for XmlResolver property is null
                    if (isTargetFrameworkSecure)
                    {
                        this.IsSecureResolver = true;
                        this.IsMaxCharactersFromEntitiesLimited = true;
                    }
                }
            }
        }
    }
}
