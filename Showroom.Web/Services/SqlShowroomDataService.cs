using Microsoft.Data.SqlClient;
using Showroom.Web.Models;

namespace Showroom.Web.Services;

public class SqlShowroomDataService : IShowroomDataService
{
    private const string TotalCarsSql = """
        SELECT COUNT(*)
        FROM Cars;
        """;

    private const string CarsByBrandSql = """
        SELECT
            b.Name,
            COUNT(c.Id) AS CarCount
        FROM Brands b
        LEFT JOIN Cars c ON c.BrandId = b.Id
        GROUP BY b.Name
        ORDER BY CarCount DESC, b.Name;
        """;

    private const string BestSellingCarsSql = """
        SELECT TOP (@Take)
            c.Name,
            b.Name,
            SUM(oi.Quantity) AS SoldQuantity
        FROM OrderItems oi
        INNER JOIN Cars c ON c.Id = oi.CarId
        INNER JOIN Brands b ON b.Id = c.BrandId
        INNER JOIN Orders o ON o.Id = oi.OrderId
        WHERE o.Status IN ('Completed', 'Paid', 'Delivered')
        GROUP BY c.Name, b.Name
        ORDER BY SUM(oi.Quantity) DESC, c.Name;
        """;

    private const string CarsSql = """
        SELECT
            c.Id,
            c.Name,
            b.Name,
            c.Price,
            c.StockQuantity,
            c.CreatedAt
        FROM Cars c
        INNER JOIN Brands b ON b.Id = c.BrandId
        ORDER BY b.Name, c.Name;
        """;

    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlShowroomDataService> _logger;

    public SqlShowroomDataService(IConfiguration configuration, ILogger<SqlShowroomDataService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("ShowroomDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new AdminDashboardViewModel
            {
                IsDatabaseConnected = false,
                StatusMessage = "Chưa cấu hình chuỗi kết nối 'ShowroomDb' trong appsettings.json."
            };
        }

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var totalCars = await ReadTotalCarsAsync(connection, cancellationToken);
            var brandInventory = await ReadBrandInventoryAsync(connection, cancellationToken);
            var bestSellingCars = await ReadBestSellingCarsAsync(connection, cancellationToken);

            return new AdminDashboardViewModel
            {
                TotalCars = totalCars,
                BrandInventory = brandInventory,
                BestSellingCars = bestSellingCars,
                IsDatabaseConnected = true,
                StatusMessage = "Kết nối SQL Server thành công."
            };
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Could not load showroom statistics from SQL Server.");

            return new AdminDashboardViewModel
            {
                IsDatabaseConnected = false,
                StatusMessage = "Không thể tải dữ liệu showroom. Hãy kiểm tra tên cơ sở dữ liệu trong appsettings.json và chạy database/setup.sql nếu schema chưa được tạo."
            };
        }
    }

    public async Task<ShowroomKnowledgeViewModel> GetShowroomKnowledgeAsync(CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(cancellationToken);
        if (!dashboard.IsDatabaseConnected)
        {
            return new ShowroomKnowledgeViewModel
            {
                Dashboard = dashboard
            };
        }

        var connectionString = _configuration.GetConnectionString("ShowroomDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new ShowroomKnowledgeViewModel
            {
                Dashboard = dashboard
            };
        }

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            var cars = await ReadCarsAsync(connection, cancellationToken);

            return new ShowroomKnowledgeViewModel
            {
                Dashboard = dashboard,
                Cars = cars
            };
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Could not load showroom knowledge from SQL Server.");
            return new ShowroomKnowledgeViewModel
            {
                Dashboard = dashboard
            };
        }
    }

    private static async Task<int> ReadTotalCarsAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(TotalCarsSql, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? 0 : Convert.ToInt32(result);
    }

    private static async Task<IReadOnlyList<BrandInventoryItem>> ReadBrandInventoryAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(CarsByBrandSql, connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<BrandInventoryItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new BrandInventoryItem
            {
                BrandName = reader.GetString(0),
                CarCount = reader.GetInt32(1)
            });
        }

        return items;
    }

    private static async Task<IReadOnlyList<TopSellingCarItem>> ReadBestSellingCarsAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(BestSellingCarsSql, connection);
        command.Parameters.AddWithValue("@Take", 5);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<TopSellingCarItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TopSellingCarItem
            {
                CarName = reader.GetString(0),
                BrandName = reader.GetString(1),
                SoldQuantity = reader.GetInt32(2)
            });
        }

        return items;
    }

    private static async Task<IReadOnlyList<CarListItemViewModel>> ReadCarsAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(CarsSql, connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<CarListItemViewModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CarListItemViewModel
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                BrandName = reader.GetString(2),
                Price = reader.GetDecimal(3),
                StockQuantity = reader.GetInt32(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }

        return items;
    }
}
