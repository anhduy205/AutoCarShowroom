# AutoCarShowroom

Do an mon hoc xay dung website showroom o to bang ASP.NET Core MVC va Entity Framework Core.

## Chuc nang hien co

- Xem danh sach xe cong khai, tim kiem theo ten, loc theo hang, nam va khoang gia.
- Xem chi tiet tung mau xe voi thong tin chung, dong co, ngoai that, noi that, ghe, tien nghi va cac nhom an toan.
- Dang nhap quan tri bang cookie authentication de truy cap chuc nang quan ly.
- Them, sua, xoa xe trong he thong.
- Upload anh xe that vao `wwwroot/uploads/cars`.
- Tu dong seed danh muc lon gom xe pho thong va sieu xe de demo showroom.
- Anh seed that duoc luu trong `wwwroot/images/catalog`.
- Project duoc cau hinh de chay voi SQL Server `.\SQLEXPRESS`.

## Tai khoan admin mac dinh

- Username: `admin`
- Password: `Admin@123`

Ban co the doi tai khoan trong [appsettings.json](/C:/Users/BEKING/source/repos/AutoCarShowroom/appsettings.json).

## Huong dan chay

1. Dam bao may da co SQL Server instance `.\SQLEXPRESS`.
2. Chay `dotnet restore`.
3. Chay `dotnet build`.
4. Chay `Update-Database` hoac `dotnet run --launch-profile http`.
5. Mo `http://localhost:5090`.

Ung dung se tu `Migrate()` va seed lai du lieu neu database chua co danh muc xe.

## Ghi chu database va hinh anh

- Project hien dung SQL Server qua chuoi ket noi trong [appsettings.json](/C:/Users/BEKING/source/repos/AutoCarShowroom/appsettings.json).
- Du lieu xe duoc seed vao database `AutoCarShowroomDb`.
- Anh that cua cac mau xe nam trong [wwwroot/images/catalog](/C:/Users/BEKING/source/repos/AutoCarShowroom/wwwroot/images/catalog).
- Thu muc `wwwroot/images` chi dung de luu file tinh nhu anh; du lieu bang xe van nam trong SQL Server thong qua migration va seeder.

## Noi dung co the trinh bay khi bao cao

- Kien truc MVC gom `Models`, `Views`, `Controllers`.
- Entity Framework Core dung de thao tac du lieu voi SQL Server.
- Khu admin duoc bao ve boi cookie authentication.
- Hinh anh xe duoc luu trong `wwwroot/images/catalog` va `wwwroot/uploads/cars`.
- He thong co du lieu mau san de demo ngay sau khi chay.
