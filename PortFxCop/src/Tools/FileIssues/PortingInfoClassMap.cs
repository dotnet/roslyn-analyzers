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
            Map(pi => pi.ShouldPort).Name("Port?").TypeConverter<ShouldPortConverter>();
        }
    }
}
