﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Roslyn.Diagnostics.CSharp.Analyzers.PreferNullLiteral,
    Roslyn.Diagnostics.CSharp.Analyzers.PreferNullLiteralCodeFixProvider>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class PreferNullLiteralTests
    {
        [Theory]
        [InlineData("default")]
        [InlineData("default(object)")]
        public async Task PreferNullLiteral_Class(string defaultValueExpression)
        {
            var source = $@"
class Type
{{
    object Method()
    {{
        return [|{defaultValueExpression}|];
    }}
}}
";
            var fixedSource = @"
class Type
{
    object Method()
    {
        return null;
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("default(object)")]
        public async Task PreferNullLiteral_DefaultParameterValue(string defaultValueExpression)
        {
            var source = $@"
class Type
{{
    void Method(object value = [|{defaultValueExpression}|])
    {{
    }}
}}
";
            var fixedSource = @"
class Type
{
    void Method(object value = null)
    {
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task PreferNullLiteral_OverloadResolution()
        {
            var source = @"
using System;

class Type
{
    void Method()
    {
        Method2([|default(object)|]);
        Method2([|default(string)|]);
        Method2([|default(IComparable)|]);
        Method2([|default(int?)|]);
        Method2(default(int));
    }

    void Method2<T>(T value)
    {
    }
}
";
            var fixedSource = @"
using System;

class Type
{
    void Method()
    {
        Method2((object)null);
        Method2((string)null);
        Method2((IComparable)null);
        Method2((int?)null);
        Method2(default(int));
    }

    void Method2<T>(T value)
    {
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task PreferNullLiteral_ParenthesizeWhereNecessary()
        {
            var source = @"
using System;

class Type
{
    void Method()
    {
        Method2([|default(object)|]?.ToString());
        Method2([|default(string)|]?.ToString());
        Method2([|default(IComparable)|]?.ToString());
        Method2([|default(int?)|]?.ToString());
        Method2(default(int).ToString());
    }

    void Method2(string value)
    {
    }
}
";
            var fixedSource = @"
using System;

class Type
{
    void Method()
    {
        Method2(((object)null)?.ToString());
        Method2(((string)null)?.ToString());
        Method2(((IComparable)null)?.ToString());
        Method2(((int?)null)?.ToString());
        Method2(default(int).ToString());
    }

    void Method2(string value)
    {
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task PreferNullLiteral_Struct()
        {
            var source = @"
class Type
{
    int Method()
    {
        return default;
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PreferNullLiteral_StructConvertedToReferenceType()
        {
            var source = @"
class Type
{
    object Method()
    {
        return default(int);
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PreferNullLiteral_UnconstrainedGeneric()
        {
            var source = @"
class Type
{
    T Method<T>()
    {
        return default;
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PreferNullLiteral_GenericConstrainedToReferenceType()
        {
            var source = @"
class Type
{
    T Method<T>()
        where T : class
    {
        return [|default|];
    }
}
";
            var fixedSource = @"
class Type
{
    T Method<T>()
        where T : class
    {
        return null;
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task PreferNullLiteral_GenericConstrainedToInterface()
        {
            var source = @"
class Type
{
    T Method<T>()
        where T : System.IComparable
    {
        return default;
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PreferNullLiteral_GenericConstrainedToValueType()
        {
            var source = @"
class Type
{
    T Method<T>()
        where T : struct
    {
        return default;
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(source, source);
        }
    }
}
