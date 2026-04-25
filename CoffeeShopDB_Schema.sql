-- ============================================================
-- Coffee Shop Management System - Multi-Vendor Marketplace
-- Database: Microsoft SQL Server
-- Course: Object Oriented Programming - 2 (OOP-2)
-- ============================================================

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CoffeeShopDB')
BEGIN
    CREATE DATABASE CoffeeShopDB;
END
GO

USE CoffeeShopDB;
GO

-- ============================================================
-- DROP EXISTING TABLES (in correct FK order)
-- ============================================================
IF OBJECT_ID('dbo.PromoCode', 'U') IS NOT NULL DROP TABLE dbo.PromoCode;
IF OBJECT_ID('dbo.ItemReview', 'U') IS NOT NULL DROP TABLE dbo.ItemReview;
IF OBJECT_ID('dbo.Complaint', 'U') IS NOT NULL DROP TABLE dbo.Complaint;
IF OBJECT_ID('dbo.Payment', 'U') IS NOT NULL DROP TABLE dbo.Payment;
IF OBJECT_ID('dbo.OrderDetails', 'U') IS NOT NULL DROP TABLE dbo.OrderDetails;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Product', 'U') IS NOT NULL DROP TABLE dbo.Product;
IF OBJECT_ID('dbo.Staff', 'U') IS NOT NULL DROP TABLE dbo.Staff;
IF OBJECT_ID('dbo.Category', 'U') IS NOT NULL DROP TABLE dbo.Category;
IF OBJECT_ID('dbo.CoffeeShop', 'U') IS NOT NULL DROP TABLE dbo.CoffeeShop;
IF OBJECT_ID('dbo.UserDetails', 'U') IS NOT NULL DROP TABLE dbo.UserDetails;
IF OBJECT_ID('dbo.Admin', 'U') IS NOT NULL DROP TABLE dbo.Admin;
GO

-- ============================================================
-- TABLE: Admin
-- ============================================================
CREATE TABLE Admin (
    AdminId     INT IDENTITY(1,1) PRIMARY KEY,
    Username    VARCHAR(26)  NOT NULL UNIQUE,
    Password    VARCHAR(26)  NOT NULL
);
GO

-- ============================================================
-- TABLE: UserDetails (Customers)
-- ============================================================
CREATE TABLE UserDetails (
    UserId      INT IDENTITY(1,1) PRIMARY KEY,
    FirstName   VARCHAR(50)  NOT NULL,
    LastName    VARCHAR(50)  NOT NULL,
    Email       VARCHAR(100) NOT NULL,
    Phone       VARCHAR(15)  NULL,
    Address     VARCHAR(200) NULL,
    Username    VARCHAR(26)  NOT NULL UNIQUE,
    Password    VARCHAR(26)  NOT NULL,
    PicLocation VARCHAR(200) NULL
);
GO

-- ============================================================
-- TABLE: CoffeeShop (Multi-Vendor Shops)
-- ============================================================
CREATE TABLE CoffeeShop (
    ShopId          INT IDENTITY(1,1) PRIMARY KEY,
    ShopName        VARCHAR(100) NOT NULL,
    Location        VARCHAR(200) NOT NULL,
    Description     VARCHAR(500) NULL,
    Phone           VARCHAR(15)  NULL,
    Email           VARCHAR(100) NULL,
    AverageRating   DECIMAL(3,2) DEFAULT 0.00,
    ImagePath       VARCHAR(200) NULL,
    IsActive        BIT          DEFAULT 1,
    CreatedDate     DATETIME     DEFAULT GETDATE()
);
GO

-- ============================================================
-- TABLE: Category
-- ============================================================
CREATE TABLE Category (
    CategoryId    INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName  VARCHAR(50)  NOT NULL,
    Description   VARCHAR(200) NULL
);
GO

-- ============================================================
-- TABLE: Staff (belongs to a CoffeeShop)
-- ============================================================
CREATE TABLE Staff (
    StaffId     INT IDENTITY(1,1) PRIMARY KEY,
    ShopId      INT          NOT NULL,
    FirstName   VARCHAR(50)  NOT NULL,
    LastName    VARCHAR(50)  NOT NULL,
    Email       VARCHAR(100) NOT NULL,
    Phone       VARCHAR(15)  NULL,
    Role        VARCHAR(30)  DEFAULT 'Barista',
    Username    VARCHAR(26)  NOT NULL UNIQUE,
    Password    VARCHAR(26)  NOT NULL,
    FOREIGN KEY (ShopId) REFERENCES CoffeeShop(ShopId)
);
GO

