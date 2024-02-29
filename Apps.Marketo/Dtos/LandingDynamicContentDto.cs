using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class LandingDynamicContentDto
    {
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int Id { get; set; }
        public int Segmentation { get; set; }
        public List<EmailSegmentDto> Segments { get; set; }
    }
}
