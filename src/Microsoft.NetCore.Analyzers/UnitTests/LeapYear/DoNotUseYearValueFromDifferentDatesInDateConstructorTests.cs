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
    using System.Globalization;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {
            public void ProperDateTimeConstruction()
            {
                DateTime utc1 = DateTime.UtcNow;
                int year = 1893;
                var orig = new DateTime(2000, 1, 2);
                var foo = new { x = 2000, y = 2 };
                var d2 = new DateTime(foo.x, foo.y, utc1.Day); // Same source but different properties.
                var d4 = new DateTime(orig.Year, orig.Month, orig.Day);
                var d11 = new DateTime(
                    orig.Year,
                    orig.Month,
                    orig.Day);
                var d12 = new DateTime(orig.Year,
                    orig.Month, orig.Day);
                var kind = DateTimeKind.Unspecified;
                var d5 = new DateTime(orig.Year, orig.Month, orig.Day, 0, 0, 1, 777, new GregorianCalendar(), kind);
                var d9 = new DateTime(orig.Year, orig.Month, utc1.Day);
                var d10 = new DateTimeOffset(orig.Year, orig.Month, utc1.Day, 0, 0, 0, new TimeSpan(3, 0, 0));
            }

            public void SafeDateTimeConstruction()
            {
                int year = 1893;
                int safeYear = 2019;
                int safeMonth = 4;
                int safeDay = 7;
                var orig = new DateTime(2000, 1, 2);
                var d3 = new DateTime(year, safeMonth, safeDay);
                var d14 = new DateTime(DateTime.Now.Year, safeMonth, 7);
                var d15 = new DateTime(DateTime.Now.Year, safeMonth, 28);
                var d22 = new DateTime(year > 1900 ? safeYear : year, safeMonth, orig.Day);
            }

            public void CustomLogicDateTimeConstruction()
            {
                DateTime utc1 = DateTime.UtcNow;
                int year = 1893;
                var orig = new DateTime(2000, 1, 2);
                var d16 = new DateTime(utc1.Year, orig.Month, orig.Day % 28);
                var d17 = new DateTime(year > 1900 ? utc1.Year : orig.Year, orig.Month, orig.Day % 28);
            }
        }
    }");
        }

        [Fact]
        public async Task DatePartOverflowBasicCases()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.Globalization;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {   
            public void DatePartOverflow()
            {
                DateTime utc1 = DateTime.UtcNow;
                int unsafeMonth = 2;
                int unsafeDay = 29;
                var orig = new DateTime(2000, 1, 2);
                var d1 = new DateTime(utc1.Year, orig.Month, orig.Day);
                DateTime utc2 = DateTime.UtcNow;
                TimeSpan threeHourTour = new TimeSpan(3, 0, 0);
                DateTimeOffset d2 = new DateTimeOffset(utc2.Year, orig.Month, orig.Day, 0, 0, 0, threeHourTour);
                DateTimeOffset d3 = new DateTimeOffset(d2.Year, orig.Month, orig.Day, 0, 0, 0, new TimeSpan(3, 0, 0));
                var d4 = new DateTime(d3.Year, orig.Month, orig.Day);
                var d5 = new DateTime(DateTime.Now.Year, unsafeMonth, unsafeDay);
            }
        }
    }",
            GetCSharpResultAt(15, 26, "utc1", "orig"),
            GetCSharpResultAt(18, 37, "utc2", "orig"),
            GetCSharpResultAt(19, 37, "d2", "orig"),
            GetCSharpResultAt(20, 26, "d3", "orig"),
            GetCSharpResultAt(21, 26, "DateTime.Now", "unsafeMonth"));
        }

        [Fact]
        public async Task DatePartOverflowParameterCases()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.Globalization;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {   
            public void DatePartOverflow()
            {
                var orig = new DateTime(2000, 1, 2);
                DateTime utcNamed = DateTime.UtcNow;
                var d1 = new DateTime(month: orig.Month, day: orig.Day, year: utcNamed.Year);
                DateTime utc = DateTime.UtcNow;
                TimeSpan threeHourTour = new TimeSpan(3, 0, 0);
                DateTimeOffset d2 = new DateTimeOffset(year: utcNamed.Year, month: orig.Month, day: orig.Day, hour: 0, minute: 0, second: 0, threeHourTour);
                var d3 = new DateTime(orig.Year > 2001 ? d2.Year : orig.Year, orig.Month, orig.Day);
                var d4 = new DateTime(orig.Year > 2001 ? orig.Year : d3.Year, orig.Month, orig.Day);
                var d5 = new DateTime(orig.Year, orig.Year > 2001 ? orig.Month : d4.Month, orig.Day);
                var d6 = new DateTime(orig.Year, orig.Year > 2001 ? d5.Month : orig.Month, orig.Day);
            }
        }
    }",
            GetCSharpResultAt(13, 26, "utcNamed", "orig"),
            GetCSharpResultAt(16, 37, "utcNamed", "orig"),
            GetCSharpResultAt(17, 26, "d2", "orig"),
            GetCSharpResultAt(18, 26, "d3", "orig"),
            GetCSharpResultAt(19, 26, "orig", "d4"),
            GetCSharpResultAt(20, 26, "orig", "d5"));
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
