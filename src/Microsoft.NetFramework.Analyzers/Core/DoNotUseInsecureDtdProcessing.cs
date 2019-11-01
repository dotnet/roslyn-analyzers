﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.NetFramework.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetFramework.Analyzers
{
    /// <summary>
    /// Secure DTD processing and entity resolution in XML
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseInsecureDtdProcessingAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3075";
        private const string HelpLink = "https://docs.microsoft.com/visualstudio/code-quality/ca3075-insecure-dtd-processing";

        internal static DiagnosticDescriptor RuleDoNotUseInsecureDtdProcessing = CreateDiagnosticDescriptor(
                                                                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseInsecureDtdProcessingGenericMessage)),
                                                                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseInsecureDtdProcessingDescription)),
                                                                                    HelpLink);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleDoNotUseInsecureDtdProcessing);

        private static void RegisterAnalyzer(OperationBlockStartAnalysisContext context, CompilationSecurityTypes types, Version frameworkVersion)
        {
            var analyzer = new OperationAnalyzer(types, frameworkVersion);
            context.RegisterOperationAction(
                analyzer.AnalyzeOperation,
                OperationKind.Invocation,
                OperationKind.SimpleAssignment,
                OperationKind.VariableDeclarator,
                OperationKind.ObjectCreation,
                OperationKind.FieldInitializer
            );
            context.RegisterOperationBlockEndAction(
                analyzer.AnalyzeOperationBlock
            );
        }


#pragma warning disable RS1026 // Enable concurrent execution
        public override void Initialize(AnalysisContext analysisContext)
