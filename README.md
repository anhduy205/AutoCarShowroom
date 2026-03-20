# AutoCarShowroom

Do an mon hoc xay dung website showroom xe bang ASP.NET Core MVC va Entity Framework Core.

## Chuc nang hien co

- Xem danh sach xe cong khai, tim kiem theo ten, loc theo nam va sap xep theo gia/nam.
- Xem chi tiet tung mau xe trong showroom.
- Dang nhap admin bang cookie authentication de truy cap chuc nang quan tri.
- Them, sua, xoa xe trong he thong.
- Upload anh xe that vao `wwwroot/uploads/cars`.
- Tu dong seed du lieu demo khi database chua co xe.

## Tai khoan admin mac dinh

- Username: `admin`
- Password: `Admin@123`

Ban co the doi tai khoan trong file [appsettings.json](/E:/AutoCarShowroom/appsettings.json).

## Huong dan chay

1. Kiem tra lai chuoi ket noi SQL Server trong [appsettings.json](/E:/AutoCarShowroom/appsettings.json).
2. Chay `dotnet build`.
3. Chay `dotnet run`.
4. Mo `https://localhost:7232` hoac `http://localhost:5090`.

## Noi dung co the trinh bay khi bao cao

- Kien truc MVC gom `Models`, `Views`, `Controllers`.
- Entity Framework Core duoc dung de thao tac va migration database.
- Khu admin duoc bao ve boi cookie authentication.
- Hinh anh xe duoc upload va luu trong `wwwroot/uploads/cars`.
- He thong co du lieu mau san de demo ngay sau khi chay.
