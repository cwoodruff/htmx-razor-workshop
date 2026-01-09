namespace RazorPagesHtmxWorkshop.Models;

/// <summary>
/// View model for the task list with filtering and pagination support.
/// </summary>
public class TaskListVm
{
    public required IReadOnlyList<TaskItem> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public string? Query { get; init; }

    /// <summary>
    /// Total number of pages based on items and page size.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    
    /// <summary>
    /// Whether there's a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
    
    /// <summary>
    /// Whether there's a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
