using Apps.Marketo.DataSourceHandlers.ProgramDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Identifiers;

public class ProgramIdentifier
{
    [Display("Program ID"), DataSource(typeof(ProgramDataHandler))]
    public string ProgramId { get; set; }
}
