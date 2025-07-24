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
    private readonly IMapper _mapper;

    public MarketService(IRepository<Market> marketRepo, IRepository<Store> storeRepo, IMapper mapper)
    {
        _marketRepo = marketRepo;
        _storeRepo = storeRepo;
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
        _mapper.Map(dto, market);
        market.UpdatedAt = DateTime.Now;
        await _marketRepo.UpdateAsync(id, market);
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

        // Toggle trạng thái từ Active <-> Suspended
        market.Status = market.Status == "Active" ? "Suspended" : "Active";
        market.UpdatedAt = DateTime.Now;
        await _marketRepo.UpdateAsync(id, market);
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
}