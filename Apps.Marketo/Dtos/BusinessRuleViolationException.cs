using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class BusinessRuleViolationException : ArgumentException
    {
        public int ErrorCode { get; set; }
        public BusinessRuleViolationException(int code, string message) : base(message)
        {
            ErrorCode = code;
        }
    }
}
