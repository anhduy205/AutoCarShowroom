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
            AddRequiredNVarCharMaxColumnIfMissing(migrationBuilder, "BodyType", "Khac");
            EnsureBrandColumn(migrationBuilder);
            AddRequiredNVarCharMaxColumnIfMissing(migrationBuilder, "Color", "Chua cap nhat");
            AddRequiredNVarCharMaxColumnIfMissing(migrationBuilder, "ModelName", "Chua cap nhat");
            AddRequiredNVarCharMaxColumnIfMissing(migrationBuilder, "Specifications", "Thong so ky thuat dang duoc cap nhat.");
            AddRequiredNVarCharMaxColumnIfMissing(migrationBuilder, "Status", "Con hang");

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Toyota',
                    ModelName = N'Camry',
                    Color = N'Nau dong',
                    BodyType = N'Sedan',
                    Status = N'Con hang',
                    Specifications = N'Dong co 2.5L, hop so tu dong 8 cap, 5 cho, ghe da, man hinh giai tri lon.'
                WHERE CarName = N'Toyota Camry 2.5Q';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Honda',
                    ModelName = N'CR-V',
                    Color = N'Trang ngoc',
                    BodyType = N'SUV',
                    Status = N'Khuyen mai',
                    Specifications = N'Dong co tang ap 1.5L, 7 cho, goi an toan Honda Sensing, cop dien.'
                WHERE CarName = N'Honda CR-V RS';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Mazda',
                    ModelName = N'CX-5',
                    Color = N'Do Soul Red',
                    BodyType = N'Crossover',
                    Status = N'Con hang',
                    Specifications = N'Dong co 2.0L, 5 cho, man hinh HUD, camera 360, goi ho tro lai i-Activsense.'
                WHERE CarName = N'Mazda CX-5 Premium';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Kia',
                    ModelName = N'Carnival',
                    Color = N'Den anh kim',
                    BodyType = N'MPV',
                    Status = N'Con hang',
                    Specifications = N'Dong co diesel 2.2L, 7 cho, ghe thuong gia hang 2, cua lua dien hai ben.'
                WHERE CarName = N'Kia Carnival Signature';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Ford',
                    ModelName = N'Everest',
                    Color = N'Xanh reu',
                    BodyType = N'SUV',
                    Status = N'Da ban',
                    Specifications = N'Dong co diesel 2.0L turbo, dan dong 4x4, 7 cho, camera 360 va 6 che do lai.'
                WHERE CarName = N'Ford Everest Titanium';
                """);

            migrationBuilder.Sql("""
                UPDATE Cars
                SET Brand = N'Hyundai',
                    ModelName = N'Accent',
                    Color = N'Xanh duong',
                    BodyType = N'Sedan',
                    Status = N'Con hang',
                    Specifications = N'Dong co 1.5L, hop so CVT, man hinh 8 inch, dieu hoa tu dong va cop rong.'
                WHERE CarName = N'Hyundai Accent AT';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropColumnIfExists(migrationBuilder, "BodyType");
            DropColumnIfExists(migrationBuilder, "Color");
            DropColumnIfExists(migrationBuilder, "ModelName");
            DropColumnIfExists(migrationBuilder, "Specifications");
            DropColumnIfExists(migrationBuilder, "Status");

            DropDefaultConstraintIfExists(migrationBuilder, "Brand");
            migrationBuilder.Sql("""
                IF COL_LENGTH('Cars', 'Brand') IS NOT NULL
                BEGIN
                    UPDATE [Cars]
                    SET [Brand] = LEFT([Brand], 120)
                    WHERE LEN([Brand]) > 120;

                    ALTER TABLE [Cars]
                    ALTER COLUMN [Brand] nvarchar(120) NOT NULL;
                END
                """);
        }

        private static void AddRequiredNVarCharMaxColumnIfMissing(MigrationBuilder migrationBuilder, string columnName, string defaultValue)
        {
            migrationBuilder.Sql($"""
                IF COL_LENGTH('Cars', '{columnName}') IS NULL
                BEGIN
                    ALTER TABLE [Cars]
                    ADD [{columnName}] nvarchar(max) NOT NULL DEFAULT N'{defaultValue}';
                END
                """);
        }

        private static void EnsureBrandColumn(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Cars', 'Brand') IS NULL
                BEGIN
                    ALTER TABLE [Cars]
                    ADD [Brand] nvarchar(max) NOT NULL DEFAULT N'Chua cap nhat';
                END
                """);

            DropDefaultConstraintIfExists(migrationBuilder, "Brand");
            migrationBuilder.Sql("""
                IF COL_LENGTH('Cars', 'Brand') IS NOT NULL
                BEGIN
                    ALTER TABLE [Cars]
                    ALTER COLUMN [Brand] nvarchar(max) NOT NULL;
                END
                """);
        }

        private static void DropColumnIfExists(MigrationBuilder migrationBuilder, string columnName)
        {
            migrationBuilder.Sql($"""
                IF COL_LENGTH('Cars', '{columnName}') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;
                    DECLARE @sql nvarchar(max);

                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON c.default_object_id = dc.object_id
                    INNER JOIN sys.tables t
                        ON t.object_id = c.object_id
                    WHERE t.name = 'Cars'
                      AND c.name = '{columnName}';

                    IF @constraintName IS NOT NULL
                    BEGIN
                        SET @sql = N'ALTER TABLE [Cars] DROP CONSTRAINT ' + QUOTENAME(@constraintName);
                        EXEC sp_executesql @sql;
                    END

                    SET @sql = N'ALTER TABLE [Cars] DROP COLUMN [{columnName}]';
                    EXEC sp_executesql @sql;
                END
                """);
        }

        private static void DropDefaultConstraintIfExists(MigrationBuilder migrationBuilder, string columnName)
        {
            migrationBuilder.Sql($"""
                IF COL_LENGTH('Cars', '{columnName}') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;
                    DECLARE @sql nvarchar(max);

                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON c.default_object_id = dc.object_id
                    INNER JOIN sys.tables t
                        ON t.object_id = c.object_id
                    WHERE t.name = 'Cars'
                      AND c.name = '{columnName}';

                    IF @constraintName IS NOT NULL
                    BEGIN
                        SET @sql = N'ALTER TABLE [Cars] DROP CONSTRAINT ' + QUOTENAME(@constraintName);
                        EXEC sp_executesql @sql;
                    END
                END
                """);
        }
    }
}
