using Apps.Marketo.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Emails.Responses
{
    public class ListEmailsResponse
    {
        public List<EmailDto> Emails { get; set; }
    }
}
