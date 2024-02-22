using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class SegmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int SegmentationId { get; set; }
    }
}
