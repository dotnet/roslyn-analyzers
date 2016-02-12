// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class ProvideCorrectArgumentsToFormattingMethodsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ProvideCorrectArgumentsToFormattingMethodsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ProvideCorrectArgumentsToFormattingMethodsAnalyzer();
        }

        #region Diagnostic Tests

        [Fact]
        public void CA2241CSharpString()
        {
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = String.Format("""", 1);
        var s = String.Format(""{0}"", 1, 2);
        var s = String.Format(""{0} {1}"", 1, 2, 3);
        var s = String.Format(""{0} {1} {2}"", 1, 2, 3, 4);

        IFormatProvider p = null;
        var s = String.Format(p, """", 1);
        var s = String.Format(p, ""{0}"", 1, 2);
        var s = String.Format(p, ""{0} {1}"", 1, 2, 3);
        var s = String.Format(p, ""{0} {1} {2}"", 1, 2, 3, 4);
    }
}
",
            GetCA2241CSharpResultAt(8, 17),
            GetCA2241CSharpResultAt(9, 17),
            GetCA2241CSharpResultAt(10, 17),
            GetCA2241CSharpResultAt(11, 17),

            GetCA2241CSharpResultAt(14, 17),
            GetCA2241CSharpResultAt(15, 17),
            GetCA2241CSharpResultAt(16, 17),
            GetCA2241CSharpResultAt(17, 17));
        }

        [Fact]
        public void CA2241CSharpConsoleWrite()
        {
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = Console.Write("""", 1);
        var s = Console.Write(""{0}"", 1, 2);
        var s = Console.Write(""{0} {1}"", 1, 2, 3);
        var s = Console.Write(""{0} {1} {2}"", 1, 2, 3, 4);
        var s = Console.Write(""{0} {1} {2} {3}"", 1, 2, 3, 4, 5);
    }
}
",
            GetCA2241CSharpResultAt(8, 17),
            GetCA2241CSharpResultAt(9, 17),
            GetCA2241CSharpResultAt(10, 17),
            GetCA2241CSharpResultAt(11, 17),
            GetCA2241CSharpResultAt(12, 17));
        }

        [Fact]
        public void CA2241CSharpConsoleWriteLine()
        {
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = Console.WriteLine("""", 1);
        var s = Console.WriteLine(""{0}"", 1, 2);
        var s = Console.WriteLine(""{0} {1}"", 1, 2, 3);
        var s = Console.WriteLine(""{0} {1} {2}"", 1, 2, 3, 4);
        var s = Console.WriteLine(""{0} {1} {2} {3}"", 1, 2, 3, 4, 5);
    }
}
",
            GetCA2241CSharpResultAt(8, 17),
            GetCA2241CSharpResultAt(9, 17),
            GetCA2241CSharpResultAt(10, 17),
            GetCA2241CSharpResultAt(11, 17),
            GetCA2241CSharpResultAt(12, 17));
        }

        [Fact]
        public void CA2241CSharpPassing()
        {
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = String.Format(""{0}"", 1);
        var s = String.Format(""{0} {1}"", 1, 2);
        var s = String.Format(""{0} {1} {2}"", 1, 2, 3);
        var s = String.Format(""{0} {1} {2} {3}"", 1, 2, 3, 4);

        var s = Console.Write(""{0}"", 1);
        var s = Console.Write(""{0} {1}"", 1, 2);
        var s = Console.Write(""{0} {1} {2}"", 1, 2, 3);
        var s = Console.Write(""{0} {1} {2} {3}"", 1, 2, 3, 4);
        var s = Console.Write(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, 5);

        var s = Console.WriteLine(""{0}"", 1);
        var s = Console.WriteLine(""{0} {1}"", 1, 2);
        var s = Console.WriteLine(""{0} {1} {2}"", 1, 2, 3);
        var s = Console.WriteLine(""{0} {1} {2} {3}"", 1, 2, 3, 4);
        var s = Console.WriteLine(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, 5);
    }
}
");
        }

        [Fact]
        public void CA2241CSharpExplicitObjectArrayNotSupported()
        {
            // currently not supported due to "https://github.com/dotnet/roslyn/issues/7342"
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = String.Format(""{0} {1} {2} {3}"", new object[] {1, 2});
        var s = Console.Write(""{0} {1} {2} {3}"", new object[] {1, 2, 3, 4, 5});
        var s = Console.WriteLine(""{0} {1} {2} {3}"", new object[] {1, 2, 3, 4, 5});
    }
}
");
        }

        [Fact]
        public void CA2241CSharpVarArgsNotSupported()
        {
            // currently not supported due to "https://github.com/dotnet/roslyn/issues/7346"
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = Console.Write(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, _arglist(5));
        var s = Console.WriteLine(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, _arglist(5));
    }
}
");
        }

        [Fact]
        public void CA2241VBString()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Sub Method()
        Dim s = String.Format("""", 1)
        Dim s = String.Format(""{0}"", 1, 2)
        Dim s = String.Format(""{0} {1}"", 1, 2, 3)
        Dim s = String.Format(""{0} {1} {2}"", 1, 2, 3, 4)

        Dim p as IFormatProvider = Nothing
        Dim s = String.Format(p, """", 1)
        Dim s = String.Format(p, ""{0}"", 1, 2)
        Dim s = String.Format(p, ""{0} {1}"", 1, 2, 3)
        Dim s = String.Format(p, ""{0} {1} {2}"", 1, 2, 3, 4)
    End Sub
End Class
",
            GetCA2241BasicResultAt(6, 17),
            GetCA2241BasicResultAt(7, 17),
            GetCA2241BasicResultAt(8, 17),
            GetCA2241BasicResultAt(9, 17),

            GetCA2241BasicResultAt(12, 17),
            GetCA2241BasicResultAt(13, 17),
            GetCA2241BasicResultAt(14, 17),
            GetCA2241BasicResultAt(15, 17));
        }

        [Fact]
        public void CA2241VBConsoleWrite()
        {
            // this works in VB
            // Dim s = Console.WriteLine(""{0} {1} {2}"", 1, 2, 3, 4)
            // since VB bind it to _arglist version where we skip analysis
            // due to a bug - https://github.com/dotnet/roslyn/issues/7346
            // we might skip it only in C# since VB doesnt support __arglist
            VerifyBasic(@"
Imports System

Public Class C
    Sub Method()
        Dim s = Console.Write("""", 1)
        Dim s = Console.Write(""{0}"", 1, 2)
        Dim s = Console.Write(""{0} {1}"", 1, 2, 3)
        Dim s = Console.Write(""{0} {1} {2}"", 1, 2, 3, 4)
        Dim s = Console.Write(""{0} {1} {2} {3}"", 1, 2, 3, 4, 5)
    End Sub
End Class
",
            GetCA2241BasicResultAt(6, 17),
            GetCA2241BasicResultAt(7, 17),
            GetCA2241BasicResultAt(8, 17),
            GetCA2241BasicResultAt(10, 17));
        }

        [Fact]
        public void CA2241VBConsoleWriteLine()
        {
            // this works in VB
            // Dim s = Console.WriteLine(""{0} {1} {2}"", 1, 2, 3, 4)
            // since VB bind it to _arglist version where we skip analysis
            // due to a bug - https://github.com/dotnet/roslyn/issues/7346
            // we might skip it only in C# since VB doesnt support __arglist
            VerifyBasic(@"
Imports System

Public Class C
    Sub Method()
        Dim s = Console.WriteLine("""", 1)
        Dim s = Console.WriteLine(""{0}"", 1, 2)
        Dim s = Console.WriteLine(""{0} {1}"", 1, 2, 3)
        Dim s = Console.WriteLine(""{0} {1} {2}"", 1, 2, 3, 4)
        Dim s = Console.WriteLine(""{0} {1} {2} {3}"", 1, 2, 3, 4, 5)
    End Sub
End Class
",
            GetCA2241BasicResultAt(6, 17),
            GetCA2241BasicResultAt(7, 17),
            GetCA2241BasicResultAt(8, 17),
            GetCA2241BasicResultAt(10, 17));
        }

        [Fact]
        public void CA2241VBPassing()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Sub Method()
        Dim s = String.Format(""{0}"", 1)
        Dim s = String.Format(""{0} {1}"", 1, 2)
        Dim s = String.Format(""{0} {1} {2}"", 1, 2, 3)
        Dim s = String.Format(""{0} {1} {2} {3}"", 1, 2, 3, 4)

        Dim s = Console.Write(""{0}"", 1)
        Dim s = Console.Write(""{0} {1}"", 1, 2)
        Dim s = Console.Write(""{0} {1} {2}"", 1, 2, 3)
        Dim s = Console.Write(""{0} {1} {2} {3}"", 1, 2, 3, 4)
        Dim s = Console.Write(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, 5)

        Dim s = Console.WriteLine(""{0}"", 1)
        Dim s = Console.WriteLine(""{0} {1}"", 1, 2)
        Dim s = Console.WriteLine(""{0} {1} {2}"", 1, 2, 3)
        Dim s = Console.WriteLine(""{0} {1} {2} {3}"", 1, 2, 3, 4)
        Dim s = Console.WriteLine(""{0} {1} {2} {3} {4}"", 1, 2, 3, 4, 5)
    End Sub
End Class
");
        }

        [Fact]
        public void CA2241VBExplicitObjectArraySupported()
        {
            VerifyBasic(@"
Imports System

Public Class C
    Sub Method()
        Dim s = String.Format(""{0} {1} {2} {3}"", new object[] {1, 2})
        Dim s = Console.Write(""{0} {1} {2} {3}"", new object[] {1, 2, 3, 4, 5})
        Dim s = Console.WriteLine(""{0} {1} {2} {3}"", new object[] {1, 2, 3, 4, 5})
    End Sub
End Class
",
            GetCA2241BasicResultAt(6, 17),
            GetCA2241BasicResultAt(7, 17),
            GetCA2241BasicResultAt(8, 17));
        }

        [Fact]
        public void CA2241CSharpFormatStringParser()
        {
            VerifyCSharp(@"
using System;

public class C
{
    void Method()
    {
        var s = String.Format(""{0,-4 :xd}"", 1);
        var s = String.Format(""{0   ,    5 : d} {1}"", 1, 2);
        var s = String.Format(""{0:d} {1} {2}"", 1, 2, 3);
        var s = String.Format(""{0, 5} {1} {2} {3}"", 1, 2, 3, 4);

        var s = Console.Write(""{0,1}"", 1);
        var s = Console.Write(""{0:   x} {1}"", 1, 2);
        var s = Console.Write(""{{escape}}{0} {1} {2}"", 1, 2, 3);
        var s = Console.Write(""{0: {{escape}} x} {1} {2} {3}"", 1, 2, 3, 4);
        var s = Console.Write(""{0 , -10  :   {{escape}}  y} {1} {2} {3} {4}"", 1, 2, 3, 4, 5);
    }
}
");
        }

        #endregion

        internal static readonly string CA2241Name = "CA2241";

        private static DiagnosticResult GetCA2241CSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA2241Name, SystemRuntimeAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsMessage);
        }

        private static DiagnosticResult GetCA2241BasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA2241Name, SystemRuntimeAnalyzersResources.ProvideCorrectArgumentsToFormattingMethodsMessage);
        }
    }
}