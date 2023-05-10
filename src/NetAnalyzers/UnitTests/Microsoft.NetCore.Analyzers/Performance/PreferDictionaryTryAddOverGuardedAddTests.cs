// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferDictionaryTryAddOverGuardedAddAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpPreferDictionaryTryAddValueOverGuardedAddFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferDictionaryTryAddOverGuardedAddAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicPreferDictionaryTryAddValueOverGuardedAddFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class PreferDictionaryTryAddOverGuardedAddTests
    {

        #region C# Tests

        private const string CSharpTemplate = @"
using System;
using System.Collections.Generic;

namespace UnitTests {{
    public class Test {{
        public void M() {{
            var d = new Dictionary<int, int>();
            {0}
        }}
    }}

    public class FakeDictionary<TKey, TValue> {{
        public bool ContainsKey(TKey key) => throw null;
        public void Add(TKey key, TValue value) => throw null;
    }}
}}";

        private const string CheckNegativeReturnedValue = @"
if (!{|#0:d.ContainsKey(5)|})
{
	{|#1:d.Add(5, 6)|};
}";

        private const string CheckNegativeReturnedValueFixed = @"
d.TryAdd(5, 6);";

        private const string CheckNegativeReturnedValueInVariable = @"
var value = 6;
if (!{|#0:d.ContainsKey(5)|})
{
	{|#1:d.Add(5, value)|};
}";

        private const string CheckNegativeReturnedValueInVariableFixed = @"
var value = 6;
d.TryAdd(5, value);";

        private const string CheckNegativeReturnedValueWithoutBraces = @"
if (!{|#0:d.ContainsKey(5)|})
	{|#1:d.Add(5, 6)|};";

        private const string CheckNegativeReturnedValueWithElseRemoval = @"
if (!{|#0:d.ContainsKey(5)|})
{
	{|#1:d.Add(5, 6)|};
} 
else
{
    var value = d[5];
    Console.WriteLine($""Value: {value}"");
}";

        private const string CheckNegativeReturnedValueWithElseRemovalFixed = @"
if (!d.TryAdd(5, 6))
{
    var value = d[5];
    Console.WriteLine($""Value: {value}"");
}";

        private const string CheckNegativeReturnedValueWithElse = @"
if (!{|#0:d.ContainsKey(5)|})
{
    {|#1:d.Add(5, 6)|};
    Console.WriteLine($""Value: {d[5]}"");
}
else
{
    var value = d[5];
    Console.WriteLine($""Value: {value}"");
}";

        private const string CheckNegativeReturnedValueWithElseFixed = @"
if (d.TryAdd(5, 6))
{
    Console.WriteLine($""Value: {d[5]}"");
}
else
{
    var value = d[5];
    Console.WriteLine($""Value: {value}"");
}";

        private const string CheckNegativeReturnedValueAndPreserveLogic = @"
if (!{|#0:d.ContainsKey(5)|})
{
    {|#1:d.Add(5, 6)|};
    Console.WriteLine($""Value: {d[5]}"");
}";

        private const string CheckNegativeReturnedValueAndPreserveLogicFixed = @"
if (d.TryAdd(5, 6))
{
    Console.WriteLine($""Value: {d[5]}"");
}";

        private const string CheckPositiveReturnedValueAndPreserveLogic = @"
if ({|#0:d.ContainsKey(5)|})
{
    Console.WriteLine($""Value existed: {d[5]}"");
}
else
{
    {|#1:d.Add(5, 6)|};
    Console.WriteLine($""Value added: {d[5]}"");
}";

        private const string CheckPositiveReturnedValueAndPreserveLogicFixed = @"
if (!d.TryAdd(5, 6))
{
    Console.WriteLine($""Value existed: {d[5]}"");
}
else
{
    Console.WriteLine($""Value added: {d[5]}"");
}";

        private const string CheckPositiveReturnedValueAndPreserveLogicRedundantElse = @"
if ({|#0:d.ContainsKey(5)|})
{
    Console.WriteLine($""Value existed: {d[5]}"");
}
else
{
    {|#1:d.Add(5, 6)|};
}";

        private const string CheckPositiveReturnedValueAndPreserveLogicRedundantElseFixed = @"
if (!d.TryAdd(5, 6))
{
    Console.WriteLine($""Value existed: {d[5]}"");
}";

        private const string FakeDictionary = @"
var dict = new FakeDictionary<int, int>();
if(!dict.ContainsKey(5))
{
    dict.Add(5, 6);
}";

        private const string LongRunningOperation = @"
if(!d.ContainsKey(5))
{
    d.Add(5, LongRunningOperation());
}

int LongRunningOperation() => throw null;";

        private const string LongRunningOperationCapturedInVariable = @"
if(!d.ContainsKey(5))
{
    var result = LongRunningOperation();
    d.Add(5, result);
}

int LongRunningOperation() => throw null;";

        private const string WithObjectInstantiation = @"
if(!d.ContainsKey(5))
{
    d.Add(5, new int());
}";

        private const string NotGuardedByContainsKey = @"
if(!d.ContainsValue(5))
{
    d.Add(5, 5);
}";

        private const string AddOnDifferentDictionary = @"
var d2 = new Dictionary<int, int>();
if (d2.ContainsKey(5))
{
    Console.WriteLine($""Value existed: {d2[5]}"");
}
else
{
    d.Add(5, 6);
}";

        private const string KeyIsModified = @"
var key = 1;
if (!d.ContainsKey(key))
{
    key++;
    d.Add(key, 2);
}";

        private const string DictionaryIsReplaced = @"
var key = 1;
if (!d.ContainsKey(key))
{
    d = new Dictionary<int, int>();
    d.Add(key, 2);
}";

        #endregion

        #region VB Tests

        private const string VbTemplate = @"
Imports System
Imports System.Collections.Generic

Namespace UnitTests
    Public Class Test
        Public Function M()
            Dim d = New Dictionary(Of Integer, Integer)
            {0}
        End Function

        Private Class FakeDictionary(Of TKey, TValue)
            Public Function ContainsKey(ByVal key As TKey) As Boolean
                Return False
            End Function

            Public Function Add(key As TKey, value As TValue)
            End Function
        End Class
    End Class
End Namespace";

        private const string CheckNegativeReturnedValueVb = @"
If Not {|#0:d.ContainsKey(5)|} Then
	{|#1:d.Add(5, 6)|}
End If";

        private const string CheckNegativeReturnedValueVbFixed = @"
d.TryAdd(5, 6)";

        private const string CheckNegativeReturnedValueWithElseRemovalVb = @"
If Not {|#0:d.ContainsKey(5)|} Then
	{|#1:d.Add(5, 6)|}
Else
    Dim value = d(5)
    Console.WriteLine($""Value: {value}"")
End If";

        private const string CheckNegativeReturnedValueWithElseRemovalVbFixed = @"
If Not d.TryAdd(5, 6) Then
    Dim value = d(5)
    Console.WriteLine($""Value: {value}"")
End If";

        private const string CheckNegativeReturnedValueWithElseVb = @"
If Not {|#0:d.ContainsKey(5)|} Then
    {|#1:d.Add(5, 6)|}
    Console.WriteLine($""Value: {d(5)}"")
Else
    Dim value = d(5)
    Console.WriteLine($""Value: {value}"")
End If";

        private const string CheckNegativeReturnedValueWithElseVbFixed = @"
If d.TryAdd(5, 6) Then
    Console.WriteLine($""Value: {d(5)}"")
Else
    Dim value = d(5)
    Console.WriteLine($""Value: {value}"")
End If";

        private const string CheckNegativeReturnedValueAndPreserveLogicVb = @"
If Not {|#0:d.ContainsKey(5)|} Then
    {|#1:d.Add(5, 6)|}
    Console.WriteLine($""Value: {d(5)}"")
End If";

        private const string CheckNegativeReturnedValueAndPreserveLogicVbFixed = @"
If d.TryAdd(5, 6) Then
    Console.WriteLine($""Value: {d(5)}"")
End If";

        private const string CheckPositiveReturnedValueAndPreserveLogicVb = @"
If {|#0:d.ContainsKey(5)|} Then
    Console.WriteLine($""Value existed: {d(5)}"")
Else
    {|#1:d.Add(5, 6)|}
    Console.WriteLine($""Value added: {d(5)}"")
End If";

        private const string CheckPositiveReturnedValueAndPreserveLogicVbFixed = @"
If Not d.TryAdd(5, 6) Then
    Console.WriteLine($""Value existed: {d(5)}"")
Else
    Console.WriteLine($""Value added: {d(5)}"")
End If";

        private const string CheckPositiveReturnedValueAndPreserveLogicRedundantElseVb = @"
If {|#0:d.ContainsKey(5)|} Then
    Console.WriteLine($""Value existed: {d(5)}"")
Else
    {|#1:d.Add(5, 6)|}
End If";

        private const string CheckPositiveReturnedValueAndPreserveLogicRedundantElseVbFixed = @"
If Not d.TryAdd(5, 6) Then
    Console.WriteLine($""Value existed: {d(5)}"")
End If";

        private const string FakeDictionaryVb = @"
Dim dict = New FakeDictionary(Of Integer, Integer)
If Not dict.ContainsKey(5) Then
    dict.Add(5, 6)
End If";

        private const string LongRunningOperationVb = @"
Dim LongRunningOperation = Function() As Integer
    Return 2
End Function

If Not d.ContainsKey(5) Then
    d.Add(5, LongRunningOperation())
End If";

        private const string NotGuardedByContainsKeyVb = @"
If Not d.ContainsValue(5) Then
    d.Add(5, 5)
End If";

        private const string AddOnDifferentDictionaryVb = @"
Dim d2 = New Dictionary(Of Integer, Integer)
If d2.ContainsKey(5) Then
    Console.WriteLine($""Value existed: {d2(5)}"")
Else
    d.Add(5, 6)
End If";

        #endregion

        [Theory]
        [InlineData(CheckNegativeReturnedValue, CheckNegativeReturnedValueFixed)]
        [InlineData(CheckNegativeReturnedValueInVariable, CheckNegativeReturnedValueInVariableFixed)]
        [InlineData(CheckNegativeReturnedValueWithoutBraces, CheckNegativeReturnedValueFixed)]
        [InlineData(CheckNegativeReturnedValueWithElseRemoval, CheckNegativeReturnedValueWithElseRemovalFixed)]
        [InlineData(CheckNegativeReturnedValueWithElse, CheckNegativeReturnedValueWithElseFixed)]
        [InlineData(CheckNegativeReturnedValueAndPreserveLogic, CheckNegativeReturnedValueAndPreserveLogicFixed)]
        [InlineData(CheckPositiveReturnedValueAndPreserveLogic, CheckPositiveReturnedValueAndPreserveLogicFixed)]
        [InlineData(CheckPositiveReturnedValueAndPreserveLogicRedundantElse, CheckPositiveReturnedValueAndPreserveLogicRedundantElseFixed)]
        public Task ShouldReportDiagnosticAsync(string codeSnippet, string fixedCodeSnippet)
        {
            string testCode = CreateCSharpTestClass(codeSnippet);
            string fixedCode = CreateCSharpTestClass(fixedCodeSnippet);
            var diagnostic = VerifyCS.Diagnostic(PreferDictionaryTryAddOverGuardedAddAnalyzer.RuleId).WithLocation(0).WithLocation(1);

            return VerifyCS.VerifyCodeFixAsync(testCode, diagnostic, fixedCode);
        }

        [Theory]
        [InlineData(FakeDictionary)]
        [InlineData(LongRunningOperation)]
        [InlineData(LongRunningOperationCapturedInVariable)]
        [InlineData(WithObjectInstantiation)]
        [InlineData(NotGuardedByContainsKey)]
        [InlineData(AddOnDifferentDictionary)]
        [InlineData(KeyIsModified)]
        [InlineData(DictionaryIsReplaced)]
        public Task ShouldNotReportDiagnosticAsync(string codeSnippet)
        {
            string testCode = CreateCSharpTestClass(codeSnippet);

            return VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Theory]
        [InlineData(CheckNegativeReturnedValueVb, CheckNegativeReturnedValueVbFixed)]
        [InlineData(CheckNegativeReturnedValueWithElseRemovalVb, CheckNegativeReturnedValueWithElseRemovalVbFixed)]
        [InlineData(CheckNegativeReturnedValueWithElseVb, CheckNegativeReturnedValueWithElseVbFixed)]
        [InlineData(CheckNegativeReturnedValueAndPreserveLogicVb, CheckNegativeReturnedValueAndPreserveLogicVbFixed)]
        [InlineData(CheckPositiveReturnedValueAndPreserveLogicVb, CheckPositiveReturnedValueAndPreserveLogicVbFixed)]
        [InlineData(CheckPositiveReturnedValueAndPreserveLogicRedundantElseVb, CheckPositiveReturnedValueAndPreserveLogicRedundantElseVbFixed)]
        public Task ShouldReportDiagnosticVbAsync(string codeSnippet, string fixedCodeSnippet)
        {
            string testCode = CreateVbTestClass(codeSnippet);
            string fixedCode = CreateVbTestClass(fixedCodeSnippet);
            var diagnostic = VerifyVB.Diagnostic(PreferDictionaryTryAddOverGuardedAddAnalyzer.RuleId).WithLocation(0).WithLocation(1);

            return VerifyVB.VerifyCodeFixAsync(testCode, diagnostic, fixedCode);
        }

        [Theory]
        [InlineData(FakeDictionaryVb)]
        [InlineData(LongRunningOperationVb)]
        [InlineData(NotGuardedByContainsKeyVb)]
        [InlineData(AddOnDifferentDictionaryVb)]
        public Task ShouldNotReportDiagnosticVbAsync(string codeSnippet)
        {
            string testCode = CreateVbTestClass(codeSnippet);

            return VerifyVB.VerifyAnalyzerAsync(testCode);
        }

        private static string CreateCSharpTestClass(string content)
        {
            return string.Format(CultureInfo.InvariantCulture, CSharpTemplate, content);
        }

        private static string CreateVbTestClass(string content)
        {
            return string.Format(CultureInfo.InvariantCulture, VbTemplate, content);
        }
    }
}