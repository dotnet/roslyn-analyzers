// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private readonly string CA3075DataViewConnectionStringMessage = DesktopAnalyzersResources.ReviewDtdProcessingPropertiesDiagnosis;

        private DiagnosticResult GetCA3075DataViewCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, CA3075DataViewConnectionStringMessage);
        }

        private DiagnosticResult GetCA3075DataViewBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, CA3075DataViewConnectionStringMessage);
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagerSetCollectionStringShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

namespace FxCopUnsafeXml
{
    public class ReviewDataViewConnectionString
    {
        public void TestMethod(string src)
        {
            DataSet ds = new DataSet();
            ds.DefaultViewManager.DataViewSettingCollectionString = src;
        }
    }
}
",
                GetCA3075DataViewCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class ReviewDataViewConnectionString
        Public Sub TestMethod(src As String)
            Dim ds As New DataSet()
            ds.DefaultViewManager.DataViewSettingCollectionString = src
        End Sub
    End Class
End Namespace",
                GetCA3075DataViewBasicResultAt(8, 13)
            );
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagernInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    public DataSet Test
    {
        get {
            var src = """";
            DataSet ds = new DataSet();
            ds.DefaultViewManager.DataViewSettingCollectionString = src;
            return ds;
        }
    }
}",
                GetCA3075DataViewCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Public ReadOnly Property Test() As DataSet
        Get
            Dim src = """"
            Dim ds As New DataSet()
            ds.DefaultViewManager.DataViewSettingCollectionString = src
            Return ds
        End Get
    End Property
End Class",
                GetCA3075DataViewBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagerInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
DataSet privateDoc;
public DataSet GetDoc
        {
            set
            {
                if (value == null)
                {
                    var src = """";
                    DataSet ds = new DataSet();
                    ds.DefaultViewManager.DataViewSettingCollectionString = src;
                    privateDoc = ds;
                }
                else
                    privateDoc = value;
            }
        }
}",
                GetCA3075DataViewCSharpResultAt(15, 21)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private privateDoc As DataSet
    Public WriteOnly Property GetDoc() As DataSet
        Set
            If value Is Nothing Then
                Dim src = """"
                Dim ds As New DataSet()
                ds.DefaultViewManager.DataViewSettingCollectionString = src
                privateDoc = ds
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075DataViewBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagerInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
  using System;
  using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try
            {
                var src = """";
                DataSet ds = new DataSet();
                ds.DefaultViewManager.DataViewSettingCollectionString = src;
            }
            catch (Exception) { throw; }
            finally { }
        }
    }",
                GetCA3075DataViewCSharpResultAt(13, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
            Dim src = """"
            Dim ds As New DataSet()
            ds.DefaultViewManager.DataViewSettingCollectionString = src
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagerInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
   using System;
   using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception)
            {
                var src = """";
                DataSet ds = new DataSet();
                ds.DefaultViewManager.DataViewSettingCollectionString = src;
            }
            finally { }
        }
    }",
                GetCA3075DataViewCSharpResultAt(14, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim src = """"
            Dim ds As New DataSet()
            ds.DefaultViewManager.DataViewSettingCollectionString = src
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagerInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
   using System;
   using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { throw; }
            finally
            {
                var src = """";
                DataSet ds = new DataSet();
                ds.DefaultViewManager.DataViewSettingCollectionString = src;
            }
        }
    }",
                GetCA3075DataViewCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim src = """"
            Dim ds As New DataSet()
            ds.DefaultViewManager.DataViewSettingCollectionString = src
        End Try
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagerInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
 using System.Threading.Tasks;
using System.Data;

    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => {
                var src = """";
                DataSet ds = new DataSet();
                ds.DefaultViewManager.DataViewSettingCollectionString = src;
            });
        }

        private async void TestMethod2()
        {
            await TestMethod();
        }
    }",
                GetCA3075DataViewCSharpResultAt(12, 17)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Data

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim src = """"
        Dim ds As New DataSet()
        ds.DefaultViewManager.DataViewSettingCollectionString = src

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(10, 9)
            );
        }

        [Fact]
        public void UseDataSetDefaultDataViewManagerInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    delegate void Del();

    Del d = delegate () {
        var src = """";
        DataSet ds = new DataSet();
        ds.DefaultViewManager.DataViewSettingCollectionString = src;
    };
}",
                GetCA3075DataViewCSharpResultAt(11, 9)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim src = """"
    Dim ds As New DataSet()
    ds.DefaultViewManager.DataViewSettingCollectionString = src

End Sub
End Class",
                GetCA3075DataViewBasicResultAt(10, 5)
            );
        }

        [Fact]
        public void UseDataViewManagerSetCollectionStringShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

namespace FxCopUnsafeXml
{
    public class ReviewDataViewConnectionString
    {
        public void TestMethod(string src)
        {
            DataViewManager manager = new DataViewManager();
            manager.DataViewSettingCollectionString = src;
        }
    }
}
",
                GetCA3075DataViewCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class ReviewDataViewConnectionString
        Public Sub TestMethod(src As String)
            Dim manager As New DataViewManager()
            manager.DataViewSettingCollectionString = src
        End Sub
    End Class
End Namespace",
                GetCA3075DataViewBasicResultAt(8, 13)
            );
        }

        [Fact]
        public void UseDataViewManagernInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    public DataSet Test
    {
        get {
            var src = """";
            DataViewManager manager = new DataViewManager();
            manager.DataViewSettingCollectionString = src;
            return manager;
        }
    }
}",
                GetCA3075DataViewCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Public ReadOnly Property Test() As DataSet
        Get
            Dim src = """"
            Dim manager As New DataViewManager()
            manager.DataViewSettingCollectionString = src
            Return manager
        End Get
    End Property
End Class",
                GetCA3075DataViewBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataViewManagerInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
DataViewManager privateDoc;
public DataViewManager GetDoc
        {
            set
            {
                if (value == null)
                {
                    var src = """";
                    DataViewManager manager = new DataViewManager();
                    manager.DataViewSettingCollectionString = src;
                    privateDoc = manager;
                }
                else
                    privateDoc = value;
            }
        }
}",
                GetCA3075DataViewCSharpResultAt(15, 21)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private privateDoc As DataViewManager
    Public WriteOnly Property GetDoc() As DataViewManager
        Set
            If value Is Nothing Then
                Dim src = """"
                Dim manager As New DataViewManager()
                manager.DataViewSettingCollectionString = src
                privateDoc = manager
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075DataViewBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseDataViewManagerInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
  using System;
  using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try
            {
                var src = """";
                DataViewManager manager = new DataViewManager();
                manager.DataViewSettingCollectionString = src;
            }
            catch (Exception) { throw; }
            finally { }
        }
    }",
                GetCA3075DataViewCSharpResultAt(13, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
            Dim src = """"
            Dim manager As New DataViewManager()
            manager.DataViewSettingCollectionString = src
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataViewManagerInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
   using System;
   using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception)
            {
                var src = """";
                DataViewManager manager = new DataViewManager();
                manager.DataViewSettingCollectionString = src;
            }
            finally { }
        }
    }",
                GetCA3075DataViewCSharpResultAt(14, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim src = """"
            Dim manager As New DataViewManager()
            manager.DataViewSettingCollectionString = src
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseDataViewManagerInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
   using System;
   using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { throw; }
            finally
            {
                var src = """";
                DataViewManager manager = new DataViewManager();
                manager.DataViewSettingCollectionString = src;
            }
        }
    }",
                GetCA3075DataViewCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim src = """"
            Dim manager As New DataViewManager()
            manager.DataViewSettingCollectionString = src
        End Try
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseDataViewManagerInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
 using System.Threading.Tasks;
using System.Data;

    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => {
                var src = """";
                DataViewManager manager = new DataViewManager();
                manager.DataViewSettingCollectionString = src;
            });
        }

        private async void TestMethod2()
        {
            await TestMethod();
        }
    }",
                GetCA3075DataViewCSharpResultAt(12, 17)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Data

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim src = """"
        Dim manager As New DataViewManager()
        manager.DataViewSettingCollectionString = src

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(10, 9)
            );
        }

        [Fact]
        public void UseDataViewManagerInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    delegate void Del();

    Del d = delegate () {
        var src = """";
        DataViewManager manager = new DataViewManager();
        manager.DataViewSettingCollectionString = src;
    };
}",
                GetCA3075DataViewCSharpResultAt(11, 9)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim src = """"
    Dim manager As New DataViewManager()
    manager.DataViewSettingCollectionString = src

    End Sub
End Class",
                GetCA3075DataViewBasicResultAt(10, 5)
            );
        }

        
    }
}
