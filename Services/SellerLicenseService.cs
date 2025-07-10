using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.License;

namespace LocalMartOnline.Services
{
    public class SellerLicenseService : ISellerLicenseService
    {
        private readonly IMongoCollection<SellerLicenses> _licenseCollection;
        private readonly IMongoCollection<SellerRegistrations> _registrationCollection;
        private readonly IMongoCollection<User> _userCollection;

        public SellerLicenseService(IMongoDatabase database)
        {
            _licenseCollection = database.GetCollection<SellerLicenses>("SellerLicenses");
            _registrationCollection = database.GetCollection<SellerRegistrations>("SellerRegistrations");
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task<SellerLicenseDto?> CreateSellerLicenseAsync(string userId, string userRole, CreateSellerLicenseDto createLicenseDto)
        {
            // Validate user role
            var validCreatorRoles = new[] { "Seller", "Admin", "MarketManagementBoardHead" };
            if (!validCreatorRoles.Contains(userRole))
                return null;

            // Validate registration exists
            var registration = await _registrationCollection
                .Find(Builders<SellerRegistrations>.Filter.Eq(r => r.Id, createLicenseDto.RegistrationId))
                .FirstOrDefaultAsync();

            if (registration == null)
                return null;

            // If creator is Seller, they can only create for their own registration
            if (userRole == "Seller" && userId != registration.UserId)
                return null;

            // Check for duplicate license type for the same registration
            var existingLicense = await _licenseCollection
                .Find(Builders<SellerLicenses>.Filter.And(
                    Builders<SellerLicenses>.Filter.Eq(l => l.RegistrationId, createLicenseDto.RegistrationId),
                    Builders<SellerLicenses>.Filter.Eq(l => l.LicenseType, createLicenseDto.LicenseType)
                ))
                .FirstOrDefaultAsync();

            if (existingLicense != null)
                return null; // License type already exists for this registration

            var license = new SellerLicenses
            {
                RegistrationId = createLicenseDto.RegistrationId,
                LicenseType = createLicenseDto.LicenseType,
                LicenseNumber = createLicenseDto.LicenseNumber,
                LicenseUrl = createLicenseDto.LicenseUrl,
                IssueDate = createLicenseDto.IssueDate,
                ExpiryDate = createLicenseDto.ExpiryDate,
                IssuingAuthority = createLicenseDto.IssuingAuthority,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _licenseCollection.InsertOneAsync(license);

            return await GetSellerLicenseByIdAsync(license.Id!);
        }

        public async Task<GetSellerLicensesResponseDto> GetAllSellerLicensesAsync(GetSellerLicensesRequestDto request)
        {
            var filterBuilder = Builders<SellerLicenses>.Filter;
            var filter = filterBuilder.Empty;

            // Filter by registration ID
            if (!string.IsNullOrEmpty(request.RegistrationId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(l => l.RegistrationId, request.RegistrationId));
            }

            // Filter by license type
            if (!string.IsNullOrEmpty(request.LicenseType))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(l => l.LicenseType, request.LicenseType));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(request.Status))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(l => l.Status, request.Status));
            }

            // Filter by expiration
            if (request.IsExpired.HasValue)
            {
                if (request.IsExpired.Value)
                {
                    filter = filterBuilder.And(filter, filterBuilder.Lt(l => l.ExpiryDate, DateTime.UtcNow));
                }
                else
                {
                    filter = filterBuilder.And(filter, filterBuilder.Gte(l => l.ExpiryDate, DateTime.UtcNow));
                }
            }

