using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReleaseNotesUtil
{
    internal class DiagnosticIdComparer : IEqualityComparer<DiagnosticDescriptor>
    {
        public static readonly DiagnosticIdComparer Instance = new DiagnosticIdComparer();

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
