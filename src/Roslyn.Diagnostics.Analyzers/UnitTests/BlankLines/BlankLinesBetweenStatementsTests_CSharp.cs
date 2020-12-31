// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslyn.Diagnostics.CSharp.Analyzers.BlankLines;
using Test.Utilities;
using Xunit;

namespace Roslyn.Diagnostics.Analyzers.UnitTests.BlankLines
{
    using Verify = CSharpCodeFixVerifier<
        CSharpBlankLinesBetweenStatementsDiagnosticAnalyzer,
        CSharpBlankLinesBetweenStatementsCodeFixProvider>;

    public class BlankLinesBetweenStatementsTests_CSharp
    {
        [Fact]
        public async Task TestNotAfterPropertyBlock()
        {
            var code =
@"
class C
{
    int X { get; }
    int Y { get; }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }
        [Fact]
        public async Task TestNotAfterMethodBlock()
        {
            var code =
@"
class C
{
    void X() { }
    void Y() { }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotAfterStatementsOnSingleLine()
        {
            var code =
@"
class C
{
    void M()
    {
        if (true) { } return;
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotAfterStatementsOnSingleLineWithComment()
        {
            var code =
@"
class C
{
    void M()
    {
        if (true) { }/*x*/return;
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotAfterStatementsOnMultipleLinesWithCommentBetween1()
        {
            var code =
@"
class C
{
    void M()
    {
        if (true)
        {
        }
        /*x*/ return;
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotAfterStatementsOnMultipleLinesWithCommentBetween2()
        {
            var code =
@"
class C
{
    void M()
    {
        if (true)
        {
        }
        /*x*/ return;
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotAfterStatementsOnMultipleLinesWithPPDirectiveBetween1()
        {
            var code =
@"
class C
{
    void M()
    {
        if (true)
        {
        }
        #pragma warning disable CS0001
        return;
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotBetweenBlockAndElseClause()
        {
            var code =
@"
class C
{
    void M()
    {
        if (true)
        {
        }
        else
        {
        }
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotBetweenBlockAndOuterBlocker()
        {
            var code =
@"
class C
{
    void M()
    {
        if (true)
        {
            {
            }
        }
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotBetweenBlockAndCase()
        {
            var code =
@"
class C
{
    void M()
    {
        switch (0)
        {
            case 0:
            {
                break;
            }
            case 1:
                break;
        }
    }
}";

            await new Verify.Test()
            {
                TestCode = code,
                FixedCode = code,
            }.RunAsync();
        }

        [Fact]
        public async Task TestBetweenBlockAndStatement1()
        {

            await new Verify.Test()
            {
                TestCode = @"
class C
{
    void M()
    {
        if (true)
        {
        [|}|]
        return;
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        if (true)
        {
        }

        return;
    }
}",
            }.RunAsync();
        }

        [Fact]
        public async Task TestBetweenBlockAndStatement2()
        {

            await new Verify.Test()
            {
                TestCode = @"
class C
{
    void M()
    {
        if (true)
        {
        [|}|] // trailing comment
        return;
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        if (true)
        {
        } // trailing comment

        return;
    }
}",
            }.RunAsync();
        }

        [Fact]
        public async Task TestBetweenBlockAndStatement3()
        {

            await new Verify.Test()
            {
                TestCode = @"
class C
{
    void M()
    {
        if (true) { [|}|]
        return;
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        if (true) { }

        return;
    }
}",
            }.RunAsync();
        }

        [Fact]
        public async Task TestBetweenBlockAndStatement4()
        {

            await new Verify.Test()
            {
                TestCode = @"
class C
{
    void M()
    {
        switch (0)
        {
        case 0:
            if (true) { [|}|]
            return;
        }
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        switch (0)
        {
        case 0:
            if (true) { }

            return;
        }
    }
}",
            }.RunAsync();
        }

        [Fact]
        public async Task TestFixAll1()
        {

            await new Verify.Test()
            {
                TestCode = @"
class C
{
    void M()
    {
        if (true)
        {
        [|}|]
        return;
        if (true)
        {
        [|}|]
        return;
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        if (true)
        {
        }

        return;
        if (true)
        {
        }

        return;
    }
}",
            }.RunAsync();
        }
    }
}