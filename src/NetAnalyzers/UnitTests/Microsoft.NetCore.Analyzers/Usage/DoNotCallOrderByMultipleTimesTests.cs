// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.MicrosoftNetCoreAnalyzers.Usage
{
    public class DoNotCallOrderByMultipleTimesTests
    {
        [Fact]
        public async Task OnlyOneOrderByCall_NoDiagnosticAsync()
        {
            Assert.False(true);
        }

        [Fact]
        public async Task OrderByCallsAreUnrelated_NoDiagnosticAsync()
        {
            Assert.False(true);
        }

        [Fact]
        public async Task AtLeastTwoOrderByCalls_OfferFixerAsync()
        {
            Assert.False(true);
        }
    }
}
