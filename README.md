# LocalMart Online - Nền tảng Số hóa & Kết nối Chợ Truyền thống Việt Nam

> **Capstone Project (SEP490)** — Đại học FPT Cần Thơ  
> **Giảng viên hướng dẫn:** ThS. Phạm Tiến Phúc  

---

## 1. Vì sao có LocalMart Online? (Problem & Solution)

### 📌 Bài toán thực tế
Tại Việt Nam, dù siêu thị và các ứng dụng đi chợ trực tuyến phát triển mạnh mẽ, **chợ truyền thống vẫn cung cấp tới hơn 60% thực phẩm tươi sống** cho các gia đình đô thị. Tuy nhiên, nhịp sống bận rộn và rào cản địa lý khiến cư dân đô thị ngày càng khó duy trì thói quen đi chợ hàng ngày. 

Ở chiều ngược lại, tiểu thương tại các chợ truyền thống đa phần là các hộ kinh doanh nhỏ lẻ, hạn chế về năng lực công nghệ và thiếu công cụ để tiếp cận khách hàng trên không gian số. Ngoài ra, các nền tảng thương mại điện tử lớn hiện nay (Shopee, GrabMart...) chủ yếu thiết kế cho siêu thị/bán lẻ lớn, hoàn toàn bỏ ngỏ các nét văn hóa đặc trưng của chợ truyền thống như: **thương lượng giá (trả giá), kiểm tra độ tươi ngon tại chỗ, và thói quen đi chợ hộ**.

### 💡 Giải pháp LocalMart Online
**LocalMart Online** ra đời như một hệ sinh thái thương mại số đa nền tảng nhằm:
*   **Số hóa tiểu thương chợ truyền thống**: Giúp tiểu thương tạo gian hàng số, đăng tải sản phẩm kèm ảnh xác thực độ tươi (timestamp & watermark).
*   **Gắn kết cư dân đô thị & Chợ truyền thống**: Cho phép người mua đặt hàng trước, hẹn giờ lấy hoặc sử dụng dịch vụ đi chợ hộ (Proxy Shopping).
*   **Bảo tồn văn hóa đi chợ**: Đưa tính năng thương lượng giá (Fast Bargain) và thanh toán linh hoạt (COD / VNPay) vào môi trường số một cách minh bạch, có kiểm soát.

---

## 2. Demo & Giao diện hệ thống (Screenshots)

Hệ thống bao gồm ứng dụng **Web Management Dashboard** (dành cho Admin, Ban quản lý chợ, Tiểu thương) và **Mobile Application** (dành cho Người mua & Người đi chợ hộ).

| Web Admin & Dashboard | Mobile App (Buyer & Proxy Shopper) |
| :---: | :---: |
| ![Web Dashboard](docs/images/web_dashboard.png) | ![Mobile App](docs/images/mobile_app.png) |
| *Giao diện quản lý chợ, duyệt tiểu thương & thống kê doanh thu* | *Giao diện tìm kiếm sản phẩm tươi, trả giá & tạo đơn đi chợ hộ* |

*(Lưu ý: Thay thế các đường dẫn trong `docs/images/` bằng hình ảnh chụp thực tế của dự án)*

---

## 3. Công nghệ sử dụng (Tech Stack)