#pragma warning restore RS1026 // Enable concurrent execution
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
                internal SyntaxNode XmlDocumentDefinition { get; set; }
                internal bool IsXmlResolverSet { get; set; }
                internal bool IsSecureResolver { get; set; }
            }

            private class XmlTextReaderEnvironment
            {
                internal SyntaxNode XmlTextReaderDefinition { get; set; }
                internal bool IsDtdProcessingSet { get; set; }
                internal bool IsDtdProcessingDisabled { get; set; }
                internal bool IsXmlResolverSet { get; set; }
                internal bool IsSecureResolver { get; set; }

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
                internal SyntaxNode XmlReaderSettingsDefinition { get; set; }
                internal bool IsDtdProcessingDisabled { get; set; }
                internal bool IsMaxCharactersFromEntitiesLimited { get; set; }
                internal bool IsSecureResolver { get; set; }
                internal bool IsConstructedInCodeBlock { get; set; }

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
                            RuleDoNotUseInsecureDtdProcessing,
                            env.XmlDocumentDefinition.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(MicrosoftNetFrameworkAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
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
                            RuleDoNotUseInsecureDtdProcessing,
                            env.XmlTextReaderDefinition.GetLocation(),
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(MicrosoftNetFrameworkAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionMessage)
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
                    case OperationKind.ObjectCreation:
                        AnalyzeObjectCreationOperation(context);
                        break;
                    case OperationKind.SimpleAssignment:
                        AnalyzeAssignment(context);
                        break;
                    case OperationKind.FieldInitializer:
                        var fieldInitializer = (IFieldInitializerOperation)context.Operation;
                        foreach (var field in fieldInitializer.InitializedFields)
                        {
                            AnalyzeObjectCreationInternal(context, field, fieldInitializer.Value);
                        }
                        break;
                    case OperationKind.VariableDeclarator:
                        var declarator = (IVariableDeclaratorOperation)context.Operation;
                        AnalyzeObjectCreationInternal(context, declarator.Symbol, declarator.GetVariableInitializer()?.Value);
                        break;
                    case OperationKind.Invocation:
                        var invocation = (IInvocationOperation)context.Operation;
                        AnalyzeMethodOverloads(context, invocation.TargetMethod, invocation.Arguments, invocation.Syntax);
                        break;
                }
            }

            private void AnalyzeMethodOverloads(OperationAnalysisContext context, IMethodSymbol method, ImmutableArray<IArgumentOperation> arguments, SyntaxNode expressionSyntax)
            {
                if (method.MatchMethodDerivedByName(_xmlTypes.XmlDocument, SecurityMemberNames.Load) ||                                    //FxCop CA3056
                    method.MatchMethodDerivedByName(_xmlTypes.XmlDocument, SecurityMemberNames.LoadXml) ||                                 //FxCop CA3057
                    method.MatchMethodDerivedByName(_xmlTypes.XPathDocument, WellKnownMemberNames.InstanceConstructorName) ||              //FxCop CA3059
                    method.MatchMethodDerivedByName(_xmlTypes.XmlSchema, SecurityMemberNames.Read) ||                                      //FxCop CA3060
                    method.MatchMethodDerivedByName(_xmlTypes.DataSet, SecurityMemberNames.ReadXml) ||                                     //FxCop CA3063
                    method.MatchMethodDerivedByName(_xmlTypes.DataSet, SecurityMemberNames.ReadXmlSchema) ||                               //FxCop CA3064
                    method.MatchMethodDerivedByName(_xmlTypes.XmlSerializer, SecurityMemberNames.Deserialize) ||                           //FxCop CA3070
                    method.MatchMethodDerivedByName(_xmlTypes.DataTable, SecurityMemberNames.ReadXml) ||                                   //FxCop CA3071
                    method.MatchMethodDerivedByName(_xmlTypes.DataTable, SecurityMemberNames.ReadXmlSchema))                               //FxCop CA3072
                {
                    if (SecurityDiagnosticHelpers.HasXmlReaderParameter(method, _xmlTypes) < 0)
                    {
                        DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessing;
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                rule,
                                expressionSyntax.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseDtdProcessingOverloadsMessage),
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
                        if (method.Parameters.Length == 1
                            && method.Parameters[0].RefKind == RefKind.None
                            && method.Parameters[0].Type.SpecialType == SpecialType.System_String)
                        {
                            // inputUri can load be a URL.  Should further investigate if this is worth flagging.
                            Diagnostic diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDtdProcessing,
                                    expressionSyntax.GetLocation(),
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(MicrosoftNetFrameworkAnalyzersResources.XmlReaderCreateWrongOverloadMessage)
                                    )
                                );
                            context.ReportDiagnostic(diag);
                        }

                        // If no XmlReaderSettings are passed, then the default
                        // XmlReaderSettings are used, with DtdProcessing set to Prohibit.
                    }
                    else if (arguments.Length > xmlReaderSettingsIndex)
                    {
                        IArgumentOperation arg = arguments[xmlReaderSettingsIndex];
                        ISymbol settingsSymbol = arg.GetReferencedMemberOrLocalOrParameter();

                        if (settingsSymbol == null)
                        {
                            return;
                        }

                        // If we have no XmlReaderSettingsEnvironment, then we don't know.
                        if (_xmlReaderSettingsEnvironments.TryGetValue(settingsSymbol, out XmlReaderSettingsEnvironment env)
                            && !env.IsDtdProcessingDisabled
                            && !(env.IsSecureResolver && env.IsMaxCharactersFromEntitiesLimited))
                        {
                            Diagnostic diag;
                            if (env.IsConstructedInCodeBlock)
                            {
                                diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDtdProcessing,
                                    expressionSyntax.GetLocation(),
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(MicrosoftNetFrameworkAnalyzersResources.XmlReaderCreateInsecureConstructedMessage)
                                    )
                                );
                            }
                            else
                            {
                                diag = Diagnostic.Create(
                                    RuleDoNotUseInsecureDtdProcessing,
                                    expressionSyntax.GetLocation(),
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(MicrosoftNetFrameworkAnalyzersResources.XmlReaderCreateInsecureInputMessage)
                                    )
                                );
                            }

                            context.ReportDiagnostic(diag);
                        }
                    }
                }
            }

            private void AnalyzeObjectCreationInternal(OperationAnalysisContext context, ISymbol variable, IOperation valueOpt)
            {
                if (!(valueOpt is IObjectCreationOperation objCreation))
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
                    AnalyzeMethodOverloads(context, objCreation.Constructor, objCreation.Arguments, objCreation.Syntax);
                }
            }

            private void AnalyzeObjectCreationForXmlDocument(OperationAnalysisContext context, ISymbol variable, IObjectCreationOperation objCreation)
            {
                if (variable == null || !_xmlDocumentEnvironments.TryGetValue(variable, out var xmlDocumentEnvironment))
                {
                    xmlDocumentEnvironment = new XmlDocumentEnvironment
                    {
                        IsSecureResolver = false,
                        IsXmlResolverSet = false
                    };
                }

                xmlDocumentEnvironment.XmlDocumentDefinition = objCreation.Syntax;
                SyntaxNode node = objCreation.Syntax;
                bool isXmlDocumentSecureResolver = false;

                if (!Equals(objCreation.Constructor.ContainingType, _xmlTypes.XmlDocument))
                {
                    isXmlDocumentSecureResolver = true;
                }

                // propertyInitlizer is not returned any more 
                // and no way to get propertysymbol
                if (objCreation.Initializer != null)
                {
                    foreach (IOperation init in objCreation.Initializer.Initializers)
                    {
                        if (init is IAssignmentOperation assign)
                        {
                            var propValue = assign.Value;
                            if (!(assign.Target is IPropertyReferenceOperation propertyReference))
                            {
                                continue;
                            }

                            var prop = propertyReference.Property;
                            if (prop.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, "XmlResolver"))
                            {
                                if (!(propValue is IConversionOperation operation))
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
                                    ReportDiagnostic(assign.Syntax, context);
                                    return;
                                }
                            }
                            else
                            {
                                AnalyzeNeverSetProperties(context, prop, assign.Syntax.GetLocation());
                            }
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
                    ReportDiagnostic(node, context);
                }

                return;

                // Local functions
                static void ReportDiagnostic(SyntaxNode node, OperationAnalysisContext context)
                {
                    Diagnostic diag = Diagnostic.Create(
                                        RuleDoNotUseInsecureDtdProcessing,
                                        node.GetLocation(),
                                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                            nameof(MicrosoftNetFrameworkAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                                        )
                                    );
                    context.ReportDiagnostic(diag);
                }
            }

            private void AnalyzeObjectCreationForXmlTextReader(OperationAnalysisContext context, ISymbol variable, IObjectCreationOperation objCreation)
            {
                if (variable == null || !_xmlTextReaderEnvironments.TryGetValue(variable, out XmlTextReaderEnvironment env))
                {
                    env = new XmlTextReaderEnvironment(_isFrameworkSecure)
                    {
                        XmlTextReaderDefinition = objCreation.Syntax
                    };
                }

                if (!Equals(objCreation.Constructor.ContainingType, _xmlTypes.XmlTextReader))
                {
                    env.IsDtdProcessingDisabled = true;
                    env.IsSecureResolver = true;
                }

                if (objCreation.Initializer != null)
                {
                    foreach (IOperation init in objCreation.Initializer.Initializers)
                    {
                        if (init is IAssignmentOperation assign)
                        {
                            var propValue = assign.Value;
                            if (!(assign.Target is IPropertyReferenceOperation propertyReference))
                            {
                                continue;
                            }

                            var prop = propertyReference.Property;
                            if (propValue is IConversionOperation operation
                                && SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverPropertyDerived(prop, _xmlTypes))
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
                            else if (SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingPropertyDerived(prop, _xmlTypes))
                            {
                                env.IsDtdProcessingSet = true;
                                env.IsDtdProcessingDisabled = !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(propValue);
                            }
                        }
                    }
                }

                // if the XmlResolver or Dtdprocessing property is explicitly set when created, and is to an insecure value, generate a warning
                if ((env.IsXmlResolverSet && !env.IsSecureResolver) ||
                    (env.IsDtdProcessingSet && !env.IsDtdProcessingDisabled))
                {
                    Diagnostic diag = Diagnostic.Create(
                        RuleDoNotUseInsecureDtdProcessing,
                        env.XmlTextReaderDefinition.GetLocation(),
                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                            nameof(MicrosoftNetFrameworkAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage)
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
                else if (variable == null && !(env.IsDtdProcessingSet && env.IsDtdProcessingDisabled))
                {
                    Diagnostic diag = Diagnostic.Create(
                        RuleDoNotUseInsecureDtdProcessing,
                        env.XmlTextReaderDefinition.GetLocation(),
                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                            nameof(MicrosoftNetFrameworkAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionMessage)
                        )
                    );
                    context.ReportDiagnostic(diag);
                }
            }

            private void AnalyzeObjectCreationForXmlReaderSettings(ISymbol variable, IObjectCreationOperation objCreation)
            {
                XmlReaderSettingsEnvironment xmlReaderSettingsEnv = new XmlReaderSettingsEnvironment(_isFrameworkSecure);

                if (variable != null)
                {
                    _xmlReaderSettingsEnvironments[variable] = xmlReaderSettingsEnv;
                }

                xmlReaderSettingsEnv.XmlReaderSettingsDefinition = objCreation.Syntax;

                if (objCreation.Initializer != null)
                {
                    foreach (IOperation init in objCreation.Initializer.Initializers)
                    {
                        if (init is IAssignmentOperation assign)
                        {
                            var propValue = assign.Value;
                            if (!(assign.Target is IPropertyReferenceOperation propertyReference))
                            {
                                continue;
                            }

                            var prop = propertyReference.Property;
                            if (SecurityDiagnosticHelpers.IsXmlReaderSettingsXmlResolverProperty(
                                    prop,
                                    _xmlTypes)
                                )
                            {

                                if (!(propValue is IConversionOperation operation))
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
                            else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsDtdProcessingProperty(prop, _xmlTypes))
                            {
                                xmlReaderSettingsEnv.IsDtdProcessingDisabled = !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(propValue);
                            }
                            else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsMaxCharactersFromEntitiesProperty(prop, _xmlTypes))
                            {
                                xmlReaderSettingsEnv.IsMaxCharactersFromEntitiesLimited = !SecurityDiagnosticHelpers.IsExpressionEqualsIntZero(propValue);
                            }
                        }
                    }
                }
            }

            private void AnalyzeXmlResolverPropertyAssignmentForXmlDocument(OperationAnalysisContext context, ISymbol assignedSymbol, IAssignmentOperation expression)
            {
                bool isSecureResolver = false;

                IConversionOperation conv = expression.Value as IConversionOperation;
                if (conv != null && SecurityDiagnosticHelpers.IsXmlSecureResolverType(conv.Operand.Type, _xmlTypes))
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
                                RuleDoNotUseInsecureDtdProcessing,
                                context.Operation.Syntax.GetLocation(),
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(MicrosoftNetFrameworkAnalyzersResources.XmlDocumentWithNoSecureResolverMessage)
                                )
                            );
                    context.ReportDiagnostic(diag);
                }

                if (_xmlDocumentEnvironments.TryGetValue(assignedSymbol, out XmlDocumentEnvironment xmlDocumentEnv))
                {
                    xmlDocumentEnv.IsXmlResolverSet = true;
                    xmlDocumentEnv.IsSecureResolver = isSecureResolver;
                }
            }

            private void AnalyzeXmlTextReaderProperties(OperationAnalysisContext context, ISymbol assignedSymbol, IAssignmentOperation expression, bool isXmlTextReaderXmlResolverProperty, bool isXmlTextReaderDtdProcessingProperty)
            {
                if (!_xmlTextReaderEnvironments.TryGetValue(assignedSymbol, out XmlTextReaderEnvironment env))
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

                IConversionOperation conv = expression.Value as IConversionOperation;

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
                else if (context.Operation?.Parent?.Kind != OperationKind.ObjectOrCollectionInitializer)
                {
                    // Generate a warning whenever the XmlResolver or DtdProcessing property is set to an insecure value
                    Diagnostic diag = Diagnostic.Create(
                        RuleDoNotUseInsecureDtdProcessing,
                        expression.Syntax.GetLocation(),
                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                            nameof(MicrosoftNetFrameworkAnalyzersResources.XmlTextReaderSetInsecureResolutionMessage)
                        )
                    );
                    context.ReportDiagnostic(diag);
                }
            }

            private void AnalyzeAssignment(OperationAnalysisContext context)
            {
                var assignment = (IAssignmentOperation)context.Operation;

                if (!(assignment.Target is IPropertyReferenceOperation propRef)) // A variable/field assignment
                {
                    var symbolAssignedTo = assignment.Target.GetReferencedMemberOrLocalOrParameter();
                    if (symbolAssignedTo != null)
                    {
                        AnalyzeObjectCreationInternal(context, symbolAssignedTo, assignment.Value);
                    }
                }
                else // A property assignment
                {
                    ISymbol assignedSymbol = propRef.Instance?.GetReferencedMemberOrLocalOrParameter();
                    if (assignedSymbol == null)
                    {
                        return;
                    }

                    if (propRef.Property.MatchPropertyByName(_xmlTypes.XmlDocument, "XmlResolver"))
                    {
                        AnalyzeXmlResolverPropertyAssignmentForXmlDocument(context, assignedSymbol, assignment);
                    }
                    else
                    {
                        bool isXmlTextReaderXmlResolverProperty =
                            SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverPropertyDerived(propRef.Property, _xmlTypes);
                        bool isXmlTextReaderDtdProcessingProperty = !isXmlTextReaderXmlResolverProperty &&
                            SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingPropertyDerived(propRef.Property, _xmlTypes);
                        if (isXmlTextReaderXmlResolverProperty || isXmlTextReaderDtdProcessingProperty)
                        {
                            AnalyzeXmlTextReaderProperties(context, assignedSymbol, assignment, isXmlTextReaderXmlResolverProperty,
                                isXmlTextReaderDtdProcessingProperty);
                        }
                        else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsType(propRef.Instance.Type, _xmlTypes))
                        {

                            if (!_xmlReaderSettingsEnvironments.TryGetValue(assignedSymbol, out XmlReaderSettingsEnvironment env))
                            {
                                env = new XmlReaderSettingsEnvironment(_isFrameworkSecure);
                                _xmlReaderSettingsEnvironments[assignedSymbol] = env;
                            }


                            if (assignment.Value is IConversionOperation conv && SecurityDiagnosticHelpers.IsXmlReaderSettingsXmlResolverProperty(propRef.Property, _xmlTypes))
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
                                env.IsDtdProcessingDisabled =
                                    !SecurityDiagnosticHelpers.IsExpressionEqualsDtdProcessingParse(assignment.Value);
                            }
                            else if (SecurityDiagnosticHelpers.IsXmlReaderSettingsMaxCharactersFromEntitiesProperty(propRef.Property,
                                _xmlTypes))
                            {
                                env.IsMaxCharactersFromEntitiesLimited =
                                    !SecurityDiagnosticHelpers.IsExpressionEqualsIntZero(assignment.Value);
                            }
                        }
                        else
                        {
                            AnalyzeNeverSetProperties(context, propRef.Property, assignment.Syntax.GetLocation());
                        }
                    }
                }
            }

            private void AnalyzeNeverSetProperties(OperationAnalysisContext context, IPropertySymbol property, Location location)
            {
                if (property.MatchPropertyDerivedByName(_xmlTypes.XmlDocument, SecurityMemberNames.InnerXml))
                {
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessing;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            rule,
                            location,
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(MicrosoftNetFrameworkAnalyzersResources.DoNotUseSetInnerXmlMessage)
                            )
                        )
                    );
                }
                else if (property.MatchPropertyDerivedByName(_xmlTypes.DataViewManager, SecurityMemberNames.DataViewSettingCollectionString))
                {
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessing;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            rule,
                            location,
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(MicrosoftNetFrameworkAnalyzersResources.ReviewDtdProcessingPropertiesMessage)
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
                                            SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.InsecureXmlDtdProcessing)),
                                            messageFormat,
                                            DiagnosticCategory.Security,
                                            DiagnosticHelpers.DefaultDiagnosticSeverity,
                                            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                            description: description,
                                            helpLinkUri: helpLink,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }
    }
}
