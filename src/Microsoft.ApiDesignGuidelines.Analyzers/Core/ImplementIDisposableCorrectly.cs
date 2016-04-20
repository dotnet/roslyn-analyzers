// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1063: Implement IDisposable Correctly
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ImplementIDisposableCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1063";

        private const string HelpLinkUri = "https://msdn.microsoft.com/library/ms244737.aspx";
        private const string DisposeMethodName = "Dispose";
        private const string GarbageCollectorTypeName = "System.GC";
        private const string SuppressFinalizeMethodName = "SuppressFinalize";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageIDisposableReimplementation = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageIDisposableReimplementation), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFinalizeOverride = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageFinalizeOverride), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeOverride = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeOverride), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeSignature = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeSignature), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageRenameDispose = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageRenameDispose), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeBoolSignature = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeBoolSignature), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeImplementation = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeImplementation), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFinalizeImplementation = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageFinalizeImplementation), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageProvideDisposeBool = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageProvideDisposeBool), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor IDisposableReimplementationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIDisposableReimplementation,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor FinalizeOverrideRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFinalizeOverride,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeOverrideRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeOverride,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeSignatureRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeSignature,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor RenameDisposeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageRenameDispose,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeBoolSignatureRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeBoolSignature,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeImplementationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeImplementation,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor FinalizeImplementationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFinalizeImplementation,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ProvideDisposeBoolRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageProvideDisposeBool,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(IDisposableReimplementationRule, FinalizeOverrideRule, DisposeOverrideRule, DisposeSignatureRule, RenameDisposeRule, DisposeBoolSignatureRule, DisposeImplementationRule, FinalizeImplementationRule, ProvideDisposeBoolRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                context =>
                {
                    INamedTypeSymbol disposableType = WellKnownTypes.IDisposable(context.Compilation);
                    if (disposableType == null)
                    {
                        return;
                    }

                    var disposeInterfaceMethod = disposableType.GetMembers(DisposeMethodName).Single() as IMethodSymbol;
                    if (disposeInterfaceMethod == null)
                    {
                        return;
                    }

                    INamedTypeSymbol garbageCollectorType = context.Compilation.GetTypeByMetadataName(GarbageCollectorTypeName);
                    if (garbageCollectorType == null)
                    {
                        return;
                    }

                    var suppressFinalizeMethod = garbageCollectorType.GetMembers(SuppressFinalizeMethodName).Single() as IMethodSymbol;
                    if (suppressFinalizeMethod == null)
                    {
                        return;
                    }

                    var analyzer = new PerCompilationAnalyzer(context.Compilation, disposableType, disposeInterfaceMethod, suppressFinalizeMethod);
                    analyzer.Initialize(context);
                });
        }

        private static bool IsDisposeBoolMethod(IMethodSymbol method)
        {
            if (method.Name == DisposeMethodName && method.MethodKind == MethodKind.Ordinary &&
                method.ReturnsVoid && method.Parameters.Length == 1)
            {
                IParameterSymbol parameter = method.Parameters[0];
                if (parameter.Type != null && parameter.Type.SpecialType == SpecialType.System_Boolean && parameter.RefKind == RefKind.None)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Analyzes single instance of compilation.
        /// </summary>
        private class PerCompilationAnalyzer
        {
            private readonly Compilation _compilation;
            private readonly INamedTypeSymbol _disposableType;
            private readonly IMethodSymbol _disposeInterfaceMethod;
            private readonly IMethodSymbol _suppressFinalizeMethod;

            public PerCompilationAnalyzer(Compilation compilation, INamedTypeSymbol disposableType, IMethodSymbol disposeInterfaceMethod, IMethodSymbol suppressFinalizeMethod)
            {
                _compilation = compilation;
                _disposableType = disposableType;
                _disposeInterfaceMethod = disposeInterfaceMethod;
                _suppressFinalizeMethod = suppressFinalizeMethod;
            }

            public void Initialize(CompilationStartAnalysisContext context)
            {
                context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
                context.RegisterOperationBlockAction(AnalyzeOperationBlock);
            }

            private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
            {
                var type = context.Symbol as INamedTypeSymbol;
                if (type != null && type.TypeKind == TypeKind.Class)
                {
                    bool implementsDisposableInBaseType = ImplementsDisposableInBaseType(type);

                    if (ImplementsDisposableDirectly(type))
                    {
                        IMethodSymbol disposeMethod = FindDisposeMethod(type);
                        if (disposeMethod != null)
                        {
                            // This is difference from FxCop implementation
                            // IDisposable Reimplementation Rule is violated only if type re-implements Dispose method, not just interface
                            // For example see unit tests:
                            // CSharp_CA1063_IDisposableReimplementation_NoDiagnostic_ImplementingInheritedInterfaceWithNoDisposeReimplementation
                            // Basic_CA1063_IDisposableReimplementation_NoDiagnostic_ImplementingInheritedInterfaceWithNoDisposeReimplementation
                            CheckIDisposableReimplementationRule(type, context, implementsDisposableInBaseType);

                            CheckDisposeSignatureRule(disposeMethod, type, context);
                            CheckRenameDisposeRule(disposeMethod, type, context);

                            if (!type.IsSealed && type.DeclaredAccessibility != Accessibility.Private)
                            {
                                IMethodSymbol disposeBoolMethod = FindDisposeBoolMethod(type);
                                if (disposeBoolMethod != null)
                                {
                                    CheckDisposeBoolSignatureRule(disposeBoolMethod, type, context);
                                }
                                else
                                {
                                    CheckProvideDisposeBoolRule(type, context);
                                }
                            }
                        }
                        else if (type.Interfaces.Contains(_disposableType))
                        {
                            // Reports violation, when type mentions IDisposable as implemented interface,
                            // even when Dispose method is not implemented, but inherited from base type
                            // For example see unit test:
                            // CSharp_CA1063_IDisposableReimplementation_Diagnostic_ReImplementingIDisposableWithNoDisposeMethod
                            CheckIDisposableReimplementationRule(type, context, implementsDisposableInBaseType);
                        }
                    }

                    if (implementsDisposableInBaseType)
                    {
                        foreach (IMethodSymbol method in type.GetMembers().OfType<IMethodSymbol>())
                        {
                            CheckDisposeOverrideRule(method, type, context);
                        }

                        CheckFinalizeOverrideRule(type, context);
                    }
                }
            }

            private void AnalyzeOperationBlock(OperationBlockAnalysisContext context)
            {
                var method = context.OwningSymbol as IMethodSymbol;
                if (method == null)
                {
                    return;
                }

                bool isFinalizerMethod = method.IsFinalizer();
                bool isDisposeMethod = method.Name == DisposeMethodName;
                if (isFinalizerMethod || isDisposeMethod)
                {
                    INamedTypeSymbol type = method.ContainingType;
                    if (type != null && type.TypeKind == TypeKind.Class &&
                        !type.IsSealed && type.DeclaredAccessibility != Accessibility.Private)
                    {
                        if (ImplementsDisposableDirectly(type))
                        {
                            IMethodSymbol disposeMethod = FindDisposeMethod(type);
                            if (disposeMethod != null)
                            {
                                if (method == disposeMethod)
                                {
                                    CheckDisposeImplementationRule(method, type, context.OperationBlocks, context);
                                }
                                else if (isFinalizerMethod)
                                {
                                    // Check implementation of finalizer only if the class explicitly implements IDisposable
                                    // If class implements interface inherited from IDisposable and IDisposable is implemented in base class
                                    // then implementation of finalizer is ignored
                                    CheckFinalizeImplementationRule(method, type, context.OperationBlocks, context);
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Check rule: Remove IDisposable from the list of interfaces implemented by {0} and override the base class Dispose implementation instead.
            /// </summary>
            private static void CheckIDisposableReimplementationRule(INamedTypeSymbol type, SymbolAnalysisContext context, bool implementsDisposableInBaseType)
            {
                if (implementsDisposableInBaseType)
                {
                    context.ReportDiagnostic(type.CreateDiagnostic(IDisposableReimplementationRule, type.Name));
                }
            }

            // CA1801: Remove unused parameters.
            // TODO: Remove the below suppression once Roslyn bug https://github.com/dotnet/roslyn/issues/8884 is fixed.
#pragma warning disable CA1801
            /// <summary>
            /// Checks rule: Ensure that {0} is declared as public and sealed.
            /// </summary>
            private static void CheckDisposeSignatureRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
#pragma warning restore CA1801
            {
                if (!method.IsPublic() ||
                    method.IsAbstract || method.IsVirtual || (method.IsOverride && !method.IsSealed))
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(DisposeSignatureRule, $"{type.Name}.{method.Name}"));
                }
            }

            // CA1801: Remove unused parameters.
            // TODO: Remove the below suppression once Roslyn bug https://github.com/dotnet/roslyn/issues/8884 is fixed.
#pragma warning disable CA1801
            /// <summary>
            /// Checks rule: Rename {0} to 'Dispose' and ensure that it is declared as public and sealed.
            /// </summary>
            private static void CheckRenameDisposeRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
#pragma warning restore CA1801
            {
                if (method.Name != DisposeMethodName)
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(RenameDisposeRule, $"{type.Name}.{method.Name}"));
                }
            }

            // CA1801: Remove unused parameters.
            // TODO: Remove the below suppression once Roslyn bug https://github.com/dotnet/roslyn/issues/8884 is fixed.
#pragma warning disable CA1801
            /// <summary>
            /// Checks rule: Remove {0}, override Dispose(bool disposing), and put the dispose logic in the code path where 'disposing' is true.
            /// </summary>
            private void CheckDisposeOverrideRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
#pragma warning restore CA1801
            {
                if (method.MethodKind == MethodKind.Ordinary && method.IsOverride && method.ReturnsVoid && method.Parameters.Length == 0)
                {
                    bool isDisposeOverride = false;
                    for (IMethodSymbol m = method.OverriddenMethod; m != null; m = m.OverriddenMethod)
                    {
                        if (m == FindDisposeMethod(m.ContainingType))
                        {
                            isDisposeOverride = true;
                            break;
                        }
                    }

                    if (isDisposeOverride)
                    {
                        context.ReportDiagnostic(method.CreateDiagnostic(DisposeOverrideRule, $"{type.Name}.{method.Name}"));
                    }
                }
            }

            /// <summary>
            /// Checks rule: Remove the finalizer from type {0}, override Dispose(bool disposing), and put the finalization logic in the code path where 'disposing' is false.
            /// </summary>
            private static void CheckFinalizeOverrideRule(INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                if (type.HasFinalizer())
                {
                    context.ReportDiagnostic(type.CreateDiagnostic(FinalizeOverrideRule, type.Name));
                }
            }

            /// <summary>
            /// Checks rule: Provide an overridable implementation of Dispose(bool) on {0} or mark the type as sealed. A call to Dispose(false) should only clean up native resources. A call to Dispose(true) should clean up both managed and native resources.
            /// </summary>
            private static void CheckProvideDisposeBoolRule(INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                context.ReportDiagnostic(type.CreateDiagnostic(ProvideDisposeBoolRule, type.Name));
            }

            // CA1801: Remove unused parameters.
            // TODO: Remove the below suppression once Roslyn bug https://github.com/dotnet/roslyn/issues/8884 is fixed.
#pragma warning disable CA1801
            /// <summary>
            /// Checks rule: Ensure that {0} is declared as protected, virtual, and unsealed.
            /// </summary>
            private static void CheckDisposeBoolSignatureRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
#pragma warning restore CA1801
            {
                if (method.DeclaredAccessibility != Accessibility.Protected ||
                    !(method.IsVirtual || method.IsAbstract || method.IsOverride) || method.IsSealed)
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(DisposeBoolSignatureRule, $"{type.Name}.{method.Name}"));
                }
            }

            /// <summary>
            /// Checks rule: Modify {0} so that it calls Dispose(true), then calls GC.SuppressFinalize on the current object instance ('this' or 'Me' in Visual Basic), and then returns.
            /// </summary>
            private void CheckDisposeImplementationRule(IMethodSymbol method, INamedTypeSymbol type, ImmutableArray<IOperation> operationBlocks, OperationBlockAnalysisContext context)
            {
                var validator = new DisposeImplementationValidator(_suppressFinalizeMethod, type);
                if (!validator.Validate(operationBlocks))
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(DisposeImplementationRule, $"{type.Name}.{method.Name}"));
                }
            }

            // CA1801: Remove unused parameters.
#pragma warning disable CA1801
            /// <summary>
            /// Checks rule: Modify {0} so that it calls Dispose(false) and then returns.
            /// </summary>
            private static void CheckFinalizeImplementationRule(IMethodSymbol method, INamedTypeSymbol type, ImmutableArray<IOperation> operationBlocks, OperationBlockAnalysisContext context)
#pragma warning restore CA1801
            {
                // TODO: Implement check of Finalize
            }

            /// <summary>
            /// Checks if type implements IDisposable interface or an interface inherited from IDisposable.
            /// Only direct implementation is taken into account, implementation in base type is ignored.
            /// </summary>
            private bool ImplementsDisposableDirectly(ITypeSymbol type)
            {
                return type.Interfaces.Any(i => i.Inherits(_disposableType));
            }

            /// <summary>
            /// Checks if base type implements IDisposable interface directly or indirectly.
            /// </summary>
            private bool ImplementsDisposableInBaseType(ITypeSymbol type)
            {
                return type.BaseType != null && type.BaseType.AllInterfaces.Contains(_disposableType);
            }

            /// <summary>
            /// Returns method that implements IDisposable.Dispose operation.
            /// Only direct implementation is taken into account, implementation in base type is ignored.
            /// </summary>
            private IMethodSymbol FindDisposeMethod(INamedTypeSymbol type)
            {
                var disposeMethod = type.FindImplementationForInterfaceMember(_disposeInterfaceMethod) as IMethodSymbol;
                if (disposeMethod != null && disposeMethod.ContainingType == type)
                {
                    return disposeMethod;
                }

                return null;
            }

            /// <summary>
            /// Returns method: void Dispose(bool)
            /// </summary>
            private static IMethodSymbol FindDisposeBoolMethod(INamedTypeSymbol type)
            {
                return type.GetMembers(DisposeMethodName).OfType<IMethodSymbol>().FirstOrDefault(IsDisposeBoolMethod);
            }
        }

        /// <summary>
        /// Validates implementation of Dispose method. The method must call Dispose(true) and then GC.SuppressFinalize(this).
        /// </summary>
        private struct DisposeImplementationValidator
        {
            // this type will be created per compilation 
            // this is actually a bug - https://github.com/dotnet/roslyn-analyzers/issues/845
#pragma warning disable RS1008
            private readonly IMethodSymbol _suppressFinalizeMethod;
            private readonly INamedTypeSymbol _type;
#pragma warning restore RS1008
            private bool _callsDisposeBool;
            private bool _callsSuppressFinalize;

            public DisposeImplementationValidator(IMethodSymbol suppressFinalizeMethod, INamedTypeSymbol type)
            {
                _callsDisposeBool = false;
                _callsSuppressFinalize = false;
                _suppressFinalizeMethod = suppressFinalizeMethod;
                _type = type;
            }

            public bool Validate(ImmutableArray<IOperation> operations)
            {
                _callsDisposeBool = false;
                _callsSuppressFinalize = false;

                if (ValidateOperations(operations))
                {
                    return _callsDisposeBool && _callsSuppressFinalize;
                }

                return false;
            }

            private bool ValidateOperations(ImmutableArray<IOperation> operations)
            {
                foreach (IOperation operation in operations)
                {
                    if (!ValidateOperation(operation))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool ValidateOperation(IOperation operation)
            {
                switch (operation.Kind)
                {
                    case OperationKind.EmptyStatement:
                    case OperationKind.LabelStatement:
                        return true;
                    case OperationKind.BlockStatement:
                        var blockStatement = (IBlockStatement)operation;
                        return ValidateOperations(blockStatement.Statements);
                    case OperationKind.ExpressionStatement:
                        var expressionStatement = (IExpressionStatement)operation;
                        return ValidateExpression(expressionStatement);
                    default:
                        return false;
                }
            }

            private bool ValidateExpression(IExpressionStatement expressionStatement)
            {
                if (expressionStatement.Expression == null || expressionStatement.Expression.Kind != OperationKind.InvocationExpression)
                {
                    return false;
                }

                var invocationExpression = (IInvocationExpression)expressionStatement.Expression;
                if (!_callsDisposeBool)
                {
                    bool result = IsDisposeBoolCall(invocationExpression);
                    if (result)
                    {
                        _callsDisposeBool = true;
                    }

                    return result;
                }
                else if (!_callsSuppressFinalize)
                {
                    bool result = IsSuppressFinalizeCall(invocationExpression);
                    if (result)
                    {
                        _callsSuppressFinalize = true;
                    }

                    return result;
                }

                return false;
            }

            private bool IsDisposeBoolCall(IInvocationExpression invocationExpression)
            {
                if (invocationExpression.TargetMethod == null ||
                    invocationExpression.TargetMethod.ContainingType != _type ||
                    !IsDisposeBoolMethod(invocationExpression.TargetMethod))
                {
                    return false;
                }

                if (invocationExpression.Instance.Kind != OperationKind.InstanceReferenceExpression)
                {
                    return false;
                }

                var instanceReferenceExpression = (IInstanceReferenceExpression)invocationExpression.Instance;
                if (instanceReferenceExpression.InstanceReferenceKind != InstanceReferenceKind.Implicit &&
                    instanceReferenceExpression.InstanceReferenceKind != InstanceReferenceKind.Explicit)
                {
                    return false;
                }

                if (invocationExpression.ArgumentsInParameterOrder.Length != 1)
                {
                    return false;
                }

                IArgument argument = invocationExpression.ArgumentsInParameterOrder[0];
                if (argument.Value.Kind != OperationKind.LiteralExpression)
                {
                    return false;
                }

                var literal = (ILiteralExpression)argument.Value;
                if (!literal.ConstantValue.HasValue || !true.Equals(literal.ConstantValue.Value))
                {
                    return false;
                }

                return true;
            }

            private bool IsSuppressFinalizeCall(IInvocationExpression invocationExpression)
            {
                if (invocationExpression.TargetMethod != _suppressFinalizeMethod)
                {
                    return false;
                }

                if (invocationExpression.ArgumentsInParameterOrder.Length != 1)
                {
                    return false;
                }

                IOperation argumentValue = invocationExpression.ArgumentsInParameterOrder[0].Value;
                if (argumentValue.Kind != OperationKind.ConversionExpression)
                {
                    return false;
                }

                var conversion = (IConversionExpression)argumentValue;
                if (conversion.ConversionKind != ConversionKind.Cast && conversion.ConversionKind != ConversionKind.CSharp && conversion.ConversionKind != ConversionKind.Basic)
                {
                    return false;
                }

                if (conversion.Operand == null || conversion.Operand.Kind != OperationKind.InstanceReferenceExpression)
                {
                    return false;
                }

                var instanceReferenceExpression = (IInstanceReferenceExpression)conversion.Operand;
                if (instanceReferenceExpression.InstanceReferenceKind != InstanceReferenceKind.Implicit &&
                    instanceReferenceExpression.InstanceReferenceKind != InstanceReferenceKind.Explicit)
                {
                    return false;
                }

                return true;
            }
        }
    }
}