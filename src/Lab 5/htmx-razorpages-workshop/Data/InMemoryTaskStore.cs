using RazorPagesHtmxWorkshop.Models;

namespace RazorPagesHtmxWorkshop.Data;

public static class InMemoryTaskStore
{
    private static int _nextId = 1;
    private static readonly List<TaskItem> _tasks = new();

    public static IReadOnlyList<TaskItem> All() => 
        _tasks.OrderByDescending(t => t.CreatedUtc).ToList();

    /// <summary>
    /// Finds a task by ID.
    /// </summary>
    public static TaskItem? Find(int id) => 
        _tasks.FirstOrDefault(t => t.Id == id);

    public static TaskItem Add(string title)
    {
        var item = new TaskItem
        {
            Id = _nextId++,
            Title = title.Trim(),
            IsDone = false,
            CreatedUtc = DateTime.UtcNow
        };

        _tasks.Add(item);
        return item;
    }

    /// <summary>
    /// Deletes a task by ID.
    /// Returns true if found and deleted, false if not found.
    /// </summary>
    public static bool Delete(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return false;
        
        _tasks.Remove(task);
        return true;
    }

    /// <summary>
    /// Useful for workshops / resetting between labs
    /// </summary>
    public static void Reset()
    {
        _tasks.Clear();
        _nextId = 1;
    }
}
