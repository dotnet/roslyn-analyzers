namespace FileIssues
{
    public class PortingInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool ShouldPort { get; set; }

        public override string ToString()
        {
            return $"{{Id: {Id}, Name: {Name}, ShouldPort: {ShouldPort}}}";
        }
    }
}
