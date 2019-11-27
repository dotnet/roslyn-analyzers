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
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.Linq;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {
            public void ProperDateTimeConstruction()
            {
                var d1 = new DateTime();
                var d2 = new DateTime(d1.Year, d1.Month, d1.Day);
                d2.AddYears(1);
                var yearDiff = d1.Year - d2.Year;

                DateTime d3 = new DateTime(d1.Year + 1);
                DateTime d4 = new DateTime(d1.Year + 1, DateTimeKind.Local);

                DateTime d5 = DateTime.Now;
                DateTime d6 = d5.AddYears(1);
                DateTime d7 = d5.AddYears(2 - 1);

                DateTimeOffset d8 = DateTime.Now;
                DateTimeOffset d9 = d8.AddYears(1);
                DateTimeOffset d10 = d8.AddYears(2 - 1);
            }

            public void SafeDateTimeConstruction()
            {
                var d1 = new DateTime();
                var d2 = new DateTime(d1.Year + 1, 1, 5);
                var d3 = new DateTime(d1.Year + 1, 6, d1.Day);
                var d4 = new DateTime(d1.Year + 1, 2, 28);
                DateTime d5 = new DateTime(new DateTime(d1.Year + 9, 1, 1).Year, 1, 1);
                DateTime d6 = new DateTime(new DateTime(d1.Year + 9, 10, 11).Year, 1, 1);
                TimeSpan threeHourTour = new TimeSpan(3, 0, 0);
                DateTimeOffset d7 = new DateTimeOffset(d1.Year + 10, 5, 5, 0, 0, 0, threeHourTour);
                DateTime[] d8 = Enumerable.Range(1, 3).Select(i => new DateTime(42 + i, 1, i, 0, 0, 0, DateTimeKind.Utc)).ToArray();
            }
            
            public void CustomLogicDateTimeConstruction()
            {
                var d1 = new DateTime();
                var d2 = new DateTime(d1.Year / 5, d1.Month, d1.Day);
                var d3 = new DateTime(d1.Year * 6, d1.Month, d1.Day);
                var d4 = new DateTime(1 > 0 ? d1.Year : d1.Year, d1.Month, d1.Day);
                var a = 45;
                var d5 = new DateTime(a < 0 ? d1.Year : d1.Year, d1.Month, d1.Day);

                DateTime d6 = new DateTime(d1.Year + 1000, d1.Month, d1.Day);
                DateTime d7 = new DateTime(d1.Year + 1, d1.Month, d1.Day % 5);
                DateTime d8 = new DateTime(d1.Year + 1, d1.Month % 30, d1.Day);
                DateTime d9 = new DateTime(d1.Year + (1 % 4), d1.Month, d1.Day % 5);
            }
        }
    }");
        }

        [Fact]
        public async Task YearIncrementDiagnosticCases()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.Globalization;
    using System.Linq;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {   
            public void BasicYearIncrement()
            {
                DateTime orig = new DateTime(2000, 1, 2);

                DateTime d1 = new DateTime(orig.Year + 1, orig.Month, orig.Day);
                var d2 = new DateTime(orig.Year - 2, orig.Month, orig.Day, new GregorianCalendar());
                DateTime d3 = new DateTime(orig.Year + 3, orig.Month, orig.Day, 0, 0, 0);
                DateTime d4 = new DateTime(orig.Year - 4, orig.Month, orig.Day, 0, 0, 1, new GregorianCalendar());
                var d5 = new DateTime(orig.Year + 5, orig.Month, orig.Day, 2, 3, 4, DateTimeKind.Utc);
                var d6 = new DateTime(orig.Year + 6, orig.Month, orig.Day, 5, 6, 7, 999);
                DateTime d7 = new DateTime(orig.Year - 7, orig.Month, orig.Day, 8, 9, 10, 888, new GregorianCalendar());
                var d8 = new DateTime(orig.Year - 8, orig.Month, orig.Day, 0, 0, 1, 777, new GregorianCalendar(), DateTimeKind.Unspecified);

                TimeSpan threeHourTour = new TimeSpan(3, 0, 0);
                var d10 = new DateTimeOffset(orig.Year + 10, orig.Month, orig.Day, 0, 0, 0, threeHourTour);
                DateTimeOffset d11 = new DateTimeOffset(orig.Year + 11, orig.Month, orig.Day, 0, 0, 0, 0, new TimeSpan(3, 0, 0));
                var d12 = new DateTimeOffset(orig.Year - 12, orig.Month, orig.Day, 0, 0, 1, 0, new GregorianCalendar(), threeHourTour);
            }
            
            public void ParameterEdgeCases()
            {
                DateTime orig = new DateTime(2000, 1, 2);

                DateTime d1Named = new DateTime(year: orig.Year + 101, month: orig.Month, orig.Day);
                var d8Named = new DateTime(month: orig.Month, day: orig.Day, hour: 0, minute: 0, second: 1, millisecond: 777, calendar: new GregorianCalendar(), kind: DateTimeKind.Unspecified, year: orig.Year - 801);
                DateTime d9 = new DateTime(new DateTime(orig.Year + 9, 2, orig.Day).Year, 1, 1);
                var d21 = new DateTime(orig.Year + 19, 2, 29);
                var a = 45;
                DateTime d23 = new DateTime(
                    a + 19,
                    orig.Month,
                    orig.Day);
                DateTime d23Named = new DateTime(
                    month: orig.Month,
                    day: orig.Day,
                    year: a + 908);
                DateTime[] d24 = Enumerable
                    .Range(1, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month))
                    .Select(i => new DateTime(24 + i, i, i, 0, 0, 0, DateTimeKind.Utc)).ToArray();
                var d31 = new DateTime(orig.Year > 2001 ? orig.Year : orig.Year + 31, orig.Month, orig.Day);
                var d32 = new DateTime(orig.Year > 2001 ? orig.Year + 32 : orig.Year, orig.Month, orig.Day);

                TimeSpan threeHourTour = new TimeSpan(3, 0, 0);
                var d10Named = new DateTimeOffset(month: orig.Month, day: orig.Day, hour: 0, minute: 0, second: 0, offset: threeHourTour, year: orig.Year + 101);
                DateTimeOffset d13 = new DateTimeOffset(new DateTime(orig.Year - 13, 2, orig.Day).Year, 1, 1, 0, 0, 0, new TimeSpan(3, 0, 0));
            }
        }
    }",
            GetCSharpResultAt(14, 31, "orig.Year + 1"),
            GetCSharpResultAt(15, 26, "orig.Year - 2"),
            GetCSharpResultAt(16, 31, "orig.Year + 3"),
            GetCSharpResultAt(17, 31, "orig.Year - 4"),
            GetCSharpResultAt(18, 26, "orig.Year + 5"),
            GetCSharpResultAt(19, 26, "orig.Year + 6"),
            GetCSharpResultAt(20, 31, "orig.Year - 7"),
            GetCSharpResultAt(21, 26, "orig.Year - 8"),
            GetCSharpResultAt(24, 27, "orig.Year + 10"),
            GetCSharpResultAt(25, 38, "orig.Year + 11"),
            GetCSharpResultAt(26, 27, "orig.Year - 12"),
            GetCSharpResultAt(33, 36, "orig.Year + 101"),
            GetCSharpResultAt(34, 31, "orig.Year - 801"),
            GetCSharpResultAt(35, 44, "orig.Year + 9"),
            GetCSharpResultAt(36, 27, "orig.Year + 19"),
            GetCSharpResultAt(38, 32, "a + 19"),
            GetCSharpResultAt(42, 37, "a + 908"),
            GetCSharpResultAt(48, 34, "24 + i"),
            GetCSharpResultAt(49, 27, "orig.Year + 31"),
            GetCSharpResultAt(50, 27, "orig.Year + 32"),
            GetCSharpResultAt(53, 32, "orig.Year + 101"),
            GetCSharpResultAt(54, 57, "orig.Year - 13"));
        }

        [Fact]
        public async Task YearIncrementVariableDiagnosticCases()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;

    namespace LeapYear.UnitTests
    {
        public class ClassName
        {   
            public void OutsideContextVariables()
            {
                var orig = new DateTime(2000, 1, 2);
                int outsideContextYearIncrementVariable = orig.Year + 1;
            }

            public void VariableYearIncrement()
            {
                var orig = new DateTime(2000, 1, 2);
                int yearIncrementVariable = orig.Year + 1;
                DateTime d1 = new DateTime(yearIncrementVariable, orig.Month, orig.Day);

                int yearIncrementAssignment = 0;
                yearIncrementAssignment = orig.Year - 2;
                DateTime d2 = new DateTime(yearIncrementAssignment, orig.Month, orig.Day);
            }

            public void NotVariableYearIncrement()
            {
                DateTime orig = new DateTime(2000, 1, 2);
                int unusedYearIncrement = orig.Year + 1;
                int year = orig.Year;
                int month = orig.Month;
                DateTime d1 = new DateTime(year, month, orig.Day);
                int outsideContextYearIncrementInitializer = 0;
                DateTime d2 = new DateTime(outsideContextYearIncrementInitializer, month, orig.Day);
            }
        }
    }",
            GetIdentifierCSharpResultAt(18, 31, "yearIncrementVariable", "orig.Year + 1"),
            GetIdentifierCSharpResultAt(22, 31, "yearIncrementAssignment", "orig.Year - 2"));
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, string expression)
            => new DiagnosticResult(DoNotIncrementYearValueInDateConstructor.Rule)
            .WithLocation(line, column)
            .WithMessage(string.Format(
                CultureInfo.CurrentCulture,
                MicrosoftNetCoreAnalyzersResources.DoNotIncrementYearInDateTimeConstructorMessage,
                expression));

        private static DiagnosticResult GetIdentifierCSharpResultAt(
            int line,
            int column,
            string expression1,
            string expression2)
            => new DiagnosticResult(DoNotIncrementYearValueInDateConstructor.IdentifierRule)
            .WithLocation(line, column)
            .WithMessage(string.Format(
                CultureInfo.CurrentCulture,
                MicrosoftNetCoreAnalyzersResources.DoNotIncrementYearInDateTimeIdentifierConstructorMessage,
                expression1,
                expression2));
    }
}
