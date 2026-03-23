using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCarShowroom.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyType",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Khác");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Chưa cập nhật");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Chưa cập nhật");

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Chưa cập nhật");

            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Thông số kỹ thuật đang được cập nhật.");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Còn hàng");

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Toyota',
                    ModelName = N'Camry',
                    Color = N'Nâu đồng',
                    BodyType = N'Sedan',
                    Status = N'Còn hàng',
                    Specifications = N'Động cơ 2.5L, hộp số tự động 8 cấp, 5 chỗ, ghế da, màn hình giải trí lớn.'
                WHERE CarName = N'Toyota Camry 2.5Q';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Honda',
                    ModelName = N'CR-V',
                    Color = N'Trắng ngọc',
                    BodyType = N'SUV',
                    Status = N'Khuyến mãi',
                    Specifications = N'Động cơ tăng áp 1.5L, 7 chỗ, gói an toàn Honda Sensing, cốp điện.'
                WHERE CarName = N'Honda CR-V RS';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Mazda',
                    ModelName = N'CX-5',
                    Color = N'Đỏ Soul Red',
                    BodyType = N'Crossover',
                    Status = N'Còn hàng',
                    Specifications = N'Động cơ 2.0L, 5 chỗ, màn hình HUD, camera 360, gói hỗ trợ lái i-Activsense.'
                WHERE CarName = N'Mazda CX-5 Premium';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Kia',
                    ModelName = N'Carnival',
                    Color = N'Đen ánh kim',
                    BodyType = N'MPV',
                    Status = N'Còn hàng',
                    Specifications = N'Động cơ diesel 2.2L, 7 chỗ, ghế thương gia hàng 2, cửa lùa điện hai bên.'
                WHERE CarName = N'Kia Carnival Signature';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Ford',
                    ModelName = N'Everest',
                    Color = N'Xanh rêu',
                    BodyType = N'SUV',
                    Status = N'Đã bán',
                    Specifications = N'Động cơ diesel 2.0L turbo, dẫn động 4x4, 7 chỗ, camera 360 và 6 chế độ lái.'
                WHERE CarName = N'Ford Everest Titanium';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Hyundai',
                    ModelName = N'Accent',
                    Color = N'Xanh dương',
                    BodyType = N'Sedan',
                    Status = N'Còn hàng',
                    Specifications = N'Động cơ 1.5L, hộp số CVT, màn hình 8 inch, điều hòa tự động và cốp rộng.'
                WHERE CarName = N'Hyundai Accent AT';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyType",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cars");
        }
    }
}
