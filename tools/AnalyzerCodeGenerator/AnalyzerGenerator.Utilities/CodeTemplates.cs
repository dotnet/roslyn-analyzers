// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzerCodeGenerator
{
    public static class CodeTemplates
    {
        // 0: REPLACE.ME
        // 1: REPLACEME
        // 2: Name
        // 3: ID
        // 4: Category
        // 5: list of message strings
        // 6: list of descriptors
        // 7: list of descriptor names
        // 8: Title
        // FileName: Name.cs
        private const string _analyzerTemplate =
@"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace {0}.Analyzers
{{                   
    /// <summary>
    /// {3}: {8}
    /// </summary>
    public abstract class {2}Analyzer : DiagnosticAnalyzer
    {{
        internal const string RuleId = ""{3}"";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof({1}AnalyzersResources.{2}Title), {1}AnalyzersResources.ResourceManager, typeof({1}AnalyzersResources));
        {5}
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof({1}AnalyzersResources.{2}Description), {1}AnalyzersResources.ResourceManager, typeof({1}AnalyzersResources));
        {6}

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create({7});

        public override void Initialize(AnalysisContext analysisContext)
        {{
            
        }}
    }}
}}";

        // 0: MessageName
        // 1: REPLACEME
        // 2: Name
        private const string _messageTemplate =
@"
        private static readonly LocalizableString s_localizableMessage{0} = new LocalizableResourceString(nameof({1}AnalyzersResources.{2}Message{0}), {1}AnalyzersResources.ResourceManager, typeof({1}AnalyzersResources));";

        // 0: MessageName
        // 1: Category
        private const string _descriptorTemplate =
@"
        internal static DiagnosticDescriptor {0}Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage{0},
                                                                             DiagnosticCategory.{1},
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);";

        // 0: REPLACE.ME
        // 1: Name
        // 2: ID
        // 3: Title
        // Filename: CSharpName.cs
        private const string _csharpAnalyzerTemplate =
@"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace {0}.Analyzers
{{                          
    /// <summary>
    /// {2}: {3}
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharp{1}Analyzer : {1}Analyzer
    {{
        
    }}
}}";

        // 0: REPLACE.ME
        // 1: Name
        // 2: Id
        // 3: Title
        // Filename: BasicName.vb
        private const string _basicAnalyzerTemplate =
@"' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace {0}.Analyzers   
    ''' <summary>
    ''' {2}: {3}
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class Basic{1}Analyzer
        Inherits {1}Analyzer

    End Class
End Namespace";

        // 0: REPLACE.ME 
        // 1: Name
        // FileName: NameTests.cs 
        private const string _analyzerTestTemplate =
@"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace {0}.Analyzers.UnitTests
{{
    public class {1}Tests : DiagnosticAnalyzerTestBase
    {{
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {{
            return new Basic{1}Analyzer();
        }}

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {{
            return new CSharp{1}Analyzer();
        }}
    }}
}}";

        // 0: REPLACE.ME
        // 1: Name  
        // 2: id
        // 3: Title
        // FileName: Name.Fixer.cs
        private const string _codeFixTemplate =
