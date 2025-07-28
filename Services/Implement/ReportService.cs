using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Report;

namespace LocalMartOnline.Services.Implement
{
    public class ReportService : IReportService
    {
        private readonly IMongoCollection<Report> _reportCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Store> _storeCollection;

        public ReportService(IMongoDatabase database)
        {
            _reportCollection = database.GetCollection<Report>("reports");
            _userCollection = database.GetCollection<User>("users");
            _productCollection = database.GetCollection<Product>("products");
            _storeCollection = database.GetCollection<Store>("stores");
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
        }        public async Task<ReportDto?> CreateReportAsync(string reporterId, CreateReportDto createReportDto)
        {
            // Basic target type validation  
            var validTargetTypes = new[] { "Product", "Store", "Seller", "Buyer" };
            if (!validTargetTypes.Contains(createReportDto.TargetType))
                return null;

            // Check if user already reported this target
            var existingReport = await _reportCollection
                .Find(Builders<Report>.Filter.And(
                    Builders<Report>.Filter.Eq(r => r.ReporterId, reporterId),
                    Builders<Report>.Filter.Eq(r => r.TargetType, createReportDto.TargetType),
                    Builders<Report>.Filter.Eq(r => r.TargetId, createReportDto.TargetId)
                ))
                .FirstOrDefaultAsync();

            if (existingReport != null)
                return null; // User already reported this target

            var report = new Report
            {
                ReporterId = reporterId,
                TargetType = createReportDto.TargetType,
                TargetId = createReportDto.TargetId,
                Reason = createReportDto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };            await _reportCollection.InsertOneAsync(report);

            return await GetReportByIdAsync(report.Id!);
        }

        public async Task<ReportDto?> UpdateReportStatusAsync(string reportId, UpdateReportStatusDto updateReportStatusDto)
        {
            var filter = Builders<Report>.Filter.Eq(r => r.Id, reportId);
            var update = Builders<Report>.Update
                .Set(r => r.Status, updateReportStatusDto.Status)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);

            var result = await _reportCollection.UpdateOneAsync(filter, update);

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

            return await GetAllReportsAsync(request);        }

        private async Task<ReportDto> MapToReportDtoAsync(Report report)
        {            var reportDto = new ReportDto
            {
                Id = report.Id!,
                ReporterId = report.ReporterId,
                TargetType = report.TargetType,
                TargetId = report.TargetId,
                Reason = report.Reason,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt
            };

            // Get reporter name
            var reporter = await _userCollection
                .Find(Builders<User>.Filter.Eq(u => u.Id, report.ReporterId))
                .FirstOrDefaultAsync();
            reportDto.ReporterName = reporter?.FullName ?? "Unknown User";

            // Get target name based on target type
            switch (report.TargetType)
            {
                case "Product":
                    var product = await _productCollection
                        .Find(Builders<Product>.Filter.Eq(p => p.Id, report.TargetId))
                        .FirstOrDefaultAsync();
                    reportDto.TargetName = product?.Name ?? "Unknown Product";
                    break;

                case "Store":
                    var store = await _storeCollection
                        .Find(Builders<Store>.Filter.Eq(s => s.Id, report.TargetId))
                        .FirstOrDefaultAsync();
                    reportDto.TargetName = store?.Name ?? "Unknown Store";
                    break;

                case "Buyer":
                case "Seller":
                    var user = await _userCollection
                        .Find(Builders<User>.Filter.Eq(u => u.Id, report.TargetId))
                        .FirstOrDefaultAsync();
                    reportDto.TargetName = user?.FullName ?? "Unknown User";
                    break;

                default:
                    reportDto.TargetName = "Unknown Target";
                    break;
            }

            return reportDto;
        }
    }
}