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
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.DiagnosticDescriptorCreationAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

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
        {|#1:new DiagnosticDescriptor(""MyDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, true, ""MyDiagnosticDescription"", ""HelpLink"")|};

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
    Private Shared ReadOnly descriptor2 As DiagnosticDescriptor = new {|#1:DiagnosticDescriptor|}(""MyDiagnosticId"", dummyLocalizableTitle, ""MyDiagnosticMessage"", ""MyDiagnosticCategory"", DiagnosticSeverity.Warning, True, ""MyDiagnosticDescription"", ""HelpLink"")
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
Category with spaces
Category with spaces and range: Prefix100-Prefix199

# Illegal: Multiple colons
CategoryMultipleColons: IdWithColon:100

# Illegal: Duplicate category
DuplicateCategory1
DuplicateCategory1
DuplicateCategory2: Prefix100-Prefix199
DuplicateCategory2: Prefix200-Prefix299

# Illegal: ID cannot be non-alphanumeric
CategoryWithBadId1: Prefix_100
CategoryWithBadId2: Prefix_100-Prefix_199

# Illegal: Id cannot have letters after number
CategoryWithBadId3: Prefix000NotAllowed
CategoryWithBadId4: Prefix000NotAllowed-Prefix099NotAllowed

# Illegal: Different prefixes in ID range
CategoryWithBadId5: Prefix000-DifferentPrefix099
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { (AdditionalFileName, additionalText) },
                    ExpectedDiagnostics =
                    {
                        GetRS1021ExpectedDiagnostic(6, 1, "Category with spaces", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(7, 1, "Category with spaces and range: Prefix100-Prefix199", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(10, 1, "CategoryMultipleColons: IdWithColon:100", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(14, 1, "DuplicateCategory1", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(16, 1, "DuplicateCategory2: Prefix200-Prefix299", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(19, 1, "CategoryWithBadId1: Prefix_100", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(20, 1, "CategoryWithBadId2: Prefix_100-Prefix_199", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(23, 1, "CategoryWithBadId3: Prefix000NotAllowed", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(24, 1, "CategoryWithBadId4: Prefix000NotAllowed-Prefix099NotAllowed", AdditionalFileName),
                        GetRS1021ExpectedDiagnostic(27, 1, "CategoryWithBadId5: Prefix000-DifferentPrefix099", AdditionalFileName),
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
        private static DiagnosticResult GetRS1021ExpectedDiagnostic(int line, int column, string invalidEntry, string additionalFile) =>
            new DiagnosticResult(DiagnosticDescriptorCreationAnalyzer.AnalyzerCategoryAndIdRangeFileInvalidRule)
                .WithLocation(AdditionalFileName, line, column)
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
                TestState =
                {
                    Sources = { source },
                },
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
                TestState =
                {
                    Sources = { source },
                },
                ReferenceAssemblies = AdditionalMetadataReferences.Default,
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck
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
