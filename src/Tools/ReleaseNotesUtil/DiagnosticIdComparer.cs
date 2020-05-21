// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReleaseNotesUtil
{
    internal class DiagnosticIdComparer : IEqualityComparer<DiagnosticDescriptor>
    {
        public static readonly DiagnosticIdComparer Instance = new DiagnosticIdComparer();

        public bool Equals([AllowNull] DiagnosticDescriptor x, [AllowNull] DiagnosticDescriptor y)
        {
            return StringComparer.Ordinal.Equals(x?.Id, y?.Id);
        }

        public int GetHashCode(DiagnosticDescriptor obj)
        {
            return StringComparer.Ordinal.GetHashCode(obj.Id);
        }
    }
}
