// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1724: Type names should not match namespaces
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class TypeNamesShouldNotMatchNamespacesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1724";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.TypeNamesShouldNotMatchNamespacesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.TypeNamesShouldNotMatchNamespacesMessageDefault), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystem = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.TypeNamesShouldNotMatchNamespacesMessageSystem), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.TypeNamesShouldNotMatchNamespacesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182257.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystem,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182257.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, SystemRule);

        private static object s_obj = new object();
        private static ImmutableDictionary<string, string> s_wellKnownSystemNamespaceTable;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                compilationStartAnalysisContext =>
                {
                    var namedTypesInCompilation = new ConcurrentBag<INamedTypeSymbol>();

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        SymbolAnalysisContext =>
                        {
                            var namedType = (INamedTypeSymbol)SymbolAnalysisContext.Symbol;
                            namedTypesInCompilation.Add(namedType);
                        }, SymbolKind.NamedType);

                    compilationStartAnalysisContext.RegisterCompilationEndAction(
                        compilationAnalysisContext =>
                        {
                            var namespaceNamesInCompilation = new ConcurrentBag<string>();
                            var compilation = compilationAnalysisContext.Compilation;
                            AddNamespacesFromCompilation(namespaceNamesInCompilation, compilation.GlobalNamespace);

                            // The entries are added in the sorted order so that the diagnostic message is always deterministic
                            var namespaceTableForCompilation = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            UpdateNamespaceTable(namespaceTableForCompilation, namespaceNamesInCompilation.ToImmutableSortedSet());

                            InitializeWellKnownSystemNamespaceTable();
                            foreach (var symbol in namedTypesInCompilation)
                            {
                                var symbolName = symbol.Name;
                                if (s_wellKnownSystemNamespaceTable.ContainsKey(symbolName))
                                {
                                    compilationAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(SystemRule, symbolName, s_wellKnownSystemNamespaceTable[symbolName]));
                                }
                                else if (namespaceTableForCompilation.ContainsKey(symbolName))
                                {
                                    compilationAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(DefaultRule, symbolName, namespaceTableForCompilation[symbolName]));
                                }
                            }
                        });
                });
        }

        private void AddNamespacesFromCompilation(ConcurrentBag<string> namespaceNamesInCompilation, INamespaceSymbol @namespace)
        {
            namespaceNamesInCompilation.Add(@namespace.ToDisplayString());

            foreach (var namespaceMember in @namespace.GetNamespaceMembers())
            {
                AddNamespacesFromCompilation(namespaceNamesInCompilation, namespaceMember);
            }
        }

        private void InitializeWellKnownSystemNamespaceTable()
        {
            if (s_wellKnownSystemNamespaceTable == null)
            {
                lock(s_obj)
                {
                    if (s_wellKnownSystemNamespaceTable == null)
                    {

#region List of Well known System Namespaces
                        var wellKnownSystemNamespaces = new List<string>
                                {
                                    "Microsoft.CSharp",
                                    "Microsoft.SqlServer.Server",
                                    "Microsoft.VisualBasic",
                                    "Microsoft.Win32",
                                    "Microsoft.Win32.SafeHandles",
                                    "System",
                                    "System.CodeDom",
                                    "System.CodeDom.Compiler",
                                    "System.Collections",
                                    "System.Collections.Generic",
                                    "System.Collections.ObjectModel",
                                    "System.Collections.Specialized",
                                    "System.ComponentModel",
                                    "System.ComponentModel.Design",
                                    "System.ComponentModel.Design.Serialization",
                                    "System.Configuration",
                                    "System.Configuration.Assemblies",
                                    "System.Data",
                                    "System.Data.Common",
                                    "System.Data.Odbc",
                                    "System.Data.OleDb",
                                    "System.Data.Sql",
                                    "System.Data.SqlClient",
                                    "System.Data.SqlTypes",
                                    "System.Deployment.Internal",
                                    "System.Diagnostics",
                                    "System.Diagnostics.CodeAnalysis",
                                    "System.Diagnostics.SymbolStore",
                                    "System.Drawing",
                                    "System.Drawing.Design",
                                    "System.Drawing.Drawing2D",
                                    "System.Drawing.Imaging",
                                    "System.Drawing.Printing",
                                    "System.Drawing.Text",
                                    "System.Globalization",
                                    "System.IO",
                                    "System.IO.Compression",
                                    "System.IO.IsolatedStorage",
                                    "System.IO.Ports",
                                    "System.Media",
                                    "System.Net",
                                    "System.Net.Cache",
                                    "System.Net.Configuration",
                                    "System.Net.Mail",
                                    "System.Net.Mime",
                                    "System.Net.NetworkInformation",
                                    "System.Net.Security",
                                    "System.Net.Sockets",
                                    "System.Reflection",
                                    "System.Reflection.Emit",
                                    "System.Resources",
                                    "System.Runtime",
                                    "System.Runtime.CompilerServices",
                                    "System.Runtime.ConstrainedExecution",
                                    "System.Runtime.Hosting",
                                    "System.Runtime.InteropServices",
                                    "System.Runtime.InteropServices.ComTypes",
                                    "System.Runtime.InteropServices.Expando",
                                    "System.Runtime.Remoting",
                                    "System.Runtime.Remoting.Activation",
                                    "System.Runtime.Remoting.Channels",
                                    "System.Runtime.Remoting.Contexts",
                                    "System.Runtime.Remoting.Lifetime",
                                    "System.Runtime.Remoting.Messaging",
                                    "System.Runtime.Remoting.Metadata",
                                    "System.Runtime.Remoting.Metadata.W3cXsd2001",
                                    "System.Runtime.Remoting.Proxies",
                                    "System.Runtime.Remoting.Services",
                                    "System.Runtime.Serialization",
                                    "System.Runtime.Serialization.Formatters",
                                    "System.Runtime.Serialization.Formatters.Binary",
                                    "System.Runtime.Versioning",
                                    "System.Security",
                                    "System.Security.AccessControl",
                                    "System.Security.Authentication",
                                    "System.Security.Cryptography",
                                    "System.Security.Cryptography.X509Certificates",
                                    "System.Security.Permissions",
                                    "System.Security.Policy",
                                    "System.Security.Principal",
                                    "System.Text",
                                    "System.Text.RegularExpressions",
                                    "System.Threading",
                                    "System.Timers",
                                    "System.Web",
                                    "System.Web.Caching",
                                    "System.Web.Compilation",
                                    "System.Web.Configuration",
                                    "System.Web.Configuration.Internal",
                                    "System.Web.Handlers",
                                    "System.Web.Hosting",
                                    "System.Web.Mail",
                                    "System.Web.Management",
                                    "System.Web.Profile",
                                    "System.Web.Security",
                                    "System.Web.SessionState",
                                    "System.Web.UI",
                                    "System.Web.UI.Adapters",
                                    "System.Web.UI.HtmlControls",
                                    "System.Web.UI.WebControls",
                                    "System.Web.UI.WebControls.Adapters",
                                    "System.Web.UI.WebControls.WebParts",
                                    "System.Web.Util",
                                    "System.Windows.Forms",
                                    "System.Windows.Forms.ComponentModel.Com2Interop",
                                    "System.Windows.Forms.Design",
                                    "System.Windows.Forms.Layout",
                                    "System.Windows.Forms.PropertyGridInternal",
                                    "System.Windows.Forms.VisualStyles",
                                    "System.Xml",
                                    "System.Xml.Schema",
                                    "System.Xml.Serialization",
                                    "System.Xml.Serialization.Advanced",
                                    "System.Xml.Serialization.Configuration",
                                    "System.Xml.XPath",
                                    "System.Xml.Xsl"
                                };
#endregion

                        var wellKnownSystemNamespaceTable = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        UpdateNamespaceTable(wellKnownSystemNamespaceTable, wellKnownSystemNamespaces);
                        s_wellKnownSystemNamespaceTable = wellKnownSystemNamespaceTable.ToImmutableDictionary();
                    }
                }
            }
        }

        private static void UpdateNamespaceTable(Dictionary<string, string> namespaceTable, IList<string> namespaces)
        {
            if (namespaces == null)
            {
                return;
            }

            foreach (string namespaceName in namespaces)
            {
                UpdateNamespaceTable(namespaceTable, namespaceName);
            }
        }

        private static void UpdateNamespaceTable(Dictionary<string, string> namespaceTable, string namespaceName)
        {
            foreach (string word in namespaceName.Split('.'))
            {
                if (!namespaceTable.ContainsKey(word))
                    namespaceTable.Add(word, namespaceName);
            }
        }
    }
}