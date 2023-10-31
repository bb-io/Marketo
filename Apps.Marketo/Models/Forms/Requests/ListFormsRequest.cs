using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Forms.Requests;

public class ListFormsRequest
{
    public int? Hours { get; set; }
    
    [DataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }
}