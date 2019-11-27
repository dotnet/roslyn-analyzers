// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.LeapYear.DoNotUseYearValueFromDifferentDatesInDateConstructor,
    Microsoft.NetCore.CSharp.Analyzers.Tasks.CSharpDoNotCreateTasksWithoutPassingATaskSchedulerFixer>;

namespace Microsoft.NetCore.Analyzers.LeapYear.UnitTests
{
    public class DoNotUseYearValueFromDifferentDatesInDateConstructorTests
    {
        [Fact]
        public async Task NoDiagnosticCases()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {
        }
    }");
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
            public void DatePartOverflow()
            {
                var d1 = new DateTime();
                var d2 = new DateTime();
                var badDateTime = new DateTime(d1.Year, d2.Month, d2.Day);
            }
        }
    }",
            GetCSharpResultAt(12, 35, "d1", "d2"));
        }

        private static DiagnosticResult GetCSharpResultAt(
            int line,
            int column,
            string expression1,
            string expression2)
            => new DiagnosticResult(DoNotUseYearValueFromDifferentDatesInDateConstructor.Rule)
            .WithLocation(line, column)
            .WithMessage(string.Format(
                CultureInfo.CurrentCulture,
                MicrosoftNetCoreAnalyzersResources.DoNotUseDatePartOverflowPatternMessage,
                expression1,
                expression2));
    }
}
