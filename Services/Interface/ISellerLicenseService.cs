using LocalMartOnline.Models.DTOs.License;

namespace LocalMartOnline.Services
{
    public interface ISellerLicenseService
    {
        // Create License (Seller, Admin, MarketManagementBoardHead)
        Task<SellerLicenseDto?> CreateSellerLicenseAsync(string userId, string userRole, CreateSellerLicenseDto createLicenseDto);        // Get Licenses
        Task<GetSellerLicensesResponseDto> GetAllSellerLicensesAsync(GetSellerLicensesRequestDto request);
        Task<SellerLicenseDto?> GetSellerLicenseByIdAsync(string licenseId);
        Task<GetSellerLicensesResponseDto> GetSellerLicensesByRegistrationAsync(string registrationId, int page = 1, int pageSize = 10);
        Task<GetSellerLicensesResponseDto> GetSellerLicensesByUserAsync(string userId, int page = 1, int pageSize = 10);

        // Update License (Only for Pending status)
        Task<SellerLicenseDto?> UpdateSellerLicenseAsync(string licenseId, string userId, string userRole, UpdateSellerLicenseDto updateLicenseDto);

        // Delete License (Seller, Admin, MarketManagementBoardHead)
        Task<bool> DeleteSellerLicenseAsync(string licenseId, string userId, string userRole);

        // Review License (Admin, MarketManagementBoardHead, MarketStaff)
        Task<SellerLicenseDto?> ReviewSellerLicenseAsync(string licenseId, string reviewerId, string reviewerRole, ReviewSellerLicenseDto reviewDto);

        // Statistics (Admin, MarketManagementBoardHead)
        Task<SellerLicenseStatisticsDto> GetSellerLicenseStatisticsAsync();

        // Permission checks
        Task<bool> CanUserManageSellerLicenseAsync(string userId, string userRole, string registrationId);
        Task<bool> CanUserReviewSellerLicenseAsync(string userRole);
    }
}
