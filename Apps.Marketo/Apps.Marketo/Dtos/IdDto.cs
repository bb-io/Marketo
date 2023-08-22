using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class IdDto
    {
        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }
    }
}
