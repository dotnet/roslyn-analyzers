// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotReferSelfInSerializableClassTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestSelfReferDirectlyDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass
{
    private TestClass testClass;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 23, DoNotReferSelfInSerializableClass.Rule, "testClass"));

            VerifyBasic(@"
Imports System

Namespace TestNamespace
    <Serializable()> _
    Class TestClass
        Private testClass As TestClass
        
        Sub TestMethod()
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(7, 17, DoNotReferSelfInSerializableClass.Rule, "testClass"));
        }

        [Fact]
        public void TestParentChildCircleDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassA;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "testClassB"),
            GetCSharpResultAt(17, 24, DoNotReferSelfInSerializableClass.Rule, "testClassA"));
        }

        [Fact]
        public void TestParentGrandchildCircleDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassD testClassDInD;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private TestClassA testClassAInC;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassD
{
    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "testClassBInA"),
            GetCSharpResultAt(19, 24, DoNotReferSelfInSerializableClass.Rule, "testClassCInB"),
            GetCSharpResultAt(29, 24, DoNotReferSelfInSerializableClass.Rule, "testClassAInC"));
        }

        [Fact]
        public void TestChildCircleDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassA testClassAInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "testClassAInA"));
        }

        [Fact]
        public void TestChildGrandchildCircleDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private TestClassB testClassBInC;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(17, 24, DoNotReferSelfInSerializableClass.Rule, "testClassCInB"),
            GetCSharpResultAt(27, 24, DoNotReferSelfInSerializableClass.Rule, "testClassBInC"));
        }

        [Fact]
        public void TestClassReferedInTwoLoopsDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassD testClassDInA;

    private TestClassB2 testClassB2InA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private TestClassA testClassAInC;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassD
{
    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB2
{
    private TestClassC2 testClassC2InB2;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC2
{
    private TestClassA testClassAInC2;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "testClassBInA"),
            GetCSharpResultAt(11, 25, DoNotReferSelfInSerializableClass.Rule, "testClassB2InA"),
            GetCSharpResultAt(21, 24, DoNotReferSelfInSerializableClass.Rule, "testClassCInB"),
            GetCSharpResultAt(31, 24, DoNotReferSelfInSerializableClass.Rule, "testClassAInC"),
            GetCSharpResultAt(49, 25, DoNotReferSelfInSerializableClass.Rule, "testClassC2InB2"),
            GetCSharpResultAt(59, 24, DoNotReferSelfInSerializableClass.Rule, "testClassAInC2"));
        }

        [Fact]
        public void TestMultiFieldsWithSameTypeDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassB testClassB2InA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    private TestClassA testClassA2InB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "testClassBInA"),
            GetCSharpResultAt(9, 24, DoNotReferSelfInSerializableClass.Rule, "testClassB2InA"),
            GetCSharpResultAt(19, 24, DoNotReferSelfInSerializableClass.Rule, "testClassAInB"),
            GetCSharpResultAt(21, 24, DoNotReferSelfInSerializableClass.Rule, "testClassA2InB"));
        }

        [Fact]
        public void TestChildCircleWithParentChildCircleDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    private TestClassB testClassBInB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "testClassBInA"),
            GetCSharpResultAt(17, 24, DoNotReferSelfInSerializableClass.Rule, "testClassAInB"),
            GetCSharpResultAt(19, 24, DoNotReferSelfInSerializableClass.Rule, "testClassBInB"));
        }

        [Fact]
        public void TestTwoIndependentParentChildCirclesDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassA2
{
    private TestClassB2 testClassB2;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB2
{
    private TestClassA2 testClassA2;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "testClassB"),
            GetCSharpResultAt(17, 24, DoNotReferSelfInSerializableClass.Rule, "testClassA"),
            GetCSharpResultAt(27, 25, DoNotReferSelfInSerializableClass.Rule, "testClassB2"),
            GetCSharpResultAt(37, 25, DoNotReferSelfInSerializableClass.Rule, "testClassA2"));
        }

        [Fact]
        public void TestSelfReferDirectlyByPropertyDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass
{
    public TestClass TestClassProperty { get; set; }

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 22, DoNotReferSelfInSerializableClass.Rule, "TestClassProperty"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithGenericTypeDiagnostic()
        {
            VerifyCSharp(@"
using System;

[System.Serializable]
public class GenericClass<T>
{
}

[Serializable()]
class TestClass
{
    private GenericClass<TestClass> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(12, 37, DoNotReferSelfInSerializableClass.Rule, "testClasses"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithArrayDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass
{
    private TestClass[] testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 25, DoNotReferSelfInSerializableClass.Rule, "testClasses"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithDoubleDimensionalArrayDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass
{
    private TestClass[][] testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 27, DoNotReferSelfInSerializableClass.Rule, "testClasses"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithListDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClass
{
    private List<TestClass> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 29, DoNotReferSelfInSerializableClass.Rule, "testClasses"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithListListDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClass
{
    private List<List<TestClass>> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 35, DoNotReferSelfInSerializableClass.Rule, "testClasses"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithListListListDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClass
{
    private List<List<List<TestClass>>> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 41, DoNotReferSelfInSerializableClass.Rule, "testClasses"));
        }

        [Fact]
        public void TestGenericChildCircleDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClassA
{
    private List<TestClassA> testClassAInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 30, DoNotReferSelfInSerializableClass.Rule, "testClassAInA"));
        }

        [Fact]
        public void TestSelfReferDirectlyByPropertyWithArrayDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass
{
    public TestClass[] TestClassProperty { get; set; }

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, DoNotReferSelfInSerializableClass.Rule, "TestClassProperty"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithinGenericTypeDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass<T>
{
    private TestClass<T> testClass;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 26, DoNotReferSelfInSerializableClass.Rule, "testClass"));
        }

        [Fact]
        public void TestSelfReferDirectlyWithDictionaryDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private Dictionary<TestClass, NormalClass> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(13, 48, DoNotReferSelfInSerializableClass.Rule, "testClasses"));
        }

        [Fact]
        public void TestWithoutSelfReferNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private int a;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassA;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestStaticSelfReferNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass
{
    private static TestClass testClass;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestStaticParentChildCircleNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private static TestClassA testClassA;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestStaticParentGrandchildCircleNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassD testClassDInD;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private static TestClassA testClassAInC;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassD
{
    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestNonSerializedAttributeSelfReferNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class TestClass
{
    [NonSerialized]
    private TestClass testClass;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestSelfReferDirectlyWithArrayNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private NormalClass[] normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestSelfReferDirectlyWithDoubleDimensionalArrayNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private NormalClass[][] normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestSelfReferDirectlyWithListNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private List<NormalClass> normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestSelfReferDirectlyWithListListNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private List<List<NormalClass>> normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotReferSelfInSerializableClass();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotReferSelfInSerializableClass();
        }
    }
}
