using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Emails.Responses
{
    public class GetEmailDynamicContentResponse
    {
        public GetEmailDynamicContentResponse(EmailSegmentDto input)
        {
            Id = input.Id;
            SegmentId = input.SegmentId;
            SegmentName = input.SegmentName;
            Content = input.Content;
            Type = input.Type;
        }

        public string Id { get; set; }

        [Display("Segment ID")]
        public int SegmentId { get; set; }

        [Display("Segment name")]
        public string SegmentName { get; set; }

        public string Content { get; set; }

        public string Type { get; set; }

        [Display("Dynamic content Id")]
        public string DynamicContentId { get; set; }
    }
}
