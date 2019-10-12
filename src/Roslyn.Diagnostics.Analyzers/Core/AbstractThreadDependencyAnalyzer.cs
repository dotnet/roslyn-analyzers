// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    public abstract class AbstractThreadDependencyAnalyzer : DiagnosticAnalyzer
    {
        protected const string AsyncEntryAttributeFullName = "Roslyn.Utilities.AsyncEntryAttribute";
        protected const string AsyncEntryAttributeName = "AsyncEntryAttribute";
        protected const string ThreadDependencyAttributeFullName = "Roslyn.Utilities.ThreadDependencyAttribute";
        protected const string ThreadDependencyAttributeName = "ThreadDependencyAttribute";

        private protected AbstractThreadDependencyAnalyzer()
        {
        }

        protected internal enum ContextDependency
        {
            Default,
            None,
            Context,
            Any,
        }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilation);
                var threadDependencyAttribute = wellKnownTypeProvider.GetOrCreateTypeByMetadataName(ThreadDependencyAttributeFullName);

                // Bail if ThreadDependencyAttribute is not defined
                if (threadDependencyAttribute is null)
                {
                    return;
                }

                HandleCompilationStart(compilationStartContext, wellKnownTypeProvider, threadDependencyAttribute);
            });
        }

        protected abstract void HandleCompilationStart(CompilationStartAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, INamedTypeSymbol threadDependencyAttribute);

        protected internal static ThreadDependencyInfo GetThreadDependencyInfo(ISymbol symbol)
            => GetThreadDependencyInfo(symbol.GetAttributes(), in GetDefaultThreadDependencyInfo(symbol));

        protected static Location TryGetThreadDependencyInfoLocation(ISymbol symbol, CancellationToken cancellationToken)
            => TryGetThreadDependencyInfoLocation(symbol.GetAttributes(), cancellationToken);

        protected static Location TryGetThreadDependencyInfoLocationForReturn(IMethodSymbol symbol, CancellationToken cancellationToken)
            => TryGetThreadDependencyInfoLocation(symbol.GetReturnTypeAttributes(), cancellationToken);

        protected internal static ThreadDependencyInfo GetThreadDependencyInfoForReturn(WellKnownTypeProvider wellKnownTypeProvider, IMethodSymbol symbol)
        {
            if (symbol.Name == nameof(Task.FromResult)
                || symbol.Name == nameof(Task.FromCanceled)
                || symbol.Name == nameof(Task.FromException))
            {
                if (symbol.ContainingType.Equals(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksTask)))
                {
                    return new ThreadDependencyInfo(isExplicit: false, mayDirectlyRequireMainThread: false, alwaysCompleted: true, perInstance: false, capturesContext: false, verified: true);
                }
            }

            return GetThreadDependencyInfo(symbol.GetReturnTypeAttributes(), in GetDefaultThreadDependencyInfo(symbol.ReturnType));
        }

        private static ref readonly ThreadDependencyInfo GetDefaultThreadDependencyInfo(ISymbol symbol)
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

                if (attribute.AttributeClass.Name == ThreadDependencyAttributeName)
                {
                    bool mayDirectlyRequireMainThread = false;
                    bool alwaysCompleted = false;
                    bool perInstance = false;
                    bool capturesContext = false;
                    bool verified = true;
                    foreach (var positionalArgument in attribute.ConstructorArguments)
                    {
                        var contextDependency = (ContextDependency)(positionalArgument.Value as int? ?? 0);
                        switch (contextDependency)
                        {
                            case ContextDependency.Default:
                            default:
                                capturesContext = defaultValue.CapturesContext;
                                break;

                            case ContextDependency.None:
                                capturesContext = false;
                                break;

                            case ContextDependency.Context:
                            case ContextDependency.Any:
                                capturesContext = true;
                                break;
                        }
                    }

                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        switch (namedArgument.Key)
                        {
                            case "AlwaysCompleted":
                                alwaysCompleted = (namedArgument.Value.Value as bool?) ?? alwaysCompleted;
                                break;
                            case "PerInstance":
                                perInstance = (namedArgument.Value.Value as bool?) ?? perInstance;
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

        private static Location TryGetThreadDependencyInfoLocation(ImmutableArray<AttributeData> attributes, CancellationToken cancellationToken)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass.Name == ThreadDependencyAttributeName)
                {
                    return attribute.ApplicationSyntaxReference.GetSyntax(cancellationToken).GetLocation();
                }
            }

            return null;
        }

        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "This type is never used for comparison.")]
        protected internal readonly struct ThreadDependencyInfo
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

            /// <summary>
            /// Gets a value indicating whether the allowed thread dependencies are explicitly stated.
            /// </summary>
            public bool IsExplicit { get; }

            /// <summary>
            /// Gets a value indicating whether this method (or a method it calls) is allowed to explicitly switch to
            /// the main thread.
            /// </summary>
            public bool MayDirectlyRequireMainThread { get; }

            /// <summary>
            /// Gets a value indicating whether the asynchronous result is always completed.
            /// </summary>
            public bool AlwaysCompleted { get; }

            /// <summary>
            /// Gets a value indicating whether the declared threading dependencies apply per instance.
            /// </summary>
            public bool PerInstance { get; }

            /// <summary>
            /// Gets a value indicating whether the method is allowed to capture the current
            /// <see cref="SynchronizationContext"/> as part of a continuation.
            /// </summary>
            public bool CapturesContext { get; }

            /// <summary>
            /// Gets a value indicating whether the dependency claims are verified.
            /// </summary>
            public bool Verified { get; }

            /// <summary>
            /// Gets a value indicating whether the method is allowed to directly or indirectly rely on the main thread
            /// for completion.
            /// </summary>
            public bool MayHaveMainThreadDependency => MayDirectlyRequireMainThread || CapturesContext;

            internal ThreadDependencyInfo WithPerInstance(bool value)
            {
                return new ThreadDependencyInfo(
                    isExplicit: IsExplicit,
                    mayDirectlyRequireMainThread: MayDirectlyRequireMainThread,
                    alwaysCompleted: AlwaysCompleted,
                    perInstance: value,
                    capturesContext: CapturesContext,
                    verified: Verified);
            }

            internal ThreadDependencyInfo WithCapturesContext(bool value)
            {
                return new ThreadDependencyInfo(
                    isExplicit: IsExplicit,
                    mayDirectlyRequireMainThread: MayDirectlyRequireMainThread,
                    alwaysCompleted: AlwaysCompleted,
                    perInstance: PerInstance,
                    capturesContext: value,
                    verified: Verified);
            }
        }
    }
}
