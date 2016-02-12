// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;

using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class AvoidUninstantiatedInternalClassesTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void CSharpDiagnosticForUninstantiatedInternalClass()
        {
            VerifyCSharp(@"
internal class C { }
",
                GetCSharpResultAt(2, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void BasicDiagnosticForUninstantiatedInternalClass()
        {
            VerifyBasic(@"
Friend Class C
End Class
",
                GetBasicResultAt(2, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CSharpNoDiagnosticForUninstantiatedInternalStruct()
        {
            VerifyCSharp(@"
internal struct CInternal { }
");
        }

        [Fact]
        public void BasicNoDiagnosticForUninstantiatedInternalStruct()
        {
            VerifyBasic(@"
Friend Structure CInternal
End Structure
");
        }

        [Fact]
        public void CSharpNoDiagnosticForUninstantiatedPublicClass()
        {
            VerifyCSharp(@"
public class C { }
");
        }

        [Fact]
        public void BasicNoDiagnosticForUninstantiatedPublicClass()
        {
            VerifyBasic(@"
Public Class C
End Class
");
        }

        [Fact]
        public void CSharpNoDiagnosticForInstantiatedInternalClass()
        {
            VerifyCSharp(@"
internal class C { }

public class D
{
    private readonly C _c = new C();
}
");
        }

        [Fact]
        public void BasicNoDiagnosticForInstantiatedInternalClass()
        {
            VerifyBasic(@"
Friend Class B
End Class

Public Class D
     Private _b As New B
End Class
");
        }

        [Fact]
        public void CSharpDiagnosticForUninstantiatedInternalClassNestedInPublicClass()
        {
            VerifyCSharp(@"
public class C
{
    internal class D { }
}
",
                GetCSharpResultAt(4, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void BasicDiagnosticForUninstantiatedInternalClassNestedInPublicClass()
        {
            VerifyBasic(@"
Public Class C
    Friend Class D
    End Class
End Class
",
                GetBasicResultAt(3, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void CSharpNoDiagnosticForInstantiatedInternalClassNestedInPublicClass()
        {
            VerifyCSharp(@"
public class C
{
    private readonly D _d = new D();

    internal class D { }
}
");
        }

        [Fact]
        public void BasicNoDiagnosticForInstantiatedInternalClassNestedInPublicClass()
        {
            VerifyBasic(@"
Public Class C
    Private ReadOnly _d = New D

    Friend Class D
    End Class
End Class
");
        }

        [Fact]
        public void CSharpNoDiagnosticForInternalStaticClass()
        {
            VerifyCSharp(@"
internal static class S { }
");
        }

        [Fact]
        public void BasicNoDiagnosticForInternalModule()
        {
            // No static classes in VB.
            VerifyBasic(@"
Friend Module M
End Module
");
        }

        [Fact]
        public void CSharpNoDiagnosticForInternalAbstractClass()
        {
            VerifyCSharp(@"
internal abstract class A { }
");
        }

        [Fact]
        public void BasicNoDiagnosticForInternalAbstractClass()
        {
            VerifyBasic(@"
Friend MustInherit Class A
End Class
");
        }

        // The following tests are just to ensure that the messages are formatted properly
        // for types within namespaces.
        [Fact]
        public void CSharpDiagnosticForUninstantiatedInternalClassInNamespace()
        {
            VerifyCSharp(@"
namespace N
{
    internal class C { }
}
",
                GetCSharpResultAt(4, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void BasicDiagnosticForUninstantiatedInternalClassInNamespace()
        {
            VerifyBasic(@"
Namespace N
    Friend Class C
    End Class
End Namespace
",
                GetBasicResultAt(3, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CSharpDiagnosticForUninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            VerifyCSharp(@"
namespace N
{
    public class C
    {
        internal class D { }
    }
}
",
                GetCSharpResultAt(6, 24, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void BasicDiagnosticForUninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            VerifyBasic(@"
Namespace N
    Public Class C
        Friend Class D
        End Class
    End Class
End Namespace
",
                GetBasicResultAt(4, 22, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidUninstantiatedInternalClassesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidUninstantiatedInternalClassesAnalyzer();
        }
    }
}