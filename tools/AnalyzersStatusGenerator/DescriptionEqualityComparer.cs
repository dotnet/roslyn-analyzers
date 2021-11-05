// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
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
        public bool Equals(DiagnosticDescriptor x, DiagnosticDescriptor y) => x.Id.Equals(y.Id, StringComparison.Ordinal);

        // CA1720: Identifier 'obj' contains type name
        // TOODO: Remove the below suppression once https://github.com/dotnet/roslyn-analyzers/issues/938 is fixed.
#pragma warning disable CA1720
        public int GetHashCode(DiagnosticDescriptor obj) => obj.Id.GetHashCode();
#pragma warning restore CA1720
    }
}
