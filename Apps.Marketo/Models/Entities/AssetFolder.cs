using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Entities;

public class AssetFolder
{
    public string Type { get; set; }
    
    [Display("Id")]
    public string Value { get; set; }

    [Display("Folder name")]
    public string FolderName { get; set; }
}