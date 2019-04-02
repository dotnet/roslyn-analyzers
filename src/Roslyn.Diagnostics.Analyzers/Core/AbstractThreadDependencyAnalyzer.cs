// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    public abstract class AbstractThreadDependencyAnalyzer : DiagnosticAnalyzer
    {
        protected const string AsyncEntryAttributeFullName = "Roslyn.Utilities.AsyncEntryAttribute";
        protected const string AsyncEntryAttributeName = "AsyncEntryAttribute";
        protected const string NoMainThreadDependencyAttributeFullName = "Roslyn.Utilities.NoMainThreadDependencyAttribute";
        protected const string NoMainThreadDependencyAttributeName = "NoMainThreadDependencyAttribute";

        private protected AbstractThreadDependencyAnalyzer()
        {
        }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var noMainThreadDependencyAttribute = compilation.GetTypeByMetadataName(NoMainThreadDependencyAttributeFullName);

                // Bail if NoMainThreadDependencyAttribute is not defined
                if (noMainThreadDependencyAttribute is null)
                {
                    return;
                }

                HandleCompilationStart(compilationStartContext, noMainThreadDependencyAttribute);
            });
        }

        protected abstract void HandleCompilationStart(CompilationStartAnalysisContext context, INamedTypeSymbol noMainThreadDependencyAttribute);

        protected ThreadDependencyInfo GetThreadDependencyInfo(ISymbol symbol)
            => GetThreadDependencyInfo(symbol.GetAttributes(), in GetDefaultThreadDependencyInfo(symbol));

        protected ThreadDependencyInfo GetThreadDependencyInfoForReturn(IMethodSymbol symbol)
            => GetThreadDependencyInfo(symbol.GetReturnTypeAttributes(), in GetDefaultThreadDependencyInfo(symbol.ReturnType));

        private ref readonly ThreadDependencyInfo GetDefaultThreadDependencyInfo(ISymbol symbol)
        {
            switch (symbol)
            {
                case IEventSymbol eventSymbol:
                    return ref GetDefaultThreadDependencyInfo(eventSymbol.Type);

                case IParameterSymbol parameterSymbol:
                    return ref GetDefaultThreadDependencyInfo(parameterSymbol.Type);

                case IPropertySymbol propertySymbol:
                    return ref GetDefaultThreadDependencyInfo(propertySymbol.Type);

                case IMethodSymbol methodSymbol:
                    return ref GetDefaultThreadDependencyInfo(methodSymbol.ReturnType);

                case IFieldSymbol fieldSymbol:
                    return ref GetDefaultThreadDependencyInfo(fieldSymbol.Type);

                case ITypeSymbol typeSymbol:
                    if (typeSymbol.IsAwaitable())
                        return ref ThreadDependencyInfo.DefaultAsynchronous;
                    else
                        return ref ThreadDependencyInfo.DefaultSynchronous;

                default:
                    return ref ThreadDependencyInfo.DefaultSynchronous;
            }
        }

        private static ThreadDependencyInfo GetThreadDependencyInfo(ImmutableArray<AttributeData> attributes, in ThreadDependencyInfo defaultValue)
        {
            var asyncEntryOverride = false;
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass.Name == AsyncEntryAttributeName)
                {
                    asyncEntryOverride = true;
                    continue;
                }

                if (attribute.AttributeClass.Name == NoMainThreadDependencyAttributeName)
                {
                    bool mayDirectlyRequireMainThread = false;
                    bool alwaysCompleted = false;
                    bool perInstance = false;
                    bool capturesContext = false;
                    bool verified = true;
                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        switch (namedArgument.Key)
                        {
                            case "AlwaysCompleted":
                                alwaysCompleted = (namedArgument.Value.Value as bool?) ?? alwaysCompleted;
                                break;
                            case "CapturesContext":
                                capturesContext = (namedArgument.Value.Value as bool?) ?? capturesContext;
                                break;
                            case "Verified":
                                verified = (namedArgument.Value.Value as bool?) ?? verified;
                                break;
                            default:
                                break;
                        }
                    }

                    return new ThreadDependencyInfo(
                        isExplicit: true,
                        mayDirectlyRequireMainThread: mayDirectlyRequireMainThread,
                        alwaysCompleted: alwaysCompleted,
                        perInstance: perInstance,
                        capturesContext: capturesContext,
                        verified: verified);
                }
            }

            if (asyncEntryOverride)
            {
                return ThreadDependencyInfo.DefaultAsynchronous;
            }

            return defaultValue;
        }

        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "This type is never used for comparison.")]
        protected readonly struct ThreadDependencyInfo
        {
            public static readonly ThreadDependencyInfo DefaultAsynchronous = new ThreadDependencyInfo(
                isExplicit: false,
                mayDirectlyRequireMainThread: true,
                alwaysCompleted: false,
                perInstance: false,
                capturesContext: true,
                verified: false);

            public static readonly ThreadDependencyInfo DefaultSynchronous = new ThreadDependencyInfo(
                isExplicit: false,
                mayDirectlyRequireMainThread: false,
                alwaysCompleted: true,
                perInstance: false,
                capturesContext: false,
                verified: false);

            public ThreadDependencyInfo(
                bool isExplicit,
                bool mayDirectlyRequireMainThread,
                bool alwaysCompleted,
                bool perInstance,
                bool capturesContext,
                bool verified)
            {
                IsExplicit = isExplicit;
                MayDirectlyRequireMainThread = mayDirectlyRequireMainThread;
                AlwaysCompleted = alwaysCompleted;
                PerInstance = perInstance;
                CapturesContext = capturesContext;
                Verified = verified;
            }

            public bool IsExplicit { get; }
            public bool MayDirectlyRequireMainThread { get; }
            public bool AlwaysCompleted { get; }
            public bool PerInstance { get; }
            public bool CapturesContext { get; }
            public bool Verified { get; }

            public bool MayHaveMainThreadDependency => MayDirectlyRequireMainThread || CapturesContext;
        }
    }
}
