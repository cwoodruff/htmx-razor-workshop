---
order: 1
icon: code-square
---

# Foundations: Hypermedia + htmx Mental Model

> **Goal:** Establish the "server-driven UI" mindset and give attendees just enough htmx vocabulary to be productive in labs.

---

## 1. What "Hypermedia" Means in Web Apps

### The Original Web Architecture

The web was born as a hypermedia system. Tim Berners-Lee didn't build a "JSON API transport layer"—he built a document network where the server sends self-describing, interactive content.

**Hypermedia** = data + controls (links and forms) bundled together in a single response.

```html
<!-- This IS hypermedia: data AND controls together -->
<article>
  <h1>Order #1234</h1>
  <p>Status: Pending</p>

  <!-- The server tells the client what actions are possible -->
  <a href="/orders/1234/cancel">Cancel Order</a>
  <form action="/orders/1234/ship" method="post">
    <button>Ship Now</button>
  </form>
</article>
```

### HTML as the Contract

In a hypermedia architecture, **HTML is the API contract**. The server doesn't return abstract data that the client must interpret—it returns ready-to-render UI with embedded affordances.

| JSON API Approach                                           | Hypermedia Approach                           |
|-------------------------------------------------------------|-----------------------------------------------|
| Server returns `{ "status": "pending", "canCancel": true }` | Server returns `<a href="/cancel">Cancel</a>` |
| Client must know business rules                             | Server encodes business rules in the response |
| Client builds UI from data                                  | Server sends ready UI                         |
| Tight coupling via shared schema                            | Loose coupling via HTML semantics             |

### State Transitions via Links and Forms

In REST's original formulation (Fielding's dissertation), **hypermedia controls drive application state**. This is HATEOAS—Hypermedia As The Engine Of Application State.

Think of it like a choose-your-own-adventure book:
- You're on page 47 (current state)
- The page shows you options: "Turn to page 62" or "Turn to page 78"
- You don't need to know the whole book's structure—just follow the links

```
┌─────────────────┐     GET /cart      ┌─────────────────┐
│   Product Page  │ ─────────────────► │   Cart Page     │
│                 │                    │                 │
│  [Add to Cart]  │                    │  [Checkout]     │
└─────────────────┘                    │  [Continue]     │
                                       └─────────────────┘
                                              │
                                    POST /checkout
                                              │
                                              ▼
                                       ┌─────────────────┐
                                       │ Checkout Page   │
                                       │                 │
                                       │ [Place Order]   │
                                       │ [Back to Cart]  │
                                       └─────────────────┘
```

**Key insight:** The server controls the workflow. The client just follows links.

### The SPA Deviation

Single-Page Applications (SPAs) broke from this model:

```
Traditional Web (Hypermedia)          SPA Architecture
────────────────────────────          ─────────────────
Browser ◄──── HTML ──── Server        Browser ◄──── JSON ──── Server
                                            │
                                      [Fat JS Client]
                                            │
                                      Interprets data,
                                      builds UI,
                                      manages state
```

SPAs moved the "application" to the browser. The server became a dumb data pipe. This created:
- Duplicated business logic (server validates, client validates)
- Complex client-side state management
- Hundreds of kilobytes of JavaScript
- SEO and accessibility challenges

---

## 2. Why htmx Exists

### The Problem htmx Solves

Modern web development often looks like this:

```javascript
// 1. Fetch JSON data
const response = await fetch('/api/users/123');
const user = await response.json();

// 2. Transform data to UI
const html = `
  <div class="user-card">
    <h2>${escapeHtml(user.name)}</h2>
    <p>${escapeHtml(user.email)}</p>
    ${user.isAdmin ? '<span class="badge">Admin</span>' : ''}
  </div>
`;

// 3. Find the right place in the DOM
const container = document.getElementById('user-container');

// 4. Swap it in
container.innerHTML = html;

// 5. Re-attach event listeners (the ones you just destroyed)
container.querySelector('.edit-btn')?.addEventListener('click', handleEdit);
```

**htmx asks:** What if the browser could do this natively for any element, not just `<a>` and `<form>`?

### HTML Over the Wire

htmx extends HTML so any element can make HTTP requests and swap the response into the DOM:

```html
<!-- Before: Requires JavaScript -->
<button onclick="loadUsers()">Load Users</button>

<!-- After: Pure HTML + htmx -->
<button hx-get="/users" hx-target="#user-list">
  Load Users
</button>
```

The server returns HTML fragments, not JSON:

