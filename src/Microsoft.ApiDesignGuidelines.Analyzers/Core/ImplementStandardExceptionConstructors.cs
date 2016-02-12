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
    /// For CSharp, example when type name is GoodException 
    ///     public GoodException()
    ///     public GoodException(string)
    ///     public GoodException(string, Exception)
    /// For Basic, example
    ///     Sub New()
    ///     Sub New(message As String)
    ///     Sub New(message As String, innerException As Exception)
    /// Redefined - because, in the original FxCop rule, it was also checking for a 4th constructor, as listed below, which after discussion with Sri was decided as covered by other analyzers
    ///     protected or private NewException(SerializationInfo, StreamingContext)
    /// </summary>

    public abstract class ImplementStandardExceptionConstructorsAnalyzer : DiagnosticAnalyzer
    {
        internal enum MissingCtorSignature { CtorWithNoParameter, CtorWithStringParameter, CtorWithStringAndExceptionParameters }

        internal const string RuleId = "CA1032";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementStandardExceptionConstructorsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMissingConstructor = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementStandardExceptionConstructorsMessageMissingConstructor), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementStandardExceptionConstructorsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor MissingConstructorRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMissingConstructor,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182151.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MissingConstructorRule);
        private INamedTypeSymbol _exceptionType;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(AnalyzeCompilationSymbol);
        }

        //abstract methods, which the language specific analyzers implements - these will return the required constructor method signatures for CSharp/Basic
        protected abstract string GetConstructorSignatureNoParameter(ISymbol symbol);
        protected abstract string GetConstructorSignatureStringTypeParameter(ISymbol symbol);
        protected abstract string GetConstructorSignatureStringAndExceptionTypeParameter(ISymbol symbol);

        private void AnalyzeCompilationSymbol(CompilationStartAnalysisContext context)
        {
            _exceptionType = context.Compilation.GetTypeByMetadataName("System.Exception");
            // Analyze named types 
            context.RegisterSymbolAction(symbolContext =>
            {
                AnalyzeSymbol(symbolContext);
            }, SymbolKind.NamedType);
        }
        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = context.Symbol as INamedTypeSymbol;

            //Check if type derives from Exception type
            if (namedTypeSymbol.BaseType == _exceptionType)
            {
                //Get the list of constructors
                ImmutableArray<IMethodSymbol> constructors = namedTypeSymbol.Constructors;

                //Set flags for the 3 different constructos, that is being searched for
                var defaultConstructorFound = false; //flag for default constructor
                var secondConstructorFound = false; //flag for constructor with string type parameter
                var thirdConstructorFound = false; //flag for constructor with string and exception type parameter

                foreach (IMethodSymbol ctor in constructors)
                {
                    ImmutableArray<IParameterSymbol> parameters = ctor.GetParameters();

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
                            parameters[1].Type == _exceptionType)
                    {
                        thirdConstructorFound = true;
                    }

                    if (defaultConstructorFound && secondConstructorFound && thirdConstructorFound)
                    {
                        //reaches here only when all 3 constructors are found - no diagnostic needed 
                        return;
                    }
                } //end of for loop

                if (!defaultConstructorFound) //missing default constructor
                {
                    BuildDiagnostic(context, namedTypeSymbol, MissingCtorSignature.CtorWithNoParameter, GetConstructorSignatureNoParameter(namedTypeSymbol));
                }

                if (!secondConstructorFound) //missing constructor with string parameter
                {
                    BuildDiagnostic(context, namedTypeSymbol, MissingCtorSignature.CtorWithStringParameter, GetConstructorSignatureStringTypeParameter(namedTypeSymbol));
                }

                if (!thirdConstructorFound) //missing constructor with string and exception type parameter - report diagnostic
                {
                    BuildDiagnostic(context, namedTypeSymbol, MissingCtorSignature.CtorWithStringAndExceptionParameters, GetConstructorSignatureStringAndExceptionTypeParameter(namedTypeSymbol));
                }
            }
        }

        private void BuildDiagnostic(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, MissingCtorSignature missingCtorSignature, string constructorSignature)
        {
            //store MissingCtorSignature enum type into dictionary, to set diagnostic property. This is needed because Diagnostic is immutable
            ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder.Add("Signature", missingCtorSignature.ToString());

            //create dignostic and store signature into diagnostic property for fixer
            Diagnostic diagnostic = namedTypeSymbol.Locations.CreateDiagnostic(MissingConstructorRule, builder.ToImmutableDictionary(), namedTypeSymbol.Name, constructorSignature);

            //report diagnostic
            context.ReportDiagnostic(diagnostic);
        }
    }
}