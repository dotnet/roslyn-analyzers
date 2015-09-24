// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace a2md
{
    public sealed class DescriptorEqualityComparer : IEqualityComparer<DiagnosticDescriptor>
    {
        public bool Equals(DiagnosticDescriptor x, DiagnosticDescriptor y) => x.Id.Equals(y.Id);

        public int GetHashCode(DiagnosticDescriptor obj) => obj.Id.GetHashCode();
    }
}
