// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// Checks overloads of the serialization binder's BindToType method
    /// to check whether they properly throw an exception
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotOverloadSerializationBinderWithoutThrowingAnException : DiagnosticAnalyzer
    {
        protected string OverloadingClassMetadataName { get; }
        enum CacheState
        {
            Absent,
            InProgress,
            PresentTrue,
            PresentFalse
        };

        internal static DiagnosticDescriptor DoNotOverloadSerializationBinderWithoutThrowingAnExceptionRule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA2340",
            nameof(MicrosoftNetCoreAnalyzersResources.DoNotOverloadSerializationBinderWithoutThrowingAnExceptionTitle),
            nameof(MicrosoftNetCoreAnalyzersResources.DoNotOverloadSerializationBinderWithoutThrowingAnExceptionMessage),
            DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create<DiagnosticDescriptor>(
                DoNotOverloadSerializationBinderWithoutThrowingAnExceptionRule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    Dictionary<IMethodSymbol, CacheState> methodCache = new Dictionary<IMethodSymbol, CacheState>();
                    WellKnownTypeProvider typeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);
                    INamedTypeSymbol SerializationBinder = null;
                    if (!typeProvider.TryGetTypeByMetadataName("System.Runtime.Serialization.SerializationBinder", out SerializationBinder))
                    {
                        System.Diagnostics.Debug.Fail("Unable to find System.Runtime.Serialization.SerializationBinder threw WellKnownTypeProvider");
                        return;
                    }

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            IMethodSymbol msym = (IMethodSymbol)symbolAnalysisContext.Symbol;
                            if (msym.Name == "BindToType")
                            {
                                if (ITypeSymbolExtensions.Inherits(msym.ContainingType, SerializationBinder))
                                {
                                    IBlockOperation msymOps = msym.GetTopmostOperationBlock(compilationStartAnalysisContext.Compilation);
                                    methodCache[msym] = CacheState.InProgress;
                                    if (!hasThrow(msymOps.Operations))
                                    {
                                        symbolAnalysisContext.ReportDiagnostic(
                                            Diagnostic.Create(
                                                DoNotOverloadSerializationBinderWithoutThrowingAnExceptionRule,
                                                msym.Locations[0],
                                                msym.ContainingType.ToDisplayString(
                                                    SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                    }
                                }
                            }
                        }
                    , SymbolKind.Method);

                    bool hasThrow(IEnumerable<Microsoft.CodeAnalysis.IOperation> ops)
                    {
                        if (ops == null)
                        {
                            //doesn't happen, but just in case
                            System.Diagnostics.Debug.Fail("hasThrow called on null value. It should be called on an IEnumerable of IOperations.");
                            return false;
                        }

                        foreach (IOperation op in ops)
                        {
                            if (op == null)
                            {
                                continue;
                            }
                            if (op.Kind == OperationKind.Invocation)
                            {
                                IMethodSymbol func = ((IInvocationOperation)op).TargetMethod;
                                CacheState methodState = CacheState.Absent;
                                if (!methodCache.TryGetValue(func, out methodState))
                                {
                                    methodState = CacheState.Absent;
                                }
                                if (methodState == CacheState.PresentTrue)
                                {
                                    return true; //we've done this before
                                }
                                if (methodState == CacheState.PresentFalse)
                                {
                                    continue; //no need to go here again
                                }
                                if (methodState == CacheState.InProgress)
                                {
                                    continue; //avoid recursive functions causing a loop
                                }
                                methodCache[func] = CacheState.InProgress;
                                IBlockOperation funcOps = func.GetTopmostOperationBlock(compilationStartAnalysisContext.Compilation);
                                if (funcOps == null)
                                {
                                    //function's source is in another assembly, and we cannot scan it
                                    //we will assume in this case that, even if this function throws an exception, it's probably not one we care about
                                    methodCache[func] = CacheState.PresentFalse;
                                    continue;
                                }
                                if (hasThrow(funcOps.Operations))
                                {
                                    methodCache[func] = CacheState.PresentTrue;
                                    return true;
                                }
                            }

                            if (op.Kind == OperationKind.Throw)
                            {
                                return true;
                            }

                            if (hasThrow(op.Descendants()))
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                }
            );
        }
    }
}
