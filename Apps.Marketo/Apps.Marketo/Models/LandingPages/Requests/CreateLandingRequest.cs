using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.LandingPages.Requests
{
    public class CreateLandingRequest
    {
        [Display("Custom head HTML")]
        public string? CustomHeadHTML { get; set; }
        public string? Description { get; set; }

        [Display("Facebook OG tags")]
        public string? FacebookOgTags { get; set; }

        [Display("Folder")]
        [DataSource(typeof(FolderDataHandler))]
        public string FolderId { get; set; }

        [Display("Folder type")]
        [DataSource(typeof(FolderTypeDataHandler))]
        public string Type { get; set; }
        public string? Keywords { get; set; }

        [Display("Mobile enabled")]
        public bool? MobileEnabled { get; set; }
        public string Name { get; set; }

        [Display("Prefill form")]
        public bool? PrefillForm { get; set; }
        public string? Robots { get; set; }

        [DataSource(typeof(LandingPageTemplateDataHandler))]
        public string Template { get; set; }
        public string? Title { get; set; }

        [Display("Url page name")]
        public string? UrlPageName { get; set; }
        public string? Workspace { get; set; }
    }
}
