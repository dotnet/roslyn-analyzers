// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Diagnostics.Test.Utilities;
using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class DoNotIgnoreMethodResultsTests : DiagnosticAnalyzerTestBase
    {
        #region Unit tests for no analyzer diagnostic

        [Fact]
        [WorkItem(462, "https://github.com/dotnet/roslyn-analyzers/issues/462")]
        public void UsedInvocationResult()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

public class C
{
    private static void M(string x, out int y)
    {
        // Object creation
        var c = new C();
        
        // String creation
        var xUpper = x.ToUpper();

        // Try parse
        if (!int.TryParse(x, out y))
        {
            return;
        }

        var result = NativeMethod();
    }

    [DllImport(""user32.dll"")]
    private static extern int NativeMethod();
}
");

            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    Private Shared Sub M(x As String, ByRef y As Integer)
        ' Object creation
        Dim c = New C()

        ' String creation
        Dim xUpper = x.ToUpper()

        ' Try parse
        If Not Integer.TryParse(x, y) Then
            Return
        End If

        Dim result = NativeMethod()
    End Sub

    <DllImport(""user32.dll"")> _
    Private Shared Function NativeMethod() As Integer
    End Function
End Class
");
        }

        #endregion

        #region Unit tests for analyzer diagnostic(s)

        [Fact]
        [WorkItem(462, "https://github.com/dotnet/roslyn-analyzers/issues/462")]
        public void UnusedStringCreation()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

class C
{
    public void DoesNotAssignStringToVariable()
    {
        Console.WriteLine(this);
        string sample = ""Sample"";
        sample.ToLower(CultureInfo.InvariantCulture);
        return;
    }
}
",
    GetCSharpStringCreationResultAt(11, 9, "DoesNotAssignStringToVariable", "ToLower"));

            VerifyBasic(@"
Imports System.Globalization

Class C
    Public Sub DoesNotAssignStringToVariable()
        Console.WriteLine(Me)
        Dim sample As String = ""Sample""
        sample.ToLower(CultureInfo.InvariantCulture)
        Return
    End Sub
End Class
",
    GetBasicStringCreationResultAt(8, 9, "DoesNotAssignStringToVariable", "ToLower"));
        }

        [Fact]
        [WorkItem(462, "https://github.com/dotnet/roslyn-analyzers/issues/462")]
        public void UnusedObjectCreation()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

class C
{
    public void DoesNotAssignObjectToVariable()
    {
        new C();
    }
}
",
    GetCSharpObjectCreationResultAt(9, 9, "DoesNotAssignObjectToVariable", "C"));

            // Following code produces syntax error for VB, so no object creation diagnostic.
            VerifyBasic(@"
Imports System
Imports System.Globalization

Class C
    Public Sub DoesNotAssignObjectToVariable()
        New C()
    End Sub
End Class
");
        }

        [Fact]
        [WorkItem(462, "https://github.com/dotnet/roslyn-analyzers/issues/462")]
        public void UnusedTryParseResult()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

public class C
{
    private static void M(string x, out int y)
    {
        // Try parse
        int.TryParse(x, out y));
    }
}
",
    GetCSharpTryParseResultAt(9, 9, "M", "TryParse"));

            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    Private Shared Sub M(x As String, ByRef y As Integer)
        ' Try parse
        Integer.TryParse(x, y)
    End Sub
End Class
",
    GetBasicTryParseResultAt(7, 9, "M", "TryParse"));
        }

        [Fact]
        [WorkItem(462, "https://github.com/dotnet/roslyn-analyzers/issues/462")]
        public void UnusedPInvokeResult()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

public class C
{
    private static void M(string x, out int y)
    {
        NativeMethod();
    }

    [DllImport(""user32.dll"")]
    private static extern int NativeMethod();
}
",
    GetCSharpHResultOrErrorCodeResultAt(8, 9, "M", "NativeMethod"));

            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    Private Shared Sub M(x As String, ByRef y As Integer)
        NativeMethod()
    End Sub

    <DllImport(""user32.dll"")> _
    Private Shared Function NativeMethod() As Integer
    End Function
End Class
",
    GetBasicHResultOrErrorCodeResultAt(6, 9, "M", "NativeMethod"));
        }

        [Fact(Skip = "746")]
        [WorkItem(746, "https://github.com/dotnet/roslyn-analyzers/issues/746")]
        public void UnusedComImportPreserveSig()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

public class C
{
    private static void M(IComClass cc)
    {
        cc.NativeMethod();
    }
}

[ComImport]
[Guid(""060DDE7F-A9CD-4669-A443-B6E25AF44E7C"")]
public interface IComClass
{
    [PreserveSig]
    int NativeMethod();
}
",
    GetCSharpHResultOrErrorCodeResultAt(8, 9, "M", "NativeMethod"));

            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    Private Shared Sub M(cc As IComClass)
        cc.NativeMethod()
    End Sub
End Class

<ComImport> _
<Guid(""060DDE7F-A9CD-4669-A443-B6E25AF44E7C"")> _
Public Interface IComClass
    <PreserveSig> _
    Function NativeMethod() As Integer
End Interface
",
    GetBasicHResultOrErrorCodeResultAt(6, 9, "M", "NativeMethod"));
        }

        #endregion

        #region Helpers


        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotIgnoreMethodResultsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotIgnoreMethodResultsAnalyzer();
        }

        private static DiagnosticResult GetCSharpStringCreationResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageStringCreation, containingMethodName, invokedMethodName);
            return GetCSharpResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicStringCreationResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageStringCreation, containingMethodName, invokedMethodName);
            return GetBasicResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpObjectCreationResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageObjectCreation, containingMethodName, invokedMethodName);
            return GetCSharpResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicObjectCreationResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageObjectCreation, containingMethodName, invokedMethodName);
            return GetBasicResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpTryParseResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageTryParse, containingMethodName, invokedMethodName);
            return GetCSharpResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicTryParseResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageTryParse, containingMethodName, invokedMethodName);
            return GetBasicResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpHResultOrErrorCodeResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageHResultOrErrorCode, containingMethodName, invokedMethodName);
            return GetCSharpResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicHResultOrErrorCodeResultAt(int line, int column, string containingMethodName, string invokedMethodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.DoNotIgnoreMethodResultsMessageHResultOrErrorCode, containingMethodName, invokedMethodName);
            return GetBasicResultAt(line, column, DoNotIgnoreMethodResultsAnalyzer.RuleId, message);
        }

        #endregion
    }
}