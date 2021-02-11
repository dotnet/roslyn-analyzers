// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.StaticHolderTypesAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpStaticHolderTypesFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.StaticHolderTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class StaticHolderTypeTests
    {
        #region Verifiers

        private static DiagnosticResult CSharpResult(int line, int column, string objectName)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(objectName);

        private static DiagnosticResult BasicResult(int line, int column, string objectName)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(objectName);

        #endregion

        [Fact]
        public async Task CA1052NoDiagnosticForEmptyNonStaticClassCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C1
{
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForEmptyInheritableClassBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B1
End Class
");
        }

        [Fact]

        public async Task CA1052NoDiagnosticForStaticClassWithOnlyStaticDeclaredMembersCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public static class C2
{
    public static void DoSomething() { }
}
");
        }

        [Fact, WorkItem(1320, "https://github.com/dotnet/roslyn-analyzers/issues/1320")]
        public async Task CA1052NoDiagnosticForSealedClassWithOnlyStaticDeclaredMembersCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public sealed class C3
{
    public static void DoSomething() { }
}
");
        }

        [Fact, WorkItem(1320, "https://github.com/dotnet/roslyn-analyzers/issues/1320")]
        public async Task CA1052NoDiagnosticForNonInheritableClassWithOnlySharedDeclaredMembersBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public NotInheritable Class B3
    Public Shared Sub DoSomething()
    End Sub
End Class
");
        }

        [Fact, WorkItem(1292, "https://github.com/dotnet/roslyn-analyzers/issues/1292")]
        public async Task CA1052NoDiagnosticForSealedClassWithPublicConstructorAndStaticMembers()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading;

public sealed class ConcurrentCreationDummy
{
    private static int creationAttempts;

    public ConcurrentCreationDummy()
    {
        if (IsCreatingFirstInstance())
        {
            CreatingFirstInstance.Set();
            CreatedSecondInstance.Wait();
        }
    }

    public static ManualResetEventSlim CreatingFirstInstance { get; } = new ManualResetEventSlim();

    public static ManualResetEventSlim CreatedSecondInstance { get; } = new ManualResetEventSlim();

