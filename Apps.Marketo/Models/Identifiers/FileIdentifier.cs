using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Identifiers;

public class FileIdentifier
{
    [Display("File ID"), DataSource(typeof(FileDataHandler))]
    public string FileId { get; set; }
}