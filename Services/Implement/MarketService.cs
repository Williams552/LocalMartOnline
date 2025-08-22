using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Market;
using LocalMartOnline.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using LocalMartOnline.Services.Interface;

namespace LocalMartOnline.Services.Implement;

public class MarketService : IMarketService
{
    private readonly IRepository<Market> _marketRepo;
    private readonly IRepository<Store> _storeRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IMapper _mapper;

    public MarketService(IRepository<Market> marketRepo, IRepository<Store> storeRepo, IRepository<Order> orderRepo, IMapper mapper)
    {
        _marketRepo = marketRepo;
        _storeRepo = storeRepo;
        _orderRepo = orderRepo;
        _mapper = mapper;
    }

    public async Task<MarketDto> CreateAsync(MarketCreateDto dto)
    {
        var market = _mapper.Map<Market>(dto);
        market.Status = "Active";
        market.CreatedAt = DateTime.Now;
        market.UpdatedAt = DateTime.Now;
        await _marketRepo.CreateAsync(market);
        var dtoResult = _mapper.Map<MarketDto>(market);
        dtoResult.StallCount = 0;
        return dtoResult;
    }

    public async Task<IEnumerable<MarketDto>> GetAllAsync()
    {
        var markets = await _marketRepo.GetAllAsync();
        var stores = await _storeRepo.GetAllAsync();
        return markets.Select(m =>
        {
            var dto = _mapper.Map<MarketDto>(m);
            dto.StallCount = stores.Count(s => s.MarketId.ToString() == m.Id && s.Status == "Open");
            return dto;
        });
    }

    public async Task<MarketDto?> GetByIdAsync(string id)
    {
        var market = await _marketRepo.GetByIdAsync(id);
        if (market == null) return null;
        var stores = await _storeRepo.GetAllAsync();
        var dto = _mapper.Map<MarketDto>(market);
        dto.StallCount = stores.Count(s => s.MarketId.ToString() == id && s.Status == "Open");
        return dto;
    }

