// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Usage
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseVolatileReadWriteAnalyzer : DiagnosticAnalyzer
    {
        private const string RuleId = "CA2263";

        internal const string ThreadVolatileReadMethodName = nameof(Thread.VolatileRead);
        internal const string ThreadVolatileWriteMethodName = nameof(Thread.VolatileWrite);
        internal const string VolatileReadMethodName = nameof(Volatile.Read);
        internal const string VolatileWriteMethodName = nameof(Volatile.Write);

        internal static readonly DiagnosticDescriptor ReadDescriptor = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(UseVolatileReadTitle)),
            CreateLocalizableResourceString(nameof(UseVolatileReadMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            description: CreateLocalizableResourceString(nameof(UseVolatileReadDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor WriteDescriptor = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(UseVolatileWriteTitle)),
            CreateLocalizableResourceString(nameof(UseVolatileWriteMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            description: CreateLocalizableResourceString(nameof(UseVolatileWriteDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(ReadDescriptor, WriteDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(context =>
            {
                ImmutableArray<ISymbol> threadVolatileReadMethods;
                ImmutableArray<ISymbol> threadVolatileWriteMethods;
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingThread, out var threadType)
                    || (threadVolatileReadMethods = threadType.GetMembers(ThreadVolatileReadMethodName)).IsEmpty
                    || (threadVolatileWriteMethods = threadType.GetMembers(ThreadVolatileWriteMethodName)).IsEmpty)
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    var invocation = (IInvocationOperation)context.Operation;
                    if (invocation.Instance is not null)
                    {
                        return;
                    }

                    if (threadVolatileReadMethods.Contains(invocation.TargetMethod))
                    {
                        context.ReportDiagnostic(invocation.CreateDiagnostic(ReadDescriptor));
                    }
                    else if (threadVolatileWriteMethods.Contains(invocation.TargetMethod))
                    {
                        context.ReportDiagnostic(invocation.CreateDiagnostic(WriteDescriptor));
                    }
                }, OperationKind.Invocation);
            });
        }
    }
}