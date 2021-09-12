// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Usage.DoNotCallOrderByMultipleTimes,
    Microsoft.CodeQuality.CSharp.Analyzers.Usage.CSharpDoNotCallOrderByMultipleTimesFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Usage.DoNotCallOrderByMultipleTimes,
    Microsoft.CodeQuality.VisualBasic.Analyzers.Usage.BasicDoNotCallOrderByMultipleTimesFixer>;

namespace Microsoft.NetCore.Analyzers.Usage.UnitTests
{
    public class DoNotCallOrderByMultipleTimesTests
    {
        [Fact]
        public async Task OnlyOneOrderByCall_NoDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        public MyClass()
        {
            var q = System.Net.NetworkInformation.NetworkInterface
                          .GetAllNetworkInterfaces()
                          .OrderBy(ni => ni.NetworkInterfaceType);
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task OnlyOneOrderByCall_NoDiagnostic_VB()
        {
            await VerifyVB.VerifyAnalyzerAsync("");
        }

        [Fact]
        public async Task OrderByCallsAreUnrelated_NoDiagnostic_CS()
        {
            await VerifyCS.VerifyAnalyzerAsync("");
        }

        [Fact]
        public async Task OrderByCallsAreUnrelated_NoDiagnostic_VB()
        {
            await VerifyVB.VerifyAnalyzerAsync("");
        }

        [Fact]
        public async Task AtLeastTwoOrderByCalls_OfferFixerAsync_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        public MyClass()
        {
            var q = [|System.Net.NetworkInformation.NetworkInterface
                          .GetAllNetworkInterfaces()
                          .OrderBy(ni => ni.NetworkInterfaceType)
                          .OrderBy(ni => ni.Name)|];
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        public MyClass()
        {
            var q = System.Net.NetworkInformation.NetworkInterface
                          .GetAllNetworkInterfaces()
                          .OrderBy(ni => ni.NetworkInterfaceType)
                          .ThenBy(ni => ni.Name);
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AtLeastTwoOrderByCalls_OfferFixerAsync_VB()
        {
            await VerifyVB.VerifyCodeFixAsync("", "");
        }

        #region Helpers
        private const string CSUsings = @"using System;
using System.Linq;";

        private const string CSNamespaceAndClassStart = @"namespace Testopolis
{
    public class MyClass
    {
";

        private const string CSNamespaceAndClassEnd = @"
    }
}";

        private const string VBUsings = @"Imports System
Imports System.Linq";
        #endregion
    }
}