    public async Task<bool> UpdateAsync(string id, MarketUpdateDto dto)
    {
        var market = await _marketRepo.GetByIdAsync(id);
        if (market == null) return false;

        var oldStatus = market.Status;

        _mapper.Map(dto, market);
        market.UpdatedAt = DateTime.Now;
        await _marketRepo.UpdateAsync(id, market);

        // Nếu market status thay đổi từ Active sang Suspended/Inactive, đóng tất cả stores
        if (oldStatus == "Active" && market.Status != "Active")
        {
            await CloseAllStoresInMarketAsync(id);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var stores = await _storeRepo.FindManyAsync(s => s.MarketId.ToString() == id && s.Status == "Open");
        if (stores.Any()) return false; // Only delete if no active stalls
        await _marketRepo.DeleteAsync(id);
        return true;
    }

    // ✅ Sửa lỗi: Lấy tất cả markets rồi filter trong memory
    public async Task<IEnumerable<MarketDto>> SearchAsync(string keyword)
    {
        var markets = await _marketRepo.GetAllAsync(); // Lấy tất cả markets
        var stores = await _storeRepo.GetAllAsync();

        // Filter trong memory với StringComparison.OrdinalIgnoreCase
        var filteredMarkets = markets.Where(m =>
            m.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            m.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (m.Description != null && m.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        );

        return filteredMarkets.Select(m =>
        {
            var dto = _mapper.Map<MarketDto>(m);
            dto.StallCount = stores.Count(s => s.MarketId.ToString() == m.Id && s.Status == "Open");
            return dto;
        });
    }

    public async Task<IEnumerable<MarketDto>> FilterAsync(string? status, string? area, int? minStalls, int? maxStalls)
    {
        var markets = await _marketRepo.GetAllAsync();
        var stores = await _storeRepo.GetAllAsync();

        var filtered = markets.Where(m =>
            (string.IsNullOrEmpty(status) || m.Status == status) &&
            (string.IsNullOrEmpty(area) || (m.Address != null && m.Address.Contains(area, StringComparison.OrdinalIgnoreCase)))
        ).ToList();

        var result = new List<MarketDto>();
        foreach (var m in filtered)
        {
            var stallCount = stores.Count(s => s.MarketId.ToString() == m.Id && s.Status == "Open");
            if ((minStalls == null || stallCount >= minStalls) && (maxStalls == null || stallCount <= maxStalls))
            {
                var dto = _mapper.Map<MarketDto>(m);
                dto.StallCount = stallCount;
                result.Add(dto);
            }
        }
        return result;
    }

    public async Task<bool> ToggleStatusAsync(string id)
    {
        var market = await _marketRepo.GetByIdAsync(id);
        if (market == null) return false;

        var oldStatus = market.Status;

        // Toggle trạng thái từ Active <-> Suspended
        market.Status = market.Status == "Active" ? "Suspended" : "Active";
        market.UpdatedAt = DateTime.Now;
        await _marketRepo.UpdateAsync(id, market);

        // Nếu market chuyển từ Active sang Suspended, đóng tất cả stores
        if (oldStatus == "Active" && market.Status == "Suspended")
        {
            await CloseAllStoresInMarketAsync(id);
        }

        return true;
    }

    public async Task<IEnumerable<MarketDto>> GetActiveMarketsAsync()
    {
        var markets = await _marketRepo.FindManyAsync(m => m.Status == "Active");
        var stores = await _storeRepo.GetAllAsync();
        return markets.Select(m =>
        {
            var dto = _mapper.Map<MarketDto>(m);
            dto.StallCount = stores.Count(s => s.MarketId.ToString() == m.Id && s.Status == "Open");
            return dto;
        });
    }

    // ✅ Sửa lỗi: Lấy Active markets trước, rồi filter trong memory
    public async Task<IEnumerable<MarketDto>> SearchActiveMarketsAsync(string keyword)
    {
        var markets = await _marketRepo.FindManyAsync(m => m.Status == "Active"); // Chỉ filter Status trong DB
        var stores = await _storeRepo.GetAllAsync();

        // Filter keyword trong memory
        var filteredMarkets = markets.Where(m =>
            m.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            m.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (m.Description != null && m.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        );

        return filteredMarkets.Select(m =>
        {
            var dto = _mapper.Map<MarketDto>(m);
            dto.StallCount = stores.Count(s => s.MarketId.ToString() == m.Id && s.Status == "Open");
            return dto;
        });
    }

    public async Task<IEnumerable<MarketDto>> FilterActiveMarketsAsync(string? area, int? minStalls, int? maxStalls)
    {
        var markets = await _marketRepo.FindManyAsync(m => m.Status == "Active"); // Chỉ filter Status trong DB
        var stores = await _storeRepo.GetAllAsync();

        // Filter area trong memory
        var filteredMarkets = markets.Where(m =>
            string.IsNullOrEmpty(area) || (m.Address != null && m.Address.Contains(area, StringComparison.OrdinalIgnoreCase))
        );

        var result = new List<MarketDto>();

        foreach (var m in filteredMarkets)
        {
            var stallCount = stores.Count(s => s.MarketId.ToString() == m.Id && s.Status == "Open");
            if ((minStalls == null || stallCount >= minStalls) && (maxStalls == null || stallCount <= maxStalls))
            {
                var dto = _mapper.Map<MarketDto>(m);
                dto.StallCount = stallCount;
                result.Add(dto);
            }
        }

        return result;
    }

    // Market Operation Methods
    public async Task<bool> IsMarketOpenAsync(string marketId)
    {
        var market = await _marketRepo.GetByIdAsync(marketId);
        if (market == null || market.Status != "Active")
            return false;

        if (string.IsNullOrEmpty(market.OperatingHours))
            return true; // Nếu không có giờ hoạt động thì mặc định luôn mở

        var currentTime = DateTime.Now;
        return IsTimeInOperatingHours(market.OperatingHours, currentTime);
    }

    public async Task<(bool IsOpen, string Reason)> GetMarketOpenStatusAsync(string marketId)
    {
        var market = await _marketRepo.GetByIdAsync(marketId);
        if (market == null)
            return (false, "Không tìm thấy chợ");

        if (market.Status != "Active")
            return (false, "Chợ hiện đang ngừng hoạt động");

        if (string.IsNullOrEmpty(market.OperatingHours))
            return (true, "Chợ đang hoạt động");

        var currentTime = DateTime.Now;
        var isInOperatingHours = IsTimeInOperatingHours(market.OperatingHours, currentTime);

        if (!isInOperatingHours)
            return (false, $"Chợ đang đóng cửa (giờ hoạt động: {market.OperatingHours})");

        return (true, "Chợ đang hoạt động");
    }

    public bool IsTimeInOperatingHours(string operatingHours, DateTime currentTime)
    {
        if (string.IsNullOrEmpty(operatingHours))
            return true;

        try
        {
            // Support formats: "06:00-18:00", "6:00 AM - 6:00 PM", "06:00 - 18:00"
            var timePattern = @"(\d{1,2}):(\d{2})(?:\s*(AM|PM))?\s*-\s*(\d{1,2}):(\d{2})(?:\s*(AM|PM))?";
            var match = System.Text.RegularExpressions.Regex.Match(operatingHours.Replace(" ", ""), timePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!match.Success)
                return true; // Nếu format không đúng thì mặc định luôn mở

            var startHour = int.Parse(match.Groups[1].Value);
            var startMinute = int.Parse(match.Groups[2].Value);
            var startAmPm = match.Groups[3].Value;

            var endHour = int.Parse(match.Groups[4].Value);
            var endMinute = int.Parse(match.Groups[5].Value);
            var endAmPm = match.Groups[6].Value;

            // Convert to 24-hour format if AM/PM is specified
            if (!string.IsNullOrEmpty(startAmPm))
            {
                if (startAmPm.ToUpper() == "PM" && startHour != 12)
                    startHour += 12;
                else if (startAmPm.ToUpper() == "AM" && startHour == 12)
                    startHour = 0;
            }

            if (!string.IsNullOrEmpty(endAmPm))
            {
                if (endAmPm.ToUpper() == "PM" && endHour != 12)
                    endHour += 12;
                else if (endAmPm.ToUpper() == "AM" && endHour == 12)
                    endHour = 0;
            }

            var openTime = new TimeSpan(startHour, startMinute, 0);
            var closeTime = new TimeSpan(endHour, endMinute, 0);
            var currentTimeSpan = currentTime.TimeOfDay;

            // Handle case where market operates across midnight (e.g., 22:00-06:00)
            if (closeTime < openTime)
            {
                return currentTimeSpan >= openTime || currentTimeSpan <= closeTime;
            }
            else
            {
                return currentTimeSpan >= openTime && currentTimeSpan <= closeTime;
            }
        }
        catch
        {
            return true; // Nếu có lỗi parse thì mặc định luôn mở
        }
    }

    public async Task UpdateStoreStatusBasedOnMarketHoursAsync()
    {
        var markets = await _marketRepo.GetAllAsync();
        var stores = await _storeRepo.GetAllAsync();

        foreach (var market in markets.Where(m => m.Status == "Active"))
        {
            var isMarketOpen = IsTimeInOperatingHours(market.OperatingHours ?? "", DateTime.Now);
            var marketStores = stores.Where(s => s.MarketId.ToString() == market.Id!.ToString()).ToList();

            foreach (var store in marketStores)
            {
                // Chỉ tự động đóng cửa hàng khi chợ đóng cửa
                // KHÔNG tự động mở lại cửa hàng khi chợ mở cửa (seller phải tự mở)
                // Thống nhất trạng thái cửa hàng mở là "Open"
                if (!isMarketOpen && store.Status == "Open")
                {
                    store.Status = "Closed";
                    store.UpdatedAt = DateTime.Now;
                    await _storeRepo.UpdateAsync(store.Id!, store);
                }
            }
        }
    }

    public async Task CloseAllStoresInMarketAsync(string marketId)
    {
        // Lấy tất cả stores trong market này
        var stores = await _storeRepo.FindManyAsync(s => s.MarketId.ToString() == marketId);

        foreach (var store in stores)
        {
            // Chỉ đóng những store đang Open, không touch Suspended stores
            if (store.Status == "Open")
            {
                store.Status = "Closed";
                store.UpdatedAt = DateTime.Now;
                await _storeRepo.UpdateAsync(store.Id!, store);
            }
        }
    }

    // Implement the missing interface method
    public async Task<MarketStatisticsDto> GetMarketStatisticsAsync(int year)
    {
        var markets = await _marketRepo.GetAllAsync();
        var stores = await _storeRepo.GetAllAsync();
        var orders = await _orderRepo.GetAllAsync();

        var marketStatsList = new List<MarketDetailStatisticsDto>();
        foreach (var market in markets.Where(m => m.CreatedAt.Year == year))
        {
            var marketStores = stores.Where(s => s.MarketId.ToString() == market.Id).ToList();
            var sellerIds = marketStores.Select(s => s.SellerId).Distinct().ToList();
            var marketOrders = orders.Where(o => sellerIds.Contains(o.SellerId)).ToList();

            // Aggregate sales data
            var totalRevenue = marketOrders.Sum(o => o.TotalAmount);
            var orderCount = marketOrders.Count;

            // Seller distribution by market
            var sellers = sellerIds.Count;

            var detail = new MarketDetailStatisticsDto
            {
                MarketId = market.Id,
                MarketName = market.Name,
                Status = market.Status,
                StoreCount = marketStores.Count,
                SellerCount = sellers,
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                AverageStoreRevenue = marketStores.Count > 0 ? totalRevenue / marketStores.Count : 0,
                AverageOrdersPerStore = marketStores.Count > 0 ? (double)orderCount / marketStores.Count : 0,
                MarketStores = marketStores.Select(s => new MarketStoreDto {
                    StoreId = s.Id ?? string.Empty,
                    StoreName = s.Name,
                    Status = s.Status
                }).ToList(),
                MarketOrders = marketOrders.Select(o => new MarketOrderDto {
                    OrderId = o.Id ?? string.Empty,
                    Amount = o.TotalAmount,
                    SellerId = o.SellerId
                }).ToList()
            };
            marketStatsList.Add(detail);
        }

        var result = new MarketStatisticsDto
        {
            TotalMarkets = markets.Count(m => m.CreatedAt.Year == year),
            ActiveMarkets = markets.Count(m => m.Status == "Active" && m.CreatedAt.Year == year),
            ClosedMarkets = markets.Count(m => m.Status != "Active" && m.CreatedAt.Year == year),
            MarketStatistics = marketStatsList
        };

        return result;
    }
}