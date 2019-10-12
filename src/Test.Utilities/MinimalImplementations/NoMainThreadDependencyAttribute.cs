// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Test.Utilities.MinimalImplementations
{
    public static class NoMainThreadDependencyAttribute
    {
        public const string CSharp = @"
namespace Roslyn.Utilities
{
    internal enum ContextDependency
    {
        Default,
        None,
        Context,
        Any,
    }
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = true)]
    internal sealed class ThreadDependencyAttribute : System.Attribute
    {
        public ThreadDependencyAttribute(ContextDependency contextDependency) { }
        public ContextDependency ContextDependency { get; }
        public bool AlwaysCompleted { get; set; }
        public bool PerInstance { get; set; }
        public bool Verified { get; set; } = true;
    }
}
";
        public const string VisualBasic = @"
Namespace Global.Roslyn.Utilities
    Friend Module ContextDependency
        Public Const [Default] As Integer = 0
        Public Const None As Integer = 1
        Public Const Context As Integer = 2
        Public Const Any As Integer = 3
    End Module

    <System.AttributeUsage(System.AttributeTargets.Field Or System.AttributeTargets.Method Or System.AttributeTargets.Parameter Or System.AttributeTargets.Property Or System.AttributeTargets.ReturnValue, AllowMultiple:=False, Inherited:=True)>
    Friend NotInheritable Class ThreadDependencyAttribute
        Inherits System.Attribute

        Public Sub New(contextDependency As Integer)
        End Sub
        Public ReadOnly Property ContextDependency As Integer
        Public Property AlwaysCompleted As Boolean
        Public Property PerInstance As Boolean
        Public Property Verified As Boolean = True
    End Class
End Namespace
";
    }
}
