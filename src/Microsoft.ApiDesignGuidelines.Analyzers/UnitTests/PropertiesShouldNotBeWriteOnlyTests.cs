// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
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

        // Valid C# Tests that should not be flagged based on CA1044 (good tests)
        [Fact]
        public void CS_CA1044Good_Read_Write()
        {
            var code = @"
using System;
namespace DesignLibrary
{
    public class CS_GoodClassWithReadWriteProperty
    {
        string CS_someName;
        public string CS_Name
        {
            get { return CS_someName; }
            set { CS_someName = CS_value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_Read_Write1()
        { var code = @"
using System;
namespace CS_GoodPropertiesShouldNotBeWriteOnlyTests1
{
    public class CS_ClassWithReadableProperty1
    {
        protected string CS_field;
        public virtual string CS_ReadableProperty1
        {
            get { return CS_field; }
            set { CS_field = CS_value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_public_Read_private_Write()
        {
            var code = @"
using System;
namespace CS_GoodPropertiesShouldNotBeWriteOnlyTests2
{
    public class CS_ClassWithReadableProperty2
    {
        protected string CS_field;
        public string CS_AccessibleProperty2
        {
            get { return CS_field; }
            private set { CS_field = CS_value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_protected_Read_private_Write()
        {
            var code = @"
using System;
namespace CS_GoodPropertiesShouldNotBeWriteOnlyTests3
{
    public class CS_ClassWithReadableProperty3
    {
        protected string CS_field;
        protected string CS_AccessibleProperty3
        {
        get { return CS_field; }
        private set { CS_field = CS_value; }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_internal_Read_private_Write()
        {
            var code = @"
using System;
namespace CS_GoodPropertiesShouldNotBeWriteOnlyTests4
{
    public class CS_GoodClassWithReadWriteProperty4
    {
        protected string CS_field;
        internal string CS_AccessibleProperty4
        {
            get { return CS_field; }
            private set { CS_field = CS_value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_protected_internal_Read_internal_Write()
        {
            var code = @"
using System;
namespace CS_GoodPropertiesShouldNotBeWriteOnlyTests5
{
    public class CS_GoodClassWithReadWriteProperty5
    {
        protected string CS_field;
        protected internal string AccessibleProperty5
        {
            get { return field; }
            internal set { field = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_public_Read_internal_Write()
        {
            var code = @"
using System;
namespace CS_GoodPropertiesShouldNotBeWriteOnlyTests6
{
    public class CS_GoodClassWithReadWriteProperty6
    {
        protected string CS_field;
        public string CS_AccessibleProperty6
        {
            get { return CS_field; }
            internal set { CS_field = CS_value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_public_Read_protected_Write()
        {
            var code = @"
using System;
namespace DesignLibrary
{
    public class CS_GoodClassWithReadWriteProperty7
    {
        protected string CS_field;
        public string CS_AccessibleProperty7
        {
            get { return CS_field; }
            protected set { CS_field = CS_value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Good_public_override_Write()
        {
            var code = @"
using System;
namespace CS_GoodPropertiesShouldNotBeWriteOnlyTests8
{
    protected string CS_field;
    public class CS_DerivedClassWithReadableProperty : CS_ClassWithReadableProperty
    {
        public override string CS_ReadableProperty8
        {
            set { CS_field = CS_value; }
        }
    }
}";
            VerifyCSharp(code);
        }
                
        [Fact]
        public void CS_CA1044Interface_Write()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests9
{
    public class Class1 : IInterface9
    {
        string IInterface.InterfaceProperty
        {
            set { }
        }
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void CS_CA1044Base_Write1()
        {
            var code = @"
using System;
namespace GoodPropertiesShouldNotBeWriteOnlyTests10
{
    public class Derived : Base10
    {
        public override string BaseProperty10
        {
            set { base.BaseProperty = value; }
        }
    }
}";
            VerifyCSharp(code);
        }

        // Valid VB Tests that should not be flagged based on CA1044 (good tests)
        [Fact]
        public void VB_CA1044Good_Read_Write()
        {
            var code = @"
Imports System
Namespace DesignLibrary
    Public Class VB_GoodClassWithReadWriteProperty
		Private VB_someName As String
		Public Property VB_Name() As String
			Get
				Return VB_someName
			End Get
			Set(ByVal value As String)
				VB_someName = VB_value
			End Set
		End Property
	End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Good_Read_Write1()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests1
    Public Class VB_GoodClassWithReadWriteProperty1
		Protected VB_field As String
		Public Overridable Property VB_ReadableProperty1() As String
			Get
				Return VB_field
			End Get
			Set(ByVal value As String)
				VB_field = VB_value
			End Set
		End Property
	End Class
";
            VerifyBasic(code);
        }

         [Fact]
         public void VB_CA1044Good_public_Read_private_Write()
         {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests2
    Public class VB_ClassWithReadableProperty2
		Protected VB_field As String
		Public Property VB_AccessibleProperty2() As String
			Get
				Return VB_field
			End Get
			Private Set(ByVal value As String)
				VB_field = VB_value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Good_protected_Read_private_Write()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests3
    Public class VB_ClassWithReadableProperty3
		Protected VB_field As String
		Protected Property VB_AccessibleProperty3() As String
		Get
			Return VB_field
		End Get
		Private Set(ByVal value As String)
			VB_field = VB_value
		End Set
		End Property
    End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Good_internal_Read_private_Write()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests4
    Public VB_class ClassWithReadableProperty4
		Protected VB_field As String
		Friend Property VB_AccessibleProperty4() As String
			Get
				Return VB_field
			End Get
			Private Set(ByVal value As String)
				VB_field = VB_value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Good_protected_internal_Read_internal_Write()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests5
    Public class VB_ClassWithReadableProperty5
		Protected VB_field As String
		Protected Friend Property AccessibleProperty5() As String
			Get
				Return field
			End Get
			Friend Set(ByVal value As String)
				field = value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Good_public_Read_internal_Write()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests6
    Public class VB_ClassWithReadableProperty6
		Protected VB_field As String
		Public Property VB_AccessibleProperty6() As String
			Get
				Return VB_field
			End Get
			Friend Set(ByVal value As String)
				VB_field = VB_value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Good_public_Read_protected_Write()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests7
    Public class VB_ClassWithReadableProperty7
		Protected VB_field As String
		Public Property VB_AccessibleProperty7() As String
			Get
				Return VB_field
			End Get
			Protected Set(ByVal value As String)
				VB_field = VB_value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Good_ClassWithReadableProperty()        
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests8
	Protected VB_field As String
	Public Class VB_DerivedClassWithReadableProperty
		Inherits VB_ClassWithReadableProperty
		Public Overrides WriteOnly Property VB_ReadableProperty8() As String
			Set(ByVal value As String)
				VB_field = VB_value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Interface_Write()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests9
	Public Class Class1
		Implements IInterface9
		Private WriteOnly Property IInterface_InterfaceProperty() As String Implements IInterface.InterfaceProperty
			Set(ByVal value As String)
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        [Fact]
        public void VB_CA1044Base_Write()
        {
            var code = @"
Imports System
Namespace VB_GoodPropertiesShouldNotBeWriteOnlyTests10
	Public Class Derived
		Inherits Base10
		Public Overrides WriteOnly Property BaseProperty10() As String
			Set(ByVal value As String)
				MyBase.BaseProperty = value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code);
        }

        // C# Tests that should be flagged with CA1044 Addgetter
        [Fact]
        public void CS_CA1044Bad_Write_with_NoRead()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests
{
    public class CS_BadClassWithWriteOnlyProperty
    {
        protected string CS_someName;
        public string CS_WriteOnlyProperty
        {
            set { CS_someName = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(8, 23, CA1044MessageAddGetter, "CS_WriteOnlyProperty"));
        }

        [Fact]
        public void CS_CA1044Bad_Write_with_NoRead1()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests1
{
    public class CS_BadClassWithWriteOnlyProperty1
    {
        protected string CS_someName;
        protected public string CS_WriteOnlyProperty1
        {
            set { CS_someName = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(8, 33, CA1044MessageAddGetter, "CS_WriteOnlyProperty1"));
        }

        [Fact]
        public void CS_CA1044Bad_Write_with_NoRead2()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests2
{
    public class CS_BadClassWithWriteOnlyProperty2
    {
        protected string CS_someName;
        protected internal string CS_WriteOnlyProperty2
        {
            set { CS_someName = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(8, 35, CA1044MessageAddGetter, "CS_WriteOnlyProperty2"));
        }

        [Fact]
        public void CS_CA1044bad_Base_Write()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests3
{
    public class CS_Base3
    {
        public virtual string CS_BaseProperty3
        {
            set { }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(7, 31, CA1044MessageAddGetter, "CS_BaseProperty3"));
        }

        [Fact]
        public void CS_CA1044bad_Interface_Write()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests4
{
    public interface CS_IInterface4
    {
        string CS_InterfaceProperty4
        {
            set;
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(7, 16, CA1044MessageAddGetter, "CS_InterfaceProperty4"));
        }

        // C# Tests that should be flagged with CA1044 MakeMoreAccessible
        [Fact]
        public void CS_CA1044Bad_InaccessibleRead()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests5
{
    public class CS_BadClassWithWriteOnlyProperty5
    {
         public string CS_InaccessibleProperty5
         {
            private get { return field; }
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(7, 24, CA1044MessageMakeMoreAccessible, "CS_InaccessibleProperty5"));
        }

        [Fact]
        public void CS_CA1044Bad_InaccessibleRead1()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests6
{
    public class CS_BadClassWithWriteOnlyProperty6
    {
        string field;
        protected string CS_InaccessibleProperty6
        {
            private get { return field; }
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(8, 26, CA1044MessageMakeMoreAccessible, "CS_InaccessibleProperty6"));
        }

        [Fact]
        public void CS_CA1044Bad_InaccessibleRead2()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests7
{
    public class CS_BadClassWithWriteOnlyProperty7
    {
        protected internal string CS_InaccessibleProperty7
        {
            internal get { return field; }
            set { field = value; }
        }
    }
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(7, 35, CA1044MessageMakeMoreAccessible, "CS_InaccessibleProperty7"));
        }

        [Fact]
        public void CS_CA1044Bad_InaccessibleRead3()
        {
            var code = @"
using System;
namespace CS_BadPropertiesShouldNotBeWriteOnlyTests8
{
    public class CS_BadClassWithWriteOnlyProperty8
    {
        public string CS_InaccessibleProperty8
        {
            internal get { return field; }
            set { field = value; }
        }
    }   
}";
            VerifyCSharp(code, GetCA1044CSharpResultAt(7, 23, CA1044MessageMakeMoreAccessible, "CS_InaccessibleProperty8"));
        }

        // VB Tests that should be flagged with CA1044 Addgetter
        [Fact]
        public void VB_CA1044Bad_Write_with_NoRead()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests
	Public Class VB_BadClassWithWriteOnlyProperty
		Protected VB_someName As String
		Public WriteOnly Property VB_WriteOnlyProperty() As String
			Set(ByVal value As String)
				VB_someName = value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(6, 29, CA1044MessageAddGetter, "VB_WriteOnlyProperty"));
        }

        [Fact]
        public void VB_CA1044Bad_Write_with_NoRead1()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests1
	Public Class VB_BadClassWithWriteOnlyProperty1
		Protected VB_someName As String
		Protected Public WriteOnly Property VB_WriteOnlyProperty1() As String
			Set(ByVal value As String)
				VB_someName = value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(6, 39, CA1044MessageAddGetter, "VB_WriteOnlyProperty1"));
        }

        [Fact]
        public void VB_CA1044Bad_Base_NoRead()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests2
	Public Class VB_BadClassWithWriteOnlyProperty2
		Protected VB_someName As String
		Protected Friend WriteOnly Property VB_WriteOnlyProperty2() As String
			Set(ByVal value As String)
				VB_someName = value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(6, 39, CA1044MessageAddGetter, "VB_WriteOnlyProperty2"));
        }

        [Fact]
        public void VB_CA1044Bad_Base_Write()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests3
	Public Class VB_Base3
		Public Overridable WriteOnly Property VB_BaseProperty3() As String
			Set(ByVal value As String)
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(5, 41, CA1044MessageAddGetter, "VB_BaseProperty3"));
        }

        [Fact]
        public void VB_CA1044Bad_Interface_Write()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests4
    Public Interface VB_IInterface4
		WriteOnly Property VB_InterfaceProperty4() As String
	End Interface
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(5, 22, CA1044MessageAddGetter, "VB_InterfaceProperty4"));
        }

        [Fact]
        public void VB_CA1044Bad_InaccessibleRead()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests5
   	Public Class VB_BadClassWithWriteOnlyProperty5
		 Public Property VB_InaccessibleProperty5() As String
			Private Get
				Return field
			End Get
			Set(ByVal value As String)
				field = value
			End Set
		 End Property
	End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(5, 20, CA1044MessageMakeMoreAccessible, "VB_InaccessibleProperty5"));
        }

        [Fact]
        public void VB_CA1044Bad_InaccessibleRead1()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests6
	Public Class VB_BadClassWithWriteOnlyProperty6
		Private field As String
		Protected Property VB_InaccessibleProperty6() As String
			Private Get
				Return field
			End Get
			Set(ByVal value As String)
				field = value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(6, 22, CA1044MessageMakeMoreAccessible, "VB_InaccessibleProperty6"));
        }

        [Fact]
        public void VB_CA1044Bad_InaccessibleRead2()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests7
	Public Class VB_BadClassWithWriteOnlyProperty7
		Protected Friend Property VB_InaccessibleProperty7() As String
			Friend Get
				Return field
			End Get
			Set(ByVal value As String)
				field = value
			End Set
		End Property
	End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(5, 29, CA1044MessageMakeMoreAccessible, "VB_InaccessibleProperty7"));
        }

        [Fact]
        public void VB_CA1044Bad_InaccessibleRead3()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests8
	Public Class VB_BadClassWithWriteOnlyProperty8
    	Public Property VB_InaccessibleProperty8() As String
	    	Friend Get
		    	Return field
		    End Get
		    Set(ByVal value As String)
			    field = value
		    End Set
	    End Property
    End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(5, 22, CA1044MessageMakeMoreAccessible, "VB_InaccessibleProperty8"));
        }

        [Fact]
        public void VB_CA1044Bad_ClassWithWriteOnlyProperty()
        {
            var code = @"
Import System
Namespace VB_BadPropertiesShouldNotBeWriteOnlyTests8
	Public Class VB_BadClassWithWriteOnlyProperty8
    	Public Property VB_InaccessibleProperty8() As String
	    	Friend Get
		    	Return field
		    End Get
		    Set(ByVal value As String)
			    field = value
		    End Set
	    End Property
    End Class
End NameSpace
";
            VerifyBasic(code, GetCA1044BasicResultAt(5, 22, CA1044MessageMakeMoreAccessible, "VB_InaccessibleProperty8"));
        }
        
        private static readonly string CA1044MessageAddGetter = MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageAddGetter;
        private static readonly string CA1044MessageMakeMoreAccessible = MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageMakeMoreAccessible;
        
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
