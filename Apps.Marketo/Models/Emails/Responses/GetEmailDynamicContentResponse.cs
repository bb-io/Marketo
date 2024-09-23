using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Emails.Responses
{
    public class GetEmailDynamicContentResponse
    {
        public GetEmailDynamicContentResponse(EmailBaseSegmentDto input)
        {
            Id = input.Id;
            Segment = input.SegmentName;
            Content = input.Content;
            Type = input.Type;
        }

        public string Id { get; set; }

        [Display("Segment")]
        public string Segment { get; set; }

        public string Content { get; set; }

        public string Type { get; set; }

        [Display("Dynamic content Id")]
        public string DynamicContentId { get; set; }
    }
}
