namespace Apps.Marketo.Dtos
{
    public class ProgramDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Channel { get; set; }
        public ProgramFolder Folder { get; set; }
        public string Status { get; set; }
        public string Workspace { get; set; }
    }

    public class ProgramFolder
    {
        public string Type { get; set; }
        public int Value { get; set; }
        public string FolderName { get; set; }
    }
}
