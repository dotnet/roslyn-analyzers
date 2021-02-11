﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Roslyn.Diagnostics.Analyzers
{
    internal static class RoslynDiagnosticIds
    {
        public const string UseEmptyEnumerableRuleId = "RS0001";
        public const string UseSingletonEnumerableRuleId = "RS0002";
        // public const string DirectlyAwaitingTaskAnalyzerRuleId = "RS0003";           // Now CA2007 => Microsoft.ApiDesignGuidelines.Analyzers.DoNotDirectlyAwaitATaskAnalyzer
        public const string UseSiteDiagnosticsCheckerRuleId = "RS0004";
        public const string DoNotUseCodeActionCreateRuleId = "RS0005";
        public const string MixedVersionsOfMefAttributesRuleId = "RS0006";
        // public const string UseArrayEmptyRuleId = "RS0007";                          // Now CA1825 => System.Runtime.Analyzers.AvoidZeroLengthArrayAllocationsAnalyzer
        // public const string ImplementIEquatableRuleId = "RS0008";                    // Now CA1067 => Microsoft.ApiDesignGuidelines.Analyzers.EquatableAnalyzer
        // public const string OverrideObjectEqualsRuleId = "RS0009";                   // Now CA1815 => Microsoft.ApiDesignGuidelines.Analyzers.OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer
        // public const string DoNotUseVerbatimCrefsRuleId = "RS0010";                  // Now RS0010 => XmlDocumentationComments.Analyzers.AvoidUsingCrefTagsWithAPrefixAnalyzer
        // public const string CancellationTokenMustBeLastRuleId = "RS0011";            // Now CA1068 => Microsoft.ApiDesignGuidelines.Analyzers.CancellationTokenParametersMustComeLastAnalyzer
        // public const string DoNotCallToImmutableArrayRuleId = "RS0012";              // Now CA2009 => System.Collections.Immutable.Analyzers.DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer
        //public const string DoNotAccessDiagnosticDescriptorRuleId = "RS0013";         // Removed (see https://github.com/dotnet/roslyn-analyzers/issues/3560)
        // public const string DoNotCallLinqOnIndexable = "RS0014";                     // Now RS0014 => System.Runtime.Analyzers.DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer
        // public const string ConsumePreserveSigRuleId = "RS0015";                     // Now CA2010 => System.Runtime.InteropServices.Analyzers.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer
        public const string DeclarePublicApiRuleId = "RS0016";
        public const string RemoveDeletedApiRuleId = "RS0017";
        // public const string DoNotCreateTasksWithoutTaskSchedulerRuleId = "RS0018";   // Now CA2008 => System.Threading.Tasks.Analyzers.DoNotCreateTasksWithoutPassingATaskSchedulerAnalyzer
        public const string SymbolDeclaredEventRuleId = "RS0019";
        // public const string DeadCodeRuleId = "RS0020";                               // Now ???
        // public const string DeadCodeTriggerRuleId = "RS0021";                        // Now ???
        public const string ExposedNoninstantiableTypeRuleId = "RS0022";
        public const string MissingSharedAttributeRuleId = "RS0023";
        public const string PublicApiFilesInvalid = "RS0024";
        public const string DuplicatedSymbolInPublicApiFiles = "RS0025";
        public const string AvoidMultipleOverloadsWithOptionalParameters = "RS0026";
        public const string OverloadWithOptionalParametersShouldHaveMostParameters = "RS0027";
        public const string RoslynAnalyzerMustUseIdInSpecifiedRangeRuleId = "RS0028";
        public const string RoslynAnalyzerMustUseCategoriesFromSpecifiedRangeRuleId = "RS0029";
        public const string SymbolIsBannedRuleId = "RS0030";
        public const string DuplicateBannedSymbolRuleId = "RS0031";
        public const string TestExportsShouldNotBeDiscoverableRuleId = "RS0032";
        public const string ImportingConstructorShouldBeObsoleteRuleId = "RS0033";
        public const string ExportedPartsShouldHaveImportingConstructorRuleId = "RS0034";
        public const string RestrictedInternalsVisibleToRuleId = "RS0035";
        public const string AnnotatePublicApiRuleId = "RS0036";
        public const string ShouldAnnotateApiFilesRuleId = "RS0037";
        public const string PreferNullLiteralRuleId = "RS0038";
        public const string RelaxTestNamingSuppressionRuleId = "RS0039";
        public const string DefaultableTypeShouldHaveDefaultableFieldsRuleId = "RS0040";
        public const string ObliviousPublicApiRuleId = "RS0041";
        public const string DoNotCopyValueRuleId = "RS0042";
        public const string DoNotCallGetTestAccessorRuleId = "RS0043";
        // public const string CreateTestAccessorRuleId = "RS0044"; // Now converted to a refactoring
        // public const string ExposeMemberForTestingRuleId = "RS0045"; // Now converted to a refactoring
        public const string AvoidOptSuffixForNullableEnableCodeRuleId = "RS0046";
        public const string NamedTypeFullNameNotNullSuppressionRuleId = "RS0047";
        public const string PublicApiFileMissing = "RS0048";
        public const string TemporaryArrayAsRefRuleId = "RS0049";

        //public const string WrapStatementsRuleId = "RS0100"; // Now ported to dotnet/roslyn https://github.com/dotnet/roslyn/pull/50358
        //public const string BlankLinesRuleId = "RS0101"; // Now ported to dotnet/roslyn https://github.com/dotnet/roslyn/pull/50358
        //public const string BracePlacementRuleId = "RS0102"; // Now ported to dotnet/roslyn https://github.com/dotnet/roslyn/pull/50358
    }
}
