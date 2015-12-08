// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class CallGCSuppressFinalizeCorrectlyTests : DiagnosticAnalyzerTestBase
    {
        private const string GCSuppressFinalizeMethodSignature = "GC.SuppressFinalize(object)";

        private static DiagnosticResult GetCA1816CSharpResultAt(int line, int column, DiagnosticDescriptor rule, string containingMethodName, string gcSuppressFinalizeMethodName)
        {
            return GetCSharpResultAt(line, column, rule, containingMethodName, gcSuppressFinalizeMethodName);
        }

        private static DiagnosticResult GetCA1816BasicResultAt(int line, int column, DiagnosticDescriptor rule, string containingMethodName, string gcSuppressFinalizeMethodName)
        {
            return GetBasicResultAt(line, column, rule, containingMethodName, gcSuppressFinalizeMethodName);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicCallGCSuppressFinalizeCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpCallGCSuppressFinalizeCorrectlyAnalyzer();
        }

        #region CSharpNoDiagnosticCases

        [Fact]
        public void DisposableWithoutFinalizerCSharpNoDiagnostic()
        {

            var code = @"
using System;
using System.ComponentModel;

public class DisposableWithoutFinalizer : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void DisposableWithFinalizerCSharpNoDiagnostic()
        {

            var code = @"
using System;
using System.ComponentModel;

public class DisposableWithFinalizer : IDisposable
{
    ~DisposableWithFinalizer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void SealedDisposableWithoutFinalizerCSharpNoDiagnostic()
        {

            var code = @"
using System;
using System.ComponentModel;

public sealed class SealedDisposableWithoutFinalizer : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void SealedDisposableWithFinalizerCSharpNoDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public sealed class SealedDisposableWithFinalizer : IDisposable
{
    ~SealedDisposableWithFinalizer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void InternalDisposableWithoutFinalizerCSharpNoDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

internal class InternalDisposableWithoutFinalizer : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void PrivateDisposableWithoutFinalizerCSharpNoDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public static class NestedClassHolder
{
    private class PrivateDisposableWithoutFinalizer : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Console.WriteLine(this);
            Console.WriteLine(disposing);
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void SealedDisposableWithoutFinalizerAndWithoutCallingSuppressFinalizeCSharpNoDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public sealed class SealedDisposableWithoutFinalizerAndWithoutCallingSuppressFinalize : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void DisposableStructCSharpNoDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public struct DisposableStruct : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void SealedDisposableCallingGCSuppressFinalizeInConstructorCSharpNoDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public sealed class SealedDisposableCallingGCSuppressFinalizeInConstructor : Component
{
    public SealedDisposableCallingGCSuppressFinalizeInConstructor()
    {
        // We don't ever want our finalizer (that we inherit from Component) to run
        // (We are sealed and we don't own any unmanaged resources).
        GC.SuppressFinalize(this);
    }
}";
            VerifyCSharp(code);
        }

        #endregion

        #region CSharpDiagnosticCases

        [Fact]
        public void SealedDisposableWithFinalizerCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

    public class SealedDisposableWithFinalizer : IDisposable
    {
        public static void Main(string[] args)
        {

        }

        ~SealedDisposableWithFinalizer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Console.WriteLine(this);
            Console.WriteLine(disposing);
        }
    }";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 17,
                column: 21,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotCalledWithFinalizerRule,
                containingMethodName: "SealedDisposableWithFinalizer.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        [Fact]
        public void DisposableWithFinalizerCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public class DisposableWithFinalizer : IDisposable
{
    ~DisposableWithFinalizer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 12,
                column: 17,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotCalledWithFinalizerRule,
                containingMethodName: "DisposableWithFinalizer.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        [Fact]
        public void InternalDisposableWithFinalizerCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

internal class InternalDisposableWithFinalizer : IDisposable
{
    ~InternalDisposableWithFinalizer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 12,
                column: 17,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotCalledWithFinalizerRule,
                containingMethodName: "InternalDisposableWithFinalizer.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        [Fact]
        public void PrivateDisposableWithFinalizerCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public static class NestedClassHolder
{
    private class PrivateDisposableWithFinalizer : IDisposable
    {
        ~PrivateDisposableWithFinalizer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

    protected virtual void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 14,
                column: 21,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotCalledWithFinalizerRule,
                containingMethodName: "NestedClassHolder.PrivateDisposableWithFinalizer.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        [Fact]
        public void DisposableWithoutFinalizerCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public class DisposableWithoutFinalizer : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Console.WriteLine(this);
        Console.WriteLine(disposing);
    }
}";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 7,
                column: 17,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotCalledRule,
                containingMethodName: "DisposableWithoutFinalizer.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        [Fact]
        public void DisposableComponentCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public class DisposableComponent : Component, IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this);
    }
}";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 7,
                column: 17,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotCalledRule,
                containingMethodName: "DisposableComponent.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        [Fact]
        public void NotADisposableClassCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public class NotADisposableClass
{
    public NotADisposableClass()
    {
        GC.SuppressFinalize(this);
    }
}";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 9,
                column: 9,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.OutsideDisposeRule,
                containingMethodName: "NotADisposableClass.NotADisposableClass()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        [Fact]
        public void DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlacesCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public class DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlaces : IDisposable
{
    public DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlaces()
    {
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        Dispose(true);
        CallGCSuppressFinalize();
    }

    private void CallGCSuppressFinalize()
    {
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Console.WriteLine(this);
            GC.SuppressFinalize(this);
        }
    }
}";
            var diagnosticResult1 = GetCA1816CSharpResultAt(
                line: 9,
                column: 9,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.OutsideDisposeRule,
                containingMethodName: "DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlaces.DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlaces()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);
            var diagnosticResult2 = GetCA1816CSharpResultAt(
                line: 12,
                column: 17,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotCalledRule,
                containingMethodName: "DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlaces.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);
            var diagnosticResult3 = GetCA1816CSharpResultAt(
                line: 20,
                column: 9,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.OutsideDisposeRule,
                containingMethodName: "DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlaces.CallGCSuppressFinalize()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);
            var diagnosticResult4 = GetCA1816CSharpResultAt(
                line: 28,
                column: 13,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.OutsideDisposeRule,
                containingMethodName: "DisposableClassThatCallsGCSuppressFinalizeInTheWrongPlaces.Dispose(bool)",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult1, diagnosticResult2, diagnosticResult3, diagnosticResult4);
        }

        [Fact]
        public void DisposableClassThatCallsGCSuppressFinalizeWithTheWrongArgumentsCSharpDiagnostic()
        {
            var code = @"
using System;
using System.ComponentModel;

public class DisposableClassThatCallsGCSuppressFinalizeWithTheWrongArguments : IDisposable
{
    public DisposableClassThatCallsGCSuppressFinalizeWithTheWrongArguments()
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Console.WriteLine(this);
        }
    }
}";
            var diagnosticResult = GetCA1816CSharpResultAt(
                line: 14,
                column: 9,
                rule: CallGCSuppressFinalizeCorrectlyAnalyzer.NotPassedThisRule,
                containingMethodName: "DisposableClassThatCallsGCSuppressFinalizeWithTheWrongArguments.Dispose()",
                gcSuppressFinalizeMethodName: GCSuppressFinalizeMethodSignature);

            VerifyCSharp(code, diagnosticResult);
        }

        #endregion
    }
}
