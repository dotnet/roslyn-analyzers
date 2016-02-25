// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace AnalyzersStatusGenerator
{
    /// <summary>
    /// An equality comparer to compare two <see cref="DiagnosticDescriptor"/>. They are considered
    /// equal if their ids are equal.
    /// </summary>
    public sealed class DescriptorEqualityComparer : IEqualityComparer<DiagnosticDescriptor>
    {
        public bool Equals(DiagnosticDescriptor x, DiagnosticDescriptor y) => x.Id.Equals(y.Id);

        public int GetHashCode(DiagnosticDescriptor obj) => obj.Id.GetHashCode();
    }
}
