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
    /// <summary>
    /// Secure DTD processing and entity resolution in XML
    /// </summary>
    public abstract class DoNotUseInsecureDTDProcessingAnalyzer<TLanguageKindEnum> : DiagnosticAnalyzer where TLanguageKindEnum : struct
    {
        internal const string RuleId = "CA3075";
        private const string HelpLink = "http://aka.ms/CA3075";
        // Do not use insecure API:

        internal static DiagnosticDescriptor RuleDoNotUseInsecureDTDProcessing = CreateDiagnosticDescriptor(SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureDTDProcessingGenericMessage)),
                                                                                                                  SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureDTDProcessingDescription)),
                                                                                                                  HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> s_supportedDiagnostics = ImmutableArray.Create(RuleDoNotUseInsecureDTDProcessing);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return DoNotUseInsecureDTDProcessingAnalyzer<TLanguageKindEnum>.s_supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    Compilation compilation = context.Compilation;
                    var xmlTypes = new CompilationSecurityTypes(compilation);
                    if (ReferencesAnyTargetType(xmlTypes))
                    {
                        Version version = SecurityDiagnosticHelpers.GetDotNetFrameworkVersion(compilation);
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
                                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                                nameof(DesktopAnalyzersResources.InsecureXmlDtdProcessing)
                                            ),
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
            private static readonly Version s_minSecureFxVersion = new Version(4, 5, 2);

            private readonly CompilationSecurityTypes _xmlTypes;
            private readonly SyntaxNodeHelper _syntaxNodeHelper;
            private readonly bool _isFrameworkSecure;

            private readonly Dictionary<ISymbol, XmlDocumentEnvironment> _xmlDocumentEnvironments = new Dictionary<ISymbol, XmlDocumentEnvironment>();
            private readonly Dictionary<ISymbol, XmlTextReaderEnvironment> _xmlTextReaderEnvironments = new Dictionary<ISymbol, XmlTextReaderEnvironment>();
            private readonly Dictionary<ISymbol, XmlReaderSettingsEnvironment> _xmlReaderSettingsEnvironments = new Dictionary<ISymbol, XmlReaderSettingsEnvironment>();

            public Analyzer(CompilationSecurityTypes xmlTypes, SyntaxNodeHelper helper, Version targetFrameworkVersion)
            {
                _xmlTypes = xmlTypes;
                _syntaxNodeHelper = helper;
                _isFrameworkSecure = targetFrameworkVersion == null ? false : targetFrameworkVersion >= Analyzer.s_minSecureFxVersion;
            }
            public void AnalyzeCodeBlockEnd(CodeBlockAnalysisContext context)
            {
                foreach (KeyValuePair<ISymbol, XmlDocumentEnvironment> p in _xmlDocumentEnvironments)
                {
                    XmlDocumentEnvironment env = p.Value;
                    if (!(env.IsXmlResolverSet | env.IsSecureResolver))
                    {
                        Diagnostic diag = Diagnostic.Create(
                            RuleDoNotUseInsecureDTDProcessing,
                            env.XmlDocumentDefinition.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage),
                                env.EnclosingConstructSymbol.Name));
                        context.ReportDiagnostic(diag);
                    }
                }


                foreach (KeyValuePair<ISymbol, XmlTextReaderEnvironment> p in _xmlTextReaderEnvironments)
                {
                    XmlTextReaderEnvironment env = p.Value;
                    if (!(env.IsXmlResolverSet | env.IsSecureResolver) ||
                        !(env.IsDtdProcessingSet | env.IsDtdProcessingDisabled))
                    {
                        Diagnostic diag = Diagnostic.Create(
                            RuleDoNotUseInsecureDTDProcessing,
                            env.XmlTextReaderDefinition.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionMessage),
                                env.EnclosingConstructSymbol.Name));

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

                IMethodSymbol method = _syntaxNodeHelper.GetCalleeMethodSymbol(node, model);
                if (method == null)
                {
                    return;
                }

                CompilationSecurityTypes xmlTypes = _xmlTypes;
                if (method.MatchMethodDerivedByName(xmlTypes.XmlDocument, SecurityMemberNames.Load) ||                                    //FxCop CA3056
                    method.MatchMethodDerivedByName(xmlTypes.XmlDocument, SecurityMemberNames.LoadXml) ||                                 //FxCop CA3057
                    method.MatchMethodDerivedByName(xmlTypes.XPathDocument, WellKnownMemberNames.InstanceConstructorName) ||         //FxCop CA3059
                    method.MatchMethodDerivedByName(xmlTypes.XmlSchema, SecurityMemberNames.Read) ||                                      //FxCop CA3060
                    method.MatchMethodDerivedByName(xmlTypes.DataSet, SecurityMemberNames.ReadXml) ||                                     //FxCop CA3063
                    method.MatchMethodDerivedByName(xmlTypes.DataSet, SecurityMemberNames.ReadXmlSchema) ||                               //FxCop CA3064
                    method.MatchMethodDerivedByName(xmlTypes.XmlSerializer, SecurityMemberNames.Deserialize) ||                           //FxCop CA3070
                    method.MatchMethodDerivedByName(xmlTypes.DataTable, SecurityMemberNames.ReadXml) ||                                   //FxCop CA3071
                    method.MatchMethodDerivedByName(xmlTypes.DataTable, SecurityMemberNames.ReadXmlSchema))                               //FxCop CA3072
                {
                    if (SecurityDiagnosticHelpers.HasXmlReaderParameter(method, xmlTypes) < 0)
                    {
                        DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                rule,
                                node.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.DoNotUseDtdProcessingOverloadsMessage),
                                    SecurityDiagnosticHelpers.GetNonEmptyParentName(node, model),
                                method.Name)
                            )
                        );
                    }
                }
                // We assume the design of derived type are secure, per Rule CA9003
                else if (method.MatchMethodByName(xmlTypes.XmlDocument, WellKnownMemberNames.InstanceConstructorName))
                {
                    if (IsObjectConstructionForTemporaryObject(node))   // REVIEW: may be hard to check
                    {
                        bool isXmlDocumentSecureResolver = false;

                        foreach (SyntaxNode arg in _syntaxNodeHelper.GetObjectInitializerExpressionNodes(node))
                        {
                            SyntaxNode argLhs = _syntaxNodeHelper.GetAssignmentLeftNode(arg);
                            SyntaxNode argRhs = _syntaxNodeHelper.GetAssignmentRightNode(arg);

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
                            Diagnostic diag = Diagnostic.Create(
                                RuleDoNotUseInsecureDTDProcessing,
                                node.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage),
                                    _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).Name)
                            );
                            context.ReportDiagnostic(diag);
                        }
                    }
                }
                // We assume the design of derived type are secure, per Rule CA9003                    
                else if (method.MatchMethodByName(xmlTypes.XmlTextReader, WellKnownMemberNames.InstanceConstructorName))
                {
                    if (IsObjectConstructionForTemporaryObject(node))   // REVIEW: may be hard to check
                    {
                        bool isXmlTextReaderSecureResolver, isXmlTextReaderDtdProcessingDisabled;
                        isXmlTextReaderSecureResolver = isXmlTextReaderDtdProcessingDisabled = false;

                        foreach (SyntaxNode arg in _syntaxNodeHelper.GetObjectInitializerExpressionNodes(node))
                        {
                            SyntaxNode argLhs = _syntaxNodeHelper.GetAssignmentLeftNode(arg);
                            SyntaxNode argRhs = _syntaxNodeHelper.GetAssignmentRightNode(arg);
                            ISymbol symArgLhs = SyntaxNodeHelper.GetSymbol(argLhs, model);
                            if (SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverProperty(symArgLhs, xmlTypes))
                            {
                                if (!(SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                    SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes)))
                                {
                                    // Generate a warning whenever the XmlTextReader.XmlResolver property is set to an insecure value
                                    Diagnostic diag = Diagnostic.Create(
                                        RuleDoNotUseInsecureDTDProcessing,
                                        node.GetLocation(),
                                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                            nameof(DesktopAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage),
                                            _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).Name
                                        )
                                    );
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
                                if (SyntaxNodeHelper.GetSymbol(argRhs, model).MatchFieldByName(xmlTypes.DtdProcessing, SecurityMemberNames.Parse))
                                {
                                    // Generate a warning whenever the XmlTextReader.DtdProcessing property is set to DtdProcessing.Parse
                                    Diagnostic diag = Diagnostic.Create(
                                        RuleDoNotUseInsecureDTDProcessing,
                                        node.GetLocation(),
                                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                            nameof(DesktopAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage),
                                            _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).Name
                                        )
                                    );
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
                            Diagnostic diag = Diagnostic.Create(
                                RuleDoNotUseInsecureDTDProcessing,
                                node.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionMessage),
                                    SecurityDiagnosticHelpers.GetNonEmptyParentName(node, model)
                                )
                            );
                            context.ReportDiagnostic(diag);
                        }
                    }
                }
                else if (method.MatchMethodDerivedByName(xmlTypes.XmlReader, SecurityMemberNames.Create))
                {
                    int xmlReaderSettingsIndex = SecurityDiagnosticHelpers.HasXmlReaderSettingsParameter(method, xmlTypes);
                    if (xmlReaderSettingsIndex < 0)     //FxCop CA3053:XmlReaderCreateWrongOverload
                    {
                        DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                rule,
                                node.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlReaderCreateWrongOverloadMessage),
                                    SecurityDiagnosticHelpers.GetNonEmptyParentName(node, model)
                                )
                            )
                        );
                    }
                    else
                    {
                        SyntaxNode settingsNode = _syntaxNodeHelper.GetInvocationArgumentExpressionNodes(node).ElementAt(xmlReaderSettingsIndex);
                        ISymbol settingsSymbol = SyntaxNodeHelper.GetSymbol(settingsNode, model);
                        XmlReaderSettingsEnvironment env = null;
                        if (!_xmlReaderSettingsEnvironments.TryGetValue(settingsSymbol, out env))
                        {
                            // symbol for settings is not found => passed in without any change => assume insecure
                            Diagnostic diag = Diagnostic.Create(
                                RuleDoNotUseInsecureDTDProcessing,
                                node.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureInputMessage),
                                    SecurityDiagnosticHelpers.GetNonEmptyParentName(node, model)
                                )
                            );
                            context.ReportDiagnostic(diag);
                        }
                        else if (!env.IsDtdProcessingDisabled && !(env.IsSecureResolver & env.IsMaxCharactersFromEntitiesLimited))
                        {
                            Diagnostic diag;
                            if (env.IsConstructedInCodeBlock)
                            {
                                diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDTDProcessing,
                                    node.GetLocation(),
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureConstructedMessage)
                                    )
                                );
                            }
                            else
                            {
                                diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDTDProcessing,
                                    node.GetLocation(),
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureInputMessage)
                                    )
                                );
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

                SyntaxNode lhs = _syntaxNodeHelper.GetAssignmentLeftNode(node);
                if (lhs == null)
                {
                    return;
                }

                IPropertySymbol property = SyntaxNodeHelper.GetCalleePropertySymbol(lhs, semanticModel);
                if (property == null)
                {
                    return;
                }

                if (property.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, SecurityMemberNames.InnerXml))                                       //FxCop CA3058
                {
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            rule,
                            node.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.DoNotUseSetInnerXmlMessage),
                                SecurityDiagnosticHelpers.GetNonEmptyParentName(node, semanticModel)
                            )
                        )
                    );
                }
                else if (property.MatchPropertyDerivedByName(_xmlTypes.DataViewManager, SecurityMemberNames.DataViewSettingCollectionString))   //FxCop CA3065
                {
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            rule,
                            node.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.ReviewDtdProcessingPropertiesMessage),
                                SecurityDiagnosticHelpers.GetNonEmptyParentName(node, semanticModel)
                            )
                        )
                    );
                }
            }

            private void AnalyzeNodeForXmlDocument(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                node = _syntaxNodeHelper.GetVariableDeclaratorOfAFieldDeclarationNode(node) ?? node;

                SyntaxNode lhs = _syntaxNodeHelper.GetAssignmentLeftNode(node);
                SyntaxNode rhs = _syntaxNodeHelper.GetAssignmentRightNode(node);

                if (lhs == null || rhs == null)
                {
                    return;
                }

                ISymbol lhsSymbol = SyntaxNodeHelper.GetSymbol(lhs, model);
                if (lhsSymbol == null)
                {
                    return;
                }

                CompilationSecurityTypes xmlTypes = _xmlTypes;
                IMethodSymbol rhsMethodSymbol = _syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                if (SecurityDiagnosticHelpers.IsXmlDocumentCtorDerived(rhsMethodSymbol, xmlTypes))
                {
                    XmlDocumentEnvironment env = new XmlDocumentEnvironment();

                    if (rhsMethodSymbol.ContainingType != xmlTypes.XmlDocument)
                    {
                        env.IsSecureResolver = true;
                    }

                    foreach (SyntaxNode arg in _syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        SyntaxNode argLhs = _syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        SyntaxNode argRhs = _syntaxNodeHelper.GetAssignmentRightNode(arg);

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
                        env.EnclosingConstructSymbol = _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                        _xmlDocumentEnvironments[lhsSymbol] = env;
                    }
                }
                else if (SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverPropertyDerived(lhsSymbol, xmlTypes))
                {
                    SyntaxNode lhsExpressionNode = _syntaxNodeHelper.GetMemberAccessExpressionNode(lhs) ?? lhs;
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
                    _xmlDocumentEnvironments.TryGetValue(lhsExpressionSymbol, out env);

                    ITypeSymbol rhsType = model.GetTypeInfo(rhs).Type;
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
                        Diagnostic diag = Diagnostic.Create(
                            RuleDoNotUseInsecureDTDProcessing,
                            node.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage),
                                _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).Name
                            )
                        );
                        context.ReportDiagnostic(diag);
                    }
                }
            }


            //Note: False negative if integer is used to set DtdProcessing instead of enumeration
            private void AnalyzeNodeForXmlTextReader(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                node = _syntaxNodeHelper.GetVariableDeclaratorOfAFieldDeclarationNode(node) ?? node;

                SyntaxNode lhs = _syntaxNodeHelper.GetAssignmentLeftNode(node);
                SyntaxNode rhs = _syntaxNodeHelper.GetAssignmentRightNode(node);

                if (lhs == null || rhs == null)
                {
                    return;
                }

                ISymbol lhsSymbol = SyntaxNodeHelper.GetSymbol(lhs, model);
                if (lhsSymbol == null)
                {
                    return;
                }

                CompilationSecurityTypes xmlTypes = _xmlTypes;
                IMethodSymbol rhsMethodSymbol = _syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                if (SecurityDiagnosticHelpers.IsXmlTextReaderCtorDerived(rhsMethodSymbol, xmlTypes))
                {
                    XmlTextReaderEnvironment env = null;
                    if (!_xmlTextReaderEnvironments.TryGetValue(lhsSymbol, out env))
                    {
                        env = new XmlTextReaderEnvironment(_isFrameworkSecure);
                    }

                    if (rhsMethodSymbol.ContainingType != xmlTypes.XmlTextReader)
                    {
                        env.IsDtdProcessingDisabled = true;
                        env.IsSecureResolver = true;
                    }

                    foreach (SyntaxNode arg in _syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        SyntaxNode argLhs = _syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        SyntaxNode argRhs = _syntaxNodeHelper.GetAssignmentRightNode(arg);

                        if (SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverPropertyDerived(SyntaxNodeHelper.GetSymbol(argLhs, model), xmlTypes))
                        {
                            env.IsXmlResolverSet = true;
                            env.IsSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes);
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingPropertyDerived(SyntaxNodeHelper.GetSymbol(argLhs, model), xmlTypes))
                        {
                            env.IsDtdProcessingSet = true;
                            env.IsDtdProcessingDisabled = !SyntaxNodeHelper.GetSymbol(argRhs, model).MatchFieldByName(xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
                        }
                    }
                    // if the XmlResolver or Dtdprocessing property is explicitly set when created, and is to an insecure value, generate a warning
                    if ((env.IsXmlResolverSet & !env.IsSecureResolver) ||
                        (env.IsDtdProcessingSet & !env.IsDtdProcessingDisabled))
                    {
                        Diagnostic diag = Diagnostic.Create(
                            RuleDoNotUseInsecureDTDProcessing,
                            node.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage),
                                _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).Name
                            )
                        );
                        context.ReportDiagnostic(diag);
                    }
                    // if the XmlResolver or Dtdprocessing property is not explicitly set when constructed for XmlTextReader type, add env to the dictionary.
                    else if (!(env.IsDtdProcessingSet & env.IsXmlResolverSet) && (rhsMethodSymbol.ContainingType == xmlTypes.XmlTextReader))
                    {
                        env.XmlTextReaderDefinition = node;
                        env.EnclosingConstructSymbol = _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);
                        _xmlTextReaderEnvironments[lhsSymbol] = env;
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
                        SyntaxNode lhsExpressionNode = _syntaxNodeHelper.GetMemberAccessExpressionNode(lhs);
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
                        _xmlTextReaderEnvironments.TryGetValue(lhsExpressionSymbol, out env);

                        ITypeSymbol rhsType = model.GetTypeInfo(rhs).Type;

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
                                 !SyntaxNodeHelper.GetSymbol(rhs, model).MatchFieldByName(xmlTypes.DtdProcessing, SecurityMemberNames.Parse))
                        {
                            if (env != null)
                            {
                                env.IsDtdProcessingDisabled = true;
                            }
                        }
                        else
                        {
                            // Generate a warning whenever the XmlResolver or DtdProcessing property is set to an insecure value
                            Diagnostic diag = Diagnostic.Create(
                                RuleDoNotUseInsecureDTDProcessing,
                                node.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage),
                                    _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model).Name
                                )
                            );
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

                SyntaxNode lhs = _syntaxNodeHelper.GetAssignmentLeftNode(node);
                SyntaxNode rhs = _syntaxNodeHelper.GetAssignmentRightNode(node);

                if (lhs == null || rhs == null)
                {
                    return;
                }

                ISymbol lhsSymbol = SyntaxNodeHelper.GetSymbol(lhs, model);
                if (lhsSymbol == null)
                {
                    return;
                }

                CompilationSecurityTypes xmlTypes = _xmlTypes;
                IMethodSymbol rhsMethodSymbol = _syntaxNodeHelper.GetCalleeMethodSymbol(rhs, model);
                if (SecurityDiagnosticHelpers.IsXmlReaderSettingsCtor(rhsMethodSymbol, xmlTypes))
                {
                    XmlReaderSettingsEnvironment env = new XmlReaderSettingsEnvironment(_isFrameworkSecure);
                    _xmlReaderSettingsEnvironments[lhsSymbol] = env;

                    env.XmlReaderSettingsDefinition = node;
                    env.EnclosingConstructSymbol = _syntaxNodeHelper.GetEnclosingConstructSymbol(node, model);

                    foreach (SyntaxNode arg in _syntaxNodeHelper.GetObjectInitializerExpressionNodes(rhs))
                    {
                        SyntaxNode argLhs = _syntaxNodeHelper.GetAssignmentLeftNode(arg);
                        SyntaxNode argRhs = _syntaxNodeHelper.GetAssignmentRightNode(arg);

                        ISymbol argLhsSymbol = SyntaxNodeHelper.GetSymbol(argLhs, model);

                        if (SecurityDiagnosticHelpers.IsXmlReaderSettingsXmlResolverProperty(argLhsSymbol, xmlTypes))
                        {
                            env.IsSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(argRhs, model) ||
                                SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(argRhs).Type, xmlTypes);
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsDtdProcessingProperty(argLhsSymbol, xmlTypes))
                        {
                            // since the default is always Prohibit, we only need update if it is set to Parse
                            if (SyntaxNodeHelper.GetSymbol(argRhs, model).MatchFieldByName(xmlTypes.DtdProcessing, SecurityMemberNames.Parse))
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
                        SyntaxNode lhsExpressionNode = _syntaxNodeHelper.GetMemberAccessExpressionNode(lhs);
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
                        if (!_xmlReaderSettingsEnvironments.TryGetValue(lhsExpressionSymbol, out env))
                        {
                            // env.IsConstructedInCodeBlock is false
                            env = new XmlReaderSettingsEnvironment();
                            _xmlReaderSettingsEnvironments[lhsExpressionSymbol] = env;
                        }

                        ITypeSymbol rhsType = model.GetTypeInfo(rhs).Type;

                        if (isXmlReaderSettingsXmlResolverProperty)
                        {
                            env.IsSecureResolver = SyntaxNodeHelper.NodeHasConstantValueNull(rhs, model) ||
                                                   SecurityDiagnosticHelpers.IsXmlSecureResolverType(rhsType, xmlTypes);
                        }
                        else if (isXmlReaderSettingsDtdProcessingProperty)
                        {
                            env.IsDtdProcessingDisabled = !SyntaxNodeHelper.GetSymbol(rhs, model).MatchFieldByName(xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
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
                internal readonly bool IsConstructedInCodeBlock;

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
