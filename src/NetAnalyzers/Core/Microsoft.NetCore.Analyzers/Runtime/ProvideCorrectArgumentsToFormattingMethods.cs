// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2241: Provide correct arguments to formatting methods
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ProvideCorrectArgumentsToFormattingMethodsAnalyzer : DiagnosticAnalyzer
    {
        internal const string DefaultRuleId = "CA2241";
        internal const string NotEnoughArgsRuleId = "CA2250";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageArgs = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsMessageArgs), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        // e.g. Console.WriteLine("{0} {1}", 42);
        internal static DiagnosticDescriptor NotEnoughArgsRule = DiagnosticDescriptorHelper.Create(
            NotEnoughArgsRuleId,
            s_localizableTitle,
            s_localizableMessageArgs,
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarningCandidate,
            description: s_localizableDescription,
            isPortedFxCopRule: true,
            isDataflowRule: false);

        // e.g. Console.WriteLine("{1}", 42);
        internal static DiagnosticDescriptor NotEnoughArgsMissingFormatIndexRule = DiagnosticDescriptorHelper.Create(
            NotEnoughArgsRuleId,
            s_localizableTitle,
            s_localizableMessageArgs,
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarningCandidate,
            description: s_localizableDescription,
            isPortedFxCopRule: true,
            isDataflowRule: false);

        // e.g. Console.WriteLine("{0}", 1, 2)
        internal static DiagnosticDescriptor TooManyArgsRule = DiagnosticDescriptorHelper.Create(
            DefaultRuleId,
            s_localizableTitle,
            s_localizableMessageArgs,
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: s_localizableDescription,
            isPortedFxCopRule: true,
            isDataflowRule: false);

        // e.g. Console.WriteLine("{1}", 1, 2, 3)
        internal static DiagnosticDescriptor TooManyArgsMissingFormatIndexRule = DiagnosticDescriptorHelper.Create(
            DefaultRuleId,
            s_localizableTitle,
            s_localizableMessageArgs,
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: s_localizableDescription,
            isPortedFxCopRule: true,
            isDataflowRule: false);

        // e.g. Console.WriteLine("{1}", 1, 2)
        internal static DiagnosticDescriptor EnoughArgsMissingFormatIndexRule = DiagnosticDescriptorHelper.Create(
            DefaultRuleId,
            s_localizableTitle,
            s_localizableMessageArgs,
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: s_localizableDescription,
            isPortedFxCopRule: true,
            isDataflowRule: false);

        //// e.g. Console.WriteLine("{0} {1}") -> resolves to the overload expecting a string, not a format string
        //internal static DiagnosticDescriptor FormatItemInNonStringFormatMethodRule = DiagnosticDescriptorHelper.Create(
        //    RuleId,
        //    s_localizableTitle,
        //    s_localizableMessageArgs,
        //    DiagnosticCategory.Usage,
        //    RuleLevel.IdeSuggestion,
        //    description: s_localizableDescription,
        //    isPortedFxCopRule: true,
        //    isDataflowRule: false);

        //// e.g. Console.WriteLine("{1}")
        //internal static DiagnosticDescriptor FormatItemInNonStringFormatMethodMissingFormatItemRule = DiagnosticDescriptorHelper.Create(
        //    RuleId,
        //    s_localizableTitle,
        //    s_localizableMessageArgs,
        //    DiagnosticCategory.Usage,
        //    RuleLevel.IdeSuggestion,
        //    description: s_localizableDescription,
        //    isPortedFxCopRule: true,
        //    isDataflowRule: false);

        /// <summary>
        /// This regex is used to remove escaped brackets from the format string before looking for valid {} pairs.
        /// </summary>
        private static readonly Regex s_removeEscapedBracketsRegex = new("{{", RegexOptions.Compiled);

        /// <summary>
        /// This regex is used to extract the text between the brackets and save the contents in a MatchCollection.
        /// </summary>
        private static readonly Regex s_extractPlaceholdersRegex = new("{(.*?)}", RegexOptions.Compiled);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                NotEnoughArgsRule,
                NotEnoughArgsMissingFormatIndexRule,
                EnoughArgsMissingFormatIndexRule,
                TooManyArgsRule,
                TooManyArgsMissingFormatIndexRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(context =>
            {
                var formatInfo = new StringFormatInfo(context.Compilation);

                context.RegisterOperationAction(context =>
                {
                    var invocation = (IInvocationOperation)context.Operation;

                    StringFormatInfo.Info? info = formatInfo.TryGet(invocation.TargetMethod, context);
                    if (info == null || invocation.Arguments.Length <= info.FormatStringIndex)
                    {
                        // not a target method
                        return;
                    }

                    IArgumentOperation formatStringArgument = invocation.Arguments[info.FormatStringIndex];
                    if (!Equals(formatStringArgument?.Value?.Type, formatInfo.String) ||
                        !(formatStringArgument?.Value?.ConstantValue.Value is string))
                    {
                        // wrong argument
                        return;
                    }

                    // __arglist is not supported here (see https://github.com/dotnet/roslyn/issues/7346)
                    // but we want to skip it only in C# since VB doesn't support __arglist
                    if (info.ExpectedStringFormatArgumentCount >= 0 &&
                        invocation.TargetMethod.IsVararg &&
                        invocation.Language == LanguageNames.CSharp)
                    {
                        return;
                    }

                    var stringFormat = (string)formatStringArgument.Value.ConstantValue.Value;
                    var stringFormatIndexes = GetStringFormatItemIndexes(stringFormat);

                    // Target method resolves to an overload with a non variable (e.g. params) argument count
                    // so we can easily analyze the arguments.
                    if (info.ExpectedStringFormatArgumentCount >= 0)
                    {
                        ReportOnArgumentsMismatch(stringFormat, info.ExpectedStringFormatArgumentCount, context, invocation);
                        return;
                    }

                    // Target method resolves to an overload with a variable argument count so we need
                    // to try to understand the number of arguments passed.

                    // ensure argument is an array
                    IArgumentOperation paramsArgument = invocation.Arguments[info.FormatStringIndex + 1];
                    if (paramsArgument.ArgumentKind is not ArgumentKind.ParamArray and not ArgumentKind.Explicit)
                    {
                        // wrong format
                        return;
                    }

                    if (paramsArgument.Value is not IArrayCreationOperation arrayCreation ||
                        arrayCreation.GetElementType() is not ITypeSymbol elementType ||
                        !Equals(elementType, formatInfo.Object) ||
                        arrayCreation.DimensionSizes.Length != 1)
                    {
                        // wrong format
                        return;
                    }

                    // compiler generating object array for params case
                    IArrayInitializerOperation intializer = arrayCreation.Initializer;
                    if (intializer == null)
                    {
                        // unsupported format
                        return;
                    }

                    // REVIEW: "ElementValues" is a bit confusing where I need to double dot those to get number of elements
                    int actualArgumentCount = intializer.ElementValues.Length;
                    ReportOnArgumentsMismatch(stringFormat, actualArgumentCount, context, invocation);
                }, OperationKind.Invocation);
            });
        }

        private static HashSet<int> GetStringFormatItemIndexes(string stringFormat)
        {
            // removing escaped left brackets and replacing with space characters so they won't
            // impede the extraction of placeholders, yet the locations of the placeholders are
            // the same as in the original string.
            var formatStringWithEscapedBracketsChangedToSpaces = s_removeEscapedBracketsRegex.Replace(stringFormat, "  ");

            var matches = s_extractPlaceholdersRegex.Matches(formatStringWithEscapedBracketsChangedToSpaces);
            var indexes = new HashSet<int>();

            foreach (Match match in matches)
            {
                var formatItemIndex = ExtractFormatItemIndex(match.Groups[1].Value);
                if (formatItemIndex.HasValue)
                {
                    indexes.Add(formatItemIndex.Value);
                }
            }

            return indexes;
        }

        private static int? ExtractFormatItemIndex(string textInsideBrackets)
        {
            var formatItemText = textInsideBrackets.IndexOf(",", StringComparison.OrdinalIgnoreCase) > 0
                ? textInsideBrackets.Split(',')[0]
                : textInsideBrackets.Split(':')[0];

            // placeholders cannot begin with whitespace
            if (formatItemText.Length > 0 && char.IsWhiteSpace(formatItemText, 0))
            {
                return null;
            }

            if (!int.TryParse(formatItemText, out var formatItemIndex) ||
                formatItemIndex < 0)
            {
                return null;
            }

            return formatItemIndex;
        }

        private static void ReportOnArgumentsMismatch(string stringFormat, int actualArgumentCount,
            OperationAnalysisContext context, IInvocationOperation invocation)
        {
            var stringFormatIndexes = GetStringFormatItemIndexes(stringFormat);

            var expectedArgumentCount = stringFormatIndexes.Count > 0
                ? stringFormatIndexes.Max() + 1
                : 0;

            if (actualArgumentCount > expectedArgumentCount)
            {
                if (HasAnyMissingFormatIndex(stringFormatIndexes, expectedArgumentCount))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(TooManyArgsMissingFormatIndexRule));
                }
                else
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(TooManyArgsRule));
                }

                return;
            }
            else if (actualArgumentCount < expectedArgumentCount)
            {
                if (HasAnyMissingFormatIndex(stringFormatIndexes, expectedArgumentCount))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(NotEnoughArgsMissingFormatIndexRule));
                }
                else
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(NotEnoughArgsRule));
                }

                return;
            }
            else
            {
                if (HasAnyMissingFormatIndex(stringFormatIndexes, expectedArgumentCount))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(EnoughArgsMissingFormatIndexRule));
                }

                return;
            }

            static bool HasAnyMissingFormatIndex(HashSet<int> stringFormatIndexes, int expectedArgumentCount)
                => stringFormatIndexes.Count > 0 &&
                    Enumerable.Range(0, expectedArgumentCount - 1).Except(stringFormatIndexes).Any();
        }

        private class StringFormatInfo
        {
            private const string Format = "format";

            private readonly ImmutableDictionary<IMethodSymbol, Info> _map;

            public StringFormatInfo(Compilation compilation)
            {
                ImmutableDictionary<IMethodSymbol, Info>.Builder builder = ImmutableDictionary.CreateBuilder<IMethodSymbol, Info>();

                INamedTypeSymbol? console = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemConsole);
                AddStringFormatMap(builder, console, "Write");
                AddStringFormatMap(builder, console, "WriteLine");

                INamedTypeSymbol @string = compilation.GetSpecialType(SpecialType.System_String);
                AddStringFormatMap(builder, @string, "Format");

                _map = builder.ToImmutable();

                String = @string;
                Object = compilation.GetSpecialType(SpecialType.System_Object);
            }

            public INamedTypeSymbol String { get; }
            public INamedTypeSymbol Object { get; }

            public Info? TryGet(IMethodSymbol method, OperationAnalysisContext context)
            {
                if (_map.TryGetValue(method, out Info? info))
                {
                    return info;
                }

                foreach (var descriptor in new[] { NotEnoughArgsRule, TooManyArgsRule })
                {
                    // Check if this the underlying method is user configured string formatting method.
                    var additionalStringFormatMethodsOption = context.Options.GetAdditionalStringFormattingMethodsOption(descriptor,
                        context.Operation.Syntax.SyntaxTree, context.Compilation, context.CancellationToken);
                    if (additionalStringFormatMethodsOption.Contains(method.OriginalDefinition) &&
                        TryGetFormatInfo(method, out info))
                    {
                        return info;
                    }

                    // Check if the user configured automatic determination of formatting methods.
                    // If so, check if the method called has a 'string format' parameter followed by an params array.
                    var determineAdditionalStringFormattingMethodsAutomatically = context.Options.GetBoolOptionValue(
                        EditorConfigOptionNames.TryDetermineAdditionalStringFormattingMethodsAutomatically, descriptor,
                        context.Operation.Syntax.SyntaxTree, context.Compilation, defaultValue: false, context.CancellationToken);
                    if (determineAdditionalStringFormattingMethodsAutomatically &&
                        TryGetFormatInfo(method, out info) &&
                        info.ExpectedStringFormatArgumentCount == -1)
                    {
                        return info;
                    }
                }

                return null;
            }

            private static void AddStringFormatMap(ImmutableDictionary<IMethodSymbol, Info>.Builder builder, INamedTypeSymbol? type, string methodName)
            {
                if (type == null)
                {
                    return;
                }

                foreach (IMethodSymbol method in type.GetMembers(methodName).OfType<IMethodSymbol>())
                {
                    if (TryGetFormatInfo(method, out var formatInfo))
                    {
                        builder.Add(method, formatInfo);
                    }
                }
            }

            private static bool TryGetFormatInfo(IMethodSymbol method, [NotNullWhen(returnValue: true)] out Info? formatInfo)
            {
                formatInfo = default;

                int formatIndex = FindParameterIndexOfName(method.Parameters, Format);
                if (formatIndex < 0 || formatIndex == method.Parameters.Length - 1)
                {
                    // no valid format string
                    return false;
                }

                if (method.Parameters[formatIndex].Type.SpecialType != SpecialType.System_String)
                {
                    // no valid format string
                    return false;
                }

                int expectedArguments = GetExpectedNumberOfArguments(method.Parameters, formatIndex);
                formatInfo = new Info(formatIndex, expectedArguments);
                return true;
            }

            private static int GetExpectedNumberOfArguments(ImmutableArray<IParameterSymbol> parameters, int formatIndex)
            {
                // check params
                IParameterSymbol nextParameter = parameters[formatIndex + 1];
                if (nextParameter.IsParams)
                {
                    return -1;
                }

                return parameters.Length - formatIndex - 1;
            }

            private static int FindParameterIndexOfName(ImmutableArray<IParameterSymbol> parameters, string name)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (string.Equals(parameters[i].Name, name, StringComparison.Ordinal))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public class Info
            {
                public Info(int formatIndex, int expectedArguments)
                {
                    FormatStringIndex = formatIndex;
                    ExpectedStringFormatArgumentCount = expectedArguments;
                }

                public int FormatStringIndex { get; }
                public int ExpectedStringFormatArgumentCount { get; }
            }
        }
    }
}