---
order: 18
icon: upload
---
# File Upload

### Implementing File Uploads with htmx and ASP.NET Core

The File Upload pattern demonstrates how to handle multi-part form data using htmx while providing real-time progress tracking. This allows for a smoother user experience compared to traditional form submissions, as the page doesn't need to refresh, and the user can see exactly how much of their file has been uploaded.

#### The Frontend: Razor & htmx

In our `FileUpload` demo, we use `hx-encoding='multipart/form-data'` to ensure the browser sends the file correctly. We also include a `<progress>` element that htmx will update during the upload process.

**`_javascript.cshtml`**
```razor
@model IFormFile
<form id='form' hx-encoding='multipart/form-data' hx-post='@Url.Page("Index", "Upload")'>
    @Html.AntiForgeryToken()
    <div class="mb-3">
        <label asp-for="@Model" class="form-label">File Upload</label>
        <input name="UploadedFile" class="form-control" type="file">
    </div>
    <button class="btn btn-primary">Upload</button>
    <progress id="js_progress" value='0' max='100' class="mt-2"></progress>

    @if (Model != null)
    {
        <div class="alert alert-success mt-2">
            File uploaded: @Model.FileName
        </div>
    }
</form>
```

**Key htmx attributes used:**
*   `hx-encoding='multipart/form-data'`: This is required for any form that includes a file input. It tells htmx to use a `FormData` object for the request.
*   `hx-post`: Sends the file data to the `Upload` handler on the server.

#### Progress Tracking with JavaScript

While htmx handles the upload, we use a small snippet of JavaScript to listen for the `htmx:xhr:progress` event and update our progress bar.

**`file-upload.js`**
```javascript
htmx.on('#form', 'htmx:xhr:progress', function(evt) {
    var progress = document.getElementById('js_progress');
    if (progress) {
        progress.setAttribute('value', evt.detail.loaded / evt.detail.total * 100);
    }
});
```

#### The Backend: C# PageModel

On the server, the `IndexModel` receives the file as an `IFormFile`. It performs validation on the file size and extension before processing it.

**`Index.cshtml.cs`**
```csharp
public class IndexModel : PageModel
{
    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    public async Task<IActionResult> OnPostUpload()
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Validate file size and extension
        var extension = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
        if (!IsAllowedExtension(extension))
        {
            return BadRequest("Invalid file type.");
        }

        // Simulate some processing time
        await Task.Delay(1200);

        // Return the partial view to show the success message
        return Partial("_javascript", UploadedFile);
    }
}
```

#### Summary

Integrating file uploads with htmx is straightforward. By setting the correct encoding and handling the `htmx:xhr:progress` event, you can create a modern, responsive file upload interface with minimal custom JavaScript. The server continues to handle the file logic exactly as it would in a traditional ASP.NET Core application, making it easy to implement robust validation and storage logic.