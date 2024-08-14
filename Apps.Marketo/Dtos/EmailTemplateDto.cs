namespace Apps.Marketo.Dtos
{
    public class EmailTemplateDto
    {
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public Folder Folder { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Url { get; set; }
        public int Version { get; set; }
        public string Workspace { get; set; }
    }
}
