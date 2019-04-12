// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Diagnostics.CSharp.Analyzers;
using Roslyn.Diagnostics.VisualBasic.Analyzers;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.ImplementWithNoMainThreadDependency,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.ImplementWithNoMainThreadDependency,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class ImplementWithNoMainThreadDependencyTests
    {
        [Fact]
        public async Task TestInterfaceMethod()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Roslyn.Utilities;

interface Interface {
    [NoMainThreadDependency]
    Task MethodAsync();
}

class Class : Interface {
    [NoMainThreadDependency]
    public Task MethodAsync() => throw null;
}
";

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, TestResources.NoMainThreadDependencyAttribute, TestResources.AsyncEntryAttribute },
                },
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task TestInterfaceMethod_MissingImpl()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Roslyn.Utilities;

interface Interface {
    [NoMainThreadDependency]
    Task MethodAsync();
}

class Class : Interface {
    [|[|public Task MethodAsync() => throw null;|]|]
}
";

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, TestResources.NoMainThreadDependencyAttribute, TestResources.AsyncEntryAttribute },
                },
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task TestInterfaceMethod_CannotCaptureContext()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Roslyn.Utilities;

interface Interface {
    [return: NoMainThreadDependency]
    Task MethodAsync();
}

class Class : Interface {
    [|[return: NoMainThreadDependency(CapturesContext = true)]
    public Task MethodAsync() => throw null;|]
}
";

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, TestResources.NoMainThreadDependencyAttribute, TestResources.AsyncEntryAttribute },
                },
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task TestInterfaceMethod_WithoutVerification()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Roslyn.Utilities;

interface Interface {
    [NoMainThreadDependency(Verified = false)]
    Task MethodAsync();
}

class Class : Interface {
    [NoMainThreadDependency(Verified = false)]
    public Task MethodAsync() => throw null;
}
";

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, TestResources.NoMainThreadDependencyAttribute, TestResources.AsyncEntryAttribute },
                },
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task TestInterfaceMethod_MustVerifyIfBaseVerified()
        {
            var source = @"
using System;
using System.Threading.Tasks;
using Roslyn.Utilities;

interface Interface {
    [return: NoMainThreadDependency]
    Task MethodAsync();
}

class Class : Interface {
    [|[return: NoMainThreadDependency(Verified = false)]
    public Task MethodAsync() => throw null;|]
}
";

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, TestResources.NoMainThreadDependencyAttribute, TestResources.AsyncEntryAttribute },
                },
            };

            await test.RunAsync();
        }
    }
}
