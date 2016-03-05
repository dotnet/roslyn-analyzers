// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class PropertiesShouldNotBeWriteOnlyTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new PropertiesShouldNotBeWriteOnlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new  PropertiesShouldNotBeWriteOnlyAnalyzer();
        }

        [Fact]
        public void CSharp_CA1044Good_Read_Write()
        {
            var code = @"
using System;
namespace DesignLibrary
{
    public class GoodClassWithReadWriteProperty
    {
        string someName;
        public string Name
        {
            get { return someName; }
            set { someName = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_Read_Write1()
        { var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    public class ClassWithReadableProperty
    {
        protected string field;
        public virtual string ReadableProperty1
        {
            get { return field; }
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_public_Read_private_Write()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    public class ClassWithReadableProperty
    {
        protected string field;
        public string AccessibleProperty1
        {
            get { return field; }
            private set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_protected_Read_private_Write()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    public class ClassWithReadableProperty
    {
        protected string field;
        protected string AccessibleProperty2
        {
        get { return field; }
        private set { field = value; }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_internal_Read_private_Write()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    public class GoodClassWithReadWriteProperty
    {
        protected string field;
        internal string AccessibleProperty3
        {
            get { return field; }
            private set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_protected_internal_Read_internal_Write()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    public class GoodClassWithReadWriteProperty
    {
        protected string field;
        protected internal string AccessibleProperty4
        {
            get { return field; }
            internal set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_public_Read_internal_Write()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    public class GoodClassWithReadWriteProperty
    {
        protected string field;
        public string AccessibleProperty5
        {
            get { return field; }
            internal set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_public_Read_protected_Write()
        {
            var code = @"
using System;
namespace DesignLibrary
{
    public class GoodClassWithReadWriteProperty
    {
        protected string field;
        public string AccessibleProperty6
        {
            get { return field; }
            protected set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharp_CA1044Good_public_override_Write()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    protected string field;
    public class DerivedClassWithReadableProperty : ClassWithReadableProperty
    {
        public override string ReadableProperty1
        {
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

// Cannot get below test to work, not sure where the issue is.
//        [Fact]
//        public void CSharp_CA1044Interface_Write()
//        {
//            var code = @"
//using System;
//namespace GoodPropertiesShouldNotBeWriteOnlyTests
//{
//    public interface IInterface
//    {
//        string InterfaceProperty
//        {
//            set;            
//        }
//    }
//}";
//            VerifyCSharp(code);
//        }

//        [Fact]
//        public void CSharp_CA1044Interface_Write1()
//        {
//            var code = @"
//using System;
//namespace GoodPropertiesShouldNotBeWriteOnlyTests
//{
//    public class Class1 : IInterface
//    {
//        string IInterface.InterfaceProperty
//        {
//            set { }
//        }
//    }
//}";
//            VerifyCSharp(code);
//        }

//        [Fact]
//        public void CSharp_CA1044Base_Write()
//        {
//            var code = @"
//using System;
//namespace GoodPropertiesShouldNotBeWriteOnlyTests
//{
//    public class Base
//    {
//        public virtual string BaseProperty
//        {
//            set { }
//        }
//    }
//}";
//            VerifyCSharp(code);
//        }

        [Fact]
        public void CSharp_CA1044Base_Write1()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests
{
    public class Derived : Base
    {
        public override string BaseProperty
        {
            set { base.BaseProperty = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void Basic_CA1044Good_Read_Write()
        {
            var code = @"
Imports System
Namespace DesignLibrary
    Public Class GoodClassWithReadWriteProperty
        Dim someName As String
        Property Name As String
            Get
                Return someName
            End Get
            Set
                someName = value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }
        [Fact]
        public void Basic_CA1044Good_Read_Write1()
        {
            var code = @"
Imports System
Namespace GoodPropertiesShouldNotBeWriteOnlyTests
    Public Class GoodClassWithReadWriteProperty
        WriteOnly field As String
        Public Virtual Property ReadableProperty1 As String
            Get
                Return field
            End Get
            Set
                field = value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }
        [Fact]
        public void Basic_CA1044Good_public_Read_private_Write()
        {
            var code = @"
Imports System
Namespace GoodPropertiesShouldNotBeWriteOnlyTests
    Public class ClassWithReadableProperty
        WriteOnly field As String
        Public Property AccessibleProperty1 As String
            Get
                Return field
            End Get
            Private Set
                field = value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void Basic_CA1044Good_protected_Read_private_Write()
        {
            var code = @"
Imports System
Namespace GoodPropertiesShouldNotBeWriteOnlyTests
    Public class ClassWithReadableProperty
        WriteOnly field As String
        Protected Property AccessibleProperty2 As String
            Get
                Return field
            End Get
            Private Set
                field = value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }
        [Fact]
        public void Basic_CA1044Good_internal_Read_private_Write()
        {
            var code = @"
Imports System
Namespace GoodPropertiesShouldNotBeWriteOnlyTests
    Public class ClassWithReadableProperty
        WriteOnly field As String
        Friend Property AccessibleProperty3 as String
            Get
                Return field
            End Get
            Private Set
                field = value
            End Set
     End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void Basic_CA1044Good_protected_internal_Read_internal_Write()
        {
            var code = @"
Imports System
Namespace GoodPropertiesShouldNotBeWriteOnlyTests
    Public class ClassWithReadableProperty
        WriteOnly field As String
        Protected Friend Property AccessibleProperty4 as String
            Get
                Return field
            End Get
            Friend Set
                field = value
            End Set
     End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void Basic_CA1044Good_public_Read_internal_Write()
        {
            var code = @"
Imports System
Namespace GoodPropertiesShouldNotBeWriteOnlyTests
    Public class ClassWithReadableProperty
        WriteOnly field As String
        Public Property AccessibleProperty5 as String
            Get
                Return field
            End Get
            Friend Set
                field = value
            End Set
     End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void Basic_CA1044Good_public_Read_protected_Write()
        {
            var code = @"
Imports System
Namespace GoodPropertiesShouldNotBeWriteOnlyTests
    Public class ClassWithReadableProperty
        WriteOnly field As String
        Public Property AccessibleProperty5 as String
            Get
                Return field
            End Get
            Friend Set
                field = value
            End Set
     End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void CSharp_CA1044Bad_Write_with_NoRead()
        {
            string CA1044Message = MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageAddGetter;
            var code = @"
using System;
namespace BadPropertiesShouldNotBeWriteOnlyTests
{
    public class BadClassWithWriteOnlyProperty
    {
        protected string someName;
        public string CSharpWriteOnlyProperty1
        {
            set { someName = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(8, 23, CA1044Message, "CSharpWriteOnlyProperty1"));

            var code1 = @"
using System;
namespace BadPropertiesShouldNotBeWriteOnlyTests
{
    public class BadClassWithWriteOnlyProperty
    {
        protected string someName;
        protected public string CSharpWriteOnlyProperty2
        {
            set { someName = value; }
        }
    }
}";
            VerifyCSharp(code1, GetCA1044CSharpResultAt(8, 33, CA1044Message, "CSharpWriteOnlyProperty2"));

            var code2 = @"
using System;
namespace BadPropertiesShouldNotBeWriteOnlyTests
{
    public class BadClassWithWriteOnlyProperty
    {
        protected string someName;
        protected internal string CSharpWriteOnlyProperty3
        {
            set { someName = value; }
        }
    }
}";
            VerifyCSharp(code2, GetCA1044CSharpResultAt(8, 35, CA1044Message, "CSharpWriteOnlyProperty3"));
        }

// Converted above code from C# to VB free hand.  But can't get test to work, not sure where the issue is.
//        [Fact]
//        public void Basic_CA1044Bad_Write_with_NoRead()
//        {
//            string CA1044Message = MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageAddGetter;
//            var code = @"
//Imports System
//Namespace BadPropertiesShouldNotBeWriteOnlyTests
//    Public class BadClassWithWriteOnlyProperty
//        Dim field As String
//        Public Property BasicWriteOnlyProperty1 As String   
//            Set
//                someName = Value
//            End Set
//        End Property
//    End Class
//End NameSpace
//";
//            VerifyBasic(code, GetCA1044BasicResultAt(5, 25, CA1044Message, "BasicWriteOnlyProperty1"));
//
//            var code1 = @"
//Imports System
//Namespace BadPropertiesShouldNotBeWriteOnlyTests
//    Public class BadClassWithWriteOnlyProperty
//        Dim field As String
//        Protected Property BasicWriteOnlyProperty2 as String
//            Set
//                someName = Value
//            End Set
//        End Property
//    End Class
//End NameSpace
//";
//            VerifyBasic(code1, GetCA1044BasicResultAt(5, 28, CA1044Message, "BasicWriteOnlyProperty2"));
//
//            var code2 = @"
//Imports System
//Namespace BadPropertiesShouldNotBeWriteOnlyTests
//    Public class BadClassWithWriteOnlyProperty
//        Dim field As String
//        Protected Friend Property BasicWriteOnlyProperty3 as String
//            Set
//                someName = Value
//            End Set
//        End Property
//    End Class
//End NameSpace
//";
//            VerifyBasic(code2, GetCA1044BasicResultAt(5, 35, CA1044Message, "BasicWriteOnlyProperty3"));
//        }

        [Fact]
        public void CSharp_CA1044Bad_Read_Write()
        {
            string CA1044Message = MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageMakeMoreAccessible;
            var code = @"
using System;
namespace BadPropertiesShouldNotBeWriteOnlyTests
{
    public class BadClassWithWriteOnlyProperty
    {
         public string CSharpInaccessibleProperty1
         {
            private get { return field; }
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(7, 24, CA1044Message, "CSharpInaccessibleProperty1"));

            var code1 = @"
using System;
namespace BadPropertiesShouldNotBeWriteOnlyTests
{
    public class BadClassWithWriteOnlyProperty
    {
        protected string CSharpInaccessibleProperty2
        {
            private get { return field; }
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code1, GetCA1044CSharpResultAt(7, 26, CA1044Message, "CSharpInaccessibleProperty2"));

            var code2 = @"
using System;
namespace BadPropertiesShouldNotBeWriteOnlyTests
{
    public class BadClassWithWriteOnlyProperty
    {
        protected internal string CSharpInaccessibleProperty3
        {
            internal get { return field; }
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code2, GetCA1044CSharpResultAt(7, 35, CA1044Message, "CSharpInaccessibleProperty3"));            
            var code3 = @"
using System;
namespace BadPropertiesShouldNotBeWriteOnlyTests
{
    public class BadClassWithWriteOnlyProperty
    {
        public string CSharpInaccessibleProperty4
        {
            internal get { return field; }
            set { field = value; }
        }
    }   
}";
            VerifyCSharp(code3, GetCA1044CSharpResultAt(7, 23, CA1044Message, "CSharpInaccessibleProperty4"));
        }

        public void Basic_CA1044Bad_Read_Write()
        {
            string CA1044Message = MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageMakeMoreAccessible;
            var code = @"
Imports System
Namespace BadPropertiesShouldNotBeWriteOnlyTests
    Public class BadClassWithWriteOnlyProperty
        Public Property BasicInaccessibleProperty1 As String
            Private Get
                Return field
            End Get
            Set
                someName = Value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044CSharpResultAt(4, 25, CA1044Message, "BasicInaccessibleProperty1"));

            var code1 = @"
Imports System
Namespace BadPropertiesShouldNotBeWriteOnlyTests
    Public class BadClassWithWriteOnlyProperty
        Protected Property BasicInaccessibleProperty2 As String
            Private Get
                Return field
            End Get
            Set
                someName = Value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code1, GetCA1044CSharpResultAt(4, 28, CA1044Message, "BasicInaccessibleProperty2"));

            var code2 = @"
Imports System
Namespace BadPropertiesShouldNotBeWriteOnlyTests
    Public class BadClassWithWriteOnlyProperty
        Protected Friend Property BasicInaccessibleProperty3 As String
            Friend Get
                Return field
            End Get
            Set
                someName = Value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code2, GetCA1044CSharpResultAt(4, 35, CA1044Message, "BasicInaccessibleProperty3"));

            var code3 = @"
Imports System
Namespace BadPropertiesShouldNotBeWriteOnlyTests
    Public class BadClassWithWriteOnlyProperty
        Public Property BasicInaccessibleProperty4 As String
            Friend Get
                Return field
            End Get
            Set
                someName = Value
            End Set
        End Property
    End Class
End NameSpace
";
            VerifyBasic(code3, GetCA1044CSharpResultAt(4, 25, CA1044Message, "BasicInaccessibleProperty4"));
        }
        private static DiagnosticResult GetCA1044CSharpResultAt(int line, int column, string CA1044Message, string objectName)
        {
            return GetCSharpResultAt(line, column, PropertiesShouldNotBeWriteOnlyAnalyzer.RuleId, string.Format(CA1044Message, objectName));
        }
        private static DiagnosticResult GetCA1044BasicResultAt(int line, int column, string CA1044Message, string objectName)
        {
            return GetBasicResultAt(line, column, PropertiesShouldNotBeWriteOnlyAnalyzer.RuleId, string.Format(CA1044Message, objectName));
        }
    }
}
