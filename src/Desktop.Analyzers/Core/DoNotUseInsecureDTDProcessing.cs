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
            var analyzer = new Analyzer(types, frameworkVersion);
            context.RegisterOperationAction(analyzer.AnalyzeOperation,
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
            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    Compilation compilation = context.Compilation;
                    var _xmlTypes = new CompilationSecurityTypes(compilation);

                    if (ReferencesAnyTargetType(_xmlTypes))
                    {
                        // context.RegisterSemanticModelAction(semanticModel => { model = semanticModel.SemanticModel; });
                        Version version = SecurityDiagnosticHelpers.GetDotNetFrameworkVersion(compilation);

                        if (version != null)
                        {
                            context.RegisterOperationBlockStartAction(
                                (c) =>
                                {
                                    RegisterAnalyzer(c, _xmlTypes, version);
                                });
                        }
                    }
                });
        }

        private class Analyzer
        {
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
                    this.IsConstructedInCodeBlock = true;
                    this.IsDtdProcessingDisabled = true;
                    // for .NET framework >= 4.5.2, the default value for XmlResolver property is null
                    if (isTargetFrameworkSecure)
                    {
                        IsSecureResolver = true;
                        IsMaxCharactersFromEntitiesLimited = true;
                    }
                }
            }

            // .NET frameworks >= 4.5.2 have secure default settings
            private static readonly Version s_minSecureFxVersion = new Version(4, 5, 2);

            private readonly CompilationSecurityTypes _xmlTypes;
            private readonly bool _isFrameworkSecure;
            private readonly HashSet<IOperation> _objectCreationOperationsAnalyzed = new HashSet<IOperation>();
            private readonly Dictionary<ISymbol, XmlDocumentEnvironment> _xmlDocumentEnvironments = new Dictionary<ISymbol, XmlDocumentEnvironment>();
            private readonly Dictionary<ISymbol, XmlTextReaderEnvironment> _xmlTextReaderEnvironments = new Dictionary<ISymbol, XmlTextReaderEnvironment>();
            private readonly Dictionary<ISymbol, XmlReaderSettingsEnvironment> _xmlReaderSettingsEnvironments = new Dictionary<ISymbol, XmlReaderSettingsEnvironment>();

            public Analyzer(CompilationSecurityTypes xmlTypes, Version targetFrameworkVersion)
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
                IInvocationExpression invoke = context.Operation as IInvocationExpression;

                if(invoke == null)
                {
                    return;
                }

                IMethodSymbol method = invoke.TargetMethod;

                if(method == null)
                {
                    return;
                }

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
                                invoke.Syntax.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.DoNotUseDtdProcessingOverloadsMessage),
                                    method.Name
                                )
                            )
                        );
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
                    if (field.Type.DerivesFrom(_xmlTypes.XmlDocument))
                    {
                        XmlDocumentEnvironment env = null;

                        if (!_xmlDocumentEnvironments.ContainsKey(field))
                        {
                            env = new XmlDocumentEnvironment();
                            env.IsSecureResolver = false;
                            env.IsXmlResolverSet = false;
                            env.XmlDocumentDefinition = fieldInit.Value.Syntax;
                        }
                        else
                        {
                            return;
                        }

                        IOperation valueOperation = fieldInit.Value;

                        if (valueOperation.Kind == OperationKind.ObjectCreationExpression)
                        {
                            AnalyzeObjectCreationInternal(context, field, valueOperation, env);
                        }
                    }
                }
            }

            private void AnalyzeObjectCreationInternal(OperationAnalysisContext context, ISymbol variable, IOperation value, XmlDocumentEnvironment env)
            {
                IObjectCreationExpression objCreation = value as IObjectCreationExpression;

                if (objCreation == null)
                {
                    return;
                }

                if (SecurityDiagnosticHelpers.IsXmlDocumentCtorDerived(objCreation.Constructor, _xmlTypes))
                {
                    SyntaxNode node = objCreation.Syntax;
                    bool isXmlDocumentSecureResolver = false;

                    if(_objectCreationOperationsAnalyzed.Contains(objCreation))
                    {
                        return;
                    }
                    else
                    {
                        _objectCreationOperationsAnalyzed.Add(objCreation);
                    }
                    
                    if (objCreation.Constructor.ContainingType != _xmlTypes.XmlDocument)
                    {
                        isXmlDocumentSecureResolver = true;
                    }
                    
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

                                if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(operation.Operand.Type, _xmlTypes))
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

                                break; // Found XmlResolver property
                            }
                        }
                    }

                    //////
                    if (env != null)
                    {
                        env.IsSecureResolver = isXmlDocumentSecureResolver;

                        if (variable != null)
                        {
                            _xmlDocumentEnvironments[variable] = env;
                        }
                    }
                    else // Temp object
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
            }

            private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
            {
                IVariableDeclaration declare = context.Operation as IVariableDeclaration;

                if (declare.Variable.Type.DerivesFrom(_xmlTypes.XmlDocument))
                {
                    if (!_xmlDocumentEnvironments.ContainsKey(declare.Variable))
                    {
                        var env = new XmlDocumentEnvironment();
                        env.IsSecureResolver = false;
                        env.IsXmlResolverSet = false;
                        env.XmlDocumentDefinition = declare.Syntax;

                        if (declare.InitialValue.Kind == OperationKind.ObjectCreationExpression)
                        {
                            AnalyzeObjectCreationInternal(context, declare.Variable, declare.InitialValue, env);
                        }
                    }
                }
            }

            private void AnalyzeAssignment(OperationAnalysisContext context)
            {
                IAssignmentExpression expression = context.Operation as IAssignmentExpression;
                
                SemanticModel model = context.Compilation.GetSemanticModel(expression.Syntax.SyntaxTree);

                if (expression.Target == null)
                {
                    return;
                }
                
                var propRef = expression.Target as IPropertyReferenceExpression;

                if (propRef == null)
                {
                    ISymbol symbolAssignedTo = expression.Target.Syntax.GetDeclaredOrReferencedSymbol(model);

                    if(symbolAssignedTo != null)
                    {
                        if(_xmlDocumentEnvironments.ContainsKey(symbolAssignedTo))
                        {
                            XmlDocumentEnvironment env = _xmlDocumentEnvironments[symbolAssignedTo];
                            env.XmlDocumentDefinition = expression.Syntax;
                            AnalyzeObjectCreationInternal(context, symbolAssignedTo, expression.Value, env);
                        }
                    }

                    return;
                }

                ISymbol assignedSymbol = propRef.Instance.Syntax.GetDeclaredOrReferencedSymbol(model);

                if (propRef.Property.MatchPropertyByName(_xmlTypes.XmlDocument, "XmlResolver"))
                {
                    XmlDocumentEnvironment env = null;
                    bool isSecureResolver = false;

                    if (_xmlDocumentEnvironments.ContainsKey(assignedSymbol))
                    {
                        env = _xmlDocumentEnvironments[assignedSymbol];
                        env.IsXmlResolverSet = true;
                    }

                    IConversionExpression conv = expression.Value as IConversionExpression;

                    if (SecurityDiagnosticHelpers.IsXmlSecureResolverType(conv.Operand.Type, _xmlTypes))
                    {
                        isSecureResolver = true;
                    }
                    else if (conv != null && conv.Operand as ILiteralExpression != null)
                    {
                        ILiteralExpression literal = conv.Operand as ILiteralExpression;

                        if (literal.ConstantValue.HasValue && literal.ConstantValue.Value == null)
                        {
                            isSecureResolver = true;
                        }
                    }
                    else // Assign XmlDocument's XmlResolver to an insecure value
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
                    
                    if(env != null)
                    {
                        env.IsSecureResolver = isSecureResolver;
                    }
                }
            }

            private void AnalyzeObjectCreationOperation(OperationAnalysisContext context)
            {
                IObjectCreationExpression expression = context.Operation as IObjectCreationExpression;

                if (expression == null)
                {
                    return;
                }

                AnalyzeObjectCreationInternal(context, null, expression, null);               
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
