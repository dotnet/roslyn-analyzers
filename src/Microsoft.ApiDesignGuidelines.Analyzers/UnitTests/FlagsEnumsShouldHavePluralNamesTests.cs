// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class FlagsEnumsShouldHavePluralNamesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new FlagsEnumsShouldHavePluralNamesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FlagsEnumsShouldHavePluralNamesAnalyzer();
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithNoFlags_SingularName()
        {
            VerifyCSharp(@" 
                            class A 
                            { 
                               enum Day 
                                {
                                    Sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }"
                          );

            VerifyBasic(@"
                        Class A
	                        Private Enum Day
		                           Sunday = 0
		                           Monday = 1
		                           Tuesday = 2

	                        End Enum
                        End Class
                        ");

        }

        [Fact]
        public void CA1714_CA1717__Test_EnumWithNoFlags_PluralName()
        {
            VerifyCSharp(@" 
                            class A 
                            { 
                               enum Days 
                                {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }",
                            GetCSharpResultAt(4, 37, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithNoFlag, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithNoFlag.MessageFormat.ToString()));

            VerifyBasic(@"
                        Class A
	                        Private Enum Days
		                           Sunday = 0
		                           Monday = 1
		                           Tuesday = 2

	                        End Enum
                        End Class
                        ",
                        GetBasicResultAt(3, 39, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithNoFlag, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithNoFlag.MessageFormat.ToString()));

        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithFlags_SingularName()
        {
            VerifyCSharp(@" 
                            class A 
                            { 
                               [System.Flags] 
                               enum Day 
                               {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }",
                            GetCSharpResultAt(5, 37, FlagsEnumsShouldHavePluralNamesAnalyzer.RuleId_EnumsWithFlag, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithFlag.MessageFormat.ToString()));

            VerifyBasic(@"
                       Class A
	                    <System.Flags> _
	                    Private Enum Day
		                    Sunday = 0
		                    Monday = 1
		                    Tuesday = 2
	                    End Enum
                        End Class",
                            GetBasicResultAt(4, 35, FlagsEnumsShouldHavePluralNamesAnalyzer.RuleId_EnumsWithFlag, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithFlag.MessageFormat.ToString()));

        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithFlags_PluralName()
        {
            VerifyCSharp(@" 
                            class A 
                            { 
                               [System.Flags] 
                               enum Days 
                               {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Class A
	                    <System.Flags> _
	                    Private Enum Days
		                    Sunday = 0
		                    Monday = 1
		                    Tuesday = 2
	                    End Enum
                        End Class");


        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithFlags_NameEndsWithS()
        {
            VerifyCSharp(@" 
                            class A 
                            { 
                               [System.Flags] 
                               enum Axis 
                               {
                                    x = 0,
                                    y = 1,
                                    z = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Class A
	                    <System.Flags> _
	                    Private Enum Axis
		                    x = 0
		                    y = 1
		                    z = 2
	                    End Enum
                        End Class");
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithFlags_PluralName_NotEndingWithS()
        {
            VerifyCSharp(@" 
                            class A 
                            { 
                               [System.Flags] 
                               enum Men 
                               {
                                    M1 = 0,
                                    M2 = 1,
                                    M3 = 2
                                       
                                };
                            }",
                            GetCSharpResultAt(5, 37, FlagsEnumsShouldHavePluralNamesAnalyzer.RuleId_EnumsWithFlag, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithFlag.MessageFormat.ToString()));

            VerifyBasic(@"
                       Class A
                        < System.Flags > _
                        Private Enum Men
                            M1 = 0
                            M2 = 1
                            M3 = 2
                        End Enum
                        End Class",
                            GetBasicResultAt(4, 38, FlagsEnumsShouldHavePluralNamesAnalyzer.RuleId_EnumsWithFlag, FlagsEnumsShouldHavePluralNamesAnalyzer.Rule_EnumsWithFlag.MessageFormat.ToString()));

        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithNoFlags_PluralWord_NotEndingWithS()
        {
            VerifyCSharp(@" 
                            class A 
                            { 
                               enum Men 
                               {
                                    M1 = 0,
                                    M2 = 1,
                                    M3 = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Class A
                        Private Enum Men
                            M1 = 0
                            M2 = 1
                            M3 = 2
                        End Enum
                        End Class");
        }
    }
}
