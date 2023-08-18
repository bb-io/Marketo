﻿using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.DataSourceHandlers
{
    public class FolderTypeDataHandler : EnumDataHandler 
    {
        protected override Dictionary<string, string> EnumValues => new()
        {
            { "Folder", "Folder" },
            { "Program", "Program" },
        };
    }
}
