---
order: 6
icon: comment
---
# Workshop Introduction — Modern UX with htmx + ASP.NET Core Razor Pages

## Welcome

Welcome to **Modern UX Without JavaScript Madness: htmx + ASP.NET Core Razor Pages**.

This workshop is designed for ASP.NET Core developers who want to build fast, modern, interactive web applications **without adopting a full SPA framework** and without moving UI state management into the browser. Instead, we’ll use a server-first approach where the server continues to own the business rules, validation, and UI rendering—and the browser simply requests and swaps **HTML fragments**.

You already know Razor Pages, C#, and how to ship server-rendered applications. The goal today is to add a small, pragmatic tool—**htmx**—that lets you incrementally modernize UX with minimal new surface area and a maintainable architecture.

## What is htmx?

**htmx** is a small library that extends HTML with attributes such as:

- `hx-get` and `hx-post` to make HTTP requests from buttons, links, and forms
- `hx-target` and `hx-swap` to control where returned HTML is inserted
- `hx-trigger` to control when requests fire (click, change, keyup, timers, etc.)
- `hx-push-url` to keep browser history in sync for back/forward navigation
- `hx-swap-oob` for out-of-band updates to multiple regions from a single response

The key difference from SPA frameworks is that htmx does not ask you to build a client-side application. It asks you to **keep building HTML**—and then makes it easy to update only the parts of the page that change.

## What is hypermedia, and why does it matter?

**Hypermedia** is the idea that the user interface itself (HTML) carries the information needed to drive the application forward—through **links, forms, and server responses**.

In practical terms:

- The server renders UI states as HTML.
- The client navigates and interacts using HTTP.
- The server responds with either a full page or a fragment.
- The UI evolves by swapping server-rendered HTML into known “fragment boundaries.”

This approach keeps the system simple because it aligns your UI with the web’s native strengths: **HTTP semantics, URLs, caching, progressive enhancement, and straightforward debugging** (you can inspect requests and HTML responses directly).

## The mental model for today

Every lab in this workshop follows the same repeatable loop:

1. **Identify a fragment boundary** (a region of the page that should update independently).
2. Give it a **stable wrapper element** with a predictable `id`.
3. Use htmx to make an **HTTP request** (`hx-get` / `hx-post`) when something happens.
4. Return **server-rendered HTML** (usually a partial) that matches the boundary.
5. **Swap** the response into the correct target (`hx-target` + `hx-swap`).

If you can do those five things, you can build most “modern UX” patterns without a SPA.

## Workshop outcomes

By the end of the workshop, you will be able to:

- Build a “fragment-first” Razor Pages UI composed of swappable partials
- Convert standard form workflows to partial updates with `hx-post` and `hx-target`
- Implement real-time validation with server-side rules and `hx-trigger`
- Build core UX patterns such as:
  - details views in a panel/modal
  - confirm + delete flows
  - history-aware filtering and pagination (`hx-push-url`)
- Create dynamic forms (add/remove rows, dependent dropdowns) with fragment endpoints
- Implement long-running UX with polling and status fragments
- Apply conventions that keep an htmx Razor Pages codebase production-livable

## How we will work today

This is a hands-on workshop. You will:

- Start from a working baseline project
- Implement each feature step-by-step
- Use DevTools frequently to inspect:
  - network requests
  - handler routes (`?handler=...`)
  - HTML fragments returned from the server
  - DOM swaps and target selection

If something breaks, that’s expected—debugging is part of the learning. We will use a repeatable troubleshooting checklist to unblock quickly.

## Conventions we will follow

To keep the project maintainable:

- Every fragment has a **single wrapper element** and a stable `id`
- `outerHTML` swaps replace the entire wrapper node
- Handlers follow consistent naming:
  - `OnGetList`, `OnPostCreate`, `OnPostDelete`, etc.
- Response rules:
  - full navigation returns pages
  - htmx requests return partial fragments
  - errors return a dedicated error/messages fragment

These conventions turn the labs into a blueprint you can reuse in real applications.

## Prerequisites

You should already be comfortable with:

- C# and ASP.NET Core Razor Pages
- basic form handling and model binding
- running and debugging a .NET app locally

You will need:

- a laptop with the .NET SDK and your preferred IDE installed
- internet access (for library docs and reference)
- browser DevTools (Chrome/Edge/Firefox)

## Before we begin

1. Open the starter solution and run the app.
2. Confirm you can set breakpoints and hit a Razor Pages handler.
3. Open DevTools → **Network** and **Elements**.
4. Confirm htmx is loaded on the page (we will verify this early).

Once your baseline is running, we’ll begin with **Fragment First**: building the page as a set of stable, swappable UI regions.

