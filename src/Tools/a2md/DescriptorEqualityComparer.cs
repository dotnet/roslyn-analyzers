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
