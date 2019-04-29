// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Test.Utilities.MinimalImplementations
{
    public static class NoMainThreadDependencyAttribute
    {
        public const string CSharp = @"
namespace Roslyn.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = true)]
    internal sealed class NoMainThreadDependencyAttribute : System.Attribute
    {
        public bool AlwaysCompleted { get; set; }
        public bool CapturesContext { get; set; }
        public bool PerInstance { get; set; }
        public bool Verified { get; set; } = true;
    }
}
";
        public const string VisualBasic = @"
Namespace Global.Roslyn.Utilities
    <System.AttributeUsage(System.AttributeTargets.Field Or System.AttributeTargets.Method Or System.AttributeTargets.Parameter Or System.AttributeTargets.Property Or System.AttributeTargets.ReturnValue, AllowMultiple:=False, Inherited:=True)>
    Friend NotInheritable Class NoMainThreadDependencyAttribute
        Inherits System.Attribute

        Public Property AlwaysCompleted As Boolean
        Public Property CapturesContext As Boolean
        Public Property PerInstance As Boolean
        Public Property Verified As Boolean = True
    End Class
End Namespace
";
    }
}
