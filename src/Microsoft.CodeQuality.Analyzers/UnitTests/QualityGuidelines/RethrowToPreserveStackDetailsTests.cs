// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines.CSharpRethrowToPreserveStackDetailsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines.BasicRethrowToPreserveStackDetailsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public partial class RethrowToPreserveStackDetailsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicRethrowToPreserveStackDetailsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpRethrowToPreserveStackDetailsAnalyzer();
        }

        [Fact]
        public async Task CA2200_NoDiagnosticsForRethrow()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowImplicitly()
    {
        try
        {
            throw new ArithmeticException();
        }
        catch (ArithmeticException e)
        { 
            throw;
        }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()
        Try
            Throw New ArithmeticException()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA2200_NoDiagnosticsForThrowAnotherException()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowExplicitly()
    {
        try
        {
            throw new ArithmeticException();
            throw new Exception();
        }
        catch (ArithmeticException e)
        {
            var i = new Exception();
            throw i;
        }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()
        Try
            Throw New ArithmeticException()
            Throw New Exception()
        Catch ex As Exception
            Dim i As New Exception()
            Throw i
        End Try
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA2200_DiagnosticForThrowCaughtException()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowExplicitly()
    {
        try
        {
            ThrowException();
        }
        catch (ArithmeticException e)
        {
            throw e;
        }
    }

    void ThrowException()
    {
        throw new ArithmeticException();
    }
}
",
           GetCA2200CSharpResultAt(14, 13));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()

        Try
            Throw New ArithmeticException()
        Catch e As ArithmeticException
            Throw e
        End Try
    End Sub
End Class
",
            GetCA2200BasicResultAt(9, 13));
        }

        [Fact]
        public async Task CA2200_NoDiagnosticsForThrowCaughtReassignedException()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowExplicitlyReassigned()
    {
        try
        {
            ThrowException();
        }
        catch (SystemException e)
        { 
            e = new ArithmeticException();
            throw e;
        }
    }

    void ThrowException()
    {
        throw new SystemException();
    }
}
");
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()

        Try
            Throw New Exception()
        Catch e As Exception
            e = New ArithmeticException()
            Throw e
        End Try
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA2200_NoDiagnosticsForEmptyBlock()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowExplicitlyReassigned()
    {
        try
        {
            ThrowException();
        }
        catch (SystemException e)
        { 

        }
    }

    void ThrowException()
    {
        throw new SystemException();
    }
}
");
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()

        Try
            Throw New Exception()
        Catch e As Exception

        End Try
    End Sub
End Class
");
        }

        [Fact]
        public void CA2200_NoDiagnosticsForThrowCaughtExceptionInAnotherScope()
        {
            VerifyCSharp(@"
using System;

class Program
{
    void CatchAndRethrowExplicitly()
    {
        try
        {
            ThrowException();
        }
        catch (ArithmeticException e)
        {
            throw e;
        }
    }

    [|void ThrowException()
    {
        throw new ArithmeticException();
    }|]
}
");
        }

        [Fact]
        public void CA2200_SingleDiagnosticForThrowCaughtExceptionInSpecificScope()
        {
            VerifyBasic(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()

        Try
            Throw New ArithmeticException()
        Catch e As ArithmeticException
            Throw e
        [|Catch e As Exception
            Throw e
        End Try|]
    End Sub
End Class
",
            GetCA2200BasicResultAt(11, 13));
        }

        [Fact]
        public async Task CA2200_MultipleDiagnosticsForThrowCaughtExceptionAtMultiplePlaces()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowExplicitly()
    {
        try
        {
            ThrowException();
        }
        catch (ArithmeticException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    void ThrowException()
    {
        throw new ArithmeticException();
    }
}
",
            GetCA2200CSharpResultAt(14, 13),
            GetCA2200CSharpResultAt(18, 13));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()

        Try
            Throw New ArithmeticException()
        Catch e As ArithmeticException
            Throw e
        Catch e As Exception
            Throw e
        End Try
    End Sub
End Class
",
            GetCA2200BasicResultAt(9, 13),
            GetCA2200BasicResultAt(11, 13));
        }

        [Fact]
        public async Task CA2200_DiagnosticForThrowOuterCaughtException()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowExplicitly()
    {
        try
        {
            throw new ArithmeticException();
        }
        catch (ArithmeticException e)
        {
            try
            {
                throw new ArithmeticException();
            }
            catch (ArithmeticException i)
            {
                throw e;
            }
        }
    }
}
",
            GetCA2200CSharpResultAt(20, 17));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()

        Try
            Throw New ArithmeticException()
        Catch e As ArithmeticException
            Try
                Throw New ArithmeticException()
            Catch ex As Exception
                Throw e
            End Try
        End Try
    End Sub