    private static bool IsCreatingFirstInstance()
    {
        return Interlocked.Increment(ref creationAttempts) == 1;
    }
}");
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C4
{
    public static void DoSomething() { }
}
",
                CSharpResult(2, 14, "C4"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithOnlySharedDeclaredMembersBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B4
    Public Shared Sub DoSomething()
    End Sub
End Class
",
                BasicResult(2, 14, "B4"));
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithBothStaticAndInstanceDeclaredMembersCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C5
{
    public void Moo() { }
    public static void DoSomething() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithBothSharedAndInstanceDeclaredMembersBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B5
    Public Sub Moo()
    End Sub

    Public Shared Sub DoSomething()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForInternalClassWithOnlyStaticDeclaredMembersCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal class C6
{
    public static void DoSomething() { }
}
");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task CA1052NoDiagnosticForEffectivelyInternalClassWithOnlyStaticDeclaredMembersCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal class C6
{
    public class Inner
    {
        public static void DoSomething() { }
    }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForFriendClassWithOnlySharedDeclaredMembersBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Friend Class B6
    Public Shared Sub DoSomething()
    End Sub
End Class
");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task CA1052NoDiagnosticForEffectivelyFriendClassWithOnlySharedDeclaredMembersBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Friend Class B6
    Public Class InnerClass
        Public Shared Sub DoSomething()
        End Sub
    End Class
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithUserDefinedOperatorCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C7
{
    public static int operator +(C7 a, C7 b)
    {
        return 0;
    }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithUserDefinedOperatorBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B7
    Public Shared Operator +(a As B7, b As B7) As Integer
        Return 0
    End Operator
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithStaticMethodAndUserDefinedOperatorCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C8
{
    public static void DoSomething() { }

    public static int operator +(C8 a, C8 b)
    {
        return 0;
    }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithSharedMethodAndUserDefinedOperatorBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B8
    Public Shared Sub DoSomething()
    End Sub

    Public Shared Operator +(a As B8, b As B8) As Integer
        Return 0
    End Operator
End Class
");
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithPublicDefaultConstructorAndStaticMethodCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C9
{
    public C9() { }

    public static void DoSomething() { }
}
",
            CSharpResult(2, 14, "C9"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithPublicDefaultConstructorAndSharedMethodBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B9
    Public Sub New()
    End Sub

    Public Shared Sub DoSomething()
    End Sub
End Class
",
            BasicResult(2, 14, "B9"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithProtectedDefaultConstructorAndStaticMethodCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C10
{
    protected C10() { }

    public static void DoSomething() { }
}
",
            CSharpResult(2, 14, "C10"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithProtectedDefaultConstructorAndSharedMethodBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B10
    Protected Sub New()
    End Sub

    Public Shared Sub DoSomething()
    End Sub
End Class
",
            BasicResult(2, 14, "B10"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithPrivateDefaultConstructorAndStaticMethodCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C11
{
    private C11() { }

    public static void DoSomething() { }
}
",
            CSharpResult(2, 14, "C11"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNonStaticClassWithPrivateDefaultConstructorAndSharedMethodBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B11
    Private Sub New()
    End Sub

    Public Shared Sub DoSomething()
    End Sub
End Class
",
            BasicResult(2, 14, "B11"));
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithPublicNonDefaultConstructorAndStaticMethodCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C12
{
    public C12(int i) { }

    public static void DoSomething() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithPublicNonDefaultConstructorAndSharedMethodBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B12
    Public Sub New(i as Integer)
    End Sub

    Public Shared Sub DoSomething()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithPublicNonDefaultConstructorWithDefaultedParametersAndStaticMethodCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C13
{
    public C13(int i = 0, string s = """") { }

    public static void DoSomething() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithPublicNonDefaultConstructorWithOptionalParametersAndSharedMethodBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B13
    Public Sub New(Optional i as Integer = 0, Optional s as String = """")
    End Sub

    Public Shared Sub DoSomething()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052DiagnosticForNestedPublicNonStaticClassWithPublicDefaultConstructorAndStaticMethodCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C14
{
    public void Moo() { }

    public class C14Inner
    {
        public C14Inner() { }
        public static void DoSomething() { }
    }
}
",
                CSharpResult(6, 18, "C14Inner"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNestedPublicNonStaticClassWithPublicDefaultConstructorAndSharedMethodBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B14
    Public Sub Moo()
    End Sub

    Public Class B14Inner
        Public Sub New()
        End Sub

        Public Shared Sub DoSomething()
        End Sub
    End Class
End Class
",
                BasicResult(6, 18, "B14Inner"));
        }

        [Fact]
        public async Task CA1052NoDiagnosticForEmptyStaticClassCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public static class C15
{
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithStaticConstructorCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C16
{
    static C16() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithStaticConstructorBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B16
    Shared Sub New()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForStaticClassWithStaticConstructorCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public static class C17
{
    static C17() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithStaticConstructorAndInstanceConstructorCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C18
{
    public C18() { }
    static C18() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithStaticConstructorAndInstanceConstructorBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B18
    Sub New()
    End Sub

    Shared Sub New()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052DiagnosticForNestedPublicClassInOtherwiseEmptyNonStaticClassCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C19
{
    public class C19Inner
    {
    }
}
",
                CSharpResult(2, 14, "C19"));
        }

        [Fact]
        public async Task CA1052DiagnosticForNestedPublicClassInOtherwiseEmptyNonStaticClassBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B19
    Public Class B19Inner
    End Class
End Class
",
                BasicResult(2, 14, "B19"));
        }

        [Fact]
        public async Task CA1052NoDiagnosticAnEnumCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public enum E20
{
    Unknown = 0
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticAnEnumBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Enum EB20
    Unknown = 0
End Enum
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticOnClassWithOnlyDefaultConstructorCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C21
{
    public C21() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticOnClassWithOnlyDefaultConstructorBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B21
    Public Sub New()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNestedPrivateNonStaticClassWithPublicDefaultConstructorAndStaticMethodCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C22
{
    public void Moo() { }

    private class C22Inner
    {
        public C22Inner() { }
        public static void DoSomething() { }
    }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNestedPrivateNonStaticClassWithPublicDefaultConstructorAndSharedMethodBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B22
    Public Sub Moo()
    End Sub

    Private Class B22Inner
        Public Sub New()
        End Sub

        Public Shared Sub DoSomething()
        End Sub
    End Class
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndBaseClassCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C23Base
{
}
public class C23 : C23Base
{
    public static void DoSomething() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndBaseClassBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B23Base
End Class
Public Class B23
	Inherits B23Base
	Public Shared Sub DoSomething()
	End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndEmptyBaseInterfaceCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public interface IC24Base
{
}
public class C24 : IC24Base
{
    public static void DoSomething() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndEmptyBaseInterfaceBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Interface IB24Base
End Interface
Public Class B24
	Implements IB24Base
	Public Shared Sub DoSomething()
	End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndNotEmptyBaseInterfaceCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public interface IC25Base
{
    void Moo();
}
public class C25 : IC25Base
{
    public static void DoSomething() { }
    void IC25Base.Moo() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndNotEmptyBaseInterfaceBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Interface IB25Base
    Sub Moo()
End Interface
Public Class B25
	Implements IB25Base
	Public Shared Sub DoSomething()
	End Sub
	Private Sub B25Base_Moo() Implements IB25Base.Moo
	End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndIncompleteBaseClassDefinitionCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C26 :{|CS1031:|}
{
    public static void DoSomething() { }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyStaticDeclaredMembersAndIncompleteBaseClassDefinitionBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B26
	Inherits{|BC30182:|}
	Public Shared Sub DoSomething()
	End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForEmptyNonStaticClassWithIncompleteBaseClassDefinitionCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C27 :{|CS1031:|}
{
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForEmptyNonStaticClassWithIncompleteBaseClassDefinitionBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B27
	Inherits{|BC30182:|}
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyPrivateAndProtectedStaticMethodsCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C28
{
    private static void SomeMethod() {}
    protected static void SomeOtherMethod() {}
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyPrivateAndProtectedStaticMethodsBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B28
	Private Shared Sub SomeMethod()
	End Sub
	Protected Shared Sub SomeOtherMethod()
	End Sub
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyExplicitConversionOperatorsCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C29
{
    public static explicit operator C29(int p) => new C29();
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyImplicitConversionOperatorsCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C29
{
    public static implicit operator C29(int p) => new C29();
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForNonStaticClassWithOnlyExplicitConversionOperatorsBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class B29
    Public Shared Widening Operator CType(ByVal p As Integer) As B29
        Return New B29()
    End Operator
End Class
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticForAbstractNonStaticClassCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public abstract class C1
{
    internal class C2 : C1 {
    }
}
");
        }

        [Fact]
        public async Task CA1052NoDiagnosticRecords()
        {
            await new VerifyCS.Test
            {
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestCode = @"
public record C
{
    public static void M() { }
}
"
            }.RunAsync();
        }
    }
}
