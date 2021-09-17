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
        public async Task TestUnmarkedPreviewInterface()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program : IProgram
            {
                static void Main(string[] args)
                {
                    new Program();
                }

                public void {|#0:MarkedMethodInInterface|}()
                {
                    throw new NotImplementedException();
                }        
            }

            public interface IProgram
            {
                [RequiresPreviewFeatures]
                void MarkedMethodInInterface();
            }
        }

            ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewMethodRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_implementsPreviewMethodMessage, "MarkedMethodInInterface", "IProgram.MarkedMethodInInterface"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestMarkedPreviewInterface()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program : {|#1:IProgram|}
            {
                static void Main(string[] args)
                {
                    new Program();
                }

                public void {|#0:UnmarkedMethodInMarkedInterface|}() { }

            }

            [RequiresPreviewFeatures]
            public interface IProgram
            {
                public void UnmarkedMethodInMarkedInterface() { }
            }
        }

            ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewMethodRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_implementsPreviewMethodMessage, "UnmarkedMethodInMarkedInterface", "IProgram.UnmarkedMethodInMarkedInterface"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewInterfaceRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_implementsPreviewInterfaceMessage, "Program", "IProgram"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestMarkedEmptyPreviewInterface()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program : {|#0:IProgram|}
            {
                static void Main(string[] args)
                {
                    new Program();
                }
            }

            [RequiresPreviewFeatures]
            public interface IProgram
            {
            }
        }

            ";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewInterfaceRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_implementsPreviewInterfaceMessage, "Program", "IProgram"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestDerivedInterface()
        {
            var csInput = @" 
using System.Runtime.Versioning; using System;
namespace Preview_Feature_Scratch
{

    interface {|#0:IZoo|} : IFoo
    {
    }

    [RequiresPreviewFeatures]
    interface IFoo
    {
        void Bar();
    }
}";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.ImplementsPreviewInterfaceRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_implementsPreviewInterfaceMessage, "IZoo", "IFoo"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }
    }
}
