using System.Reflection;
using Microsoft.CodeAnalysis;

namespace ReleaseNotesUtil
{
    internal sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
    {
        public static IAnalyzerAssemblyLoader Instance = new AnalyzerAssemblyLoader();

        private AnalyzerAssemblyLoader() { }

        public void AddDependencyLocation(string fullPath)
        {
        }

        public Assembly LoadFromPath(string fullPath)
        {
            return Assembly.LoadFrom(fullPath);
        }
    }
}
