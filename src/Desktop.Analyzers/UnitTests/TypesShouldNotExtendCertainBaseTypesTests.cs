// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public class TypesShouldNotExtendCertainBaseTypesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new TypesShouldNotExtendCertainBaseTypesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TypesShouldNotExtendCertainBaseTypesAnalyzer();
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
            DiagnosticResult[] expected = new[]
            {
                GetCSharpApplicationExceptionResultAt(4, 7, "C1", "System.ApplicationException")
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
            DiagnosticResult[] expected = new[]
            {
                GetCSharpXmlDocumentResultAt(4, 7, "C1", "System.Xml.XmlDocument")
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
            DiagnosticResult[] expected = new[]
            {
                GetCSharpCollectionBaseResultAt(4, 7, "C1", "System.Collections.CollectionBase"),
                GetCSharpDictionaryBaseResultAt(8, 7, "C2", "System.Collections.DictionaryBase"),
                GetCSharpQueueResultAt(12, 7, "C3", "System.Collections.Queue"),
                GetCSharpReadOnlyCollectionResultAt(16, 7, "C4", "System.Collections.ReadOnlyCollectionBase"),
                GetCSharpSortedListResultAt(20, 7, "C5", "System.Collections.SortedList"),
                GetCSharpStackResultAt(24, 7, "C6", "System.Collections.Stack")
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
            DiagnosticResult[] expected = new[]
            {
                GetBasicApplicationExceptionResultAt(4, 14, "C1", "System.ApplicationException")
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
            DiagnosticResult[] expected = new[]
            {
                GetBasicXmlDocumentResultAt(4, 14, "C1", "System.Xml.XmlDocument")
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
            DiagnosticResult[] expected = new[]
            {
                GetBasicCollectionBaseResultAt(4, 14, "C1", "System.Collections.CollectionBase"),
                GetBasicDictionaryBaseResultAt(9, 14, "C2", "System.Collections.DictionaryBase"),
                GetBasicQueueResultAt(14, 14, "C3", "System.Collections.Queue"),
                GetBasicReadOnlyCollectionBaseResultAt(19, 14, "C4", "System.Collections.ReadOnlyCollectionBase"),
                GetBasicSortedListResultAt(24, 14, "C5", "System.Collections.SortedList"),
                GetBasicStackResultAt(29, 14, "C6", "System.Collections.Stack")
            };

            VerifyBasic(source, expected);
        }

        private static DiagnosticResult GetCSharpCollectionBaseResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsCollectionBase, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicCollectionBaseResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsCollectionBase, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpDictionaryBaseResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsDictionaryBase, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicDictionaryBaseResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsDictionaryBase, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpQueueResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsQueue, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicQueueResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsQueue, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpReadOnlyCollectionResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsReadOnlyCollectionBase, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicReadOnlyCollectionBaseResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsReadOnlyCollectionBase, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpSortedListResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsSortedList, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicSortedListResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsSortedList, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpStackResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsStack, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicStackResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsStack, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpApplicationExceptionResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemApplicationException, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicApplicationExceptionResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemApplicationException, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetCSharpXmlDocumentResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemXmlXmlDocument, declaredTypeName, badBaseTypeName);
            return GetCSharpResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicXmlDocumentResultAt(int line, int column, string declaredTypeName, string badBaseTypeName)
        {
            string message = string.Format(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemXmlXmlDocument, declaredTypeName, badBaseTypeName);
            return GetBasicResultAt(line, column, TypesShouldNotExtendCertainBaseTypesAnalyzer.RuleId, message);
        }
    }
}