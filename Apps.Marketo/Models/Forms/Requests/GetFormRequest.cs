﻿using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Forms.Requests;

public class GetFormRequest
{
    [Display("Form")]
    [DataSource(typeof(FormDataHandler))]
    public string FormId { get; set; }
}