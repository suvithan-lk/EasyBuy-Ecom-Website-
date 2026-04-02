/*
Runtime note:
- The backend now uses EF Core with SQL Server LocalDB through `Backend/Program.cs`.
- On first run, the API creates and seeds the `EasyBuy` database automatically from `Backend/Data/EasyBuyDbContext.cs` and `Backend/Data/SeedData.cs`.
- This script is now a manual reference/bootstrap file, not the runtime source of truth.
*/

IF DB_ID(N'EasyBuy') IS NULL
BEGIN
    CREATE DATABASE EasyBuy;
END;
GO

USE EasyBuy;
GO

DROP TABLE IF EXISTS dbo.ReviewLikes;
DROP TABLE IF EXISTS dbo.Reviews;
DROP TABLE IF EXISTS dbo.Payments;
DROP TABLE IF EXISTS dbo.OrderTrackingEvents;
DROP TABLE IF EXISTS dbo.OrderItems;
DROP TABLE IF EXISTS dbo.Orders;
DROP TABLE IF EXISTS dbo.Wishlist;
DROP TABLE IF EXISTS dbo.Cart;
DROP TABLE IF EXISTS dbo.ProductImages;
DROP TABLE IF EXISTS dbo.Products;
DROP TABLE IF EXISTS dbo.Coupons;
DROP TABLE IF EXISTS dbo.Addresses;
DROP TABLE IF EXISTS dbo.Categories;
DROP TABLE IF EXISTS dbo.Users;
GO

