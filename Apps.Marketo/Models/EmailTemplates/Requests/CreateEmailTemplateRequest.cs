using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.EmailTemplates.Requests
{
    public class CreateEmailTemplateRequest
    {
        public string Name { get; set; }

        [Display("Folder")]
        [DataSource(typeof(EmailTemplateFolderDataHandler))]
        public string FolderId { get; set; }
        public string? Description { get; set; }
        public string Content { get; set; }
    }
}
