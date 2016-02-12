// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2241: Provide correct arguments to formatting methods
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ProvideCorrectArgumentsToFormattingMethodsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2241";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: @"https://msdn.microsoft.com/en-us/library/ms182361.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                var formatInfo = new StringFormatInfo(compilationContext.Compilation);

                compilationContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationExpression)operationContext.Operation;

                    StringFormatInfo.Info info = formatInfo.TryGet(invocation.TargetMethod);
                    if (info == null || invocation.ArgumentsInParameterOrder.Length <= info.FormatStringIndex)
                    {
                        // not a target method
                        return;
                    }

                    IArgument formatStringArgument = invocation.ArgumentsInParameterOrder[info.FormatStringIndex];
                    if (!object.Equals(formatStringArgument?.Value?.ResultType, formatInfo.String) ||
                        !(formatStringArgument?.Value?.ConstantValue.Value is string))
                    {
                        // wrong argument
                        return;
                    }

                    var stringFormat = (string)formatStringArgument.Value.ConstantValue.Value;
                    int expectedStringFormatArgumentCount = GetFormattingArguments(stringFormat);

                    // explict parameter case
                    if (info.ExpectedStringFormatArgumentCount >= 0)
                    {
                        // TODO: due to a bug - https://github.com/dotnet/roslyn/issues/7346
                        //       vararg case is disabled.
                        //       we might check this only for C# since __arglist is not supported in VB
                        //
                        //       we need to implement proper support for __arglist once the bug is fixed.
                        if (invocation.TargetMethod.IsVararg)
                        {
                            // can't deal with this for now.
                            return;
                        }

                        if (info.ExpectedStringFormatArgumentCount != expectedStringFormatArgumentCount)
                        {
                            operationContext.ReportDiagnostic(operationContext.Operation.Syntax.CreateDiagnostic(Rule));
                        }

                        return;
                    }

                    // params case
                    IArgument paramsArgument = invocation.ArgumentsInParameterOrder[info.FormatStringIndex + 1];
                    if (paramsArgument.ArgumentKind != ArgumentKind.ParamArray)
                    {
                        // wrong format
                        return;
                    }

                    var arrayCreation = paramsArgument.Value as IArrayCreationExpression;
                    if (arrayCreation == null ||
                        !object.Equals(arrayCreation.ElementType, formatInfo.Object) ||
                        arrayCreation.DimensionSizes.Length != 1)
                    {
                        // wrong format
                        return;
                    }

                    // compiler generating object array for params case
                    IArrayInitializer intializer = arrayCreation.Initializer;
                    if (intializer == null)
                    {
                        // unsupported format
                        return;
                    }

                    // REVIEW: "ElementValues" is a bit confusing where I need to double dot those to get number of elements
                    int actualArgumentCount = intializer.ElementValues.Length;
                    if (actualArgumentCount != expectedStringFormatArgumentCount)
                    {
                        operationContext.ReportDiagnostic(operationContext.Operation.Syntax.CreateDiagnostic(Rule));
                    }
                }, OperationKind.InvocationExpression);
            });
        }

        private int GetFormattingArguments(string format)
        {
            // code is from mscorlib
            // https://github.com/dotnet/coreclr/blob/bc146608854d1db9cdbcc0b08029a87754e12b49/src/mscorlib/src/System/Text/StringBuilder.cs#L1312

            // return count of this format - {index[,alignment][:formatString]}
            var count = 0;

            var pos = 0;
            int len = format.Length;
            var ch = '\x0';

            // main loop
            while (true)
            {
                int p = pos;
                int i = pos;

                // loop to find starting "{"
                while (pos < len)
                {
                    ch = format[pos];

                    pos++;
                    if (ch == '}')
                    {
                        if (pos < len && format[pos] == '}') // Treat as escape character for }}
                            pos++;
                        else
                            return -1;
                    }

                    if (ch == '{')
                    {
                        if (pos < len && format[pos] == '{') // Treat as escape character for {{
                            pos++;
                        else
                        {
                            pos--;
                            break;
                        }
                    }
                }

                // finished with "{"
                if (pos == len)
                {
                    break;
                }

                pos++;

                if (pos == len || (ch = format[pos]) < '0' || ch > '9')
                {
                    // finished with "{x"
                    return -1;
                }

                // searching for index
                var index = 0;
                do
                {
                    index = index * 10 + ch - '0';

                    pos++;
                    if (pos == len)
                    {
                        // wrong index format
                        return -1;
                    }

                    ch = format[pos];
                } while (ch >= '0' && ch <= '9' && index < 1000000);

                // eat up whitespace
                while (pos < len && (ch = format[pos]) == ' ')
                {
                    pos++;
                }

                // searching for alignment
                var width = 0;
                if (ch == ',')
                {
                    pos++;

                    // eat up whitespace
                    while (pos < len && format[pos] == ' ')
                    {
                        pos++;
                    }

                    if (pos == len)
                    {
                        // wrong format, reached end without "}"
                        return -1;
                    }

                    ch = format[pos];
                    if (ch == '-')
                    {
                        pos++;

                        if (pos == len)
                        {
                            // wrong format. reached end without "}"
                            return -1;
                        }

                        ch = format[pos];
                    }

                    if (ch < '0' || ch > '9')
                    {
                        // wrong format after "-"
                        return -1;
                    }

                    do
                    {
                        width = width * 10 + ch - '0';
                        pos++;

                        if (pos == len)
                        {
                            // wrong width format
                            return -1;
                        }

                        ch = format[pos];
                    } while (ch >= '0' && ch <= '9' && width < 1000000);
                }

                // eat up whitespace
                while (pos < len && (ch = format[pos]) == ' ')
                {
                    pos++;
                }

                // searching for embeded format string
                if (ch == ':')
                {
                    pos++;
                    p = pos;
                    i = pos;

                    while (true)
                    {
                        if (pos == len)
                        {
                            // reached end without "}"
                            return -1;
                        }

                        ch = format[pos];
                        pos++;

                        if (ch == '{')
                        {
                            if (pos < len && format[pos] == '{')  // Treat as escape character for {{
                                pos++;
                            else
                                return -1;
                        }
                        else if (ch == '}')
                        {
                            if (pos < len && format[pos] == '}')  // Treat as escape character for }}
                                pos++;
                            else
                            {
                                pos--;
                                break;
                            }
                        }
                    }
                }

                if (ch != '}')
                {
                    // "}" is expected
                    return -1;
                }

                pos++;

                count++;
            } // end of main loop

            return count;
        }

        private class StringFormatInfo
        {
            private const string Format = "format";

            private readonly ImmutableDictionary<IMethodSymbol, Info> _map;

            public StringFormatInfo(Compilation compilation)
            {
                ImmutableDictionary<IMethodSymbol, Info>.Builder builder = ImmutableDictionary.CreateBuilder<IMethodSymbol, Info>();

                INamedTypeSymbol console = WellKnownTypes.Console(compilation);
                AddStringFormatMap(builder, console, "Write");
                AddStringFormatMap(builder, console, "WriteLine");

                INamedTypeSymbol @string = WellKnownTypes.String(compilation);
                AddStringFormatMap(builder, @string, "Format");

                _map = builder.ToImmutable();

                String = @string;
                Object = WellKnownTypes.Object(compilation);
            }

            public INamedTypeSymbol String { get; }
            public INamedTypeSymbol Object { get; }

            public Info TryGet(IMethodSymbol method)
            {
                Info info;
                if (_map.TryGetValue(method, out info))
                {
                    return info;
                }

                return null;
            }

            private void AddStringFormatMap(ImmutableDictionary<IMethodSymbol, Info>.Builder builder, INamedTypeSymbol type, string methodName)
            {
                if (type == null)
                {
                    return;
                }

                foreach (IMethodSymbol method in type.GetMembers(methodName).OfType<IMethodSymbol>())
                {
                    int formatIndex = FindParameterIndexOfName(method.Parameters, Format);
                    if (formatIndex < 0 || formatIndex == method.Parameters.Length - 1)
                    {
                        // no valid format string
                        continue;
                    }

                    int expectedArguments = GetExpectedNumberOfArguments(method.Parameters, formatIndex);
                    builder.Add(method, new Info(formatIndex, expectedArguments));
                }
            }

            private int GetExpectedNumberOfArguments(ImmutableArray<IParameterSymbol> parameters, int formatIndex)
            {
                // check params
                IParameterSymbol nextParameter = parameters[formatIndex + 1];
                if (nextParameter.IsParams)
                {
                    return -1;
                }

                return parameters.Length - formatIndex - 1;
            }

            private int FindParameterIndexOfName(ImmutableArray<IParameterSymbol> parameters, string name)
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