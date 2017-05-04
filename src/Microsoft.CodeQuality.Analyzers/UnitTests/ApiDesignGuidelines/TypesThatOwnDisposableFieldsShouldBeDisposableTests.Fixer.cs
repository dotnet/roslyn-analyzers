﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public partial class TypesThatOwnDisposableFieldsShouldBeDisposableFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicTypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new TypesThatOwnDisposableFieldsShouldBeDisposableFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpTypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TypesThatOwnDisposableFieldsShouldBeDisposableFixer();
        }

        [Fact]
        public void CA1001CSharpCodeFixNoDispose()
        {
            VerifyCSharpFix(@"
using System;
using System.IO;

// This class violates the rule.
public class NoDisposeClass
{
    FileStream newFile;

    public NoDisposeClass()
    {
        newFile = new FileStream("""", FileMode.Append);
    }
}
",
@"
using System;
using System.IO;

// This class violates the rule.
public class NoDisposeClass : IDisposable
{
    FileStream newFile;

    public NoDisposeClass()
    {
        newFile = new FileStream("""", FileMode.Append);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact]
        public void CA1001BasicCodeFixNoDispose()
        {
            VerifyBasicFix(@"
Imports System
Imports System.IO

' This class violates the rule. 
Public Class NoDisposeMethod

    Dim newFile As FileStream

    Sub New()
        newFile = New FileStream("""", FileMode.Append)
    End Sub

End Class
",
@"
Imports System
Imports System.IO

' This class violates the rule. 
Public Class NoDisposeMethod
    Implements IDisposable

    Dim newFile As FileStream

    Sub New()
        newFile = New FileStream("""", FileMode.Append)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class
");
        }

        [Fact]
        public void CA1001CSharpCodeFixHasDispose()
        {
            VerifyCSharpFix(@"
using System;
using System.IO;

// This class violates the rule.
public class NoDisposeClass
{
    FileStream newFile = new FileStream("""", FileMode.Append);

    void Dispose() {
// Some content
}
}
",
@"
using System;
using System.IO;

// This class violates the rule.
public class NoDisposeClass : IDisposable
{
    FileStream newFile = new FileStream("""", FileMode.Append);

    public void Dispose() {
// Some content
}
}
");
        }

        [Fact]
        public void CA1001CSharpCodeFixHasWrongDispose()
        {
            VerifyCSharpFix(@"
using System;
using System.IO;

// This class violates the rule.
public partial class NoDisposeClass
{
    FileStream newFile = new FileStream("""", FileMode.Append);

    void Dispose(int x) {
// Some content
}
}
",
@"
using System;
using System.IO;

// This class violates the rule.
public partial class NoDisposeClass : IDisposable
{
    FileStream newFile = new FileStream("""", FileMode.Append);

    void Dispose(int x) {
// Some content
}

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact]
        public void CA1001BasicCodeFixHasDispose()
        {
            VerifyBasicFix(@"
Imports System
Imports System.IO

' This class violates the rule. 
Public Class NoDisposeMethod

    Dim newFile As FileStream = New FileStream("""", FileMode.Append)

    Sub Dispose()

    End Sub
End Class
",
@"
Imports System
Imports System.IO

' This class violates the rule. 
Public Class NoDisposeMethod
    Implements IDisposable

    Dim newFile As FileStream = New FileStream("""", FileMode.Append)

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class
");
        }

        [Fact]
        public void CA1001BasicCodeFixHasWrongDispose()
        {
            VerifyBasicFix(@"
Imports System
Imports System.IO

' This class violates the rule. 
Public Class NoDisposeMethod

    Dim newFile As FileStream = New FileStream("""", FileMode.Append)

    Sub Dispose(x As Integer)
    End Sub
End Class
",
@"
Imports System
Imports System.IO

' This class violates the rule. 
Public Class NoDisposeMethod
    Implements IDisposable

    Dim newFile As FileStream = New FileStream("""", FileMode.Append)

    Sub Dispose(x As Integer)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class
");
        }
    }
}
