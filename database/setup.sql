IF DB_ID(N'AutoCarShowroomDb') IS NULL
BEGIN
    CREATE DATABASE AutoCarShowroomDb;
END;
GO

USE AutoCarShowroomDb;
GO

IF OBJECT_ID(N'dbo.RevenueRecords', N'U') IS NOT NULL DROP TABLE dbo.RevenueRecords;
IF OBJECT_ID(N'dbo.Bookings', N'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID(N'dbo.Cars', N'U') IS NOT NULL DROP TABLE dbo.Cars;
GO

CREATE TABLE dbo.Cars
(
    CarID INT IDENTITY(1,1) PRIMARY KEY,
    CarName NVARCHAR(MAX) NOT NULL,
    Brand NVARCHAR(MAX) NOT NULL,
    ModelName NVARCHAR(MAX) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Year INT NOT NULL,
    Color NVARCHAR(MAX) NOT NULL,
    BodyType NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(MAX) NOT NULL,
    Image NVARCHAR(MAX) NOT NULL,
    Specifications NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    EngineAndChassis NVARCHAR(MAX) NOT NULL,
    Exterior NVARCHAR(MAX) NOT NULL,
    Interior NVARCHAR(MAX) NOT NULL,
    Seats NVARCHAR(MAX) NOT NULL,
    Convenience NVARCHAR(MAX) NOT NULL,
    SecurityAndAntiTheft NVARCHAR(MAX) NOT NULL,
    ActiveSafety NVARCHAR(MAX) NOT NULL,
    PassiveSafety NVARCHAR(MAX) NOT NULL
);
GO

CREATE TABLE dbo.Orders
(
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode NVARCHAR(40) NOT NULL,
    CustomerName NVARCHAR(MAX) NOT NULL,
    PhoneNumber NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    Address NVARCHAR(MAX) NOT NULL,
    Note NVARCHAR(MAX) NULL,
    PaymentMethod NVARCHAR(MAX) NOT NULL,
    OrderStatus NVARCHAR(MAX) NOT NULL,
    PaymentStatus NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    CONSTRAINT UQ_Orders_OrderCode UNIQUE (OrderCode)
);
GO

CREATE TABLE dbo.OrderItems
(
    OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    CarId INT NOT NULL,
    CarName NVARCHAR(MAX) NOT NULL,
    CarImage NVARCHAR(MAX) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Cars FOREIGN KEY (CarId) REFERENCES dbo.Cars(CarID)
);
GO

CREATE TABLE dbo.Bookings
(
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    BookingCode NVARCHAR(40) NOT NULL,
    CarId INT NOT NULL,
    CarName NVARCHAR(MAX) NOT NULL,
    CarImage NVARCHAR(MAX) NOT NULL,
    QuotedPrice DECIMAL(18,2) NOT NULL,
    CustomerName NVARCHAR(MAX) NOT NULL,
    PhoneNumber NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    AppointmentAt DATETIME2 NOT NULL,
    Note NVARCHAR(MAX) NULL,
    BookingStatus NVARCHAR(MAX) NOT NULL,
    AdminNote NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Bookings_BookingCode UNIQUE (BookingCode),
    CONSTRAINT FK_Bookings_Cars FOREIGN KEY (CarId) REFERENCES dbo.Cars(CarID)
);
GO

CREATE TABLE dbo.RevenueRecords
(
    RevenueRecordId INT IDENTITY(1,1) PRIMARY KEY,
    Amount DECIMAL(18,2) NOT NULL,
    ReceivedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    SourceType NVARCHAR(MAX) NOT NULL,
    OrderId INT NULL,
    BookingId INT NULL,
    Note NVARCHAR(MAX) NULL,
    CONSTRAINT FK_RevenueRecords_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId),
    CONSTRAINT FK_RevenueRecords_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId)
);
GO

CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
CREATE INDEX IX_OrderItems_CarId ON dbo.OrderItems(CarId);
CREATE INDEX IX_Bookings_CarId ON dbo.Bookings(CarId);
CREATE INDEX IX_RevenueRecords_OrderId ON dbo.RevenueRecords(OrderId);
CREATE INDEX IX_RevenueRecords_BookingId ON dbo.RevenueRecords(BookingId);
GO

PRINT N'Schema da duoc tao theo dung cau truc hien tai cua project.';
PRINT N'Sau khi chay script, co the khoi dong app de EF Core seed du lieu demo vao bang Cars.';
GO
