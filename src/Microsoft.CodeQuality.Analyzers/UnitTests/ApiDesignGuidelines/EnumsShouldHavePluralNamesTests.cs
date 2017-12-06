// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class EnumsShouldHavePluralNamesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new EnumsShouldHavePluralNamesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnumsShouldHavePluralNamesAnalyzer();
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithNoFlags_SingularName()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               public enum Day 
                                {
                                    Sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }"
                          );

            VerifyBasic(@"
                        Public Class A
	                        Public Enum Day
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
                            public class A 
                            { 
                               public enum Days 
                                {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }",
                            GetCSharpResultAt(4, 44, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717.MessageFormat.ToString()));

            VerifyBasic(@"
                        Public Class A
	                        Public Enum Days
		                           Sunday = 0
		                           Monday = 1
		                           Tuesday = 2

	                        End Enum
                        End Class
                        ",
                        GetBasicResultAt(3, 38, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717.MessageFormat.ToString()));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void CA1714_CA1717__Test_EnumWithNoFlags_PluralName_Internal()
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
}

public class A2
{ 
    private enum Days 
    {
        sunday = 0,
        Monday = 1,
        Tuesday = 2
                                       
    };
}

internal class A3
{ 
    public enum Days 
    {
        sunday = 0,
        Monday = 1,
        Tuesday = 2
                                       
    };
}
");

            VerifyBasic(@"
Class A
	Private Enum Days
		Sunday = 0
		Monday = 1
		Tuesday = 2
	End Enum
End Class

Public Class A2
	Private Enum Days
		Sunday = 0
		Monday = 1
		Tuesday = 2
	End Enum
End Class

Friend Class A3
	Public Enum Days
		Sunday = 0
		Monday = 1
		Tuesday = 2
	End Enum
End Class
");
        }

        [Fact]
        public void CA1714_CA1717__Test_EnumWithNoFlags_PluralName_UpperCase()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               public enum DAYS 
                                {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }",
                            GetCSharpResultAt(4, 44, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717.MessageFormat.ToString()));

            VerifyBasic(@"
                        Public Class A
	                        Public Enum DAYS
		                           Sunday = 0
		                           Monday = 1
		                           Tuesday = 2

	                        End Enum
                        End Class
                        ",
                        GetBasicResultAt(3, 38, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1717.MessageFormat.ToString()));
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithFlags_SingularName()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               [System.Flags] 
                               public enum Day 
                               {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }",
                            GetCSharpResultAt(5, 44, EnumsShouldHavePluralNamesAnalyzer.RuleId_Plural, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1714.MessageFormat.ToString()));

            VerifyBasic(@"
                       Public Class A
	                    <System.Flags> _
	                    Public Enum Day
		                    Sunday = 0
		                    Monday = 1
		                    Tuesday = 2
	                    End Enum
                        End Class",
                            GetBasicResultAt(4, 34, EnumsShouldHavePluralNamesAnalyzer.RuleId_Plural, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1714.MessageFormat.ToString()));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void CA1714_CA1717_Test_EnumWithFlags_SingularName_Internal()
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
                                       
    }
}

public class A2
{ 
    [System.Flags] 
    private enum Day 
    {
        sunday = 0,
        Monday = 1,
        Tuesday = 2
                                       
    }
}

internal class A3
{ 
    [System.Flags] 
    public enum Day 
    {
        sunday = 0,
        Monday = 1,
        Tuesday = 2
                                       
    }
}
");

            VerifyBasic(@"
Class A
    <System.Flags> _
    Enum Day
	    Sunday = 0
	    Monday = 1
	    Tuesday = 2
    End Enum
End Class

Public Class A2
    <System.Flags> _
    Private Enum Day
	    Sunday = 0
	    Monday = 1
	    Tuesday = 2
    End Enum
End Class

Friend Class A3
    <System.Flags> _
    Public Enum Day
	    Sunday = 0
	    Monday = 1
	    Tuesday = 2
    End Enum
End Class
");
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithFlags_PluralName()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               [System.Flags] 
                               public enum Days 
                               {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Public Class A
	                    <System.Flags> _
	                    Public Enum Days
		                    Sunday = 0
		                    Monday = 1
		                    Tuesday = 2
	                    End Enum
                        End Class");
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithFlags_PluralName_UpperCase()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               [System.Flags] 
                               public enum DAYS 
                               {
                                    sunday = 0,
                                    Monday = 1,
                                    Tuesday = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Public Class A
	                    <System.Flags> _
	                    Public Enum DAYS
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
                            public class A 
                            { 
                               [System.Flags] 
                               public enum Axis 
                               {
                                    x = 0,
                                    y = 1,
                                    z = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Public Class A
	                    <System.Flags> _
	                    Public Enum Axis
		                    x = 0
		                    y = 1
		                    z = 2
	                    End Enum
                        End Class");
        }

        [Fact]
        // TODO: when the Isplural function handles better checks.
        // this test should not fail.
        public void CA1714_CA1717_Test_EnumWithFlags_PluralName_NotEndingWithS()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               [System.Flags] 
                               public enum Men 
                               {
                                    M1 = 0,
                                    M2 = 1,
                                    M3 = 2
                                       
                                };
                            }",
                            GetCSharpResultAt(5, 44, EnumsShouldHavePluralNamesAnalyzer.RuleId_Plural, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1714.MessageFormat.ToString()));

            VerifyBasic(@"
                       Public Class A
                        < System.Flags > _
                        Public Enum Men
                            M1 = 0
                            M2 = 1
                            M3 = 2
                        End Enum
                        End Class",
                            GetBasicResultAt(4, 37, EnumsShouldHavePluralNamesAnalyzer.RuleId_Plural, EnumsShouldHavePluralNamesAnalyzer.Rule_CA1714.MessageFormat.ToString()));
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithNoFlags_PluralWord_NotEndingWithS()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               public enum Men 
                               {
                                    M1 = 0,
                                    M2 = 1,
                                    M3 = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Public Class A
                        Public Enum Men
                            M1 = 0
                            M2 = 1
                            M3 = 2
                        End Enum
                        End Class");
        }

        [Fact]
        public void CA1714_CA1717_Test_EnumWithNoFlags_irregularPluralWord_EndingWith_ae()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               [System.Flags] 
                               public enum formulae 
                               {
                                    M1 = 0,
                                    M2 = 1,
                                    M3 = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Public Class A
                        < System.Flags > _
                        Public Enum formulae
                            M1 = 0
                            M2 = 1
                            M3 = 2
                        End Enum
                        End Class");
        }
        [Fact]
        public void CA1714_CA1717_Test_EnumWithNoFlags_irregularPluralWord_EndingWith_i()
        {
            VerifyCSharp(@" 
                            public class A 
                            { 
                               [System.Flags] 
                               public enum trophi 
                               {
                                    M1 = 0,
                                    M2 = 1,
                                    M3 = 2
                                       
                                };
                            }");

            VerifyBasic(@"
                       Public Class A
                        < System.Flags > _
                        Public Enum trophi
                            M1 = 0
                            M2 = 1
                            M3 = 2
                        End Enum
                        End Class");
        }
    }
}
