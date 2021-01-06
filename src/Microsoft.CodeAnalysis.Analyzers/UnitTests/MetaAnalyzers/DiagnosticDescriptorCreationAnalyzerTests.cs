﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.DiagnosticDescriptorCreationAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.DefineDiagnosticDescriptorArgumentsCorrectlyFix>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.DiagnosticDescriptorCreationAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.DefineDiagnosticDescriptorArgumentsCorrectlyFix>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests.MetaAnalyzers
{
    public class DiagnosticDescriptorCreationAnalyzerTests
    {
        #region RS1007 (UseLocalizableStringsInDescriptorRuleId) and RS1015 (ProvideHelpUriInDescriptorRuleId)

        [Fact]
        public async Task RS1007_RS1015_CSharp_VerifyDiagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true)|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(0),
                GetRS1028ResultAt(0));
        }

        [Fact]
        public async Task RS1007_RS1015_VisualBasic_VerifyDiagnostic()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = {|#0:new {|#1:DiagnosticDescriptor|}(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:= true)|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(1),
                GetRS1028ResultAt(1));
        }

        [Fact]
        public async Task RS1007_RS1015_CSharp_VerifyDiagnostic_NamedArgumentCases()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", messageFormat: ""MyDiagnosticMessage"", title: ""MyDiagnosticTitle"", {|#2:helpLinkUri: null|}, category: ""MyDiagnosticCategory"", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true)|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#3:new DiagnosticDescriptor(""MyDiagnosticId"", messageFormat: ""MyDiagnosticMessage"", title: ""MyDiagnosticTitle"", category: ""MyDiagnosticCategory"", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true)|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1028ResultAt(0),
                GetRS1015ExpectedDiagnostic(2),
                GetRS1007ExpectedDiagnostic(3),
                GetRS1015ExpectedDiagnostic(3),
                GetRS1028ResultAt(3));
        }

        [Fact]
        public async Task RS1007_RS1015_VisualBasic_VerifyDiagnostic_NamedArgumentCases()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = {|#0:new {|#1:DiagnosticDescriptor|}(""MyDiagnosticId"", title:=""MyDiagnosticTitle"", {|#2:helpLinkUri:=Nothing|}, messageFormat:=""MyDiagnosticMessage"", category:=""MyDiagnosticCategory"", defaultSeverity:=DiagnosticSeverity.Warning, isEnabledByDefault:= true)|}
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#3:new {|#4:DiagnosticDescriptor|}(""MyDiagnosticId"", title:=""MyDiagnosticTitle"", messageFormat:=""MyDiagnosticMessage"", category:=""MyDiagnosticCategory"", defaultSeverity:=DiagnosticSeverity.Warning, isEnabledByDefault:= true)|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1028ResultAt(1),
                GetRS1015ExpectedDiagnostic(2),
                GetRS1007ExpectedDiagnostic(3),
                GetRS1015ExpectedDiagnostic(4),
                GetRS1028ResultAt(4));
        }

        [Fact]
        public async Task RS1007_RS1015_CSharp_NoDiagnosticCases()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableString dummyLocalizableTitle = new LocalizableResourceString(""dummyName"", null, null);

    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, true, ""MyDiagnosticDescription."", ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#2:new DiagnosticDescriptor(helpLinkUri: ""HelpLink"", id: ""MyDiagnosticId"", messageFormat:""MyDiagnosticMessage"", title: dummyLocalizableTitle, category: ""MyDiagnosticCategory"", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true)|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}
