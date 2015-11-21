namespace FileIssues
{
    public class PortingInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }

        public Disposition Disposition { get; set; }

        public bool NeedsReview { get; set; }

        public override string ToString()
        {
            return $"{{Id: {Id}, Name: {Name}, ShouldPort: {Disposition}}}";
        }
    }
}
