using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "CoffeeShopDB.db");

        public static string ConnectionString => $"Data Source={DbPath}";

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static void InitializeDatabase()
        {
            using var conn = GetConnection();

            string createTables = @"
                CREATE TABLE IF NOT EXISTS Admin (
                    AdminId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS UserDetails (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Phone TEXT,
                    Address TEXT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    PicLocation TEXT
                );

                CREATE TABLE IF NOT EXISTS Category (
                    CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoryName TEXT NOT NULL,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS Product (
                    ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoryId INTEGER,
                    ProductName TEXT NOT NULL,
                    Description TEXT,
                    Price REAL NOT NULL,
                    Stock INTEGER DEFAULT 0,
                    ImagePath TEXT,
                    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId)
                );

                CREATE TABLE IF NOT EXISTS Orders (
                    OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    OrderDate TEXT DEFAULT (datetime('now','localtime')),
                    TotalAmount REAL DEFAULT 0,
                    Status TEXT DEFAULT 'Pending',
                    FOREIGN KEY (UserId) REFERENCES UserDetails(UserId)
                );

                CREATE TABLE IF NOT EXISTS OrderDetails (
                    OrderDetailId INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER,
                    ProductId INTEGER,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    Subtotal REAL NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
                    FOREIGN KEY (ProductId) REFERENCES Product(ProductId)
                );

                CREATE TABLE IF NOT EXISTS Payment (
                    PaymentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER,
                    PaymentDate TEXT DEFAULT (datetime('now','localtime')),
                    Amount REAL NOT NULL,
                    PaymentMethod TEXT NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
                );

                CREATE TABLE IF NOT EXISTS Staff (
                    StaffId INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Phone TEXT,
                    Role TEXT DEFAULT 'Barista',
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                );
            ";

            using var cmd = new SqliteCommand(createTables, conn);
            cmd.ExecuteNonQuery();

            // Seed default admin
            SeedDefaultData(conn);
        }

        private static void SeedDefaultData(SqliteConnection conn)
        {
            // Insert default admin if not exists
            var checkAdmin = new SqliteCommand("SELECT COUNT(*) FROM Admin", conn);
            long adminCount = (long)checkAdmin.ExecuteScalar()!;
            if (adminCount == 0)
            {
                var insertAdmin = new SqliteCommand(
                    "INSERT INTO Admin (Username, Password) VALUES ('admin', 'admin123')", conn);
                insertAdmin.ExecuteNonQuery();
            }

            // Insert default categories if not exists
            var checkCat = new SqliteCommand("SELECT COUNT(*) FROM Category", conn);
            long catCount = (long)checkCat.ExecuteScalar()!;
            if (catCount == 0)
            {
                string[] categories = {
                    "INSERT INTO Category (CategoryName, Description) VALUES ('Hot Coffee', 'Freshly brewed hot coffee beverages')",
                    "INSERT INTO Category (CategoryName, Description) VALUES ('Cold Coffee', 'Chilled and iced coffee drinks')",
                    "INSERT INTO Category (CategoryName, Description) VALUES ('Tea', 'Premium tea selections')",
                    "INSERT INTO Category (CategoryName, Description) VALUES ('Pastries', 'Fresh baked goods and pastries')",
                    "INSERT INTO Category (CategoryName, Description) VALUES ('Snacks', 'Light snacks and sandwiches')"
                };
                foreach (var sql in categories)
                {
                    new SqliteCommand(sql, conn).ExecuteNonQuery();
                }
            }

            // Insert default products if not exists
            var checkProd = new SqliteCommand("SELECT COUNT(*) FROM Product", conn);
            long prodCount = (long)checkProd.ExecuteScalar()!;
            if (prodCount == 0)
            {
                string[] products = {
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (1, 'Espresso', 'Rich and bold single shot espresso', 120.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (1, 'Cappuccino', 'Espresso with steamed milk foam', 180.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (1, 'Latte', 'Espresso with steamed milk', 200.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (1, 'Americano', 'Espresso diluted with hot water', 150.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (1, 'Mocha', 'Espresso with chocolate and steamed milk', 220.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (2, 'Iced Latte', 'Chilled espresso with cold milk', 220.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (2, 'Cold Brew', 'Slow-steeped cold coffee', 200.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (2, 'Frappuccino', 'Blended iced coffee with cream', 280.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (3, 'Green Tea', 'Premium Japanese green tea', 100.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (3, 'Chai Latte', 'Spiced tea with steamed milk', 180.00, 100)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (4, 'Croissant', 'Buttery flaky French croissant', 120.00, 50)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (4, 'Chocolate Muffin', 'Rich chocolate chip muffin', 100.00, 50)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (5, 'Club Sandwich', 'Triple-decker club sandwich', 250.00, 30)",
                    "INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock) VALUES (5, 'Chicken Wrap', 'Grilled chicken Caesar wrap', 220.00, 30)"
                };
                foreach (var sql in products)
                {
                    new SqliteCommand(sql, conn).ExecuteNonQuery();
                }
            }
        }
    }
}
