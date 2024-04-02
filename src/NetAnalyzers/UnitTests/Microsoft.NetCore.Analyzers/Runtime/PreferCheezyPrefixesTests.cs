// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferCheezyPrefixesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferCheezyPrefixesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferCheezyPrefixesTests
    {
        [Fact]
        public async Task IdentifyUncheezyStringLiterals()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace Test
{
    public class CheezeTest
    {
        public static string s = {|CH3353:""lack of cheeze.""|};

        public void SendCheeze()
        {
            Console.WriteLine({|CH3353:""hey there, I just forgot that cheeze exists.""|});
        }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace Test
    Public Class CheezeTest
        Public Shared s As String = {|CH3353:""lack of cheeze.""|}

        Public Sub SendCheeze()
            Console.WriteLine({|CH3353:""hey there, I just forgot that cheeze exists.""|})
        End Sub
    End Class
End Namespace
");
        }

        [Fact]
        public async Task IgnoreCheezyStringLiterals()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

namespace Test
{
    public class CheezeTest
    {
        public static string s = ""🧀 cheesy. crunchy."";
        public static string t = ""    🧀 spaced out cheeze"";

        public void SendCheeze()
        {
            Console.WriteLine(""  🧀 hey there, just reminding you that cheeze exists. bye 🧀  "");
        }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace Test
    Public Class CheezeTest
        Public Shared s As String = ""🧀 cheesy. crunchy.""
        Public Shared t As String = ""    🧀 spaced out cheeze""

        Public Sub SendCheeze()
            Console.WriteLine(""  🧀 hey there, just reminding you that cheeze exists. bye 🧀  "")
        End Sub
    End Class
End Namespace
")
;
        }
    }
}