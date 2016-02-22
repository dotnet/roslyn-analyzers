// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;
using Desktop.Analyzers.Common;
using Analyzer.Utilities;

namespace Desktop.Analyzers
{
    /// <summary>
    /// Secure DTD processing and entity resolution in XML
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseInsecureDTDProcessingAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3075";
        private const string HelpLink = "http://aka.ms/CA3075";
        private static readonly Version s_minSecureFxVersion = new Version(4, 5, 2);

        private CompilationSecurityTypes _xmlTypes;
        private bool _isFrameworkSecure;

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

        private readonly Dictionary<ISymbol, XmlDocumentEnvironment> _xmlDocumentEnvironments = new Dictionary<ISymbol, XmlDocumentEnvironment>();
        private readonly Dictionary<ISymbol, XmlTextReaderEnvironment> _xmlTextReaderEnvironments = new Dictionary<ISymbol, XmlTextReaderEnvironment>();
        private readonly Dictionary<ISymbol, XmlReaderSettingsEnvironment> _xmlReaderSettingsEnvironments = new Dictionary<ISymbol, XmlReaderSettingsEnvironment>();

        // Do not use insecure API:

        internal static DiagnosticDescriptor RuleDoNotUseInsecureDTDProcessing = CreateDiagnosticDescriptor(
                                                                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureDTDProcessingGenericMessage)),
                                                                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureDTDProcessingDescription)),
                                                                                    HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> s_supportedDiagnostics = ImmutableArray.Create(RuleDoNotUseInsecureDTDProcessing);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return s_supportedDiagnostics;
            }
        }


        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    Compilation compilation = context.Compilation;
                    _xmlTypes = new CompilationSecurityTypes(compilation);

                    if (ReferencesAnyTargetType(_xmlTypes))
                    {
                        // context.RegisterSemanticModelAction(semanticModel => { model = semanticModel.SemanticModel; });
                        Version version = SecurityDiagnosticHelpers.GetDotNetFrameworkVersion(compilation);
                        _isFrameworkSecure = version == null ? false : version >= s_minSecureFxVersion;

                        if (version != null)
                        {
                            context.RegisterOperationAction(AnalyzeOperation,
                                OperationKind.InvocationExpression,
                                OperationKind.ObjectCreationExpression,
                                OperationKind.AssignmentExpression,
                                OperationKind.VariableDeclaration
                                );
                            context.RegisterOperationBlockAction(
                                AnalyzeOperationBlock
                                );
                        }
                    }
                });
        }

        public void AnalyzeOperationBlock(OperationBlockAnalysisContext context)
        {

        }

        public void AnalyzeOperation(OperationAnalysisContext context)
        {
            switch (context.Operation.Kind)
            {
                case OperationKind.ObjectCreationExpression:
                    //AnalyzeObjectCreationExpression(context);
                    break;
                case OperationKind.AssignmentExpression:
                    //AnalyzeAssignmentExpression(context);
                    break;
                case OperationKind.VariableDeclaration:
                    AnalyzeVariableDeclaration(context);
                    break;

            }
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
        {
            IVariableDeclaration declare = context.Operation as IVariableDeclaration;

            if(declare.Variable.Type.DerivesFrom(_xmlTypes.XmlDocument))
            {
                if(!_xmlDocumentEnvironments.ContainsKey(declare.Variable))
                {
                    var env = new XmlDocumentEnvironment();
                    _xmlDocumentEnvironments[declare.Variable] = env;
                    env.IsSecureResolver = false;
                    env.IsXmlResolverSet = false;
                    env.XmlDocumentDefinition = declare.Syntax;

                    if(declare.InitialValue.Kind == OperationKind.LiteralExpression)
                    {
                        ILiteralExpression literal = declare.InitialValue as ILiteralExpression;
                        // Null
                    }
                    else if (declare.InitialValue.Kind == OperationKind.ObjectCreationExpression)
                    {
                        IObjectCreationExpression objCreation = declare.InitialValue as IObjectCreationExpression;

                        if (objCreation == null)
                        {
                            return;
                        }

                        if (SecurityDiagnosticHelpers.IsXmlDocumentCtorDerived(objCreation.Constructor, _xmlTypes))
                        {
                            SyntaxNode node = objCreation.Syntax;
                            bool isXmlDocumentSecureResolver = false;

                            foreach (ISymbolInitializer init in objCreation.MemberInitializers)
                            {
                                var propertyInitializer = init as IPropertyInitializer;

                                if (propertyInitializer != null)
                                {
                                    if (propertyInitializer.InitializedProperty.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, "XmlResolver"))
                                    {
                                        IConversionExpression operation = propertyInitializer.Value as IConversionExpression;

                                        if (operation == null)
                                        {
                                            return;
                                        }

                                        if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(propertyInitializer.Value.Type, _xmlTypes))
                                        {
                                            isXmlDocumentSecureResolver = true;
                                        }
                                        else if (operation.Operand as ILiteralExpression != null)
                                        {
                                            ILiteralExpression literal = operation.Operand as ILiteralExpression;

                                            if (literal.ConstantValue.HasValue && literal.ConstantValue.Value == null)
                                            {
                                                isXmlDocumentSecureResolver = true;
                                            }
                                        }
                                        else // Non secure resolvers?
                                        {
                                            IObjectCreationExpression objCreate = operation.Operand as IObjectCreationExpression;

                                            if (objCreate != null)
                                            {
                                                Diagnostic diag = Diagnostic.Create(
                                                    RuleDoNotUseInsecureDTDProcessing,
                                                    propertyInitializer.Syntax.GetLocation(),
                                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                                        nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                                                    )
                                                );
                                                context.ReportDiagnostic(diag);
                                            }

                                            return;
                                        }

                                        break;
                                    }
                                }
                            }
                            
                            env.IsSecureResolver = isXmlDocumentSecureResolver;
                        }
                    }
                }   
            }
        }

        private void AnalyzeAssignmentExpression(OperationAnalysisContext context)
        {
            IAssignmentExpression expression = context.Operation as IAssignmentExpression;
            SemanticModel model = context.Compilation.GetSemanticModel(expression.Syntax.SyntaxTree);

            if(expression.Target == null)
            {
                return;
            }

            var propRef = expression.Target as IPropertyReferenceExpression;

            if(propRef == null)
            {
                return;
            }

            if (propRef.Property.MatchPropertyByName(_xmlTypes.XmlDocument, "XmlResolver") &&
                expression.Target.Type.DerivesFrom(_xmlTypes.XmlResolver))
            {
                if(_xmlDocumentEnvironments.ContainsKey(propRef.Instance.))
            }
        }

        private void AnalyzePropertyInitializerExpression(OperationAnalysisContext context)
        {
            IPropertyInitializer propertyInitializer = context.Operation as IPropertyInitializer;

            if (propertyInitializer != null)
            {
                if (propertyInitializer.InitializedProperty.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, "XmlResolver"))
                {
                    if (
                        !propertyInitializer.ConstantValue.HasValue &&
                        SecurityDiagnosticHelpers.IsXmlSecureResolverType(propertyInitializer.Value.Type, _xmlTypes)
                     )
                    {
                        Diagnostic diag = Diagnostic.Create(RuleDoNotUseInsecureDTDProcessing, propertyInitializer.Syntax.GetLocation());
                        context.ReportDiagnostic(diag);
                    }
                }
            }
        }

        private void AnalyzeObjectCreationExpression(OperationAnalysisContext context)
        {
            IObjectCreationExpression expression = context.Operation as IObjectCreationExpression;

            if (expression == null)
            {
                return;
            }

            if(SecurityDiagnosticHelpers.IsXmlDocumentCtorDerived(expression.Constructor, _xmlTypes))
            {
                SyntaxNode node = expression.Syntax;
                bool isXmlDocumentSecureResolver = false;

                foreach (ISymbolInitializer init in expression.MemberInitializers)
                {
                    var propertyInitializer = init as IPropertyInitializer;

                    if(propertyInitializer != null)
                    {
                        if (propertyInitializer.InitializedProperty.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, "XmlResolver"))
                        {
                            IConversionExpression operation = propertyInitializer.Value as IConversionExpression;

                            if (operation == null)
                            {
                                return;
                            }

                            if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(propertyInitializer.Value.Type, _xmlTypes))
                            {
                                isXmlDocumentSecureResolver = true;
                            }
                            else if(operation.Operand as ILiteralExpression != null)
                            {
                                ILiteralExpression literal = operation.Operand as ILiteralExpression;

                                if(literal.ConstantValue.HasValue && literal.ConstantValue.Value == null)
                                {
                                    isXmlDocumentSecureResolver = true;
                                }
                            }
                            else // Non secure resolvers
                            {
                                IObjectCreationExpression objCreate = operation.Operand as IObjectCreationExpression;

                                if (objCreate != null)
                                {
                                    Diagnostic diag = Diagnostic.Create(
                                        RuleDoNotUseInsecureDTDProcessing,
                                        propertyInitializer.Syntax.GetLocation(),
                                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                            nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                                        )
                                    );
                                    context.ReportDiagnostic(diag);
                                }

                                return;
                            }
                        }
                    }
                }

                if (!isXmlDocumentSecureResolver)
                {
                    Diagnostic diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDTDProcessing,
                                    node.GetLocation(),
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                                    )
                                );
                    context.ReportDiagnostic(diag);
                }
            }
            else if(SecurityDiagnosticHelpers.IsXmlTextReaderCtorDerived(expression.Constructor, _xmlTypes))
            {
                XmlTextReaderEnvironment env = null;
                
                if (!_xmlTextReaderEnvironments.TryGetValue(expression.Constructor, out env))
                {
                    env = new XmlTextReaderEnvironment(_isFrameworkSecure);
                    _xmlTextReaderEnvironments[expression.Constructor] = env;
                }

                if (expression.Type != _xmlTypes.XmlTextReader)
                {
                    env.IsDtdProcessingDisabled = true;
                    env.IsSecureResolver = true;
                }
            }
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
                                            SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureXmlDtdProcessing)),
                                            messageFormat,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
                                            description: description,
                                            helpLinkUri: helpLink,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }

        public void AnalyzeCodeBlockEnd(CodeBlockAnalysisContext context)
        {
            foreach (var p in _xmlDocumentEnvironments)
            {
                var env = p.Value;
                if (!(env.IsXmlResolverSet | env.IsSecureResolver))
                {
                    Diagnostic diag = Diagnostic.Create(RuleDoNotUseInsecureDTDProcessing,
                                                        env.XmlDocumentDefinition.GetLocation(),
                                                        env.EnclosingConstructSymbol.Name);
                    context.ReportDiagnostic(diag);
                }
            }


            foreach (var p in _xmlTextReaderEnvironments)
            {
                var env = p.Value;
                if (!(env.IsXmlResolverSet | env.IsSecureResolver) ||
                    !(env.IsDtdProcessingSet | env.IsDtdProcessingDisabled))
                {
                    Diagnostic diag = Diagnostic.Create(
                        RuleDoNotUseInsecureDTDProcessing,
                        env.XmlTextReaderDefinition.GetLocation(),
                        env.EnclosingConstructSymbol.Name
                    );
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}
