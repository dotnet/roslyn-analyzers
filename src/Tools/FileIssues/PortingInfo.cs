namespace FileIssues
{
    public class PortingInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ProposedAnalyzer { get; set; }

        public string Notes { get; set; }

        public Disposition Disposition { get; set; }

        public Priority OriginalPriority { get; set; }

        public Priority RevisedPriority { get; set; }

        public string Dependency { get; set; }

        public bool Soon
        {
            get
            {
                return RevisedPriority == Priority.High
                    || (RevisedPriority == Priority.None && OriginalPriority == Priority.High); 
            }
        }
    }
}
