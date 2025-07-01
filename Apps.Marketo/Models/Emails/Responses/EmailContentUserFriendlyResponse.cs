using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

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

        [Display("Email content items")]
        public List<EmailContentUserFriendlyDto> EmailContentItems { get; set; }
    }

    public class EmailContentUserFriendlyDto
    {
        [Display("Content type")]
        public string ContentType { get; set; }

        [Display("HTML ID")]
        public string HtmlId { get; set; }
        public int Index { get; set; }

        [Display("Is locked")]
        public bool IsLocked { get; set; }

        [Display("Parent HTML ID")]
        public string ParentHtmlId { get; set; }
        public string Value { get; set; }
    }
}
