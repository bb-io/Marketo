﻿namespace Apps.Marketo.Dtos
{
    public class SnippetDynamicContentDto
    {
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int Id { get; set; }
        public int Segmentation { get; set; }
        public List<SnippetSegmentDto> Content { get; set; }
    }
}
