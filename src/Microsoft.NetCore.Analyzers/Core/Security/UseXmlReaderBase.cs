// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    public abstract class UseXmlReaderBase : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.UseXmlReaderMessage),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.UseXmlReaderDescription),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources));

        /// <summary>
        /// Metadata name of the type which is recommended to use method take XmlReader as parameter.
        /// </summary>
        protected abstract string TypeMetadataName { get; }

        /// <summary>
        /// Metadata name of the method which is recommended to use XmlReader as parameter.
        /// </summary>
        protected abstract string MethodMetadataName { get; }

        protected abstract DiagnosticDescriptor Rule { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected static LocalizableString Description => s_Description;

        protected static LocalizableString Message => s_Message;

        public override void Initialize(AnalysisContext context)
        {
            Debug.Assert(TypeMetadataName != null);
            Debug.Assert(MethodMetadataName != null);
            Debug.Assert(Rule != null);

            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var compilation = compilationStartAnalysisContext.Compilation;
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                if (!wellKnownTypeProvider.TryGetTypeByMetadataName(
                            TypeMetadataName,
                            out INamedTypeSymbol xmlSchemaTypeSymbol))
                {
                    return;
                }

                wellKnownTypeProvider.TryGetTypeByMetadataName(
                            WellKnownTypeNames.SystemXmlXmlReader,
                            out INamedTypeSymbol xmlReaderTypeSymbol);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var operation = operationAnalysisContext.Operation;
                    IMethodSymbol methodSymbol = null;
                    string methodName = null;

                    switch (operation.Kind)
                    {
                        case OperationKind.Invocation:
                            methodSymbol = (operation as IInvocationOperation).TargetMethod;
                            methodName = methodSymbol.Name;
                            break;

                        case OperationKind.ObjectCreation:
                            methodSymbol = (operation as IObjectCreationOperation).Constructor;
                            methodName = methodSymbol.ContainingType.Name;
                            break;

                        default:
                            return;
                    }

                    if (methodName.StartsWith(MethodMetadataName, StringComparison.Ordinal) &&
                        methodSymbol.IsOverrideOrVirtualMethodOf(xmlSchemaTypeSymbol))
                    {
                        if (xmlReaderTypeSymbol != null &&
                            methodSymbol.Parameters.Length > 0 &&
                            methodSymbol.Parameters[0].Type.Equals(xmlReaderTypeSymbol))
                        {
                            return;
                        }

                        operationAnalysisContext.ReportDiagnostic(
                            operation.CreateDiagnostic(
                                Rule,
                                methodSymbol.ContainingType.Name,
                                methodName));
                    }
                }, OperationKind.Invocation, OperationKind.ObjectCreation);
            });
        }
    }
}