```html
<!-- Server response (a partial, not a full page) -->
<ul id="user-list">
  <li>Alice</li>
  <li>Bob</li>
</ul>
```

### Progressive Enhancement

htmx builds on progressive enhancement—the idea that your app should work without JavaScript, then get better with it.

```html
<!-- Works without JavaScript (full page reload) -->
<form action="/search" method="get">
  <input name="q" type="text">
  <button>Search</button>
</form>

<!-- Enhanced with htmx (inline update) -->
<form action="/search" method="get"
      hx-get="/search"
      hx-target="#results"
      hx-trigger="submit, input changed delay:300ms">
  <input name="q" type="text">
  <button>Search</button>
</form>
```

Both versions work. The htmx version is just smoother.

### Less JavaScript Surface Area

| Metric            | Typical SPA           | htmx Approach            |
|-------------------|-----------------------|--------------------------|
| JS bundle size    | 200-500+ KB           | ~14 KB (htmx)            |
| Client-side state | Redux/Zustand/etc.    | None (server owns state) |
| Build tooling     | Webpack, Vite, etc.   | Optional                 |
| Learning curve    | Framework + ecosystem | HTML attributes          |

**htmx's philosophy:** The server already knows how to build HTML. Let it.

---

## 3. Core htmx Concepts

### 3.1 Requests: `hx-get` / `hx-post` / `hx-put` / `hx-patch` / `hx-delete`

These attributes issue AJAX requests when triggered:

```html
<!-- GET request -->
<button hx-get="/api/status">Check Status</button>

<!-- POST request -->
<button hx-post="/api/orders" hx-vals='{"item": "widget"}'>
  Place Order
</button>

<!-- DELETE request -->
<button hx-delete="/api/orders/123">Cancel Order</button>
```

**Default behavior:**
- Triggered by: `click` (for most elements), `submit` (for forms), `change` (for inputs)
- Response swapped into: the element that made the request
- Swap strategy: `innerHTML`

### 3.2 Targeting: `hx-target`

By default, htmx swaps the response into the element that triggered the request. Use `hx-target` to swap somewhere else:

```html
<!-- Swap into a different element -->
<button hx-get="/users" hx-target="#user-list">
  Load Users
</button>
<div id="user-list"><!-- Users appear here --></div>

<!-- Target selectors -->
<button hx-target="#specific-id">By ID</button>
<button hx-target=".some-class">By class (first match)</button>
<button hx-target="closest div">Closest ancestor</button>
<button hx-target="find .child">First matching descendant</button>
<button hx-target="next .sibling">Next sibling matching</button>
<button hx-target="previous .sibling">Previous sibling matching</button>
<button hx-target="this">The triggering element itself</button>
```

### 3.3 Swapping: `hx-swap`

Controls how the response content replaces target content:

```html
<!-- innerHTML (default): Replace target's children -->
<div hx-get="/content" hx-swap="innerHTML">
  <!-- new content goes INSIDE here -->
</div>

<!-- outerHTML: Replace the entire target element -->
<div hx-get="/content" hx-swap="outerHTML">
  <!-- this entire div gets replaced -->
</div>

<!-- beforebegin: Insert before the target -->
<ul>
  <li hx-get="/new-item" hx-swap="beforebegin">
    <!-- new content appears ABOVE this li -->
  </li>
</ul>

<!-- afterend: Insert after the target -->
<li hx-get="/new-item" hx-swap="afterend">
  <!-- new content appears BELOW this li -->
</li>

<!-- beforeend: Append inside target (great for infinite scroll) -->
<ul hx-get="/more-items" hx-swap="beforeend">
  <li>Item 1</li>
  <li>Item 2</li>
  <!-- new items append here -->
</ul>

<!-- afterbegin: Prepend inside target -->
<ul hx-get="/latest" hx-swap="afterbegin">
  <!-- new items appear first -->
  <li>Old Item</li>
</ul>

<!-- delete: Remove the target (no response body needed) -->
<button hx-delete="/items/1" hx-target="closest li" hx-swap="delete">
  Remove
</button>

<!-- none: Make request but don't swap anything -->
<button hx-post="/track-click" hx-swap="none">
  Track Me
</button>
```

**Swap modifiers** (combine with any strategy):

