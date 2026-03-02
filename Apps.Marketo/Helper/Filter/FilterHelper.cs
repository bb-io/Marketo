using Apps.Marketo.Api;
using Apps.Marketo.Models.Entities;
using Apps.Marketo.Helper.FileFolder;

namespace Apps.Marketo.Helper.Filter;

public static class FilterHelper
{
    public static IEnumerable<T> ApplyNamePatternFilter<T>(
        this IEnumerable<T> items, 
        List<string>? namePatterns, 
        bool? excludeMatched,
        Func<T, string> nameSelector)
    {
        if (namePatterns == null || namePatterns.Count == 0)
            return items;

        return items.Where(x => FileFolderHelper.IsFilePathMatchingPattern(
            namePatterns,
            nameSelector(x),
            excludeMatched ?? false)
        );
    }

    public static async Task<IEnumerable<T>> ApplyIgnoreInArchiveFilter<T>(
        this IEnumerable<T> items,
        MarketoClient client,
        bool? ignoreInArchive,
        Func<T, AssetFolder> folderSelector)
    {
        if (ignoreInArchive != true)
            return items;

        var nonArchivedItems = new List<T>();
        var folderArchiveCache = new Dictionary<string, bool>();

        foreach (var item in items)
        {
            var folder = folderSelector(item);
            string folderId = folder.Value.ToString();

            if (!folderArchiveCache.TryGetValue(folderId, out bool isArchived))
            {
                isArchived = await FileFolderHelper.IsAssetInArchievedFolder(client, folder);
                folderArchiveCache[folderId] = isArchived;
            }

            if (!isArchived)
                nonArchivedItems.Add(item);
        }

        return nonArchivedItems;
    }

    public static IEnumerable<T> ApplyDateAfterFilter<T>(
        this IEnumerable<T> items, 
        DateTime? after,
        Func<T, DateTime?> dateSelector)
    {
        if (after == null) 
            return items;

        return items.Where(x => dateSelector(x) >= after.Value);
    }

    public static IEnumerable<T> ApplyDateBeforeFilter<T>(
        this IEnumerable<T> items,
        DateTime? before,
        Func<T, DateTime?> dateSelector)
    {
        if (before == null)
            return items;

        return items.Where(x => dateSelector(x) <= before.Value);
    }

    public static IEnumerable<T> ApplyFolderIdFilter<T>(
        this IEnumerable<T> items,
        string? folderId,
        Func<T, AssetFolder> folderSelector)
    {
        if (string.IsNullOrEmpty(folderId))
            return items;

        return items.Where(x => folderSelector(x).Value.ToString() == folderId.Split("_").First());
    }
}
