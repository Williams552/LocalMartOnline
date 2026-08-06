# LocalMart Online

Đồ án tốt nghiệp đại học FPT Cần Thơ (SEP490_22).  
Giảng viên hướng dẫn: ThS. Phạm Tiến Phúc.

---

## 1. Lý do phát triển

Ở các đô thị Việt Nam, hơn 60% gia đình vẫn mua thực phẩm tươi sống tại chợ truyền thống. Dù vậy, dân văn phòng hay người bận rộn lại rất khó đi chợ hàng ngày vì lệch giờ họp, ngại kẹt xe hoặc không có thời gian chọn đồ. 

Các ứng dụng như ShopeeFood hay GrabMart tập trung vào chuỗi siêu thị và cửa hàng tiện lợi. Họ không thiết kế cho chợ truyền thống; nơi tiểu thương bán hàng không có máy POS, đồ ăn ngon phải nhìn tận mắt và giá cả vẫn dựa vào chuyện trả giá trực tiếp.

LocalMart Online giải quyết góc khuất này. Hệ thống cho phép tiểu thương đưa sạp hàng lên mạng với hình ảnh chụp thực tế có đính timestamp. Người mua có thể chọn đồ, hẹn giờ ghé lấy hoặc thuê người đi chợ hộ (Proxy Shopper) nhặt hàng giúp. Tính năng trả giá (Fast Bargain) cũng được giữ lại để không làm mất thói quen mua bán quen thuộc.

---

## 2. Demo & Giao diện

Hệ thống chia làm hai phần chính: Web Dashboard (ReactJS) cho Admin, Ban quản lý chợ, Tiểu thương và App Mobile (React Native) cho Người mua, Người đi chợ hộ.

| Web Admin / Merchant | Mobile App (Buyer & Proxy) |
| :--- | :--- |
| ![Web Dashboard](docs/images/web_dashboard.png) | ![Mobile App](docs/images/mobile_app.png) |
| *Quản lý sạp, duyệt tiểu thương, xem thống kê* | *Tìm gian hàng, trả giá và tạo đơn đi chợ hộ* |

*(Ảnh chụp màn hình lưu tại thư mục `docs/images/`)*

---

## 3. Công nghệ sử dụng

- **Backend**: C# .NET 8 Web API, SignalR (trò chuyện và cập nhật trạng thái đơn realtime).
- **Web Admin**: ReactJS, TailwindCSS.
- **Mobile**: React Native (chạy cả Android và iOS).
- **Database**: MongoDB (lưu thông tin chợ, sạp hàng, lịch sử trả giá và nhật ký hoạt động).
- **Lưu trữ ảnh**: Cloudinary (tự động gắn watermark và ngày giờ lên ảnh sản phẩm).
- **Dịch vụ phụ trợ**: Python (gợi ý sản phẩm bằng LightFM), Firebase Cloud Messaging (thông báo), VNPay Sandbox, Google Maps API.
- **Container**: Docker & Docker Compose.

---

## 4. Kiến trúc hệ thống

Ứng dụng chia tầng rõ ràng theo mô hình Clean / Layered Architecture:

```
Client (ReactJS / React Native)
   │
   ▼ (REST API / SignalR)
.NET 8 Backend
 ├── Controllers       : Tiếp nhận endpoint
 ├── Services          : Thao tác logic nghiệp vụ
 ├── Repositories      : Xử lý dữ liệu MongoDB (Generic Repository)
 └── Models & DTOs     : Chứa cấu hình entity và DTO
   │
   ├─► MongoDB
   ├─► Cloudinary
   └─► VNPay / Firebase
```

---

## 5. Tính năng chính

