using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class TagTypeDto
    {
        public string TagType { get; set; }
        public string ApplicableProgramTypes { get; set; }
        public bool Required { get; set; }
    }
}