```html
<!-- Add transition timing -->
<div hx-get="/content" hx-swap="innerHTML swap:500ms">
  <!-- Waits 500ms before swapping -->
</div>

<!-- Settle time for CSS transitions -->
<div hx-get="/content" hx-swap="innerHTML settle:300ms">
  <!-- Allows 300ms for CSS transitions after swap -->
</div>

<!-- Scroll behavior -->
<div hx-get="/content" hx-swap="innerHTML scroll:top">
  <!-- Scrolls to top of target after swap -->
</div>

<!-- Focus behavior -->
<div hx-get="/form" hx-swap="innerHTML focus-scroll:true">
  <!-- Scrolls to focused element -->
</div>
```

### 3.4 Triggers: `hx-trigger`

Controls when requests fire:

```html
<!-- Standard events -->
<button hx-get="/data" hx-trigger="click">Click me</button>
<input hx-get="/search" hx-trigger="keyup">
<form hx-post="/submit" hx-trigger="submit">

<!-- Multiple triggers -->
<input hx-get="/search" hx-trigger="keyup, change">

<!-- Modifiers -->
<input hx-get="/search" hx-trigger="keyup changed delay:500ms">
<!--
  - changed: only if value changed
  - delay:500ms: debounce (wait 500ms after last event)
-->

<!-- Throttle (max once per interval) -->
<div hx-get="/status" hx-trigger="every 2s">
  <!-- Polls every 2 seconds -->
</div>

<!-- Trigger once -->
<img hx-get="/load-image" hx-trigger="revealed once">
<!--
  - revealed: when element scrolls into viewport
  - once: only trigger one time
-->

<!-- From another element -->
<input id="search" name="q">
<div hx-get="/results" hx-trigger="keyup from:#search delay:300ms">
  <!-- Triggered by keyup on the input -->
</div>

<!-- Load trigger (fires on page load) -->
<div hx-get="/initial-data" hx-trigger="load">
  Loading...
</div>

<!-- Intersection observer -->
<div hx-get="/more" hx-trigger="intersect threshold:0.5">
  <!-- Fires when 50% visible -->
</div>
```

**Common trigger patterns:**

```html
<!-- Live search -->
<input type="search" name="q"
       hx-get="/search"
       hx-trigger="input changed delay:300ms"
       hx-target="#results">

<!-- Infinite scroll -->
<div hx-get="/page/2"
     hx-trigger="revealed"
     hx-swap="afterend">
  Loading more...
</div>

<!-- Auto-save -->
<form hx-post="/autosave"
      hx-trigger="change delay:1s"
      hx-swap="none">
```

### 3.5 Indicators: `hx-indicator`

Shows loading states during requests:

```html
<style>
  /* htmx adds htmx-request class during requests */
  .htmx-indicator {
    display: none;
  }
  .htmx-request .htmx-indicator {
    display: inline;
  }
  .htmx-request.htmx-indicator {
    display: inline;
  }
</style>

<!-- Indicator inside the triggering element -->
<button hx-get="/slow-operation">
  Submit
  <span class="htmx-indicator">Loading...</span>
</button>

<!-- Indicator elsewhere -->
<button hx-get="/data" hx-indicator="#spinner">Load</button>
<div id="spinner" class="htmx-indicator">
  <img src="/spinner.gif" alt="Loading">
</div>

<!-- Disable button during request -->
<button hx-get="/data" hx-disabled-elt="this">
  Click Me
</button>
```

### 3.6 History: `hx-push-url`

Updates browser history and URL:

```html
<!-- Push new URL to history -->
<a hx-get="/products/123"
   hx-target="#main"
   hx-push-url="true">
  View Product
</a>

<!-- Push custom URL -->
<button hx-get="/search?q=widgets"
        hx-target="#results"
        hx-push-url="/search/widgets">
  Search Widgets
</button>

<!-- Replace current history entry (no back button) -->
<form hx-post="/step2"
      hx-target="#wizard"
      hx-replace-url="true">
```

**History restoration:** When users hit back/forward, htmx automatically restores the previous page state via AJAX (using the saved URL).

### 3.7 Out-of-Band Swaps: `hx-swap-oob`

Update multiple parts of the page from a single response:

```html
<!-- In your response HTML: -->
<div id="main-content">
  <!-- This goes to the normal target -->
  <h1>Product Details</h1>
  ...
</div>

<!-- These swap themselves by ID, regardless of target -->
<div id="cart-count" hx-swap-oob="true">3 items</div>
<div id="notifications" hx-swap-oob="innerHTML">
  <span class="badge">New message!</span>
</div>
```

**OOB with different swap strategies:**

