using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Forms.Requests;

public class DownloadFormRequest
{
    [Display("Ignore form fields"), DataSource(typeof(FormFieldDataHandler))]
    public List<string>? IgnoreFields { get; set; }

    [Display("Ignore visibility rules content")]
    public bool? IgnoreVisibilityRules { get; set; }
}
