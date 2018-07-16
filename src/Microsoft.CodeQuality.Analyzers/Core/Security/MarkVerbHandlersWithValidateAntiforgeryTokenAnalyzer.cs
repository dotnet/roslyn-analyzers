// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using System;

namespace Microsoft.CodeQuality.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MarkVerbHandlersWithValidateAntiforgeryTokenAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3147";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(MicrosoftSecurityAnalyzersResources.MarkVerbHandlersWithValidateAntiforgeryTokenTitle),
            MicrosoftSecurityAnalyzersResources.ResourceManager,
            typeof(MicrosoftSecurityAnalyzersResources));

        private static readonly LocalizableString NoVerbsMessage = new LocalizableResourceString(
            nameof(MicrosoftSecurityAnalyzersResources.MarkVerbHandlersWithValidateAntiforgeryTokenNoVerbsMessage),
            MicrosoftSecurityAnalyzersResources.ResourceManager,
            typeof(MicrosoftSecurityAnalyzersResources));

        private static readonly LocalizableString NoVerbsNoTokenMessage = new LocalizableResourceString(
            nameof(MicrosoftSecurityAnalyzersResources.MarkVerbHandlersWithValidateAntiforgeryTokenNoVerbsNoTokenMessage),
            MicrosoftSecurityAnalyzersResources.ResourceManager,
            typeof(MicrosoftSecurityAnalyzersResources));

        private static readonly LocalizableString GetAndTokenMessage = new LocalizableResourceString(
            nameof(MicrosoftSecurityAnalyzersResources.MarkVerbHandlersWithValidateAntiforgeryTokenGetAndTokenMessage),
            MicrosoftSecurityAnalyzersResources.ResourceManager,
            typeof(MicrosoftSecurityAnalyzersResources));

        private static readonly LocalizableString GetAndOtherAndTokenMessage = new LocalizableResourceString(
            nameof(MicrosoftSecurityAnalyzersResources.MarkVerbHandlersWithValidateAntiforgeryTokenGetAndOtherAndTokenMessage),
            MicrosoftSecurityAnalyzersResources.ResourceManager,
            typeof(MicrosoftSecurityAnalyzersResources));

        private static readonly LocalizableString VerbsAndNoTokenMessage = new LocalizableResourceString(
            nameof(MicrosoftSecurityAnalyzersResources.MarkVerbHandlersWithValidateAntiforgeryTokenVerbsAndNoTokenMessage),
            MicrosoftSecurityAnalyzersResources.ResourceManager,
            typeof(MicrosoftSecurityAnalyzersResources));

        private static readonly DiagnosticDescriptor NoVerbsRule = new DiagnosticDescriptor(
            RuleId,
            Title, 
            NoVerbsMessage, 
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: false);

        private static readonly DiagnosticDescriptor NoVerbsNoTokenRule = new DiagnosticDescriptor(
            RuleId,
            Title,
            NoVerbsNoTokenMessage,
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: false);

        private static readonly DiagnosticDescriptor GetAndTokenRule = new DiagnosticDescriptor(
            RuleId,
            Title,
            GetAndTokenMessage,
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: false);

        private static readonly DiagnosticDescriptor GetAndOtherAndTokenRule = new DiagnosticDescriptor(
            RuleId,
            Title,
            GetAndOtherAndTokenMessage,
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: false);

        private static readonly DiagnosticDescriptor VerbsAndNoTokenRule = new DiagnosticDescriptor(
            RuleId,
            Title,
            VerbsAndNoTokenMessage,
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoVerbsRule, NoVerbsNoTokenRule, GetAndTokenRule, GetAndOtherAndTokenRule, VerbsAndNoTokenRule);

        /// <summary>
        /// Helper for examining System.Web.Mvc attributes on MVC controller methods.
        /// </summary>
        private class MvcAttributeSymbols
        {
            INamedTypeSymbol ValidateAntiforgeryTokenAttributeSymbol { get; set; }
            INamedTypeSymbol HttpGetAttributeSymbol { get; set; }
            INamedTypeSymbol HttpPostAttributeSymbol { get; set; }
            INamedTypeSymbol HttpPutAttributeSymbol { get; set; }
            INamedTypeSymbol HttpDeleteAttributeSymbol { get; set; }
            INamedTypeSymbol HttpPatchAttributeSymbol { get; set; }
            INamedTypeSymbol AcceptVerbsAttributeSymbol { get; set; }
            INamedTypeSymbol NonActionAttributeSymbol { get; set; }
            INamedTypeSymbol ChildActionOnlyAttributeSymbol { get; set; }
            INamedTypeSymbol HttpVerbsSymbol { get; set; }

            public MvcAttributeSymbols(Compilation compilation)
            {
                this.ValidateAntiforgeryTokenAttributeSymbol = WellKnownTypes.ValidateAntiforgeryTokenAttribute(compilation);
                this.HttpGetAttributeSymbol = WellKnownTypes.HttpGetAttribute(compilation);
                this.HttpPostAttributeSymbol = WellKnownTypes.HttpPostAttribute(compilation);
                this.HttpPutAttributeSymbol = WellKnownTypes.HttpPutAttribute(compilation);
                this.HttpDeleteAttributeSymbol = WellKnownTypes.HttpDeleteAttribute(compilation);
                this.HttpPatchAttributeSymbol = WellKnownTypes.HttpPatchAttribute(compilation);
                this.AcceptVerbsAttributeSymbol = WellKnownTypes.AcceptVerbsAttribute(compilation);
                this.NonActionAttributeSymbol = WellKnownTypes.NonActionAttribute(compilation);
                this.ChildActionOnlyAttributeSymbol = WellKnownTypes.ChildActionOnlyAttribute(compilation);
                this.HttpVerbsSymbol = WellKnownTypes.HttpVerbs(compilation);
            }

            /// <summary>
            /// Gets relevant info from the attributes on the MVC controller action we're examining.
            /// </summary>
            /// <param name="attributeDatas">Attributes on the MVC controller action.</param>
            /// <param name="verbs">Information on which HTTP verbs are specified.</param>
            /// <param name="antiforgeryTokenDefined">Indicates that the ValidateAntiforgeryToken attribute was specified.</param>
            /// <param name="isAction">Indicates that the MVC controller method doesn't have an attribute saying it's not really an action.</param>
            public void ComputeAttributeInfo(
                ImmutableArray<AttributeData> attributeDatas, 
                out MvcHttpVerbs verbs,
                out bool antiforgeryTokenDefined,
                out bool isAction)
            {
                verbs = default(MvcHttpVerbs);
                antiforgeryTokenDefined = false;
                isAction = true;    // Presumed an MVC controller action until proven otherwise.

                foreach (AttributeData a in attributeDatas)
                {
                    if (IsAttributeClass(a, this.ValidateAntiforgeryTokenAttributeSymbol))
                    {
                        antiforgeryTokenDefined = true;
                    }
                    else if (IsAttributeClass(a, this.HttpGetAttributeSymbol))
                    {
                        verbs |= MvcHttpVerbs.Get;
                    }
                    else if (IsAttributeClass(a, this.HttpPostAttributeSymbol))
                    {
                        verbs |= MvcHttpVerbs.Post;
                    }
                    else if (IsAttributeClass(a, this.HttpPutAttributeSymbol))
                    {
                        verbs |= MvcHttpVerbs.Put;
                    }
                    else if (IsAttributeClass(a, this.HttpDeleteAttributeSymbol))
                    {
                        verbs |= MvcHttpVerbs.Delete;
                    }
                    else if (IsAttributeClass(a, this.HttpPatchAttributeSymbol))
                    {
                        verbs |= MvcHttpVerbs.Patch;
                    }
                    else if (IsAttributeClass(a, this.AcceptVerbsAttributeSymbol))
                    {
                        if (a.AttributeConstructor.Parameters != null
                            && a.AttributeConstructor.Parameters.Length == 1)
                        {
                            ITypeSymbol parameterType = a.AttributeConstructor.Parameters[0].Type;
                            if (a.AttributeConstructor.Parameters[0].IsParams
                                && parameterType.TypeKind == TypeKind.Array)
                            {
                                IArrayTypeSymbol parameterArrayType = (IArrayTypeSymbol) parameterType;
                                if (parameterArrayType.Rank == 1
                                    && parameterArrayType.ElementType.SpecialType == SpecialType.System_String)
                                {
                                    // The [AcceptVerbs("Put", "Post")] case.

                                    foreach (TypedConstant tc in a.ConstructorArguments[0].Values)
                                    {
                                        string s = tc.Value as string;
                                        MvcHttpVerbs v;
                                        if (s != null && Enum.TryParse(s, true /* ignoreCase */, out v))
                                        {
                                            verbs |= v;
                                        }
                                    }

                                    continue;
                                }
                            }
                            else if (parameterType.TypeKind == TypeKind.Enum
                                && parameterType == this.HttpVerbsSymbol)
                            {
                                // The [AcceptVerbs(HttpVerbs.Delete)] case.

                                int i = (int) a.ConstructorArguments[0].Value;
                                verbs |= (MvcHttpVerbs) i;

                                continue;
                            }
                        }
                        
                        // If we reach here, then we didn't handle the [AcceptVerbs] constructor overload.
                    }
                    else if (IsAttributeClass(a, this.NonActionAttributeSymbol)
                        || IsAttributeClass(a, this.ChildActionOnlyAttributeSymbol))
                    {
                        isAction = false;
                    }
                }
            }

            /// <summary>
            /// Determines if the .NET attribute is of the specified type.
            /// </summary>
            /// <param name="attributeData">The .NET attribute to check.</param>
            /// <param name="symbol">The type of .NET attribute to compare.</param>
            /// <returns>True if .NET attribute's type matches the specified type, false otherwise.</returns>
            private static bool IsAttributeClass(AttributeData attributeData, INamedTypeSymbol symbol)
            {
                return symbol != null && attributeData.AttributeClass == symbol;
            }
        }

        /// <summary>
        /// ASP.NET MVC's implementation of HttpVerbs.
        /// </summary>
        [Flags]
        private enum MvcHttpVerbs
        {
            None = 0,

            /// <summary>
            /// Retrieves the information or entity that is identified by the URI of the request.
            /// </summary>
            Get = 1,

            /// <summary>
            /// Posts a new entity as an addition to a URI.
            /// </summary>
            Post = 2,

            /// <summary>
            /// Replaces an entity that is identified by a URI.
            /// </summary>
            Put = 4,

            /// <summary>
            /// Requests that a specified URI be deleted.
            /// </summary>
            Delete = 8,

            /// <summary>
            /// Retrieves the message headers for the information or entity that is identified by the URI of the request.
            /// </summary>
            Head = 0x10,

            /// <summary>
            /// Requests that a set of changes described in the request entity be applied to the resource identified by the Request-URI.
            /// </summary>
            Patch = 0x20,

            /// <summary>
            /// Represents a request for information about the communication options available on the request/response chain identified by the Request-URI.
            /// </summary>
            Options = 0x40
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartContext) =>
                {
                    INamedTypeSymbol mvcControllerSymbol = WellKnownTypes.MvcController(compilationStartContext.Compilation);
                    INamedTypeSymbol mvcControllerBaseSymbol = WellKnownTypes.MvcControllerBase(compilationStartContext.Compilation);
                    INamedTypeSymbol actionResultSymbol = WellKnownTypes.ActionResult(compilationStartContext.Compilation);

                    if ((mvcControllerSymbol == null && mvcControllerBaseSymbol == null) || actionResultSymbol == null)
                    {
                        // No MVC controllers that return an ActionResult here.
                        return;
                    }

                    MvcAttributeSymbols mvcAttributeSymbols = new MvcAttributeSymbols(compilationStartContext.Compilation);

                    compilationStartContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolContext) =>
                        {
                            // TODO enhancements: Consider looking at non-ActionResult-derived return types as well.
                            IMethodSymbol methodSymbol = symbolContext.Symbol as IMethodSymbol;
                            if (methodSymbol == null
                                || methodSymbol.MethodKind != MethodKind.Ordinary 
                                || methodSymbol.IsStatic
                                || !methodSymbol.IsPublic()
                                || !methodSymbol.ReturnType.Inherits(actionResultSymbol)  // FxCop implementation only looks at ActionResult-derived return types.
                                || (!methodSymbol.ContainingType.Inherits(mvcControllerSymbol)
                                    && !methodSymbol.ContainingType.Inherits(mvcControllerBaseSymbol)))
                            {
                                return;
                            }

                            ImmutableArray<AttributeData> methodAttributes = methodSymbol.GetAttributes();
                            MvcHttpVerbs verbs;
                            bool isAntiforgeryTokenDefined;
                            bool isAction;
                            mvcAttributeSymbols.ComputeAttributeInfo(methodAttributes, out verbs, out isAntiforgeryTokenDefined, out isAction);

                            if (!isAction)
                            {
                                return;
                            }

                            if (verbs == MvcHttpVerbs.None)
                            {
                                // no verbs specified
                                if (isAntiforgeryTokenDefined)
                                {
                                    // antiforgery token attribute is set, but verbs are not specified
                                    symbolContext.ReportDiagnostic(Diagnostic.Create(NoVerbsRule, methodSymbol.Locations[0], methodSymbol.MetadataName));
                                }
                                else
                                {
                                    // no verbs, no antiforgery token attribute
                                    symbolContext.ReportDiagnostic(Diagnostic.Create(NoVerbsNoTokenRule, methodSymbol.Locations[0], methodSymbol.MetadataName));
                                }
                            }
                            else
                            {
                                // verbs are defined 
                                if (isAntiforgeryTokenDefined)
                                {
                                    if (verbs.HasFlag(MvcHttpVerbs.Get))
                                    {
                                        symbolContext.ReportDiagnostic(Diagnostic.Create(GetAndTokenRule, methodSymbol.Locations[0], methodSymbol.MetadataName));

                                        if ((verbs & (MvcHttpVerbs.Post | MvcHttpVerbs.Put | MvcHttpVerbs.Delete | MvcHttpVerbs.Patch)) != MvcHttpVerbs.None)
                                        {
                                            // both verbs, antiforgery token attribute
                                            symbolContext.ReportDiagnostic(Diagnostic.Create(GetAndOtherAndTokenRule, methodSymbol.Locations[0], methodSymbol.MetadataName));
                                        }
                                    }
                                }
                                else
                                {
                                    if ((verbs & (MvcHttpVerbs.Post | MvcHttpVerbs.Put | MvcHttpVerbs.Delete | MvcHttpVerbs.Patch)) != MvcHttpVerbs.None)
                                    {
                                        // HttpPost, no antiforgery token attribute
                                        symbolContext.ReportDiagnostic(Diagnostic.Create(VerbsAndNoTokenRule, methodSymbol.Locations[0], methodSymbol.MetadataName));
                                    }
                                }
                            }
                        },
                        SymbolKind.Method);
                }
            );
        }
    }
}
