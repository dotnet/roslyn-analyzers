// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class CollectionPropertiesShouldBeReadOnlyTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new CollectionPropertiesShouldBeReadOnlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CollectionPropertiesShouldBeReadOnlyAnalyzer();
        }

        [Fact]
        public void CSharp_CA2227_Test()
        {
            VerifyCSharp(@"
using System;

public class A
{
    public System.Collections.ICollection Col { get; set; }
}
", GetCSharpResultAt(6, 43, CollectionPropertiesShouldBeReadOnlyAnalyzer.RuleId, CollectionPropertiesShouldBeReadOnlyAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void CSharp_CA2227_Test_Internal()
        {
            VerifyCSharp(@"
using System;

internal class A
{
    public System.Collections.ICollection Col { get; set; }
}

public class A2
{
    public System.Collections.ICollection Col { get; private set; }
}

public class A3
{
    internal System.Collections.ICollection Col { get; set; }
}

public class A4
{
    private class A5
    {
        public System.Collections.ICollection Col { get; set; }
    }
}
");
        }

        [Fact]
        public void Basic_CA2227_Test()
        {
            VerifyBasic(@"
Imports System

Public Class A
    Public Property Col As System.Collections.ICollection
End Class
", GetBasicResultAt(5, 21, CollectionPropertiesShouldBeReadOnlyAnalyzer.RuleId, CollectionPropertiesShouldBeReadOnlyAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void Basic_CA2227_Test_Internal()
        {
            VerifyBasic(@"
Imports System

Friend Class A
    Public Property Col As System.Collections.ICollection
End Class

Public Class A2
    Public Property Col As System.Collections.ICollection
        Get
            Return Nothing
        End Get
        Private Set(value As System.Collections.ICollection)
        End Set
    End Property
End Class

Public Class A3
    Friend Property Col As System.Collections.ICollection
        Get
            Return Nothing
        End Get
        Set(value As System.Collections.ICollection)
        End Set
    End Property
End Class

Public Class A4
    Private Class A5
        Public Property Col As System.Collections.ICollection
            Get
                Return Nothing
            End Get
            Set(value As System.Collections.ICollection)
            End Set
        End Property
    End Class
End Class
");
        }

        [Fact]
        public void CSharp_CA2227_Inherited()
        {
            VerifyCSharp(@"
using System;

public class A<T>
{
    public System.Collections.Generic.List<T> Col { get; set; }
}
", GetCSharpResultAt(6, 47, CollectionPropertiesShouldBeReadOnlyAnalyzer.RuleId, CollectionPropertiesShouldBeReadOnlyAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void CSharp_CA2227_NotPublic()
        {
            VerifyCSharp(@"
using System;

class A
{
    internal System.Collections.ICollection Col { get; set; }
    protected System.Collections.ICollection Col2 { get; set; }
    private System.Collections.ICollection Col3 { get; set; }
    public System.Collections.ICollection Col4 { get; }
    public System.Collections.ICollection Col5 { get; protected set; }
    public System.Collections.ICollection Col6 { get; private set; }
}
");
        }

        [Fact]
        public void CSharp_CA2227_Array()
        {
            VerifyCSharp(@"
using System;

class A
{
    public int[] Col { get; set; }
}
");
        }

        [Fact]
        public void CSharp_CA2227_Indexer()
        {
            VerifyCSharp(@"
using System;

class A
{
    public System.Collections.ICollection this[int i]
    {
        get { throw new NotImplementedException(); }
        set { }
    }
}
");
        }

        [Fact]
        public void CSharp_CA2227_DataMember()
        {
            VerifyCSharp(@"
using System;

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class DataMemberAttribute : Attribute
    {
    }
}

class A
{
    [System.Runtime.Serialization.DataMember]
    public System.Collections.ICollection Col { get; set; }
}
");
        }
    }
}