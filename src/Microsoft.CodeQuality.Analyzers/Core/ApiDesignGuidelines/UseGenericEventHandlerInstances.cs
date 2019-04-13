﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1003: Use generic event handler instances
    /// CA1009: A delegate that handles a public or protected event does not have the correct signature, return type, or parameter names.
    /// 
    /// Recommends that event handlers use <see cref="System.EventHandler{TEventArgs}"/>
    /// </summary>
    /// <remarks>
    /// NOTE: Legacy FxCop reports CA1009 for delegate type that handles a public or protected event and does not have the correct signature, return type, or parameter names.
    ///       This rule recommends fixing the signature to use a valid non-generic event handler.
    ///       We do not report CA1009, but instead report CA1003 and recommend using a generic event handler.
    /// </remarks>
    public abstract class UseGenericEventHandlerInstancesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1003";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericEventHandlerInstancesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageForDelegate = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericEventHandlerInstancesForDelegateMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionForDelegate = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericEventHandlerInstancesForDelegateDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageForEvent = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericEventHandlerInstancesForEventMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionForEvent = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericEventHandlerInstancesForEventDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageForEvent2 = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericEventHandlerInstancesForEvent2Message), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionForEvent2 = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericEventHandlerInstancesForEvent2Description), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor RuleForDelegates = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessageForDelegate,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: false,
            description: s_localizableDescriptionForDelegate,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1003-use-generic-event-handler-instances",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        internal static DiagnosticDescriptor RuleForEvents = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessageForEvent,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: false,
            description: s_localizableDescriptionForEvent,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1003-use-generic-event-handler-instances",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        internal static DiagnosticDescriptor RuleForEvents2 = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessageForEvent2,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: false,
            description: s_localizableDescriptionForEvent2,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1003-use-generic-event-handler-instances",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleForDelegates, RuleForEvents, RuleForEvents2);
        protected abstract bool IsAssignableTo(Compilation compilation, ITypeSymbol fromSymbol, ITypeSymbol toSymbol);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    INamedTypeSymbol eventArgs = WellKnownTypes.EventArgs(context.Compilation);
                    if (eventArgs == null)
                    {
                        return;
                    }

                    // Only analyze compilations that have a generic event handler defined.
                    if (WellKnownTypes.GenericEventHandler(context.Compilation) == null)
                    {
                        return;
                    }

                    bool IsDelegateTypeWithInvokeMethod(INamedTypeSymbol namedType) =>
                        namedType.TypeKind == TypeKind.Delegate && namedType.DelegateInvokeMethod != null;

                    bool IsEventArgsParameter(IParameterSymbol parameter)
                    {
                        var type = parameter.Type;
                        if (IsAssignableTo(context.Compilation, type, eventArgs))
                        {
                            return true;
                        }

                        // FxCop compat: Struct with name ending with "EventArgs" are allowed.
                        if (type.IsValueType)
                        {
                            return type.Name.EndsWith("EventArgs", StringComparison.Ordinal);
                        }

                        return false;
                    }

                    bool IsValidNonGenericEventHandler(IMethodSymbol delegateInvokeMethod)
                    {
                        Debug.Assert(delegateInvokeMethod != null);

                        return delegateInvokeMethod.ReturnsVoid &&
                            delegateInvokeMethod.Parameters.Length == 2 &&
                            delegateInvokeMethod.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                            IsEventArgsParameter(delegateInvokeMethod.Parameters[1]);
                    }

                    context.RegisterSymbolAction(symbolContext =>
                    {
                        // Note all the descriptors/rules for this analyzer have the same ID and category and hence
                        // will always have identical configured visibility.
                        var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                        if (namedType.MatchesConfiguredVisibility(symbolContext.Options, RuleForDelegates, symbolContext.CancellationToken) &&
                            IsDelegateTypeWithInvokeMethod(namedType) &&
                            IsValidNonGenericEventHandler(namedType.DelegateInvokeMethod))
                        {
                            // CA1003: Remove '{0}' and replace its usage with a generic EventHandler, for e.g. EventHandler&lt;T&gt;, where T is a valid EventArgs
                            symbolContext.ReportDiagnostic(namedType.CreateDiagnostic(RuleForDelegates, namedType.Name));
                        }
                    }, SymbolKind.NamedType);

                    INamedTypeSymbol comSourceInterfacesAttribute = WellKnownTypes.ComSourceInterfaceAttribute(context.Compilation);
                    bool ContainingTypeHasComSourceInterfacesAttribute(IEventSymbol eventSymbol) =>
                        comSourceInterfacesAttribute != null &&
                        eventSymbol.ContainingType.GetAttributes().Any(a => Equals(a.AttributeClass, comSourceInterfacesAttribute));

                    context.RegisterSymbolAction(symbolContext =>
                    {
                        // NOTE: Legacy FxCop reports CA1009 for delegate type that handles a public or protected event and does not have the correct signature, return type, or parameter names.
                        //       which recommends fixing the signature to use a valid non-generic event handler.
                        //       We do not report CA1009, but instead report CA1003 and recommend using a generic event handler.
                        // Note all the descriptors/rules for this analyzer have the same ID and category and hence
                        // will always have identical configured visibility.
                        var eventSymbol = (IEventSymbol)symbolContext.Symbol;
                        if (eventSymbol.MatchesConfiguredVisibility(symbolContext.Options, RuleForEvents, symbolContext.CancellationToken) &&
                            !eventSymbol.IsOverride &&
                            !eventSymbol.IsImplementationOfAnyInterfaceMember() &&
                            !ContainingTypeHasComSourceInterfacesAttribute(eventSymbol) &&
                            eventSymbol.Type is INamedTypeSymbol eventType &&
                            IsDelegateTypeWithInvokeMethod(eventType))
                        {
                            if (eventType.IsImplicitlyDeclared)
                            {
                                // CA1003: Change the event '{0}' to use a generic EventHandler by defining the event type explicitly, for e.g. Event MyEvent As EventHandler(Of MyEventArgs).
                                symbolContext.ReportDiagnostic(eventSymbol.CreateDiagnostic(RuleForEvents2, eventSymbol.Name));
                            }
                            else if (!IsValidNonGenericEventHandler(eventType.DelegateInvokeMethod))
                            {
                                // CA1003: Change the event '{0}' to replace the type '{1}' with a generic EventHandler, for e.g. EventHandler&lt;T&gt;, where T is a valid EventArgs
                                symbolContext.ReportDiagnostic(eventSymbol.CreateDiagnostic(RuleForEvents, eventSymbol.Name, eventType.ToDisplayString()));
                            }

                        }
                    }, SymbolKind.Event);
                });
        }
    }
}
