using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class SegmentDto
    {
        public string Id { get; set; }

        public int SegmentId { get; set; }

        public string SegmentName { get; set; }

        public string Content { get; set; }

        public string Type { get; set; }
    }
}
