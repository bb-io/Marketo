using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class BaseResponseDto<T>
    {
        public List<Error> Errors { get; set; }
        public string RequestId { get; set; }
        public List<T> Result { get; set; }
        public bool Success { get; set; }
        public List<string> Warnings { get; set; }
    }
}
