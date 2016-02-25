// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    public static class IMethodSymbolExtensions
    {
        /// <summary>
        /// Checks if the given method overrides Object.Equals.
        /// </summary>
        public static bool IsEqualsOverride(this IMethodSymbol method)
        {
            return method != null &&
                   method.IsOverride &&
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
            return method != null &&
                   method.IsOverride &&
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
            return method != null &&
                   method.IsOverride &&
                   method.ReturnType.SpecialType == SpecialType.System_String &&
                   method.Name == WellKnownMemberNames.ObjectToString &&
                   method.Parameters.Length == 0 &&
                   IsObjectMethodOverride(method);
        }

        /// <summary>
        /// Checks if the given method overrides a method from System.Object
        /// </summary>
        private static bool IsObjectMethodOverride(IMethodSymbol method)
        {
            IMethodSymbol overriddenMethod = method.OverriddenMethod;
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

            IMethodSymbol overridden = method.OverriddenMethod;
            if (overridden == null)
            {
                return false;
            }

            for (IMethodSymbol o = overridden.OverriddenMethod; o != null; o = o.OverriddenMethod)
            {
                overridden = o;
            }

            return overridden.ContainingType.SpecialType == SpecialType.System_Object; // it is object.Finalize
        }

        /// <summary>
        /// Checks if the given method is an implementation of the given interface method 
        /// Substituted with the given typeargument.
        /// </summary>
        public static bool IsImplementationOfInterfaceMethod(this IMethodSymbol method, ITypeSymbol typeArgument, INamedTypeSymbol interfaceType, string interfaceMethodName)
        {
            INamedTypeSymbol constructedInterface = typeArgument != null ? interfaceType?.Construct(typeArgument) : interfaceType;
            var interfaceMethod = constructedInterface?.GetMembers(interfaceMethodName).Single() as IMethodSymbol;

            return IsInterfaceMethodImplementation(method, interfaceMethod);
        }

        /// <summary>
        /// Checks if the given method implements any interface method, implicitly or explicitly.
        /// </summary>
        public static bool IsImplementationOfAnyInterfaceMethod(this IMethodSymbol method)
        {
            if (method.ExplicitInterfaceImplementations.Length > 0)
            {
                return true;
            }

            // This is an approximation, because another class could derive from this one
            // and rely on methodSymbol implementing one of *it's* interfaces methods, but
            // it's good enough.
            foreach (INamedTypeSymbol interfaceSymbol in method.ContainingType.AllInterfaces)
            {
                foreach (IMethodSymbol interfaceMethod in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    if (IsInterfaceMethodImplementation(method, interfaceMethod))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsInterfaceMethodImplementation(IMethodSymbol method, IMethodSymbol interfaceMethod)
        {
            return interfaceMethod != null && method.Equals(method.ContainingType.FindImplementationForInterfaceMember(interfaceMethod));
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
                // Identify the implementor of IDisposable.Dispose in the given method's containing type and check
                // if it is the given method.
                INamedTypeSymbol iDisposable = WellKnownTypes.IDisposable(compilation);
                if (method.IsImplementationOfInterfaceMethod(null, iDisposable, "Dispose"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the method is a property getter.
        /// </summary>
        public static bool IsPropertyGetter(this IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.PropertyGet &&
                   method.AssociatedSymbol?.GetParameters().Length == 0;
        }

        /// <summary>
        /// Checks if the method is the getter for an indexer.
        /// </summary>
        public static bool IsIndexerGetter(this IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.PropertyGet &&
                   method.AssociatedSymbol.IsIndexer();
        }

        /// <summary>
        /// Checks if the method is an accessor for an event.
        /// </summary>
        public static bool IsEventAccessor(this IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.EventAdd ||
                   method.MethodKind == MethodKind.EventRaise ||
                   method.MethodKind == MethodKind.EventRemove;
        }
    }
}
