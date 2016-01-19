using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;

namespace Analyzer.Utilities
{
    public static class IMethodSymbolExtensions
    {
        public static bool IsEqualsOverride(this IMethodSymbol method)
        {
            return method.IsOverride &&
                   method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                   method.Parameters.Length == 1 &&
                   method.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                   IsObjectMethodOverride(method);
        }

        public static bool IsGetHashCodeOverride(this IMethodSymbol method)
        {
            return method.IsOverride &&
                   method.ReturnType.SpecialType == SpecialType.System_Int32 &&
                   method.Parameters.Length == 0 &&
                   IsObjectMethodOverride(method);
        }

        public static bool IsObjectMethodOverride(this IMethodSymbol method)
        {
            var overriddenMethod = method.OverriddenMethod;
            while (overriddenMethod != null)
            {
                if (overriddenMethod.ContainingType.SpecialType == SpecialType.System_Object)
                {
                    return true;
                }

                overriddenMethod = overriddenMethod.OverriddenMethod;
            }

            return false;
        }

        public static bool IsEqualsInterfaceImplementation(this IMethodSymbol method, Compilation compilation)
        {
            if (method.Name != "Equals")
            { 
                return false;
            }

            int paramCount = method.Parameters.Length;
            if (method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                (paramCount == 1 || paramCount == 2))
            {
                var iEqualityComparer = compilation.GetTypeByMetadataName("System.IEqualityComparer");
                var iEqualityComparerEquals = iEqualityComparer.GetMembers("Equals").Single();

                if (method.ContainingType.FindImplementationForInterfaceMember(iEqualityComparerEquals) == method)
                {
                    return true;
                }

                var iEquatable = compilation.GetTypeByMetadataName("System.IEquatable");
                var iEquatableEquals = iEquatable.GetMembers("Equals").Single();

                if (method.ContainingType.FindImplementationForInterfaceMember(iEquatableEquals) == method)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsGetHashCodeInterfaceImplementation(this IMethodSymbol method, Compilation compilation)
        {
            if (method.Name != "GetHashCode")
            {
                return false;
            }

            if (method.ReturnType.SpecialType == SpecialType.System_Int32 && method.Parameters.Length == 1)
            {
                var iEqualityComparer = compilation.GetTypeByMetadataName("System.IEqualityComparer");
                var iEqualityComparerGetHashCode = iEqualityComparer.GetMembers("Equals").Single();

                if (method.ContainingType.FindImplementationForInterfaceMember(iEqualityComparerGetHashCode) == method)
                {
                    return true;
                }

                var iHashCodeProvider = compilation.GetTypeByMetadataName("System.Collections.IHashCodeProvider");
                var iHashCodeProviderGetHashCode = iHashCodeProvider.GetMembers("Equals").Single();

                if (method.ContainingType.FindImplementationForInterfaceMember(iHashCodeProviderGetHashCode) == method)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsToString(this IMethodSymbol method)
        {
            return (method != null &&
                    method.ReturnType.SpecialType == SpecialType.System_String &&
                    method.Name == WellKnownMemberNames.ObjectToString &&
                    method.Parameters.Length == 0);
        }

        public static bool IsFinalizer(this IMethodSymbol method)
        {
            if (method.MethodKind == MethodKind.Destructor)
            {
                return true; // for C#
            }

            if (method.Name != "Finalize" || method.Parameters.Length != 0 || !method.ReturnsVoid)
            {
                return false;
            }

            var overridden = method.OverriddenMethod;
            if (overridden == null)
            {
                return false;
            }

            for (var o = overridden.OverriddenMethod; o != null; o = o.OverriddenMethod)
            {
                overridden = o;
            }

            return overridden.ContainingType.SpecialType == SpecialType.System_Object; // it is object.Finalize
        }
        public static bool IsDisposeImplementation(this IMethodSymbol method, Compilation compilation)
        {
            if (method.Name != "Dispose")
            {
                return false;
            }

            if (method.ReturnType.SpecialType == SpecialType.System_Void && method.Parameters.Length == 0)
            {
                var iDisposable = compilation.GetTypeByMetadataName("System.IDisposable");
                var iDisposableDispose = iDisposable.GetMembers("Dispose").Single();

                if (method.ContainingType.FindImplementationForInterfaceMember(iDisposableDispose) == method)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
