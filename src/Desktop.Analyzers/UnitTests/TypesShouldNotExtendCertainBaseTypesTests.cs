// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public class TypesShouldNotExtendCertainBaseTypesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicTypesShouldNotExtendCertainBaseTypesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpTypesShouldNotExtendCertainBaseTypesAnalyzer();
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_CSharp_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class C : Attribute
{
}
");
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_CSharp_ApplicationException()
        {
            var source = @"
using System;

class C1 : ApplicationException
{
}
";
            var expected = new[]
            {
                GetCSharpResultAt(4, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemApplicationExceptionRule, "C1", "System.ApplicationException")
            };

            VerifyCSharp(source, expected);
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_CSharp_XmlDocument()
        {
            var source = @"
using System.Xml;

class C1 : XmlDocument
{
}
";
            var expected = new[]
            {
                GetCSharpResultAt(4, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemXmlXmlDocumentRule, "C1", "System.Xml.XmlDocument")
            };

            VerifyCSharp(source, expected);
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_CSharp_Collection()
        {
            var source = @"
using System.Collections;

class C1 : CollectionBase
{
}

class C2 : DictionaryBase
{
}

class C3 : Queue
{
}

class C4 : ReadOnlyCollectionBase
{
}

class C5 : SortedList
{
}

class C6 : Stack
{
}";
            var expected = new[]
            {
                GetCSharpResultAt(4, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsCollectionBaseRule, "C1", "System.Collections.CollectionBase"),
                GetCSharpResultAt(8, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsDictionaryBaseRule, "C2", "System.Collections.DictionaryBase"),
                GetCSharpResultAt(12, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsQueueRule, "C3", "System.Collections.Queue"),
                GetCSharpResultAt(16, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsReadOnlyCollectionBaseRule, "C4", "System.Collections.ReadOnlyCollectionBase"),
                GetCSharpResultAt(20, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsSortedListRule, "C5", "System.Collections.SortedList"),
                GetCSharpResultAt(24, 7, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsStackRule, "C6", "System.Collections.Stack")
            };

            VerifyCSharp(source, expected);
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_Basic_NoDiagnostic()
        {
            VerifyBasic(@"
Imports System

Public Class Class2
    Inherits Attribute

End Class
");
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_Basic_ApplicationException()
        {
            var source = @"
Imports System

Public Class C1
    Inherits ApplicationException

End Class

";
            var expected = new[]
            {
                GetBasicResultAt(4, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemApplicationExceptionRule, "C1", "System.ApplicationException")
            };

            VerifyBasic(source, expected);
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_Basic_XmlDocument()
        {
            var source = @"
Imports System.Xml

Public Class C1
    Inherits XmlDocument

End Class
";
            var expected = new[]
            {
                GetBasicResultAt(4, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemXmlXmlDocumentRule, "C1", "System.Xml.XmlDocument")
            };

            VerifyBasic(source, expected);
        }

        [Fact]
        public void TypesShouldNotExtendCertainBaseTypes_Basic_Collection()
        {
            var source = @"
Imports System.Collections

Public Class C1
    Inherits CollectionBase

End Class

Public Class C2
    Inherits DictionaryBase

End Class

Public Class C3
    Inherits Queue

End Class

Public Class C4
    Inherits ReadOnlyCollectionBase

End Class

Public Class C5
    Inherits SortedList

End Class

Public Class C6
    Inherits Stack

End Class
";
            var expected = new[]
            {
                GetBasicResultAt(4, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsCollectionBaseRule, "C1", "System.Collections.CollectionBase"),
                GetBasicResultAt(9, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsDictionaryBaseRule, "C2", "System.Collections.DictionaryBase"),
                GetBasicResultAt(14, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsQueueRule, "C3", "System.Collections.Queue"),
                GetBasicResultAt(19, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsReadOnlyCollectionBaseRule, "C4", "System.Collections.ReadOnlyCollectionBase"),
                GetBasicResultAt(24, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsSortedListRule, "C5", "System.Collections.SortedList"),
                GetBasicResultAt(29, 14, TypesShouldNotExtendCertainBaseTypesAnalyzer.SystemCollectionsStackRule, "C6", "System.Collections.Stack")
            };

            VerifyBasic(source, expected);
        }
    }
}