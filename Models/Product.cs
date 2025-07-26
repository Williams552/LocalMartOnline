using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models
{
    public enum ProductStatus
    {
        Active,      // Còn hàng - hiển thị cho user và seller
        OutOfStock,  // Hết hàng - chỉ hiển thị cho seller
        Inactive     // Đã xóa - không hiển thị cho ai
    }

    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("store_id")]
        public string StoreId { get; set; } = string.Empty; // ObjectId as string

        [BsonElement("category_id")]
        public string CategoryId { get; set; } = string.Empty; // ObjectId as string

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("unit_id")]
        public string UnitId { get; set; } = string.Empty;

        [BsonElement("images")]
        public List<string> Images { get; set; } = new List<string>();

        [BsonElement("minimum_quantity")]
        public decimal MinimumQuantity { get; set; } = 1;

        [BsonElement("stock_quantity")]
        public decimal StockQuantity { get; set; } = 0; // Added stock management

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public ProductStatus Status { get; set; } = ProductStatus.Active;

        [BsonElement("purchase_count")]
        public int PurchaseCount { get; set; } = 0; // Số lần được mua thành công

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Helper properties
        [BsonIgnore]
        public bool IsVisibleToUsers => Status == ProductStatus.Active;

        [BsonIgnore]
        public bool IsVisibleToSellers => Status == ProductStatus.Active || Status == ProductStatus.OutOfStock || Status == ProductStatus.Inactive;
    }
}