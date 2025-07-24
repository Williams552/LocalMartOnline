# Tài liệu kỹ thuật - Get Notification APIs

## Tổng quan
Hệ thống notification trong LocalMartOnline cung cấp các API để lấy danh sách thông báo và đếm số thông báo chưa đọc của người dùng. Hệ thống sử dụng MongoDB để lưu trữ và JWT token để xác thực.

## Kiến trúc hệ thống

### 1. Models
```csharp
// Models/Notification.cs
public class Notification
{
    public string Id { get; set; }           // ObjectId của MongoDB
    public string UserId { get; set; }       // ID của người dùng nhận thông báo
    public string Title { get; set; }        // Tiêu đề thông báo
    public string Message { get; set; }      // Nội dung thông báo
    public string Type { get; set; }         // Loại thông báo (order, system, etc.)
    public bool IsRead { get; set; }         // Trạng thái đã đọc
    public DateTime CreatedAt { get; set; }  // Thời gian tạo
}

// Models/DTOs/NotificationDto.cs
public class NotificationDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public bool IsRead { get; set; }
    public string CreatedAt { get; set; }    // Format: "yyyy-MM-dd HH:mm:ss"
}
```

### 2. Service Interface
```csharp
public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(string userId);
    Task<(IEnumerable<NotificationDto> Notifications, int Total)> GetNotificationsPagedAsync(string userId, int page, int limit);
    Task<int> GetUnreadCountAsync(string userId);
}
```

## API Endpoints

### 1. GET /api/notification - Lấy danh sách thông báo có phân trang

#### Mô tả
Lấy danh sách thông báo của người dùng hiện tại với hỗ trợ phân trang.

#### Authentication
- **Required**: Bearer JWT token
- **Roles**: Tất cả các role đã đăng nhập

#### Request Parameters
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | int | No | 1 | Số trang (bắt đầu từ 1) |
| limit | int | No | 5 | Số lượng thông báo mỗi trang |

#### Request Example
```http
GET /api/notification?page=1&limit=10
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response Success (200 OK)
```json
{
    "success": true,
    "data": [
        {
            "id": "507f1f77bcf86cd799439011",
            "userId": "507f1f77bcf86cd799439012",
            "title": "Đơn hàng mới",
            "message": "Bạn có đơn hàng mới từ cửa hàng ABC",
            "type": "order",
            "isRead": false,
            "createdAt": "2024-01-15 10:30:00"
        },
        {
            "id": "507f1f77bcf86cd799439013",
            "userId": "507f1f77bcf86cd799439012",
            "title": "Khuyến mãi đặc biệt",
            "message": "Giảm giá 20% cho tất cả sản phẩm",
            "type": "promotion",
            "isRead": true,
            "createdAt": "2024-01-14 15:45:00"
        }
    ],
    "total": 25
}
```

#### Response Error (401 Unauthorized)
```json
{
    "success": false,
    "message": "Không xác định được user."
}
```

#### Business Logic
1. Lấy userId từ JWT token trong request header
2. Validate userId không rỗng
3. Truy vấn MongoDB với filter `UserId == userId`
4. Sắp xếp theo `CreatedAt` giảm dần (mới nhất trước)
5. Áp dụng phân trang với `Skip((page-1) * limit).Take(limit)`
6. Convert entity sang DTO với format ngày tháng
7. Trả về danh sách và tổng số bản ghi

#### Performance Considerations
- **Index**: Cần tạo index trên `UserId` và `CreatedAt` để tối ưu query
- **Caching**: Có thể cache kết quả trong Redis với TTL ngắn
- **Pagination**: Giới hạn `limit` tối đa để tránh query quá lớn

---

### 2. GET /api/notification/unread-count - Đếm số thông báo chưa đọc

#### Mô tả
Lấy số lượng thông báo chưa đọc của người dùng hiện tại.

#### Authentication
- **Required**: Bearer JWT token
- **Roles**: Tất cả các role đã đăng nhập

#### Request Example
```http
GET /api/notification/unread-count
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response Success (200 OK)
```json
{
    "success": true,
    "data": 3
}
```

#### Response Error (401 Unauthorized)
```json
{
    "success": false,
    "message": "Không xác định được user."
}
```

#### Business Logic
1. Lấy userId từ JWT token
2. Validate userId không rỗng
3. Truy vấn MongoDB với filter `UserId == userId && IsRead == false`
4. Đếm số lượng bản ghi phù hợp
5. Trả về số đếm

#### Performance Considerations
- **Index**: Cần tạo compound index trên `(UserId, IsRead)`
- **Caching**: Cache trong Redis với TTL ngắn, invalidate khi có thông báo mới
- **Real-time**: Có thể tích hợp SignalR để push real-time updates

## Database Schema

### MongoDB Collection: notifications
```javascript
{
    "_id": ObjectId("507f1f77bcf86cd799439011"),
    "UserId": "507f1f77bcf86cd799439012",
    "Title": "Đơn hàng mới",
    "Message": "Bạn có đơn hàng mới từ cửa hàng ABC",
    "Type": "order",
    "IsRead": false,
    "CreatedAt": ISODate("2024-01-15T10:30:00.000Z")
}
```