            // Filter by date range
            if (request.FromDate.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Gte(l => l.CreatedAt, request.FromDate.Value));
            }

            if (request.ToDate.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Lte(l => l.CreatedAt, request.ToDate.Value));
            }            // Filter by user ID if provided
            if (!string.IsNullOrEmpty(request.UserId))
            {
                var userRegistrations = await _registrationCollection
                    .Find(Builders<SellerRegistrations>.Filter.Eq(r => r.UserId, request.UserId))
                    .ToListAsync();

                var registrationIds = userRegistrations.Select(r => r.Id).ToList();
                filter = filterBuilder.And(filter, filterBuilder.In(l => l.RegistrationId, registrationIds));
            }

            var totalCount = await _licenseCollection.CountDocumentsAsync(filter);

            var licenses = await _licenseCollection
                .Find(filter)
                .Sort(Builders<SellerLicenses>.Sort.Descending(l => l.CreatedAt))
                .Skip((request.Page - 1) * request.PageSize)
                .Limit(request.PageSize)
                .ToListAsync();

            var licenseDtos = new List<SellerLicenseDto>();

            foreach (var license in licenses)
            {
                var licenseDto = await MapToSellerLicenseDtoAsync(license);
                licenseDtos.Add(licenseDto);
            }

            return new GetSellerLicensesResponseDto
            {
                Licenses = licenseDtos,
                TotalCount = (int)totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<SellerLicenseDto?> GetSellerLicenseByIdAsync(string licenseId)
        {
            var license = await _licenseCollection
                .Find(Builders<SellerLicenses>.Filter.Eq(l => l.Id, licenseId))
                .FirstOrDefaultAsync();

            if (license == null)
                return null;

            return await MapToSellerLicenseDtoAsync(license);
        }

        public async Task<GetSellerLicensesResponseDto> GetSellerLicensesByRegistrationAsync(string registrationId, int page = 1, int pageSize = 10)
        {
            var request = new GetSellerLicensesRequestDto
            {
                RegistrationId = registrationId,
                Page = page,
                PageSize = pageSize
            };

            return await GetAllSellerLicensesAsync(request);
        }

        public async Task<GetSellerLicensesResponseDto> GetSellerLicensesByUserAsync(string userId, int page = 1, int pageSize = 10)
        {
            var request = new GetSellerLicensesRequestDto
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            return await GetAllSellerLicensesAsync(request);
        }

        public async Task<SellerLicenseDto?> UpdateSellerLicenseAsync(string licenseId, string userId, string userRole, UpdateSellerLicenseDto updateLicenseDto)
        {
            var license = await _licenseCollection
                .Find(Builders<SellerLicenses>.Filter.Eq(l => l.Id, licenseId))
                .FirstOrDefaultAsync();

            if (license == null || license.Status != "Pending")
                return null;

            // Check permissions
            var canManage = await CanUserManageSellerLicenseAsync(userId, userRole, license.RegistrationId);
            if (!canManage)
                return null;

            var filter = Builders<SellerLicenses>.Filter.Eq(l => l.Id, licenseId);
            var update = Builders<SellerLicenses>.Update
                .Set(l => l.LicenseType, updateLicenseDto.LicenseType)
                .Set(l => l.LicenseNumber, updateLicenseDto.LicenseNumber)
                .Set(l => l.LicenseUrl, updateLicenseDto.LicenseUrl)
                .Set(l => l.IssueDate, updateLicenseDto.IssueDate)
                .Set(l => l.ExpiryDate, updateLicenseDto.ExpiryDate)
                .Set(l => l.IssuingAuthority, updateLicenseDto.IssuingAuthority)
                .Set(l => l.UpdatedAt, DateTime.UtcNow);

            var result = await _licenseCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
                return null;

            return await GetSellerLicenseByIdAsync(licenseId);
        }

        public async Task<bool> DeleteSellerLicenseAsync(string licenseId, string userId, string userRole)
        {
            var license = await _licenseCollection
                .Find(Builders<SellerLicenses>.Filter.Eq(l => l.Id, licenseId))
                .FirstOrDefaultAsync();

            if (license == null)
                return false;

            // Check permissions
            var canManage = await CanUserManageSellerLicenseAsync(userId, userRole, license.RegistrationId);
            if (!canManage)
                return false;

            var result = await _licenseCollection.DeleteOneAsync(Builders<SellerLicenses>.Filter.Eq(l => l.Id, licenseId));
            return result.DeletedCount > 0;
        }

        public async Task<SellerLicenseDto?> ReviewSellerLicenseAsync(string licenseId, string reviewerId, string reviewerRole, ReviewSellerLicenseDto reviewDto)
        {
            // Check if user can review
            if (!await CanUserReviewSellerLicenseAsync(reviewerRole))
                return null;

            var license = await _licenseCollection
                .Find(Builders<SellerLicenses>.Filter.Eq(l => l.Id, licenseId))
                .FirstOrDefaultAsync();

            if (license == null || license.Status != "Pending")
                return null;

            // Validate status
            var validStatuses = new[] { "Verified", "Rejected" };
            if (!validStatuses.Contains(reviewDto.Status))
                return null;

            var filter = Builders<SellerLicenses>.Filter.Eq(l => l.Id, licenseId);
            var update = Builders<SellerLicenses>.Update
                .Set(l => l.Status, reviewDto.Status)
                .Set(l => l.RejectionReason, reviewDto.RejectionReason)
                .Set(l => l.UpdatedAt, DateTime.UtcNow);

            var result = await _licenseCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
                return null;

            return await GetSellerLicenseByIdAsync(licenseId);
        }

        public async Task<SellerLicenseStatisticsDto> GetSellerLicenseStatisticsAsync()
        {
            var totalLicenses = await _licenseCollection.CountDocumentsAsync(Builders<SellerLicenses>.Filter.Empty);
            var pendingLicenses = await _licenseCollection.CountDocumentsAsync(Builders<SellerLicenses>.Filter.Eq(l => l.Status, "Pending"));
            var verifiedLicenses = await _licenseCollection.CountDocumentsAsync(Builders<SellerLicenses>.Filter.Eq(l => l.Status, "Verified"));
            var rejectedLicenses = await _licenseCollection.CountDocumentsAsync(Builders<SellerLicenses>.Filter.Eq(l => l.Status, "Rejected"));

            var expiredLicenses = await _licenseCollection.CountDocumentsAsync(
                Builders<SellerLicenses>.Filter.Lt(l => l.ExpiryDate, DateTime.UtcNow));

            var expiringLicenses = await _licenseCollection.CountDocumentsAsync(
                Builders<SellerLicenses>.Filter.And(
                    Builders<SellerLicenses>.Filter.Gte(l => l.ExpiryDate, DateTime.UtcNow),
                    Builders<SellerLicenses>.Filter.Lte(l => l.ExpiryDate, DateTime.UtcNow.AddDays(30))
                ));

            // Get licenses by type
            var licensesByType = new Dictionary<string, int>();
            var licenseTypes = new[] { "BusinessLicense", "FoodSafetyCertificate", "TaxRegistration", "EnvironmentalPermit", "Other" };
            foreach (var type in licenseTypes)
            {
                var count = await _licenseCollection.CountDocumentsAsync(Builders<SellerLicenses>.Filter.Eq(l => l.LicenseType, type));
                licensesByType[type] = (int)count;
            }

            // Get licenses by status
            var licensesByStatus = new Dictionary<string, int>
            {
                ["Pending"] = (int)pendingLicenses,
                ["Verified"] = (int)verifiedLicenses,
                ["Rejected"] = (int)rejectedLicenses
            };

            return new SellerLicenseStatisticsDto
            {
                TotalLicenses = (int)totalLicenses,
                PendingLicenses = (int)pendingLicenses,
                VerifiedLicenses = (int)verifiedLicenses,
                RejectedLicenses = (int)rejectedLicenses,
                ExpiredLicenses = (int)expiredLicenses,
                ExpiringLicenses = (int)expiringLicenses,
                LicensesByType = licensesByType,
                LicensesByStatus = licensesByStatus
            };
        }

        public Task<bool> CanUserManageSellerLicenseAsync(string userId, string userRole, string registrationId)
        {
            // Admin and MarketManagementBoardHead can manage any license
            if (userRole == "Admin" || userRole == "MarketManagementBoardHead")
                return Task.FromResult(true);

            // Seller can only manage their own licenses
            if (userRole == "Seller")
            {
                // Need to check if the registration belongs to this user
                return CheckRegistrationOwnershipAsync(userId, registrationId);
            }

            return Task.FromResult(false);
        }

        public Task<bool> CanUserReviewSellerLicenseAsync(string userRole)
        {
            var validReviewerRoles = new[] { "Admin", "MarketManagementBoardHead", "MarketStaff" };
            return Task.FromResult(validReviewerRoles.Contains(userRole));
        }        private async Task<bool> CheckRegistrationOwnershipAsync(string userId, string registrationId)
        {
            var registration = await _registrationCollection
                .Find(Builders<SellerRegistrations>.Filter.And(
                    Builders<SellerRegistrations>.Filter.Eq(r => r.Id, registrationId),
                    Builders<SellerRegistrations>.Filter.Eq(r => r.UserId, userId)
                ))
                .FirstOrDefaultAsync();

            return registration != null;
        }

        private async Task<SellerLicenseDto> MapToSellerLicenseDtoAsync(SellerLicenses license)
        {
            var licenseDto = new SellerLicenseDto
            {
                Id = license.Id!,
                RegistrationId = license.RegistrationId,
                LicenseType = license.LicenseType,
                LicenseNumber = license.LicenseNumber,
                LicenseUrl = license.LicenseUrl,
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                IssuingAuthority = license.IssuingAuthority,
                Status = license.Status,
                RejectionReason = license.RejectionReason,
                CreatedAt = license.CreatedAt,
                UpdatedAt = license.UpdatedAt
            };

            // Get registration info
            var registration = await _registrationCollection
                .Find(Builders<SellerRegistrations>.Filter.Eq(r => r.Id, license.RegistrationId))
                .FirstOrDefaultAsync();

            if (registration != null)
            {
                licenseDto.UserId = registration.UserId;
                licenseDto.StoreName = registration.StoreName;

                // Get seller name
                var seller = await _userCollection
                    .Find(Builders<User>.Filter.Eq(u => u.Id, registration.UserId))
                    .FirstOrDefaultAsync();
                licenseDto.SellerName = seller?.FullName ?? "Unknown Seller";
            }

            return licenseDto;
        }
    }
}
