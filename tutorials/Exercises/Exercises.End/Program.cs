using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// add dependencies to services collection
builder.Services.AddHttpClient();
builder.Services.AddRazorPages(o => {
    // this is to make demos easier
    // don't do this in production
    o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
}).AddRazorRuntimeCompilation();

// define asp.net request pipeline
var app = builder.Build();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapRazorPages();

app.Run();