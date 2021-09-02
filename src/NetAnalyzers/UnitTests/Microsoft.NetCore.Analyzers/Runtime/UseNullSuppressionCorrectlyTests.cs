// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using NullSuppressionAnalyzer = Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseNullSuppressionCorrectlyAnalyzer;
using CSharpLanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseNullSuppressionCorrectlyAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseNullSuppressionCorrectlyFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class UseNullSuppressionCorrectlyTests
    {
        [Fact]
        public async Task IdentifySuppressedNullLiterals()
        {
            // Null suppression for null literals defeats the purpose of null suppression
            await new VerifyCS.Test
            {
                TestCode = @"
using System;

public class A
{
    public void B()
    {
        string x = {|#0:null!|};
        Console.WriteLine(x.Length);
    }
}",
                ExpectedDiagnostics = { VerifyCS.Diagnostic(NullSuppressionAnalyzer.LiteralAlwaysNullRule).WithLocation(0) },
                FixedCode = @"
using System;

public class A
{
    public void B()
    {
        string x = null;
        Console.WriteLine(x.Length);
    }
}
",
                LanguageVersion = CSharpLanguageVersion.Default,
            }.RunAsync();
        }

        [Fact]
        public async Task IdentifySuppressedDefaultNullLiterals()
        {
            // Null suppression for null-valued default literals defeats the purpose of null suppression
            await new VerifyCS.Test
            {
                TestCode = @"
using System;

public class A
{
    public void B()
    {
        string x = {|#0:default!|};
        Console.WriteLine(x.Length);
    }
}
",
                ExpectedDiagnostics = { VerifyCS.Diagnostic(NullSuppressionAnalyzer.LiteralAlwaysNullRule).WithLocation(0) },
                FixedCode = @"
using System;

public class A
{
    public void B()
    {
        string x = default;
        Console.WriteLine(x.Length);
    }
}
",
                LanguageVersion = CSharpLanguageVersion.Default,
            }.RunAsync();
        }

        [Fact]
        public async Task IdentifySuppressedDefaultNonNullLiterals()
        {
            // Non-null default literals don't need null suppression
            await new VerifyCS.Test
            {
                TestCode = @"
using System;

public class A
{
    public void B()
    {
        DateTime x = {|#0:default!|};
        Console.WriteLine(x.Day);
    }
}
",
                ExpectedDiagnostics = { VerifyCS.Diagnostic(NullSuppressionAnalyzer.NeverNullLiteralsRule).WithLocation(0) },
                FixedCode = @"
using System;

public class A
{
    public void B()
    {
        DateTime x = default;
        Console.WriteLine(x.Day);
    }
}
",
                LanguageVersion = CSharpLanguageVersion.Default,
            }.RunAsync();
        }

        [Fact]
        public async Task IdentifySuppressedLiterals()
        {
            // We have a non-null or non-default literal, which should not need null suppression
            await new VerifyCS.Test
            {
                TestCode = @"
using System;

public class A
{
    public void B()
    {
        string x = {|#0:""hi""!|};
        Console.WriteLine(x.Length);
    }
}
",
                ExpectedDiagnostics = { VerifyCS.Diagnostic(NullSuppressionAnalyzer.NeverNullLiteralsRule).WithLocation(0) },
                FixedCode = @"
using System;

public class A
{
    public void B()
    {
        string x = ""hi"";
        Console.WriteLine(x.Length);
    }
}
",
                LanguageVersion = CSharpLanguageVersion.Default,
            }.RunAsync();
        }
    }
}