// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Desktop.Analyzers.Common;
using System.Collections.Concurrent;

namespace Desktop.Analyzers
{
    public abstract class CA3077DiagnosticAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3077";

        internal static DiagnosticDescriptor RuleXmlDocumentDerivedClassDesignNoConstructor = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlDocumentDerivedClassNoConstructorDiagnosis)),
                                                                                                                         DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.MakeDtdProcessingSecureInApiDesignXmlDocumentDescription)));

        internal static DiagnosticDescriptor RuleXmlDocumentDerivedClassDesignConstructorNoSecureXmlResolver = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlDocumentDerivedClassConstructorNoSecureXmlResolverDiagnosis)),
                                                                                                                                          DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.MakeDtdProcessingSecureInApiDesignXmlDocumentDescription)));

        internal static DiagnosticDescriptor RuleXmlDocumentDerivedClassDesignSetInsecureXmlResolverInMethod = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlDocumentDerivedClassSetInsecureXmlResolverInMethodDiagnosis)),
                                                                                                                                          DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.MakeDtdProcessingSecureInApiDesignXmlDocumentDescription)));

        internal static DiagnosticDescriptor RuleXmlTextReaderDerivedClassDesignNoConstructor = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlTextReaderDerivedClassNoConstructorDiagnosis)),
                                                                                                                           DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.MakeDtdProcessingSecureInApiDesignXmlTextReaderDescription)));

        internal static DiagnosticDescriptor RuleXmlTextReaderDerivedClassDesignConstructorNoSecureSettings = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlTextReaderDerivedClassConstructorNoSecureSettingsDiagnosis)),
                                                                                                                                         DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.MakeDtdProcessingSecureInApiDesignXmlTextReaderDescription)));

        internal static DiagnosticDescriptor RuleXmlTextReaderDerivedClassDesignSetInsecureSettingsInMethod = CreateDiagnosticDescriptor(DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.XmlTextReaderDerivedClassSetInsecureSettingsInMethodDiagnosis)),
                                                                                                                                         DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.MakeDtdProcessingSecureInApiDesignXmlTextReaderDescription)));
  
        private static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create(RuleXmlDocumentDerivedClassDesignNoConstructor,
                                                                                                                  RuleXmlDocumentDerivedClassDesignConstructorNoSecureXmlResolver,
                                                                                                                  RuleXmlDocumentDerivedClassDesignSetInsecureXmlResolverInMethod,
                                                                                                                  RuleXmlTextReaderDerivedClassDesignNoConstructor,
                                                                                                                  RuleXmlTextReaderDerivedClassDesignConstructorNoSecureSettings,
                                                                                                                  RuleXmlTextReaderDerivedClassDesignSetInsecureSettingsInMethod);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return CA3077DiagnosticAnalyzer.supportedDiagnostics;
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
                            Analyzer analyzer = GetAnalyzer(context, xmlTypes, version);
                            context.RegisterSymbolAction(analyzer.AnalyzeSymbol, SymbolKind.NamedType);
                        }
                    }
                });
        }

        private static bool ReferencesAnyTargetType(CompilationSecurityTypes types)
        {
            return types.XmlDocument != null
                || types.XmlTextReader != null;
        }

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(LocalizableResourceString messageFormat, LocalizableResourceString description, string helpLink = null)
        {
            return new DiagnosticDescriptor(RuleId,
                                            DiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureDtdProcessingInApiDesign)),
                                            messageFormat,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
                                            description: description,
                                            helpLinkUri: helpLink,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }

        protected abstract Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes types, Version targetFrameworkVersion);

        protected class Analyzer
        {
            // .NET frameworks >= 4.5.2 have secure default settings for XmlTextReader:
            //      DtdProcessing is enabled with null resolver
            private static readonly Version minSecureFxVersion = new Version(4, 5, 2);

            private readonly CompilationSecurityTypes xmlTypes;
            private readonly SyntaxNodeHelper syntaxNodeHelper;
            private readonly bool isFrameworkSecure;

            // key: symbol for type derived from XmlDocument/XmlTextReader (exclude base type itself)
            // value: if it has explicitly defined constructor
            private readonly ConcurrentDictionary<INamedTypeSymbol, bool> xmlDocumentDerivedTypes = new ConcurrentDictionary<INamedTypeSymbol, bool>();
            private readonly ConcurrentDictionary<INamedTypeSymbol, bool> xmlTextReaderDerivedTypes = new ConcurrentDictionary<INamedTypeSymbol, bool>();


            public Analyzer(CompilationSecurityTypes xmlTypes, SyntaxNodeHelper helper, Version targetFrameworkVersion)
            {
                this.xmlTypes = xmlTypes;
                this.syntaxNodeHelper = helper;
                this.isFrameworkSecure = targetFrameworkVersion == null ? false : targetFrameworkVersion >= Analyzer.minSecureFxVersion;
            }

            public void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
                // an alternative is to do syntax analysis during the symbol analysis, which might cause AST construction on the fly
                AnalyzeNodeForXmlDocumentDerivedTypeConstructorDecl(context);
                AnalyzeNodeForXmlDocumentDerivedTypeMethodDecl(context);
                if (!this.isFrameworkSecure)
                {
                    AnalyzeNodeForXmlTextReaderDerivedTypeConstructorDecl(context);
                }
                AnalyzeNodeForXmlTextReaderDerivedTypeMethodDecl(context);
            }

            public void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                AnalyzeSymbolForXmlDocumentDerivedType(context);
                AnalyzeSymbolForXmlTextReaderDerivedType(context);
            }

            private void AnalyzeNodeForXmlDocumentDerivedTypeConstructorDecl(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                IMethodSymbol methodSymbol = SyntaxNodeHelper.GetDeclaredSymbol(node, model) as IMethodSymbol;

                if (methodSymbol == null || 
                    methodSymbol.MethodKind != MethodKind.Constructor ||
                    !((methodSymbol.ContainingType != xmlTypes.XmlDocument) && methodSymbol.ContainingType.IsDerivedFrom(xmlTypes.XmlDocument, baseTypesOnly: true)))
                {
                    return;
                }

                bool hasSetSecureXmlResolver = false;

                var assignments = this.syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (var a in assignments)
                {
                    bool isTargetProperty;
                    // this is intended to be an assignment, not a bug
                    if (hasSetSecureXmlResolver = IsAssigningIntendedValueToPropertyDerivedFromType(a,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverProperty(s, this.xmlTypes);
                            },
                            (n) =>
                            {
                                return SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                    SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, this.xmlTypes);
                            },
                            out isTargetProperty))
                    {
                        break;
                    }
                }

                if (!hasSetSecureXmlResolver)
                {
                    var rule = RuleXmlDocumentDerivedClassDesignConstructorNoSecureXmlResolver;
                    context.ReportDiagnostic(CreateDiagnostic(methodSymbol.Locations, rule));
                }
            }

            // Trying to find every "this.XmlResolver = [Insecure Resolve];" in methods of types derived from XmlDocment and generate a warning for each
            private void AnalyzeNodeForXmlDocumentDerivedTypeMethodDecl(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                IMethodSymbol methodSymbol = SyntaxNodeHelper.GetDeclaredSymbol(node, model) as IMethodSymbol;

                if (methodSymbol == null ||
                    // skip constructors since we report on the absence of secure assignment in AnalyzeNodeForXmlDocumentDerivedTypeConstructorDecl
                    methodSymbol.MethodKind == MethodKind.Constructor ||
                    !((methodSymbol.ContainingType != xmlTypes.XmlDocument) && methodSymbol.ContainingType.IsDerivedFrom(xmlTypes.XmlDocument, baseTypesOnly: true)))
                {
                    return;
                }

                var assignments = this.syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (var assignment in assignments)
                {
                    bool isTargetProperty;
                    // this is intended to be an assignment, not a bug
                    if (IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverProperty(s, this.xmlTypes);
                            },
                            (n) =>
                            {
                                return !(SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                         SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, this.xmlTypes));
                            },
                            out isTargetProperty)
                        )
                    {
                        var rule = RuleXmlDocumentDerivedClassDesignSetInsecureXmlResolverInMethod;
                        context.ReportDiagnostic(CreateDiagnostic(assignment.GetLocation(), rule));
                    }
                }
            }

            private void AnalyzeNodeForXmlTextReaderDerivedTypeConstructorDecl(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                IMethodSymbol methodSymbol = SyntaxNodeHelper.GetDeclaredSymbol(node, model) as IMethodSymbol;

                if (methodSymbol == null ||
                    methodSymbol.MethodKind != MethodKind.Constructor ||
                    !((methodSymbol.ContainingType != xmlTypes.XmlTextReader) && methodSymbol.ContainingType.IsDerivedFrom(xmlTypes.XmlTextReader, baseTypesOnly: true)))
                {
                    return;
                }

                bool hasSetSecureXmlResolver = false;
                bool isDtdProcessingDisabled = false;

                var assignments = this.syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (var assignment in assignments)
                {
                    bool isTargetProperty = false;

                    hasSetSecureXmlResolver = hasSetSecureXmlResolver || IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverProperty(s, this.xmlTypes);
                            },
                            (n) =>
                            {
                                return SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                       SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, this.xmlTypes);
                            },
                            out isTargetProperty);

                    if (isTargetProperty)
                    {
                        continue;
                    }

                    isDtdProcessingDisabled = isDtdProcessingDisabled || IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingProperty(s, this.xmlTypes);
                            },
                            (n) =>
                            {
                                return !SyntaxNodeHelper.GetSymbol(n, model).MatchField(this.xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
                            },
                            out isTargetProperty);

                    if (hasSetSecureXmlResolver && isDtdProcessingDisabled)
                    {
                        return;
                    }
                }
                
                var rule = RuleXmlTextReaderDerivedClassDesignConstructorNoSecureSettings;
                context.ReportDiagnostic(CreateDiagnostic(methodSymbol.Locations, rule));
            }

            private void AnalyzeNodeForXmlTextReaderDerivedTypeMethodDecl(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                IMethodSymbol methodSymbol = SyntaxNodeHelper.GetDeclaredSymbol(node, model) as IMethodSymbol;

                if (methodSymbol == null ||
                   !((methodSymbol.ContainingType != xmlTypes.XmlTextReader) && methodSymbol.ContainingType.IsDerivedFrom(xmlTypes.XmlTextReader, baseTypesOnly: true)))
                {
                    return;
                }

                // If the default value are not secure, the AnalyzeNodeForXmlTextReaderDerivedTypeConstructorDecl would be skipped,
                // therefoer we need to check constructor for any insecure settings.
                // Otherwise, we skip checking constructors
                if (this.isFrameworkSecure && methodSymbol.MethodKind == MethodKind.Constructor)
                {
                    return;
                }

                bool hasSetXmlResolver = false;
                bool hasSetInsecureXmlResolver = true;
                bool isDtdProcessingSet = false;
                bool isDtdProcessingEnabled = true;

                List<Location> locs = null;
                Location insecureXmlResolverAssignLoc = null;
                Location issecureDTDProcessingLoc = null;

                var assignments = this.syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (var assignment in assignments)
                {
                    bool ret;
                    bool isTargetProperty;

                    ret = IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverProperty(s, this.xmlTypes);
                            },
                            (n) =>
                            {
                                return !(SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                        SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, this.xmlTypes));
                            },
                            out isTargetProperty
                            );

                    if (isTargetProperty)
                    {
                        hasSetXmlResolver = true;
                        hasSetInsecureXmlResolver &= ret; // use 'AND' to avoid false positives (but imcrease false negative rate) 
                        if (ret)
                        {
                            if (locs == null)
                            {
                                locs = new List<Location>();
                            }
                            locs.Add(assignment.GetLocation());
                        }
                        continue;
                    }

                    ret = IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingProperty(s, this.xmlTypes);
                            },
                            (n) =>
                            {
                                return SyntaxNodeHelper.GetSymbol(n, model).MatchField(this.xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
                            },
                            out isTargetProperty);

                    if (isTargetProperty)
                    {
                        isDtdProcessingSet = true;
                        isDtdProcessingEnabled &= ret; // use 'AND' to avoid false positives (but imcrease false negative rate)
                        if (ret)
                        {
                            if (locs == null)
                            {
                                locs = new List<Location>();
                            }
                            locs.Add(assignment.GetLocation());
                        }
                    }
                }

                // neither XmlResolver nor DtdProcessing is explicitly set
                if (!(hasSetXmlResolver || isDtdProcessingSet))
                {
                    return;
                }
                // explicitly set XmlResolver and/or DtdProcessing to secure value
                else if (!hasSetInsecureXmlResolver || !isDtdProcessingEnabled)
                {
                    return;
                }
                // didn't explicitly set either one of XmlResolver and DtdProcessing to secure value 
                // but explicitly set XmlResolver and/or DtdProcessing to insecure value
                else
                {
                    if (insecureXmlResolverAssignLoc != null)
                    {
                        locs.Add(insecureXmlResolverAssignLoc);
                    }
                    if (issecureDTDProcessingLoc != null)
                    {
                        locs.Add(issecureDTDProcessingLoc);
                    }
                    var rule = RuleXmlTextReaderDerivedClassDesignSetInsecureSettingsInMethod;
                    // TODO: Only first location is shown in error, maybe we want to report on method instead?
                    //       Or on each insecure assignment?
                    context.ReportDiagnostic(CreateDiagnostic(locs, rule));
                }
            }

            // report warning if no explicit definition of contrsuctor in XmlDocument derived types
            private void AnalyzeSymbolForXmlDocumentDerivedType(SymbolAnalysisContext context)
            {
                ISymbol symbol = context.Symbol;
                if (symbol.Kind != SymbolKind.NamedType)
                {
                    return;
                }
                var typeSymbol = (INamedTypeSymbol)symbol;
                var xmlDocumentSym = this.xmlTypes.XmlDocument;
                if ((typeSymbol != xmlDocumentSym) && typeSymbol.IsDerivedFrom(xmlDocumentSym, baseTypesOnly: true))
                {
                    bool explicitlyDeclared = true;

                    if (typeSymbol.Constructors.Length == 1)
                    {
                        IMethodSymbol constructor = typeSymbol.Constructors[0];
                        explicitlyDeclared = !constructor.IsImplicitlyDeclared;

                        if (!explicitlyDeclared)
                        {
                            var rule = RuleXmlDocumentDerivedClassDesignNoConstructor;
                            context.ReportDiagnostic(CreateDiagnostic(typeSymbol.Locations, rule));
                        }
                    }

                    this.xmlDocumentDerivedTypes.AddOrUpdate(typeSymbol, explicitlyDeclared, (k, v) => explicitlyDeclared);
                }
            }

            // report warning if no explicit definition of contrsuctor in XmlTextReader derived types
            private void AnalyzeSymbolForXmlTextReaderDerivedType(SymbolAnalysisContext context)
            {
                ISymbol symbol = context.Symbol;
                if (symbol.Kind != SymbolKind.NamedType)
                {
                    return;
                }
                var typeSymbol = (INamedTypeSymbol)symbol;
                var xmlTextReaderSym = this.xmlTypes.XmlTextReader;
                if ((typeSymbol != xmlTextReaderSym) && typeSymbol.IsDerivedFrom(xmlTextReaderSym, baseTypesOnly: true))
                {
                    bool explicitlyDeclared = true;

                    if (typeSymbol.Constructors.Length == 1)
                    {
                        IMethodSymbol constructor = typeSymbol.Constructors[0];
                        explicitlyDeclared = !constructor.IsImplicitlyDeclared;

                        if (!explicitlyDeclared && !this.isFrameworkSecure)
                        {
                            var rule = RuleXmlTextReaderDerivedClassDesignNoConstructor;
                            context.ReportDiagnostic(CreateDiagnostic(typeSymbol.Locations, rule));
                        }
                    }

                    this.xmlTextReaderDerivedTypes.AddOrUpdate(typeSymbol, explicitlyDeclared, (k, v) => explicitlyDeclared);
                }
            }

            private bool IsAssigningIntendedValueToPropertyDerivedFromType(SyntaxNode assignment,
                SemanticModel model,
                Func<ISymbol, bool> isTargetPropertyFunc,
                Func<SyntaxNode, bool> isIntendedValueFunc,
                out bool isTargetProperty)
            {
                bool isIntendedValue;

                SyntaxNode left = this.syntaxNodeHelper.GetAssignmentLeftNode(assignment);
                SyntaxNode right = this.syntaxNodeHelper.GetAssignmentRightNode(assignment);

                var leftSymbol = SyntaxNodeHelper.GetCalleePropertySymbol(left, model);

                isTargetProperty = isTargetPropertyFunc(leftSymbol);

                if (!isTargetProperty)
                {
                    return false;
                }

                // call to isIntendedValueFunc must be after checking isTargetProperty
                // since the logic of isIntendedValueFunc relies on corresponding SyntaxNode
                isIntendedValue = isIntendedValueFunc(right);

                // Here's an example that needs some extra check:
                //
                //    class TestClass : XmlDocument 
                //    { 
                //        private XmlDocument doc = new XmlDocument();
                //        public TestClass(XmlDocument doc)
                //        {
                //            this.doc.XmlResolver = null;
                //        }
                //    }
                //
                // Even though the assignment would return true for both isTargetPropertyFunc and isIntendedValueFunc,
                // it is not setting the actual property for this class.

                // The goal is to find all assignment like in the example above, "this.xxx.xxx.Property = ...;".
                // For simplicity, here we adopt a simple but inaccurate logic:
                //   If the target is a member access node, then the only pattern we are looking for is "this.Property"
                SyntaxNode memberAccessNode = this.syntaxNodeHelper.GetDescendantMemberAccessExpressionNodes(left).FirstOrDefault();

                // if assignment target doesn't have any member access node, 
                // then we treat it as an instance property access without explicit 'this' ('Me' in VB)
                if (memberAccessNode == null)
                {
                    //stop here, to avoid false positive, as long as there's one setting <Property> to secure value, we are happy
                    return isIntendedValue;
                }

                var exp = this.syntaxNodeHelper.GetMemberAccessExpressionNode(memberAccessNode);
                var expSymbol = SyntaxNodeHelper.GetSymbol(exp, model);

                isTargetProperty = expSymbol.Kind == SymbolKind.Parameter && ((IParameterSymbol)expSymbol).IsThis;
                if (!isTargetProperty)
                {
                    return false;
                }

                var name = this.syntaxNodeHelper.GetMemberAccessNameNode(memberAccessNode);
                var nameSymbol = SyntaxNodeHelper.GetSymbol(name, model);

                isTargetProperty = isTargetPropertyFunc(nameSymbol);
                if (!isTargetProperty)
                {
                    return false;
                }

                // stop here, same reason as stated above
                return isIntendedValue;
            }

            public static Diagnostic CreateDiagnostic(
                Location location,
                DiagnosticDescriptor rule,
                params object[] args)
            {
                return CreateDiagnostic(new []{location}, rule, args);
            }

            public static Diagnostic CreateDiagnostic(
                IEnumerable<Location> locations,
                DiagnosticDescriptor rule,
                params object[] args)
            {
                var location = locations.First(l => l.IsInSource);
                var additionalLocations = locations.Where(l => l.IsInSource).Skip(1);
                return Diagnostic.Create(rule,
                         location: location,
                         additionalLocations: additionalLocations,
                         messageArgs: args);
            }
        }
    }
}
