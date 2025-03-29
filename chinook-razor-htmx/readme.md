# Chinook Sample with Htmx

Getting started with this sample, you'll need to run the following .NET commands.

Restore the NuGet packages

```bash
dotnet restore
```
Install the .NET EF Core tools

```bash
dotnet tool restore
```

Run the database migrations to create the SQLite database.

```bash
dotnet ef database update --project ChinookHTMX
```

To run the app, use the following command:

```bash
dotnet run
```

Then navigate to `http://localhost:5288`