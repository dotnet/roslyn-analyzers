// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.LeapYear.DoNotIncrementYearValueInDateConstructor,
    Microsoft.NetCore.CSharp.Analyzers.Tasks.CSharpDoNotCreateTasksWithoutPassingATaskSchedulerFixer>;

namespace Microsoft.NetCore.Analyzers.LeapYear.UnitTests
{
    public class DoNotIncrementYearValueInDateConstructorTests
    {
        [Fact]
        public async Task NoDiagnosticCases()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"");
        }

        [Fact]
        public async Task DiagnosticCases()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {   
            public void YearIncrement()
            {
                var d1 = new DateTime();
                var badDateTime = new DateTime(d1.Year + 1, d1.Month, d1.Day);
            }
        }
    }",
            GetCSharpResultAt(11, 35, "d1.Year + 1"));
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, string expression)
            => new DiagnosticResult(DoNotIncrementYearValueInDateConstructor.Rule)
            .WithLocation(line, column)
            .WithMessage(string.Format(
                CultureInfo.CurrentCulture,
                MicrosoftNetCoreAnalyzersResources.DoNotIncrementYearInDateTimeConstructorMessage,
                expression));
    }
}
