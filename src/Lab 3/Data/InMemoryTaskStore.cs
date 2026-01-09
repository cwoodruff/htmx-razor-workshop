using System;
using System.Collections.Generic;
using System.Linq;
using RazorPagesHtmxWorkshop.Models;

namespace RazorPagesHtmxWorkshop.Data;

public static class InMemoryTaskStore
{
    private static int _nextId = 1;
    private static readonly List<TaskItem> _tasks = new();

    public static IReadOnlyList<TaskItem> All() =>
        _tasks.OrderByDescending(t => t.CreatedUtc).ToList();

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

    // Useful for workshops / resetting between labs
    public static void Reset()
    {
        _tasks.Clear();
        _nextId = 1;
    }
}
