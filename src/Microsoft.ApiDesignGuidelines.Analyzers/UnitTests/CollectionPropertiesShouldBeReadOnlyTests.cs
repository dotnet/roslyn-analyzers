// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
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

class A
{
    public System.Collections.ICollection Col { get; set; }
}
", GetCSharpResultAt(6, 43, CollectionPropertiesShouldBeReadOnlyAnalyzer.RuleId, CollectionPropertiesShouldBeReadOnlyAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void Basic_CA2227_Test()
        {
            VerifyBasic(@"
Imports System

Class A
    Public Property Col As System.Collections.ICollection
End Class
", GetBasicResultAt(5, 21, CollectionPropertiesShouldBeReadOnlyAnalyzer.RuleId, CollectionPropertiesShouldBeReadOnlyAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void CSharp_CA2227_Inherited()
        {
            VerifyCSharp(@"
using System;

class A<T>
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
    public System.Collections.ICollection Col { get; }
    public System.Collections.ICollection Col { get; protected set; }
    public System.Collections.ICollection Col { get; private set; }
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
    public System.Collections.ICollection Col[int i] { get; set; }
}
");
        }

        [Fact]
        public void CSharp_CA2227_DataMember()
        {
            VerifyCSharp(@"
using System;

class A
{
    [System.Runtime.Serialization.DataMember]
    public System.Collections.ICollection Col { get; set; }
}
");
        }
    }
}