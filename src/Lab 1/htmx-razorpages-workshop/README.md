# htmx ASP.NET Core Razor Pages Workshop (Minimal App)

This is a minimal Razor Pages app intended for hands-on workshop labs.

## Requirements
- .NET SDK **10.0** (or update the project TargetFramework)

## Run
```bash
dotnet restore
dotnet run
```

Then open the displayed URL.

## Pages
- `/` Home
- `/Labs` Lab outline
- `/Tasks` Demo page designed with fragment boundaries:
  - `#messages`
  - `#task-form`
  - `#task-list`

## Notes
- htmx is loaded via CDN in `_Layout.cshtml`.
- This baseline uses server-side validation and full-page reloads.
  Later labs convert specific interactions to htmx swaps.
