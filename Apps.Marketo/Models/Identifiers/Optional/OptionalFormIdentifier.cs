using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Identifiers.Optional;

public class OptionalFormIdenfitier
{
    [Display("Form ID"), DataSource(typeof(FormDataHandler))]
    public string? FormId { get; set; }
}