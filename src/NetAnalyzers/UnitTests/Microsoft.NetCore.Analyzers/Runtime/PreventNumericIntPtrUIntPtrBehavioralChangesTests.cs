// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpPreventNumericIntPtrUIntPtrBehavioralChanges,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreventNumericIntPtrUIntPtrBehavioralChangesTests
    {

        [Fact]
        public async Task IntPtrAdditionWithFieldReference()
        {
            await PopulateTestCs(@"
using System;

class Program
{
    IntPtr intPtr1;
    IntPtr intPtr2;

    public void M1()
    {
        checked
        {
            intPtr2 = {|#0:intPtr1 + 2|}; // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.
            intPtr1++; 
            intPtr1+=2;
        }

        intPtr2 = checked({|#1:intPtr1 + 2|}); // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.

        intPtr2 = intPtr1 + 2; // unchecked context 
        intPtr2 = 2 + intPtr1; // Assume it should not warn here

        checked
        {
            intPtr2 = unchecked(intPtr1 + 2); // wrapped with unchecked
        }
    }
}",
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(0).WithArguments("+"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(1).WithArguments("+")).RunAsync();
        }

        [Fact]
        public async Task NintAdditionNotWarn()
        {
            await PopulateTestCs(@"
using System;

class Program
{
    nint nint1;
    nint nint2;

    public void M1()
    {
        checked
        {
            nint2 = nint1 + 2;
        }

        nint2 = checked(nint1 + 2);

        nint2 = nint1 + 2;
    }
}").RunAsync();
        }

        [Fact]
        public async Task IntPtrAdditionWithParameterReference()
        {
            await PopulateTestCs(@"
using System;

class Program
{
    private IntPtr M2(IntPtr intPtr, int a)
    {
        return checked({|#0:intPtr + a|}); // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.
    }

    private IntPtr M3(IntPtr intPtr, int a)
    {
        return intPtr + a; 
    }

    private nint M4(IntPtr intPtr, int a)
    {
        return checked({|#1:intPtr + a|}); // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.
    }

    private nint M5(IntPtr intPtr, int a)
    {
        return intPtr + a;
    }

    private IntPtr M6(nint intPtr, int a)
    {
        return checked(intPtr + a);
    }

    private IntPtr M7(nint intPtr, int a)
    {
        return intPtr + a;
    }
}",
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(0).WithArguments("+"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(1).WithArguments("+")).RunAsync();
        }

        [Fact]
        public async Task UIntPtrAdditionWithFieldReference()
        {
            await PopulateTestCs(@"
using System;

class Program
{
    UIntPtr uintPutr1;
    UIntPtr uintPtr2;

    public void M1()
    {
        checked
        {
            uintPtr2 = {|#0:uintPutr1 + 2|}; // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.
        }

        uintPtr2 = checked({|#1:uintPutr1 + 2|}); // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.

        uintPtr2 = uintPutr1 + 2;
    }
}",
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(0).WithArguments("+"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(1).WithArguments("+")).RunAsync();
        }

        [Fact]
        public async Task IntPtrAdditionWithLocalReference()
        {
            await PopulateTestCs(@"
using System;

class Program
{
    public void M1()
    {
        IntPtr intPtr1 = IntPtr.Zero;
        IntPtr intPtr2;

        checked
        {
            intPtr2 = {|#0:intPtr1 + 2|}; // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.
        }

        intPtr2 = checked({|#1:intPtr1 + 2|}); // Built in operator '+' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.

        intPtr2 = intPtr1 + 2;

        checked
        {
            intPtr2 = unchecked(intPtr1 + 2);
        }
    }
}",
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(0).WithArguments("+"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.OperatorThrowsRule).WithLocation(1).WithArguments("+")).RunAsync();
        }

        [Fact]
        public async Task IntPtrExplicitConversion()
        {
            await PopulateTestCs(@"
using System;

class Program
{
    IntPtr intPtr1 = IntPtr.Zero;
    IntPtr intPtr2;
    
    public unsafe void M1(UIntPtr uintPtr1, ulong uLongValue)
    {
        void* ptr = null;
        long longValue = 0;

        checked
        {
            ptr = {|#0:(void*)intPtr1|}; // Built in explicit conversion '(*Void)IntPtr' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.

            intPtr2 = {|#1:(IntPtr)ptr|}; // Built in explicit conversion '(IntPtr)*Void' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.
        }

        intPtr1 = {|#2:(IntPtr)longValue|}; // Built in operator '(IntPtr)Int64' now may not throw when overflowing in unchecked context. Wrap the expression with 'checked' statement to restore old behavior.

        int a = {|#3:(int)intPtr1|}; // Built in explicit conversion '(Int32)IntPtr' now may throw when overflowing in checked context. Wrap the expression with 'unchecked' statement to restore old behavior.

        uintPtr1 = {|#4:(UIntPtr)uLongValue|}; // Built in operator '(UIntPtr)UInt64' now may not throw when overflowing in unchecked context. Wrap the expression with 'checked' statement to restore old behavior.

        uint ui = {|#5:(uint)uintPtr1|}; // Built in operator '(UInt32)UIntPtr' now may not throw when overflowing in unchecked context. Wrap the expression with 'checked' statement to restore old behavior.

        checked
        {
            intPtr2 = unchecked(intPtr1 + 2);
        }
    }
}",
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.ConversionThrowsRule).WithLocation(0).WithArguments("(*Void)IntPtr"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.ConversionThrowsRule).WithLocation(1).WithArguments("(IntPtr)*Void"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.NotThrowRule).WithLocation(2).WithArguments("(IntPtr)Int64"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.NotThrowRule).WithLocation(3).WithArguments("(Int32)IntPtr"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.NotThrowRule).WithLocation(4).WithArguments("(UIntPtr)UInt64"),
            VerifyCS.Diagnostic(PreventNumericIntPtrUIntPtrBehavioralChanges.NotThrowRule).WithLocation(5).WithArguments("(UInt32)UIntPtr")).RunAsync();
        }

        private static VerifyCS.Test PopulateTestCs(string sourceCode, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview
            };
            test.ExpectedDiagnostics.AddRange(expected);
            return test;
        }
    }
}
