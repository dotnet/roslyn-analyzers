// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Test.Utilities.MinimalImplementations
{
    public static class ExternalApi
    {
        public const string CSharp = @"
namespace Roslyn.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    internal sealed class ExternalApiAttribute : System.Attribute
    {
    }
}
";

        public const string VisualBasic = @"
Namespace Global.Roslyn.Utilities
    <System.AttributeUsage(System.AttributeTargets.All, AllowMultiple:=False, Inherited:=False)>
    Public NotInheritable Class ExternalApiAttribute
        Inherits System.Attribute
    End Class
End Namespace
";
    }
}
