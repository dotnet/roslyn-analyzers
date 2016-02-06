// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.UnitTests
{
    public class CA1009Tests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DeclareEventHandlersCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DeclareEventHandlersCorrectlyAnalyzer();
        }

        [Fact]
        public void TestCSEventWithEventArgs()
        {
            var test = @"
using System;

public class C
{
    public event EventHandler<EventArgs> E;
}";

            VerifyCSharp(test);
        }

        [Fact]
        public void TestCSEventWithParameterInheritedFromEventArgs()
        {
            var test = @"
using System;

public class C
{
    public event EventHandler<EA> E;
}

public class EA : EventArgs
{
}";

            VerifyCSharp(test);
        }

        [Fact]
        public void TestCSEventWithCustomDelegate()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D(object sender, EA e);

public class EA : EventArgs
{
}";

            VerifyCSharp(test);
        }

        [Fact]
        public void TestCSEventWithReturnValue()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate int D(object sender, EA e);

public class EA : EventArgs
{
}";

            VerifyCSharp(test, GetCA1009CSharpResultAt(6, 20, "E"));
        }

        [Fact]
        public void TestCSEventWith3Parameters()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D(object sender, EventArgs e, int i);";

            VerifyCSharp(test, GetCA1009CSharpResultAt(6, 20, "E"));
        }

        [Fact]
        public void TestCSEventWithFirstParameterNotObjecType()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D(string sender, EventArgs e);";

            VerifyCSharp(test, GetCA1009CSharpResultAt(6, 20, "E"));
        }

        [Fact]
        public void TestCSEventWithSecondParameterNotInheritedFromEventArgs()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D(object sender, ArgumentNullException e);";

            VerifyCSharp(test, GetCA1009CSharpResultAt(6, 20, "E"));
        }

        [Fact]
        public void TestCSEventWithNoParameters()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D();
";

            VerifyCSharp(test, GetCA1009CSharpResultAt(6, 20, "E"));
        }

        [Fact]
        public void TestCSEventWithFirstParameterByReference()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D(ref object sender, EventArgs e);";

            VerifyCSharp(test, GetCA1009CSharpResultAt(6, 20, "E"));
        }

        [Fact]
        public void TestCSEventWithSecondParameterOut()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D(object sender, out EventArgs e);";

            VerifyCSharp(test, GetCA1009CSharpResultAt(6, 20, "E"));
        }

        [Fact]
        public void TestCSEventWithNoParametersWithScope()
        {
            var test = @"
using System;

public class C
{
    public event D E;
}

public delegate void D();

[|public class F
{
    public event EventHandler E;
}|]
";

            VerifyCSharp(test);
        }

        [Fact]
        public void TestVBEventWithEventArgs()
        {
            var test = @"
Imports System

Public Class C
    Public Event E As EventHandler
End Class";

            VerifyBasic(test);
        }

        [Fact]
        public void TestVBEventWithParameterInheritedFromEventArgs()
        {
            var test = @"
Imports System

Public Class C
    Public Event E As EventHandler(Of EA)
End Class

Public Class EA
    Inherits EventArgs
End Class";

            VerifyBasic(test);
        }

        [Fact]
        public void TestVBEventWithCustomDelegate()
        {
            var test = @"
Imports System

Public Class C
    Public Event E(sender As Object, e As EA)
End Class

Public Class EA
    Inherits EventArgs
End Class";

            VerifyBasic(test);
        }

        [Fact]
        public void TestVBEventWithCustomDelegateWithExplicitByVal()
        {
            var test = @"
Imports System

Public Class C
    Public Event E(ByVal sender As Object, ByVal e As EA)
End Class

Public Class EA
    Inherits EventArgs
End Class";

            VerifyBasic(test);
        }

        [Fact]
        public void TestVBEventWith3Parameters()
        {
            var test = @"
Imports System

Public Class C
    Public Event E(ByVal sender As Object, ByVal e As EventArgs, ByVal i As Integer)
End Class";

            VerifyBasic(test, GetCA1009BasicResultAt(5, 18, "E"));
        }

        [Fact]
        public void TestVBEventWithFirstParameterNotObjecType()
        {
            var test = @"
Imports System

Public Class C
    Public Event E(ByVal sender As String, ByVal e As EventArgs)
End Class";

            VerifyBasic(test, GetCA1009BasicResultAt(5, 18, "E"));
        }

        [Fact]
        public void TestVBEventWithSecondParameterNotInheritedFromEventArgs()
        {
            var test = @"
Imports System

Public Class C
    Public Event E(ByVal sender As Object, ByVal e As ArgumentNullException)
End Class";

            VerifyBasic(test, GetCA1009BasicResultAt(5, 18, "E"));
        }

        [Fact]
        public void TestVBEventWithNoParameters()
        {
            var test = @"
Imports System

Public Class C
    Public Event E()
End Class";

            VerifyBasic(test, GetCA1009BasicResultAt(5, 18, "E"));
        }

        [Fact]
        public void TestVBEventWithFirstParameterByReference()
        {
            var test = @"
Imports System

Public Class C
    Public Event E(ByRef sender As Object, ByVal e As EventArgs)
End Class";

            VerifyBasic(test, GetCA1009BasicResultAt(5, 18, "E"));
        }

        [Fact]
        public void TestVBEventWithSecondParameterByReference()
        {
            var test = @"
Imports System

Public Class C
    Public Event E(ByVal sender As Object, ByRef e As EventArgs)
End Class";

            VerifyBasic(test, GetCA1009BasicResultAt(5, 18, "E"));
        }

        [Fact]
        public void TestVBEventWithNoParametersWithScope()
        {
            var test = @"
Imports System

Public Class C
    Public Event E()
End Class

[|Public Class D
    Public Event E As EventHandler
End Class|]
";

            VerifyBasic(test);
        }

        internal static string CA1009Name = "CA1009";
        internal static string CA1009Message = MicrosoftApiDesignGuidelinesAnalyzersResources.DeclareEventHandlersCorrectlyMessage;

        private static DiagnosticResult GetCA1009CSharpResultAt(int line, int column, string objectName)
        {
            return GetCSharpResultAt(line, column, CA1009Name, string.Format(CA1009Message, objectName));
        }

        private static DiagnosticResult GetCA1009BasicResultAt(int line, int column, string objectName)
        {
            return GetBasicResultAt(line, column, CA1009Name, string.Format(CA1009Message, objectName));
        }
    }
}
