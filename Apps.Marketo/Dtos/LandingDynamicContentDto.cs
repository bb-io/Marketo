namespace Apps.Marketo.Dtos
{
    public class LandingDynamicContentDto
    {
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int Id { get; set; }
        public int Segmentation { get; set; }
        public List<EmailBaseSegmentDto> Segments { get; set; }
    }
}
