namespace shot_reminder_2.Contracts.Common;

public record PagedResponse<T>(IReadOnlyList<T> Items, PaginationMetadata Pagination);
