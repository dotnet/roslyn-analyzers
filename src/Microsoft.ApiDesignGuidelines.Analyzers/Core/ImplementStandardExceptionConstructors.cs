// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1032 - redefined: Implement standard exception constructors
    /// Cause: A type extends System.Exception and does not declare all the required constructors. 
    /// Description: Exception types must implement the following constructors. Failure to provide the full set of constructors can make it difficult to correctly handle exceptions
    ///     public GoodException()
    ///     public GoodException(string)
    ///     public GoodException(string, Exception)
    /// </summary>

    public abstract class ImplementStandardExceptionConstructorsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1032";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementStandardExceptionConstructorsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));      
        private static readonly LocalizableString s_localizableMessageMissingConstructor = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementStandardExceptionConstructorsMessageMissingConstructor), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementStandardExceptionConstructorsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        
        internal static DiagnosticDescriptor MissingConstructorRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMissingConstructor,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182151.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MissingConstructorRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // Analyze named types 
            analysisContext.RegisterSymbolAction(symbolContext =>
            {
                AnalyzeSymbol(symbolContext.Symbol, symbolContext);

            }, SymbolKind.NamedType);
        }

        //abstract methods, which the language specific analyzers implements - these will return the required constructor method signatures for CSharp/Basic
        protected abstract string GetParameterLessConstructorMessage(ISymbol symbol);
        protected abstract string GetConstructorWithStringTypeParameter(ISymbol symbol);
        protected abstract string GetConstructorWithStringAndExceptionTypeParameter(ISymbol symbol);

        private void AnalyzeSymbol(ISymbol symbol, SymbolAnalysisContext context)
        {
            var namedTypeSymbol = context.Symbol as INamedTypeSymbol;

            //Check if type derives from Exception type
            if (namedTypeSymbol.BaseType.MetadataName == context.Compilation.GetTypeByMetadataName("System.Exception").MetadataName)
            {
                //Get the list of constructors
                var constructors = namedTypeSymbol.Constructors;

                //Set flags for the 3 different constructos, that is being searched for
                var defaultConstructorFound = false; //flag for default constructor
                var secondConstructorFound = false; //flag for constructor with string type parameter
                var thirdConstructorFound = false; //flag for constructor with string and exception type parameter

                foreach (var ctor in constructors)
                {
                    var parameters = ctor.GetParameters();

                    //case 1: Default constructor - no parameters
                    if (parameters.Length == 0)
                    {
                        defaultConstructorFound = true;
                    }
                    //case 2: Constructor with string type parameter
                    else if (parameters.Length == 1 && parameters[0].Type.SpecialType == SpecialType.System_String)
                    {
                        secondConstructorFound = true;
                    }
                    //case 3: Constructor with string type and exception type parameter
                    else if (parameters.Length == 2 && parameters[0].Type.SpecialType == SpecialType.System_String &&
                            parameters[1].Type == context.Compilation.GetTypeByMetadataName("System.Exception"))
                    {
                        thirdConstructorFound = true;
                    }

                    if (defaultConstructorFound && secondConstructorFound && thirdConstructorFound)
                    {
                        //reaches here only when all 3 constructors are found - no diagnostic needed 
                        return;
                    }

                } //end of for loop

                if (!defaultConstructorFound)
                {
                    //Missing default constructor - report diagnostic
                    Diagnostic diagnostic;
                    diagnostic = Diagnostic.Create(MissingConstructorRule, symbol.Locations[0], symbol.Name, GetParameterLessConstructorMessage(symbol));
                    context.ReportDiagnostic(diagnostic);
                }

                if (!secondConstructorFound)
                {
                    //Missing constructor with string parameter - report diagnostic
                    Diagnostic diagnostic;
                    diagnostic = Diagnostic.Create(MissingConstructorRule, symbol.Locations[0], symbol.Name, GetConstructorWithStringTypeParameter(symbol));
                    context.ReportDiagnostic(diagnostic);
                }

                if (!thirdConstructorFound)
                {
                    //Missing constructor with string and exception type parameter - report diagnostic
                    Diagnostic diagnostic;
                    diagnostic = Diagnostic.Create(MissingConstructorRule, symbol.Locations[0], symbol.Name, GetConstructorWithStringAndExceptionTypeParameter(symbol));
                    context.ReportDiagnostic(diagnostic);
                }

            }
        }
    }
}