// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public static class AssertEx
    {
        public static void ContainsDiagnostic(ImmutableArray<Diagnostic> diagnostics, string id, int line, int character)
        {
            Assert.Equal(1, diagnostics.Count(d =>
                                d.Id == id &&
                                d.Location.GetLineSpan().StartLinePosition.Line + 1 == line &&
                                d.Location.GetLineSpan().StartLinePosition.Character + 1 == character));
        }
    }
}
