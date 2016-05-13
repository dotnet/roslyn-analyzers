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
        // public const string MixedVersionsOfMefAttributesRuleId = "RS0006";           // Now RS0006 => Microsoft.Composition.Analyzers.DoNotMixAttributesFromDifferentVersionsOfMEFAnalyzer
        // public const string UseArrayEmptyRuleId = "RS0007";                          // Now CA1825 => System.Runtime.Analyzers.AvoidZeroLengthArrayAllocationsAnalyzer
        // public const string ImplementIEquatableRuleId = "RS0008";                    // Now CA1067 => Microsoft.ApiDesignGuidelines.Analyzers.EquatableAnalyzer
        // public const string OverrideObjectEqualsRuleId = "RS0009";                   // Now CA1815 => Microsoft.ApiDesignGuidelines.Analyzers.OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer
        // public const string DoNotUseVerbatimCrefsRuleId = "RS0010";                  // Now RS0010 => XmlDocumentationComments.Analyzers.AvoidUsingCrefTagsWithAPrefixAnalyzer
        // public const string CancellationTokenMustBeLastRuleId = "RS0011";            // Now CA1068 => Microsoft.ApiDesignGuidelines.Analyzers.CancellationTokenParametersMustComeLastAnalyzer
        // public const string DoNotCallToImmutableArrayRuleId = "RS0012";              // Now RS0012 => System.Collections.Immutable.Analyzers.DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer
        public const string DoNotAccessDiagnosticDescriptorRuleId = "RS0013";
        // public const string DoNotCallLinqOnIndexable = "RS0014";                     // Now RS0014 => System.Runtime.Analyzers.DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer
        // public const string ConsumePreserveSigRuleId = "RS0015";                     // Now RS0015 => System.Runtime.InteropServices.Analyzers.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer
        public const string DeclarePublicApiRuleId = "RS0016";
        public const string RemoveDeletedApiRuleId = "RS0017";
        // public const string DoNotCreateTasksWithoutTaskSchedulerRuleId = "RS0018";   // Now RS0018 => System.Threading.Tasks.Analyzers.DoNotCreateTasksWithoutPassingATaskSchedulerAnalyzer
        public const string SymbolDeclaredEventRuleId = "RS0019";
        // public const string DeadCodeRuleId = "RS0020";                               // Now ???
        // public const string DeadCodeTriggerRuleId = "RS0021";                        // Now ???
        public const string ExposedNoninstantiableTypeRuleId = "RS0022";
        // public const string MissingSharedAttributeRuleId = "RS0023";                 // Now RS0023 => Microsoft.Composition.Analyzers.PartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer
        public const string PublicApiFilesInvalid = "RS0024";
        public const string DuplicatedSymbolInPublicApiFiles = "RS0025";
    }
}