### Recommended Indexes
```javascript
// Index chính cho query thông báo theo user
db.notifications.createIndex({ "UserId": 1, "CreatedAt": -1 })

// Index cho đếm thông báo chưa đọc
db.notifications.createIndex({ "UserId": 1, "IsRead": 1 })

// Index cho query theo type nếu cần
db.notifications.createIndex({ "UserId": 1, "Type": 1, "CreatedAt": -1 })
```

## Error Handling

### Common Error Codes
| Status Code | Error Type | Description |
|-------------|------------|-------------|
| 200 | Success | Request thành công |
| 401 | Unauthorized | JWT token không hợp lệ hoặc thiếu |
| 401 | Unauthorized | Không xác định được user từ token |
| 500 | Internal Server Error | Lỗi server nội bộ |

### Error Response Format
```json
{
    "success": false,
    "message": "Mô tả lỗi chi tiết",
    "data": null
}
```

## Security Considerations

### Authentication & Authorization
- Tất cả endpoints đều yêu cầu JWT token hợp lệ
- Mỗi user chỉ có thể truy cập thông báo của chính mình
- UserId được lấy từ JWT claims, không từ request parameters

### Data Privacy
- Không expose thông tin user khác trong response
- Filter chặt chẽ theo UserId từ JWT token
- Log access cần tuân thủ GDPR/privacy regulations

## Integration Examples

### Frontend JavaScript
```javascript
// Lấy danh sách thông báo
async function getNotifications(page = 1, limit = 10) {
    try {
        const response = await fetch(`/api/notification?page=${page}&limit=${limit}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to fetch notifications');
        }
        
        const result = await response.json();
        return result;
    } catch (error) {
        console.error('Error fetching notifications:', error);
        throw error;
    }
}

// Lấy số thông báo chưa đọc
async function getUnreadCount() {
    try {
        const response = await fetch('/api/notification/unread-count', {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            }
        });
        
        const result = await response.json();
        return result.data;
    } catch (error) {
        console.error('Error fetching unread count:', error);
        return 0;
    }
}
```

### Mobile App (React Native/Flutter)
```dart
// Flutter example
Future<NotificationResponse> getNotifications({int page = 1, int limit = 10}) async {
    final token = await TokenStorage.getToken();
    
    final response = await http.get(
        Uri.parse('$baseUrl/api/notification?page=$page&limit=$limit'),
        headers: {
            'Authorization': 'Bearer $token',
            'Content-Type': 'application/json',
        },
    );
    
    if (response.statusCode == 200) {
        return NotificationResponse.fromJson(json.decode(response.body));
    } else {
        throw Exception('Failed to load notifications');
    }
}
```

## Testing

### Unit Tests
```csharp
[Test]
public async Task GetNotificationsPaged_ValidUser_ReturnsNotifications()
{
    // Arrange
    var userId = "507f1f77bcf86cd799439012";
    var mockNotifications = CreateMockNotifications(userId);
    _mockNotificationRepo.Setup(r => r.FindManyAsync(It.IsAny<Expression<Func<Notification, bool>>>()))
                        .ReturnsAsync(mockNotifications);
    
    // Act
    var result = await _notificationService.GetNotificationsPagedAsync(userId, 1, 5);
    
    // Assert
    Assert.That(result.Notifications.Count(), Is.EqualTo(5));
    Assert.That(result.Total, Is.EqualTo(10));
}
```

### Integration Tests
```csharp
[Test]
public async Task GetNotifications_WithValidToken_Returns200()
{
    // Arrange
    var token = await GetValidJwtToken();
    
    // Act
    var response = await _client.GetAsync("/api/notification?page=1&limit=5");
    response.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<NotificationResponse>(content);
    Assert.That(result.Success, Is.True);
}
```

## Monitoring & Logging

### Key Metrics
- Response time cho mỗi endpoint
- Số lượng request per user per day
- Error rate và error types
- Database query performance

### Logging Examples
```csharp
// In NotificationController
_logger.LogInformation("User {UserId} requested notifications page {Page} limit {Limit}", 
    userId, page, limit);

// In NotificationService
_logger.LogWarning("No notifications found for user {UserId}", userId);
_logger.LogError(ex, "Failed to retrieve notifications for user {UserId}", userId);
```

## Future Enhancements

### 1. Real-time Notifications
- Tích hợp SignalR Hub
- Push notification qua Firebase
- WebSocket connection cho web app

### 2. Advanced Features
- Mark as read/unread API
- Bulk operations (mark all as read)
- Notification categories/filters
- Search notifications
- Notification templates

### 3. Performance Optimization
- Redis caching layer
- Database sharding theo UserId
- Message queue cho notification processing
- CDN cho static notification content

### 4. Analytics & Insights
- Notification engagement tracking
- User behavior analytics
- A/B testing cho notification content
- Delivery success metrics
