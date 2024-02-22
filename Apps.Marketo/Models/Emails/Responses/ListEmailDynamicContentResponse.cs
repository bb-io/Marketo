using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Emails.Responses
{
    public class ListEmailDynamicContentResponse
    {
        [Display("Email dynamic content list")]
        public List<EmailSegmentDto> EmailDynamicContentList { get; set; }
    }
}
