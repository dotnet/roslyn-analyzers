// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ImplementStandardExceptionConstructorsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicImplementStandardExceptionConstructorsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpImplementStandardExceptionConstructorsAnalyzer();
        }
        #region CSharp Unit Tests

        [Fact]
        public void CSharp_CA1032_NoDiagnostic_NotDerivingFromException()
        {
            VerifyCSharp(@"
//example of a class that doesn't derive from Exception type
public class NotDerivingFromException
{
}  

");
        }

        [Fact]
        public void CSharp_CA1032_NoDiagnostic_GoodException1()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type with all the minimum needed constructors
public class GoodException1 : Exception
{
    public GoodException1()
    {
    }
    public GoodException1(string message): base(message) 
    {
    }     
    public GoodException1(string message, Exception innerException) : base(message, innerException)
    {
    }
}  

");
        }

        [Fact]
        public void CSharp_CA1032_NoDiagnostic_GoodException2()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type with all the minimum needed constructors plus an extra constructor
public class GoodException2 : Exception
{
    public GoodException2()
    {
    }
    public GoodException2(string message): base(message) 
    {
    }     
    public GoodException2(int i, string message)
    {
    }
    public GoodException2(string message, Exception innerException) : base(message, innerException)
    {
    }
}  

");
        }

        [Fact]
        public void CSharp_CA1032_Diagnostic_MissingAllConstructors()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type and missing all minimum required constructors - in this case system creates a default parameterless constructor
public class BadException1 : Exception
{
}  
",
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException1", constructor: "public BadException1(string message)"),
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException1", constructor: "public BadException1(string message, Exception innerException)"));
        }

        [Fact]
        public void CSharp_CA1032_Diagnostic_MissingTwoConstructors()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type and missing 2 minimum required constructors 
public class BadException2 : Exception
{
    public BadException2()
    {
    } 
}  
",
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException2", constructor: "public BadException2(string message)"),
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException2", constructor: "public BadException2(string message, Exception innerException)"));
        }

        [Fact]
        public void CSharp_CA1032_Diagnostic_MissingDefaultConstructor()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type with missing default constructor 
public class BadException3 : Exception
{
    public BadException3(string message): base(message) 
    {
    }     
    public BadException3(string message, Exception innerException) : base(message, innerException)
    {
    }
}  
",
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException3", constructor: "public BadException3()"));
        }

        [Fact]
        public void CSharp_CA1032_Diagnostic_MissingConstructor2()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type with missing constructor containing string type parameter
public class BadException4 : Exception
{
    public BadException4()
    {
    }   
    public BadException4(string message, Exception innerException) : base(message, innerException)
    {
    }
} 
",
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException4", constructor: "public BadException4(string message)"));
        }

        [Fact]
        public void CSharp_CA1032_Diagnostic_MissingConstructor3()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type with missing constructor containing string type and exception type parameter
public class BadException5 : Exception
{
    public BadException5()
    {
    }   
    public BadException5(string message): base(message) 
    {
    } 
} 
",
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException5", constructor: "public BadException5(string message, Exception innerException)"));
        }

        [Fact]
        public void CSharp_CA1032_Diagnostic_SurplusButMissingConstructor3()
        {
            VerifyCSharp(@"
using System;
//example of a class that derives from Exception type, and has 3 constructors but missing constructor containing string type parameter only
public class BadException6 : Exception
{
    public BadException6()
    {
    }   
    public BadException6(int i, string message)
    {
    }
    public BadException6(string message, Exception innerException) : base(message, innerException)
    {
    }
} 
",
            GetCA1032CSharpMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException6", constructor: "public BadException6(string message)"));
        }

        #endregion

        #region VB Unit Test

        [Fact]
        public void Basic_CA1032_NoDiagnostic_NotDerivingFromException()
        {
            VerifyBasic(@"
'example of a class that doesn't derive from Exception type
Public Class NotDerivingFromException
End Class

");
        }

        [Fact]
        public void Basic_CA1032_NoDiagnostic_GoodException1()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type with all the minimum needed constructors
Public Class GoodException1 : Inherits Exception
    Sub New()
    End Sub
    Sub New(message As String)
    End Sub
    Sub New(message As String, innerException As Exception)
    End Sub
End Class

");
        }

        [Fact]
        public void Basic_CA1032_NoDiagnostic_GoodException2()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type with all the minimum needed constructors plus an extra constructor
Public Class GoodException2 : Inherits Exception
    Sub New()
    End Sub
    Sub New(message As String)
    End Sub
    Sub New(message As String, innerException As Exception)
    End Sub
    Sub New(i As Integer, message As String)
    End Sub
End Class 

");
        }

        [Fact]
        public void Basic_CA1032_Diagnostic_MissingAllConstructors()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type and missing all minimum required constructors - in this case system creates a default parameterless constructor
