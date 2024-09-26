﻿namespace Apps.Marketo.Dtos
{
    public class TagValueDto
    {
        public string TagType { get; set; }
        public string ApplicableProgramTypes { get; set; }
        public bool Required { get; set; }
        public string AllowableValues { get; set; }
    }
}
