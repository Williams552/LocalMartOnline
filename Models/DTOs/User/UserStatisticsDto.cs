using System;
using System.Collections.Generic;

namespace LocalMartOnline.Models.DTOs.User
{
    public class UserStatisticsDto
    {
        // Only include Buyer, Seller, Proxy roles in registration trends
        public Dictionary<string, List<RegistrationTrendDto>> RegistrationTrendsByRole { get; set; } = new Dictionary<string, List<RegistrationTrendDto>> {
            { "Buyer", new List<RegistrationTrendDto>() },
            { "Seller", new List<RegistrationTrendDto>() },
            { "Proxy", new List<RegistrationTrendDto>() }
        };

        // Basic user counts
        public int TotalUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public int NewRegistrations { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public double UserGrowthRate { get; set; }

        // Comprehensive statistics
        public Dictionary<string, int> RoleDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ActivityLevels { get; set; } = new Dictionary<string, int>();
        public List<RegistrationTrendDto> RegistrationTrends { get; set; } = new List<RegistrationTrendDto>();
        public List<TopUserDto> TopBuyers { get; set; } = new List<TopUserDto>();
        public List<TopUserDto> TopSellers { get; set; } = new List<TopUserDto>();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
    }

    public class RegistrationTrendDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public Dictionary<string, int> ByRole { get; set; } = new Dictionary<string, int>();
    }

    public class TopUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
        public decimal Rating { get; set; }
    }
}