```html
<!-- Append to a list -->
<li id="todo-list" hx-swap-oob="beforeend">
  <span>New todo item</span>
</li>

<!-- Replace specific element -->
<tr id="row-123" hx-swap-oob="outerHTML">
  <td>Updated data</td>
</tr>

<!-- Delete an element -->
<div id="flash-message" hx-swap-oob="delete"></div>
```

**Use cases:**
- Update shopping cart count after adding item
- Show toast notifications
- Update multiple related data (edit user → update user list AND user count)
- Refresh CSRF tokens

---

## 4. Razor Pages Fit

htmx and Razor Pages are a natural pairing. Razor Pages already returns HTML. htmx just asks for smaller pieces of it.

### 4.1 Handlers as Endpoints

Razor Pages handlers become htmx endpoints:

```csharp
// Pages/Products/Index.cshtml.cs
public class IndexModel : PageModel
{
    // Standard page load: GET /Products
    public void OnGet() { }

    // htmx endpoint: GET /Products?handler=Search&q=widget
    public IActionResult OnGetSearch(string q)
    {
        var results = _db.Products.Where(p => p.Name.Contains(q));
        return Partial("_ProductList", results);
    }

    // htmx endpoint: POST /Products?handler=AddToCart
    public IActionResult OnPostAddToCart(int productId)
    {
        _cart.Add(productId);
        return Partial("_CartSummary", _cart);
    }

    // htmx endpoint: DELETE /Products?handler=Remove&id=123
    public IActionResult OnDeleteRemove(int id)
    {
        _db.Products.Remove(id);
        return Content(""); // Empty response for hx-swap="delete"
    }
}
```

```html
<!-- Pages/Products/Index.cshtml -->
<input type="search" name="q"
       hx-get="?handler=Search"
       hx-trigger="input changed delay:300ms"
       hx-target="#product-list">

<div id="product-list">
    <partial name="_ProductList" model="Model.Products" />
</div>
```

### 4.2 Partials for Fragments

Create partial views that render just the fragment htmx needs:

```
Pages/
├── Products/
│   ├── Index.cshtml           # Full page
│   ├── Index.cshtml.cs        # Page model with handlers
│   └── _ProductList.cshtml    # Partial for htmx responses
│   └── _ProductCard.cshtml    # Smaller partial
│   └── _CartSummary.cshtml    # Another partial
```

```html
<!-- Pages/Products/_ProductList.cshtml -->
@model IEnumerable<Product>

@foreach (var product in Model)
{
    <partial name="_ProductCard" model="product" />
}

@if (!Model.Any())
{
    <p class="text-muted">No products found.</p>
}
```

```html
<!-- Pages/Products/_ProductCard.cshtml -->
@model Product

<div class="product-card" id="product-@Model.Id">
    <h3>@Model.Name</h3>
    <p>@Model.Price.ToString("C")</p>
    <button hx-post="?handler=AddToCart"
            hx-vals='{"productId": @Model.Id}'
            hx-target="#cart-summary">
        Add to Cart
    </button>
</div>
```

### 4.3 Validation + Antiforgery

**Antiforgery tokens:** Required for POST/PUT/PATCH/DELETE requests:

```html
<!-- Option 1: Include token in form -->
<form hx-post="?handler=Create">
    @Html.AntiForgeryToken()
    <!-- form fields -->
</form>

<!-- Option 2: Configure htmx to include token in all requests -->
<script>
    document.body.addEventListener('htmx:configRequest', (e) => {
        e.detail.headers['RequestVerificationToken'] =
            document.querySelector('input[name="__RequestVerificationToken"]').value;
    });
</script>

<!-- Place a token somewhere on the page -->
@Html.AntiForgeryToken()
```

**Server-side validation with htmx:**

```csharp
public IActionResult OnPostCreate()
{
    if (!ModelState.IsValid)
    {
        // Return the form partial with validation errors
        Response.Headers["HX-Retarget"] = "#create-form";
        Response.Headers["HX-Reswap"] = "outerHTML";
        return Partial("_CreateForm", Input);
    }

    // Success: return updated list
    return Partial("_ProductList", _db.Products.ToList());
}
```

```html
<!-- _CreateForm.cshtml -->
@model CreateProductInput

<form id="create-form" hx-post="?handler=Create" hx-target="#product-list">
    @Html.AntiForgeryToken()

    <div class="form-group">
        <label asp-for="Name"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>

    <button type="submit">Create</button>
</form>
```

**Client-side validation (optional enhancement):**

```html
<!-- Inline validation as user types -->
<input asp-for="Email"
       hx-get="?handler=ValidateEmail"
       hx-trigger="blur changed"
       hx-target="next .validation-message">
<span class="validation-message"></span>
```

