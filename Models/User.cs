using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalMartOnline.Models
{
    public class        User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("full_name")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [BsonElement("is_email_verified")]
        public bool IsEmailVerified { get; set; }

        [BsonElement("two_factor_enabled")]
        public bool TwoFactorEnabled { get; set; }

        [BsonElement("avatar_url")]
        public string? AvatarUrl { get; set; }

        [BsonElement("operating_area")]
        public string? OperatingArea { get; set; }

        [BsonElement("preferred_language")]
        public string? PreferredLanguage { get; set; }

        [BsonElement("preferred_theme")]
        public string? PreferredTheme { get; set; }

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("updated_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("otp_token")]
        public string? OTPToken { get; set; }

        [BsonElement("otp_expiry")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? OTPExpiry { get; set; }

        [BsonElement("user_token")]
        public string? UserToken { get; set; }
    }
}
