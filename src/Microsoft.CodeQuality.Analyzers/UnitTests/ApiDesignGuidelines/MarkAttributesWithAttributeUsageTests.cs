﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public partial class MarkAttributesWithAttributeUsageTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MarkAttributesWithAttributeUsageAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MarkAttributesWithAttributeUsageAnalyzer();
        }

        [Fact]
        public void TestCSSimpleAttributeClass()
        {
            VerifyCSharp(@"
using System;

class C : Attribute
{
}
", GetCA1018CSharpResultAt(4, 7, "C"));
        }

        [Fact]
        public void TestCSInheritedAttributeClass()
        {
            VerifyCSharp(@"
using System;

[AttributeUsage(AttributeTargets.Method)]
class C : Attribute
{
}
class D : C
{
}
", GetCA1018CSharpResultAt(8, 7, "D"));
        }

        [Fact]
        public void TestCSInheritedAttributeClassWithScope()
        {
            VerifyCSharp(@"
using System;

[|[AttributeUsage(AttributeTargets.Method)]
class C : Attribute
{
}|]
class D : C
{
}
");
        }

        [Fact]
        public void TestCSAbstractAttributeClass()
        {
            VerifyCSharp(@"
using System;

abstract class C : Attribute
{
}
");
        }

        [Fact]
        public void TestVBSimpleAttributeClass()
        {
            VerifyBasic(@"
Imports System

Class C
    Inherits Attribute
End Class
", GetCA1018BasicResultAt(4, 7, "C"));
        }

        [Fact]
        public void TestVBInheritedAttributeClass()
        {
            VerifyBasic(@"
Imports System

<AttributeUsage(AttributeTargets.Method)>
Class C
    Inherits Attribute
End Class
Class D
    Inherits C
End Class
", GetCA1018BasicResultAt(8, 7, "D"));
        }

        [Fact]
        public void TestVBInheritedAttributeClassWithScope()
        {
            VerifyBasic(@"
Imports System

[|<AttributeUsage(AttributeTargets.Method)>
Class C
    Inherits Attribute
End Class|]
Class D
    Inherits C
End Class
");
        }

        [Fact]
        public void TestVBAbstractAttributeClass()
        {
            VerifyBasic(@"
Imports System

MustInherit Class C
    Inherits Attribute
End Class
");
        }

        private static DiagnosticResult GetCA1018CSharpResultAt(int line, int column, string objectName)
        {
            return GetCSharpResultAt(line, column, MarkAttributesWithAttributeUsageAnalyzer.RuleId,
                string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAttributesWithAttributeUsageMessageDefault, objectName));
        }

        private static DiagnosticResult GetCA1018BasicResultAt(int line, int column, string objectName)
        {
            return GetBasicResultAt(line, column, MarkAttributesWithAttributeUsageAnalyzer.RuleId,
                string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAttributesWithAttributeUsageMessageDefault, objectName));
        }
    }
}
