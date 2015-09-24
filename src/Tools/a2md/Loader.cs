using System.Reflection;
using Microsoft.CodeAnalysis;

namespace a2md
{
    public sealed class Loader : IAnalyzerAssemblyLoader
    {
        public void AddDependencyLocation(string fullPath)
        {
        }

        public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
    }
}