CREATE TABLE dbo.Users (
    Id NVARCHAR(50) NOT NULL PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    [Name] NVARCHAR(120) NOT NULL,
    Phone NVARCHAR(30) NULL,
    [Role] NVARCHAR(20) NOT NULL CONSTRAINT CK_Users_Role CHECK ([Role] IN (N'Admin', N'Customer', N'Vendor')),
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.Addresses (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(50) NOT NULL,
    AddressLine NVARCHAR(255) NOT NULL,
    City NVARCHAR(120) NOT NULL,
    PostalCode NVARCHAR(20) NOT NULL,
    IsShipping BIT NOT NULL CONSTRAINT DF_Addresses_IsShipping DEFAULT 0,
    IsBilling BIT NOT NULL CONSTRAINT DF_Addresses_IsBilling DEFAULT 0,
    CONSTRAINT FK_Addresses_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Categories (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(120) NOT NULL,
    Slug NVARCHAR(150) NOT NULL UNIQUE,
    [Description] NVARCHAR(255) NULL
);
GO

CREATE TABLE dbo.Products (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(150) NOT NULL,
    Slug NVARCHAR(180) NOT NULL UNIQUE,
    [Summary] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CompareAtPrice DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL CONSTRAINT CK_Products_Stock CHECK (Stock >= 0),
    CategoryId INT NOT NULL,
    Rating DECIMAL(3,2) NOT NULL CONSTRAINT DF_Products_Rating DEFAULT 0,
    ReviewCount INT NOT NULL CONSTRAINT DF_Products_ReviewCount DEFAULT 0,
    DiscountPercent AS CASE
        WHEN CompareAtPrice > Price AND CompareAtPrice > 0
            THEN CONVERT(INT, ROUND(((CompareAtPrice - Price) / CompareAtPrice) * 100, 0))
        ELSE 0
    END PERSISTED,
    WarrantyMonths INT NOT NULL CONSTRAINT DF_Products_Warranty DEFAULT 12,
    Badge NVARCHAR(50) NULL,
    Accent NVARCHAR(200) NULL,
    ShippingDays INT NOT NULL CONSTRAINT DF_Products_Shipping DEFAULT 3,
    IsFeatured BIT NOT NULL CONSTRAINT DF_Products_IsFeatured DEFAULT 0,
    IsNewArrival BIT NOT NULL CONSTRAINT DF_Products_IsNewArrival DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
);
GO

CREATE TABLE dbo.ProductImages (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsMain BIT NOT NULL CONSTRAINT DF_ProductImages_IsMain DEFAULT 0,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Coupons (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    DiscountType NVARCHAR(20) NOT NULL CONSTRAINT CK_Coupons_Type CHECK (DiscountType IN (N'Percent', N'Fixed')),
    DiscountValue DECIMAL(10,2) NOT NULL,
    MinimumSpend DECIMAL(10,2) NOT NULL CONSTRAINT DF_Coupons_MinSpend DEFAULT 0,
    ExpiryDate DATETIME2(0) NOT NULL,
    UsageLimit INT NOT NULL,
    UsedCount INT NOT NULL CONSTRAINT DF_Coupons_UsedCount DEFAULT 0
);
GO

CREATE TABLE dbo.Cart (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(50) NULL,
    GuestId NVARCHAR(80) NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CONSTRAINT CK_Cart_Quantity CHECK (Quantity > 0),
    AddedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Cart_AddedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Cart_Identity CHECK (
        (UserId IS NOT NULL AND GuestId IS NULL) OR
        (UserId IS NULL AND GuestId IS NOT NULL)
    ),
    CONSTRAINT FK_Cart_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Cart_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
GO

CREATE UNIQUE INDEX UX_Cart_User_Product
    ON dbo.Cart(UserId, ProductId)
    WHERE UserId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_Cart_Guest_Product
    ON dbo.Cart(GuestId, ProductId)
    WHERE GuestId IS NOT NULL;
GO

CREATE TABLE dbo.Wishlist (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(50) NOT NULL,
    ProductId INT NOT NULL,
    AddedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Wishlist_AddedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Wishlist_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Wishlist_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT UX_Wishlist_User_Product UNIQUE (UserId, ProductId)
);
GO

CREATE TABLE dbo.Orders (
    Id INT IDENTITY(1001,1) NOT NULL PRIMARY KEY,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    UserId NVARCHAR(50) NOT NULL,
    CouponId INT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    DiscountAmount DECIMAL(10,2) NOT NULL CONSTRAINT DF_Orders_Discount DEFAULT 0,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT CK_Orders_Status CHECK ([Status] IN (N'Pending', N'Paid', N'Shipped', N'Delivered', N'Cancelled')),
    PaymentId NVARCHAR(100) NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    TrackingNumber NVARCHAR(80) NULL,
    CourierName NVARCHAR(100) NULL,
    OrderDate DATETIME2(0) NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT SYSUTCDATETIME(),
    EstimatedDeliveryDate DATETIME2(0) NULL,
    ShippingDate DATETIME2(0) NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    ShippingCost DECIMAL(10,2) NOT NULL CONSTRAINT DF_Orders_Shipping DEFAULT 0,
    Notes NVARCHAR(300) NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Orders_Coupons FOREIGN KEY (CouponId) REFERENCES dbo.Coupons(Id)
);
GO

CREATE TABLE dbo.OrderItems (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
    UnitPrice DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
GO

CREATE TABLE dbo.OrderTrackingEvents (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId INT NOT NULL,
    EventCode NVARCHAR(50) NOT NULL,
    Title NVARCHAR(120) NOT NULL,
    [Description] NVARCHAR(255) NOT NULL,
    [Location] NVARCHAR(150) NOT NULL,
    OccurredAt DATETIME2(0) NOT NULL,
    IsCompleted BIT NOT NULL CONSTRAINT DF_OrderTrackingEvents_Completed DEFAULT 1,
    CONSTRAINT FK_OrderTrackingEvents_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Payments (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId INT NOT NULL UNIQUE,
    PayHerePaymentId NVARCHAR(100) NULL,
    Amount DECIMAL(10,2) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT CK_Payments_Status CHECK ([Status] IN (N'Success', N'Failed', N'Pending')),
    PaymentDate DATETIME2(0) NOT NULL CONSTRAINT DF_Payments_Date DEFAULT SYSUTCDATETIME(),
    WebhookData NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Reviews (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(50) NOT NULL,
    ProductId INT NOT NULL,
    Rating INT NOT NULL CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(1000) NOT NULL,
    Headline NVARCHAR(150) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Reviews_CreatedAt DEFAULT SYSUTCDATETIME(),
    VerifiedPurchase BIT NOT NULL CONSTRAINT DF_Reviews_Verified DEFAULT 0,
    CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
GO

CREATE TABLE dbo.ReviewLikes (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ReviewId INT NOT NULL,
    UserId NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_ReviewLikes_Reviews FOREIGN KEY (ReviewId) REFERENCES dbo.Reviews(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ReviewLikes_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UX_ReviewLikes_Review_User UNIQUE (ReviewId, UserId)
);
GO

CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
CREATE INDEX IX_Products_Name ON dbo.Products([Name]);
CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
CREATE INDEX IX_Reviews_ProductId ON dbo.Reviews(ProductId);
GO

CREATE OR ALTER TRIGGER dbo.trg_OrderItems_ReduceStock
ON dbo.OrderItems
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Stock = p.Stock - i.Quantity
    FROM dbo.Products AS p
    INNER JOIN inserted AS i ON i.ProductId = p.Id;
END;
GO

INSERT INTO dbo.Users (Id, Email, [Name], Phone, [Role])
VALUES
    (N'admin-001', N'admin@easybuy.demo', N'EasyBuy Admin', N'+94 77 000 0000', N'Admin'),
    (N'customer-001', N'ayesha@easybuy.demo', N'Ayesha Fernando', N'+94 71 555 0101', N'Customer');
GO

INSERT INTO dbo.Categories ([Name], Slug, [Description])
VALUES
    (N'Audio', N'audio', N'Headphones, speakers, and compact listening gear.'),
    (N'Workstations', N'workstations', N'Keyboards, docks, and productivity hardware.'),
    (N'Home Office', N'home-office', N'Desk essentials built for long sessions.'),
    (N'Wearables', N'wearables', N'Smart devices for motion, sleep, and focus.');
GO

INSERT INTO dbo.Products (
    [Name], Slug, [Summary], [Description], Price, CompareAtPrice, Stock, CategoryId,
    Rating, ReviewCount, WarrantyMonths, Badge, Accent, ShippingDays, IsFeatured, IsNewArrival
)
VALUES
    (N'Nebula ANC Headphones', N'nebula-anc-headphones', N'Studio-grade wireless headphones with adaptive noise control.',
        N'Nebula blends wide soundstage tuning, low-latency Bluetooth, and all-day comfort for focused work or travel.',
        189.00, 239.00, 14, 1, 4.80, 186, 24, N'Bestseller', N'linear-gradient(135deg, #13293d 0%, #1b4965 50%, #5fa8d3 100%)', 2, 1, 0),
    (N'Atlas Mechanical Keyboard', N'atlas-mechanical-keyboard', N'Low-profile aluminum keyboard with tactile silent switches.',
        N'Atlas is tuned for writers and coders who want precision, damping, and clean desk aesthetics without gaming excess.',
        129.00, 159.00, 21, 2, 4.70, 142, 18, N'New', N'linear-gradient(135deg, #302b63 0%, #0f0c29 100%)', 3, 1, 1),
    (N'Motion Ergonomic Chair', N'motion-ergonomic-chair', N'Breathable mesh chair with dynamic lumbar support and lockable recline.',
        N'Motion is tuned for long workdays with flexible lumbar support, waterfall seat geometry, and quiet multi-surface casters.',
        329.00, 399.00, 5, 3, 4.90, 74, 36, N'Editor Pick', N'linear-gradient(135deg, #000428 0%, #004e92 100%)', 5, 1, 0),
    (N'Verse Smartwatch', N'verse-smartwatch', N'Fitness-forward smartwatch with sleep coaching and dual-band GPS.',
        N'Verse focuses on recovery, movement, and clean notifications with a bright AMOLED display and five-day battery life.',
        219.00, 269.00, 17, 4, 4.70, 121, 24, N'Fitness Lab', N'linear-gradient(135deg, #1d4350 0%, #a43931 100%)', 2, 1, 1);
GO

INSERT INTO dbo.Products (
    [Name], Slug, [Summary], [Description], Price, CompareAtPrice, Stock, CategoryId,
    Rating, ReviewCount, WarrantyMonths, Badge, Accent, ShippingDays, IsFeatured, IsNewArrival
)
VALUES
    (N'Pulse 4K Webcam', N'pulse-4k-webcam', N'Ultra-clear webcam with auto-framing and low-light correction.',
        N'Pulse keeps remote meetings sharp with a wide sensor, privacy shutter, and software-free framing presets.',
        99.00, 129.00, 32, 2, 4.60, 94, 12, N'Remote Ready', N'linear-gradient(135deg, #1f4037 0%, #99f2c8 100%)', 2, 1, 0);
GO

INSERT INTO dbo.Coupons (Code, DiscountType, DiscountValue, MinimumSpend, ExpiryDate, UsageLimit, UsedCount)
VALUES
    (N'WELCOME10', N'Percent', 10.00, 100.00, DATEADD(MONTH, 4, SYSUTCDATETIME()), 500, 37),
    (N'FREESHIP', N'Fixed', 18.00, 80.00, DATEADD(MONTH, 2, SYSUTCDATETIME()), 250, 91);
GO

INSERT INTO dbo.Wishlist (UserId, ProductId)
VALUES
    (N'customer-001', 2),
    (N'customer-001', 4);
GO

INSERT INTO dbo.Orders (
    OrderNumber, UserId, CouponId, TotalAmount, DiscountAmount, [Status], PaymentId,
    PaymentMethod, TrackingNumber, CourierName, OrderDate, EstimatedDeliveryDate,
    ShippingAddress, ShippingCost, Notes
)
VALUES
    (N'EB-1001', N'customer-001', 1, 205.20, 22.80, N'Delivered', N'PAY-1001',
        N'Card', N'TRK-1001', N'EasyBuy Express', DATEADD(DAY, -12, SYSUTCDATETIME()), DATEADD(DAY, -8, SYSUTCDATETIME()),
        N'14 Lake View Avenue, Colombo 05, 00500', 0.00,
        N'Leave at reception if unavailable.');
GO

INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, UnitPrice)
VALUES
    (1001, 2, 1, 129.00),
    (1001, 5, 1, 99.00);
GO

INSERT INTO dbo.OrderTrackingEvents (OrderId, EventCode, Title, [Description], [Location], OccurredAt, IsCompleted)
VALUES
    (1001, N'order_placed', N'Order placed', N'We received your order and created the shipment.', N'EasyBuy online store', DATEADD(DAY, -12, SYSUTCDATETIME()), 1),
    (1001, N'payment_confirmed', N'Payment confirmed', N'Card payment was approved and captured.', N'Payment gateway', DATEADD(MINUTE, 7, DATEADD(DAY, -12, SYSUTCDATETIME())), 1),
    (1001, N'packed', N'Packed for dispatch', N'Your items were packed and sealed at the fulfilment hub.', N'Fulfilment hub - Colombo', DATEADD(HOUR, 3, DATEADD(DAY, -11, SYSUTCDATETIME())), 1),
    (1001, N'shipped', N'Shipped', N'The package left the hub and moved through the courier network.', N'EasyBuy Express linehaul', DATEADD(HOUR, 8, DATEADD(DAY, -10, SYSUTCDATETIME())), 1),
    (1001, N'out_for_delivery', N'Out for delivery', N'The courier is making the final delivery attempt today.', N'Colombo 05 delivery zone', DATEADD(HOUR, 8, DATEADD(DAY, -8, SYSUTCDATETIME())), 1),
    (1001, N'delivered', N'Delivered', N'Package delivered successfully to the provided address.', N'14 Lake View Avenue, Colombo 05', DATEADD(HOUR, 13, DATEADD(DAY, -8, SYSUTCDATETIME())), 1);
GO