- **Phân quyền 7 vai trò**: Admin, Ban quản lý chợ, Nhân viên chợ, Đại diện chính quyền, Tiểu thương, Người mua, Người đi chợ hộ.
- **Quản lý chợ & sạp hàng**: Khởi tạo vị trí chợ, xét duyệt tiểu thương mở gian và thu phí dịch vụ.
- **Sản phẩm & Đơn hàng**: Đăng bán đồ tươi kèm ảnh timestamp chứng thực, đặt trước hoặc giao hàng.
- **Dịch vụ đi chợ hộ**: Người mua gửi yêu cầu chi tiết (chọn rau tươi, cá sống), người đi chợ hộ nhận đơn, chụp ảnh tại sạp để xác nhận rồi giao.
- **Trả giá (Fast Bargain)**: Cho phép người mua đàm phán giá nhanh với người bán trong khoảng giảm giá cho phép.
- **Thống kê**: Báo cáo doanh thu, lượt bán và cảnh báo vi phạm.

---

## 6. API

Tài liệu API tự động phát sinh qua Swagger UI khi khởi chạy Backend.

- **Đường dẫn Swagger**: `http://localhost:8080/swagger`

Các nhóm API chính trong hệ thống:
- `/api/Auth` và `/api/User`: Quản lý tài khoản, phân quyền.
- `/api/Market` và `/api/Store`: Quản lý chợ, sạp hàng.
- `/api/Product`: Đăng và quản lý mặt hàng.
- `/api/Order` và `/api/ProxyShopper`: Tạo đơn, xử lý giao hàng hộ.
- `/api/FastBargain`: Đề xuất và nhận phản hồi trả giá.
- `/api/VnPay`: Cổng thanh toán thử nghiệm.

---

## 7. Chạy thử

Yêu cầu máy đã cài Docker và Docker Compose.

1. Clone mã nguồn:
```bash
git clone https://github.com/Williams552/LocalMartOnline.git
cd LocalMartOnline
```

2. Tạo file cấu hình từ file mẫu:
```bash
# Windows PowerShell
copy appsettings.Example.json appsettings.json

# Linux / macOS
cp appsettings.Example.json appsettings.json
```

3. Khởi chạy bằng Docker Compose:
```bash
docker compose up -d --build
```

Mở trình duyệt truy cập `http://localhost:8080/swagger` để kiểm tra các endpoint.

---

## 8. Đội ngũ & Vai trò

Nhóm sinh viên thực hiện (SEP490_22 - Đại học FPT Cần Thơ):

- **Nguyễn Tiến Đạt (CE171314)** - Leader: Thiết kế kiến trúc hệ thống, phát triển Backend API và tích hợp thanh toán.
- **Huỳnh Gia Bảo (CE171767)**: Phát triển Web Admin, giao diện quản lý chợ và báo cáo.
- **Nguyễn Vinh Khang (CE170767)**: Lập trình ứng dụng di động React Native cho Người mua và Người đi chợ hộ.
- **Nguyễn Vi Khang (CE171639)**: Thiết kế MongoDB, viết API sản phẩm, đơn hàng và xử lý SignalR.
- **Trần Nguyễn Vi Nhân (CE171483)**: Kiểm thử hệ thống (Test cases, UAT), viết tài liệu SRS/SDS.

GVHD: **ThS. Phạm Tiến Phúc**

---

## 9. Trạng thái & Giới hạn

Để người đọc và nhà tuyển dụng đánh giá đúng thực tế dự án:

1. **Đồ án tốt nghiệp**: Đây là sản phẩm nghiên cứu phục vụ bảo vệ đồ án tốt nghiệp đại học, không phải ứng dụng thương mại đang hoạt động độc lập trên thị trường.
2. **Dữ liệu mô phỏng**: Toàn bộ dữ liệu gian hàng, chợ và đơn hàng đều là dữ liệu giả lập (mock data) phục vụ mục đích kiểm thử.
3. **Chưa deploy Production**: Ứng dụng hiện chỉ chạy ở môi trường Local và Staging thử nghiệm.
4. **Chưa tích hợp thanh toán thực cho đơn hàng**: Hệ thống có kết nối VNPay Sandbox để thử nghiệm thanh toán điện tử, nhưng luồng mua bán nông sản thực tế vẫn ưu tiên COD (trả tiền mặt khi nhận hàng sau khi kiểm tra độ tươi).