using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos;
public class DebugInputs
{
    public string Url { get; set; }
    public string MethodType { get; set; }
    public List<string> ParamNames { get; set; }
    public List<string> ParamValues { get; set; }
}