### Backend
*   **Framework**: .NET 8 (C#) — RESTful Web APIs high performance.
*   **Realtime Communication**: ASP.NET Core SignalR (Trò chuyện thời gian thực & Cập nhật trạng thái đơn hàng).
*   **Authentication & Security**: JWT (JSON Web Token), Role-Based Access Control (RBAC).

### Frontend & Mobile
*   **Web Management**: ReactJS, TailwindCSS, Axios, Chart.js / Recharts.
*   **Mobile App**: React Native (Cross-platform Android & iOS).

### Cơ sở dữ liệu & Lưu trữ
*   **Database**: MongoDB (NoSQL) — Linh hoạt lưu trữ cấu hình chợ, thông tin gian hàng linh hoạt và sản phẩm tươi sống.
*   **Cloud Storage**: Cloudinary (Lưu trữ ảnh & tự động đóng dấu Watermark / Timestamp chứng thực tươi mới).

### Dịch vụ tích hợp (Third-party Integrations)
*   **AI Service**: Python (Recommendation System hỗ trợ gợi ý sản phẩm phù hợp thói quen mua sắm).
*   **Payment Gateway**: VNPay Sandbox API (Thanh toán phí dịch vụ & đơn hàng điện tử).
*   **Push Notifications**: Firebase Cloud Messaging (FCM).
*   **Geolocation**: Google Maps API (Định vị vị trí chợ & tìm kiếm tiểu thương gần nhất).

---

## 4. Kiến trúc hệ thống (Architecture)

Hệ thống được thiết kế theo kiến trúc **Modular Layered Architecture (Clean/N-Tier Architecture)** đảm bảo tính mở rộng, bảo trì và dễ dàng kiểm thử:

```
[ Clients: ReactJS Web / React Native Mobile ]
                       │ (HTTP REST / WebSocket SignalR)
                       ▼
┌────────────────────────────────────────────────────────┐
│               .NET 8 Web API Gateway                   │
├────────────────────────────────────────────────────────┤
│ Controllers       - Handling Requests & Responses     │
│ Services          - Business Logic & Domain Rules      │
│ Repositories      - Generic Repository Pattern        │
│ Models & DTOs     - MongoDB Entities & Data Transfer   │
└──────────────────────┬─────────────────────────────────┘
                       │
         ┌─────────────┼──────────────┐
         ▼             ▼              ▼
   [ MongoDB ]   [ Cloudinary ]   [ VNPay / FCM ]
```

### Cấu trúc thư mục Source Code Backend:
- `Controllers/`: Chứa các API endpoint tiếp nhận request.
- `Services/`: Chứa logic nghiệp vụ (`Services.Interface` và `Services.Implement`).
- `Repositories/`: Tầng giao tiếp dữ liệu generic với MongoDB (`IRepository`, `Repository`).
- `Models/`: Các Entity ánh xạ trực tiếp vào Mongo Collections (User, Market, Store, Product, Order, ProxyRequest...).
- `DTOs/`: Các đối tượng truyền nhận dữ liệu chuẩn hóa giữa Client - Server.
- `Hubs/`: SignalR Hubs phục vụ thông báo và Chat realtime.

---

## 5. Tính năng chính (Key Features)

### 👥 1. Phân quyền đa vai trò (Multi-Role RBAC)
Hệ thống hỗ trợ 7 nhóm vai trò riêng biệt: Admin, Ban quản lý chợ (Market Management Board), Nhân viên chợ (Market Staff), Đại diện chính quyền, Tiểu thương (Seller), Người mua (Buyer), Người đi chợ hộ (Proxy Shopper).

### 🏪 2. Quản lý Chợ & Gian hàng (Market & Vendor Operations)
- Tạo mới, cập nhật và quản lý không gian các chợ truyền thống trên bản đồ số.
- Phê duyệt tiểu thương đăng ký mở sạp, quản lý thu phí chợ và giám sát tuân thủ nội quy.

### 🍏 3. Quản lý Sản phẩm tươi sống & Đơn hàng
- Đăng tải danh mục hàng tươi sống kèm ảnh chụp trực tiếp có watermark ngày/giờ.
- Quản lý giỏ hàng, đặt hàng trước (Pre-order), theo dõi tiến trình đơn hàng theo thời gian thực.

### 🛵 4. Dịch vụ Đi chợ hộ (Proxy Shopping)
- Người mua tạo yêu cầu đi chợ hộ kèm ghi chú riêng (ví dụ: *"chọn cá lóc tươi còn bơi"*).
- Người đi chợ hộ nhận đơn, chụp ảnh xác thực tại sạp và giao hàng tận nơi.

### 💬 5. Thương lượng giá nhanh (Fast Bargain)
- Số hóa nét văn hóa "trả giá" thông qua quy trình đàm phán giá tự động/bán tự động giữa Người mua và Tiểu thương trong hạn mức giảm giá cho phép.

### 📊 6. Thống kê & Báo cáo
- Dashboard phân tích doanh thu, sản lượng bán ra, số lượng đơn hàng theo ngày/tần suất cho Tiểu thương và Ban quản lý.

---

## 6. Giao diện API (API Documentation)

Hệ thống cung cấp tài liệu API chuẩn **Swagger UI**:

- **URL Swagger**: `http://localhost:8080/swagger` (hoặc `http://localhost:5183/swagger` khi chạy local).

### Các nhóm API chính:
*   `/api/Auth` — Đăng ký, Đăng nhập, Xóa/Khôi phục tài khoản, JWT Authentication.
*   `/api/Market` — Quản lý thông tin chợ truyền thống & vị trí.
*   `/api/Store` — Quản lý sạp hàng & thông tin tiểu thương.
*   `/api/Product` — CRUD sản phẩm, đăng tải hình ảnh chứng thực.
*   `/api/Order` — Tạo đơn hàng, cập nhật trạng thái giao/nhận.
*   `/api/ProxyShopper` — Nhận đơn đi chợ hộ & cập nhật minh chứng.
*   `/api/FastBargain` — Gửi đề xuất giá & phản hồi trả giá.
*   `/api/VnPay` — Tạo URL thanh toán & nhận Callback kết quả.
*   `/api/Report` — Báo cáo thống kê & báo cáo vi phạm.

---

## 7. Hướng dẫn chạy thử (Quick Start)

Yêu cầu môi trường: **Git**, **Docker** và **Docker Compose** đã được cài đặt trên máy.

### Bước 1: Clone kho mã nguồn
```bash
git clone https://github.com/Williams552/LocalMartOnline.git
cd LocalMartOnline
```

### Bước 2: Khởi tạo file cấu hình
Sao chép file cấu hình mẫu `appsettings.Example.json` thành `appsettings.json`:
```bash
# Trên Linux / macOS / Git Bash:
cp appsettings.Example.json appsettings.json

# Trên Windows PowerShell:
copy appsettings.Example.json appsettings.json
```
*(Bạn có thể chỉnh sửa lại các thông tin chuỗi kết nối MongoDB, Key JWT, VNPay trong `appsettings.json` nếu cần)*

### Bước 3: Chạy ứng dụng bằng Docker Compose
```bash
docker compose up -d --build
```

Sau khi các container khởi chạy thành công:
- **Backend API & Swagger**: Truy cập tại `http://localhost:8080/swagger`
- **MongoDB**: Chạy tại port `27017`

---

## 8. Đội ngũ phát triển & Vai trò (Team & Roles)

Dự án được thực hiện bởi nhóm sinh viên Kỹ thuật Phần mềm — Đại học FPT Cần Thơ (Nhóm `SEP490_22`):

| Họ và tên | Mã SV | Vai trò | Trách nhiệm chính |
| :--- | :---: | :---: | :--- |
| **Nguyễn Tiến Đạt** | CE171314 | **Leader / System Architect** | Quản lý dự án, thiết kế kiến trúc hệ thống, phát triển Backend API & tích hợp thanh toán. |
| **Huỳnh Gia Bảo** | CE171767 | **Fullstack Developer** | Phát triển các tính năng Web Admin, quản lý chợ & báo cáo thống kê. |
| **Nguyễn Vinh Khang** | CE170767 | **Frontend & Mobile Developer** | Thiết kế UI/UX, phát triển ứng dụng di động React Native cho Buyer & Proxy Shopper. |
| **Nguyễn Vi Khang** | CE171639 | **Backend & Database Engineer** | Thiết kế cơ sở dữ liệu MongoDB, xây dựng API sản phẩm, đơn hàng & SignalR Realtime. |
| **Trần Nguyễn Vi Nhân** | CE171483 | **QA & Documentation Specialist** | Kiểm thử phần mềm (Test Cases, UAT), xây dựng tài liệu SRS/SDS & hỗ trợ quy trình. |

*Chân thành cảm ơn **ThS. Phạm Tiến Phúc** đã tận tình hướng dẫn nhóm trong suốt quá trình thực hiện đồ án!*

---

## 9. Trạng thái dự án & Giới hạn hiện tại (Project Status & Limitations)

Để đảm bảo tính minh bạch và cung cấp thông tin chính xác nhất đến người đánh giá/nhà tuyển dụng:

*   🎓 **Bản chất dự án**: Đây là **Đồ án tốt nghiệp đại học (Capstone Project)** được xây dựng nhằm nghiên cứu giải pháp công nghệ cho bài toán thực tế của chợ truyền thống Việt Nam.
*   🧪 **Dữ liệu mô phỏng (Mock Data)**: Toàn bộ dữ liệu về sản phẩm, gian hàng, chợ và người dùng trên hệ thống hiện tại phục vụ mục đích kiểm thử và demo nghiệm thu, chưa phải dữ liệu kinh doanh thực tế.
*   ☁️ **Môi trường triển khai**: Dự án đã hoàn thành giai đoạn thử nghiệm tại môi trường Local / Staging. Chưa triển khai đưa vào vận hành thực tế thương mại (Production).
*   💳 **Quy trình thanh toán đơn hàng**: Hệ thống đã tích hợp thành công cổng thanh toán **VNPay (Môi trường Sandbox)** cho luồng thử nghiệm trực tuyến. Tuy nhiên, luồng giao dịch sản phẩm chính vẫn ưu tiên quy trình **COD (Thanh toán khi nhận hàng)** để phù hợp nhất với tập quán mua sắm thực phẩm tươi sống tại các chợ truyền thống.

---
© 2025 LocalMart Online Team — FPT University Can Tho Campus. All rights reserved.