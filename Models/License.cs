using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    [BsonIgnoreExtraElements]
    public class SellerLicenses
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("registration_id")]
        public string RegistrationId { get; set; } = string.Empty; // Reference to SellerRegistrations Id

        [BsonElement("license_type")]
        public string LicenseType { get; set; } = string.Empty; // BusinessLicense, FoodSafetyCertificate, TaxRegistration, EnvironmentalPermit, Other

        [BsonElement("license_number")]
        public string? LicenseNumber { get; set; }

        [BsonElement("license_url")]
        public string LicenseUrl { get; set; } = string.Empty;

        [BsonElement("issue_date")]
        public DateTime? IssueDate { get; set; }

        [BsonElement("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [BsonElement("issuing_authority")]
        public string? IssuingAuthority { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, Verified, Rejected

        [BsonElement("rejection_reason")]
        public string? RejectionReason { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
