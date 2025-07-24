# Fast Bargain API - Get My Bargains

## Endpoint mới: GET /api/fastbargain/my

### Mô tả
API để lấy danh sách fast bargains của người dùng hiện tại **theo role**:
- **Buyer**: Chỉ lấy những bargains mà user là buyer (`BuyerId == userId`)
- **Seller**: Chỉ lấy những bargains mà user là seller (`SellerId == userId`)  
- **Admin/Other roles**: Lấy tất cả bargains mà user tham gia

### Authentication
- **Required**: Bearer JWT token với `ClaimTypes.NameIdentifier` và `ClaimTypes.Role`
- **Roles**: Tất cả các role đã đăng nhập

### Role-based Authorization
| User Role | Filter Logic | Mô tả |
|-----------|--------------|-------|
| Buyer | `BuyerId == userId` | Chỉ bargains mình đã tạo để mua |
| Seller | `SellerId == userId` | Chỉ bargains cho sản phẩm của mình |
| Admin/Other | `BuyerId == userId OR SellerId == userId` | Tất cả bargains liên quan |

### Request Example
```http
GET /api/fastbargain/my
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

### Response Success (200 OK)
```json
{
    "success": true,
    "userRole": "Buyer",
    "data": [
        {
            "bargainId": "507f1f77bcf86cd799439011",
            "status": "Pending",
            "finalPrice": null,
            "productName": "iPhone 15 Pro Max",
            "originalPrice": 30000000,
            "productImages": [
                "https://example.com/image1.jpg",
                "https://example.com/image2.jpg"
            ],
            "buyerId": "buyer123",
            "sellerId": "seller456", 
            "userRole": "Buyer",
            "proposals": [
                {
                    "bargainId": "507f1f77bcf86cd799439011",
                    "userId": "buyer123",
                    "proposedPrice": 25000000,
                    "proposedAt": "2024-01-15T10:30:00"
                },
                {
                    "bargainId": "507f1f77bcf86cd799439011",
                    "userId": "seller456",
                    "proposedPrice": 28000000,
                    "proposedAt": "2024-01-15T11:00:00"
                }
            ]
        }
    ]
}
```

### Response Error (401 Unauthorized - Missing Role)
```json
{
    "success": false,
    "message": "Không xác định được role của user."
}
```

### Response Error (401 Unauthorized)
```json
{
    "success": false,
    "message": "Không xác định được user."
}
```

### Business Logic
1. Lấy userId từ JWT token (`ClaimTypes.NameIdentifier`)
2. Lấy userRole từ JWT token (`ClaimTypes.Role`)
3. Validate userId và userRole không rỗng
4. **Role-based filtering**:
   - **Buyer role**: Query với filter `BuyerId == userId`
   - **Seller role**: Query với filter `SellerId == userId`  
   - **Other roles**: Query với filter `BuyerId == userId OR SellerId == userId`
5. Convert entities sang DTOs với thông tin product và images
6. Set field `UserRole` trong response để frontend biết vai trò của user
7. Trả về danh sách bargains và userRole

### Sự khác biệt với API khác

| API Endpoint | Mô tả | Filter | Role-based |
|--------------|-------|--------|------------|
| `/api/fastbargain/user/{userId}` | Lấy bargains theo userId cụ thể (chỉ buyer) | `BuyerId == userId` | ❌ |
| `/api/fastbargain/my` | Lấy bargains theo role của user hiện tại | Role-dependent | ✅ |
| `/api/fastbargain/seller/{sellerId}` | Lấy bargains theo sellerId cụ thể | `SellerId == sellerId` | ❌ |

### Ưu điểm của phân quyền theo role:
- **Bảo mật**: User chỉ thấy bargains trong phạm vi quyền của mình
- **UI/UX**: Giao diện hiển thị đúng vai trò (buyer/seller view)
- **Performance**: Query hiệu quả hơn với filter chính xác
- **Business Logic**: Tuân thủ quy tắc kinh doanh về phân quyền

### Use Cases
- **Buyer Dashboard**: Xem tất cả bargains mình đã tạo và trạng thái
- **Seller Dashboard**: Xem tất cả bargains cho sản phẩm của mình  
- **Role-specific Actions**: Hiển thị actions phù hợp với vai trò
- **Analytics**: Thống kê theo từng role riêng biệt

### Frontend Integration Example

#### JavaScript/Fetch API
```javascript
async function getMyBargains() {
    try {
        const response = await fetch('/api/fastbargain/my', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to fetch my bargains');
        }
        
        const result = await response.json();
        return result.data;
    } catch (error) {
        console.error('Error fetching my bargains:', error);
        throw error;
    }
}
```

#### React Hook Example
```javascript
import { useState, useEffect } from 'react';

function useMyBargains() {
    const [bargains, setBargains] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    
    useEffect(() => {
        const fetchBargains = async () => {
            try {
                setLoading(true);
                const data = await getMyBargains();
                setBargains(data);
                setError(null);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };
        
        fetchBargains();
    }, []);
    
    return { bargains, loading, error, refetch: fetchBargains };
}

// Usage in component
function MyBargainsPage() {
    const { bargains, loading, error } = useMyBargains();
    
    if (loading) return <div>Loading...</div>;
    if (error) return <div>Error: {error}</div>;
    
    return (
        <div>
            <h2>My Bargains ({bargains.length})</h2>
            {bargains.map(bargain => (
                <div key={bargain.bargainId} className="bargain-card">
                    <h3>{bargain.productName}</h3>
                    <p>Status: {bargain.status}</p>
                    <p>Original Price: ${bargain.originalPrice?.toLocaleString()}</p>
                    {bargain.finalPrice && (
                        <p>Final Price: ${bargain.finalPrice.toLocaleString()}</p>
                    )}
                    <p>Proposals: {bargain.proposals.length}</p>
                </div>
            ))}
        </div>
    );
}
```

### Security Considerations
- ✅ User chỉ có thể xem bargains mà mình tham gia
- ✅ UserId được lấy từ JWT token, không từ request parameter
- ✅ Không expose thông tin của users khác
- ✅ Authorization middleware bảo vệ endpoint

### Performance Considerations
- **Index**: Tạo compound index trên `(BuyerId, SellerId)` trong MongoDB
- **Caching**: Cache kết quả với TTL ngắn nếu cần
- **Pagination**: Có thể thêm pagination nếu user có nhiều bargains

### Testing Example
```http
### Test My Bargains API
GET http://localhost:5000/api/fastbargain/my
Authorization: Bearer {{jwt_token}}

### Expected Response
# Should return all bargains where user is buyer or seller
# Response should include product details and proposal history
```
