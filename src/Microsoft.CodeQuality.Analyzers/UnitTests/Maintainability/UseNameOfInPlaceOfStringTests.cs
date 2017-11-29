// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class UseNameofInPlaceOfStringTests : DiagnosticAnalyzerTestBase
    {
        #region Unit tests for no analyzer diagnostic

        [Fact]
        public void NoDiagnostic_NoArguments()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException();
    }
}");
        }

        [Fact]
        public void NoDiagnostic_NullLiteral()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException(null);
    }
}");
        }

        [Fact]
        public void NoDiagnostic_StringIsAReservedWord()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException(""static"");
    }
}");
        }

        [Fact]
        public void NoDiagnostic_NoMatchingParametersInScope()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int y)
    {
        throw new ArgumentNullException(""x"");
    }
}");
        }

        [Fact]
        public void NoDiagnostic_NameColonOtherParameterName()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int y)
    {
        Console.WriteLine(format:""x"");
    }
}");
        }

        [Fact]
        public void NoDiagnostic_NotStringLiteral()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        string param = ""x"";
        throw new ArgumentNullException(param);
    }
}");
        }

        [Fact]
        public void NoDiagnostic_NotValidIdentifier()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException(""9x"");
    }
}");
        }

        [Fact]
        public void NoDiagnostic_NoArgumentList()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException(
    }
}", TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnostic_NoMatchingParameter()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException(""test"", ""test2"", ""test3"");
    }
}", TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnostic_MatchesParameterButNotCalledParamName()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        Console.WriteLine(""x"");
    }
}");
        }

        [Fact]
        public void NoDiagnostic_MatchesPropertyButNotCalledPropertyName()
        {
            VerifyCSharp(@"
using System;
using System.ComponentModel;

public class Person : INotifyPropertyChanged
{
    private string name;
    public event PropertyChangedEventHandler PropertyChanged;

    public string PersonName {
        get { return name; }
        set
        {
            name = value;
            Console.WriteLine(""PersonName"");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
        }

        [Fact]
        public void NoDiagnostic_PositionalArgumentOtherParameterName()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        Console.WriteLine(""x"");
    }
}");
        }


        #endregion


        #region Unit tests for analyzer diagnostic(s)

        [Fact]
        public void Diagnostic_ArgumentMatchesAParameterInScope()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException(""x"");
    }
}",
    GetCSharpNameofResultAt(7, 41, "x"));
        }

        [Fact]
        public void Diagnostic_VB_ArgumentMatchesAParameterInScope()
        {
            VerifyBasic(@"
Imports System

Module Mod1
    Sub f(s As String)
        Throw New ArgumentNullException(""s"")
    End Sub
End Module",
    GetBasicNameofResultAt(6, 41, "s"));
        }

        [Fact]
        public void Diagnostic_ArgumentMatchesAPropertyInScope()
        {
            VerifyCSharp(@"
using System.ComponentModel;

public class Person : INotifyPropertyChanged
{
    private string name;
    public event PropertyChangedEventHandler PropertyChanged;

    public string PersonName {
        get { return name; }
        set
        {
            name = value;
            OnPropertyChanged(""PersonName"");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}",
    GetCSharpNameofResultAt(14, 31, "PersonName"));
        }

        [Fact]
        public void Diagnostic_ArgumentMatchesAPropertyInScope2()
        {
            VerifyCSharp(@"
using System.ComponentModel;

public class Person : INotifyPropertyChanged
{
    private string name;
    public event PropertyChangedEventHandler PropertyChanged;

    public string PersonName 
    {
        get { return name; }
        set
        {
            name = value;
            OnPropertyChanged(""PersonName"");
        }
    }

    public string PersonName2
    {
        get { return name; }
        set
        {
            name = value; 
            OnPropertyChanged(nameof(PersonName2));
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}",
    GetCSharpNameofResultAt(15, 31, "PersonName"));
        }

        [Fact]
        public void Diagnostic_ArgumentNameColonParamName()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException(paramName:""x"");
    }
}",
    GetCSharpNameofResultAt(7, 51, "x"));
        }

        [Fact]
        public void Diagnostic_ArgumentNameColonPropertyName()
        {
            VerifyCSharp(@"
using System.ComponentModel;

public class Person : INotifyPropertyChanged
{
    private string name;
    public event PropertyChangedEventHandler PropertyChanged;

    public string PersonName {
        get { return name; }
        set
        {
            name = value;
            OnPropertyChanged(propertyName:""PersonName"");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}",
    GetCSharpNameofResultAt(14, 44, "PersonName"));
        }


        [Fact]
        public void Diagnostic_AnonymousFunctionMultiline1()
        {
            VerifyCSharp(@"
using System;

class Test
{
    void Method(int x)
    {
        Action<int> a = (int y) =>
        {
            throw new ArgumentException(""somemessage"", ""x"");
        };
    }
}",
    GetCSharpNameofResultAt(10, 56, "x"));
        }

        [Fact]
        public void Diagnostic_AnonymousFunctionMultiLine2()
        {
            VerifyCSharp(@"
using System;

class Test
{
    void Method(int x)
    {
        Action<int> a = (int y) =>
        {
            throw new ArgumentException(""somemessage"", ""y"");
        };
    }
}",
    GetCSharpNameofResultAt(10, 56, "y"));
        }

        [Fact]
        public void Diagnostic_AnonymousFunctionSingleLine1()
        {
            VerifyCSharp(@"
using System;

class Test
{
    void Method(int x)
    {
        Action<int> a = (int y) => throw new ArgumentException(""somemessage"", ""y"");
    }
}",
    GetCSharpNameofResultAt(8, 79, "y"));
        }

        [Fact]
        public void Diagnostic_AnonymousFunctionSingleLine2()
        {
            VerifyCSharp(@"
using System;

class Test
{
    void Method(int x)
    {
        Action<int> a = (int y) => throw new ArgumentException(""somemessage"", ""x"");
    }
}",
    GetCSharpNameofResultAt(8, 79, "x"));
        }

        [Fact]
        public void Diagnostic_AnonymousFunctionMultipleParameters()
        {
            VerifyCSharp(@"
using System;

class Test
{
    void Method(int x)
    {
        Action<int, int> a = (j, k) => throw new ArgumentException(""somemessage"", ""x"");
    }
}",
    GetCSharpNameofResultAt(8, 83, "x"));
        }

        [Fact]
        public void Diagnostic_LocalFunction1()
        {
            VerifyCSharp(@"
using System;

class Test
{
    void Method(int x)
    {
        void AnotherMethod(int y, int z)
            {
                throw new ArgumentException(""somemessage"", ""x"");
            }
    }
}",
    GetCSharpNameofResultAt(10, 60, "x"));
        }

        [Fact]
        public void Diagnostic_LocalFunction2()
        {
            VerifyCSharp(@"
using System;

class Test
{
    void Method(int x)
    {
        void AnotherMethod(int y, int z)
            {
                throw new ArgumentException(""somemessage"", ""y"");
            }
    }
}",
    GetCSharpNameofResultAt(10, 60, "y"));
        }

        [Fact]
        public void Diagnostic_Delegate()
        {
            VerifyCSharp(@"
using System;

namespace ConsoleApp14
{
    class Program
    {
         class test
        {
            Action<int> x2 = delegate (int xyz)
            {
                throw new ArgumentNullException(""xyz"");
            };
        }
    }
}",
    GetCSharpNameofResultAt(12, 49, "xyz"));
        }

        #endregion

        private DiagnosticResult GetBasicNameofResultAt(int line, int column, string name)
        {
            var message = string.Format(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringMessage, name);
            return GetBasicResultAt(line, column, UseNameofInPlaceOfStringAnalyzer.RuleId, message);
        }

        private DiagnosticResult GetCSharpNameofResultAt(int line, int column, string name)
        {
            var message = string.Format(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringMessage, name);
            return GetCSharpResultAt(line, column, UseNameofInPlaceOfStringAnalyzer.RuleId, message);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseNameofInPlaceOfStringAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseNameofInPlaceOfStringAnalyzer();
        }
    }
}
