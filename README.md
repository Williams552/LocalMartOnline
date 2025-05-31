# LocalMartOnline - Hướng dẫn cấu trúc dự án

## 1. Cấu trúc thư mục chính

- **Controllers/**: Chứa các controller xử lý request từ client (API endpoint)
  - `UserController.cs`: Xử lý các API liên quan đến người dùng (User)
  - `MongoDBController.cs`: Kiểm tra kết nối và truy vấn dữ liệu MongoDB
  - `WeatherForecastController.cs`: Controller mẫu (có thể xóa nếu không dùng)
- **Models/**: Chứa các class mô hình dữ liệu (model)
  - `User.cs`: Định nghĩa cấu trúc dữ liệu người dùng, ánh xạ với collection `users` trong MongoDB
  - `MongoDBSettings.cs`: Class cấu hình kết nối MongoDB (nếu dùng kiểu binding config)
- **Repositories/**: Chứa các class thao tác dữ liệu (repository)
  - `IGenericRepository.cs`: Interface generic cho các thao tác CRUD
  - `GenericRepository.cs`: Cài đặt repository generic cho MongoDB
- **Services/**: Chứa các service dùng chung
  - `MongoDBService.cs`: Service quản lý kết nối và truy vấn collection MongoDB
- **appsettings.json**: File cấu hình ứng dụng (connection string, logging, ...)
- **Program.cs**: File khởi tạo ứng dụng, khai báo các service, middleware, DI container

## 2. Kết nối MongoDB Atlas
- Thông tin kết nối được cấu hình trong `appsettings.json`:
  ```json
  "MongoDB": {
    "ConnectionString": "mongodb+srv://<user>:<pass>@<cluster>.mongodb.net/?retryWrites=true&w=majority&appName=William",
    "DatabaseName": "LocalMartOnline"
  }
  ```
- Đảm bảo user, password, cluster, database đúng và IP đã được whitelist trên MongoDB Atlas.

## 3. Khai báo service trong Program.cs
- Đăng ký các service cần thiết cho DI:
  ```csharp
  builder.Services.AddSingleton<IMongoClient>(...);
  builder.Services.AddSingleton<IMongoDatabase>(...);
  builder.Services.AddScoped<MongoDBService>();
  builder.Services.AddScoped<IGenericRepository<User>>(...);
  ```
- Các service này sẽ được inject vào controller hoặc repository khi cần sử dụng.

## 4. Controller
- **UserController**: Xử lý các API liên quan đến người dùng, sử dụng repository để thao tác dữ liệu.
- **MongoDBController**: Có endpoint `/api/mongodb/test-connection` để kiểm tra kết nối MongoDB, endpoint `/api/mongodb` để lấy dữ liệu mẫu từ collection `users`.
- **WeatherForecastController**: Controller mẫu, có thể dùng để test hoặc tham khảo.

## 5. Model
- **User.cs**: Định nghĩa đầy đủ các trường của user, ánh xạ với dữ liệu thực tế trong MongoDB.
- **MongoDBSettings.cs**: (Nếu dùng) để binding cấu hình từ appsettings vào class C#.

## 6. Repository
- **IGenericRepository**: Định nghĩa các phương thức CRUD chung cho mọi entity.
- **GenericRepository**: Cài đặt thao tác CRUD cho MongoDB, sử dụng generic để tái sử dụng cho nhiều model.

## 7. Service
- **MongoDBService**: Quản lý kết nối và truy vấn collection MongoDB, giúp controller/repository lấy collection dễ dàng.

## 8. Hướng dẫn quy trình làm việc với Git cho team

### A. Các nhánh chính
- **main**: Làm xong mới merge vào nên giờ là cứ để đây
- **develop**: Chứa code mới nhất đã tích hợp từ các nhánh tính năng, là nơi test tổng hợp.

### B. Nhánh cho từng tính năng/bugfix
Mỗi thành viên khi làm một tính năng/bugfix sẽ tạo nhánh riêng từ develop, ví dụ:
- `username/ten-tinh-nang`
- `dat/user-authentication`

### C. Quy trình làm việc chuẩn
1. Clone repo về máy, checkout nhánh develop:
```bash
git clone <repository-url>
git checkout develop
```

2. Tạo nhánh mới cho tính năng/bugfix:
```bash
git checkout -b username/ten-tinh-nang
```

3. Làm việc, commit, push lên GitHub:
```bash
git add .
git commit -m "Mô tả rõ ràng về thay đổi"
git push origin feature/ten-tinh-nang
```

4. Tạo Pull Request (PR) từ nhánh feature vào develop.
5. Review code, test, rồi merge vào develop.
6. Khi chuẩn bị release, leader sẽ merge develop vào main.

### D. Lưu ý khi làm việc nhóm
- Không commit trực tiếp lên main hoặc develop.
- Luôn tạo PR để review code.
- Thường xuyên pull develop về để tránh xung đột.
- Đặt tên nhánh rõ ràng, có ý nghĩa.
- Ghi chú rõ ràng trong commit message.

## 9. Tài khoản test hệ thống

| Username   | Password       | Role   |
|------------|----------------|--------|
| buyer01    | 123456         | Buyer  |
| admin01    | 123456         | Admin  |

> Có thể đăng ký thêm tài khoản test mới qua endpoint `/api/Auth/register` (role mặc định là Buyer).

---
**Mọi thắc mắc về cấu trúc hoặc cách mở rộng dự án, hãy đọc README này hoặc liên hệ leader dự án!**

