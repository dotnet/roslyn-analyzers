using Newtonsoft.Json;

namespace AnalyzersStatusGenerator
{
    /// <summary>
    /// Represents a diagnostic in the RoslynAnalyzers projects
    /// </summary>
    public class AnalyzersStatusInfo
    {
        /// <summary>
        /// The Id of the diagnostic including the prefix 'SA' or 'SX'
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The short name if the diagnostic that is used in the class name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if the rule is implemented for C# projects.
        /// </summary>
        public bool HasCSharpImplementation { get; set; }

        /// <summary>
        /// True if the rule is implemented for VB projects.
        /// </summary>
        public bool HasVBImplementation { get; set; }

        /// <summary>
        /// Represents if the diagnostic is enabled or not. This can indicate if the
        /// diagnostic is enabled by default or not, or if it is disabled because
        /// there are no tests for the diagnostic.
        /// </summary>
        public string IsEnabledByDefault { get; set; }

        /// <summary>
        /// Represents whether or not there is a code fix for the diagnostic.
        /// </summary>
        public bool HasCodeFix { get; set; }

        /// <summary>
        /// Returns the title of this diagnostic
        /// no reason.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Returns the category of this diagnostic
        /// no reason.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Returns help link for this diagnostic
        /// </summary>
        public string HelpLink { get; set; }

        /// <summary>
        /// Returns the analyzer package to which this diagnostic belongs.
        /// </summary>
        public string AnalyzerPackage { get; set; }

        /// <summary>
        /// Returns a string representing this diagnostic
        /// </summary>
        public override string ToString()
        {
            return this.Id + " " + this.Name;
        }

        /// <summary>
        /// Returns a json representation of this diagnostic
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Creates an instance of the <see cref="AnalyzersStatusInfo"/> class
        /// that is populated with the data stored in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">A json representing a <see cref="AnalyzersStatusInfo"/></param>
        /// <returns>A <see cref="AnalyzersStatusInfo"/> that is populated with the data stored in <paramref name="value"/>.</returns>
        public static AnalyzersStatusInfo FromJson(string value)
        {
            return JsonConvert.DeserializeObject<AnalyzersStatusInfo>(value);
        }
    }
}
