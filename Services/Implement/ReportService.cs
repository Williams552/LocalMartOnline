using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Report;
using System.Linq;

namespace LocalMartOnline.Services.Implement
{
    public class ReportService : IReportService
    {
        private readonly IMongoCollection<Report> _reportCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Store> _storeCollection;
        private readonly IMongoCollection<ProductUnit> _productUnitCollection;
        private readonly IMongoCollection<ProductImage> _productImageCollection;
        private readonly IMongoCollection<SellerRegistration> _sellerRegistrationCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<OrderItem> _orderItemCollection;


        public ReportService(IMongoDatabase database)
        {
            _reportCollection = database.GetCollection<Report>("Reports");
            _userCollection = database.GetCollection<User>("Users");
            _productCollection = database.GetCollection<Product>("Products");
            _productUnitCollection = database.GetCollection<ProductUnit>("ProductUnits");
            _productImageCollection = database.GetCollection<ProductImage>("ProductImages");
            _storeCollection = database.GetCollection<Store>("Stores");
            _sellerRegistrationCollection = database.GetCollection<SellerRegistration>("SellerRegistrations");
            _categoryCollection = database.GetCollection<Category>("Categories");
            _orderCollection = database.GetCollection<Order>("Orders"); // Thêm dòng này
            _orderItemCollection = database.GetCollection<OrderItem>("OrderItems"); // Thêm dòng này
        }

        public Task<object> GetRevenueStatisticsAsync(string from, string to)
        {
            // TODO: Tổng hợp doanh thu từ DB
            return Task.FromResult<object>(new { TotalRevenue = 0 });
        }

        public Task<ReportFileDto> ExportRevenueReportAsync(string from, string to)
        {
            // TODO: Tổng hợp và xuất file báo cáo doanh thu
            return Task.FromResult(new ReportFileDto());
        }

        public Task<string> GenerateReportAsync(GenerateReportRequestDto dto)
        {
            // TODO: Tổng hợp và lưu bản ghi báo cáo, trả về reportId
            return Task.FromResult("report-id-demo");
        }

        public Task<ReportFileDto> ExportReportAsync(string reportId)
        {
            // TODO: Lấy dữ liệu báo cáo từ DB và xuất file
            return Task.FromResult(new ReportFileDto());
        }

        public Task<object> GetMarketSalesReportAsync(string marketId, string from, string to)
        {
            // TODO: Tổng hợp doanh số bán hàng của chợ
            return Task.FromResult<object>(new { MarketSales = 0 });
        }

        public Task<int> GetNumberOfSellersAsync(string marketId)
        {
            // TODO: Đếm số lượng seller
            return Task.FromResult(0);
        }

