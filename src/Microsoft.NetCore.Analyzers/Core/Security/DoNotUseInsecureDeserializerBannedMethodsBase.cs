// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
        protected abstract ImmutableHashSet<string> BannedMethodNames { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for the diagnostic to create when a banned method is invoked.
        /// </summary>
        /// <remarks>The string format message argument is the target method name.</remarks>
        protected abstract DiagnosticDescriptor BannedMethodDescriptor { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(BannedMethodDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            ImmutableHashSet<string> cachedBannedMethodNames = this.BannedMethodNames;

            Debug.Assert(this.DeserializerTypeMetadataName != null);
            Debug.Assert(cachedBannedMethodNames != null);
            Debug.Assert(!cachedBannedMethodNames.IsEmpty);
            Debug.Assert(this.BannedMethodDescriptor != null);

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
                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    IInvocationOperation invocationOperation = 
                                        (IInvocationOperation) operationAnalysisContext.Operation;
                                    if (invocationOperation.TargetMethod.ContainingType == deserializerTypeSymbol
                                        && cachedBannedMethodNames.Contains(invocationOperation.TargetMethod.MetadataName))
                                    {
                                        operationAnalysisContext.ReportDiagnostic(
                                            Diagnostic.Create(
                                                this.BannedMethodDescriptor,
                                                invocationOperation.Syntax.GetLocation(),
                                                invocationOperation.TargetMethod.ToDisplayString(
                                                    SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                    }
                                },
                                OperationKind.Invocation);
                        });
                });
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
