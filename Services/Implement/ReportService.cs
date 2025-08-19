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

        public ReportService(IMongoDatabase database)
        {
            _reportCollection = database.GetCollection<Report>("Reports");
            _userCollection = database.GetCollection<User>("Users");
            _productCollection = database.GetCollection<Product>("Products");
            _productUnitCollection = database.GetCollection<ProductUnit>("ProductUnits");
            _productImageCollection = database.GetCollection<ProductImage>("ProductImages");
            _storeCollection = database.GetCollection<Store>("Stores");
            _sellerRegistrationCollection = database.GetCollection<SellerRegistration>("SellerRegistrations");
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

        public Task<IEnumerable<ViolatingStoreDto>> GetViolatingStoresAsync(string marketId)
        {
            // TODO: Lấy danh sách cửa hàng vi phạm
            return Task.FromResult<IEnumerable<ViolatingStoreDto>>(new List<ViolatingStoreDto>());
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

            // 6. Seller performance tiers (simplified - based on store rating)
            var storesWithRatings = allStores.Where(s => s.Rating > 0).ToList();
            metrics.SellerPerformanceTiers = new Dictionary<string, int>
            {
                { "Excellent (4.5-5.0)", storesWithRatings.Count(s => s.Rating >= 4.5m) },
                { "Good (4.0-4.4)", storesWithRatings.Count(s => s.Rating >= 4.0m && s.Rating < 4.5m) },
                { "Average (3.0-3.9)", storesWithRatings.Count(s => s.Rating >= 3.0m && s.Rating < 4.0m) },
                { "Below Average (2.0-2.9)", storesWithRatings.Count(s => s.Rating >= 2.0m && s.Rating < 3.0m) },
                { "Poor (0-1.9)", storesWithRatings.Count(s => s.Rating < 2.0m) }
            };

            return metrics;
        }
    }
}