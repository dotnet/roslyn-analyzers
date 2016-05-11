﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.UnitTests
{
    public class IdentifiersShouldHaveCorrectPrefixTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new IdentifiersShouldHaveCorrectPrefixAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new IdentifiersShouldHaveCorrectPrefixAnalyzer();
        }

        [Fact]
        public void TestInterfaceNamesCSharp()
        {
            VerifyCSharp(@"
public interface Controller
{
    void SomeMethod();
}

public interface 日本語
{
    void SomeMethod();
}

public interface _Controller
{
    void SomeMethod();
}

public interface _日本語
{
    void SomeMethod();
}

public interface Internet
{
    void SomeMethod();
}

public interface Iinternet
{
    void SomeMethod();
}

public class Class1
{
    public interface Controller
    {
        void SomeMethod();
    }
}

public interface IAmAnInterface
{
    void SomeMethod();
}
",
                GetCA1715CSharpResultAt(2, 18, CA1715InterfaceMessage, "Controller"),
                GetCA1715CSharpResultAt(7, 18, CA1715InterfaceMessage, "\u65E5\u672C\u8A9E"),
                GetCA1715CSharpResultAt(12, 18, CA1715InterfaceMessage, "_Controller"),
                GetCA1715CSharpResultAt(17, 18, CA1715InterfaceMessage, "_\u65E5\u672C\u8A9E"),
                GetCA1715CSharpResultAt(22, 18, CA1715InterfaceMessage, "Internet"),
                GetCA1715CSharpResultAt(27, 18, CA1715InterfaceMessage, "Iinternet"),
                GetCA1715CSharpResultAt(34, 22, CA1715InterfaceMessage, "Controller"));
        }

        [Fact]
        public void TestTypeParameterNamesCSharp()
        {
            VerifyCSharp(@"
using System;

public class IInterface<VSome>
{
}

public class IAnotherInterface<本語>
{
}

public delegate void Callback<VSome>();

public class Class2<VSome>
{
}

public class Class2<T, VSome>
{
}

public class Class3<Type>
{
}

public class Class3<T, Type>
{
}

public class Base<Key, Value>
{
}

public class Derived<Key, Value> : Base<Key, Value>
{
}

public class Class4<Type1>
{
    public void AnotherMethod<Type2>()
    {
        Console.WriteLine(typeof(Type2).ToString());
    }

    public void Method<Type2>(Type2 type)
    {
        Console.WriteLine(type);
    }

    public void Method<KType, VType>(KType key, VType value)
    {
        Console.WriteLine(key.ToString() + value.ToString());
    }
}

public class Class5<_Type1>
{
    public void Method<_K, _V>(_K key, _V value)
    {
        Console.WriteLine(key.ToString() + value.ToString());
    }
}

public class Class6<TTypeParameter>
{
}
",
                GetCA1715CSharpResultAt(4, 25, CA1715TypeParameterMessage, "VSome"),
                GetCA1715CSharpResultAt(8, 32, CA1715TypeParameterMessage, "\u672C\u8A9E"),
                GetCA1715CSharpResultAt(12, 31, CA1715TypeParameterMessage, "VSome"),
                GetCA1715CSharpResultAt(14, 21, CA1715TypeParameterMessage, "VSome"),
                GetCA1715CSharpResultAt(18, 24, CA1715TypeParameterMessage, "VSome"),
                GetCA1715CSharpResultAt(22, 21, CA1715TypeParameterMessage, "Type"),
                GetCA1715CSharpResultAt(26, 24, CA1715TypeParameterMessage, "Type"),
                GetCA1715CSharpResultAt(30, 19, CA1715TypeParameterMessage, "Key"),
                GetCA1715CSharpResultAt(30, 24, CA1715TypeParameterMessage, "Value"),
                GetCA1715CSharpResultAt(34, 22, CA1715TypeParameterMessage, "Key"),
                GetCA1715CSharpResultAt(34, 27, CA1715TypeParameterMessage, "Value"),
                GetCA1715CSharpResultAt(38, 21, CA1715TypeParameterMessage, "Type1"),
                GetCA1715CSharpResultAt(40, 31, CA1715TypeParameterMessage, "Type2"),
                GetCA1715CSharpResultAt(45, 24, CA1715TypeParameterMessage, "Type2"),
                GetCA1715CSharpResultAt(50, 24, CA1715TypeParameterMessage, "KType"),
                GetCA1715CSharpResultAt(50, 31, CA1715TypeParameterMessage, "VType"),
                GetCA1715CSharpResultAt(56, 21, CA1715TypeParameterMessage, "_Type1"),
                GetCA1715CSharpResultAt(58, 24, CA1715TypeParameterMessage, "_K"),
                GetCA1715CSharpResultAt(58, 28, CA1715TypeParameterMessage, "_V"));
        }

        [Fact]
        public void TestTypeParameterNamesCSharp_NoDiagnosticCases()
        {
            VerifyCSharp(@"
using System;

public class IInterface<V>
{
}

public delegate void Callback<V>();

public class Class2<T, V>
{
}

public class Class4<T>
{
    public void Method<K, V>(K key, V value)
    {
        Console.WriteLine(key.ToString() + value.ToString());
    }
}

public class Class6<TTypeParameter>
{
}
");
        }

        [Fact]
        public void TestInterfaceNamesBasic()
        {
            VerifyBasic(@"
Public Interface Controller
    Sub SomeMethod()
End Interface

Public Interface 日本語
    Sub SomeMethod()
End Interface

Public Interface _Controller
    Sub SomeMethod()
End Interface

Public Interface _日本語
    Sub SomeMethod()
End Interface

Public Interface Internet
    Sub SomeMethod()
End Interface

Public Interface Iinternet
    Sub SomeMethod()
End Interface

Public Class Class1
    Public Interface Controller
        Sub SomeMethod()
    End Interface
End Class

Public Interface IAmAnInterface
    Sub SomeMethod()
End Interface
",
                GetCA1715BasicResultAt(2, 18, CA1715InterfaceMessage, "Controller"),
                GetCA1715BasicResultAt(6, 18, CA1715InterfaceMessage, "\u65E5\u672C\u8A9E"),
                GetCA1715BasicResultAt(10, 18, CA1715InterfaceMessage, "_Controller"),
                GetCA1715BasicResultAt(14, 18, CA1715InterfaceMessage, "_\u65E5\u672C\u8A9E"),
                GetCA1715BasicResultAt(18, 18, CA1715InterfaceMessage, "Internet"),
                GetCA1715BasicResultAt(22, 18, CA1715InterfaceMessage, "Iinternet"),
                GetCA1715BasicResultAt(27, 22, CA1715InterfaceMessage, "Controller"));
        }

        [Fact]
        public void TestTypeParameterNamesBasic()
        {
            VerifyBasic(@"
Import System

Public Class IInterface(Of VSome)
End Class

Public Class IAnotherInterface(Of 本語)
End Class

Public Delegate Sub Callback(Of VSome)()

Public Class Class2(Of VSome)
End Class

Public Class Class2(Of T, VSome)
End Class

Public Class Class3(Of Type)
End Class

Public Class Class3(Of T, Type)
End Class

Public Class Base(Of Key, Value)
End Class

Public Class Derived(Of Key, Value)
    Inherits Base(Of Key, Value)
End Class

Public Class Class4(Of Type1)
    Public Sub AnotherMethod(Of Type2)()
        Console.WriteLine(GetType(Type2).ToString())
    End Sub

    Public Sub Method(Of Type2)(type As Type2)
        Console.WriteLine(type)
    End Sub

    Public Sub Method(Of KType, VType)(key As KType, value As VType)
        Console.WriteLine(key.ToString() + value.ToString())
    End Sub
End Class

Public Class Class5(Of _Type1)
    Public Sub Method(Of _K, _V)(key As _K, value As _V)
        Console.WriteLine(key.ToString() + value.ToString())
    End Sub
End Class

Public Class Class6(Of TTypeParameter)
End Class
",
                GetCA1715BasicResultAt(4, 28, CA1715TypeParameterMessage, "VSome"),
                GetCA1715BasicResultAt(7, 35, CA1715TypeParameterMessage, "\u672C\u8A9E"),
                GetCA1715BasicResultAt(10, 33, CA1715TypeParameterMessage, "VSome"),
                GetCA1715BasicResultAt(12, 24, CA1715TypeParameterMessage, "VSome"),
                GetCA1715BasicResultAt(15, 27, CA1715TypeParameterMessage, "VSome"),
                GetCA1715BasicResultAt(18, 24, CA1715TypeParameterMessage, "Type"),
                GetCA1715BasicResultAt(21, 27, CA1715TypeParameterMessage, "Type"),
                GetCA1715BasicResultAt(24, 22, CA1715TypeParameterMessage, "Key"),
                GetCA1715BasicResultAt(24, 27, CA1715TypeParameterMessage, "Value"),
                GetCA1715BasicResultAt(27, 25, CA1715TypeParameterMessage, "Key"),
                GetCA1715BasicResultAt(27, 30, CA1715TypeParameterMessage, "Value"),
                GetCA1715BasicResultAt(31, 24, CA1715TypeParameterMessage, "Type1"),
                GetCA1715BasicResultAt(32, 33, CA1715TypeParameterMessage, "Type2"),
                GetCA1715BasicResultAt(36, 26, CA1715TypeParameterMessage, "Type2"),
                GetCA1715BasicResultAt(40, 26, CA1715TypeParameterMessage, "KType"),
                GetCA1715BasicResultAt(40, 33, CA1715TypeParameterMessage, "VType"),
                GetCA1715BasicResultAt(45, 24, CA1715TypeParameterMessage, "_Type1"),
                GetCA1715BasicResultAt(46, 26, CA1715TypeParameterMessage, "_K"),
                GetCA1715BasicResultAt(46, 30, CA1715TypeParameterMessage, "_V"));
        }

        [Fact]
        public void TestTypeParameterNamesBasic_NoDiagnosticCases()
        {
            VerifyBasic(@"
Import System

Public Class IInterface(Of V)
End Class

Public Delegate Sub Callback(Of V)()

Public Class Class2(Of T, V)
End Class

Public Class Class4(Of T)
    Public Sub Method(Of K, V)(key As K, value As V)
        Console.WriteLine(key.ToString() + value.ToString())
    End Sub
End Class

Public Class Class6(Of TTypeParameter)
End Class
");
        }

        internal static readonly string CA1715InterfaceMessage = MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectPrefixMessageInterface;
        internal static readonly string CA1715TypeParameterMessage = MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectPrefixMessageTypeParameter;

        private static DiagnosticResult GetCA1715CSharpResultAt(int line, int column, string message, string name)
        {
            return GetCSharpResultAt(line, column, IdentifiersShouldHaveCorrectPrefixAnalyzer.RuleId, string.Format(message, name));
        }

        private static DiagnosticResult GetCA1715BasicResultAt(int line, int column, string message, string name)
        {
            return GetBasicResultAt(line, column, IdentifiersShouldHaveCorrectPrefixAnalyzer.RuleId, string.Format(message, name));
        }
    }
}