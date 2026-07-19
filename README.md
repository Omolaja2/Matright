# PharMarket ERP

**Pharmacy & Supermarket Enterprise Resource Planning System**

A comprehensive ERP solution built with ASP.NET Core MVC (.NET 10) designed specifically for Pharmacy and Supermarket operations. PharMarket handles inventory management, point-of-sale, financial tracking, and business analytics — all in one platform.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Architecture](#project-architecture)
- [Folder Structure](#folder-structure)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)

---

## Features

### 1. Inventory & Stock Management
- Track stock in **store** (backroom/warehouse) and on **shelf** (display floor)
- Real-time stock levels with automatic deduction on sales
- **Expiration date tracking** with alerts for near-expiry items
- **Out of stock tracking** with low-stock notifications
- Product categories (Pharmacy items, Grocery, Household, etc.)
- Supplier management

### 2. Pricing
- **Cost price** (purchase price from supplier)
- **Sales price** (retail price to customer)
- Automatic **profit margin** calculation
- Price history tracking

### 3. Point of Sale (POS)
- Fast checkout interface for cashier operations
- Multiple payment methods:
  - **Cash at hand** (physical cash)
  - **POS** (card payments)
  - **Transfer** (bank/mobile transfers)
- Receipt generation
- Daily sales summary

### 4. Purchasing & Stock-In
- Create purchase orders to suppliers
- Stock-in recording with quantity verification
- Automatic stock level updates on purchase
- Purchase history tracking

### 5. Financial Management
- **Expenses tracking** (rent, utilities, salaries, etc.)
- **Capital tracking** (initial investment, additional capital)
- **Cash at hand** monitoring
- **Profit & Loss analysis**
- Revenue vs Expenses dashboard

### 6. Tax Management
- Tax rate configuration (VAT, sales tax)
- Tax calculation on each sale
- Tax reports for filing

### 7. Dashboard & Analytics
- **Chart dashboard** with visual analytics:
  - Daily/Weekly/Monthly sales trends
  - Top-selling products
  - Revenue vs Expenses graph
  - Stock value overview
  - Profit margin analysis
  - Expiration alerts chart
- **Profit & Loss statement**
- Sales reports (by date range, by category, by payment method)
- Stock valuation reports

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 10 MVC |
| Language | C# |
| Database | SQL Server (via Entity Framework Core) |
| ORM | Entity Framework Core |
| Frontend | Razor Views + Bootstrap 5 |
| Charts | Chart.js |
| Icons | Font Awesome |
| JavaScript | jQuery |

---

## Project Architecture

The project follows **MVC (Model-View-Controller)** pattern with a **Service Layer** for business logic separation.

```
Presentation Layer (Views + Controllers)
            |
    Service Layer (Business Logic)
            |
    Data Layer (EF Core + Repository)
            |
      Database (SQL Server)
```

---

## Folder Structure

```
PharMarket/
|
|-- Controllers/                    # MVC Controllers
|   |-- HomeController.cs           # Dashboard / Home
|   |-- ProductsController.cs       # Product CRUD
|   |-- StockController.cs          # Stock management (store & shelf)
|   |-- SalesController.cs          # POS & Sales
|   |-- PurchasesController.cs      # Purchase orders & stock-in
|   |-- ExpensesController.cs       # Expense tracking
|   |-- ReportsController.cs        # Reports & Analytics
|   |-- TaxController.cs            # Tax configuration & reports
|   |-- SuppliersController.cs      # Supplier management
|   |-- CategoriesController.cs     # Category management
|   |-- AuthController.cs           # Login/Register (future)
|
|-- Models/                         # Entity Models (Database tables)
|   |-- Product.cs
|   |-- Category.cs
|   |-- Supplier.cs
|   |-- Stock.cs
|   |-- Sale.cs
|   |-- SaleItem.cs
|   |-- Purchase.cs
|   |-- PurchaseItem.cs
|   |-- Expense.cs
|   |-- Capital.cs
|   |-- Transaction.cs
|   |-- TaxSetting.cs
|   |-- ErrorViewModel.cs
|
|-- ViewModels/                     # View-specific models
|   |-- DashboardViewModel.cs
|   |-- POSViewModel.cs
|   |-- ProductViewModel.cs
|   |-- StockViewModel.cs
|   |-- SalesReportViewModel.cs
|   |-- ProfitLossViewModel.cs
|   |-- ExpenseViewModel.cs
|
|-- Data/                           # Database context & migrations
|   |-- AppDbContext.cs
|   |-- Migrations/
|
|-- Services/                       # Business logic layer
|   |-- IProductService.cs
|   |-- ProductService.cs
|   |-- IStockService.cs
|   |-- StockService.cs
|   |-- ISalesService.cs
|   |-- SalesService.cs
|   |-- IReportsService.cs
|   |-- ReportsService.cs
|   |-- IFinanceService.cs
|   |-- FinanceService.cs
|
|-- Views/                          # Razor Views
|   |-- Home/
|   |   |-- Index.cshtml            # Dashboard
|   |-- Products/
|   |   |-- Index.cshtml            # Product list
|   |   |-- Create.cshtml           # Add product
|   |   |-- Edit.cshtml             # Edit product
|   |   |-- Details.cshtml          # Product details
|   |-- Stock/
|   |   |-- Index.cshtml            # Stock overview
|   |   |-- Store.cshtml            # Store/backroom stock
|   |   |-- Shelf.cshtml            # Shelf/floor stock
|   |   |-- Transfer.cshtml         # Transfer stock store<->shelf
|   |   |-- LowStock.cshtml         # Out of stock items
|   |   |-- Expiring.cshtml         # Near-expiry items
|   |-- Sales/
|   |   |-- POS.cshtml              # Point of Sale interface
|   |   |-- Index.cshtml            # Sales history
|   |   |-- Details.cshtml          # Sale details
|   |-- Purchases/
|   |   |-- Index.cshtml            # Purchase list
|   |   |-- Create.cshtml           # Create purchase order
|   |   |-- Details.cshtml          # Purchase details
|   |-- Expenses/
|   |   |-- Index.cshtml            # Expense list
|   |   |-- Create.cshtml           # Add expense
|   |-- Reports/
|   |   |-- Sales.cshtml            # Sales reports
|   |   |-- ProfitLoss.cshtml       # P&L statement
|   |   |-- Stock.cshtml            # Stock valuation
|   |   |-- Tax.cshtml              # Tax reports
|   |-- Suppliers/
|   |   |-- Index.cshtml
|   |   |-- Create.cshtml
|   |-- Categories/
|   |   |-- Index.cshtml
|   |-- Shared/
|       |-- _Layout.cshtml          # Main layout
|       |-- _Sidebar.cshtml         # Navigation sidebar
|       |-- _TopNav.cshtml          # Top navigation bar
|       |-- _Footer.cshtml
|       |-- _ValidationScriptsPartial.cshtml
|
|-- wwwroot/                        # Static files
|   |-- css/                        # Custom styles
|   |-- js/                         # Custom JavaScript
|   |-- lib/                        # Third-party libraries
|   |   |-- bootstrap/
|   |   |-- jquery/
|   |   |-- chart.js/
|   |   |-- fontawesome/
|   |-- images/                     # Images & icons
|
|-- Properties/
|   |-- launchSettings.json
|
|-- Program.cs                      # Application entry point & configuration
|-- appsettings.json                # Configuration (connection string, etc.)
|-- PharMarket.csproj               # Project file
|-- PharMarket.sln                  # Solution file
|-- README.md
```

---

## Database Schema

### Products Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| Name | string | Product name |
| Barcode | string | Unique barcode/SKU |
| Description | string | Product description |
| CategoryId | int (FK) | Reference to Category |
| SupplierId | int (FK) | Reference to Supplier |
| CostPrice | decimal | Purchase price from supplier |
| SalesPrice | decimal | Retail price to customer |
| TaxRate | decimal | Tax percentage |
| MinimumStock | int | Minimum stock alert threshold |
| IsActive | bool | Active/Inactive status |
| CreatedAt | DateTime | Record creation date |

### Stock Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| ProductId | int (FK) | Reference to Product |
| StoreQuantity | int | Stock in backroom/store |
| ShelfQuantity | int | Stock on display shelf |
| ExpirationDate | DateTime? | Expiry date (nullable for non-expirable items) |

### Categories Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| Name | string | Category name |
| Description | string | Category description |

### Suppliers Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| Name | string | Supplier name |
| ContactPerson | string | Contact person |
| Phone | string | Phone number |
| Email | string | Email address |
| Address | string | Physical address |

### Sales Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| InvoiceNumber | string | Unique invoice number |
| SaleDate | DateTime | Date and time of sale |
| SubTotal | decimal | Total before tax |
| TaxAmount | decimal | Tax amount |
| TotalAmount | decimal | Grand total |
| PaymentMethod | enum | Cash / POS / Transfer |
| AmountPaid | decimal | Amount received |
| ChangeGiven | decimal | Change returned (cash) |
| CashierName | string | Name of cashier |

### SaleItems Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| SaleId | int (FK) | Reference to Sale |
| ProductId | int (FK) | Reference to Product |
| Quantity | int | Number of items sold |
| UnitPrice | decimal | Price at time of sale |
| CostPrice | decimal | Cost price at time of sale |
| Total | decimal | Line total |

### Purchases Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| OrderNumber | string | Unique order number |
| SupplierId | int (FK) | Reference to Supplier |
| PurchaseDate | DateTime | Date of purchase |
| TotalAmount | decimal | Total cost |
| Status | enum | Pending / Received / Cancelled |

### PurchaseItems Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| PurchaseId | int (FK) | Reference to Purchase |
| ProductId | int (FK) | Reference to Product |
| Quantity | int | Quantity ordered |
| UnitCost | decimal | Cost per unit |
| ExpirationDate | DateTime? | Expiry date of received goods |
| Total | decimal | Line total |

### Expenses Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| Description | string | Expense description |
| Amount | decimal | Expense amount |
| Category | string | Rent, Utilities, Salary, etc. |
| ExpenseDate | DateTime | Date of expense |
| PaymentMethod | enum | Cash / POS / Transfer |
| Receipt | string | Receipt reference/notes |

### Capital Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| Description | string | Description |
| Amount | decimal | Capital amount |
| Type | enum | Initial / Additional / Withdrawal |
| Date | DateTime | Date recorded |

### Transactions Table (Cash Tracking)
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| Type | enum | Sale / Expense / Capital / Transfer |
| ReferenceId | int? | ID of related record |
| Amount | decimal | Transaction amount |
| Direction | enum | Credit (in) / Debit (out) |
| PaymentMethod | enum | Cash / POS / Transfer |
| Description | string | Transaction description |
| TransactionDate | DateTime | Date and time |
| RunningBalance | decimal | Balance after transaction |

### TaxSettings Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment ID |
| TaxName | string | e.g., VAT |
| TaxRate | decimal | e.g., 7.5 |
| IsEnabled | bool | Active status |

---

## API Endpoints

### Dashboard
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` or `/Dashboard` | Main dashboard with charts |
| GET | `/Dashboard/SalesChart` | Sales data for chart (JSON) |
| GET | `/Dashboard/StockChart` | Stock levels chart (JSON) |
| GET | `/Dashboard/ProfitLossChart` | P&L chart (JSON) |

### Products
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Products` | List all products (with search/filter) |
| GET | `/Products/Create` | Show create form |
| POST | `/Products/Create` | Add new product |
| GET | `/Products/Edit/{id}` | Show edit form |
| POST | `/Products/Edit/{id}` | Update product |
| GET | `/Products/Details/{id}` | Product details with stock info |
| POST | `/Products/Delete/{id}` | Soft-delete product |
| GET | `/Products/Search?q=` | Search products by name/barcode |

### Stock
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Stock` | Stock overview (all products) |
| GET | `/Stock/Store` | Store/backroom stock only |
| GET | `/Stock/Shelf` | Shelf/floor stock only |
| POST | `/Stock/Transfer` | Transfer stock between store <-> shelf |
| GET | `/Stock/LowStock` | Out of stock / low stock items |
| GET | `/Stock/Expiring` | Items expiring within X days |
| POST | `/Stock/Adjust` | Manual stock adjustment |

### Sales (POS)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Sales/POS` | Point of Sale interface |
| POST | `/Sales/Create` | Process a new sale |
| GET | `/Sales` | Sales history (with date filter) |
| GET | `/Sales/Details/{id}` | Sale details / receipt |
| GET | `/Sales/DailySummary` | End-of-day summary |
| GET | `/Sales/ByPaymentMethod` | Sales grouped by payment method |

### Purchases
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Purchases` | List all purchases |
| GET | `/Purchases/Create` | Create purchase order form |
| POST | `/Purchases/Create` | Submit purchase order |
| GET | `/Purchases/Details/{id}` | Purchase details |
| POST | `/Purchases/Receive/{id}` | Mark purchase as received (stock-in) |
| GET | `/Purchases/BySupplier` | Purchases grouped by supplier |

### Expenses
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Expenses` | List all expenses |
| GET | `/Expenses/Create` | Add expense form |
| POST | `/Expenses/Create` | Submit new expense |
| GET | `/Expenses/Edit/{id}` | Edit expense form |
| POST | `/Expenses/Edit/{id}` | Update expense |
| GET | `/Expenses/ByCategory` | Expenses grouped by category |

### Reports
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Reports/Sales` | Sales report with filters |
| GET | `/Reports/ProfitLoss` | Profit & Loss statement |
| GET | `/Reports/Stock` | Stock valuation report |
| GET | `/Reports/Tax` | Tax summary report |
| GET | `/Reports/Expenses` | Expense report |
| GET | `/Reports/Export` | Export report as PDF/Excel |

### Suppliers
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Suppliers` | List all suppliers |
| GET | `/Suppliers/Create` | Add supplier form |
| POST | `/Suppliers/Create` | Submit new supplier |
| GET | `/Suppliers/Edit/{id}` | Edit supplier form |
| POST | `/Suppliers/Edit/{id}` | Update supplier |
| GET | `/Suppliers/Details/{id}` | Supplier details + purchase history |

### Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Categories` | List all categories |
| POST | `/Categories/Create` | Add new category |
| POST | `/Categories/Edit/{id}` | Update category |
| POST | `/Categories/Delete/{id}` | Delete category |

### Tax
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Tax` | Tax settings |
| POST | `/Tax/Update` | Update tax rate |

### Transactions (Cash at Hand)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Transactions` | Transaction history |
| GET | `/Transactions/CashAtHand` | Current cash balance |
| GET | `/Transactions/ByPaymentMethod` | Breakdown by Cash/POS/Transfer |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/PharMarket.git
   cd PharMarket
   ```

2. **Update connection string** in `appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PharMarketDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Run migrations** (after creating them)
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Open** `http://localhost:5000` in your browser

---

## License

This project is proprietary software for internal business use.

---

**Built with care for Pharmacy & Supermarket management.**
