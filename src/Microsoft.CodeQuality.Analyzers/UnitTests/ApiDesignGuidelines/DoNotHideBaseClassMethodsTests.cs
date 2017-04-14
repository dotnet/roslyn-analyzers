// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotHideBaseClassMethodsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotHideBaseClassMethodsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotHideBaseClassMethodsAnalyzer();
        }

        [Fact]
        public void CA1061_DerivedMethodMatchesBaseMethod_NoDiagnostic()
        {
            const string Test = @"
using System;

class BaseType
{
    public void Method(string input)
    {
    }
}

class DerivedType : BaseType
{
    public void Method(string input)
    {
    }
}";

            VerifyCSharp(Test);
        }

        [Fact]
        public void CA1061_BaseMethodHasLessDerivedParameter_NoDiagnostic()
        {
            const string Test = @"
using System;

class BaseType
{
    public void Method(object input)
    {
    }
}

class DerivedType : BaseType
{
    public void Method(string input)
    {
    }
}";

            VerifyCSharp(Test);
        }

        [Fact]
        public void CA1061_DerivedMethodHasLessDerivedParameter_Diagnostic()
        {
            const string Test = @"
using System;

class BaseType
{
    public void Method(string input1, string input2)
    {
    }
}

class DerivedType : BaseType
{
    public void Method(object input1, string input2)
    {
    }
}";

            VerifyCSharp(Test, GetCA1061ResultAt(13, 17, "DerivedType.Method(object, string)", "BaseType.Method(string, string)"));
        }

        [Fact]
        public void CA1061_DerivedMethodHasLessDerivedParameter_BaseMethodPrivate_NoDiagnostic()
        {
            const string Test = @"
using System;

class BaseType
{
    private void Method(string input)
    {
    }
}

class DerivedType : BaseType
{
    public void Method(object input)
    {
    }
}";

            // Note: This behavior differs from FxCop's CA1061, but I think it makes sense
            VerifyCSharp(Test);
        }

        [Fact]
        public void CA1061_DerivedMethodHasLessDerivedParameter_DerivedMethodPrivate_Diagnostic()
        {
            const string Test = @"
using System;

class BaseType
{
    public void Method(string input)
    {
    }
}

class DerivedType : BaseType
{
    private void Method(object input)
    {
    }
}";

            VerifyCSharp(Test, GetCA1061ResultAt(13, 18, "DerivedType.Method(object)", "BaseType.Method(string)"));
        }

        [Fact]
        public void CA1061_DerivedMethodHasLessDerivedParameter_MatchingMethodInBasesBase_Diagnostic()
        {
            const string Test = @"
using System;

class BaseBaseType
{
    public void Method(string input)
    {
    }
}

class BaseType : BaseBaseType
{
}

class DerivedType : BaseType
{
    public void Method(object input)
    {
    }
}";

            VerifyCSharp(Test, GetCA1061ResultAt(17, 17, "DerivedType.Method(object)", "BaseBaseType.Method(string)"));
        }

        [Fact]
        public void CA1601_DerivedMethodOverridesAbstractBaseMethod_NoDiagnostic()
        {
            const string Test = @"
using System;

abstract class BaseType
{
    public abstract void Method(string input);
}

class DerivedType : BaseType
{
    public override void Method(string input)
    {
    }
}";

            VerifyCSharp(Test);
        }

        [Fact]
        public void CA1061_DerivedMethodOverridesBaseMethod_NoDiagnostic()
        {
            const string Test = @"
using System;

class BaseType
{
    public virtual void Method(string input)
    {
    }
}

class DerivedType : BaseType
{
    public override void Method(string input)
    {
    }
}";

            VerifyCSharp(Test);
        }

        [Fact]
        public void CA1061_DerivedMethodImplementsInterfaceMethod_NoDiagnostic()
        {
            const string Test = @"
using System;

interface IFace
{
    void Method(string input);
}

class DerivedType : IFace
{
    public void Method(string input)
    {
    }
}";

            VerifyCSharp(Test);
        }

        [Fact]
        public void CA1061_DerivedMethodDoesNotMatchBaseMethod_NoDiagnostic()
        {
            const string Test = @"
using System;

class BaseType
{
    public void Method(string input, string input2)
    {
    }
}

class DerivedType : BaseType
{
    public void Method(object input)
    {
    }
}";

            VerifyCSharp(Test);
        }

        private DiagnosticResult GetCA1061ResultAt(int line, int column, string derivedMethod, string baseMethod)
        {
            var message = string.Format(
                MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotHideBaseClassMethodsMessage, 
                derivedMethod, 
                baseMethod);

            return GetCSharpResultAt(line, column, DoNotHideBaseClassMethodsAnalyzer.RuleId, message);
        }
    }
}
