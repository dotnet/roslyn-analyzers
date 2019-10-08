﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// Base class for insecure deserializer analyzers.
    /// </summary>
    /// <remarks>This aids in implementing:
    /// 1. Detecting potentially insecure deserialization method calls.
    /// 2. Detecting references to potentially insecure methods.
    /// </remarks>
    public abstract class DoNotUseInsecureDeserializerMethodsBase : DiagnosticAnalyzer
    {
        /// <summary>
        /// Metadata name of the potentially insecure deserializer type.
        /// </summary>
        protected abstract string DeserializerTypeMetadataName { get; }

        /// <summary>
        /// Metadata names of potentially insecure methods.
        /// </summary>
        /// <remarks>Use <see cref="StringComparer.Ordinal"/>.</remarks>
        protected abstract ImmutableHashSet<string> DeserializationMethodNames { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when a potentially insecure method is invoked
        /// or referenced (e.g. used as a delegate).
        /// </summary>
        /// <remarks>The string format message argument is the method signature.</remarks>
        protected abstract DiagnosticDescriptor MethodUsedDescriptor { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create<DiagnosticDescriptor>(
                this.MethodUsedDescriptor);

        public sealed override void Initialize(AnalysisContext context)
        {
            ImmutableHashSet<string> cachedDeserializationMethodNames = this.DeserializationMethodNames;

            Debug.Assert(this.DeserializerTypeMetadataName != null);
            Debug.Assert(cachedDeserializationMethodNames != null);
            Debug.Assert(!cachedDeserializationMethodNames.IsEmpty);
            Debug.Assert(this.MethodUsedDescriptor != null);

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

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            IInvocationOperation invocationOperation =
                                (IInvocationOperation)operationAnalysisContext.Operation;
                            if (Equals(invocationOperation.Instance?.Type, deserializerTypeSymbol)
                                && cachedDeserializationMethodNames.Contains(invocationOperation.TargetMethod.MetadataName))
                            {
                                operationAnalysisContext.ReportDiagnostic(
                                    Diagnostic.Create(
                                        this.MethodUsedDescriptor,
                                        invocationOperation.Syntax.GetLocation(),
                                        invocationOperation.TargetMethod.ToDisplayString(
                                            SymbolDisplayFormat.MinimallyQualifiedFormat)));
                            }
                        },
                        OperationKind.Invocation);

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            IMethodReferenceOperation methodReferenceOperation =
                                (IMethodReferenceOperation)operationAnalysisContext.Operation;
                            if (Equals(methodReferenceOperation.Instance?.Type, deserializerTypeSymbol)
                                && cachedDeserializationMethodNames.Contains(methodReferenceOperation.Method.MetadataName))
                            {
                                operationAnalysisContext.ReportDiagnostic(
                                    Diagnostic.Create(
                                        this.MethodUsedDescriptor,
                                        methodReferenceOperation.Syntax.GetLocation(),
                                        methodReferenceOperation.Method.ToDisplayString(
                                            SymbolDisplayFormat.MinimallyQualifiedFormat)));
                            }
                        },
                        OperationKind.MethodReference);
                });
        }
    }
}
