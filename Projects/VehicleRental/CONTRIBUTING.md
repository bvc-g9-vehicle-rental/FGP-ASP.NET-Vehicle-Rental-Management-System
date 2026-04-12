# Contributing Guide — Zoubera Issaka

Hi Zoubera! First of all, thank you so much for agreeing to be my project teammate — I really appreciate it and I'm glad we're working together on this!

I just want to say that I'm very easy-going and flexible when it comes to how we work on this. I've already built out the full project to get us started, but please don't feel like you can't touch anything. This is our project together, not just mine.

You are completely welcome to:
- Change anything you don't like — code, layout, styling, structure
- Redo parts of the project your own way if you prefer
- Take ownership of any feature or module and rewrite it
- Contribute as much or as little as you'd like — there's no pressure either way
- Suggest improvements and I'll be happy to work on them together

Honestly, I just want us to submit a good project and do well. It doesn't matter to me who did what — we're a team and I'm happy however you want to contribute. So please make yourself at home in the code!

If anything is confusing or you want to hop on a quick call to walk through it together, just let me know. 😊

---

This guide will help you get set up and understand the project structure.

## Your Branch

Your branch is **`zoubera-dev`**. Always work here — never directly on `main`.

```bash
# Clone the repo (first time only)
git clone https://github.com/bvc-g9-vehicle-rental/Vehicle-Rental-Management-System.git
cd Vehicle-Rental-Management-System

# Switch to your branch
git checkout zoubera-dev
```

## Running the App Locally

You need: [.NET 10 SDK](https://dotnet.microsoft.com/download) + [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

```bash
dotnet run
```

Then open `http://localhost:5254` in your browser.

Or just use the live app: **[https://vehicle-rental-app.azurewebsites.net](https://vehicle-rental-app.azurewebsites.net)**

---

## Project Structure (what matters for you)

```
VehicleRental/
├── Views/                        ← YOUR MAIN FOCUS
│   ├── Shared/
│   │   └── _Layout.cshtml        ← Master layout (navbar, footer — shared by ALL pages)
│   ├── Account/
│   │   ├── Login.cshtml          ← Login page
│   │   └── Register.cshtml       ← Register page
│   ├── Home/
│   │   └── Index.cshtml          ← Dashboard page
│   ├── Vehicle/
│   │   ├── Index.cshtml          ← Vehicle list
│   │   ├── Create.cshtml         ← Add vehicle form
│   │   ├── Edit.cshtml           ← Edit vehicle form
│   │   ├── Details.cshtml        ← Vehicle details
│   │   └── Delete.cshtml         ← Delete confirmation
│   ├── Customer/                 ← Same structure as Vehicle
│   ├── Reservation/              ← Same structure as Vehicle + Cancel.cshtml
│   ├── Billing/
│   │   ├── Index.cshtml          ← Billing list
│   │   └── Details.cshtml        ← Bill details
│   └── Report/
│       └── Index.cshtml          ← Reports page
│
├── wwwroot/                      ← Static files (CSS, JS, images)
│   └── css/
│       └── site.css              ← Global custom styles
│
├── Controllers/                  ← Eddie's code — connects data to views (read only)
├── Models/                       ← Eddie's code — database models (read only)
└── Repositories/                 ← Eddie's code — data access (read only)
```

---

## How Views Work (quick explanation)

Each page is a `.cshtml` file — it's HTML with some C# mixed in using `@`.

Example from `Views/Vehicle/Index.cshtml`:
```html
@model IEnumerable<VehicleRental.Models.Vehicle>  <!-- data passed from controller -->

<h2>Vehicles</h2>
<table class="table">
    @foreach (var v in Model) {   <!-- loop through vehicles -->
        <tr>
            <td>@v.Make</td>      <!-- output a value -->
            <td>@v.Model</td>
        </tr>
    }
</table>
```

The **controller** fetches data from the database and passes it to the view. You don't need to touch the controllers — just focus on how the data looks in the view.

---

## The Shared Layout

**`Views/Shared/_Layout.cshtml`** is the master template — the navbar and overall page structure lives here. Every other page is injected into it via `@RenderBody()`. If you want to change the navbar or add something that appears on every page, edit this file.

Bootstrap 5 is already included via CDN in `_Layout.cshtml`.

---

## What You Can Work On

These are just suggestions — please don't feel any pressure. The project is already fully complete and working, so if you're happy with how everything looks and functions, there's absolutely nothing you need to change. But if you'd like to contribute, here are some ideas:

- Improve the styling of any page in `Views/`
- Make pages more responsive (mobile-friendly)
- Improve the navbar in `_Layout.cshtml`
- Add better form validation messages
- Improve the Dashboard (`Views/Home/Index.cshtml`) layout
- Add success/error alert styling
- Any UI improvements you think would look better

Totally up to you — even just reviewing the code and giving feedback counts as contributing!

## Presentation Recording

For the final submission (due April 17), we need to record a short presentation of the live app. I'm happy to do the recording myself, but if you'd like to do it or we can do it together, that would be great too. Just let me know what works best for you and we'll figure it out. 😊

---

## Saving Your Work

```bash
# After making changes
git add .
git commit -m "ui: improve dashboard layout"
git push origin zoubera-dev
```

Use clear commit messages starting with `ui:` or `style:` so it's easy to see what you changed.

---

## Questions?

Message Eddie on Teams/WhatsApp — or check the live app at [https://vehicle-rental-app.azurewebsites.net](https://vehicle-rental-app.azurewebsites.net) to see how things currently look.
