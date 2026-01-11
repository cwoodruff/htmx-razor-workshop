# Chinook Music Store (Work in Progress)

## Introduction

The **Chinook Music Store** is a real-world demonstration application that showcases how htmx can transform traditional ASP.NET Core Razor Pages applications into modern, interactive web experiences—without the complexity of a full-blown Single Page Application (SPA).

### About the Chinook Database

The Chinook database is a widely-used sample database representing a fictitious online music store that sells digital music. It includes realistic entities like artists, albums, tracks, customers, invoices, and playlists, making it an ideal foundation for demonstrating practical web application patterns.

### Why This Application Exists

Most developers today assume that building a modern, interactive business application requires:
- A complex SPA framework (React, Angular, Vue) managing client-side state
- A separate backend API layer with dozens of endpoints
- Complicated authentication using JWT tokens and OAuth flows
- Build pipelines, bundlers, and intricate deployment processes

**The Chinook Music Store challenges this assumption.** It demonstrates that you can build rich, interactive user experiences using a simpler, server-first approach with htmx and ASP.NET Core Razor Pages. This approach offers:
- **Server-owned business logic and validation** - Your domain rules live where they belong
- **Reduced complexity** - No client-side state management, no API versioning headaches, no JWT token juggling
- **Better developer experience** - Write C# for both UI rendering and business logic
- **Progressive enhancement** - The application works without JavaScript and enhances with htmx
- **Simpler deployment** - One application, one deployment target, fewer moving parts

### Explore the Application

- **Live Demo**: [https://chinook-razor-htmx-hfg2ehd2dfgeg0bw.centralus-01.azurewebsites.net/](https://chinook-razor-htmx-hfg2ehd2dfgeg0bw.centralus-01.azurewebsites.net/)
- **Source Code**: [https://github.com/cwoodruff/chinook-razor-htmx](https://github.com/cwoodruff/chinook-razor-htmx)

The Chinook Music Store serves as a companion to this workshop, illustrating how the patterns you've learned—fragment-first design, partial updates, real-time validation, and hypermedia-driven interactions—scale to a complete business application with real-world requirements.

### What You'll See

The application demonstrates practical implementations of:
- Browse and search functionality across artists, albums, and tracks
- Shopping cart interactions without page reloads
- Customer management and order processing
- Playlist creation and management
- Real-time filtering and pagination
- Form validation and error handling
- All built using the server-first, hypermedia-driven patterns from this workshop

This isn't a toy example or a proof-of-concept. It's a fully-functional music store that proves you can deliver modern web experiences without the architectural complexity that has become the default in modern web development.