@"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;     
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace {0}.Analyzers
{{                              
    /// <summary>
    /// {2}: {3}
    /// </summary>
    public abstract class {1}Fixer : CodeFixProvider
    {{
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create({1}Analyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {{
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }}

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {{                              
            // This is to get rid of warning CS1998, please remove when implementing this analyzer
            await new Task(() => {{ }});
            throw new NotImplementedException();
        }}
    }}
}}";

        // 0: REPLACE.ME
        // 1: Name   
        // 2: id
        // 3: Title
        // Filename: CSharpName.cs
        private const string _csharpCodeFixTemplate =
@"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
                                 
using System.Composition;
using System.Diagnostics;  
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace {0}.Analyzers
{{                                 
    /// <summary>
    /// {2}: {3}
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharp{1}Fixer : {1}Fixer
    {{ 
        
    }}
}}";

        // 0: REPLACE.ME
        // 1: Name
        // 2: id
        // 3: Title
        // Filename: BasicName.vb
        private const string _basicCodeFixTemplate =
@"' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace {0}.Analyzers     
    ''' <summary>
    ''' {2}: {3}
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class Basic{1}Fixer
        Inherits {1}Fixer 

    End Class
End Namespace
";

        // 0: REPLACE.ME 
        // 1: Name
        // FileName: NameTests.Fixer.cs 
        private const string _codeFixTestTemplate =
@"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace {0}.Analyzers.UnitTests
{{
    public class {1}FixerTests : CodeFixTestBase
    {{
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {{
            return new Basic{1}Analyzer();
        }}

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {{
            return new CSharp{1}Analyzer();
        }}

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {{
            return new Basic{1}Fixer();
        }}

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {{
            return new CSharp{1}Fixer();
        }}
    }}
}}";

        // 0: REPLACE.ME
        // 1: concatenation of a list of CategoryFieldTemplate.
        // FileName: DiagnosticCategory.cs  
        private const string _categotyTemplate =
@"// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace {0}.Analyzers
{{
    internal static class DiagnosticCategory
    {{{1}}}
}}";

        // 0: REPLACEME
        // 1: ActualCategoryName
        // This is used as insertions to CategotyTemplate 
        private const string _categoryFieldTemplate =
@"        public static readonly string {1} = {0}AnalyzersResources.Category{1};";

        // 0: Name
        // 1: Title
        // 2: Description
        private const string  _diagnosticResourceDataTemplate =
@"
  <data name=""{0}Title"" xml:space=""preserve"">
    <value>{1}</value>
  </data>
  <data name=""{0}Description"" xml:space=""preserve"">
    <value>{2}</value>
  </data>";

        // 0: Name
        // 1: MessageName
        // 2: Message
        private const string _diagnosticResourceDataMessageTemplate =
@"
  <data name=""{0}Message{1}"" xml:space=""preserve"">
    <value>{2}</value>
  </data>";

        // 0: CategoryName            
        private const string _categoryResourceDataTemplate =
@"
  <data name=""Category{0}"" xml:space=""preserve"">
    <value>{0}</value>
  </data>";

        // FileName
        private const string _compileItemTemplate = 
@"    <Compile Include=""{0}"" />
";

        // REPLACEME
        private const string _nugetProjectItemTemplate =
@"    <Project Include = ""..\{0}.Analyzers\NuGet\{0}.Analyzers.NuGet.proj"" />";

        //REPLACE.ME
        private const string _unitTestAssemblyItemTemplate =
@"      <TestAssemblies Include=""Binaries\$(Configuration)\{0}.Analyzers.UnitTests.dll"" />";

        public static string GenerateAnalyzer(string analyzer, CheckData check)
        {
            var messageBuilder  = new StringBuilder();
            var descriptorBuilder = new StringBuilder();
            var descriptorNamesBuilder = new StringBuilder();
            var replaceme = analyzer.Replace(".", String.Empty);

            if (check.Messages != null && check.Messages.Count > 1)
            {
                int i = 0;
                foreach (var pair in check.Messages)
                {
                    // 0: MessageName
                    // 1: REPLACEME
                    // 2: Name
                    messageBuilder.AppendFormat(_messageTemplate, pair.Key, replaceme, check.Name);
                    // 0: MessageName
                    // 1: Category
                    descriptorBuilder.AppendFormat(_descriptorTemplate, pair.Key, check.Category);
                    descriptorNamesBuilder.Append(pair.Key + "Rule" + (i < check.Messages.Count - 1 ? ", " : ""));
                    ++i;
                }
            }
            else
            {
                messageBuilder.AppendFormat(_messageTemplate, "", replaceme, check.Name);
                descriptorBuilder.AppendFormat(_descriptorTemplate, "", check.Category);
                descriptorNamesBuilder.Append("Rule");
            }
            // 0: REPLACE.ME
            // 1: REPLACEME
            // 2: Name
            // 3: ID
            // 4: Category
            // 5: list of message strings
            // 6: list of descriptors
            // 7: list of descritor names
            // 8: Title
            return string.Format(_analyzerTemplate, 
                                 analyzer,
                                 replaceme,
                                 check.Name,
                                 check.Id,
                                 check.Category,
                                 messageBuilder.ToString(),
                                 descriptorBuilder.ToString(),
                                 descriptorNamesBuilder.ToString(),
                                 check.Title);
        }

        public static string GenerateAnalyzerFileName(CheckData check)
        {
            return check.Name + ".cs";
        }

        public static string GenerateCSharpAnalyzer(string analyzer, CheckData check)
        {
            return string.Format(_csharpAnalyzerTemplate, analyzer, check.Name, check.Id, check.Title);
        }

        public static string GenerateCSharpAnalyzerFileName(CheckData check)
        {
            return "CSharp" + check.Name + ".cs";
        }

        public static string GenerateBasicAnalyzer(string analyzer, CheckData check)
        {
            return string.Format(_basicAnalyzerTemplate, analyzer, check.Name, check.Id, check.Title);
        }

        public static string GenerateBasicAnalyzerFileName(CheckData check)
        {
            return "Basic" + check.Name + ".vb";
        }

        public static string GenerateAnalyzerTests(string analyer, CheckData check)
        {
            return string.Format(_analyzerTestTemplate, analyer, check.Name);
        }

        public static string GenerateAnalyzerTestsFileName(CheckData check)
        {
            return check.Name + "Tests.cs";
        }

        public static string GenerateCodeFix(string analyzer, CheckData check)
        {
            return string.Format(_codeFixTemplate, analyzer, check.Name, check.Id, check.Title);
        }

        public static string GenerateCodeFixFileName(CheckData check)
        {
            return check.Name + ".Fixer.cs";
        }

        public static string GenerateCSharpCodeFix(string analyzer, CheckData check)
        {
            return string.Format(_csharpCodeFixTemplate, analyzer, check.Name, check.Id, check.Title);
        }

        public static string GenerateCSharpCodeFixFileName(CheckData check)
        {
            return "CSharp" + check.Name + ".Fixer.cs";
        }

        public static string GenerateBasicCodeFix(string analyzer, CheckData check)
        {
            return string.Format(_basicCodeFixTemplate, analyzer, check.Name, check.Id, check.Title);
        }

        public static string GenerateBasicCodeFixFileName(CheckData check)
        {
            return "Basic" + check.Name + ".Fixer.vb";
        }

        public static string GenerateCodeFixTests(string analyzer, CheckData check)
        {
            return string.Format(_codeFixTestTemplate, analyzer, check.Name);
        }

        public static string GenerateCodeFixTestsFileName(CheckData check)
        {
            return check.Name + "Tests.Fixer.cs";
        }

        public static string GenerateCategory(string analyzer, IEnumerable<string> categories)
        {
            var sb = new StringBuilder();
            var analyzerCondensed = analyzer.Replace(".", String.Empty);
            foreach (var category in categories)
            {
                sb.AppendFormat(_categoryFieldTemplate, analyzerCondensed, category);
            }
            return string.Format(_categotyTemplate, analyzer, sb.ToString());
        }

        public const string CategoryFileName = "DiagnosticCategory.cs";

        public static string GenerateDiagnosticResourceData(CheckData check)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(_diagnosticResourceDataTemplate,
                            check.Name,
                            check.Title,
                            check.Description);
            if (check.Messages != null && check.Messages.Count > 1)
            {   
                foreach (var pair in check.Messages)
                {
                    sb.AppendFormat(_diagnosticResourceDataMessageTemplate,
                                    check.Name,
                                    pair.Key,
                                    pair.Value);
                }
            }
            else
            {
                // use title as message
                sb.AppendFormat(_diagnosticResourceDataMessageTemplate,
                                check.Name,
                                "",
                                check.Title);
            }
            return sb.ToString();
        }

        public static string GenerateCategoriesResourceData(IEnumerable<string> categories)
        {
            var sb = new StringBuilder();
            foreach (var category in categories)
            {
                sb.AppendFormat(_categoryResourceDataTemplate, category);
            }
            return sb.ToString();
        }

        public static string GenerateCompileItem(string sourceFile)
        {
            return string.Format(_compileItemTemplate, sourceFile);
        }

        public static string GenerateNuGetProjectItem(string analyzer)
        {
            return string.Format(_nugetProjectItemTemplate, analyzer);
        }

        public static string GenerateUnitTestAssemblyItem(string analyzer)
        {
            return string.Format(_unitTestAssemblyItemTemplate, analyzer);
        }
    }
}
