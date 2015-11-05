using System.Collections.Generic;

namespace Roslyn.Analyzers.SolutionGenerator
{
    internal class CheckData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Messages { get; set; }
        public string AnalyzerProject { get; set; }
        public string Category { get; set; }
        public PortStatus Port { get; set; }
    }
}