-- ============================================================
-- TABLE: Product (belongs to a CoffeeShop)
-- ============================================================
CREATE TABLE Product (
    ProductId     INT IDENTITY(1,1) PRIMARY KEY,
    ShopId        INT          NOT NULL,
    CategoryId    INT          NOT NULL,
    ProductName   VARCHAR(100) NOT NULL,
    Description   VARCHAR(300) NULL,
    Price         DECIMAL(10,2) NOT NULL,
    Stock         INT          DEFAULT 0,
    ImagePath     VARCHAR(200) NULL,
    FOREIGN KEY (ShopId) REFERENCES CoffeeShop(ShopId),
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId)
);
GO

-- ============================================================
-- TABLE: Orders (belongs to a CoffeeShop)
-- ============================================================
CREATE TABLE Orders (
    OrderId        INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT          NOT NULL,
    ShopId         INT          NOT NULL,
    OrderDate      DATETIME     DEFAULT GETDATE(),
    TotalAmount    DECIMAL(10,2) DEFAULT 0,
    DiscountAmount DECIMAL(10,2) DEFAULT 0,
    PromoCode      VARCHAR(20)  NULL,
    Status         VARCHAR(30)  DEFAULT 'Awaiting Approval',
    FOREIGN KEY (UserId) REFERENCES UserDetails(UserId),
    FOREIGN KEY (ShopId) REFERENCES CoffeeShop(ShopId)
);
GO

-- ============================================================
-- TABLE: OrderDetails
-- ============================================================
CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId       INT          NOT NULL,
    ProductId     INT          NOT NULL,
    Quantity      INT          NOT NULL,
    UnitPrice     DECIMAL(10,2) NOT NULL,
    Subtotal      DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    FOREIGN KEY (ProductId) REFERENCES Product(ProductId)
);
GO

