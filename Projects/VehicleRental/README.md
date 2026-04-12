# Vehicle Rental Management System

A web-based vehicle rental management system built with ASP.NET MVC for Bow Valley College — Web Programming course (Group 9).

## Live Demo

🌐 **[https://vehicle-rental-app.azurewebsites.net](https://vehicle-rental-app.azurewebsites.net)**

## Team Members

| Name | GitHub |
|---|---|
| Eddie Chongtham | [@eddie7ch](https://github.com/eddie7ch) |
| Zoubera Issaka | [@zoubera](https://github.com/zoubera) |

## Features

1. **Login & Registration** — Secure user authentication with SHA-256 password hashing
2. **Dashboard** — Overview of fleet, reservations, customers, and revenue
3. **Vehicle Management** — Add, edit, delete vehicles; track availability
4. **Customer Management** — Full customer database with CRUD operations
5. **Reservation Management** — Create, modify, and cancel reservations with an interactive pickup location map (Leaflet.js + OpenStreetMap)
6. **Billing** — Auto-calculate rental cost, tax, and additional charges; online payment via Stripe Checkout
7. **Reports** — Reservation history, revenue summary, and vehicle availability reports

## Getting Started

### Use the Live App
1. Visit [https://vehicle-rental-app.azurewebsites.net](https://vehicle-rental-app.azurewebsites.net)
2. Click **Register** to create a new account, or use the default admin:
   - Username: `admin`
   - Password: `Admin123!`

### Run Locally
```bash
git clone https://github.com/bvc-g9-vehicle-rental/Vehicle-Rental-Management-System.git
cd Vehicle-Rental-Management-System
dotnet run
```
> Requires .NET 10 SDK and SQL Server LocalDB

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET MVC (.NET 10) |
| Database | Entity Framework Core + Azure SQL |
| Auth | Custom session-based (SHA-256) |
| UI | Razor Views + Bootstrap 5 |
| Payments | Stripe Checkout (Stripe.net) |
| Maps | Leaflet.js + OpenStreetMap (CDN, no API key) |
| Hosting | Azure App Service (Free F1) |
| Version Control | Git + GitHub |

## Project Structure

```
VehicleRental/
├── Controllers/       # MVC Controllers (Account, Home, Vehicle, Customer, Reservation, Billing, Report)
├── Models/            # EF Core models + ViewModels
├── Views/             # Razor Views
├── Repositories/      # Repository pattern (data access layer)
├── Data/              # DbContext + DbSeeder
├── Migrations/        # EF Core migrations
└── wwwroot/           # Static files (CSS, JS)
```

## Branches

| Branch | Purpose |
|---|---|
| `main` | Stable production code |
| `eddie-dev` | Eddie's development branch |
| `zoubera-dev` | Zoubera's development branch |