```csharp
public IActionResult OnGetValidateEmail(string email)
{
    if (string.IsNullOrEmpty(email))
        return Content("<span class='text-danger'>Email is required</span>");

    if (!IsValidEmail(email))
        return Content("<span class='text-danger'>Invalid email format</span>");

    if (_db.Users.Any(u => u.Email == email))
        return Content("<span class='text-danger'>Email already in use</span>");

    return Content("<span class='text-success'>✓</span>");
}
```

---

## Quick Reference Card

| htmx Attribute    | Purpose                | Example                              |
|-------------------|------------------------|--------------------------------------|
| `hx-get`          | GET request            | `hx-get="/api/data"`                 |
| `hx-post`         | POST request           | `hx-post="/api/submit"`              |
| `hx-put`          | PUT request            | `hx-put="/api/update"`               |
| `hx-patch`        | PATCH request          | `hx-patch="/api/modify"`             |
| `hx-delete`       | DELETE request         | `hx-delete="/api/remove"`            |
| `hx-target`       | Where to swap response | `hx-target="#results"`               |
| `hx-swap`         | How to swap            | `hx-swap="outerHTML"`                |
| `hx-trigger`      | When to trigger        | `hx-trigger="click"`                 |
| `hx-indicator`    | Loading indicator      | `hx-indicator="#spinner"`            |
| `hx-push-url`     | Update browser URL     | `hx-push-url="true"`                 |
| `hx-vals`         | Extra values to send   | `hx-vals='{"id": 1}'`                |
| `hx-headers`      | Extra headers          | `hx-headers='{"X-Custom": "value"}'` |
| `hx-confirm`      | Confirmation dialog    | `hx-confirm="Are you sure?"`         |
| `hx-disabled-elt` | Disable during request | `hx-disabled-elt="this"`             |
| `hx-swap-oob`     | Out-of-band swap       | `hx-swap-oob="true"`                 |

---

## Mental Model Summary

1. **Server owns the state.** Your Razor Page model is the source of truth.

2. **Server renders the UI.** Partials return ready-to-display HTML fragments.

3. **HTML is the contract.** No JSON serialization, no client-side templates.

4. **Links and forms drive state.** htmx just makes more elements behave like links and forms.

5. **Enhance progressively.** Start with working forms, add htmx for smoothness.

```
┌───────────────────────────────────────────────────────────────────┐
│                         BROWSER                                   │
│                                                                   │
│   ┌─────────────┐    click     ┌─────────────┐                    │
│   │   Button    │ ───────────► │    htmx     │                    │
│   │  hx-get     │              │   library   │                    │
│   └─────────────┘              └──────┬──────┘                    │
│                                       │                           │
│                                  AJAX Request                     │
│                                  GET /products?handler=Search     │
└───────────────────────────────────────┼───────────────────────────┘
                                        │
                                        ▼
┌───────────────────────────────────────────────────────────────────┐
│                         SERVER                                    │
│                                                                   │
│   ┌──────────────────────────────────────────────────────────┐    │
│   │  IndexModel.OnGetSearch(q)                               │    │
│   │                                                          │    │
│   │  var results = _db.Products.Where(...);                  │    │
│   │  return Partial("_ProductList", results);                │    │
│   └──────────────────────────────────────────────────────────┘    │
│                           │                                       │
│                           ▼                                       │
│   ┌──────────────────────────────────────────────────────────┐    │
│   │  _ProductList.cshtml                                     │    │
│   │                                                          │    │
│   │  @foreach (var p in Model)                               │    │
│   │  {                                                       │    │
│   │      <div class="product">@p.Name</div>                  │    │
│   │  }                                                       │    │
│   └──────────────────────────────────────────────────────────┘    │
│                           │                                       │
│                      HTML Fragment                                │
└───────────────────────────┼───────────────────────────────────────┘
                            │
                            ▼
┌───────────────────────────────────────────────────────────────────┐
│                         BROWSER                                   │
│                                                                   │
│   ┌─────────────┐              ┌─────────────────────────────┐    │
│   │    htmx     │ ──────────►  │  <div id="product-list">    │    │
│   │   library   │   swap       │    <div>Widget A</div>      │    │
│   └─────────────┘   innerHTML  │    <div>Widget B</div>      │    │
│                                │  </div>                     │    │
│                                └─────────────────────────────┘    │
└───────────────────────────────────────────────────────────────────┘
```

**You're ready for the labs!**
