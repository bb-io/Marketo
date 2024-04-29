using Apps.Marketo.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Emails.Responses
{
    public class EmailContentUserFriendlyResponse
    {
        public EmailContentUserFriendlyResponse(List<EmailContentDto> contentItems)
        {
            EmailContentItems = contentItems.Select(x => new EmailContentUserFriendlyDto()
            {
                ContentType = x.ContentType,
                HtmlId = x.HtmlId,
                Index = x.Index,
                IsLocked = x.IsLocked,
                ParentHtmlId = x.ParentHtmlId,
                Value = JsonConvert.SerializeObject(x.Value)
            }).ToList();
        }

        public List<EmailContentUserFriendlyDto> EmailContentItems { get; set; }
    }

    public class EmailContentUserFriendlyDto
    {
        public string ContentType { get; set; }
        public string HtmlId { get; set; }
        public int Index { get; set; }
        public bool IsLocked { get; set; }
        public string ParentHtmlId { get; set; }
        public string Value { get; set; }
    }
}
