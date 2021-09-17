// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDetectPreviewFeatureAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public partial class DetectPreviewFeatureUnitTests
    {
        [Fact]
        public async Task TestDictionaryFieldWithPreviewType()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        using System.Collections.Generic;
        namespace Preview_Feature_Scratch
        {

            class Program
            {
                private Dictionary<int, {|#0:PreviewType|}> _genericPreviewFieldDictionary;
#nullable enable
                private Dictionary<int, {|#3:PreviewType|}?>? _genericPreviewFieldDictionaryWithNullable;
#nullable disable
                private List<List<List<List<{|#1:PreviewType|}>>>> _genericPreviewField;
                private List<List<AGenericClass<Int32>>> _genericClassField;
                private List<List<{|#2:AGenericPreviewClass<Int32>|}>> _genericPreviewClassField;


                public Program()
                {
                } 

                static void Main(string[] args)
                {
                }
            }

            [RequiresPreviewFeatures]
            class PreviewType
            {

            }

            class AGenericClass<T>
            {
            }

            [RequiresPreviewFeatures]
            class AGenericPreviewClass<T>
            {
            }

        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewFieldDictionary", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(3).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewFieldDictionaryWithNullable", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewField", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(2).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewClassField", "AGenericPreviewClass"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestPreviewTypeField()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program
            {
                private {|#0:PreviewType|} _field;
                private AGenericClass<{|#2:PreviewType|}> _genericPreviewField;
                private AGenericClass<bool> _noDiagnosticField;

                public Program()
                {
                    _field = {|#1:new PreviewType()|};
                } 

                static void Main(string[] args)
                {
                }
            }

            [RequiresPreviewFeatures]
            class PreviewType
            {

            }

            class AGenericClass<T>
            {

            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_field", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(2).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewField", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestArrayOfGenericPreviewFields()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            public class Program
            {
                private {|#0:PreviewType|} _field;
                private {|#4:PreviewType|}[] _fieldArray;
                private {|#5:PreviewType|}[][] _fieldArrayOfArray;
                private AGenericClass<{|#2:PreviewType|}>[] _genericPreviewField;

                public Program()
                {
                    _field = {|#1:new PreviewType()|};
                } 

                static void Main(string[] args)
                {
                }
            }

            [RequiresPreviewFeatures]
            public class PreviewType
            {

            }

            public class AGenericClass<T>
                where T : {|#3:PreviewType|}
            {

            }

#nullable enable
            public class AGenericClassWithNullable<T>
                where T : {|#6:PreviewType?|}
            {
                private {|#7:PreviewType|}? _fieldNullable;
                private {|#8:PreviewType|}[]? _fieldArrayNullable;
                private {|#9:PreviewType|}[][]? _fieldArrayOfArrayNullable;
            }
#nullable disable
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_field", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(2).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewField", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(3).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "AGenericClass", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(4).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_fieldArray", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(5).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_fieldArrayOfArray", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.UsesPreviewTypeParameterRule).WithLocation(6).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_usesPreviewTypeParameterMessage, "AGenericClassWithNullable", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(7).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_fieldNullable", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(8).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_fieldArrayNullable", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(9).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_fieldArrayOfArrayNullable", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestPreviewTypeArrayOfGenericPreviewFields()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            public class Program
            {
                private {|#0:PreviewType|} _field;
                private {|#3:AGenericClass<{|#2:PreviewType|}>|}[] _genericPreviewField;

                public Program()
                {
                    _field = {|#1:new PreviewType()|};
                } 

                static void Main(string[] args)
                {
                }
            }

            [RequiresPreviewFeatures]
            public class PreviewType
            {

            }

            [RequiresPreviewFeatures]
            public class AGenericClass<T>
                where T : PreviewType
            {

            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_field", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(2).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewField", "PreviewType"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(3).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "_genericPreviewField", "AGenericClass"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestField()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program
            {
                [RequiresPreviewFeatures]
                private bool _field;

                public Program()
                {
                    {|#0:_field|} = true;
                } 

                static void Main(string[] args)
                {
                }
            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "_field"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }
    }
}