-- ============================================================
-- TABLE: Payment (with PaymentProvider for bKash/Nagad/Rocket)
-- ============================================================
CREATE TABLE Payment (
    PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT          NOT NULL,
    PaymentDate     DATETIME     DEFAULT GETDATE(),
    Amount          DECIMAL(10,2) NOT NULL,
    DiscountAmount  DECIMAL(10,2) DEFAULT 0,
    FinalAmount     DECIMAL(10,2) NOT NULL,
    PaymentMethod   VARCHAR(30)  NOT NULL,
    PaymentProvider VARCHAR(30)  NULL,
    PromoCode       VARCHAR(20)  NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
GO

-- ============================================================
-- TABLE: ItemReview (Customer reviews for products)
-- ============================================================
CREATE TABLE ItemReview (
    ReviewId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT          NOT NULL,
    ProductId   INT          NOT NULL,
    OrderId     INT          NOT NULL,
    Rating      INT          NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    ReviewText  VARCHAR(500) NULL,
    ReviewDate  DATETIME     DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES UserDetails(UserId),
    FOREIGN KEY (ProductId) REFERENCES Product(ProductId),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
GO

-- ============================================================
-- TABLE: Complaint
-- ============================================================
CREATE TABLE Complaint (
    ComplaintId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT          NOT NULL,
    ShopId        INT          NOT NULL,
    OrderId       INT          NULL,
    Subject       VARCHAR(100) NOT NULL,
    Description   VARCHAR(500) NOT NULL,
    Status        VARCHAR(20)  DEFAULT 'Open',   -- 'Open', 'In Progress', 'Resolved', 'Closed'
    CreatedDate   DATETIME     DEFAULT GETDATE(),
    ResolvedDate  DATETIME     NULL,
    FOREIGN KEY (UserId) REFERENCES UserDetails(UserId),
    FOREIGN KEY (ShopId) REFERENCES CoffeeShop(ShopId),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
GO

-- ============================================================
-- TABLE: PromoCode (Discount Codes)
-- ============================================================
CREATE TABLE PromoCode (
    PromoId       INT IDENTITY(1,1) PRIMARY KEY,
    Code          VARCHAR(20)  NOT NULL UNIQUE,
    DiscountPct   INT          NOT NULL CHECK (DiscountPct >= 1 AND DiscountPct <= 100),
    IsActive      BIT          DEFAULT 1,
    ExpiryDate    DATETIME     NULL
);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Default Admin
INSERT INTO Admin (Username, Password) VALUES ('admin', 'admin123');

-- Categories
INSERT INTO Category (CategoryName, Description) VALUES
    ('Hot Coffee',  'Freshly brewed hot coffee beverages'),
    ('Cold Coffee', 'Chilled and iced coffee drinks'),
    ('Tea',         'Premium tea selections'),
    ('Pastries',    'Fresh baked goods and pastries'),
    ('Snacks',      'Light snacks and sandwiches');

-- Coffee Shops (Multi-Vendor)
INSERT INTO CoffeeShop (ShopName, Location, Description, Phone, Email, AverageRating) VALUES
    ('Brew Haven',          'Dhanmondi 27, Dhaka',       'Artisan coffee house with handcrafted brews and cozy vibes',    '01711-223344', 'info@brewhaven.com',     4.50),
    ('The Daily Grind',     'Gulshan 2, Dhaka',          'Premium coffee bar serving specialty single-origin beans',       '01811-556677', 'hello@dailygrind.com',   4.20),
    ('Cafe Mocha Bliss',    'Banani 11, Dhaka',          'European-style cafe with exquisite pastries and coffee',         '01911-889900', 'contact@mochabliss.com', 4.70),
    ('Bean & Beyond',       'Uttara Sector 4, Dhaka',    'Modern coffee shop with a futuristic ambiance',                  '01611-112233', 'info@beanandbeyond.com', 3.90),
    ('Sunrise Roasters',    'Mirpur DOHS, Dhaka',        'Farm-to-cup coffee experience with in-house roasting',           '01511-445566', 'brew@sunriseroast.com',  4.30);

-- Staff (each belongs to a shop)
INSERT INTO Staff (ShopId, FirstName, LastName, Email, Phone, Role, Username, Password) VALUES
    (1, 'Rahim',  'Khan',    'rahim@brewhaven.com',    '01711-001122', 'Manager',  'rahim',  'staff123'),
    (2, 'Karim',  'Ahmed',   'karim@dailygrind.com',   '01811-003344', 'Barista',  'karim',  'staff123'),
    (3, 'Fatima', 'Begum',   'fatima@mochabliss.com',  '01911-005566', 'Cashier',  'fatima', 'staff123'),
    (4, 'Arif',   'Hossain', 'arif@beanandbeyond.com', '01611-007788', 'Barista',  'arif',   'staff123'),
    (5, 'Nusrat', 'Jahan',   'nusrat@sunriseroast.com','01511-009900', 'Manager',  'nusrat', 'staff123');

-- Products for Brew Haven (ShopId = 1)
INSERT INTO Product (ShopId, CategoryId, ProductName, Description, Price, Stock) VALUES
    (1, 1, 'Espresso',        'Rich and bold single shot espresso',        120.00, 100),
    (1, 1, 'Cappuccino',      'Espresso with steamed milk foam',           180.00, 100),
    (1, 1, 'Latte',           'Espresso with steamed milk',                200.00, 100),
    (1, 2, 'Iced Latte',      'Chilled espresso with cold milk',           220.00, 100),
    (1, 2, 'Cold Brew',       'Slow-steeped cold coffee',                  200.00, 100),
    (1, 3, 'Green Tea',       'Premium Japanese green tea',                100.00, 100),
    (1, 4, 'Croissant',       'Buttery flaky French croissant',            120.00,  50),
    (1, 5, 'Club Sandwich',   'Triple-decker club sandwich',               250.00,  30);

-- Products for The Daily Grind (ShopId = 2)
INSERT INTO Product (ShopId, CategoryId, ProductName, Description, Price, Stock) VALUES
    (2, 1, 'Americano',       'Espresso diluted with hot water',           150.00, 100),
    (2, 1, 'Mocha',           'Espresso with chocolate and steamed milk',  220.00, 100),
    (2, 1, 'Flat White',      'Double espresso with velvety steamed milk', 210.00, 100),
    (2, 2, 'Frappuccino',     'Blended iced coffee with cream',            280.00, 100),
    (2, 3, 'Chai Latte',      'Spiced tea with steamed milk',              180.00, 100),
    (2, 4, 'Chocolate Muffin','Rich chocolate chip muffin',                100.00,  50),
    (2, 5, 'Chicken Wrap',    'Grilled chicken Caesar wrap',               220.00,  30);

-- Products for Cafe Mocha Bliss (ShopId = 3)
INSERT INTO Product (ShopId, CategoryId, ProductName, Description, Price, Stock) VALUES
    (3, 1, 'Caramel Macchiato','Espresso with caramel drizzle and milk',   250.00, 100),
    (3, 1, 'Turkish Coffee',   'Strong traditional Turkish brew',          180.00, 100),
    (3, 2, 'Iced Mocha',       'Chilled mocha with whipped cream',         260.00, 100),
    (3, 2, 'Nitro Cold Brew',  'Nitrogen-infused cold coffee',             300.00, 100),
    (3, 3, 'Matcha Latte',     'Japanese matcha with steamed milk',        220.00, 100),
    (3, 4, 'Blueberry Scone',  'Fresh baked blueberry scone',              140.00,  50),
    (3, 5, 'Panini',           'Grilled Italian panini with cheese',       280.00,  30);

-- Products for Bean & Beyond (ShopId = 4)
INSERT INTO Product (ShopId, CategoryId, ProductName, Description, Price, Stock) VALUES
    (4, 1, 'Double Espresso',  'Intense double-shot espresso',             160.00, 100),
    (4, 1, 'Vanilla Latte',    'Latte with vanilla syrup',                 230.00, 100),
    (4, 2, 'Iced Americano',   'Chilled Americano over ice',               170.00, 100),
    (4, 3, 'Oolong Tea',       'Premium Chinese oolong tea',               130.00, 100),
    (4, 4, 'Cinnamon Roll',    'Warm cinnamon roll with icing',            150.00,  50),
    (4, 5, 'Veggie Wrap',      'Fresh vegetable and hummus wrap',          200.00,  30);

-- Products for Sunrise Roasters (ShopId = 5)
INSERT INTO Product (ShopId, CategoryId, ProductName, Description, Price, Stock) VALUES
    (5, 1, 'Pour Over',        'Hand-poured single origin coffee',         190.00, 100),
    (5, 1, 'Cortado',          'Equal parts espresso and steamed milk',    170.00, 100),
    (5, 2, 'Affogato',         'Espresso poured over vanilla gelato',      250.00, 100),
    (5, 2, 'Vietnamese Iced',  'Strong coffee with condensed milk on ice', 200.00, 100),
    (5, 3, 'Earl Grey',        'Classic English Earl Grey tea',            110.00, 100),
    (5, 4, 'Banana Bread',     'Moist homemade banana bread slice',        130.00,  50),
    (5, 5, 'Tuna Sandwich',    'Fresh tuna salad sandwich',                240.00,  30);

-- Sample Customer
INSERT INTO UserDetails (FirstName, LastName, Email, Phone, Address, Username, Password) VALUES
    ('Test', 'Customer', 'test@email.com', '01700-000000', 'Dhaka, Bangladesh', 'customer1', 'pass123');

-- Promo Codes
INSERT INTO PromoCode (Code, DiscountPct) VALUES ('AIUB20', 20);
INSERT INTO PromoCode (Code, DiscountPct) VALUES ('WELCOME10', 10);
INSERT INTO PromoCode (Code, DiscountPct) VALUES ('COFFEE50', 50);

GO

-- ============================================================
-- QUERIES FOR VERIFICATION
-- ============================================================
SELECT 'Admin'       AS TableName, COUNT(*) AS RecordCount FROM Admin
UNION ALL SELECT 'UserDetails', COUNT(*) FROM UserDetails
UNION ALL SELECT 'CoffeeShop',  COUNT(*) FROM CoffeeShop
UNION ALL SELECT 'Category',    COUNT(*) FROM Category
UNION ALL SELECT 'Staff',       COUNT(*) FROM Staff
UNION ALL SELECT 'Product',     COUNT(*) FROM Product
UNION ALL SELECT 'Orders',      COUNT(*) FROM Orders
UNION ALL SELECT 'Payment',     COUNT(*) FROM Payment
UNION ALL SELECT 'ItemReview',  COUNT(*) FROM ItemReview
UNION ALL SELECT 'Complaint',   COUNT(*) FROM Complaint
UNION ALL SELECT 'PromoCode',   COUNT(*) FROM PromoCode;
GO

PRINT '============================================================';
PRINT 'Coffee Shop Management System Database Created Successfully!';
PRINT '============================================================';
GO
