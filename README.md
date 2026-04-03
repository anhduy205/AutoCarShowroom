# AutoCarShowroom

Do an mon hoc xay dung website showroom o to bang ASP.NET Core MVC va Entity Framework Core.

## Tinh nang hien co

- Khach xem danh sach xe cong khai, tim kiem theo ten, loc theo hang, loai xe, nam, trang thai va khoang gia.
- Khach xem chi tiet tung mau xe va dat lich xem xe.
- Khach mua xe theo luong 1 xe / 1 don voi thanh toan mo phong.
- Admin dang nhap bang cookie authentication de quan ly xe, don mua, booking va doanh thu.
- Admin them, sua, xoa xe va upload anh xe vao `wwwroot/uploads/cars`.
- He thong tu migrate database va seed du lieu demo khi khoi dong neu bang `Cars` chua co du lieu.
- Anh demo duoc luu trong `wwwroot/images/catalog`.

## Tai khoan admin mac dinh

- Username: `admin`
- Password: `Admin@123`

Thong tin nay duoc cau hinh trong [`appsettings.json`](./appsettings.json).

## Cong nghe chinh

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server
- Cookie Authentication

## Huong dan chay local

1. Dam bao may da co SQL Server instance `.\SQLEXPRESS` hoac sua lai chuoi ket noi trong [`appsettings.json`](./appsettings.json).
2. Chay `dotnet restore`.
3. Chay `dotnet build`.
4. Chay `Update-Database` hoac `dotnet ef database update`.
5. Chay `dotnet run --launch-profile http`.
6. Mo `http://localhost:5090`.

Luu y:

- Trong moi truong `Development`, app khong ep redirect sang HTTPS nua de de test local hon.
- Khi khoi dong, app se tu `Migrate()` va tu seed du lieu demo neu database chua co du lieu xe.

## Database

- Runtime dang dung SQL Server qua `DefaultConnection` trong [`appsettings.json`](./appsettings.json).
- `ShowroomDbContext` va migration nam trong `Models/` va `Migrations/`.
- Script reset database thu cong nam trong [`database/setup.sql`](./database/setup.sql).
- Neu dung EF Core binh thuong, uu tien migration thay vi chay script SQL tay.

## Thu muc quan trong

- `Controllers/`: luong MVC cho khach va admin
- `Models/`: entity, workflow va DbContext
- `ViewModels/`: model hien thi cho tung man hinh
- `Views/`: giao dien Razor
- `Data/`: seeder du lieu demo
- `Validation/`: rule validate dung chung
- `wwwroot/`: CSS, JS, anh demo va anh upload

## Ghi chu khi bao cao do an

- He thong co 2 luong khach hang: mua xe truc tiep va dat lich xem xe.
- He thong co 3 khu admin: quan ly xe, quan ly booking, quan ly doanh thu.
- Doanh thu hien dang la luong nhap tay de phu hop pham vi do an.
- Thanh toan hien la mo phong noi bo, chua tich hop cong thanh toan that.
