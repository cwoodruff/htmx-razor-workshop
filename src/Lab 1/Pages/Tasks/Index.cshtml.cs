using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesHtmxWorkshop.Data;
using RazorPagesHtmxWorkshop.Models;

namespace RazorPagesHtmxWorkshop.Pages.Tasks;

public class IndexModel : PageModel
{
    public IReadOnlyList<TaskItem> Tasks { get; private set; } = Array.Empty<TaskItem>();

    [BindProperty]
    public NewTaskInput Input { get; set; } = new();

    [TempData]
    public string? FlashMessage { get; set; }

    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

    public IActionResult OnPostCreate()
    {
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Title is required.");
        }

        if (!ModelState.IsValid)
        {
            Tasks = InMemoryTaskStore.All();
            FlashMessage = "Please correct the errors and try again.";
            return Page();
        }

        InMemoryTaskStore.Add(Input.Title);
        FlashMessage = "Task added.";
        return RedirectToPage();
    }

    public IActionResult OnPostReset()
    {
        InMemoryTaskStore.Reset();
        FlashMessage = "Tasks reset.";
        return RedirectToPage();
    }

    public class NewTaskInput
    {
        public string Title { get; set; } = "";
    }
}
