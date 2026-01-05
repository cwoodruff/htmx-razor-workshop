---
order: 14
icon: stopwatch
---
# Progress Bar

### Implementing a Progress Bar with htmx and ASP.NET Core

The Progress Bar pattern is essential for long-running tasks. It allows you to initiate a process on the server and provide real-time updates to the user without a full page reload, improving the perceived performance and user experience of your application.

#### 1. Starting the Job

In `Index.cshtml`, we have a simple button that triggers the long-running process. When clicked, it replaces the current container with the initial progress UI.

```html
<div hx-target="this" hx-swap="outerHTML">
    <h3>Start Progress</h3>
    <form>
        @Html.AntiForgeryToken()
        <button class="btn btn-primary" hx-post="@Url.Page("Index", "StartJob")">
            Start Job
        </button>
    </form>
</div>
```

#### 2. Polling for Status

The server returns the `_Progress.cshtml` partial. This partial contains the logic to poll the server for the current job status.

**`_Progress.cshtml`**
```razor
@{
    string Status = ViewData["Status"].ToString();
}

<div hx-trigger="done" hx-get="@Url.Page("Index", "FinalizeJob")" hx-swap="outerHTML" hx-target="this">
    <h3 id="pblabel">@Status</h3>

    <div hx-get="@Url.Page("Index", "JobStatus")"
         hx-trigger="@(Status == "Running" ? "every 600ms" : "none")"
         hx-target="this"
         hx-swap="innerHTML">
        <partial name="_ProgressBar"/>
    </div>
</div>
```
**Key htmx attributes used:**
*   `hx-get`: Fetches the current progress from the `JobStatus` handler.
*   `hx-trigger="every 600ms"`: Tells htmx to poll the server every 600 milliseconds while the status is "Running".
*   `hx-trigger="done"`: Listens for a custom `done` event from the server to finalize the UI.

#### 3. The Progress Bar Fragment

The actual progress bar is rendered in a separate partial view, making it easy to update during polling.

**`_ProgressBar.cshtml`**
```razor
@{
    string PercentDone = ViewData["PercentDone"].ToString();
}
<div class="progress" role="progressbar" aria-valuenow="@PercentDone">
    <div class="progress-bar" style="width:@PercentDone%"></div>
</div>
```

#### 4. The Backend: C# PageModel

The `IndexModel` manages the state of the job and handles the polling requests.

**`Index.cshtml.cs`**
```csharp
public class IndexModel : PageModel
{
    public static int percent { get; set; } = 0;
    [ViewData] public string PercentDone { get; set; }
    [ViewData] public string Status { get; set; }

    public PartialViewResult OnPostStartJob()
    {
        percent = 2;
        Status = "Running";
        PercentDone = percent.ToString();
        return Partial("_Progress");
    }

    public PartialViewResult OnGetJobStatus()
    {
        // Simulate progress increment
        percent = IncrementProgress(percent);

        if (percent >= 100)
        {
            // Signal htmx that the job is done via a response header
            HttpContext.Response.Headers["HX-Trigger"] = "done";
        }

        PercentDone = percent.ToString();
        return Partial("_ProgressBar");
    }

    public PartialViewResult OnGetFinalizeJob()
    {
        percent = 0;
        Status = "Complete";
        PercentDone = "100";
        return Partial("_Progress");
    }
}
```

#### Why this works well

1.  **Server-Driven State**: The server maintains the truth about the job's progress. The client simply polls and renders the current state.
2.  **Clean Completion**: By using the `HX-Trigger` header, the server can tell htmx to stop polling and switch to a "Completed" view without the client needing complex logic.
3.  **Low Resource Usage**: Polling occurs only while the job is active, and each request returns a tiny fragment of HTML, minimizing overhead.
4.  **Accessibility**: By updating the `aria-valuenow` and progress bar width dynamically, the UI remains accessible to screen readers.