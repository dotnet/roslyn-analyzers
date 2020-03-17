﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpDefineAccessorsForAttributeArgumentsAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DefineAccessorsForAttributeArgumentsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines.BasicDefineAccessorsForAttributeArgumentsAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DefineAccessorsForAttributeArgumentsFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public partial class DefineAccessorsForAttributeArgumentsTests
    {
        [Fact]
        public async Task CSharp_CA1019_AddAccessor()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class NoAccessorTestAttribute : Attribute
{
    private string m_name;

    public NoAccessorTestAttribute(string name)
    {
        m_name = name;
    }
}",
                VerifyCS.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.DefaultRule).WithSpan(9, 43, 9, 47).WithArguments("name", "NoAccessorTestAttribute"),
@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class NoAccessorTestAttribute : Attribute
{
    private string m_name;

    public NoAccessorTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name { get; }
}");
        }

        [Fact]
        public async Task CSharp_CA1019_AddAccessor1()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class SetterOnlyTestAttribute : Attribute
{
    private string m_name;

    public SetterOnlyTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name 
    { 
        set { m_name = value; }
    }
}",
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.DefaultRule).WithSpan(9, 43, 9, 47).WithArguments("name", "SetterOnlyTestAttribute"),
                        VerifyCS.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.RemoveSetterRule).WithSpan(16, 9, 16, 12).WithArguments("Name", "name"),
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class SetterOnlyTestAttribute : Attribute
{
    private string m_name;

    public SetterOnlyTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name 
    {
        internal set { m_name = value; }

        get
        {
            throw new NotImplementedException();
        }
    }
}",
                    },
                },
                NumberOfFixAllIterations = 2,
            }.RunAsync();
        }

        [Fact]
        public async Task CSharp_CA1019_MakeGetterPublic()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class InternalGetterTestAttribute : Attribute
{
    private string m_name;

    public InternalGetterTestAttribute(string name)
    {
        m_name = name;
    }

    internal string Name
    {
        get { return m_name; }
        set { m_name = value; }
    }
}",
                VerifyCS.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.IncreaseVisibilityRule).WithSpan(16, 9, 16, 12).WithArguments("Name", "name"),
@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class InternalGetterTestAttribute : Attribute
{
    private string m_name;

    public InternalGetterTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name
    {
        get { return m_name; }

        internal set { m_name = value; }
    }
}");
        }

        [Fact]
        public async Task CSharp_CA1019_MakeGetterPublic2()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class InternalGetterTestAttribute : Attribute
{
    private string m_name;

    public InternalGetterTestAttribute(string name)
    {
        m_name = name;
    }

    internal string Name
    {
        get { return m_name; }
        set { m_name = value; }
    }
}",
                VerifyCS.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.IncreaseVisibilityRule).WithSpan(16, 9, 16, 12).WithArguments("Name", "name"),
@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class InternalGetterTestAttribute : Attribute
{
    private string m_name;

    public InternalGetterTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name
    {
        get { return m_name; }

        internal set { m_name = value; }
    }
}");
        }

        [Fact]
        public async Task CSharp_CA1019_MakeGetterPublic3()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class InternalGetterTestAttribute : Attribute
{
    private string m_name;

    public InternalGetterTestAttribute(string name)
    {
        m_name = name;
    }

    internal string Name
    {
        get { return m_name; }
    }
}",
                VerifyCS.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.IncreaseVisibilityRule).WithSpan(16, 9, 16, 12).WithArguments("Name", "name"),
@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class InternalGetterTestAttribute : Attribute
{
    private string m_name;

    public InternalGetterTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name
    {
        get { return m_name; }
    }
}");
        }

        [Fact]
        public async Task CSharp_CA1019_MakeSetterInternal()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class PublicSetterTestAttribute : Attribute
{
    private string m_name;

    public PublicSetterTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name
    {
        get { return m_name; }
        set { m_name = value; }
    }
}",
                VerifyCS.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.RemoveSetterRule).WithSpan(17, 9, 17, 12).WithArguments("Name", "name"),
@"
using System;

[AttributeUsage(AttributeTargets.All)]
public sealed class PublicSetterTestAttribute : Attribute
{
    private string m_name;

    public PublicSetterTestAttribute(string name)
    {
        m_name = name;
    }

    public string Name
    {
        get { return m_name; }

        internal set { m_name = value; }
    }
}");
        }

        [Fact]
        public async Task VisualBasic_CA1019_AddAccessor()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class NoAccessorTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub
End Class",
                VerifyVB.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.DefaultRule).WithSpan(9, 20, 9, 24).WithArguments("name", "NoAccessorTestAttribute"),
@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class NoAccessorTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public ReadOnly Property Name As String
        Get
        End Get
    End Property
End Class");
        }

        [Fact]
        public async Task VisualBasic_CA1019_AddAccessor2()
        {
            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class SetterOnlyTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Friend WriteOnly Property Name() As String
        Set
            m_name = value
        End Set
    End Property
End Class",
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.DefaultRule).WithSpan(9, 20, 9, 24).WithArguments("name", "SetterOnlyTestAttribute"),
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class SetterOnlyTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public Property Name() As String
        Friend Set
            m_name = value
        End Set
        Get
            Throw New NotImplementedException()
        End Get
    End Property
End Class",
                    },
                },
                NumberOfIncrementalIterations = 2,
                NumberOfFixAllIterations = 2,
            }.RunAsync();
        }

        [Fact]
        public async Task VisualBasic_CA1019_MakeGetterPublic()
        {
            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class InternalGetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public Property Name() As String
        Friend Get
            Return m_name
        End Get
        Set
            m_name = value
        End Set
    End Property
End Class",
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.IncreaseVisibilityRule).WithSpan(14, 16, 14, 19).WithArguments("Name", "name"),
                        VerifyVB.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.RemoveSetterRule).WithSpan(17, 9, 17, 12).WithArguments("Name", "name"),
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class InternalGetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Friend Set
            m_name = value
        End Set
    End Property
End Class",
                    },
                },
                NumberOfFixAllIterations = 2,
            }.RunAsync();
        }

        [Fact]
        public async Task VisualBasic_CA1019_MakeGetterPublic2()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class InternalGetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Friend Property Name() As String
        Get
            Return m_name
        End Get
        Set
            m_name = value
        End Set
    End Property
End Class",
                VerifyVB.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.IncreaseVisibilityRule).WithSpan(14, 9, 14, 12).WithArguments("Name", "name"),
@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class InternalGetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Friend Set
            m_name = value
        End Set
    End Property
End Class");
        }

        [Fact]
        public async Task VisualBasic_CA1019_MakeGetterPublic3()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class InternalGetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Friend ReadOnly Property Name() As String
        Get
            Return m_name
        End Get
    End Property
End Class",
                VerifyVB.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.IncreaseVisibilityRule).WithSpan(14, 9, 14, 12).WithArguments("Name", "name"),
@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class InternalGetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return m_name
        End Get
    End Property
End Class");
        }

        [Fact]
        public async Task VisualBasic_CA1019_MakeSetterInternal()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class PublicSetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set
            m_name = value
        End Set
    End Property
End Class",
                VerifyVB.Diagnostic(DefineAccessorsForAttributeArgumentsAnalyzer.RemoveSetterRule).WithSpan(17, 9, 17, 12).WithArguments("Name", "name"),
@"
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public NotInheritable Class PublicSetterTestAttribute
    Inherits Attribute
    Private m_name As String
    
    Public Sub New(name As String)
        m_name = name
    End Sub

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Friend Set
            m_name = value
        End Set
    End Property
End Class");
        }
    }
}
