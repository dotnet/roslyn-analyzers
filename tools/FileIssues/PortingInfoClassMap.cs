using CsvHelper.Configuration;

namespace FileIssues
{
    /// <summary>
    /// Class used by CsvHelper to map columns from the CSV file to properties of
    /// the <see cref="PortingInfo"/> class.
    /// </summary>
    public sealed class PortingInfoClassMap: CsvClassMap<PortingInfo>
    {
        public PortingInfoClassMap()
        {
            Map(pi => pi.Id).Name("Id");
            Map(pi => pi.Name).Name("Name");
            Map(pi => pi.Title).Name("Title");
            Map(pi => pi.Description).Name("Description");
            Map(pi => pi.Notes).Name("Notes");
            Map(pi => pi.ProposedAnalyzer).Name("Proposed Analyzer");
            Map(pi => pi.Disposition).Name("Port?").TypeConverter<DispositionConverter>();
            Map(pi => pi.RevisedPriority).Name("Revised Priority").TypeConverter<PriorityConverter>();
            Map(pi => pi.OriginalPriority).Name("Original Priority").TypeConverter<PriorityConverter>();
            Map(pi => pi.Dependency).Name("Dependency").TypeConverter<DependencyConverter>();
        }
    }
}
