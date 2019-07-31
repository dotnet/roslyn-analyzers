// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotOverloadSerializationBinderWithoutThrowingAnException : DiagnosticAnalyzer
    {
        protected string OverloadingClassMetadataName { get; }
        enum cacheState
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
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

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
                    Dictionary<IMethodSymbol, cacheState> methodCache = new Dictionary<IMethodSymbol, cacheState>();
                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            IMethodSymbol msym = (IMethodSymbol)symbolAnalysisContext.Symbol;
                            if (msym.Name == "BindToType")
                            {
                                if (isStandardSerBinder(msym.ContainingType))
                                {
                                    IBlockOperation msymOps = msym.GetTopmostOperationBlock(compilationStartAnalysisContext.Compilation);
                                    methodCache[msym] = cacheState.InProgress;
                                    if (!hasThrow(msymOps.Operations))
                                    {
                                        symbolAnalysisContext.ReportDiagnostic(
                                            Diagnostic.Create(
                                                DoNotOverloadSerializationBinderWithoutThrowingAnExceptionRule,
                                                msym.Locations[0],
                                                msym.ToDisplayString(
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
                            return false; //never happens, left for comprehensiveness
                        }
                        foreach (IOperation op in ops)
                        {
                            if (op == null) { continue; }
                            if (op.Kind == OperationKind.Invocation)
                            {
                                IMethodSymbol func = ((IInvocationOperation)op).TargetMethod;
                                cacheState methodState = cacheState.Absent;
                                if (!methodCache.TryGetValue(func, out methodState))
                                {
                                    methodState = cacheState.Absent;
                                }
                                if (methodState == cacheState.PresentTrue)
                                {
                                    return true; //we've done this before
                                }
                                if (methodState == cacheState.PresentFalse)
                                {
                                    continue; //no need to go here again
                                }
                                if (methodState == cacheState.InProgress)
                                {
                                    continue; //avoid recursive functions causing a loop
                                }
                                methodCache[func] = cacheState.InProgress;
                                IBlockOperation funcOps = func.GetTopmostOperationBlock(compilationStartAnalysisContext.Compilation);
                                if (funcOps == null)
                                {
                                    //function's source is in another assembly, and we cannot scan it
                                    //we will assume in this case that, even if this function throws an exception, it's probably not one we care about
                                    methodCache[func] = cacheState.PresentFalse;
                                    continue;
                                }
                                if (hasThrow(funcOps.Operations))
                                {
                                    methodCache[func] = cacheState.PresentTrue;
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

        bool isStandardSerBinder(INamedTypeSymbol sb)
        {
            if (sb == null) { return false; }
            INamedTypeSymbol parent = sb;
            while (true)
            {
                if (parent == null)
                {
                    return false; //reached object without finding serializationBinder
                }
                if (parent.Name == "SerializationBinder")
                {
                    if (parent.ContainingNamespace == null) { return false; }
                    if (parent.ContainingNamespace.Name != "Serialization") { return false; }
                    INamespaceSymbol SysRunSer = parent.ContainingNamespace;
                    if (SysRunSer.ContainingNamespace == null) { return false; }
                    if (SysRunSer.ContainingNamespace.Name != "Runtime") { return false; }
                    INamespaceSymbol SysRun = SysRunSer.ContainingNamespace;
                    if (SysRun.ContainingNamespace == null) { return false; }
                    if (SysRun.ContainingNamespace.Name != "System") { return false; }
                    return true;
                }
                parent = parent.BaseType;
            }
        }

    }
}
