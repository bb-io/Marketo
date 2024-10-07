﻿using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Folder.Requests;

public class GetFolderInfoRequest
{
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; }
    
    [Display("Root folder", Description = "This property is used only to help filter for the Folder property. It is not used in the request.")]
    [DataSource(typeof(FolderDataHandler))]
    public string? RootFolder { get; set; }
}