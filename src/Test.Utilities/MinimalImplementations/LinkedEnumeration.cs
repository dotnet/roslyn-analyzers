// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Test.Utilities.MinimalImplementations
{
    public static class LinkedEnumeration
    {
        public const string CSharp = @"
namespace Roslyn.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    internal sealed class LinkedEnumerationAttribute : System.Attribute
    {
        public LinkedEnumerationAttribute(System.Type sourceEnumeration) => SourceEnumeration = sourceEnumeration;
        public System.Type SourceEnumeration { get; }
    }
}
";

        public const string VisualBasic = @"
Namespace Global.Roslyn.Utilities
    <System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple:=False, Inherited:=False)>
    Public NotInheritable Class LinkedEnumerationAttribute
        Inherits System.Attribute

        Public Sub New(sourceEnumeration As System.Type)
            Me.SourceEnumeration = sourceEnumeration
        End Sub

        Public ReadOnly Property SourceEnumeration As System.Type
    End Class
End Namespace
";
    }
}