Public Class BadException1 : Inherits Exception
End Class
",
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException1", constructor: "Public Sub New(message As String)"),
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException1", constructor: "Public Sub New(message As String, innerException As Exception)"));
        }

        [Fact]
        public void Basic_CA1032_Diagnostic_MissingTwoConstructors()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type and missing 2 minimum required constructors 
Public Class BadException2 : Inherits Exception
    Sub New()
    End Sub
End Class 
",
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException2", constructor: "Public Sub New(message As String)"),
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException2", constructor: "Public Sub New(message As String, innerException As Exception)"));
        }

        [Fact]
        public void Basic_CA1032_Diagnostic_MissingDefaultConstructor()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type with missing default constructor 
Public Class BadException3 : Inherits Exception
    Sub New(message As String)
    End Sub
    Sub New(message As String, innerException As Exception)
    End Sub
End Class  
",
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException3", constructor: "Public Sub New()"));
        }

        [Fact]
        public void Basic_CA1032_Diagnostic_MissingConstructor2()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type with missing constructor containing string type parameter
Public Class BadException4 : Inherits Exception
    Sub New()
    End Sub
    Sub New(message As String, innerException As Exception)
    End Sub
End Class 
",
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException4", constructor: "Public Sub New(message As String)"));
        }

        [Fact]
        public void Basic_CA1032_Diagnostic_MissingConstructor3()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type with missing constructor containing string type and exception type parameter
Public Class BadException5 : Inherits Exception
    Sub New()
    End Sub
    Sub New(message As String)
    End Sub
End Class 
",
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException5", constructor: "Public Sub New(message As String, innerException As Exception)"));
        }

        [Fact]
        public void Basic_CA1032_Diagnostic_SurplusButMissingConstructor3()
        {
            VerifyBasic(@"
Imports System
'example of a class that derives from Exception type, and has 3 constructors but missing constructor containing string type parameter only
Public Class BadException6 : Inherits Exception
    Sub New()
    End Sub
    Sub New(message As String, innerException As Exception)
    End Sub
    Sub New(i As Integer, message As String)
    End Sub
End Class 
",
            GetCA1032BasicMissingConstructorResultAt(line: 4, column: 14, typeName: "BadException6", constructor: "Public Sub New(message As String)"));
        }
        #endregion

        #region Helpers

        private static DiagnosticResult GetCA1032CSharpMissingConstructorResultAt(int line, int column, string typeName, string constructor)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftCodeQualityAnalyzersResources.ImplementStandardExceptionConstructorsMessageMissingConstructor, typeName, constructor);
            return GetCSharpResultAt(line, column, ImplementStandardExceptionConstructorsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCA1032BasicMissingConstructorResultAt(int line, int column, string typeName, string constructor)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftCodeQualityAnalyzersResources.ImplementStandardExceptionConstructorsMessageMissingConstructor, typeName, constructor);
            return GetBasicResultAt(line, column, ImplementStandardExceptionConstructorsAnalyzer.RuleId, message);
        }
        #endregion
    }
}