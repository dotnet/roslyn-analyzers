// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1052: Static holder classes should be marked static, and should not have default
    /// constructors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This analyzer combines FxCop rules 1052 and 1053, with updated guidance. It detects
    /// "static holder types": types whose only members are static, except possibly for a
    /// default constructor. In C#, such a type should be marked static, and the default
    /// constructor removed. In VB, such a type should be replaced with a module.
    /// </para>
    /// <para>
    /// This analyzer behaves as similarly as possible to the existing implementations of the FxCop
    /// rules, even when those implementations appear to conflict with the MSDN documentation of
    /// those rules. For example, like FxCop, this analyzer emits a diagnostic when it detects a
    /// static holder class that is declared "sealed", even though the documentation of CA1052
    /// says that the cause of the diagnostic is that the class was not declared sealed. Like
    /// FxCop, this analyzer does not emit a diagnostic when a non-default constructor is declared,
    /// even though the title of CA1053 is "Static holder types should not have constructors".
    /// Like FxCop, this analyzer does emit a diagnostic when the type has a private default
    /// constructor, even though the documentation of CA1053 says it should only trigger for public
    /// or protected default constructor. Like FxCop, this analyzer does not emit a diagnostic when 
    /// class has a base class, however the diagnostic is emitted if class supports empty interface.
    /// </para>
    /// <para>
    /// The rationale for all of this is to facilitate a smooth transition from FxCop rules to the
    /// corresponding Roslyn-based analyzers.
    /// </para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class StaticHolderTypesAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1052";

        private static readonly LocalizableString s_title = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StaticHolderTypesShouldBeStaticOrNotInheritable),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_messageFormat = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StaticHolderTypeIsNotStatic),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            s_title,
            s_messageFormat,
            DiagnosticCategory.Design,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "http://msdn.microsoft.com/library/ms182168.aspx",
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) => context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol as INamedTypeSymbol;
            if (!symbol.IsStatic
                && (symbol.IsPublic() || symbol.IsProtected())
                && symbol.IsStaticHolderType())
            {
                context.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
            }
        }
    }

    internal static class CA1052Extensions
    {
        /// <summary>
        /// Returns a value indicating whether the specified symbol is a static
        /// holder type.
        /// </summary>
        /// <param name="symbol">
        /// The symbol being examined.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="symbol"/> is a static holder type;
        /// otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// A symbol is a static holder type if it is a class with at least one
        /// "qualifying member" (<see cref="IsQualifyingMember(ISymbol)"/>) and no
        /// "disqualifying members" (<see cref="IsDisqualifyingMember(ISymbol)"/>).
        /// </remarks>
        internal static bool IsStaticHolderType(this INamedTypeSymbol symbol)
        {
            if (symbol.TypeKind != TypeKind.Class)
            {
                return false;
            }

            if (symbol.BaseType.SpecialType != SpecialType.System_Object)
            {
                return false;
            }

            IEnumerable<ISymbol> declaredMembers = symbol.GetMembers().Where(m => !m.IsImplicitlyDeclared);

            return declaredMembers.Any(IsQualifyingMember) && !declaredMembers.Any(IsDisqualifyingMember);
        }

        /// <summary>
        /// Returns a value indicating whether the specified symbol qualifies as a
        /// member of a static holder class.
        /// </summary>
        /// <param name="member">
        /// The member being examined.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="member"/> qualifies as a member of
        /// a static holder class; otherwise <c>false</c>.
        /// </returns>
        private static bool IsQualifyingMember(ISymbol member)
        {
            // A type member *does* qualify as a member of a static holder class,
            // because even though it is *not* static, it is nevertheless not
            // per-instance.
            if (member.IsType())
            {
                return true;
            }

            // An user-defined operator method is not a valid member of a static holder
            // class, because even though it is static, it takes instances as
            // parameters, so presumably the author of the class intended for it to be
            // instantiated.
            if (member.IsUserDefinedOperator())
            {
                return false;
            }

            return member.IsStatic;
        }

        /// <summary>
        /// Returns a value indicating whether the presence of the specified symbol
        /// disqualifies a class from being considered a static holder class.
        /// </summary>
        /// <param name="member">
        /// The member being examined.
        /// </param>
        /// <returns>
        /// <c>true</c> if the presence of <paramref name="member"/> disqualifies the
        /// current type as a static holder class; otherwise <c>false</c>.
        /// </returns>
        private static bool IsDisqualifyingMember(ISymbol member)
        {
            // An user-defined operator method disqualifies a class from being considered
            // a static holder, because even though it is static, it takes instances as
            // parameters, so presumably the author of the class intended for it to be
            // instantiated.
            if (member.IsUserDefinedOperator())
            {
                return true;
            }

            // A type member does *not* disqualify a class from being considered a static
            // holder, because even though it is *not* static, it is nevertheless not
            // per-instance.
            if (member.IsType())
            {
                return false;
            }

            // Any instance member other than a default constructor disqualifies a class
            // from being considered a static holder class.
            return !member.IsStatic && !member.IsDefaultConstructor();
        }

        internal static bool IsProtected(this ISymbol symbol)
        {
            return symbol.DeclaredAccessibility == Accessibility.Protected;
        }

        public static bool IsDefaultConstructor(this ISymbol symbol)
        {
            return symbol.IsConstructor() && symbol.GetParameters().Length == 0;
        }
    }
}