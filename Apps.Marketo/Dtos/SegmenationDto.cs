namespace Apps.Marketo.Dtos
{
    public class SegmenationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string Url { get; set; }
        public SegmentationFolder Folder { get; set; }
        public string Status { get; set; }
        public string Workspace { get; set; }
    }

    public class SegmentationFolder
    {
        public string Type { get; set; }
        public int Value { get; set; }
    }
}
