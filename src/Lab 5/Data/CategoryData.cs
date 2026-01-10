namespace RazorPagesHtmxWorkshop.Data;

/// <summary>
/// Sample data for demonstrating dependent dropdowns.
/// Category → Subcategory cascading selection.
/// </summary>
public static class CategoryData
{
    private static readonly Dictionary<string, List<string>> _subcategories = new()
    {
        ["Work"] = new() { "Meeting", "Report", "Email", "Review" },
        ["Personal"] = new() { "Shopping", "Exercise", "Reading", "Travel" },
        ["Home"] = new() { "Cleaning", "Repairs", "Gardening", "Cooking" },
        ["Learning"] = new() { "Course", "Tutorial", "Practice", "Research" }
    };

    public static IReadOnlyList<string> GetCategories() => 
        _subcategories.Keys.ToList();

    public static IReadOnlyList<string> GetSubcategories(string? category) =>
        string.IsNullOrWhiteSpace(category) || !_subcategories.ContainsKey(category)
            ? Array.Empty<string>()
            : _subcategories[category];
}
