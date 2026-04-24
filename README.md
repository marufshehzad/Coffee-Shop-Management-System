# ☕ Coffee Shop Management System

A complete **C# Windows Forms** desktop application for managing a coffee shop, built with **.NET 9.0** and **SQLite** database.

> **Course:** Object Oriented Programming - 2 (OOP-2)

## 🚀 Features

### Customer
- Browse coffee menu with category filter & search
- Add items to cart and place orders
- Make payments (Cash, Credit Card, Debit Card, Mobile Banking, bKash)
- View order & payment history
- Manage account profile & change password

### Staff / Barista
- Dashboard with order & revenue stats
- Manage products (Add / Edit / Delete menu items)
- Track & update order status (Pending → Preparing → Ready → Completed)
- View payment records

### Admin
- Full system dashboard with stats
- View & delete users, staff, products
- View all orders & payments with revenue summary
- Create new staff accounts with role assignment

## 🗂️ Database Schema

| Table | Description |
|-------|-------------|
| `Admin` | Admin credentials |
| `UserDetails` | Customer accounts |
| `Staff` | Staff accounts with roles |
| `Category` | Product categories |
| `Product` | Menu items (price, stock, description) |
| `Orders` | Orders with status tracking |
| `OrderDetails` | Items in each order |
| `Payment` | Payment records |

## 🔑 Default Credentials

| Role | Username | Password |
|------|----------|----------|
| Admin | `admin` | `admin123` |
| Customer | *Sign up via the app* | |
| Staff | *Created by Admin* | |

## 🛠️ Tech Stack

- **Language:** C# (.NET 9.0)
- **UI Framework:** Windows Forms
- **Database:** SQLite (via Microsoft.Data.Sqlite)
- **Architecture:** Desktop Application with ADO.NET

## 📦 How to Run

```bash
# Clone the repository
git clone https://github.com/marufshehzad/Coffee-Shop-Management-System.git

# Navigate to project directory
cd Coffee-Shop-Management-System

# Run the application
dotnet run
```

## 📸 Application Screens

- **Login Page** - Tabbed login for Customer / Staff / Admin
- **Customer Sign Up** - Registration with validation
- **Customer Dashboard** - Browse menu, place orders, make payments
- **Staff Dashboard** - Manage products, track orders
- **Admin Dashboard** - Full system control panel

## 📁 Project Structure

```
CoffeeShopManagement/
├── Program.cs                  # Entry point
├── DatabaseHelper.cs           # SQLite DB setup & seed data
├── CoffeeShopManagement.csproj # Project file
└── Forms/
    ├── LoginForm.cs            # Tabbed login
    ├── CustomerSignUpForm.cs   # Customer registration
    ├── CustomerDashboard.cs    # Customer main interface
    ├── MenuBrowseForm.cs       # Menu browsing & ordering
    ├── AccountForm.cs          # Profile management
    ├── StaffDashboard.cs       # Staff operations
    ├── ProductManageForm.cs    # Product CRUD
    └── AdminDashboard.cs       # Admin control panel
```

## 📝 License

This project is created for academic purposes.
