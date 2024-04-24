using Apps.Marketo.DataSourceHandlers.ProgramDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Program.Request
{
    public class GetProgramRequest
    {
        [Display("Program")]
        [DataSource(typeof(ProgramDataHandler))]
        public string ProgramId { get; set; }
    }
}