End Class
",
            GetCA2200BasicResultAt(12, 17));
        }

        [Fact]
        public async Task CA2200_NoDiagnosticsForNestingWithCompileErrors()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrowExplicitly()
    {   
        try
        {
            try
            {
                throw new ArithmeticException();
            }
            catch (ArithmeticException e)
            {
                throw;
            }
            catch (ArithmeticException)   // error CS0160: A previous catch clause already catches all exceptions of this or of a super type ('ArithmeticException')
            {
                try
                {
                    throw new ArithmeticException();
                }
                catch (ArithmeticException i)
                {
                    throw e;   // error CS0103: The name 'e' does not exist in the current context
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }
}
",
            new DiagnosticResult("CS0160", CodeAnalysis.DiagnosticSeverity.Error).WithLocation(18, 20),
            new DiagnosticResult("CS0103", CodeAnalysis.DiagnosticSeverity.Error).WithLocation(26, 27));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrowExplicitly()
        Try
            Try
                Throw New ArithmeticException()
            Catch ex As ArithmeticException
                Throw
            Catch i As ArithmeticException
                Try
                    Throw New ArithmeticException()
                Catch e As Exception
                    Throw ex   ' error BC30451: 'ex' is not declared. It may be inaccessible due to its protection level.
                End Try
            End Try
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class
",
            new DiagnosticResult("BC30451", CodeAnalysis.DiagnosticSeverity.Error).WithLocation(14, 27));
        }

        [Fact]
        public async Task CA2200_NoDiagnosticsForCatchWithoutIdentifier()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrow(Exception exception)
    {
        try
        {            
        }
        catch (Exception)
        { 
            var finalException = new InvalidOperationException(""barf"", exception);
            throw finalException;
        }
    }
}
");
        }

        [Fact]
        [WorkItem(2167, "https://github.com/dotnet/roslyn-analyzers/issues/2167")]
        public async Task CA2200_NoDiagnosticsForCatchWithoutArgument()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void CatchAndRethrow(Exception exception)
    {
        try
        {            
        }
        catch
        { 
            var finalException = new InvalidOperationException(""barf"", exception);
            throw finalException;
        }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class Program
    Sub CatchAndRethrow(exception As Exception)
        Try
            
        Catch
            Dim finalException = new InvalidOperationException(""barf"", exception)
            Throw finalException
        End Try
    End Sub
End Class
");
        }

        private static DiagnosticResult GetCA2200BasicResultAt(int line, int column)
            => new DiagnosticResult(RethrowToPreserveStackDetailsAnalyzer.Rule)
                .WithLocation(line, column)
                .WithMessage(MicrosoftCodeQualityAnalyzersResources.RethrowToPreserveStackDetailsMessage);

        private static DiagnosticResult GetCA2200CSharpResultAt(int line, int column)
            => new DiagnosticResult(RethrowToPreserveStackDetailsAnalyzer.Rule)
                .WithLocation(line, column)
                .WithMessage(MicrosoftCodeQualityAnalyzersResources.RethrowToPreserveStackDetailsMessage);
    }
}
