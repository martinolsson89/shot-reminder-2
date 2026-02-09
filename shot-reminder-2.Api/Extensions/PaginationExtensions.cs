using shot_reminder_2.Contracts.Common;

namespace shot_reminder_2.Api.Extensions;

public static class PaginationExtensions
{
    public static PagedResponse<TDestination> ToPagedResponse<TSource, TDestination>(
        this IReadOnlyList<TSource>? source,
        int pageNumber,
        int pageSize,
        Func<TSource, TDestination> map)
    {
        var items = source ?? Array.Empty<TSource>();
        var totalItemCount = items.Count;
        var totalPageCount = totalItemCount == 0 ? 0 : (int)Math.Ceiling(totalItemCount / (double)pageSize);
        var skip = (pageNumber - 1) * pageSize;

        var pagedItems = items
            .Skip(skip)
            .Take(pageSize)
            .Select(map)
            .ToList();

        return new PagedResponse<TDestination>(
            pagedItems,
            new PaginationMetadata(pageNumber, pageSize, totalItemCount, totalPageCount));
    }
}
