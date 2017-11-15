// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.Maintainability;
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
        throw new ArgumentNullException([||]);
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
        throw new ArgumentNullException([|""x""|]);
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
        Console.WriteLine(format:[|""x""|]);
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
        string param = [|""x""|];
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
        throw new ArgumentNullException([|""9x""|]);
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
        throw new ArgumentNullException([|""x""|]);
    }
}",
    GetCSharpNameofResultAt(7, 41));
        }

        [Fact]
        public void Diagnostic_ArgumentMatchesPropertyInScope()
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
    GetCSharpNameofResultAt(14, 31));
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
        throw new ArgumentNullException(paramName:[|""x""|]);
    }
}",
    GetCSharpNameofResultAt(7, 51));
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
    GetCSharpNameofResultAt(14, 44));
        }



        #endregion

        private DiagnosticResult[] GetBasicNameofResultAt(int v1, int v2)
        {
            throw new NotImplementedException();
        }

        private DiagnosticResult GetCSharpNameofResultAt(int line, int column
            
            )
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringMessage, "test");
            return GetCSharpResultAt(line, column, UseNameofInPlaceOfStringAnalyzer<SyntaxKind>.RuleId, message);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            //   return new visu();
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpUseNameofInPlaceOfString();
        }
    }
}