        public async Task<GetReportsResponseDto> GetAllReportsAsync(GetReportsRequestDto request)
        {
            var filterBuilder = Builders<Report>.Filter;
            var filter = filterBuilder.Empty;

            // Filter by reporter ID
            if (!string.IsNullOrEmpty(request.ReporterId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(r => r.ReporterId, request.ReporterId));
            }

            // Filter by target type
            if (!string.IsNullOrEmpty(request.TargetType))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(r => r.TargetType, request.TargetType));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(request.Status))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(r => r.Status, request.Status));
            }

            // Filter by date range
            if (request.FromDate.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Gte(r => r.CreatedAt, request.FromDate.Value));
            }

            if (request.ToDate.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Lte(r => r.CreatedAt, request.ToDate.Value));
            }

            var totalCount = await _reportCollection.CountDocumentsAsync(filter);

            var reports = await _reportCollection
                .Find(filter)
                .Sort(Builders<Report>.Sort.Descending(r => r.CreatedAt))
                .Skip((request.Page - 1) * request.PageSize)
                .Limit(request.PageSize)
                .ToListAsync();

            var reportDtos = new List<ReportDto>();

            foreach (var report in reports)
            {
                var reportDto = await MapToReportDtoAsync(report);
                reportDtos.Add(reportDto);
            }

            return new GetReportsResponseDto
            {
                Reports = reportDtos,
                TotalCount = (int)totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<ReportDto?> GetReportByIdAsync(string reportId)
        {
            var report = await _reportCollection
                .Find(Builders<Report>.Filter.Eq(r => r.Id, reportId))
                .FirstOrDefaultAsync();

            if (report == null)
                return null;

            return await MapToReportDtoAsync(report);
        }

        public async Task<ReportDto?> CreateReportAsync(string reporterId, CreateReportDto createReportDto)
        {
            // Basic target type validation  
            var validTargetTypes = new[] { "Product", "Store", "Seller", "Buyer" };
            if (!validTargetTypes.Contains(createReportDto.TargetType))
                return null;

            var report = new Report
            {
                ReporterId = reporterId,
                TargetType = createReportDto.TargetType,
                TargetId = createReportDto.TargetId,
                Title = createReportDto.Title,
                Reason = createReportDto.Reason,
                EvidenceImage = createReportDto.EvidenceImage,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }; await _reportCollection.InsertOneAsync(report);

            return await GetReportByIdAsync(report.Id!);
        }

        public async Task<ReportDto?> UpdateReportStatusAsync(string reportId, UpdateReportStatusDto updateReportStatusDto)
        {
            var filter = Builders<Report>.Filter.Eq(r => r.Id, reportId);
            var updateBuilder = Builders<Report>.Update
                .Set(r => r.Status, updateReportStatusDto.Status)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);

            // Thêm AdminResponse nếu có
            if (!string.IsNullOrEmpty(updateReportStatusDto.AdminResponse))
            {
                updateBuilder = updateBuilder.Set(r => r.AdminResponse, updateReportStatusDto.AdminResponse);
            }

            var result = await _reportCollection.UpdateOneAsync(filter, updateBuilder);

            if (result.ModifiedCount == 0)
                return null;

            return await GetReportByIdAsync(reportId);
        }

        public async Task<GetReportsResponseDto> GetReportsByReporterAsync(string reporterId, int page = 1, int pageSize = 10)
        {
            var request = new GetReportsRequestDto
            {
                ReporterId = reporterId,
                Page = page,
                PageSize = pageSize
            };

            return await GetAllReportsAsync(request);        
        }

        // Debug method to check database collections
        public async Task<object> DebugDatabaseAsync()
        {
            try
            {
                // Get IMongoDatabase from service
                var database = _userCollection.Database;
                
                // List all collections
                var collections = await database.ListCollectionNamesAsync();
                var collectionNames = await collections.ToListAsync();
                
                var result = new
                {
                    DatabaseName = database.DatabaseNamespace.DatabaseName,
                    AllCollections = collectionNames,
                    CollectionCounts = new Dictionary<string, long>()
                };

                // Count documents in each collection
                foreach (var collectionName in collectionNames)
                {
                    try
                    {
                        var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
                        var count = await collection.CountDocumentsAsync(new MongoDB.Bson.BsonDocument());
                        ((Dictionary<string, long>)result.CollectionCounts)[collectionName] = count;
                    }
                    catch
                    {
                        ((Dictionary<string, long>)result.CollectionCounts)[collectionName] = -1; // Error counting
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message, StackTrace = ex.StackTrace };
            }
        }

        private async Task<ReportDto> MapToReportDtoAsync(Report report)
        {
            var reportDto = new ReportDto
            {
                Id = report.Id!,
                ReporterId = report.ReporterId,
                TargetType = report.TargetType,
                TargetId = report.TargetId,
                Title = report.Title,
                Reason = report.Reason,
                EvidenceImage = report.EvidenceImage,
                AdminResponse = report.AdminResponse,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                TargetPrice = null, // Khởi tạo null cho tất cả target types
                TargetImages = new List<string>(), // Khởi tạo empty list
                TargetUnit = null // Khởi tạo null cho tất cả target types
            };

            // Get reporter name và phone
            try
            {   
                var reporter = await _userCollection.Find(u => u.Id == report.ReporterId).FirstOrDefaultAsync();
                
                reportDto.ReporterName = reporter?.FullName ?? $"User Not Found ({report.ReporterId})";
                reportDto.ReporterPhone = reporter?.PhoneNumber ?? "No Phone";
            }
            catch (Exception ex)
            {
                reportDto.ReporterName = $"Error: {ex.Message}";
                reportDto.ReporterPhone = "";
            }
            try
            {
                switch (report.TargetType)
                {
                    case "Product":
                        var productFilter = Builders<Product>.Filter.Eq(p => p.Id, report.TargetId);
                        var product = await _productCollection.Find(productFilter).FirstOrDefaultAsync();
                        
                        if (product != null)
                        {   
                            reportDto.TargetName = product.Name;
                            reportDto.TargetPrice = product.Price;
                            
                            // Lấy hình ảnh sản phẩm
                            var productImages = await _productImageCollection
                                .Find(Builders<ProductImage>.Filter.Eq(img => img.ProductId, product.Id))
                                .ToListAsync();
                            
                            reportDto.TargetImages = productImages.Select(img => img.ImageUrl).ToList();
                            
                            // Lấy thông tin đơn vị sản phẩm
                            if (!string.IsNullOrEmpty(product.UnitId))
                            {
                                var productUnit = await _productUnitCollection
                                    .Find(Builders<ProductUnit>.Filter.Eq(pu => pu.Id, product.UnitId))
                                    .FirstOrDefaultAsync();
                                
                                if (productUnit != null)
                                {
                                    reportDto.TargetUnit = productUnit.Name;
                                }
                            }
                        }
                        else
                        {
                            reportDto.TargetName = $"Product Not Found ({report.TargetId})";
                            reportDto.TargetPrice = null;
                            reportDto.TargetImages = new List<string>();
                            reportDto.TargetUnit = null;
                        }
                        break;

                    case "Store":
                        var storeFilter = Builders<Store>.Filter.Eq(s => s.Id, report.TargetId);
                        var store = await _storeCollection.Find(storeFilter).FirstOrDefaultAsync();
                        
                        reportDto.TargetName = store?.Name ?? $"Store Not Found ({report.TargetId})";
                        break;

                    case "Seller":
                        var userFilter = Builders<User>.Filter.Eq(u => u.Id, report.TargetId);
                        var user = await _userCollection.Find(userFilter).FirstOrDefaultAsync();
                        
                        reportDto.TargetName = user?.FullName ?? $"User Not Found ({report.TargetId})";
                        break;
                    case "Buyer":
                        var buyerFilter = Builders<User>.Filter.Eq(u => u.Id, report.TargetId);
                        var buyer = await _userCollection.Find(buyerFilter).FirstOrDefaultAsync();

                        reportDto.TargetName = buyer?.FullName ?? $"User Not Found ({report.TargetId})";
                        break;

                    default:
                        reportDto.TargetName = $"Unknown Target Type: {report.TargetType}";
                        break;
                }
            }
            catch (Exception ex)
            {
                reportDto.TargetName = $"Error: {ex.Message}";
            }

            return reportDto;
        }

        public async Task<SellerMetricsDto> GetNumberOfSellersAsync(string? marketId = null, DateTime? from = null, DateTime? to = null)
        {
            var metrics = new SellerMetricsDto();

            // Build filter for stores
            var storeFilter = Builders<Store>.Filter.Empty;
            var userFilter = Builders<User>.Filter.Empty;
            var sellerRegFilter = Builders<LocalMartOnline.Models.SellerRegistration>.Filter.Empty;

            // Apply market filter if specified
            if (!string.IsNullOrEmpty(marketId))
            {
                storeFilter = Builders<Store>.Filter.Eq(s => s.MarketId, marketId);
                sellerRegFilter = Builders<LocalMartOnline.Models.SellerRegistration>.Filter.Eq(sr => sr.MarketId, marketId);
            }

            // Apply date filters if specified
            if (from.HasValue)
            {
                storeFilter = Builders<Store>.Filter.And(storeFilter, Builders<Store>.Filter.Gte(s => s.CreatedAt, from.Value));
                userFilter = Builders<User>.Filter.And(userFilter, Builders<User>.Filter.Gte(u => u.CreatedAt, from.Value));
                sellerRegFilter = Builders<LocalMartOnline.Models.SellerRegistration>.Filter.And(sellerRegFilter, Builders<LocalMartOnline.Models.SellerRegistration>.Filter.Gte(sr => sr.CreatedAt, from.Value));
            }

            if (to.HasValue)
            {
                storeFilter = Builders<Store>.Filter.And(storeFilter, Builders<Store>.Filter.Lte(s => s.CreatedAt, to.Value));
                userFilter = Builders<User>.Filter.And(userFilter, Builders<User>.Filter.Lte(u => u.CreatedAt, to.Value));
                sellerRegFilter = Builders<LocalMartOnline.Models.SellerRegistration>.Filter.And(sellerRegFilter, Builders<LocalMartOnline.Models.SellerRegistration>.Filter.Lte(sr => sr.CreatedAt, to.Value));
            }

            // 1. Total active sellers (stores with "Active" status)
            var activeStoreFilter = Builders<Store>.Filter.And(
                storeFilter,
                Builders<Store>.Filter.Eq(s => s.Status, "Active")
            );
            metrics.TotalActiveSellers = (int)await _storeCollection.CountDocumentsAsync(activeStoreFilter);

            // 2. Sellers by market
            var allStores = await _storeCollection.Find(storeFilter).ToListAsync();
            metrics.SellersByMarket = allStores
                .GroupBy(s => s.MarketId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 3. New seller registrations (period)
            var approvedSellerRegFilter = Builders<LocalMartOnline.Models.SellerRegistration>.Filter.And(
                sellerRegFilter,
                Builders<LocalMartOnline.Models.SellerRegistration>.Filter.Eq(sr => sr.Status, "Approved")
            );
            metrics.NewSellerRegistrations = (int)await _sellerRegistrationCollection.CountDocumentsAsync(approvedSellerRegFilter);

            // 4. Seller activity levels (based on store status)
            metrics.SellerActivityLevels = allStores
                .GroupBy(s => s.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // 5. Store-to-seller ratio (assuming 1 store per seller)
            var totalUsers = await _userCollection.CountDocumentsAsync(userFilter);
            metrics.StoreToSellerRatio = totalUsers > 0 ? (double)allStores.Count / totalUsers : 0;

            // 6. Seller performance tiers (based on average rating from Review collection)
            var reviewCollection = _userCollection.Database.GetCollection<Review>("Reviews");
            var reviewFilterBuilder = Builders<Review>.Filter;
            var reviewFilter = reviewFilterBuilder.Eq(r => r.TargetType, "Seller");
            if (!string.IsNullOrEmpty(marketId)) {
                // Lấy sellerIds của market này
                var sellerIds = allStores.Select(s => s.SellerId).Distinct().ToList();
                reviewFilter = reviewFilterBuilder.And(
                    reviewFilter,
                    reviewFilterBuilder.In(r => r.TargetId, sellerIds)
                );
            }
            var sellerReviews = await reviewCollection.Find(reviewFilter).ToListAsync();
            var sellerGroups = sellerReviews
                .GroupBy(r => r.TargetId)
                .Select(g => new {
                    SellerId = g.Key,
                    AvgRating = g.Any() ? g.Average(r => r.Rating) : 0
                })
                .ToList();

            metrics.SellerPerformanceTiers = new Dictionary<string, int>
            {
                { "Excellent (4.5-5.0)", sellerGroups.Count(s => s.AvgRating >= 4.5) },
                { "Good (4.0-4.4)", sellerGroups.Count(s => s.AvgRating >= 4.0 && s.AvgRating < 4.5) },
                { "Average (3.0-3.9)", sellerGroups.Count(s => s.AvgRating >= 3.0 && s.AvgRating < 4.0) },
                { "Below Average (2.0-2.9)", sellerGroups.Count(s => s.AvgRating >= 2.0 && s.AvgRating < 3.0) },
                { "Poor (0-1.9)", sellerGroups.Count(s => s.AvgRating < 2.0) }
            };

            return metrics;
        }

        public async Task<IEnumerable<ViolatingStoreDto>> GetViolatingStoresAsync(string marketId)
        {
            try
            {
                // Bước 1: Lấy unresolved reports targeting stores
                var reportFilterBuilder = Builders<Report>.Filter;
                var reportsFilter = reportFilterBuilder.And(
                    reportFilterBuilder.Eq(r => r.TargetType, "Store"),
                    reportFilterBuilder.In(r => r.Status, new[] { "Pending", "Under Investigation" })
                );

                var reports = await _reportCollection
                    .Find(reportsFilter)
                    .ToListAsync();

                if (!reports.Any())
                    return new List<ViolatingStoreDto>();

                // Bước 2: Group reports by store và extract store IDs
                var reportsByStore = reports.GroupBy(r => r.TargetId).ToDictionary(g => g.Key, g => g.ToList());
                var storeIds = reportsByStore.Keys.ToList();

                // Bước 3: Lấy detailed store information
                var storeFilterBuilder = Builders<Store>.Filter; // Fix: Tạo riêng filterBuilder cho Store
                var storeFilter = storeFilterBuilder.In(s => s.Id, storeIds);

                // Filter by market if specified
                if (!string.IsNullOrEmpty(marketId))
                {
                    storeFilter = storeFilterBuilder.And(storeFilter, storeFilterBuilder.Eq(s => s.MarketId, marketId));
                }

                var stores = await _storeCollection.Find(storeFilter).ToListAsync();

                if (!stores.Any())
                    return new List<ViolatingStoreDto>();

                // Bước 4: Lấy seller information
                var sellerIds = stores.Select(s => s.SellerId).Distinct().ToList();
                var sellers = await _userCollection
                    .Find(Builders<User>.Filter.In(u => u.Id, sellerIds))
                    .ToListAsync();

                // Fix: Handle nullable Id with null check
                var sellersDict = sellers
                    .Where(u => u.Id != null)
                    .ToDictionary(u => u.Id!, u => u);

                // Bước 5: Lấy market information  
                var marketIds = stores.Select(s => s.MarketId).Distinct().ToList();
                var markets = await _userCollection.Database.GetCollection<Market>("Markets")
                    .Find(Builders<Market>.Filter.In(m => m.Id, marketIds))
                    .ToListAsync();

                // Fix: Handle nullable Id with null check
                var marketsDict = markets
                    .Where(m => m.Id != null)
                    .ToDictionary(m => m.Id!, m => m);

                // Bước 6: Create ViolatingStoreDto cho each violating store
                var violatingStores = new List<ViolatingStoreDto>();

                foreach (var store in stores)
                {
                    if (store.Id == null || !reportsByStore.TryGetValue(store.Id, out var storeReports))
                        continue;

                    var seller = sellersDict.GetValueOrDefault(store.SellerId);
                    var market = marketsDict.GetValueOrDefault(store.MarketId);

                    // Calculate violation metrics
                    var totalReports = storeReports.Count;
                    var pendingReports = storeReports.Count(r => r.Status == "Pending");
                    var investigatingReports = storeReports.Count(r => r.Status == "Under Investigation");

                    // Calculate severity score
                    var severityScore = CalculateViolationSeverity(storeReports);
                    var severityLevel = GetSeverityLevel(severityScore);

                    // Get violation types breakdown
                    var violationTypes = GetViolationTypes(storeReports);

                    // Calculate recent violation frequency (last 30 days)
                    var recentViolationFrequency = CalculateRecentViolationFrequency(storeReports, 30);

                    // Get last violation date
                    var lastViolationDate = storeReports.Max(r => r.CreatedAt);

                    // Calculate customer impact level
                    var customerImpactLevel = GetCustomerImpactLevel(severityScore, totalReports);

                    // Get recommended actions
                    var recommendedActions = GetRecommendedActions(severityLevel, violationTypes);

                    // Get recent reports (last 5)
                    var recentReports = GetRecentViolations(storeReports.Take(5).ToList());

                    var violatingStore = new ViolatingStoreDto
                    {
                        StoreId = store.Id,
                        StoreName = store.Name,
                        StoreAddress = store.Address ?? "",
                        ContactNumber = store.ContactNumber ?? "",
                        OwnerName = seller?.FullName ?? "Unknown",
                        OwnerEmail = seller?.Email ?? "",
                        OwnerPhone = seller?.PhoneNumber ?? "",
                        MarketName = market?.Name ?? "Unknown Market",
                        MarketAddress = market?.Address ?? "",
                        TotalReports = totalReports,
                        PendingReports = pendingReports,
                        UnderInvestigationReports = investigatingReports,
                        SeverityLevel = severityLevel,
                        SeverityScore = severityScore,
                        ViolationTypes = violationTypes,
                        RecentViolationFrequency = recentViolationFrequency,
                        LastViolationDate = lastViolationDate,
                        CustomerImpactLevel = customerImpactLevel,
                        RecommendedActions = recommendedActions,
                        RecentReports = recentReports
                    };

                    violatingStores.Add(violatingStore);
                }

                // Bước 7: Sort by priority (severity score high to low)
                return violatingStores.OrderByDescending(vs => vs.SeverityScore).ToList();
            }
            catch (Exception ex)
            {
                // Log error
                throw new Exception($"Error getting violating stores: {ex.Message}", ex);
            }
        }

        // Helper methods
        private decimal CalculateViolationSeverity(List<Report> reports)
        {
            if (!reports.Any()) return 0;

            var baseScore = reports.Count * 10; // 10 points per report
            var recentBonus = reports.Count(r => r.CreatedAt > DateTime.UtcNow.AddDays(-7)) * 5; // 5 bonus points for recent reports
            var investigatingPenalty = reports.Count(r => r.Status == "Under Investigation") * 15; // 15 points for under investigation

            return baseScore + recentBonus + investigatingPenalty;
        }

        private string GetSeverityLevel(decimal score)
        {
            return score switch
            {
                >= 100 => "Critical",
                >= 60 => "High",
                >= 30 => "Medium",
                _ => "Low"
            };
        }

        private List<string> GetViolationTypes(List<Report> reports)
        {
            return reports
                .Where(r => !string.IsNullOrEmpty(r.Reason))
                .Select(r => ClassifyViolationType(r.Reason))
                .Distinct()
                .ToList();
        }

        private string ClassifyViolationType(string reason)
        {
            var lowerReason = reason.ToLower();

            if (lowerReason.Contains("fake") || lowerReason.Contains("giả"))
                return "Fake Products";
            if (lowerReason.Contains("price") || lowerReason.Contains("giá"))
                return "Price Issues";
            if (lowerReason.Contains("quality") || lowerReason.Contains("chất lượng"))
                return "Quality Issues";
            if (lowerReason.Contains("service") || lowerReason.Contains("dịch vụ"))
                return "Poor Service";
            if (lowerReason.Contains("delivery") || lowerReason.Contains("giao hàng"))
                return "Delivery Issues";

            return "Other";
        }

        private decimal CalculateRecentViolationFrequency(List<Report> reports, int days)
        {
            var recentReports = reports.Count(r => r.CreatedAt > DateTime.UtcNow.AddDays(-days));
            return (decimal)recentReports / days; // reports per day
        }

        private string GetCustomerImpactLevel(decimal severityScore, int totalReports)
        {
            if (severityScore >= 100 || totalReports >= 10)
                return "Critical";
            if (severityScore >= 60 || totalReports >= 5)
                return "High";
            if (severityScore >= 30 || totalReports >= 3)
                return "Medium";

            return "Low";
        }

        private List<string> GetRecommendedActions(string severityLevel, List<string> violationTypes)
        {
            var actions = new List<string>();

            switch (severityLevel)
            {
                case "Critical":
                    actions.Add("Immediate suspension pending investigation");
                    actions.Add("Notify law enforcement if applicable");
                    actions.Add("Issue public warning");
                    break;
                case "High":
                    actions.Add("Formal warning letter");
                    actions.Add("Mandatory compliance training");
                    actions.Add("Increased monitoring");
                    break;
                case "Medium":
                    actions.Add("Written warning");
                    actions.Add("Schedule compliance review");
                    break;
                case "Low":
                    actions.Add("Verbal warning");
                    actions.Add("Self-assessment checklist");
                    break;
            }

            // Add specific actions based on violation types
            if (violationTypes.Contains("Fake Products"))
                actions.Add("Product authenticity verification required");
            if (violationTypes.Contains("Quality Issues"))
                actions.Add("Quality control audit");

            return actions;
        }

        private List<RecentViolationDto> GetRecentViolations(List<Report> reports)
        {
            return reports
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RecentViolationDto
                {
                    ReportId = r.Id!,
                    Title = r.Title,
                    ViolationType = ClassifyViolationType(r.Reason),
                    ReportedDate = r.CreatedAt,
                    Status = r.Status,
                    ReporterName = "Anonymous", // Protect reporter privacy
                    Severity = GetViolationSeverity(r)
                })
                .ToList();
        }

        private string GetViolationSeverity(Report report)
        {
            var reason = report.Reason.ToLower();

            if (reason.Contains("fake") || reason.Contains("fraud"))
                return "Critical";
            if (reason.Contains("unsafe") || reason.Contains("dangerous"))
                return "High";
            if (reason.Contains("quality") || reason.Contains("service"))
                return "Medium";

            return "Low";
        }

        public async Task<ProductStatisticsDto> GetProductStatisticsAsync(string? categoryId = null, string period = "30d")
        {
            try
            {
                // Bước 1: Build filter cho products
                var productFilterBuilder = Builders<Product>.Filter;
                var productFilter = productFilterBuilder.Empty;

                // Filter by category if specified
                if (!string.IsNullOrEmpty(categoryId))
                {
                    productFilter = productFilterBuilder.Eq(p => p.CategoryId, categoryId);
                }

                // Bước 2: Lấy tất cả products
                var products = await _productCollection
                    .Find(productFilter)
                    .ToListAsync();

                if (!products.Any())
                {
                    return new ProductStatisticsDto
                    {
                        Period = period,
                        GeneratedAt = DateTime.UtcNow
                    };
                }

                // Bước 3: Calculate basic statistics
                var totalProducts = products.Count;
                var activeProducts = products.Count(p => p.Status == ProductStatus.Active);
                var inactiveProducts = products.Count(p => p.Status == ProductStatus.Inactive);
                var outOfStockProducts = products.Count(p => p.Status == ProductStatus.OutOfStock);
                var suspendedProducts = products.Count(p => p.Status == ProductStatus.Suspended);

                var prices = products.Select(p => p.Price).ToList();
                var averagePrice = prices.Any() ? prices.Average() : 0;
                var minPrice = prices.Any() ? prices.Min() : 0;
                var maxPrice = prices.Any() ? prices.Max() : 0;

                // Bước 4: Get category information
                var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
                var categories = await _categoryCollection
                    .Find(Builders<Category>.Filter.In(c => c.Id, categoryIds))
                    .ToListAsync();
                var categoriesDict = categories
                    .Where(c => c.Id != null)
                    .ToDictionary(c => c.Id!, c => c);

                // Bước 5: Get store information
                var storeIds = products.Select(p => p.StoreId).Distinct().ToList();
                var stores = await _storeCollection
                    .Find(Builders<Store>.Filter.In(s => s.Id, storeIds))
                    .ToListAsync();
                var storesDict = stores
                    .Where(s => s.Id != null)
                    .ToDictionary(s => s.Id!, s => s);

                // Bước 6: Get market information for stores
                var marketIds = stores.Select(s => s.MarketId).Distinct().ToList();
                var markets = await _userCollection.Database.GetCollection<Market>("Markets")
                    .Find(Builders<Market>.Filter.In(m => m.Id, marketIds))
                    .ToListAsync();
                var marketsDict = markets
                    .Where(m => m.Id != null)
                    .ToDictionary(m => m.Id!, m => m);

                // Bước 7: Calculate category breakdown
                var categoryBreakdown = products
                    .GroupBy(p => p.CategoryId)
                    .Select(g => new CategoryProductStatsDto
                    {
                        CategoryId = g.Key,
                        CategoryName = categoriesDict.GetValueOrDefault(g.Key)?.Name ?? "Unknown Category",
                        ProductCount = g.Count(),
                        ActiveProducts = g.Count(p => p.Status == ProductStatus.Active),
                        AveragePrice = g.Average(p => p.Price),
                        TotalValue = g.Sum(p => p.Price),
                        Percentage = (decimal)g.Count() / totalProducts * 100
                    })
                    .OrderByDescending(c => c.ProductCount)
                    .ToList();

                // Bước 8: Get best selling products (top 10)
                var bestSellingProducts = products
                    .OrderByDescending(p => p.PurchaseCount)
                    .Take(10)
                    .Select((p, index) => new BestSellingProductDto
                    {
                        ProductId = p.Id ?? "",
                        ProductName = p.Name,
                        StoreName = storesDict.GetValueOrDefault(p.StoreId)?.Name ?? "Unknown Store",
                        CategoryName = categoriesDict.GetValueOrDefault(p.CategoryId)?.Name ?? "Unknown Category",
                        Price = p.Price,
                        PurchaseCount = p.PurchaseCount,
                        Rank = index + 1,
                        PrimaryImageUrl = GetPrimaryImageUrl(p.Id).Result
                    })
                    .ToList();

                // Bước 9: Calculate price range distribution
                var priceRangeDistribution = CalculatePriceRangeDistribution(prices, totalProducts);

                // Bước 10: Get top stores by product count
                var topStoresByProducts = products
                    .GroupBy(p => p.StoreId)
                    .Select(g => new
                    {
                        StoreId = g.Key,
                        ProductCount = g.Count(),
                        ActiveProducts = g.Count(p => p.Status == ProductStatus.Active),
                        AveragePrice = g.Average(p => p.Price)
                    })
                    .OrderByDescending(s => s.ProductCount)
                    .Take(10)
                    .Select((s, index) => new StoreProductStatsDto
                    {
                        StoreId = s.StoreId,
                        StoreName = storesDict.GetValueOrDefault(s.StoreId)?.Name ?? "Unknown Store",
                        MarketName = GetMarketNameByStoreId(s.StoreId, storesDict, marketsDict),
                        ProductCount = s.ProductCount,
                        ActiveProducts = s.ActiveProducts,
                        AveragePrice = s.AveragePrice,
                        Rank = index + 1
                    })
                    .ToList();

                // Bước 11: Create final statistics
                return new ProductStatisticsDto
                {
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    InactiveProducts = inactiveProducts,
                    OutOfStockProducts = outOfStockProducts,
                    SuspendedProducts = suspendedProducts,
                    AveragePrice = averagePrice,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    CategoryBreakdown = categoryBreakdown,
                    BestSellingProducts = bestSellingProducts,
                    PriceRangeDistribution = priceRangeDistribution,
                    TopStoresByProducts = topStoresByProducts,
                    Period = period,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting product statistics: {ex.Message}", ex);
            }
        }

        // Helper methods
        private async Task<string?> GetPrimaryImageUrl(string? productId)
        {
            if (string.IsNullOrEmpty(productId))
                return null;

            try
            {
                var primaryImage = await _productImageCollection
                    .Find(Builders<ProductImage>.Filter.And(
                        Builders<ProductImage>.Filter.Eq(img => img.ProductId, productId)          
                    ))
                    .FirstOrDefaultAsync();

                return primaryImage?.ImageUrl;
            }
            catch
            {
                return null;
            }
        }

        private List<PriceRangeStatsDto> CalculatePriceRangeDistribution(List<decimal> prices, int totalProducts)
        {
            if (!prices.Any()) return new List<PriceRangeStatsDto>();

            var ranges = new List<(string name, decimal min, decimal max)>
    {
        ("Under 50K", 0, 50000),
        ("50K - 100K", 50000, 100000),
        ("100K - 500K", 100000, 500000),
        ("500K - 1M", 500000, 1000000),
        ("1M - 5M", 1000000, 5000000),
        ("Over 5M", 5000000, decimal.MaxValue)
    };

            return ranges.Select(range =>
            {
                var count = prices.Count(p => p >= range.min && (range.max == decimal.MaxValue ? true : p < range.max));
                return new PriceRangeStatsDto
                {
                    RangeName = range.name,
                    MinPrice = range.min,
                    MaxPrice = range.max == decimal.MaxValue ? 0 : range.max,
                    ProductCount = count,
                    Percentage = totalProducts > 0 ? (decimal)count / totalProducts * 100 : 0
                };
            }).ToList();
        }

        private string GetMarketNameByStoreId(string storeId, Dictionary<string, Store> storesDict, Dictionary<string, Market> marketsDict)
        {
            if (storesDict.TryGetValue(storeId, out var store) && marketsDict.TryGetValue(store.MarketId, out var market))
            {
                return market.Name;
            }
            return "Unknown Market";
        }

        public async Task<CategoryStatisticsDto> GetCategoryStatisticsAsync(string period = "30d")
        {
            try
            {
                // Bước 1: Lấy tất cả categories
                var categories = await _categoryCollection
                    .Find(Builders<Category>.Filter.Empty)
                    .ToListAsync();

                if (!categories.Any())
                {
                    return new CategoryStatisticsDto
                    {
                        Period = period,
                        GeneratedAt = DateTime.UtcNow
                    };
                }

                // Bước 2: Calculate basic category statistics
                var totalCategories = categories.Count;
                var activeCategories = categories.Count(c => c.IsActive);
                var inactiveCategories = categories.Count(c => !c.IsActive);

                // Bước 3: Lấy tất cả products
                var products = await _productCollection
                    .Find(Builders<Product>.Filter.Empty)
                    .ToListAsync();

                var totalProducts = products.Count;

                // Bước 4: Get store information for market context
                var storeIds = products.Select(p => p.StoreId).Distinct().ToList();
                var stores = await _storeCollection
                    .Find(Builders<Store>.Filter.In(s => s.Id, storeIds))
                    .ToListAsync();
                var storesDict = stores
                    .Where(s => s.Id != null)
                    .ToDictionary(s => s.Id!, s => s);

                // Bước 5: Get market information
                var marketIds = stores.Select(s => s.MarketId).Distinct().ToList();
                var markets = await _userCollection.Database.GetCollection<Market>("Markets")
                    .Find(Builders<Market>.Filter.In(m => m.Id, marketIds))
                    .ToListAsync();
                var marketsDict = markets
                    .Where(m => m.Id != null)
                    .ToDictionary(m => m.Id!, m => m);

                // Bước 6: Calculate category performance (simulated revenue data)
                var categoryPerformance = products
                    .GroupBy(p => p.CategoryId)
                    .Select(g =>
                    {
                        var category = categories.FirstOrDefault(c => c.Id == g.Key);
                        var totalRevenue = g.Sum(p => p.Price * p.PurchaseCount); // Simulated revenue
                        var averagePrice = g.Average(p => p.Price);

                        return new CategoryPerformanceDto
                        {
                            CategoryId = g.Key,
                            CategoryName = category?.Name ?? "Unknown Category",
                            TotalProducts = g.Count(),
                            ActiveProducts = g.Count(p => p.Status == ProductStatus.Active),
                            TotalRevenue = totalRevenue,
                            OrderCount = g.Sum(p => p.PurchaseCount), // Simulated order count
                            AveragePrice = averagePrice,
                            MarketShare = 0, // Will calculate later
                            GrowthRate = CalculateCategoryGrowthRate(g.Key, period), // Simulated
                            PerformanceTier = "Medium" // Will calculate later
                        };
                    })
                    .OrderByDescending(c => c.TotalRevenue)
                    .ToList();

                // Bước 7: Calculate market share and performance tiers
                var totalMarketRevenue = categoryPerformance.Sum(c => c.TotalRevenue);
                for (int i = 0; i < categoryPerformance.Count; i++)
                {
                    var category = categoryPerformance[i];
                    category.Rank = i + 1;
                    category.MarketShare = totalMarketRevenue > 0 ?
                        (category.TotalRevenue / totalMarketRevenue) * 100 : 0;
                    category.PerformanceTier = GetPerformanceTier(category.MarketShare, category.Rank);
                }

                // Bước 8: Calculate category distribution
                var categoryDistribution = products
                    .GroupBy(p => p.CategoryId)
                    .Select(g =>
                    {
                        var category = categories.FirstOrDefault(c => c.Id == g.Key);
                        var revenue = g.Sum(p => p.Price * p.PurchaseCount);

                        return new CategoryDistributionDto
                        {
                            CategoryId = g.Key,
                            CategoryName = category?.Name ?? "Unknown Category",
                            ProductCount = g.Count(),
                            Percentage = (decimal)g.Count() / totalProducts * 100,
                            Revenue = revenue,
                            RevenuePercentage = totalMarketRevenue > 0 ?
                                (revenue / totalMarketRevenue) * 100 : 0,
                            Status = category?.IsActive == true ? "Active" : "Inactive"
                        };
                    })
                    .OrderByDescending(c => c.ProductCount)
                    .ToList();

                // Bước 9: Generate category trends (simulated data for demonstration)
                var categoryTrends = categoryPerformance.Take(5) // Top 5 categories
                    .Select(c => new CategoryTrendDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Period = period,
                        DataPoints = GenerateTrendDataPoints(c.CategoryId, period),
                        TrendDirection = GetTrendDirection(c.GrowthRate),
                        GrowthRate = c.GrowthRate,
                        Seasonality = DetermineSeasonality(c.CategoryName)
                    })
                    .ToList();

                // Bước 10: Calculate market share analysis with top stores per category
                var marketShareAnalysis = categoryPerformance.Take(10) // Top 10 categories
                    .Select(c =>
                    {
                        var categoryProducts = products.Where(p => p.CategoryId == c.CategoryId).ToList();
                        var topStores = categoryProducts
                            .GroupBy(p => p.StoreId)
                            .Select(sg =>
                            {
                                var store = storesDict.GetValueOrDefault(sg.Key);
                                var market = store != null ? marketsDict.GetValueOrDefault(store.MarketId) : null;
                                var storeRevenue = sg.Sum(p => p.Price * p.PurchaseCount);

                                return new CategoryCompetitorDto
                                {
                                    StoreId = sg.Key,
                                    StoreName = store?.Name ?? "Unknown Store",
                                    MarketName = market?.Name ?? "Unknown Market",
                                    Revenue = storeRevenue,
                                    ProductCount = sg.Count(),
                                    MarketShare = c.TotalRevenue > 0 ?
                                        (storeRevenue / c.TotalRevenue) * 100 : 0
                                };
                            })
                            .OrderByDescending(s => s.Revenue)
                            .Take(3) // Top 3 stores per category
                            .ToList();

                        return new CategoryMarketShareDto
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName,
                            Revenue = c.TotalRevenue,
                            MarketShare = c.MarketShare,
                            ProductCount = c.TotalProducts,
                            TopStores = topStores,
                            Trend = GetTrendDirection(c.GrowthRate),
                            GrowthRate = c.GrowthRate
                        };
                    })
                    .ToList();

                // Bước 11: Create final statistics
                var averageProductsPerCategory = totalCategories > 0 ?
                    (decimal)totalProducts / totalCategories : 0;

                return new CategoryStatisticsDto
                {
                    TotalCategories = totalCategories,
                    ActiveCategories = activeCategories,
                    InactiveCategories = inactiveCategories,
                    TotalProducts = totalProducts,
                    TotalRevenue = totalMarketRevenue,
                    AverageProductsPerCategory = averageProductsPerCategory,
                    TopPerformingCategories = categoryPerformance.Take(10).ToList(),
                    CategoryDistribution = categoryDistribution,
                    CategoryTrends = categoryTrends,
                    MarketShareAnalysis = marketShareAnalysis,
                    Period = period,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting category statistics: {ex.Message}", ex);
            }
        }

        // Helper methods for category statistics
        private decimal CalculateCategoryGrowthRate(string categoryId, string period)
        {
            // Simulated growth rate calculation
            // In real implementation, this would compare current period vs previous period
            var random = new Random(categoryId.GetHashCode());
            return (decimal)(random.NextDouble() * 40 - 20); // -20% to +20%
        }

        private string GetPerformanceTier(decimal marketShare, int rank)
        {
            if (rank <= 3 && marketShare >= 15)
                return "Top Tier";
            if (rank <= 10 && marketShare >= 5)
                return "High Performance";
            if (marketShare >= 2)
                return "Medium Performance";
            return "Emerging";
        }

        private List<CategoryTrendDataPoint> GenerateTrendDataPoints(string categoryId, string period)
        {
            // Simulated trend data points
            // In real implementation, this would query historical data
            var dataPoints = new List<CategoryTrendDataPoint>();
            var days = GetPeriodDays(period);
            var random = new Random(categoryId.GetHashCode());

            for (int i = 0; i < Math.Min(days, 30); i += Math.Max(1, days / 10)) // Up to 10 data points
            {
                var date = DateTime.UtcNow.AddDays(-days + i);
                dataPoints.Add(new CategoryTrendDataPoint
                {
                    Date = date,
                    Revenue = (decimal)(random.NextDouble() * 1000000), // Random revenue
                    ProductCount = random.Next(10, 100),
                    OrderCount = random.Next(50, 500),
                    AveragePrice = (decimal)(random.NextDouble() * 500000 + 50000)
                });
            }

            return dataPoints.OrderBy(d => d.Date).ToList();
        }

        private int GetPeriodDays(string period)
        {
            return period.ToLower() switch
            {
                "7d" => 7,
                "30d" => 30,
                "3m" => 90,
                "6m" => 180,
                "1y" => 365,
                _ => 30
            };
        }

        private string GetTrendDirection(decimal growthRate)
        {
            if (growthRate > 10)
                return "Strong Growth";
            if (growthRate > 2)
                return "Growing";
            if (growthRate > -2)
                return "Stable";
            if (growthRate > -10)
                return "Declining";
            return "Sharp Decline";
        }

        private string DetermineSeasonality(string categoryName)
        {
            // Simple seasonality determination based on category name
            var lowerName = categoryName.ToLower();

            if (lowerName.Contains("food") || lowerName.Contains("thực phẩm"))
                return "Year-round with holiday peaks";
            if (lowerName.Contains("clothing") || lowerName.Contains("fashion") || lowerName.Contains("thời trang"))
                return "Seasonal (Spring/Fall peaks)";
            if (lowerName.Contains("electronic") || lowerName.Contains("điện tử"))
                return "Holiday season peaks";
            if (lowerName.Contains("home") || lowerName.Contains("gia dụng"))
                return "Spring cleaning peaks";

            return "Stable throughout year";
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(string period = "30d", string? status = null)
        {
            try
            {
                // Bước 1: Build filter cho orders
                var orderFilterBuilder = Builders<Order>.Filter;
                var orderFilter = orderFilterBuilder.Empty;

                // Filter by period
                var periodStart = GetPeriodStart(period);
                orderFilter = orderFilterBuilder.And(orderFilter,
                    orderFilterBuilder.Gte(o => o.CreatedAt, periodStart));

                // Filter by status if specified
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
                    {
                        orderFilter = orderFilterBuilder.And(orderFilter,
                            orderFilterBuilder.Eq(o => o.Status, orderStatus));
                    }
                }

                // Bước 2: Lấy orders trong period
                var orders = await _orderCollection
                    .Find(orderFilter)
                    .ToListAsync();

                if (!orders.Any())
                {
                    return new OrderStatisticsDto
                    {
                        Period = period,
                        GeneratedAt = DateTime.UtcNow
                    };
                }

                // Bước 3: Calculate basic order statistics
                var totalOrders = orders.Count;
                var completedOrders = orders.Count(o => o.Status == OrderStatus.Completed);
                var pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending);
                var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
                var confirmedOrders = orders.Count(o => o.Status == OrderStatus.Confirmed);
                var paidOrders = orders.Count(o => o.Status == OrderStatus.Paid);

                // Calculate revenue from completed orders
                var totalRevenue = orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .Sum(o => o.TotalAmount);

                var averageOrderValue = totalOrders > 0 ?
                    orders.Average(o => o.TotalAmount) : 0;

                var orderCompletionRate = totalOrders > 0 ?
                    (decimal)completedOrders / totalOrders * 100 : 0;

                var uniqueCustomers = orders.Select(o => o.BuyerId).Distinct().Count();

                // Bước 4: Get user information for customer and seller details
                var buyerIds = orders.Select(o => o.BuyerId).Distinct().ToList();
                var sellerIds = orders.Select(o => o.SellerId).Distinct().ToList();
                var userIds = buyerIds.Union(sellerIds).Distinct().ToList();

                var users = await _userCollection
                    .Find(Builders<User>.Filter.In(u => u.Id, userIds))
                    .ToListAsync();
                var usersDict = users
                    .Where(u => u.Id != null)
                    .ToDictionary(u => u.Id!, u => u);

                // Bước 5: Get store information
                var stores = await _storeCollection
                    .Find(Builders<Store>.Filter.In(s => s.SellerId, sellerIds))
                    .ToListAsync();
                var storesBySeller = stores
                    .GroupBy(s => s.SellerId)
                    .ToDictionary(g => g.Key, g => g.First());

                // Bước 6: Get market information
                var marketIds = stores.Select(s => s.MarketId).Distinct().ToList();
                var markets = await _userCollection.Database.GetCollection<Market>("Markets")
                    .Find(Builders<Market>.Filter.In(m => m.Id, marketIds))
                    .ToListAsync();
                var marketsDict = markets
                    .Where(m => m.Id != null)
                    .ToDictionary(m => m.Id!, m => m);

                // Bước 7: Calculate order status breakdown
                var ordersByStatus = orders
                    .GroupBy(o => o.Status.ToString())
                    .Select(g => new OrderStatusStatsDto
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        Percentage = (decimal)g.Count() / totalOrders * 100,
                        TotalValue = g.Sum(o => o.TotalAmount),
                        AverageValue = g.Average(o => o.TotalAmount)
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList();

                // Bước 8: Generate order trends (daily data points)
                var orderTrends = GenerateOrderTrends(orders, periodStart);

                // Bước 9: Get top customers
                var topCustomers = orders
                    .GroupBy(o => o.BuyerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        OrderCount = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount),
                        AverageOrderValue = g.Average(o => o.TotalAmount),
                        LastOrderDate = g.Max(o => o.CreatedAt)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(10)
                    .Select((c, index) => new TopCustomerDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerName = usersDict.GetValueOrDefault(c.CustomerId)?.FullName ?? "Unknown Customer",
                        OrderCount = c.OrderCount,
                        TotalSpent = c.TotalSpent,
                        AverageOrderValue = c.AverageOrderValue,
                        LastOrderDate = c.LastOrderDate,
                        Rank = index + 1
                    })
                    .ToList();

                // Bước 10: Get top sellers
                var topSellers = orders
                    .GroupBy(o => o.SellerId)
                    .Select(g => new
                    {
                        SellerId = g.Key,
                        OrderCount = g.Count(),
                        TotalRevenue = g.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
                        AverageOrderValue = g.Average(o => o.TotalAmount),
                        CompletionRate = g.Count() > 0 ? (decimal)g.Count(o => o.Status == OrderStatus.Completed) / g.Count() * 100 : 0
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .Take(10)
                    .Select((s, index) =>
                    {
                        var store = storesBySeller.GetValueOrDefault(s.SellerId);
                        var market = store != null ? marketsDict.GetValueOrDefault(store.MarketId) : null;

                        return new TopSellerDto
                        {
                            SellerId = s.SellerId,
                            SellerName = usersDict.GetValueOrDefault(s.SellerId)?.FullName ?? "Unknown Seller",
                            StoreName = store?.Name ?? "Unknown Store",
                            MarketName = market?.Name ?? "Unknown Market",
                            OrderCount = s.OrderCount,
                            TotalRevenue = s.TotalRevenue,
                            AverageOrderValue = s.AverageOrderValue,
                            CompletionRate = s.CompletionRate,
                            Rank = index + 1
                        };
                    })
                    .ToList();

                // Bước 11: Calculate peak ordering hours
                var peakOrderingHours = orders
                    .GroupBy(o => o.CreatedAt.Hour)
                    .Select(g => new HourlyOrderStatsDto
                    {
                        Hour = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
                        Percentage = (decimal)g.Count() / totalOrders * 100,
                        TimeSlot = GetTimeSlot(g.Key)
                    })
                    .OrderByDescending(h => h.OrderCount)
                    .ToList();

                // Bước 12: Calculate order metrics
                var orderItems = await GetOrderItemsForOrders(orders.Select(o => o.Id!).ToList());
                var orderMetrics = CalculateOrderMetrics(orders, orderItems);

                // Bước 13: Calculate customer patterns
                var customerPatterns = CalculateCustomerPatterns(orders, buyerIds);

                // Bước 14: Calculate growth rates (simulated)
                var revenueGrowth = CalculateGrowthRate("revenue", period);
                var orderGrowth = CalculateGrowthRate("orders", period);

                // Bước 15: Create final statistics
                return new OrderStatisticsDto
                {
                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    PendingOrders = pendingOrders,
                    CancelledOrders = cancelledOrders,
                    ConfirmedOrders = confirmedOrders,
                    PaidOrders = paidOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue,
                    OrderCompletionRate = orderCompletionRate,
                    UniqueCustomers = uniqueCustomers,
                    RevenueGrowth = revenueGrowth,
                    OrderGrowth = orderGrowth,
                    OrdersByStatus = ordersByStatus,
                    OrderTrends = orderTrends,
                    TopCustomers = topCustomers,
                    TopSellers = topSellers,
                    PeakOrderingHours = peakOrderingHours,
                    OrderMetrics = orderMetrics,
                    CustomerPatterns = customerPatterns,
                    Period = period,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting order statistics: {ex.Message}", ex);
            }
        }

        // Helper methods for order statistics
        private DateTime GetPeriodStart(string period)
        {
            return period.ToLower() switch
            {
                "7d" => DateTime.UtcNow.AddDays(-7),
                "30d" => DateTime.UtcNow.AddDays(-30),
                "3m" => DateTime.UtcNow.AddMonths(-3),
                "6m" => DateTime.UtcNow.AddMonths(-6),
                "1y" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.UtcNow.AddDays(-30)
            };
        }

        private List<OrderTrendDto> GenerateOrderTrends(List<Order> orders, DateTime periodStart)
        {
            var trends = new List<OrderTrendDto>();
            var totalDays = (DateTime.UtcNow - periodStart).Days;
            var interval = Math.Max(1, totalDays / 10); // Max 10 data points

            for (int i = 0; i <= totalDays; i += interval)
            {
                var date = periodStart.AddDays(i);
                var dayOrders = orders.Where(o => o.CreatedAt.Date == date.Date).ToList();

                if (dayOrders.Any() || i == 0) // Include first day even if no orders
                {
                    var completedOrders = dayOrders.Where(o => o.Status == OrderStatus.Completed).ToList();

                    trends.Add(new OrderTrendDto
                    {
                        Date = date,
                        OrderCount = dayOrders.Count,
                        Revenue = completedOrders.Sum(o => o.TotalAmount),
                        AverageOrderValue = dayOrders.Any() ? dayOrders.Average(o => o.TotalAmount) : 0,
                        CompletionRate = dayOrders.Any() ?
                            (decimal)completedOrders.Count / dayOrders.Count * 100 : 0,
                        CumulativeOrders = orders.Count(o => o.CreatedAt.Date <= date.Date)
                    });
                }
            }

            return trends.OrderBy(t => t.Date).ToList();
        }

        private string GetTimeSlot(int hour)
        {
            return hour switch
            {
                >= 6 and < 12 => "Morning (6AM-12PM)",
                >= 12 and < 18 => "Afternoon (12PM-6PM)",
                >= 18 and < 22 => "Evening (6PM-10PM)",
                _ => "Night (10PM-6AM)"
            };
        }

        private async Task<List<OrderItem>> GetOrderItemsForOrders(List<string> orderIds)
        {
            try
            {
                return await _orderItemCollection
                    .Find(Builders<OrderItem>.Filter.In(oi => oi.OrderId, orderIds))
                    .ToListAsync();
            }
            catch
            {
                return new List<OrderItem>();
            }
        }

        private OrderMetricsDto CalculateOrderMetrics(List<Order> orders, List<OrderItem> orderItems)
        {
            var totalItems = orderItems.Sum(oi => (int)oi.Quantity);
            var averageItemPrice = orderItems.Any() ?
                orderItems.Average(oi => oi.PriceAtPurchase) : 0;
            var averageItemsPerOrder = orders.Any() ?
                (decimal)totalItems / orders.Count : 0;

            // Calculate weekday vs weekend
            var weekdayOrders = orders.Where(o => (int)o.CreatedAt.DayOfWeek >= 1 && (int)o.CreatedAt.DayOfWeek <= 5).ToList();
            var weekendOrders = orders.Where(o => (int)o.CreatedAt.DayOfWeek == 0 || (int)o.CreatedAt.DayOfWeek == 6).ToList();

            var weekdayRevenue = weekdayOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);
            var weekendRevenue = weekendOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);

            return new OrderMetricsDto
            {
                TotalItems = totalItems,
                AverageItemPrice = averageItemPrice,
                AverageItemsPerOrder = averageItemsPerOrder,
                AverageProcessingTime = TimeSpan.FromHours(24), // Simulated
                RefundRate = 0, // Would need refund data
                CancellationRate = orders.Any() ?
                    (decimal)orders.Count(o => o.Status == OrderStatus.Cancelled) / orders.Count * 100 : 0,
                WeekdayVsWeekend = new WeekComparisonDto
                {
                    WeekdayOrders = weekdayOrders.Count,
                    WeekendOrders = weekendOrders.Count,
                    WeekdayRevenue = weekdayRevenue,
                    WeekendRevenue = weekendRevenue,
                    WeekdayAvgOrder = weekdayOrders.Any() ? weekdayOrders.Average(o => o.TotalAmount) : 0,
                    WeekendAvgOrder = weekendOrders.Any() ? weekendOrders.Average(o => o.TotalAmount) : 0
                }
            };
        }

        private CustomerPatternsDto CalculateCustomerPatterns(List<Order> orders, List<string> buyerIds)
        {
            var totalUniqueCustomers = buyerIds.Count;
            var customerOrderCounts = orders.GroupBy(o => o.BuyerId)
                .ToDictionary(g => g.Key, g => g.Count());

            var newCustomers = customerOrderCounts.Count(c => c.Value == 1);
            var returningCustomers = customerOrderCounts.Count(c => c.Value > 1);
            var averageOrdersPerCustomer = totalUniqueCustomers > 0 ?
                (decimal)orders.Count / totalUniqueCustomers : 0;

            return new CustomerPatternsDto
            {
                TotalUniqueCustomers = totalUniqueCustomers,
                NewCustomers = newCustomers,
                ReturningCustomers = returningCustomers,
                AverageOrdersPerCustomer = averageOrdersPerCustomer,
                CustomerRetentionRate = totalUniqueCustomers > 0 ?
                    (decimal)returningCustomers / totalUniqueCustomers * 100 : 0,
                RepeatCustomerRate = totalUniqueCustomers > 0 ?
                    (decimal)returningCustomers / totalUniqueCustomers * 100 : 0
            };
        }

        private decimal CalculateGrowthRate(string metric, string period)
        {
            // Simulated growth rate calculation
            // In real implementation, would compare with previous period
            var random = new Random($"{metric}{period}".GetHashCode());
            return (decimal)(random.NextDouble() * 30 - 10); // -10% to +20%
        }
    }

}