",
                GetRS1028ResultAt(0),
                GetRS1028ResultAt(1),
                GetRS1028ResultAt(2));
        }

        [Fact]
        public async Task RS1007_RS1015_VisualBasic_NoDiagnosticCases()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly dummyLocalizableTitle As LocalizableString = new LocalizableResourceString(""dummyName"", Nothing, Nothing)
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = new {|#0:DiagnosticDescriptor|}(""MyDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:=true, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new {|#1:DiagnosticDescriptor|}(""MyDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""MyDiagnosticDescription."", ""HelpLink"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new {|#2:DiagnosticDescriptor|}(helpLinkUri:=""HelpLink"", id:=""MyDiagnosticId"", title:=dummyLocalizableTitle, messageFormat:=""MyDiagnosticMessage"", category:=""MyDiagnosticCategory"", defaultSeverity:=DiagnosticSeverity.Warning, isEnabledByDefault:=true)

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1028ResultAt(0),
                GetRS1028ResultAt(1),
                GetRS1028ResultAt(2));
        }

        #endregion

        #region RS1017 (DiagnosticIdMustBeAConstantRuleId) and RS1019 (UseUniqueDiagnosticIdRuleId)

        [Fact]
        public async Task RS1017_RS1019_CSharp_VerifyDiagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly string NonConstantDiagnosticId = ""NonConstantDiagnosticId"";
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor({|#1:NonConstantDiagnosticId|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer2 : DiagnosticAnalyzer
{
    private static LocalizableString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        {|#3:new DiagnosticDescriptor({|#4:""DuplicateDiagnosticId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1028ResultAt(0),
                GetRS1017ExpectedDiagnostic(1, "descriptor"),
                GetRS1028ResultAt(2),
                GetRS1028ResultAt(3),
                GetRS1019ExpectedDiagnostic(4, "DuplicateDiagnosticId", "MyAnalyzer"));
        }

        [Fact]
        public async Task RS1017_RS1019_CSharp_VerifyDiagnostic_CreateHelper()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly string NonConstantDiagnosticId = ""NonConstantDiagnosticId"";
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        DiagnosticDescriptorHelper.Create({|#0:NonConstantDiagnosticId|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"");

    private static readonly DiagnosticDescriptor descriptor2 =
        DiagnosticDescriptorHelper.Create(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer2 : DiagnosticAnalyzer
{
    private static LocalizableString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        DiagnosticDescriptorHelper.Create({|#1:""DuplicateDiagnosticId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"");


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}" + CSharpDiagnosticDescriptorCreationHelper,
                GetRS1017ExpectedDiagnostic(0, "descriptor"),
                GetRS1019ExpectedDiagnostic(1, "DuplicateDiagnosticId", "MyAnalyzer"));
        }

        [Fact]
        public async Task RS1017_RS1019_VisualBasic_VerifyDiagnostic()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer
    Private Shared ReadOnly NonConstantDiagnosticId = ""NonConstantDiagnosticId""
    Private Shared ReadOnly dummyLocalizableTitle As LocalizableString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = new {|#0:DiagnosticDescriptor|}({|#1:NonConstantDiagnosticId|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:=true, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new {|#2:DiagnosticDescriptor|}(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:=true, helpLinkUri:=""HelpLink"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer2
	Inherits DiagnosticAnalyzer
    Private Shared ReadOnly dummyLocalizableTitle As LocalizableString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = new {|#3:DiagnosticDescriptor|}({|#4:""DuplicateDiagnosticId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:=true, helpLinkUri:=""HelpLink"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1028ResultAt(0),
                GetRS1017ExpectedDiagnostic(1, "descriptor"),
                GetRS1028ResultAt(2),
                GetRS1028ResultAt(3),
                GetRS1019ExpectedDiagnostic(4, "DuplicateDiagnosticId", "MyAnalyzer"));
        }

        [Fact]
        public async Task RS1017_RS1019_VisualBasic_VerifyDiagnostic_CreateHelper()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer
    Private Shared ReadOnly NonConstantDiagnosticId = ""NonConstantDiagnosticId""
    Private Shared ReadOnly dummyLocalizableTitle As LocalizableString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create({|#0:NonConstantDiagnosticId|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer2
	Inherits DiagnosticAnalyzer
    Private Shared ReadOnly dummyLocalizableTitle As LocalizableString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create({|#1:""DuplicateDiagnosticId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
" + VisualBasicDiagnosticDescriptorCreationHelper,
                GetRS1017ExpectedDiagnostic(0, "descriptor"),
                GetRS1019ExpectedDiagnostic(1, "DuplicateDiagnosticId", "MyAnalyzer"));
        }

        [Fact]
        public async Task RS1017_RS1019_CSharp_NoDiagnosticCases()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private const string ConstantDiagnosticId = ""ConstantDiagnosticId"";
    private static LocalizableString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor(ConstantDiagnosticId, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#1:new DiagnosticDescriptor(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    // Allow multiple descriptors with same rule ID in the same analyzer.
    private static readonly DiagnosticDescriptor descriptor3 =
        {|#2:new DiagnosticDescriptor(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage2"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}
",
                GetRS1028ResultAt(0),
                GetRS1028ResultAt(1),
                GetRS1028ResultAt(2));
        }

        [Fact]
        public async Task RS1017_RS1019_CSharp_NoDiagnosticCases_CreateHelper()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private const string ConstantDiagnosticId = ""ConstantDiagnosticId"";
    private static LocalizableString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        DiagnosticDescriptorHelper.Create(ConstantDiagnosticId, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"");

    private static readonly DiagnosticDescriptor descriptor2 =
        DiagnosticDescriptorHelper.Create(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"");

    // Allow multiple descriptors with same rule ID in the same analyzer.
    private static readonly DiagnosticDescriptor descriptor3 =
        DiagnosticDescriptorHelper.Create(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage2"", ""MyDiagnosticCategory"");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}
" + CSharpDiagnosticDescriptorCreationHelper);
        }

        [Fact]
        public async Task RS1017_RS1019_VisualBasic_NoDiagnosticCases()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer
    Const ConstantDiagnosticId As String = ""ConstantDiagnosticId""
    Private Shared ReadOnly dummyLocalizableTitle As LocalizableString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = new {|#0:DiagnosticDescriptor|}(ConstantDiagnosticId, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:=true, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new {|#1:DiagnosticDescriptor|}(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:=true, helpLinkUri:=""HelpLink"")
    ' Allow multiple descriptors with same rule ID in the same analyzer.
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new {|#2:DiagnosticDescriptor|}(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage2"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault:=true, helpLinkUri:=""HelpLink"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1028ResultAt(0),
                GetRS1028ResultAt(1),
                GetRS1028ResultAt(2));
        }

        [Fact]
        public async Task RS1017_RS1019_VisualBasic_NoDiagnosticCases_CreateHelper()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer
    Const ConstantDiagnosticId As String = ""ConstantDiagnosticId""
    Private Shared ReadOnly dummyLocalizableTitle As LocalizableString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(ConstantDiagnosticId, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"")
    ' Allow multiple descriptors with same rule ID in the same analyzer.
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""DuplicateDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage2"", ""MyDiagnosticCategory"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
" + VisualBasicDiagnosticDescriptorCreationHelper);
        }

        #endregion

        #region RS1018 (DiagnosticIdMustBeInSpecifiedFormatRuleId) and RS1020 (UseCategoriesFromSpecifiedRangeRuleId)

        [Fact]
        public async Task RS1018_RS1020_CSharp_VerifyDiagnostic()
        {
            var source = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", {|#1:""NotAllowedCategory""|}, DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor({|#3:""DifferentPrefixId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#4:new DiagnosticDescriptor({|#5:""Prefix200""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor4 =
        {|#6:new DiagnosticDescriptor({|#7:""Prefix101""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor5 =
        {|#8:new DiagnosticDescriptor({|#9:""MySecondPrefix400""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor6 =
        {|#10:new DiagnosticDescriptor({|#11:""MyThirdPrefix""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2, descriptor3, descriptor4, descriptor5, descriptor6);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}
";
            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1028ResultAt(0),
                        GetRS1020ExpectedDiagnostic(1, "NotAllowedCategory", AdditionalFileName),
                        GetRS1028ResultAt(2),
                        GetRS1018ExpectedDiagnostic(3, "DifferentPrefixId", "CategoryWithPrefix", "PrefixXXXX", AdditionalFileName),
                        GetRS1028ResultAt(4),
                        GetRS1018ExpectedDiagnostic(5, "Prefix200", "CategoryWithRange", "Prefix0-Prefix99", AdditionalFileName),
                        GetRS1028ResultAt(6),
                        GetRS1018ExpectedDiagnostic(7, "Prefix101", "CategoryWithId", "Prefix100-Prefix100", AdditionalFileName),
                        GetRS1028ResultAt(8),
                        GetRS1018ExpectedDiagnostic(9, "MySecondPrefix400", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName),
                        GetRS1028ResultAt(10),
                        GetRS1018ExpectedDiagnostic(11, "MyThirdPrefix", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName)
                    }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        [Fact]
        public async Task RS1018_RS1020_CSharp_VerifyDiagnostic_CreateHelper()
        {
            var source = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        DiagnosticDescriptorHelper.Create(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", {|#0:""NotAllowedCategory""|});

    private static readonly DiagnosticDescriptor descriptor2 =
        DiagnosticDescriptorHelper.Create({|#1:""DifferentPrefixId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"");

    private static readonly DiagnosticDescriptor descriptor3 =
        DiagnosticDescriptorHelper.Create({|#2:""Prefix200""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"");

    private static readonly DiagnosticDescriptor descriptor4 =
        DiagnosticDescriptorHelper.Create({|#3:""Prefix101""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"");

    private static readonly DiagnosticDescriptor descriptor5 =
        DiagnosticDescriptorHelper.Create({|#4:""MySecondPrefix400""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"");

    private static readonly DiagnosticDescriptor descriptor6 =
        DiagnosticDescriptorHelper.Create({|#5:""MyThirdPrefix""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2, descriptor3, descriptor4, descriptor5, descriptor6);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}" + CSharpDiagnosticDescriptorCreationHelper;

            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1020ExpectedDiagnostic(0, "NotAllowedCategory", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(1, "DifferentPrefixId", "CategoryWithPrefix", "PrefixXXXX", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(2, "Prefix200", "CategoryWithRange", "Prefix0-Prefix99", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(3, "Prefix101", "CategoryWithId", "Prefix100-Prefix100", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(4, "MySecondPrefix400", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(5, "MyThirdPrefix", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName)
                    }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        [Fact]
        public async Task RS1018_RS1020_VisualBasic_VerifyDiagnostic()
        {
            var source = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = New {|#0:DiagnosticDescriptor|}(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", {|#1:""NotAllowedCategory""|}, DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = New {|#2:DiagnosticDescriptor|}({|#3:""DifferentPrefixId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = New {|#4:DiagnosticDescriptor|}({|#5:""Prefix200""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = New {|#6:DiagnosticDescriptor|}({|#7:""Prefix101""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor5 As DiagnosticDescriptor = New {|#8:DiagnosticDescriptor|}({|#9:""MySecondPrefix400""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor6 As DiagnosticDescriptor = New {|#10:DiagnosticDescriptor|}({|#11:""MyThirdPrefix""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor, descriptor2, descriptor3, descriptor4, descriptor5, descriptor6)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class
";
            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1028ResultAt(0),
                        GetRS1020ExpectedDiagnostic(1, "NotAllowedCategory", AdditionalFileName),
                        GetRS1028ResultAt(2),
                        GetRS1018ExpectedDiagnostic(3, "DifferentPrefixId", "CategoryWithPrefix", "PrefixXXXX", AdditionalFileName),
                        GetRS1028ResultAt(4),
                        GetRS1018ExpectedDiagnostic(5, "Prefix200", "CategoryWithRange", "Prefix0-Prefix99", AdditionalFileName),
                        GetRS1028ResultAt(6),
                        GetRS1018ExpectedDiagnostic(7, "Prefix101", "CategoryWithId", "Prefix100-Prefix100", AdditionalFileName),
                        GetRS1028ResultAt(8),
                        GetRS1018ExpectedDiagnostic(9, "MySecondPrefix400", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName),
                        GetRS1028ResultAt(10),
                        GetRS1018ExpectedDiagnostic(11, "MyThirdPrefix", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName),
                    }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        [Fact]
        public async Task RS1018_RS1020_VisualBasic_VerifyDiagnostic_CreateHelper()
        {
            var source = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", {|#0:""NotAllowedCategory""|})
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create({|#1:""DifferentPrefixId""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create({|#2:""Prefix200""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create({|#3:""Prefix101""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"")
    Private Shared ReadOnly descriptor5 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create({|#4:""MySecondPrefix400""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"")
    Private Shared ReadOnly descriptor6 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create({|#5:""MyThirdPrefix""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor, descriptor2, descriptor3, descriptor4, descriptor5, descriptor6)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class
" + VisualBasicDiagnosticDescriptorCreationHelper;

            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1020ExpectedDiagnostic(0, "NotAllowedCategory", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(1, "DifferentPrefixId", "CategoryWithPrefix", "PrefixXXXX", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(2, "Prefix200", "CategoryWithRange", "Prefix0-Prefix99", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(3, "Prefix101", "CategoryWithId", "Prefix100-Prefix100", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(4, "MySecondPrefix400", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName),
                        GetRS1018ExpectedDiagnostic(5, "MyThirdPrefix", "CategoryWithPrefixRangeAndId", "MyFirstPrefixXXXX, MySecondPrefix0-MySecondPrefix99, MySecondPrefix300-MySecondPrefix300", AdditionalFileName),
                    }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        [Fact]
        public async Task RS1018_RS1020_CSharp_NoDiagnosticCases()
        {
            var source = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithNoIdRangeOrFormat"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#1:new DiagnosticDescriptor(""Prefix"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor2_2 =
        {|#2:new DiagnosticDescriptor(""Prefix101"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#3:new DiagnosticDescriptor(""Prefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor4 =
        {|#4:new DiagnosticDescriptor(""Prefix100"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor5 =
        {|#5:new DiagnosticDescriptor(""MyFirstPrefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor6 =
        {|#6:new DiagnosticDescriptor(""MySecondPrefix050"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor7 =
        {|#7:new DiagnosticDescriptor(""MySecondPrefix300"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2, descriptor2_2, descriptor3, descriptor4, descriptor5, descriptor6, descriptor7);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}
";
            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1028ResultAt(0),
                        GetRS1028ResultAt(1),
                        GetRS1028ResultAt(2),
                        GetRS1028ResultAt(3),
                        GetRS1028ResultAt(4),
                        GetRS1028ResultAt(5),
                        GetRS1028ResultAt(6),
                        GetRS1028ResultAt(7),
                    }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        [Fact]
        public async Task RS1018_RS1020_CSharp_NoDiagnosticCases_CreateHelper()
        {
            var source = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        DiagnosticDescriptorHelper.Create(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithNoIdRangeOrFormat"");

    private static readonly DiagnosticDescriptor descriptor2 =
        DiagnosticDescriptorHelper.Create(""Prefix"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"");

    private static readonly DiagnosticDescriptor descriptor2_2 =
        DiagnosticDescriptorHelper.Create(""Prefix101"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"");

    private static readonly DiagnosticDescriptor descriptor3 =
        DiagnosticDescriptorHelper.Create(""Prefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"");

    private static readonly DiagnosticDescriptor descriptor4 =
        DiagnosticDescriptorHelper.Create(""Prefix100"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"");

    private static readonly DiagnosticDescriptor descriptor5 =
        DiagnosticDescriptorHelper.Create(""MyFirstPrefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"");

    private static readonly DiagnosticDescriptor descriptor6 =
        DiagnosticDescriptorHelper.Create(""MySecondPrefix050"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"");

    private static readonly DiagnosticDescriptor descriptor7 =
        DiagnosticDescriptorHelper.Create(""MySecondPrefix300"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2, descriptor2_2, descriptor3, descriptor4, descriptor5, descriptor6, descriptor7);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}" + CSharpDiagnosticDescriptorCreationHelper;

            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        [Fact]
        public async Task RS1018_RS1020_VisualBasic_NoDiagnosticCases()
        {
            var source = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = New {|#0:DiagnosticDescriptor|}(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithNoIdRangeOrFormat"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = New {|#1:DiagnosticDescriptor|}(""Prefix"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor2_2 As DiagnosticDescriptor = New {|#2:DiagnosticDescriptor|}(""Prefix"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = New {|#3:DiagnosticDescriptor|}(""Prefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = New {|#4:DiagnosticDescriptor|}(""Prefix100"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor5 As DiagnosticDescriptor = New {|#5:DiagnosticDescriptor|}(""MyFirstPrefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor6 As DiagnosticDescriptor = New {|#6:DiagnosticDescriptor|}(""MySecondPrefix050"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")
    Private Shared ReadOnly descriptor7 As DiagnosticDescriptor = New {|#7:DiagnosticDescriptor|}(""MySecondPrefix300"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault:=True, helpLinkUri:=""HelpLink"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor, descriptor2, descriptor2_2, descriptor3, descriptor4, descriptor5, descriptor6, descriptor7)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class
";
            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1028ResultAt(0),
                        GetRS1028ResultAt(1),
                        GetRS1028ResultAt(2),
                        GetRS1028ResultAt(3),
                        GetRS1028ResultAt(4),
                        GetRS1028ResultAt(5),
                        GetRS1028ResultAt(6),
                        GetRS1028ResultAt(7),
                    }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        [Fact]
        public async Task RS1018_RS1020_VisualBasic_NoDiagnosticCases_CreateHelper()
        {
            var source = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithNoIdRangeOrFormat"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""Prefix"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"")
    Private Shared ReadOnly descriptor2_2 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""Prefix"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""Prefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""Prefix100"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"")
    Private Shared ReadOnly descriptor5 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""MyFirstPrefix001"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"")
    Private Shared ReadOnly descriptor6 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""MySecondPrefix050"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"")
    Private Shared ReadOnly descriptor7 As DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(""MySecondPrefix300"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor, descriptor2, descriptor2_2, descriptor3, descriptor4, descriptor5, descriptor6, descriptor7)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class
" + VisualBasicDiagnosticDescriptorCreationHelper;

            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

CategoryWithNoIdRangeOrFormat
CategoryWithPrefix: Prefix
CategoryWithRange: Prefix000-Prefix099
CategoryWithId: Prefix100
CategoryWithPrefixRangeAndId: MyFirstPrefix, MySecondPrefix000-MySecondPrefix099, MySecondPrefix300
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        #endregion

        #region RS1021 (AnalyzerCategoryAndIdRangeFileInvalidRuleId)

        [Fact]
        public async Task RS1021_VerifyDiagnostic()
        {
            var source = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor =
        {|#0:new DiagnosticDescriptor(""Id1"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#1:new DiagnosticDescriptor(""DifferentPrefixId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefix"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#2:new DiagnosticDescriptor(""Prefix200"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithRange"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor4 =
        {|#3:new DiagnosticDescriptor(""Prefix101"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor5 =
        {|#4:new DiagnosticDescriptor(""MySecondPrefix400"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    private static readonly DiagnosticDescriptor descriptor6 =
        {|#5:new DiagnosticDescriptor(""MyThirdPrefix"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""CategoryWithPrefixRangeAndId"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor, descriptor2, descriptor3, descriptor4, descriptor5, descriptor6);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}
";
            string additionalText = @"
# FORMAT:
# 'Category': Comma separate list of 'StartId-EndId' or 'Id' or 'Prefix'

# Illegal: spaces in category name
{|#6:Category with spaces|}
{|#7:Category with spaces and range: Prefix100-Prefix199|}

# Illegal: Multiple colons
{|#8:CategoryMultipleColons: IdWithColon:100|}

# Illegal: Duplicate category
DuplicateCategory1
{|#9:DuplicateCategory1|}
DuplicateCategory2: Prefix100-Prefix199
{|#10:DuplicateCategory2: Prefix200-Prefix299|}

# Illegal: ID cannot be non-alphanumeric
{|#11:CategoryWithBadId1: Prefix_100|}
{|#12:CategoryWithBadId2: Prefix_100-Prefix_199|}

# Illegal: Id cannot have letters after number
{|#13:CategoryWithBadId3: Prefix000NotAllowed|}
{|#14:CategoryWithBadId4: Prefix000NotAllowed-Prefix099NotAllowed|}

# Illegal: Different prefixes in ID range
{|#15:CategoryWithBadId5: Prefix000-DifferentPrefix099|}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1021ExpectedDiagnostic(6, "Category with spaces", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(7, "Category with spaces and range: Prefix100-Prefix199", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(8, "CategoryMultipleColons: IdWithColon:100", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(9, "DuplicateCategory1", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(10, "DuplicateCategory2: Prefix200-Prefix299", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(11, "CategoryWithBadId1: Prefix_100", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(12, "CategoryWithBadId2: Prefix_100-Prefix_199", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(13, "CategoryWithBadId3: Prefix000NotAllowed", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(14, "CategoryWithBadId4: Prefix000NotAllowed-Prefix099NotAllowed", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(15, "CategoryWithBadId5: Prefix000-DifferentPrefix099", AdditionalFileName),
                        GetRS1028ResultAt(0),
                        GetRS1028ResultAt(1),
                        GetRS1028ResultAt(2),
                        GetRS1028ResultAt(3),
                        GetRS1028ResultAt(4),
                        GetRS1028ResultAt(5),
                    }
                },
                SolutionTransforms = { WithoutEnableReleaseTrackingWarning }
            }.RunAsync();
        }

        #endregion

        #region RS1028 (ProvideCustomTagsInDescriptorRuleId)
        [Fact]
        public async Task ReportOnMissingCustomTags()
        {
            await VerifyCSharpAnalyzerAsync(@"
using Microsoft.CodeAnalysis;
public class MyAnalyzer
{
    internal static DiagnosticDescriptor Rule1 = {|#0:new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false)|};
    internal static DiagnosticDescriptor Rule2 = {|#1:new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false)|};
    public void SomeMethod()
    {
        var diag = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false);
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(0),
                GetRS1028ResultAt(0),
                GetRS1015ExpectedDiagnostic(1),
                GetRS1028ResultAt(1));

            await VerifyBasicAnalyzerAsync(@"
Imports Microsoft.CodeAnalysis
Public Class MyAnalyzer
    Friend Shared Rule1 As DiagnosticDescriptor = {|#0:New {|#1:DiagnosticDescriptor|}("""", """", """", """", DiagnosticSeverity.Warning, False)|}
    Friend Shared Rule2 As DiagnosticDescriptor = New {|#2:DiagnosticDescriptor|}("""", New LocalizableResourceString("""", Nothing, Nothing), New LocalizableResourceString("""", Nothing, Nothing), """", DiagnosticSeverity.Warning, False)
    Public Sub SomeMethod()
        Dim diag = New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False)
    End Sub
End Class",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(1),
                GetRS1028ResultAt(1),
                GetRS1015ExpectedDiagnostic(2),
                GetRS1028ResultAt(2));
        }

        [Fact]
        public async Task DoNotReportOnNamedCustomTags()
        {
            await VerifyCSharpAnalyzerAsync(@"
using Microsoft.CodeAnalysis;
public class MyAnalyzer
{
    internal static DiagnosticDescriptor Rule1 = {|#0:new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, customTags: """")|};
    internal static DiagnosticDescriptor Rule2 = {|#1:new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false, customTags: """")|};
    public void SomeMethod()
    {
        var diag = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, customTags: """");
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(1));

            // Named arguments are incompatible with ParamArray in VB.NET
        }

        [Fact]
        public async Task DoNotReportOnCustomTags()
        {
            await VerifyCSharpAnalyzerAsync(@"
using Microsoft.CodeAnalysis;
public class MyAnalyzer
{
    internal static DiagnosticDescriptor Rule1 = {|#0:new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, null, {|#1:null|}, """")|};
    internal static DiagnosticDescriptor Rule2 = new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false, new LocalizableResourceString("""", null, null), """", """");
    internal static DiagnosticDescriptor Rule3 = {|#2:new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, null, {|#3:null|}, new[] { """", """" })|};
    internal static DiagnosticDescriptor Rule4 = new DiagnosticDescriptor("""", new LocalizableResourceString("""", null, null),
        new LocalizableResourceString("""", null, null), """", DiagnosticSeverity.Warning, false, new LocalizableResourceString("""", null, null), """", new[] { """", """" });
    public void SomeMethod()
    {
        var diag = new DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, false, null, null, """");
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2),
                GetRS1015ExpectedDiagnostic(3));

            await VerifyBasicAnalyzerAsync(@"
Imports Microsoft.CodeAnalysis
Public Class MyAnalyzer
    Friend Shared Rule1 As DiagnosticDescriptor = {|#0:New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False, Nothing, {|#1:Nothing|}, """")|}
    Friend Shared Rule2 As DiagnosticDescriptor = New DiagnosticDescriptor("""", New LocalizableResourceString("""", Nothing, Nothing), New LocalizableResourceString("""", Nothing, Nothing), """", DiagnosticSeverity.Warning, False, New LocalizableResourceString("""", Nothing, Nothing), """", """")
    Friend Shared Rule3 As DiagnosticDescriptor = {|#2:New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False, Nothing, {|#3:Nothing|}, { """", """" })|}
    Friend Shared Rule4 As DiagnosticDescriptor = New DiagnosticDescriptor("""", New LocalizableResourceString("""", Nothing, Nothing), New LocalizableResourceString("""", Nothing, Nothing), """", DiagnosticSeverity.Warning, False, New LocalizableResourceString("""", Nothing, Nothing), """", { """", """" })
    Public Sub SomeMethod()
        Dim diag = New DiagnosticDescriptor("""", """", """", """", DiagnosticSeverity.Warning, False, Nothing, Nothing, """")
    End Sub
End Class",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1015ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2),
                GetRS1015ExpectedDiagnostic(3));
        }
        #endregion

        #region RS1029 (DoNotUseReservedDiagnosticIdRuleId)

        [Fact, WorkItem(1727, "https://github.com/dotnet/roslyn-analyzers/issues/1727")]
        public async Task RS1029_AlreadyUsedId_Diagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor({|#0:""CA0""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor({|#1:""CS0""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor({|#2:""BC0""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor({|#3:""CA00000000000000000000""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor5 =
        new DiagnosticDescriptor({|#4:""CS00000000000000000000""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor6 =
        new DiagnosticDescriptor({|#5:""BC00000000000000000000""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1029ResultAt(0, "CA0"),
                GetRS1029ResultAt(1, "CS0"),
                GetRS1029ResultAt(2, "BC0"),
                GetRS1029ResultAt(3, "CA00000000000000000000"),
                GetRS1029ResultAt(4, "CS00000000000000000000"),
                GetRS1029ResultAt(5, "BC00000000000000000000"));

            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = New DiagnosticDescriptor({|#0:""CA0""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = New DiagnosticDescriptor({|#1:""CS0""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = New DiagnosticDescriptor({|#2:""BC0""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = New DiagnosticDescriptor({|#3:""CA00000000000000000000""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor5 As DiagnosticDescriptor = New DiagnosticDescriptor({|#4:""CS00000000000000000000""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor6 As DiagnosticDescriptor = New DiagnosticDescriptor({|#5:""BC00000000000000000000""|}, dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor1)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class",
                GetRS1029ResultAt(0, "CA0"),
                GetRS1029ResultAt(1, "CS0"),
                GetRS1029ResultAt(2, "BC0"),
                GetRS1029ResultAt(3, "CA00000000000000000000"),
                GetRS1029ResultAt(4, "CS00000000000000000000"),
                GetRS1029ResultAt(5, "BC00000000000000000000"));
        }

        [Fact, WorkItem(1727, "https://github.com/dotnet/roslyn-analyzers/issues/1727")]
        public async Task RS1029_DiagnosticIdSimilarButNotReserved_NoDiagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""CAA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""CSA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""BCA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor(""CA00A0"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor5 =
        new DiagnosticDescriptor(""CS00A0"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor6 =
        new DiagnosticDescriptor(""BC00A0"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}");

            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = New DiagnosticDescriptor(""CAA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = New DiagnosticDescriptor(""CSA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = New DiagnosticDescriptor(""BCA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = New DiagnosticDescriptor(""CA00A0"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor5 As DiagnosticDescriptor = New DiagnosticDescriptor(""CS00A0"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor6 As DiagnosticDescriptor = New DiagnosticDescriptor(""BC00A0"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor1)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class");
        }

        [Fact, WorkItem(1727, "https://github.com/dotnet/roslyn-analyzers/issues/1727")]
        public async Task RS1029_DiagnosticIdSimilarButTooShort_NoDiagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""CA"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""CS"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""BC"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}");

            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = New DiagnosticDescriptor(""CA"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = New DiagnosticDescriptor(""CS"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = New DiagnosticDescriptor(""BC"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor1)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class");
        }

        [Theory, WorkItem(1727, "https://github.com/dotnet/roslyn-analyzers/issues/1727")]
        [InlineData("Microsoft.CodeAnalysis.VersionCheckAnalyzer")]
        [InlineData("Microsoft.CodeAnalysis.NetAnalyzers")]
        [InlineData("Microsoft.CodeAnalysis.CSharp.NetAnalyzers")]
        [InlineData("Microsoft.CodeAnalysis.VisualBasic.NetAnalyzers")]
        [InlineData("Microsoft.CodeQuality.Analyzers")]
        [InlineData("Microsoft.CodeQuality.CSharp.Analyzers")]
        [InlineData("Microsoft.CodeQuality.VisualBasic.Analyzers")]
        [InlineData("Microsoft.NetCore.Analyzers")]
        [InlineData("Microsoft.NetCore.CSharp.Analyzers")]
        [InlineData("Microsoft.NetCore.VisualBasic.Analyzers")]
        [InlineData("Microsoft.NetFramework.Analyzers")]
        [InlineData("Microsoft.NetFramework.CSharp.Analyzers")]
        [InlineData("Microsoft.NetFramework.VisualBasic.Analyzers")]
        [InlineData("Text.Analyzers")]
        [InlineData("Text.CSharp.Analyzers")]
        [InlineData("Text.VisualBasic.Analyzers")]
        public async Task RS1029_CADiagnosticIdOnRoslynAnalyzers_NoDiagnostic(string assemblyName)
        {
            await new VerifyCS.Test
            {
                TestCode = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static LocalizableResourceString dummyLocalizableTitle = null;

    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""CA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                SolutionTransforms =
                {
                    (solution, projectId) => solution.GetProject(projectId).WithAssemblyName(assemblyName).Solution,
                    WithoutEnableReleaseTrackingWarning,
                },
            }.RunAsync();

            await new VerifyVB.Test
            {
                TestCode = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
    Inherits DiagnosticAnalyzer

    Private Shared dummyLocalizableTitle As LocalizableResourceString = Nothing
    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = New DiagnosticDescriptor(""CA0000"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""NotAllowedCategory"", DiagnosticSeverity.Warning, True, Nothing, ""HelpLink"", ""customTag"")

    Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(descriptor1)
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal context As AnalysisContext)
    End Sub
End Class",
                SolutionTransforms =
                {
                    (solution, projectId) => solution.GetProject(projectId).WithAssemblyName(assemblyName).Solution,
                    WithoutEnableReleaseTrackingWarning,
                }
            }.RunAsync();
        }

        #endregion

        #region RS1031 (DefineDiagnosticTitleCorrectlyRule)

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleStringEndsWithPeriod_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:""MyDiagnosticTitle.""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Title = {|#4:""MyDiagnosticTitle.""|};
    private static readonly DiagnosticDescriptor descriptor3 =
        {|#5:new DiagnosticDescriptor(""MyDiagnosticId"", {|#6:s_title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private static string s_title = {|#7:""MyDiagnosticTitle.""|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Title = ""MyDiagnosticTitle"";
    private static readonly DiagnosticDescriptor descriptor3 =
        {|#5:new DiagnosticDescriptor(""MyDiagnosticId"", s_title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private static string s_title = ""MyDiagnosticTitle"";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(4),
                GetRS1007ExpectedDiagnostic(5),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(6).WithLocation(7));

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:""MyDiagnosticTitle.""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Title As String = {|#4:""MyDiagnosticTitle.""|}
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = {|#5:new DiagnosticDescriptor(""MyDiagnosticId"", {|#6:s_title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly s_title As String = {|#7:""MyDiagnosticTitle.""|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Title As String = ""MyDiagnosticTitle""
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = {|#5:new DiagnosticDescriptor(""MyDiagnosticId"", s_title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly s_title As String = ""MyDiagnosticTitle""

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(4),
                GetRS1007ExpectedDiagnostic(5),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(6).WithLocation(7));
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleStringEndsWithPeriod_ResxFile_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>MyDiagnosticTitle1.</value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value>MyDiagnosticTitle2.</value>
    <comment>Optional comment.</comment>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>MyDiagnosticTitle1</value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value>MyDiagnosticTitle2</value>
    <comment>Optional comment.</comment>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableString Title = new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableString Title = new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As LocalizableString = New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As LocalizableString = New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleIsMultiSentence_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:""MyDiagnostic. Title.""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Title = {|#4:""MyDiagnostic. Title""|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnostic"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Title = ""MyDiagnostic"";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(4));

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:""MyDiagnostic. Title.""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Title As String = {|#4:""MyDiagnostic. Title""|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnostic"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Title As String = ""MyDiagnostic""

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(4));
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleIsMultiSentence_ResxFile_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>MyDiagnostic. Title.</value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value>MyDiagnostic. Title</value>
    <comment>Optional comment.</comment>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>MyDiagnostic</value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value>MyDiagnostic</value>
    <comment>Optional comment.</comment>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleIsMultiSentence_MultipleDescriptorsUsingSameTitle_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId1"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId2"", {|#3:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Title = {|#4:""MyDiagnosticTitle. AnalyzerTitle.""|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId1"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId2"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Title = ""MyDiagnosticTitle"";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
        GetRS1007ExpectedDiagnostic(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(4),
        GetRS1007ExpectedDiagnostic(2),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(4)
    );

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId1"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId2"", {|#3:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Title As String = {|#4:""MyDiagnosticTitle. AnalyzerTitle.""|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId1"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId2"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Title As String = ""MyDiagnosticTitle""

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
            GetRS1007ExpectedDiagnostic(0),
            VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(4),
            GetRS1007ExpectedDiagnostic(2),
            VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(4));
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleIsMultiSentence_MultipleDescriptorsUsingSameTitle_ResxFile_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerTitle"" xml:space=""preserve"">
    <value>MyDiagnostic. Title.</value>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerTitle"" xml:space=""preserve"">
    <value>MyDiagnostic</value>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId1"", {|#0:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId2"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId1"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId2"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId1"", {|#0:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId2"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId1"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId2"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleStringContainsLineReturn_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:""MyDiagnostic\rTitle""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:""MyDiagnostic\nTitle""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#4:new DiagnosticDescriptor(""MyDiagnosticId"", {|#5:""MyDiagnostic\r\nTitle""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnostic"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnostic"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#4:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnostic"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(3),
                GetRS1007ExpectedDiagnostic(4),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(5).WithLocation(5));

            // NOTE: Code fix does not handle binary operations.
            var vbCode = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualBasic

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:""MyDiagnostic"" & vbCr & ""Title""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:""MyDiagnostic"" & vbLf & ""Title""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = {|#4:new DiagnosticDescriptor(""MyDiagnosticId"", {|#5:""MyDiagnostic"" & vbCrLf & ""Title""|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
";
            await VerifyBasicCodeFixAsync(vbCode, vbCode,
                GetRS1007ExpectedDiagnostic(0),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3).WithLocation(3),
                GetRS1007ExpectedDiagnostic(4),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(5).WithLocation(5));
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_TitleStringContainsLineReturn_ResxFile_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>MyDiagnostic
Title.</value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value>MyDiagnostic
Title</value>
    <comment>Optional comment.</comment>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>MyDiagnostic</value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value>MyDiagnostic</value>
    <comment>Optional comment.</comment>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1)
    });
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1031_ValidTitleString_NoDiagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", ""Title can contain A.B qualifications"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""Title can contain 'A.B' qualifications"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1007ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2));

            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", ""Title can contain A.B qualifications"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""Title can contain 'A.B' qualifications"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1007ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2));
        }

        [WindowsOnlyFact, WorkItem(3958, "https://github.com/dotnet/roslyn-analyzers/issues/3958")]
        public async Task RS1031_LeadingOrTailingWhitespace_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>Title with trailing space </value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value> Title with leading space</value>
  </data>
  <data name=""AnalyzerTitle3"" xml:space=""preserve"">
    <value>  " + "\t" + @"    Title with leading and trailing spaces/tabs  " + "\t" + @"    </value>
  </data>
  <data name=""AnalyzerTitle4"" xml:space=""preserve"">
    <value>Title with trailing space. </value>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerTitle1"" xml:space=""preserve"">
    <value>Title with trailing space</value>
  </data>
  <data name=""AnalyzerTitle2"" xml:space=""preserve"">
    <value>Title with leading space</value>
  </data>
  <data name=""AnalyzerTitle3"" xml:space=""preserve"">
    <value>Title with leading and trailing spaces/tabs</value>
  </data>
  <data name=""AnalyzerTitle4"" xml:space=""preserve"">
    <value>Title with trailing space</value>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle3"", null, typeof(Resources));
    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#2:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:new LocalizableResourceString(""AnalyzerTitle4"", null, typeof(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", new LocalizableResourceString(""AnalyzerTitle1"", null, typeof(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", new LocalizableResourceString(""AnalyzerTitle2"", null, typeof(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly LocalizableResourceString Title = new LocalizableResourceString(""AnalyzerTitle3"", null, typeof(Resources));
    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor(""MyDiagnosticId"", new LocalizableResourceString(""AnalyzerTitle4"", null, typeof(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(2),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3),
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#0:New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#1:New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle3"", Nothing, GetType(Resources))
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#2:Title|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", {|#3:New LocalizableResourceString(""AnalyzerTitle4"", Nothing, GetType(Resources))|}, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", New LocalizableResourceString(""AnalyzerTitle1"", Nothing, GetType(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", New LocalizableResourceString(""AnalyzerTitle2"", Nothing, GetType(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Title As New LocalizableResourceString(""AnalyzerTitle3"", Nothing, GetType(Resources))
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", Title, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", New LocalizableResourceString(""AnalyzerTitle4"", Nothing, GetType(Resources)), ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(1),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(2),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticTitleCorrectlyRule).WithLocation(3),
    });
        }

        #endregion // RS1031 (DefineDiagnosticTitleCorrectlyRule)

        #region RS1032 (DefineDiagnosticMessageCorrectlyRule)

        [WindowsOnlyFact, WorkItem(3576, "https://github.com/dotnet/roslyn-analyzers/issues/3576")]
        public async Task RS1032_MessageStringEndsWithPeriodAndIsNotMultiSentence_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:""MyDiagnosticMessage.""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#3:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Message = {|#4:""MyDiagnostic.Message.""|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Message = ""MyDiagnostic.Message"";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(3).WithLocation(4));

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:""MyDiagnosticMessage.""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#3:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Message As String = {|#4:""MyDiagnostic.Message.""|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Const Message As String = ""MyDiagnostic.Message""

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(3).WithLocation(4));
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1032_MessageStringEndsWithPeriodAndIsNotMultiSentence_ResxFile_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerMessage1"" xml:space=""preserve"">
    <value>MyDiagnosticMessage.</value>
  </data>
  <data name=""AnalyzerMessage2"" xml:space=""preserve"">
    <value>MyDiagnostic.Message.</value>
    <comment>Optional comment.</comment>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerMessage1"" xml:space=""preserve"">
    <value>MyDiagnosticMessage</value>
  </data>
  <data name=""AnalyzerMessage2"" xml:space=""preserve"">
    <value>MyDiagnostic.Message</value>
    <comment>Optional comment.</comment>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#0:new LocalizableResourceString(""AnalyzerMessage1"", null, typeof(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableString Message = new LocalizableResourceString(""AnalyzerMessage2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", new LocalizableResourceString(""AnalyzerMessage1"", null, typeof(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableString Message = new LocalizableResourceString(""AnalyzerMessage2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1)
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#0:New LocalizableResourceString(""AnalyzerMessage1"", Nothing, GetType(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Message As LocalizableString = New LocalizableResourceString(""AnalyzerMessage2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", New LocalizableResourceString(""AnalyzerMessage1"", Nothing, GetType(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Message As LocalizableString = New LocalizableResourceString(""AnalyzerMessage2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1)
    });
        }

        [WindowsOnlyFact, WorkItem(3576, "https://github.com/dotnet/roslyn-analyzers/issues/3576")]
        public async Task RS1032_MessageStringIsMultiSentenceAndDoesNotEndWithPeriod_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:""Message is. Multi-sentence""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message is. Multi-sentence."", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1).WithLocation(1));

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:""Message is. Multi-sentence""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message is. Multi-sentence."", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1).WithLocation(1));
        }

        [WindowsOnlyFact, WorkItem(3576, "https://github.com/dotnet/roslyn-analyzers/issues/3576")]
        public async Task RS1032_MessageStringContainsLineReturn_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:""MyDiagnostic\rMessage""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#3:""MyDiagnostic\nMessage""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#4:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#5:""MyDiagnostic\r\nMessage""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnostic. Message."", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnostic. Message."", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#4:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnostic. Message."", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(3).WithLocation(3),
                GetRS1007ExpectedDiagnostic(4),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(5).WithLocation(5));

            // NOTE: Code fix does not handle binary operations.
            var vbCode = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualBasic

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:""MyDiagnostic"" & vbCr & ""Message""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#3:""MyDiagnostic"" & vbLf & ""Message""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = {|#4:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#5:""MyDiagnostic"" & vbCrLf & ""Message""|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
";
            await VerifyBasicCodeFixAsync(vbCode, vbCode,
                GetRS1007ExpectedDiagnostic(0),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(3).WithLocation(3),
                GetRS1007ExpectedDiagnostic(4),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(5).WithLocation(5));
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1032_MessageStringContainsLineReturn_ResxFile_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerMessage1"" xml:space=""preserve"">
    <value>MyDiagnostic
Message1.</value>
  </data>
  <data name=""AnalyzerMessage2"" xml:space=""preserve"">
    <value>MyDiagnostic.
Message2</value>
    <comment>Optional comment.</comment>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerMessage1"" xml:space=""preserve"">
    <value>MyDiagnostic. Message1.</value>
  </data>
  <data name=""AnalyzerMessage2"" xml:space=""preserve"">
    <value>MyDiagnostic. Message2.</value>
    <comment>Optional comment.</comment>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#0:new LocalizableResourceString(""AnalyzerMessage1"", null, typeof(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableString Message = new LocalizableResourceString(""AnalyzerMessage2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", new LocalizableResourceString(""AnalyzerMessage1"", null, typeof(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableString Message = new LocalizableResourceString(""AnalyzerMessage2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1)
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#0:New LocalizableResourceString(""AnalyzerMessage1"", Nothing, GetType(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Message As LocalizableString = New LocalizableResourceString(""AnalyzerMessage2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", New LocalizableResourceString(""AnalyzerMessage1"", Nothing, GetType(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Message As LocalizableString = New LocalizableResourceString(""AnalyzerMessage2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1)
    });
        }

        [WindowsOnlyFact, WorkItem(3576, "https://github.com/dotnet/roslyn-analyzers/issues/3576")]
        public async Task RS1032_ValidMessageString_NoDiagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message is a. Multi-sentence."", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message can contain A.B qualifications"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor4 =
        {|#3:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message can contain 'A.B' qualifications"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1007ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2),
                GetRS1007ExpectedDiagnostic(3));

            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message is a. Multi-sentence."", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message can contain A.B qualifications"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = {|#3:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""Message can contain 'A.B' qualifications"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1007ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2),
                GetRS1007ExpectedDiagnostic(3));
        }

        [WindowsOnlyFact, WorkItem(3958, "https://github.com/dotnet/roslyn-analyzers/issues/3958")]
        public async Task RS1032_LeadingOrTrailingWhitespaces_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerMessage1"" xml:space=""preserve"">
    <value>Message with trailing whitespace </value>
  </data>
  <data name=""AnalyzerMessage2"" xml:space=""preserve"">
    <value> Message with leading whitespace</value>
    <comment>Optional comment.</comment>
  </data>
  <data name=""AnalyzerMessage3"" xml:space=""preserve"">
    <value>  " + "\t" + @"    Message with leading and trailing spaces/tabs  " + "\t" + @"    </value>
  </data>
  <data name=""AnalyzerMessage4"" xml:space=""preserve"">
    <value>Message with period and trailing whitespace. </value>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerMessage1"" xml:space=""preserve"">
    <value>Message with trailing whitespace</value>
  </data>
  <data name=""AnalyzerMessage2"" xml:space=""preserve"">
    <value>Message with leading whitespace</value>
    <comment>Optional comment.</comment>
  </data>
  <data name=""AnalyzerMessage3"" xml:space=""preserve"">
    <value>Message with leading and trailing spaces/tabs</value>
  </data>
  <data name=""AnalyzerMessage4"" xml:space=""preserve"">
    <value>Message with period and trailing whitespace</value>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#0:new LocalizableResourceString(""AnalyzerMessage1"", null, typeof(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly LocalizableString Message = new LocalizableResourceString(""AnalyzerMessage2"", null, typeof(Resources));
    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#2:new LocalizableResourceString(""AnalyzerMessage3"", null, typeof(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#3:new LocalizableResourceString(""AnalyzerMessage4"", null, typeof(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", new LocalizableResourceString(""AnalyzerMessage1"", null, typeof(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly LocalizableString Message = new LocalizableResourceString(""AnalyzerMessage2"", null, typeof(Resources));
    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", new LocalizableResourceString(""AnalyzerMessage3"", null, typeof(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", new LocalizableResourceString(""AnalyzerMessage4"", null, typeof(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(2),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(3),
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#0:New LocalizableResourceString(""AnalyzerMessage1"", Nothing, GetType(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Message As LocalizableString = New LocalizableResourceString(""AnalyzerMessage2"", Nothing, GetType(Resources))
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#1:Message|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#2:New LocalizableResourceString(""AnalyzerMessage3"", Nothing, GetType(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", {|#3:New LocalizableResourceString(""AnalyzerMessage4"", Nothing, GetType(Resources))|}, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", New LocalizableResourceString(""AnalyzerMessage1"", Nothing, GetType(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Message As LocalizableString = New LocalizableResourceString(""AnalyzerMessage2"", Nothing, GetType(Resources))
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", Message, ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", New LocalizableResourceString(""AnalyzerMessage3"", Nothing, GetType(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", New LocalizableResourceString(""AnalyzerMessage4"", Nothing, GetType(Resources)), ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(1),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(2),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticMessageCorrectlyRule).WithLocation(3),
    });
        }

        #endregion // RS1032 (DefineDiagnosticMessageCorrectlyRule)

        #region RS1033 (DefineDiagnosticDescriptionCorrectlyRule)

        [WindowsOnlyFact, WorkItem(3577, "https://github.com/dotnet/roslyn-analyzers/issues/3577")]
        public async Task RS1033_DescriptionStringDoesNotEndWithPunctuation_Diagnostic()
        {
            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#1:description: {|#2:""MyDiagnosticDescription""|}|}, helpLinkUri: ""HelpLink"", customTags: """")|};
    private static readonly DiagnosticDescriptor descriptor2 =
        {|#3:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#4:description: Description|}, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Description = {|#5:""MyDiagnosticDescription""|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}", @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: ""MyDiagnosticDescription."", helpLinkUri: ""HelpLink"", customTags: """")|};
    private static readonly DiagnosticDescriptor descriptor2 =
        {|#3:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: ""HelpLink"", customTags: """")|};
    private const string Description = ""MyDiagnosticDescription."";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(1).WithLocation(2),
                GetRS1007ExpectedDiagnostic(3),
                VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(4).WithLocation(5));

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#1:""Description""|}, ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#3:Description|}, ""HelpLinkUrl"", ""Tag"")|}
    Private Const Description As String = {|#4:""Description""|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, Description, ""HelpLinkUrl"", ""Tag"")|}
    Private Const Description As String = ""Description.""

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(1).WithLocation(1),
                GetRS1007ExpectedDiagnostic(2),
                VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(3).WithLocation(4));
        }

        [WindowsOnlyFact, WorkItem(3575, "https://github.com/dotnet/roslyn-analyzers/issues/3575")]
        public async Task RS1033_DescriptionStringDoesNotEndWithPunctuation_ResxFile_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerDescription1"" xml:space=""preserve"">
    <value>MyDiagnosticDescription1</value>
  </data>
  <data name=""AnalyzerDescription2"" xml:space=""preserve"">
    <value>MyDiagnostic. Description2</value>
    <comment>Optional comment.</comment>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerDescription1"" xml:space=""preserve"">
    <value>MyDiagnosticDescription1.</value>
  </data>
  <data name=""AnalyzerDescription2"" xml:space=""preserve"">
    <value>MyDiagnostic. Description2.</value>
    <comment>Optional comment.</comment>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#0:description: new LocalizableResourceString(""AnalyzerDescription1"", null, typeof(Resources))|}, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#1:description: Description|}, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Description = new LocalizableResourceString(""AnalyzerDescription2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: new LocalizableResourceString(""AnalyzerDescription1"", null, typeof(Resources)), helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: ""HelpLink"", customTags: """");
    private static readonly LocalizableResourceString Description = new LocalizableResourceString(""AnalyzerDescription2"", null, typeof(Resources));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
    expected: new[] {
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(0),
        VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(1)
    });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#0:New LocalizableResourceString(""AnalyzerDescription1"", Nothing, GetType(Resources))|}, ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#1:Description|}, ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Description As LocalizableString = New LocalizableResourceString(""AnalyzerDescription2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, New LocalizableResourceString(""AnalyzerDescription1"", Nothing, GetType(Resources)), ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, Description, ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Description As LocalizableString = New LocalizableResourceString(""AnalyzerDescription2"", Nothing, GetType(Resources))

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(1)
    });
        }

        [WindowsOnlyFact, WorkItem(3577, "https://github.com/dotnet/roslyn-analyzers/issues/3577")]
        public async Task RS1033_DescriptionEndsWithPunctuation_NoDiagnostic()
        {
            await VerifyCSharpAnalyzerAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: ""MyDiagnosticDescription."", helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor2 =
        {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: ""MyDiagnosticDescription!"", helpLinkUri: ""HelpLink"", customTags: """")|};

    private static readonly DiagnosticDescriptor descriptor3 =
        {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: ""MyDiagnosticDescription?"", helpLinkUri: ""HelpLink"", customTags: """")|};

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1007ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2));

            await VerifyBasicAnalyzerAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualBasic

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = {|#0:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"",
        DiagnosticSeverity.Warning, True, ""Description."", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"",
        DiagnosticSeverity.Warning, True, ""Description!"", ""HelpLinkUrl"", ""Tag"")|}

    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = {|#2:new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"",
        DiagnosticSeverity.Warning, True, ""Description?"", ""HelpLinkUrl"", ""Tag"")|}

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
                GetRS1007ExpectedDiagnostic(0),
                GetRS1007ExpectedDiagnostic(1),
                GetRS1007ExpectedDiagnostic(2));
        }

        [WindowsOnlyFact, WorkItem(3958, "https://github.com/dotnet/roslyn-analyzers/issues/3958")]
        public async Task RS1033_LeadingOrTrailingWhitespaces_Diagnostic()
        {
            var additionalFileName = "Resources.resx";
            var additionalFileText = @"
<root>
  <data name=""AnalyzerDescription1"" xml:space=""preserve"">
    <value>Description with trailing space </value>
  </data>
  <data name=""AnalyzerDescription2"" xml:space=""preserve"">
    <value> Description with leading space</value>
  </data>
  <data name=""AnalyzerDescription3"" xml:space=""preserve"">
    <value>  " + "\t" + @"    Description with leading and trailing spaces/tabs  " + "\t" + @"    </value>
  </data>
  <data name=""AnalyzerDescription4"" xml:space=""preserve"">
    <value>Description with trailing space. </value>
  </data>
</root>";
            var fixedAdditionalFileText = @"
<root>
  <data name=""AnalyzerDescription1"" xml:space=""preserve"">
    <value>Description with trailing space.</value>
  </data>
  <data name=""AnalyzerDescription2"" xml:space=""preserve"">
    <value>Description with leading space.</value>
  </data>
  <data name=""AnalyzerDescription3"" xml:space=""preserve"">
    <value>Description with leading and trailing spaces/tabs.</value>
  </data>
  <data name=""AnalyzerDescription4"" xml:space=""preserve"">
    <value>Description with trailing space.</value>
  </data>
</root>";

            await VerifyCSharpCodeFixAsync(@"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#0:description: new LocalizableResourceString(""AnalyzerDescription1"", null, typeof(Resources))|}, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly LocalizableResourceString Description = new LocalizableResourceString(""AnalyzerDescription2"", null, typeof(Resources));
    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#1:description: Description|}, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#2:description: new LocalizableResourceString(""AnalyzerDescription3"", null, typeof(Resources))|}, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, {|#3:description: new LocalizableResourceString(""AnalyzerDescription4"", null, typeof(Resources))|}, helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                additionalFileName: additionalFileName,
                additionalFileText: additionalFileText,
                fixedAdditionalFileText: fixedAdditionalFileText,
                fixedSource: @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

class Resources { }

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor1 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: new LocalizableResourceString(""AnalyzerDescription1"", null, typeof(Resources)), helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly LocalizableResourceString Description = new LocalizableResourceString(""AnalyzerDescription2"", null, typeof(Resources));
    private static readonly DiagnosticDescriptor descriptor2 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor3 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: new LocalizableResourceString(""AnalyzerDescription3"", null, typeof(Resources)), helpLinkUri: ""HelpLink"", customTags: """");

    private static readonly DiagnosticDescriptor descriptor4 =
        new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: new LocalizableResourceString(""AnalyzerDescription4"", null, typeof(Resources)), helpLinkUri: ""HelpLink"", customTags: """");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}",
                expected: new[] {
                    VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(0),
                    VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(1),
                    VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(2),
                    VerifyCS.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(3),
                });

            await VerifyBasicCodeFixAsync(@"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#0:New LocalizableResourceString(""AnalyzerDescription1"", Nothing, GetType(Resources))|}, ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Description As LocalizableString = New LocalizableResourceString(""AnalyzerDescription2"", Nothing, GetType(Resources))
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#1:Description|}, ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#2:New LocalizableResourceString(""AnalyzerDescription3"", Nothing, GetType(Resources))|}, ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, {|#3:New LocalizableResourceString(""AnalyzerDescription4"", Nothing, GetType(Resources))|}, ""HelpLinkUrl"", ""Tag"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
",
    additionalFileName: additionalFileName,
    additionalFileText: additionalFileText,
    fixedAdditionalFileText: fixedAdditionalFileText,
    fixedSource: @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Class Resources
End Class

<DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer

    Private Shared ReadOnly descriptor1 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, New LocalizableResourceString(""AnalyzerDescription1"", Nothing, GetType(Resources)), ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly Description As LocalizableString = New LocalizableResourceString(""AnalyzerDescription2"", Nothing, GetType(Resources))
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, Description, ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor3 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, New LocalizableResourceString(""AnalyzerDescription3"", Nothing, GetType(Resources)), ""HelpLinkUrl"", ""Tag"")
    Private Shared ReadOnly descriptor4 As DiagnosticDescriptor = new DiagnosticDescriptor(""MyDiagnosticId"", ""MyDiagnosticTitle"", ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, New LocalizableResourceString(""AnalyzerDescription4"", Nothing, GetType(Resources)), ""HelpLinkUrl"", ""Tag"")

	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(descriptor1, descriptor2, descriptor3, descriptor4)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
", expected: new[] {
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(0),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(1),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(2),
        VerifyVB.Diagnostic(DiagnosticDescriptorCreationAnalyzer.DefineDiagnosticDescriptionCorrectlyRule).WithLocation(3),
    });
        }

        #endregion // RS1033 (DefineDiagnosticDescriptionCorrectlyRule)

        #region Helpers

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.UseLocalizableStringsInDescriptorRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1007ExpectedDiagnostic(int markupKey) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.UseLocalizableStringsInDescriptorRule)
                .WithLocation(markupKey)
                .WithArguments(WellKnownTypeNames.MicrosoftCodeAnalysisLocalizableString);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.ProvideHelpUriInDescriptorRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1015ExpectedDiagnostic(int markupKey) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.ProvideHelpUriInDescriptorRule)
                .WithLocation(markupKey);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.DiagnosticIdMustBeAConstantRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1017ExpectedDiagnostic(int markupKey, string descriptorName) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.DiagnosticIdMustBeAConstantRule)
                .WithLocation(markupKey)
                .WithArguments(descriptorName);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.DiagnosticIdMustBeInSpecifiedFormatRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1018ExpectedDiagnostic(int markupKey, string diagnosticId, string category, string format, string additionalFile) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.DiagnosticIdMustBeInSpecifiedFormatRule)
                .WithLocation(markupKey)
                .WithArguments(diagnosticId, category, format, additionalFile);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.UseUniqueDiagnosticIdRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1019ExpectedDiagnostic(int markupKey, string duplicateId, string otherAnalyzerName) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.UseUniqueDiagnosticIdRule)
                .WithLocation(markupKey)
                .WithArguments(duplicateId, otherAnalyzerName);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.UseCategoriesFromSpecifiedRangeRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1020ExpectedDiagnostic(int markupKey, string category, string additionalFile) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.UseCategoriesFromSpecifiedRangeRule)
                .WithLocation(markupKey)
                .WithArguments(category, additionalFile);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.ProvideCustomTagsInDescriptorRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1028ResultAt(int markupKey) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.ProvideCustomTagsInDescriptorRule)
                .WithLocation(markupKey);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.AnalyzerCategoryAndIdRangeFileInvalidRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1021ExpectedDiagnostic(int markupKey, string invalidEntry, string additionalFile) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.AnalyzerCategoryAndIdRangeFileInvalidRule)
                .WithLocation(markupKey)
                .WithArguments(invalidEntry, additionalFile);

        /// <summary>
        /// Creates an expected diagnostic for <inheritdoc cref="DiagnosticDescriptorCreationAnalyzer.DoNotUseReservedDiagnosticIdRule"/>
        /// </summary>
        private static DiagnosticResult GetRS1029ResultAt(int markupKey, string ruleId) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.DoNotUseReservedDiagnosticIdRule)
                .WithLocation(markupKey)
                .WithArguments(ruleId);

        private static async Task VerifyCSharpAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                ReferenceAssemblies = AdditionalMetadataReferences.Default,
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck
            };

            test.SolutionTransforms.Add(WithoutEnableReleaseTrackingWarning);
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        private static async Task VerifyBasicAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new VerifyVB.Test
            {
                TestCode = source,
                ReferenceAssemblies = AdditionalMetadataReferences.Default,
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck
            };

            test.SolutionTransforms.Add(WithoutEnableReleaseTrackingWarning);
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        private static async Task VerifyCSharpCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                FixedCode = fixedSource,
                ReferenceAssemblies = AdditionalMetadataReferences.Default,
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck
            };

            test.SolutionTransforms.Add(WithoutEnableReleaseTrackingWarning);
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        private static async Task VerifyBasicCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
        {
            var test = new VerifyVB.Test
            {
                TestCode = source,
                FixedCode = fixedSource,
                ReferenceAssemblies = AdditionalMetadataReferences.Default,
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck
            };

            test.SolutionTransforms.Add(WithoutEnableReleaseTrackingWarning);
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        private static async Task VerifyCSharpCodeFixAsync(string source, string additionalFileName, string additionalFileText, string fixedSource, string fixedAdditionalFileText, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (additionalFileName, additionalFileText), },
                },
                FixedState =
                {
                    Sources = { fixedSource },
                    AdditionalFiles = { (additionalFileName, fixedAdditionalFileText), },
                },
                ReferenceAssemblies = AdditionalMetadataReferences.Default,
            };

            test.SolutionTransforms.Add(WithoutEnableReleaseTrackingWarning);
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        private static async Task VerifyBasicCodeFixAsync(string source, string additionalFileName, string additionalFileText, string fixedSource, string fixedAdditionalFileText, params DiagnosticResult[] expected)
        {
            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (additionalFileName, additionalFileText), },
                },
                FixedState =
                {
                    Sources = { fixedSource },
                    AdditionalFiles = { (additionalFileName, fixedAdditionalFileText), },
                },
                ReferenceAssemblies = AdditionalMetadataReferences.Default,
            };

            test.SolutionTransforms.Add(WithoutEnableReleaseTrackingWarning);
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        private static readonly ImmutableDictionary<string, ReportDiagnostic> s_enableReleaseTrackingWarningDisabled = ImmutableDictionary<string, ReportDiagnostic>.Empty
            .Add(DiagnosticDescriptorCreationAnalyzer.EnableAnalyzerReleaseTrackingRule.Id, ReportDiagnostic.Suppress);

        private static Solution WithoutEnableReleaseTrackingWarning(Solution solution, ProjectId projectId)
        {
            var compilationOptions = solution.GetProject(projectId)!.CompilationOptions!;
            compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItems(s_enableReleaseTrackingWarningDisabled));
            return solution.WithProjectCompilationOptions(projectId, compilationOptions);
        }

        private const string AdditionalFileName = "DiagnosticCategoryAndIdRanges.txt";

        private const string CSharpDiagnosticDescriptorCreationHelper = @"
internal static class DiagnosticDescriptorHelper
{
    // Dummy DiagnosticDescriptor creation helper.
    public static DiagnosticDescriptor Create(
        string id,
        LocalizableString title,
        LocalizableString messageFormat,
        string category)
    => null;
}";
        private const string VisualBasicDiagnosticDescriptorCreationHelper = @"
Friend Partial Module DiagnosticDescriptorHelper
    ' Dummy DiagnosticDescriptor creation helper.
    Function Create(id As String, title As LocalizableString, messageFormat As LocalizableString, category As String) As DiagnosticDescriptor
        Return Nothing
    End Function
End Module";

        #endregion
    }
}
