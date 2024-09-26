using Apps.Marketo.DataSourceHandlers.ProgramDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Program.Request
{
    public class GetProgramRequest
    {
        [Display("Program")]
        [DataSource(typeof(ProgramDataHandler))]
        public string ProgramId { get; set; }
    }
}
