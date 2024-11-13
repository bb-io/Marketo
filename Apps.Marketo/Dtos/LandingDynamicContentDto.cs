namespace Apps.Marketo.Dtos
{
    public class LandingDynamicContentDto<T>
    {
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int Id { get; set; }
        public int Segmentation { get; set; }
        public List<T> Segments { get; set; }
    }
}
