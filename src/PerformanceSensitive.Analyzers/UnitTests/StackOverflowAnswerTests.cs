// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    // Taken from http://stackoverflow.com/questions/7995606/boxing-occurrence-in-c-sharp

    public partial class ExplicitAllocationAnalyzerTests
    {
        [Fact]
        public void Converting_any_value_type_to_System_Object_type()
        {
            var source = @"
using Roslyn.Utilities;

public struct S { }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        object box = new S();
    }
}";
            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(11,22): info HAA0502: Explicit new reference type allocation
                        GetCSharpResultAt(11, 22, ExplicitAllocationAnalyzer.NewObjectRule));
        }

        [Fact]
        public void Converting_any_value_type_to_System_ValueType_type()
        {
            var source = @"
using Roslyn.Utilities;

public struct S { }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        System.ValueType box = new S();
    }
}";
            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(11,32): info HAA0502: Explicit new reference type allocation
                        GetCSharpResultAt(11, 32, ExplicitAllocationAnalyzer.NewObjectRule));
        }

        [Fact]
        public void Converting_any_value_type_into_interface_reference()
        {
            var source = @"
using Roslyn.Utilities;

interface I { }

public struct S : I { }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        I box = new S();
    }
}";
            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(13,17): info HAA0502: Explicit new reference type allocation
                        GetCSharpResultAt(13, 17, ExplicitAllocationAnalyzer.NewObjectRule));
        }
    }

    public partial class TypeConversionAllocationAnalyzerTests
    {

        [Fact]
        public void Converting_any_enumeration_type_to_System_Enum_type()
        {
            var source = @"
using Roslyn.Utilities;

enum E { A }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        System.Enum box = E.A;
    }
}";
            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(11,27): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(11, 27, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void Creating_delegate_from_value_type_instance_method()
        {
            var source = @"
using System;
using Roslyn.Utilities;

struct S { public void M() {} }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        Action box = new S().M;
    }
}";
            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(12,22): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(12, 22, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                        // Test0.cs(12,22): warning HAA0602: Struct instance method being used for delegate creation, this will result in a boxing instruction
                        GetCSharpResultAt(12, 22, TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule));
        }
    }

    public partial class CallSiteImplicitAllocationAnalyzerTests
    {
        [Fact]
        public void Calling_non_overridden_virtual_methods_on_value_types()
        {
            var source = @"
using System;
using Roslyn.Utilities;

enum E { A }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        E.A.GetHashCode();
    }
}";
            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(12,9): warning HAA0102: Non-overridden virtual method call on a value type adds a boxing or constrained instruction
                        GetCSharpResultAt(12, 9, CallSiteImplicitAllocationAnalyzer.ValueTypeNonOverridenCallRule));
        }
    }

    public partial class ConcatenationAllocationAnalyzerTests
    {
        [Fact]
        public void Non_constant_value_types_in_CSharp_string_concatenation()
        {
            var source = @"
using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        System.DateTime c = System.DateTime.Now;
        string s1 = ""char value will box"" + c;
    }
}";
            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(11,45): warning HAA0202: Value type (System.DateTime) is being boxed to a reference type for a string concatenation.
                        GetCSharpResultAt(11, 45, ConcatenationAllocationAnalyzer.ValueTypeToReferenceTypeInAStringConcatenationRule, "System.DateTime"));
        }
    }
}
