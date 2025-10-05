using htmx_examples_blazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();
builder.Services.AddControllers();
builder.Services.AddAntiforgery();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<htmx_examples_blazor.Pages.BulkUpdate.IContactService, htmx_examples_blazor.Pages.BulkUpdate.ContactService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>();

app.Run();
