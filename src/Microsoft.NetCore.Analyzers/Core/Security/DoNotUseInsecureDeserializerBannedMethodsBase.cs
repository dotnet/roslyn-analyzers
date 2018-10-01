// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// Base class for insecure deserializer analyzers.
    /// </summary>
    /// <remarks>This aids in implementing:
    /// 1. Banned methods.
    /// </remarks>
    public abstract class DoNotUseInsecureDeserializerBannedMethodsBase : DiagnosticAnalyzer
    {
        /// <summary>
        /// Metadata name of the potentially insecure deserializer type.
        /// </summary>
        protected abstract string DeserializerTypeMetadataName { get; }

        /// <summary>
        /// Metadata names of banned methods, which should not be used at all.
        /// </summary>
        protected virtual ImmutableHashSet<string> BannedMethodNames { get { return null; } }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for the diagnostic to create when a banned method is invoked.
        /// </summary>
        /// <remarks>Must be non-null if BannedMethods is non-null.  The string format message argument is the target method name.</remarks>
        protected virtual DiagnosticDescriptor BannedMethodDescriptor {  get { return null; } }

        // Statically cache things, so derived classes can be lazy and just return a new collection
        // everytime in their BannedMethodNames, etc overrides.
        private static object StaticCacheInitializationLock = new object();
        private static bool IsStaticCacheInitialized = false;
        private static ImmutableHashSet<string> CachedBannedMethodNames;

        public override void Initialize(AnalysisContext context)
        {
            if (!IsStaticCacheInitialized)
            {
                lock (StaticCacheInitializationLock)
                {
                    if (!IsStaticCacheInitialized)
                    {
                        CachedBannedMethodNames = this.BannedMethodNames;
                        IsStaticCacheInitialized = true;
                    }
                }
            }

            if (CachedBannedMethodNames != null && this.BannedMethodDescriptor == null)
            {
                throw new NotImplementedException($"{nameof(BannedMethodNames)} is defined, but {nameof(BannedMethodDescriptor)} is not");
            }

            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    INamedTypeSymbol deserializerTypeSymbol =
                        compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(this.DeserializerTypeMetadataName);
                    if (deserializerTypeSymbol == null)
                    {
                        return;
                    }

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            if (CachedBannedMethodNames != null)
                            {
                                operationBlockStartAnalysisContext.RegisterOperationAction(
                                    (OperationAnalysisContext operationAnalysisContext) =>
                                    {
                                        this.HandleInvocationOperation(
                                            deserializerTypeSymbol,
                                            operationAnalysisContext);
                                    },
                                    OperationKind.Invocation);
                            }
                        });
                });
        }

        private void HandleInvocationOperation(
            INamedTypeSymbol deserializerTypeSymbol, 
            OperationAnalysisContext operationAnalysisContext)
        {
            IInvocationOperation invocationOperation = (IInvocationOperation) operationAnalysisContext.Operation;
            if (invocationOperation.TargetMethod.ContainingType == deserializerTypeSymbol
                && CachedBannedMethodNames.Contains(invocationOperation.TargetMethod.MetadataName))
            {
                operationAnalysisContext.ReportDiagnostic(
                    Diagnostic.Create(
                        this.BannedMethodDescriptor,
                        invocationOperation.Syntax.GetLocation(),
                        invocationOperation.TargetMethod.MetadataName));
            }
        }

        /// <summary>
        /// Gets a <see cref="LocalizableResourceString"/> from <see cref="MicrosoftNetCoreSecurityResources"/>.
        /// </summary>
        /// <param name="name">Name of the resource string to retrieve.</param>
        /// <returns>The corresponding <see cref="LocalizableResourceString"/>.</returns>
        protected static LocalizableResourceString GetResourceString(string name)
        {
            return new LocalizableResourceString(
                    name,
                    MicrosoftNetCoreSecurityResources.ResourceManager,
                    typeof(MicrosoftNetCoreSecurityResources));
        }
    }
}
