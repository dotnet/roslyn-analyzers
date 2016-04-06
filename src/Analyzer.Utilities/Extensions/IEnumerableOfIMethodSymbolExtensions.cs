using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    public static class IEnumerableOfIMethodSymbolExtensions
    {
        /// <summary>
        /// Returns a list of method symbols from a given list of the method symbols, which has its parameter type as
        /// expectedParameterType as its first parameter or the last parameter in addition to matching all the other 
        /// parameter types of the selectedOverload method symbol
        /// </summary>
        /// <param name="methods">List of <see cref="IMethodSymbol"/> to scan for possible overloads</param>
        /// <param name="selectedOverload"><see cref="IMethodSymbol"/> that is currently picked by the user</param>
        /// <param name="expectedParameterType"><see cref="INamedTypeSymbol"/> type of the leading parameter or the trailing parameter</param>
        /// <param name="trailingOnly"><see cref="INamedTypeSymbol"/> If the expected parameter should appear at the trailing position of the parameter list of the method overload</param>
        public static IEnumerable<IMethodSymbol> GetMethodOverloadsWithDesiredParameterAtLeadingOrTrailing(
             this IEnumerable<IMethodSymbol> methods,
             IMethodSymbol selectedOverload,
             INamedTypeSymbol expectedParameterType,
             bool trailingOnly = false)
        {
            return methods.Where(candidateMethod =>
            {
                if (selectedOverload.Parameters.Count() + 1 != candidateMethod.Parameters.Count())
                {
                    return false;
                }

                 // The expected method overload should either have the expectedParameterType parameter as the first argument or as the last argument
                 // Assume expectedParameterType is the last parameter so j, which is the index of the parameter
                 // in candidateMethod to compare against selectedOverload's parameter is set to 0
                 int j = 0;

                if (!trailingOnly && candidateMethod.Parameters.First().Type.Equals(expectedParameterType))
                {
                     // If expectedParameterType is the first parameter then the parameters to compare in candidateMethod against selectedOverload
                     // is offset by 1
                     j = 1;
                }
                else if (!candidateMethod.Parameters.Last().Type.Equals(expectedParameterType))
                {
                     // expectedParameterType is neither the first parameter nor the last parameter
                     return false;
                }

                for (int i = 0; i < selectedOverload.Parameters.Count(); i++, j++)
                {
                    if (!selectedOverload.Parameters[i].Type.Equals(candidateMethod.Parameters[j].Type) ||
                        selectedOverload.Parameters[i].IsParams != candidateMethod.Parameters[j].IsParams)
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        /// <summary>
        /// Returns a list of method symbols from a given list of the method symbols, which has its parameter type as
        /// expectedParameterType as its last parameter in addition to matching all the other parameter types of the 
        /// selectedOverload method symbol
        /// </summary>
        /// <param name="methods">List of <see cref="IMethodSymbol"/> to scan for possible overloads</param>
        /// <param name="selectedOverload"><see cref="IMethodSymbol"/> that is currently picked by the user</param>
        /// <param name="expectedParameterType"><see cref="INamedTypeSymbol"/> type of the leading parameter or the trailing parameter</param>
        public static IEnumerable<IMethodSymbol> GetMethodOverloadsWithDesiredParameterAtTrailing(
             this IEnumerable<IMethodSymbol> methods,
             IMethodSymbol selectedOverload,
             INamedTypeSymbol expectedParameterType)
        {
            return GetMethodOverloadsWithDesiredParameterAtLeadingOrTrailing(methods, selectedOverload, expectedParameterType, true);
        }

        /// <summary>
        /// Return all the method symbols with the name matching the displayName case-insensitively
        /// </summary>
        /// <param name="stringFormatMembers"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static IMethodSymbol GetSingleOrDefaultMemberWithName(this IEnumerable<IMethodSymbol> stringFormatMembers, string displayName)
        {
             return stringFormatMembers?.Where(member => string.Equals(member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), displayName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
        }
    }
}
