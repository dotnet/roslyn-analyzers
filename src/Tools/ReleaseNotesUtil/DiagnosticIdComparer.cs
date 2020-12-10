// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReleaseNotesUtil
{
    internal class DiagnosticIdComparer : IEqualityComparer<DiagnosticDescriptor>
    {
        public static readonly DiagnosticIdComparer Instance = new();

        public bool Equals(DiagnosticDescriptor x, DiagnosticDescriptor y)
        {
            return StringComparer.Ordinal.Equals(x.Id, y.Id);
        }

        public int GetHashCode(DiagnosticDescriptor obj)
        {
            return StringComparer.Ordinal.GetHashCode(obj.Id);
        }
    }
}
