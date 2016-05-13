// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;
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

        private void RegisterAnalyzer(OperationBlockStartAnalysisContext context, CompilationSecurityTypes types, Version frameworkVersion)
        {
            var analyzer = new OperationAnalyzer(types, frameworkVersion);
            context.RegisterOperationAction(
                analyzer.AnalyzeOperation,
                OperationKind.InvocationExpression,
                OperationKind.AssignmentExpression,
                OperationKind.VariableDeclaration,
                OperationKind.ObjectCreationExpression,
                OperationKind.FieldInitializerAtDeclaration
            );
            context.RegisterOperationBlockEndAction(
                analyzer.AnalyzeOperationBlock
            );
        }


        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Make analyzer thread-safe
            //analysisContext.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics in generated code.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    Compilation compilation = context.Compilation;
                    var xmlTypes = new CompilationSecurityTypes(compilation);

                    if (ReferencesAnyTargetType(xmlTypes))
                    {
                        Version version = SecurityDiagnosticHelpers.GetDotNetFrameworkVersion(compilation);

                        if (version != null)
                        {
                            context.RegisterOperationBlockStartAction(
                                (c) =>
                                {
                                    RegisterAnalyzer(c, xmlTypes, version);
                                });
                        }
                    }
                });
        }

        private class OperationAnalyzer
        {
            #region Environment classes
            private class XmlDocumentEnvironment
            {
                internal SyntaxNode XmlDocumentDefinition;
                internal bool IsXmlResolverSet;
                internal bool IsSecureResolver;
            }

            private class XmlTextReaderEnvironment
            {
                internal SyntaxNode XmlTextReaderDefinition;
                internal bool IsDtdProcessingSet;
                internal bool IsDtdProcessingDisabled;
                internal bool IsXmlResolverSet;
                internal bool IsSecureResolver;

                internal XmlTextReaderEnvironment(bool isTargetFrameworkSecure)
                {
                    // for .NET framework >= 4.5.2, the default value for XmlResolver property is null
                    if (isTargetFrameworkSecure)
                    {
                        IsSecureResolver = true;
                    }
                }
            }

            private class XmlReaderSettingsEnvironment
            {
                internal SyntaxNode XmlReaderSettingsDefinition;
                internal bool IsDtdProcessingDisabled;
                internal bool IsMaxCharactersFromEntitiesLimited;
                internal bool IsSecureResolver;
                internal bool IsConstructedInCodeBlock;

                // this constructor is used for keep track of XmlReaderSettings not created in the code block
                internal XmlReaderSettingsEnvironment() { }

                // this constructor is used for keep track of XmlReaderSettings craeted in the code block
                internal XmlReaderSettingsEnvironment(bool isTargetFrameworkSecure)
                {
                    IsConstructedInCodeBlock = true;
                    IsDtdProcessingDisabled = true;
                    // for .NET framework >= 4.5.2, the default value for XmlResolver property is null
                    if (isTargetFrameworkSecure)
                    {
                        IsSecureResolver = true;
                        IsMaxCharactersFromEntitiesLimited = true;
                    }
                }
            }
            #endregion

            // .NET frameworks >= 4.5.2 have secure default settings
            private static readonly Version s_minSecureFxVersion = new Version(4, 5, 2);

            private readonly CompilationSecurityTypes _xmlTypes;
            private readonly bool _isFrameworkSecure;
            private readonly HashSet<IOperation> _objectCreationOperationsAnalyzed = new HashSet<IOperation>();
            private readonly Dictionary<ISymbol, XmlDocumentEnvironment> _xmlDocumentEnvironments = new Dictionary<ISymbol, XmlDocumentEnvironment>();
            private readonly Dictionary<ISymbol, XmlTextReaderEnvironment> _xmlTextReaderEnvironments = new Dictionary<ISymbol, XmlTextReaderEnvironment>();
            private readonly Dictionary<ISymbol, XmlReaderSettingsEnvironment> _xmlReaderSettingsEnvironments = new Dictionary<ISymbol, XmlReaderSettingsEnvironment>();

            public OperationAnalyzer(CompilationSecurityTypes xmlTypes, Version targetFrameworkVersion)
            {
                _xmlTypes = xmlTypes;
                _isFrameworkSecure = targetFrameworkVersion == null ? false : targetFrameworkVersion >= s_minSecureFxVersion;
            }

            public void AnalyzeOperationBlock(OperationBlockAnalysisContext context)
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
                                nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                            )
                        );
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
                                nameof(DesktopAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionMessage)
                            )
                        );

                        context.ReportDiagnostic(diag);
                    }
                }
            }


            public void AnalyzeOperation(OperationAnalysisContext context)
            {
                switch (context.Operation.Kind)
                {
                    case OperationKind.ObjectCreationExpression:
                        AnalyzeObjectCreationOperation(context);
                        break;
                    case OperationKind.AssignmentExpression:
                        AnalyzeAssignment(context);
                        break;
                    case OperationKind.FieldInitializerAtDeclaration:
                        AnalyzeFieldDeclaration(context);
                        break;
                    case OperationKind.VariableDeclaration:
                        AnalyzeVariableDeclaration(context);
                        break;
                    case OperationKind.InvocationExpression:
                        AnalyzeInvocation(context);
                        break;
                }
            }

            private void AnalyzeInvocation(OperationAnalysisContext context)
            {
                IInvocationExpression invocationExpression = context.Operation as IInvocationExpression;

                if(invocationExpression == null)
                {
                    return;
                }

                IMethodSymbol method = invocationExpression.TargetMethod;

                if(method == null)
                {
                    return;
                }

                AnalyzeMethodOverloads(context, method, invocationExpression);
            }

            private void AnalyzeMethodOverloads(OperationAnalysisContext context, IMethodSymbol method, IHasArgumentsExpression expression)
            {
                if (method.MatchMethodDerivedByName(_xmlTypes.XmlDocument, SecurityMemberNames.Load) ||                                    //FxCop CA3056
                    method.MatchMethodDerivedByName(_xmlTypes.XmlDocument, SecurityMemberNames.LoadXml) ||                                 //FxCop CA3057
                    method.MatchMethodDerivedByName(_xmlTypes.XPathDocument, WellKnownMemberNames.InstanceConstructorName) ||         //FxCop CA3059
                    method.MatchMethodDerivedByName(_xmlTypes.XmlSchema, SecurityMemberNames.Read) ||                                      //FxCop CA3060
                    method.MatchMethodDerivedByName(_xmlTypes.DataSet, SecurityMemberNames.ReadXml) ||                                     //FxCop CA3063
                    method.MatchMethodDerivedByName(_xmlTypes.DataSet, SecurityMemberNames.ReadXmlSchema) ||                               //FxCop CA3064
                    method.MatchMethodDerivedByName(_xmlTypes.XmlSerializer, SecurityMemberNames.Deserialize) ||                           //FxCop CA3070
                    method.MatchMethodDerivedByName(_xmlTypes.DataTable, SecurityMemberNames.ReadXml) ||                                   //FxCop CA3071
                    method.MatchMethodDerivedByName(_xmlTypes.DataTable, SecurityMemberNames.ReadXmlSchema))                               //FxCop CA3072
                {
                    if (SecurityDiagnosticHelpers.HasXmlReaderParameter(method, _xmlTypes) < 0)
                    {
                        DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                rule,
                                expression.Syntax.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.DoNotUseDtdProcessingOverloadsMessage),
                                    method.Name
                                )
                            )
                        );
                    }
                }
                else if (method.MatchMethodDerivedByName(_xmlTypes.XmlReader, SecurityMemberNames.Create))
                {
                    int xmlReaderSettingsIndex = SecurityDiagnosticHelpers.GetXmlReaderSettingsParameterIndex(method, _xmlTypes);

                    if (xmlReaderSettingsIndex < 0)
                    {
                        DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                        Diagnostic diag = Diagnostic.Create(
                                RuleDoNotUseInsecureDTDProcessing,
                                expression.Syntax.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlReaderCreateWrongOverloadMessage)
                                )
                            );
                        context.ReportDiagnostic(diag);
                    }
                    else
                    {
                        SemanticModel model = context.Compilation.GetSemanticModel(context.Operation.Syntax.SyntaxTree);
                        IArgument arg = expression.ArgumentsInParameterOrder[xmlReaderSettingsIndex];
                        ISymbol settingsSymbol = arg.Value.Syntax.GetDeclaredOrReferencedSymbol(model);
                        
                        if(settingsSymbol == null)
                        {
                            return;
                        }

                        XmlReaderSettingsEnvironment env;

                        if (!_xmlReaderSettingsEnvironments.TryGetValue(settingsSymbol, out env))
                        {
                            // symbol for settings is not found => passed in without any change => assume insecure
                            Diagnostic diag = Diagnostic.Create(
                                RuleDoNotUseInsecureDTDProcessing,
                                expression.Syntax.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureInputMessage)
                                )
                            );
                            context.ReportDiagnostic(diag);
                        }
                        else if (!env.IsDtdProcessingDisabled && !(env.IsSecureResolver && env.IsMaxCharactersFromEntitiesLimited))
                        {
                            Diagnostic diag;
                            if (env.IsConstructedInCodeBlock)
                            {
                                diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDTDProcessing,
                                    expression.Syntax.GetLocation(),
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(DesktopAnalyzersResources.XmlReaderCreateInsecureConstructedMessage)
                                    )
                                );
                            }
                            else
                            {
                                diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDTDProcessing,
                                    expression.Syntax.GetLocation(),
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

            private void AnalyzeFieldDeclaration(OperationAnalysisContext context)
            {
                IFieldInitializer fieldInit = context.Operation as IFieldInitializer;

                if(fieldInit == null)
                {
                    return;
                }

                foreach (IFieldSymbol field in fieldInit.InitializedFields)
                {
                    IOperation valueOperation = fieldInit.Value;
                    AnalyzeObjectCreationInternal(context, field, valueOperation);
                }
            }

            private void AnalyzeObjectCreationInternal(OperationAnalysisContext context, ISymbol variable, IOperation value)
            {
                IObjectCreationExpression objCreation = value as IObjectCreationExpression;

                if (objCreation == null)
                {
                    return;
                }

                if (_objectCreationOperationsAnalyzed.Contains(objCreation))
                {
                    return;
                }
                else
                {
                    _objectCreationOperationsAnalyzed.Add(objCreation);
                }

                if (SecurityDiagnosticHelpers.IsXmlDocumentCtorDerived(objCreation.Constructor, _xmlTypes))
                {
                    AnalyzeObjectCreationForXmlDocument(context, variable, objCreation);
                }
                else if (SecurityDiagnosticHelpers.IsXmlTextReaderCtorDerived(objCreation.Constructor, _xmlTypes))
                {
                    AnalyzeObjectCreationForXmlTextReader(context, variable, objCreation);
                }
                else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsCtor(objCreation.Constructor, _xmlTypes))
                {
                    AnalyzeObjectCreationForXmlReaderSettings(variable, objCreation);
                }
                else
                {
                    AnalyzeMethodOverloads(context, objCreation.Constructor, objCreation);
                }
            }

            private void AnalyzeObjectCreationForXmlDocument(OperationAnalysisContext context, ISymbol variable, IObjectCreationExpression objCreation)
            {
                XmlDocumentEnvironment xmlDocumentEnvironment;

                if (variable == null || !_xmlDocumentEnvironments.ContainsKey(variable))
                {
                    xmlDocumentEnvironment = new XmlDocumentEnvironment
                    {
                        IsSecureResolver = false,
                        IsXmlResolverSet = false
                    };
                }
                else
                {
                    xmlDocumentEnvironment = _xmlDocumentEnvironments[variable];
                }

                xmlDocumentEnvironment.XmlDocumentDefinition = objCreation.Syntax;
                SyntaxNode node = objCreation.Syntax;
                bool isXmlDocumentSecureResolver = false;

                if (objCreation.Constructor.ContainingType != _xmlTypes.XmlDocument)
                {
                    isXmlDocumentSecureResolver = true;
                }

                foreach (ISymbolInitializer init in objCreation.MemberInitializers)
                {
                    var prop = init as IPropertyInitializer;

                    if (prop != null)
                    {
                        if (prop.InitializedProperty.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, "XmlResolver"))
                        {
                            IConversionExpression operation = prop.Value as IConversionExpression;

                            if (operation == null)
                            {
                                return;
                            }

                            if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(operation.Operand.Type, _xmlTypes))
                            {
                                isXmlDocumentSecureResolver = true;
                            }
                            else if (SecurityDiagnosticHelpers.IsExpressionEqualsNull(operation.Operand))
                            {
                                isXmlDocumentSecureResolver = true;
                            }
                            else // Non secure resolvers
                            {
                                IObjectCreationExpression xmlResolverObjCreated = operation.Operand as IObjectCreationExpression;

                                if (xmlResolverObjCreated != null)
                                {
                                    Diagnostic diag = Diagnostic.Create(
                                        RuleDoNotUseInsecureDTDProcessing,
                                        prop.Syntax.GetLocation(),
                                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                            nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                                        )
                                    );
                                    context.ReportDiagnostic(diag);
                                }

                                return;
                            }
                        }
                        else
                        {
                            AnalyzeNeverSetProperties(context, prop.InitializedProperty, prop.Syntax.GetLocation());
                        }
                    }
                }


                xmlDocumentEnvironment.IsSecureResolver = isXmlDocumentSecureResolver;

                if (variable != null)
                {
                    _xmlDocumentEnvironments[variable] = xmlDocumentEnvironment;
                }
                else if (!xmlDocumentEnvironment.IsSecureResolver) // Insecure temp object
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

            private void AnalyzeObjectCreationForXmlTextReader(OperationAnalysisContext context, ISymbol variable, IObjectCreationExpression objCreation)
            {
                XmlTextReaderEnvironment env;

                if (variable == null || !_xmlTextReaderEnvironments.TryGetValue(variable, out env))
                {
                    env = new XmlTextReaderEnvironment(_isFrameworkSecure)
                    {
                        XmlTextReaderDefinition = objCreation.Syntax
                    };
                }

                if (objCreation.Constructor.ContainingType != _xmlTypes.XmlTextReader)
                {
                    env.IsDtdProcessingDisabled = true;
                    env.IsSecureResolver = true;
                }

                foreach (ISymbolInitializer init in objCreation.MemberInitializers)
                {
                    var prop = init as IPropertyInitializer;

                    if (prop != null)
                    {
                        IConversionExpression operation = prop.Value as IConversionExpression;

                        if (operation != null && SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverPropertyDerived(prop.InitializedProperty, _xmlTypes))
                        {
                            env.IsXmlResolverSet = true;

                            if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(operation.Operand.Type, _xmlTypes))
                            {
                                env.IsSecureResolver = true;
                            }
                            else if (SecurityDiagnosticHelpers.IsExpressionEqualsNull(operation.Operand))
                            {
                                env.IsSecureResolver = true;
                            }
                            else
                            {
                                env.IsSecureResolver = false;
                            }
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingPropertyDerived(prop.InitializedProperty, _xmlTypes))
                        {
                            env.IsDtdProcessingSet = true;
                            env.IsDtdProcessingDisabled = !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(prop.Value);
                        }
                    }
                }

                // if the XmlResolver or Dtdprocessing property is explicitly set when created, and is to an insecure value, generate a warning
                if ((env.IsXmlResolverSet && !env.IsSecureResolver) ||
                    (env.IsDtdProcessingSet && !env.IsDtdProcessingDisabled))
                {
                    Diagnostic diag = Diagnostic.Create(
                        RuleDoNotUseInsecureDTDProcessing,
                        env.XmlTextReaderDefinition.GetLocation(),
                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                            nameof(DesktopAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage)
                        )
                    );
                    context.ReportDiagnostic(diag);
                }
                // if the XmlResolver or Dtdprocessing property is not explicitly set when constructed for a non-temp XmlTextReader object, add env to the dictionary.
                else if (variable != null && !(env.IsDtdProcessingSet && env.IsXmlResolverSet))
                {
                    _xmlTextReaderEnvironments[variable] = env;
                }
                // if the is not set or set to Parse for a temporary object, report right now.
                else if (variable == null && !(env.IsDtdProcessingSet && env.IsXmlResolverSet && env.IsDtdProcessingDisabled && env.IsSecureResolver))
                {
                    Diagnostic diag = Diagnostic.Create(
                        RuleDoNotUseInsecureDTDProcessing,
                        env.XmlTextReaderDefinition.GetLocation(),
                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                            nameof(DesktopAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionMessage)
                        )
                    );
                    context.ReportDiagnostic(diag);
                }
            }

            private void AnalyzeObjectCreationForXmlReaderSettings(ISymbol variable, IObjectCreationExpression objCreation)
            {
                XmlReaderSettingsEnvironment xmlReaderSettingsEnv = new XmlReaderSettingsEnvironment(_isFrameworkSecure);

                if (variable != null)
                {
                    _xmlReaderSettingsEnvironments[variable] = xmlReaderSettingsEnv;
                }

                xmlReaderSettingsEnv.XmlReaderSettingsDefinition = objCreation.Syntax;
                foreach (ISymbolInitializer init in objCreation.MemberInitializers)
                {
                    var prop = init as IPropertyInitializer;

                    if (prop != null)
                    {
                        if (SecurityDiagnosticHelpers.IsXmlReaderSettingsXmlResolverProperty(
                                prop.InitializedProperty,
                                _xmlTypes)
                            )
                        {
                            IConversionExpression operation = prop.Value as IConversionExpression;

                            if (operation == null)
                            {
                                return;
                            }

                            if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(operation.Operand.Type, _xmlTypes))
                            {
                                xmlReaderSettingsEnv.IsSecureResolver = true;
                            }
                            else if (SecurityDiagnosticHelpers.IsExpressionEqualsNull(operation.Operand))
                            {
                                xmlReaderSettingsEnv.IsSecureResolver = true;
                            }
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsDtdProcessingProperty(prop.InitializedProperty, _xmlTypes))
                        {
                            xmlReaderSettingsEnv.IsDtdProcessingDisabled = !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(prop.Value);
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsMaxCharactersFromEntitiesProperty(prop.InitializedProperty, _xmlTypes))
                        {
                            xmlReaderSettingsEnv.IsMaxCharactersFromEntitiesLimited = !SecurityDiagnosticHelpers.IsExpressionEqualsIntZero(prop.Value);
                        }
                    }
                }
            }

            private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
            {
                IVariableDeclaration declare = context.Operation as IVariableDeclaration;
                AnalyzeObjectCreationInternal(context, declare.Variable, declare.InitialValue);
            }

            private void AnalyzeXmlResolverPropertyAssignmentForXmlDocument(OperationAnalysisContext context, ISymbol assignedSymbol, IAssignmentExpression expression)
            {
                bool isSecureResolver = false;
                IConversionExpression conv = expression.Value as IConversionExpression;

                if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(conv.Operand.Type, _xmlTypes))
                {
                    isSecureResolver = true;
                }
                else if (conv != null && SecurityDiagnosticHelpers.IsExpressionEqualsNull(conv.Operand))
                {
                    isSecureResolver = true;
                }
                else // Assigning XmlDocument's XmlResolver to an insecure value
                {
                    Diagnostic diag = Diagnostic.Create(
                                RuleDoNotUseInsecureDTDProcessing,
                                context.Operation.Syntax.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                                )
                            );
                    context.ReportDiagnostic(diag);
                }

                if (_xmlDocumentEnvironments.ContainsKey(assignedSymbol))
                {
                    XmlDocumentEnvironment xmlDocumentEnv = _xmlDocumentEnvironments[assignedSymbol];
                    xmlDocumentEnv.IsXmlResolverSet = true;
                    xmlDocumentEnv.IsSecureResolver = isSecureResolver;
                }
            }

            private void AnalyzeXmlTextReaderProperties(OperationAnalysisContext context, ISymbol assignedSymbol, IAssignmentExpression expression, bool isXmlTextReaderXmlResolverProperty, bool isXmlTextReaderDtdProcessingProperty)
            {
                XmlTextReaderEnvironment env;

                if (!_xmlTextReaderEnvironments.TryGetValue(assignedSymbol, out env))
                {
                    env = new XmlTextReaderEnvironment(_isFrameworkSecure);
                }

                if (isXmlTextReaderXmlResolverProperty)
                {
                    env.IsXmlResolverSet = true;
                }
                else
                {
                    env.IsDtdProcessingSet = true;
                }

                IConversionExpression conv = expression.Value as IConversionExpression;
                
                if (isXmlTextReaderXmlResolverProperty && conv != null && SecurityDiagnosticHelpers.IsXmlSecureResolverType(conv.Operand.Type, _xmlTypes))
                {
                    env.IsSecureResolver = true;
                }
                else if (isXmlTextReaderXmlResolverProperty && conv != null && SecurityDiagnosticHelpers.IsExpressionEqualsNull(conv.Operand))
                {
                    env.IsSecureResolver = true;
                }
                else if (isXmlTextReaderDtdProcessingProperty && conv == null && !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(expression.Value))
                {
                    env.IsDtdProcessingDisabled = !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(expression.Value);
                }
                else
                {
                    // Generate a warning whenever the XmlResolver or DtdProcessing property is set to an insecure value
                    Diagnostic diag = Diagnostic.Create(
                        RuleDoNotUseInsecureDTDProcessing,
                        expression.Syntax.GetLocation(),
                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                            nameof(DesktopAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage)
                        )
                    );
                    context.ReportDiagnostic(diag);
                }
            }

            private void AnalyzeAssignment(OperationAnalysisContext context)
            {
                IAssignmentExpression expression = context.Operation as IAssignmentExpression;
                
                if (expression.Target == null)
                {
                    return;
                }

                SemanticModel model = context.Compilation.GetSemanticModel(expression.Syntax.SyntaxTree);
                var propRef = expression.Target as IPropertyReferenceExpression;

                if (propRef == null) // A variable/field assignment
                {
                    ISymbol symbolAssignedTo = expression.Target.Syntax.GetDeclaredOrReferencedSymbol(model);

                    if (symbolAssignedTo != null)
                    {
                        AnalyzeObjectCreationInternal(context, symbolAssignedTo, expression.Value);
                    }
                }
                else // A property assignment
                {
                    ISymbol assignedSymbol = propRef.Instance.Syntax.GetDeclaredOrReferencedSymbol(model);

                    if (propRef.Property.MatchPropertyByName(_xmlTypes.XmlDocument, "XmlResolver"))
                    {
                        AnalyzeXmlResolverPropertyAssignmentForXmlDocument(context, assignedSymbol, expression);
                    }
                    else
                    {
                        bool isXmlTextReaderXmlResolverProperty = SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverPropertyDerived(propRef.Property, _xmlTypes);
                        bool isXmlTextReaderDtdProcessingProperty = !isXmlTextReaderXmlResolverProperty &&
                                                                  SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingPropertyDerived(propRef.Property, _xmlTypes);
                        if (isXmlTextReaderXmlResolverProperty || isXmlTextReaderDtdProcessingProperty)
                        {
                            AnalyzeXmlTextReaderProperties(context, assignedSymbol, expression, isXmlTextReaderXmlResolverProperty, isXmlTextReaderDtdProcessingProperty);
                        }
                        else if(SecurityDiagnosticHelpers.IsXmlReaderSettingsType(propRef.Instance.Type, _xmlTypes))
                        {
                            XmlReaderSettingsEnvironment env;
                            
                            if(!_xmlReaderSettingsEnvironments.TryGetValue(assignedSymbol, out env))
                            {
                                env = new XmlReaderSettingsEnvironment(_isFrameworkSecure);
                                _xmlReaderSettingsEnvironments[assignedSymbol] = env;
                            }

                            IConversionExpression conv = expression.Value as IConversionExpression;

                            if (conv != null && SecurityDiagnosticHelpers.IsXmlReaderSettingsXmlResolverProperty(
                                propRef.Property,
                                _xmlTypes)
                            )
                            {
                                if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(conv.Operand.Type, _xmlTypes))
                                {
                                    env.IsSecureResolver = true;
                                }
                                else if (SecurityDiagnosticHelpers.IsExpressionEqualsNull(conv.Operand))
                                {
                                    env.IsSecureResolver = true;
                                }
                            }
                            else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsDtdProcessingProperty(propRef.Property, _xmlTypes))
                            {
                                env.IsDtdProcessingDisabled = !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(expression.Value);
                            }
                            else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsMaxCharactersFromEntitiesProperty(propRef.Property, _xmlTypes))
                            {
                                env.IsMaxCharactersFromEntitiesLimited = !SecurityDiagnosticHelpers.IsExpressionEqualsIntZero(expression.Value);
                            }
                        }
                        else
                        {
                            AnalyzeNeverSetProperties(context, propRef.Property, expression.Syntax.GetLocation());
                        }

                    }
                }
            }

            private void AnalyzeNeverSetProperties(OperationAnalysisContext context, IPropertySymbol property, Location location)
            {
                if (property.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, SecurityMemberNames.InnerXml))
                {
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            rule,
                            location,
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.DoNotUseSetInnerXmlMessage)
                            )
                        )
                    );
                }
                else if (property.MatchPropertyDerivedByName(_xmlTypes.DataViewManager, SecurityMemberNames.DataViewSettingCollectionString))
                {
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDTDProcessing;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            rule,
                            location,
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.ReviewDtdProcessingPropertiesMessage)
                            )
                        )
                    );
                }
            }

            private void AnalyzeObjectCreationOperation(OperationAnalysisContext context)
            {
                AnalyzeObjectCreationInternal(context, null, context.Operation);
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
    }
}
