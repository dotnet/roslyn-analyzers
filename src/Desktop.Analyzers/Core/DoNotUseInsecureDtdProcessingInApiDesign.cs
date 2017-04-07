﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Collections.Concurrent;
using Analyzer.Utilities.Extensions;
using Desktop.Analyzers.Helpers;

namespace Desktop.Analyzers
{
    public abstract class DoNotUseInsecureDtdProcessingInApiDesignAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3077";
        private const string HelpLink = "http://aka.ms/CA3077";

        internal static DiagnosticDescriptor RuleDoNotUseInsecureDtdProcessingInApiDesign = CreateDiagnosticDescriptor(SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureDtdProcessingGenericMessage)),
                                                                                                                        SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.DoNotUseInsecureDtdProcessingInApiDesignDescription)),
                                                                                                                         HelpLink);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleDoNotUseInsecureDtdProcessingInApiDesign);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Make analyzer thread-safe.
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

                        // bail if we are not analyzing project targeting .NET Framework
                        // TODO: should we throw an exception to notify user?
                        if (version != null)
                        {
                            SymbolAndNodeAnalyzer analyzer = GetAnalyzer(context, xmlTypes, version);
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
                                            SecurityDiagnosticHelpers.GetLocalizableResourceString(nameof(DesktopAnalyzersResources.InsecureDtdProcessingInApiDesign)),
                                            messageFormat,
                                            DiagnosticCategory.Security,
                                            DiagnosticSeverity.Warning,
                                            isEnabledByDefault: true,
                                            description: description,
                                            helpLinkUri: helpLink,
                                            customTags: WellKnownDiagnosticTags.Telemetry);
        }

        protected abstract SymbolAndNodeAnalyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes types, Version targetFrameworkVersion);

        protected sealed class SymbolAndNodeAnalyzer
        {
            // .NET frameworks >= 4.5.2 have secure default settings for XmlTextReader:
            //      DtdProcessing is enabled with null resolver
            private static readonly Version s_minSecureFxVersion = new Version(4, 5, 2);

            private readonly CompilationSecurityTypes _xmlTypes;
            private readonly SyntaxNodeHelper _syntaxNodeHelper;
            private readonly bool _isFrameworkSecure;

            // key: symbol for type derived from XmlDocument/XmlTextReader (exclude base type itself)
            // value: if it has explicitly defined constructor
            private readonly ConcurrentDictionary<INamedTypeSymbol, bool> _xmlDocumentDerivedTypes = new ConcurrentDictionary<INamedTypeSymbol, bool>();
            private readonly ConcurrentDictionary<INamedTypeSymbol, bool> _xmlTextReaderDerivedTypes = new ConcurrentDictionary<INamedTypeSymbol, bool>();


            public SymbolAndNodeAnalyzer(CompilationSecurityTypes xmlTypes, SyntaxNodeHelper helper, Version targetFrameworkVersion)
            {
                _xmlTypes = xmlTypes;
                _syntaxNodeHelper = helper;
                _isFrameworkSecure = targetFrameworkVersion == null ? false : targetFrameworkVersion >= SymbolAndNodeAnalyzer.s_minSecureFxVersion;
            }

            public void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
                // an alternative is to do syntax analysis during the symbol analysis, which might cause AST construction on the fly
                AnalyzeNodeForXmlDocumentDerivedTypeConstructorDecl(context);
                AnalyzeNodeForXmlDocumentDerivedTypeMethodDecl(context);
                if (!_isFrameworkSecure)
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
                    !((methodSymbol.ContainingType != _xmlTypes.XmlDocument) && methodSymbol.ContainingType.DerivesFrom(_xmlTypes.XmlDocument, baseTypesOnly: true)))
                {
                    return;
                }

                bool hasSetSecureXmlResolver = false;

                IEnumerable<SyntaxNode> assignments = _syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (SyntaxNode a in assignments)
                {
                    // this is intended to be an assignment, not a bug
                    if (hasSetSecureXmlResolver = IsAssigningIntendedValueToPropertyDerivedFromType(a,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverProperty(s, _xmlTypes);
                            },
                            (n) =>
                            {
                                return SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                    SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, _xmlTypes);
                            },
                            out bool isTargetProperty))
                    {
                        break;
                    }
                }

                if (!hasSetSecureXmlResolver)
                {
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessingInApiDesign;
                    context.ReportDiagnostic(
                        CreateDiagnostic(
                            methodSymbol.Locations,
                            rule,
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.XmlDocumentDerivedClassConstructorNoSecureXmlResolverMessage),
                                SecurityDiagnosticHelpers.GetNonEmptyParentName(node, model)
                            )
                        )
                    );
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
                    !((methodSymbol.ContainingType != _xmlTypes.XmlDocument) && methodSymbol.ContainingType.DerivesFrom(_xmlTypes.XmlDocument, baseTypesOnly: true)))
                {
                    return;
                }

                IEnumerable<SyntaxNode> assignments = _syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (SyntaxNode assignment in assignments)
                {
                    // this is intended to be an assignment, not a bug
                    if (IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlDocumentXmlResolverProperty(s, _xmlTypes);
                            },
                            (n) =>
                            {
                                return !(SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                         SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, _xmlTypes));
                            },
                            out bool isTargetProperty)
                        )
                    {
                        DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessingInApiDesign;
                        context.ReportDiagnostic(
                            CreateDiagnostic(
                                assignment.GetLocation(),
                                rule,
                                SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                    nameof(DesktopAnalyzersResources.XmlDocumentDerivedClassSetInsecureXmlResolverInMethodMessage),
                                    methodSymbol.Name
                                )
                            )
                        );
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
                    !((methodSymbol.ContainingType != _xmlTypes.XmlTextReader) && methodSymbol.ContainingType.DerivesFrom(_xmlTypes.XmlTextReader, baseTypesOnly: true)))
                {
                    return;
                }

                bool hasSetSecureXmlResolver = false;
                bool isDtdProcessingDisabled = false;

                IEnumerable<SyntaxNode> assignments = _syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (SyntaxNode assignment in assignments)
                {
                    bool isTargetProperty = false;

                    hasSetSecureXmlResolver = hasSetSecureXmlResolver || IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverProperty(s, _xmlTypes);
                            },
                            (n) =>
                            {
                                return SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                       SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, _xmlTypes);
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
                                return SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingProperty(s, _xmlTypes);
                            },
                            (n) =>
                            {
                                return !SyntaxNodeHelper.GetSymbol(n, model).MatchFieldByName(_xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
                            },
                            out isTargetProperty);

                    if (hasSetSecureXmlResolver && isDtdProcessingDisabled)
                    {
                        return;
                    }
                }

                DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessingInApiDesign;
                context.ReportDiagnostic(
                    CreateDiagnostic(
                        methodSymbol.Locations,
                        rule,
                        SecurityDiagnosticHelpers.GetLocalizableResourceString(
                            nameof(DesktopAnalyzersResources.XmlTextReaderDerivedClassConstructorNoSecureSettingsMessage),
                            SecurityDiagnosticHelpers.GetNonEmptyParentName(node, model)
                        )
                    )
                );
            }

            private void AnalyzeNodeForXmlTextReaderDerivedTypeMethodDecl(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;

                IMethodSymbol methodSymbol = SyntaxNodeHelper.GetDeclaredSymbol(node, model) as IMethodSymbol;

                if (methodSymbol == null ||
                   !((methodSymbol.ContainingType != _xmlTypes.XmlTextReader) && methodSymbol.ContainingType.DerivesFrom(_xmlTypes.XmlTextReader, baseTypesOnly: true)))
                {
                    return;
                }

                // If the default value are not secure, the AnalyzeNodeForXmlTextReaderDerivedTypeConstructorDecl would be skipped,
                // therefoer we need to check constructor for any insecure settings.
                // Otherwise, we skip checking constructors
                if (_isFrameworkSecure && methodSymbol.MethodKind == MethodKind.Constructor)
                {
                    return;
                }

                bool hasSetXmlResolver = false;
                bool hasSetInsecureXmlResolver = true;
                bool isDtdProcessingSet = false;
                bool isDtdProcessingEnabled = true;

                List<Location> locs = null;
                Location insecureXmlResolverAssignLoc = null;
                Location issecureDtdProcessingLoc = null;

                IEnumerable<SyntaxNode> assignments = _syntaxNodeHelper.GetDescendantAssignmentExpressionNodes(node);
                foreach (SyntaxNode assignment in assignments)
                {
                    bool ret;

                    ret = IsAssigningIntendedValueToPropertyDerivedFromType(assignment,
                            model,
                            (s) =>
                            {
                                return SecurityDiagnosticHelpers.IsXmlTextReaderXmlResolverProperty(s, _xmlTypes);
                            },
                            (n) =>
                            {
                                return !(SyntaxNodeHelper.NodeHasConstantValueNull(n, model) ||
                                        SecurityDiagnosticHelpers.IsXmlSecureResolverType(model.GetTypeInfo(n).Type, _xmlTypes));
                            },
                            out bool isTargetProperty
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
                                return SecurityDiagnosticHelpers.IsXmlTextReaderDtdProcessingProperty(s, _xmlTypes);
                            },
                            (n) =>
                            {
                                return SyntaxNodeHelper.GetSymbol(n, model).MatchFieldByName(_xmlTypes.DtdProcessing, SecurityMemberNames.Parse);
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
                    if (issecureDtdProcessingLoc != null)
                    {
                        locs.Add(issecureDtdProcessingLoc);
                    }
                    DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessingInApiDesign;
                    // TODO: Only first location is shown in error, maybe we want to report on method instead?
                    //       Or on each insecure assignment?
                    context.ReportDiagnostic(
                        CreateDiagnostic(
                            locs,
                            rule,
                            SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                nameof(DesktopAnalyzersResources.XmlTextReaderDerivedClassSetInsecureSettingsInMethodMessage),
                                methodSymbol.Name
                            )
                        )
                    );
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
                INamedTypeSymbol xmlDocumentSym = _xmlTypes.XmlDocument;
                if ((typeSymbol != xmlDocumentSym) && typeSymbol.DerivesFrom(xmlDocumentSym, baseTypesOnly: true))
                {
                    bool explicitlyDeclared = true;

                    if (typeSymbol.Constructors.Length == 1)
                    {
                        IMethodSymbol constructor = typeSymbol.Constructors[0];
                        explicitlyDeclared = !constructor.IsImplicitlyDeclared;

                        if (!explicitlyDeclared)
                        {
                            DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessingInApiDesign;
                            context.ReportDiagnostic(
                                CreateDiagnostic(
                                    typeSymbol.Locations,
                                    rule,
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(DesktopAnalyzersResources.XmlDocumentDerivedClassNoConstructorMessage),
                                        typeSymbol.Name
                                    )
                                )
                            );
                        }
                    }

                    _xmlDocumentDerivedTypes.AddOrUpdate(typeSymbol, explicitlyDeclared, (k, v) => explicitlyDeclared);
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
                INamedTypeSymbol xmlTextReaderSym = _xmlTypes.XmlTextReader;
                if ((typeSymbol != xmlTextReaderSym) && typeSymbol.DerivesFrom(xmlTextReaderSym, baseTypesOnly: true))
                {
                    bool explicitlyDeclared = true;

                    if (typeSymbol.Constructors.Length == 1)
                    {
                        IMethodSymbol constructor = typeSymbol.Constructors[0];
                        explicitlyDeclared = !constructor.IsImplicitlyDeclared;

                        if (!explicitlyDeclared && !_isFrameworkSecure)
                        {
                            DiagnosticDescriptor rule = RuleDoNotUseInsecureDtdProcessingInApiDesign;
                            context.ReportDiagnostic(
                                CreateDiagnostic(
                                    typeSymbol.Locations,
                                    rule,
                                    SecurityDiagnosticHelpers.GetLocalizableResourceString(
                                        nameof(DesktopAnalyzersResources.XmlTextReaderDerivedClassNoConstructorMessage),
                                        symbol.Name
                                    )
                                )
                            );
                        }
                    }

                    _xmlTextReaderDerivedTypes.AddOrUpdate(typeSymbol, explicitlyDeclared, (k, v) => explicitlyDeclared);
                }
            }

            private bool IsAssigningIntendedValueToPropertyDerivedFromType(SyntaxNode assignment,
                SemanticModel model,
                Func<IPropertySymbol, bool> isTargetPropertyFunc,
                Func<SyntaxNode, bool> isIntendedValueFunc,
                out bool isTargetProperty)
            {
                bool isIntendedValue;

                SyntaxNode left = _syntaxNodeHelper.GetAssignmentLeftNode(assignment);
                SyntaxNode right = _syntaxNodeHelper.GetAssignmentRightNode(assignment);

                IPropertySymbol leftSymbol = SyntaxNodeHelper.GetCalleePropertySymbol(left, model);

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
                SyntaxNode memberAccessNode = _syntaxNodeHelper.GetDescendantMemberAccessExpressionNodes(left).FirstOrDefault();

                // if assignment target doesn't have any member access node, 
                // then we treat it as an instance property access without explicit 'this' ('Me' in VB)
                if (memberAccessNode == null)
                {
                    //stop here, to avoid false positive, as long as there's one setting <Property> to secure value, we are happy
                    return isIntendedValue;
                }

                SyntaxNode exp = _syntaxNodeHelper.GetMemberAccessExpressionNode(memberAccessNode);
                ISymbol expSymbol = SyntaxNodeHelper.GetSymbol(exp, model);

                isTargetProperty = expSymbol.Kind == SymbolKind.Parameter && ((IParameterSymbol)expSymbol).IsThis;
                if (!isTargetProperty)
                {
                    return false;
                }

                SyntaxNode name = _syntaxNodeHelper.GetMemberAccessNameNode(memberAccessNode);
                ISymbol nameSymbol = SyntaxNodeHelper.GetSymbol(name, model);

                isTargetProperty = isTargetPropertyFunc(nameSymbol as IPropertySymbol);
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
                return CreateDiagnostic(new[] { location }, rule, args);
            }

            public static Diagnostic CreateDiagnostic(
                IEnumerable<Location> locations,
                DiagnosticDescriptor rule,
                params object[] args)
            {
                Location location = locations.First(l => l.IsInSource);
                IEnumerable<Location> additionalLocations = locations.Where(l => l.IsInSource).Skip(1);
                return Diagnostic.Create(rule,
                         location: location,
                         additionalLocations: additionalLocations,
                         messageArgs: args);
            }
        }
    }
}
