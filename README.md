# AutoCarShowroom

Đồ án môn học xây dựng website showroom ô tô bằng ASP.NET Core MVC và Entity Framework Core.

## Chức năng hiện có

- Xem danh sách xe công khai, tìm kiếm theo tên, lọc theo hãng, năm và khoảng giá.
- Xem chi tiết từng mẫu xe trong showroom.
- Đăng nhập quản trị bằng cookie authentication để truy cập chức năng quản lý.
- Thêm, sửa, xóa xe trong hệ thống.
- Upload ảnh xe thật vào `wwwroot/uploads/cars`.
- Tự động seed danh mục lớn gồm xe phổ thông và siêu xe để demo showroom.
- Đi kèm sẵn file database SQLite trong `App_Data/AutoCarShowroom.db`.

## Tài khoản admin mặc định

- Username: `admin`
- Password: `Admin@123`

Bạn có thể đổi tài khoản trong file [appsettings.json](/C:/Users/BEKING/source/repos/AutoCarShowroom/appsettings.json).

## Hướng dẫn chạy

1. Chạy `dotnet restore`.
2. Chạy `dotnet build`.
3. Chạy `dotnet run --launch-profile http`.
4. Mở `http://localhost:5090`.

Nếu muốn chạy HTTPS, dùng profile `https` trong `launchSettings.json`.

## Ghi chú database

- Project hiện dùng SQLite file, không còn phụ thuộc `.\SQLEXPRESS`.
- File database mẫu nằm ở `App_Data/AutoCarShowroom.db`.
- Khi khởi động, ứng dụng sẽ tự tạo thư mục dữ liệu nếu chưa có và tự seed lại danh mục xe mẫu.

## Nội dung có thể trình bày khi báo cáo

- Kiến trúc MVC gồm `Models`, `Views`, `Controllers`.
- Entity Framework Core dùng để thao tác dữ liệu.
- Khu admin được bảo vệ bởi cookie authentication.
- Hình ảnh xe được upload và lưu trong `wwwroot/uploads/cars`.
- Hệ thống có dữ liệu mẫu sẵn để demo ngay sau khi chạy.
