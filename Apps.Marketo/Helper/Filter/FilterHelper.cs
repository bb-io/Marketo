using Apps.Marketo.Api;
using Apps.Marketo.Helper.Filter.Interfaces;

namespace Apps.Marketo.Helper.Filter;

public static class FilterHelper
{
    public static List<T> ApplyNamePatternFilter<T>(
        this List<T> items, 
        List<string>? namePatterns, 
        bool? excludeMatched) where T : IEntityName
    {
        if (namePatterns == null || namePatterns.Count == 0)
            return items;

        return items.Where(x => FileFolderHelper.IsFilePathMatchingPattern(
            namePatterns,
            x.Name,
            excludeMatched ?? false)
        ).ToList();
    }

    public static async Task<List<T>> ApplyIgnoreInArchiveFilter<T>(
        this List<T> items,
        MarketoClient client,
        bool? ignoreInArchive) where T : IEntityFolder
    {
        if (ignoreInArchive != true)
            return items;

        var nonArchivedItems = new List<T>();
        var folderArchiveCache = new Dictionary<string, bool>();

        foreach (var item in items)
        {
            string folderId = item.Folder.Value.ToString();

            if (!folderArchiveCache.TryGetValue(folderId, out bool isArchived))
            {
                isArchived = await FileFolderHelper.IsAssetInArchievedFolder(client, item.Folder);
                folderArchiveCache[folderId] = isArchived;
            }

            if (!isArchived)
                nonArchivedItems.Add(item);
        }

        return nonArchivedItems;
    }
}
