using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities.Extensions
{
    public static class IEnumerableOfIMethodSymbolExtensions
    {
        public static IEnumerable<IMethodSymbol> GetMethodOverloadsWithDesiredParameterAtLeadingOrTrailing(
            this IEnumerable<IMethodSymbol> methods,
            IMethodSymbol selectedOverload,
            INamedTypeSymbol expectedParameterType)
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

                if (candidateMethod.Parameters.First().Type.Equals(expectedParameterType))
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
    }
}
