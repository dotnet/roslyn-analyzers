// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Test.Utilities.MinimalImplementations
{
    public static class SystemWebUI
    {
        public const string CSharp = @"
namespace System.Web.UI
{
    using System;
    using System.Web;

    public class Control
    {
        public virtual System.Web.UI.Page Page { get; set; }
    }

    public class Page : Control
    {
        public System.Web.HttpRequest Request { get; }

        public string ViewStateUserKey { get; set; }

        protected virtual void OnInit(EventArgs e)
        {
        }
    }

    public class LosFormatter
    {
        public object Deserialize(System.IO.Stream stream)
        {
            return null;
        }

        public object Deserialize(System.IO.TextReader input)
        {
            return null;
        }

        public object Deserialize(string input)
        {
            return null;
        }

        public void Serialize(System.IO.Stream stream, object value) { }

        public void Serialize(System.IO.TextWriter output, object value) { }
    }

    public class ObjectStateFormatter
    {
        public object Deserialize(System.IO.Stream inputStream)
        {
            return null;
        }

        public object Deserialize(System.IO.TextReader input)
        {
            return null;
        }

        public object Deserialize(string inputString)
        {
            return null;
        }

        public void Serialize(System.IO.Stream stream, object value) { }

        public void Serialize(System.IO.TextWriter output, object value) { }
    }
}";

        public const string VisualBasic = @"
Imports System
Imports System.Web

Namespace System.Web.UI
    Public Class Control
        Public Property Page As System.Web.UI.Page
    End Class

    Public Class Page
        Public Property Request As System.Web.HttpRequest

        Public Property ViewStateUserKey As String

        Protected Overridable Sub OnInit(ByVal e As EventArgs)
        End Sub
    End Class

    Public Class LosFormatter
        Public Function Deserialize(ByVal stream As System.IO.Stream) As Object
            Return Nothing
        End Function

        Public Function Deserialize(ByVal input As System.IO.TextReader) As Object
            Return Nothing
        End Function

        Public Function Deserialize(ByVal input As String) As Object
            Return Nothing
        End Function

        Public Sub Serialize(ByVal stream As System.IO.Stream, ByVal value As Object)
        End Sub

        Public Sub Serialize(ByVal output As System.IO.TextWriter, ByVal value As Object)
        End Sub
    End Class

    Public Class ObjectStateFormatter
        Public Function Deserialize(ByVal inputStream As System.IO.Stream) As Object
            Return Nothing
        End Function

        Public Function Deserialize(ByVal input As System.IO.TextReader) As Object
            Return Nothing
        End Function

        Public Function Deserialize(ByVal inputString As String) As Object
            Return Nothing
        End Function

        Public Sub Serialize(ByVal stream As System.IO.Stream, ByVal value As Object)
        End Sub

        Public Sub Serialize(ByVal output As System.IO.TextWriter, ByVal value As Object)
        End Sub
    End Class
End Namespace
";
    }
}