// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotGuardSetAddOrRemoveByContains,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpDoNotGuardSetAddOrRemoveByContainsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotGuardSetAddOrRemoveByContains,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicDoNotGuardSetAddOrRemoveByContainsFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class DoNotGuardSetAddOrRemoveByContainsTests
    {
        #region Tests
        [Fact]
        public async Task NonInvocationConditionDoesNotThrow_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        public MyClass()
        {
            if (!true) { }
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task AddIsTheOnlyStatement_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (![|MySet.Contains(""Item"")|])
                MySet.Add(""Item"");
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            MySet.Add(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveIsTheOnlyStatement_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if ([|MySet.Contains(""Item"")|])
                MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddIsTheOnlyStatementInABlock_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (![|MySet.Contains(""Item"")|])
            {
                MySet.Add(""Item"");
            }
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            MySet.Add(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveIsTheOnlyStatementInABlock_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if ([|MySet.Contains(""Item"")|])
            {
                MySet.Remove(""Item"");
            }
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddHasElseStatement_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (![|MySet.Contains(""Item"")|])
                MySet.Add(""Item"");
            else
                throw new Exception(""Item already exists"");
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (!MySet.Add(""Item""))
                throw new Exception(""Item already exists"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveHasElseStatement_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if ([|MySet.Contains(""Item"")|])
                MySet.Remove(""Item"");
            else
                throw new Exception(""Item doesn't exist"");
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (!MySet.Remove(""Item""))
                throw new Exception(""Item doesn't exist"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddHasElseBlock_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (![|MySet.Contains(""Item"")|])
            {
                MySet.Add(""Item"");
            }
            else
            {
                throw new Exception(""Item already exists"");
            }
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (!MySet.Add(""Item""))
            {
                throw new Exception(""Item already exists"");
            }
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveHasElseBlock_OffersFixer_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if ([|MySet.Contains(""Item"")|])
            {
                MySet.Remove(""Item"");
            }
            else
            {
                throw new Exception(""Item doesn't exist"");
            }
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (!MySet.Remove(""Item""))
            {
                throw new Exception(""Item doesn't exist"");
            }
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddWithAdditionalStatements_ReportsDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (![|MySet.Contains(""Item"")|])
            {
                MySet.Add(""Item"");
                Console.WriteLine();
            }
        }
        " + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task RemoveWithAdditionalStatements_ReportsDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if ([|MySet.Contains(""Item"")|])
            {
                MySet.Remove(""Item"");
                Console.WriteLine();
            }
        }
        " + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task AddWithNonNegatedContains_NoDiagnostics_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (MySet.Contains(""Item""))
                MySet.Add(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task RemoveWithNegatedContains_NoDiagnostics_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (!MySet.Contains(""Item""))
                MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task AdditionalCondition_NoDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (MySet.Contains(""Item"") && MySet.Count > 2)
                MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task ConditionInVariable_NoDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            var result = MySet.Contains(""Item"");
            if (result)
	            MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task RemoveInSeparateLine_NoDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            if (MySet.Contains(""Item""))
	            _ = MySet.Count;
	        MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task NotSetRemove_NoDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();
        private bool Remove(string item) => false;

        public MyClass()
        {
            if (MySet.Contains(""Item""))
	            Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task TriviaIsPreserved_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            // reticulates the splines
            if ([|MySet.Contains(""Item"")|])
            {
                MySet.Remove(""Item"");
            }
        }" + CSNamespaceAndClassEnd;

            string fixedSource = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> MySet = new HashSet<string>();

        public MyClass()
        {
            // reticulates the splines
            MySet.Remove(""Item"");
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddIsTheOnlyStatement_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not [|MySet.Contains(""Item"")|] Then MySet.Add(""Item"")
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            MySet.Add(""Item"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveIsTheOnlyStatement_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If ([|MySet.Contains(""Item"")|]) Then MySet.Remove(""Item"")
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            MySet.Remove(""Item"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddIsTheOnlyStatementInBlock_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not [|MySet.Contains(""Item"")|] Then
                MySet.Add(""Item"")
            End If
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            MySet.Add(""Item"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveIsTheOnlyStatementInBlock_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If ([|MySet.Contains(""Item"")|]) Then
                MySet.Remove(""Item"")
            End If
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            MySet.Remove(""Item"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddHasElseStatement_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not [|MySet.Contains(""Item"")|] Then MySet.Add(""Item"") Else Throw new Exception(""Item already exists"")
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not MySet.Add(""Item"") Then Throw new Exception(""Item already exists"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveHasElseStatement_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If [|MySet.Contains(""Item"")|] Then MySet.Remove(""Item"") Else Throw new Exception(""Item doesn't exist"")
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not MySet.Remove(""Item"") Then Throw new Exception(""Item doesn't exist"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddHasElseBlock_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not [|MySet.Contains(""Item"")|] Then
                MySet.Add(""Item"")
            Else
                Throw new Exception(""Item already exists"")
            End If
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not MySet.Add(""Item"") Then
                Throw new Exception(""Item already exists"")
            End If
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task RemoveHasElseBlock_OffersFixer_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If [|MySet.Contains(""Item"")|] Then
                MySet.Remove(""Item"")
            Else
                Throw new Exception(""Item doesn't exist"")
            End If
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not MySet.Remove(""Item"") Then
                Throw new Exception(""Item doesn't exist"")
            End If
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task AddWithNonNegatedContains_NoDiagnostics_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If MySet.Contains(""Item"") Then MySet.Add(""Item"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task RemoveWithNegatedContains_NoDiagnostics_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not MySet.Contains(""Item"") Then MySet.Remove(""Item"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task AddWithAdditionalStatements_ReportsDiagnostic_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If Not [|MySet.Contains(""Item"")|] Then
                MySet.Add(""Item"")
                Console.WriteLine()
            End If
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task RemoveWithAdditionalStatements_ReportsDiagnostic_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            If [|MySet.Contains(""Item"")|] Then
                MySet.Remove(""Item"")
                Console.WriteLine()
            End If
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task TriviaIsPreserved_VB()
        {
            string source = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            ' reticulates the splines
            If ([|MySet.Contains(""Item"")|]) Then
                MySet.Remove(""Item"")
            End If
        End Sub
    End Class
End Namespace";

            string fixedSource = @"
" + VBUsings + @"
Namespace Testopolis
    Public Class SomeClass
        Public MySet As New HashSet(Of String)()

        Public Sub New()
            ' reticulates the splines
            MySet.Remove(""Item"")
        End Sub
    End Class
End Namespace";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        [WorkItem(6377, "https://github.com/dotnet/roslyn-analyzers/issues/6377")]
        public async Task ContainsAndRemoveCalledOnDifferentInstances_NoDiagnostic_CS()
        {
            string source = CSUsings + CSNamespaceAndClassStart + @"
        private readonly HashSet<string> SetField1 = new HashSet<string>();
        private readonly HashSet<string> SetField2 = new HashSet<string>();

        public HashSet<string> SetProperty1 { get; } = new HashSet<string>();

        public MyClass()
        {
            if (SetField2.Contains(""Item""))
                SetField1.Remove(""Item"");

            if (!SetField1.Contains(""Item""))
            {
                SetField2.Remove(""Item"");
            }

            if (SetProperty1.Contains(""Item""))
                SetField1.Remove(""Item"");

            if (!SetField1.Contains(""Item""))
            {
                SetProperty1.Remove(""Item"");
            }

            var mySetLocal4 = new HashSet<string>();
            if (mySetLocal4.Contains(""Item""))
                SetField1.Remove(""Item"");

            if (!SetField1.Contains(""Item""))
            {
                mySetLocal4.Remove(""Item"");
            }
        }

        private void RemoveItem(HashSet<string> setParam)
        {
            if (setParam.Contains(""Item""))
                SetField1.Remove(""Item"");

            if (!SetField1.Contains(""Item""))
            {
                setParam.Remove(""Item"");
            }
        }" + CSNamespaceAndClassEnd;

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        #endregion

        #region Helpers
        private const string CSUsings = @"using System;
using System.Collections.Generic;";

        private const string CSNamespaceAndClassStart = @"namespace Testopolis
{
    public class MyClass
    {
";

        private const string CSNamespaceAndClassEnd = @"
    }
}";

        private const string VBUsings = @"Imports System
Imports System.Collections.Generic";
        #endregion
    }
}
