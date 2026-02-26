using Apps.Marketo.Api;
using Apps.Marketo.Helper.Interfaces;

namespace Apps.Marketo.Helper.Filter;

public static class FilterHelper
{
    public static IEnumerable<T> ApplyNamePatternFilter<T>(
        this IEnumerable<T> items, 
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

    public static async Task<IEnumerable<T>> ApplyIgnoreInArchiveFilter<T>(
        this IEnumerable<T> items,
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

    public static IEnumerable<T> ApplyUpdatedAtFilter<T>(
        this IEnumerable<T> items, 
        DateTime? updatedAfter, 
        DateTime? updatedBefore) where T : IEntityUpdatedAt
    {
        if (updatedAfter == null && updatedBefore == null) 
            return items;

        if (updatedAfter != null)
            items = items.Where(x => x.UpdatedAt >= updatedAfter.Value);

        if (updatedBefore != null)
            items = items.Where(x => x.UpdatedAt <= updatedBefore.Value);

        return items;
    }

    public static IEnumerable<T> ApplyCreatedAtFilter<T>(
        this IEnumerable<T> items,
        DateTime? createdAfter,
        DateTime? createdBefore) where T : IEntityCreatedAt
    {
        if (createdAfter == null && createdBefore == null)
            return items;

        if (createdAfter != null)
            items = items.Where(x => x.CreatedAt >= createdAfter.Value);

        if (createdBefore != null)
            items = items.Where(x => x.CreatedAt <= createdBefore.Value);

        return items;
    }

    public static IEnumerable<T> ApplyFolderIdFilter<T>(
        this IEnumerable<T> items,
        string? folderId) where T : IEntityFolder
    {
        if (string.IsNullOrEmpty(folderId))
            return items;

        return items.Where(x => x.Folder.Value.ToString() == folderId.Split("_").First()).ToList();
    }
}
