﻿using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Forms.Requests
{
    public class IgnoreFieldsRequest
    {
        [Display("Ignore form fields")]
        [DataSource(typeof(FormFieldDataHandler))]
        public List<string>? IgnoreFields { get; set; }
    }
}
