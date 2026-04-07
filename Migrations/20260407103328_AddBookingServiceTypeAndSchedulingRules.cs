using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCarShowroom.Migrations
{
    public partial class AddBookingServiceTypeAndSchedulingRules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Bookings', 'ServiceType') IS NULL
                BEGIN
                    ALTER TABLE [Bookings]
                    ADD [ServiceType] nvarchar(max) NOT NULL CONSTRAINT [DF_Bookings_ServiceType] DEFAULT N'Xem xe';
                END

                IF COL_LENGTH('Bookings', 'ServiceType') IS NOT NULL
                BEGIN
                    EXEC(N'
                        UPDATE [Bookings]
                        SET [ServiceType] = N''Xem xe''
                        WHERE [ServiceType] IS NULL OR LTRIM(RTRIM([ServiceType])) = N'''';
                    ');
                END
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Bookings]
                SET [BookingStatus] = CASE
                    WHEN [BookingStatus] = N'Mới đặt' THEN N'Chờ xác nhận'
                    WHEN [BookingStatus] = N'Cho xac nhan' THEN N'Chờ xác nhận'
                    WHEN [BookingStatus] = N'Đã xác nhận' THEN N'Đã xác nhận'
                    WHEN [BookingStatus] = N'Đã đến hẹn' THEN N'Đã đến hẹn'
                    WHEN [BookingStatus] = N'Đã bán' THEN N'Đã bán'
                    WHEN [BookingStatus] = N'Không bán được' THEN N'Không bán được'
                    WHEN [BookingStatus] = N'Đã hủy' THEN N'Đã hủy'
                    ELSE [BookingStatus]
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Bookings]
                SET [BookingStatus] = CASE
                    WHEN [BookingStatus] IN (N'Chờ xác nhận', N'Đề nghị đổi giờ', N'Đã từ chối') THEN N'Mới đặt'
                    ELSE [BookingStatus]
                END;
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Bookings', 'ServiceType') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;
                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    INNER JOIN sys.tables t ON t.object_id = c.object_id
                    WHERE t.name = 'Bookings' AND c.name = 'ServiceType';

                    IF @constraintName IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [Bookings] DROP CONSTRAINT [' + @constraintName + N']');
                    END

                    ALTER TABLE [Bookings] DROP COLUMN [ServiceType];
                END
                """);
        }
    }
}
