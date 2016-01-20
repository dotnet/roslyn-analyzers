using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;

namespace Analyzer.Utilities
{
    public static class IMethodSymbolExtensions
    {
        /// <summary>
        /// Checks if the given method overrides Object.Equals.
        /// </summary>
        public static bool IsEqualsOverride(this IMethodSymbol method)
        {
            return method.IsOverride &&
                   method.Name == WellKnownMemberNames.ObjectEquals &&
                   method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                   method.Parameters.Length == 1 &&
                   method.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                   IsObjectMethodOverride(method);
        }

        /// <summary>
        /// Checks if the given method overrides Object.GetHashCode.
        /// </summary>
        public static bool IsGetHashCodeOverride(this IMethodSymbol method)
        {
            return method.IsOverride &&
                   method.Name == WellKnownMemberNames.ObjectGetHashCode &&  
                   method.ReturnType.SpecialType == SpecialType.System_Int32 &&
                   method.Parameters.Length == 0 &&
                   IsObjectMethodOverride(method);
        }

        /// <summary>
        /// Checks if the given method overrides Object.ToString.
        /// </summary>
        public static bool IsToStringOverride(this IMethodSymbol method)
        {
            return (method != null &&
                    method.ReturnType.SpecialType == SpecialType.System_String &&
                    method.Name == WellKnownMemberNames.ObjectToString &&
                    method.Parameters.Length == 0 &&
                    IsObjectMethodOverride(method));
        }

        /// <summary>
        /// Checks if the given method overrides a method from System.Object
        /// </summary>
        private static bool IsObjectMethodOverride(IMethodSymbol method)
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

        /// <summary>
        /// Checks if the given method is a Finalizer implementation.
        /// </summary>
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

        /// <summary>
        /// Checks if the given method implements IDisposable.Dispose()
        /// </summary>
        public static bool IsDisposeImplementation(this IMethodSymbol method, Compilation compilation)
        {
            if (method.Name != "Dispose")
            {
                return false;
            }

            if (method.ReturnType.SpecialType == SpecialType.System_Void && method.Parameters.Length == 0)
            {
                // Identify the implemtor of IDisposable.Dispose in the given method's containing type and check
                // if it is the given method.
                var iDisposable = compilation.GetTypeByMetadataName("System.IDisposable");
                var iDisposableDispose = iDisposable?.GetMembers("Dispose").Single();

                if (iDisposableDispose != null && method.ContainingType.FindImplementationForInterfaceMember(iDisposableDispose) == method)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
