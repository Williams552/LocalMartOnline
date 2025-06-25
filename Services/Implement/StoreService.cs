using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Store;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace LocalMartOnline.Services.Implement
{
    public class StoreService : IStoreService
    {
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<StoreFollow> _followRepo;
        private readonly IMapper _mapper;

        public StoreService(
            IRepository<Store> storeRepo,
            IRepository<StoreFollow> followRepo,
            IMapper mapper)
        {
            _storeRepo = storeRepo;
            _followRepo = followRepo;
            _mapper = mapper;
        }

        // UC030: Open Store
        public async Task<StoreDto> CreateStoreAsync(StoreCreateDto dto)
        {
            var store = _mapper.Map<Store>(dto);
            store.Status = "Open";
            store.CreatedAt = DateTime.UtcNow;
            store.UpdatedAt = DateTime.UtcNow;
            store.Rating = 0.0m;
            await _storeRepo.CreateAsync(store);
            return _mapper.Map<StoreDto>(store);
        }

        // UC031: Close Store
        public async Task<bool> CloseStoreAsync(string id)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return false;
            if (store.Status == "Closed") return true;
            store.Status = "Closed";
            store.UpdatedAt = DateTime.UtcNow;
            await _storeRepo.UpdateAsync(id, store);
            return true;
        }

        // UC032: Update Store
        public async Task<bool> UpdateStoreAsync(string id, StoreUpdateDto dto)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return false;
            _mapper.Map(dto, store);
            store.UpdatedAt = DateTime.UtcNow;
            await _storeRepo.UpdateAsync(id, store);
            return true;
        }

        // UC037: Follow Store
        public async Task<bool> FollowStoreAsync(long userId, long storeId)
        {
            // Tìm store theo ObjectId (Id) thay vì StoreId
            var store = await _storeRepo.GetByIdAsync(storeId.ToString());
            if (store == null) return false;

            // Check if already followed
            var follows = await _followRepo.FindManyAsync(f => f.UserId == userId && f.StoreId == storeId);
            if (follows.Any()) return false;
            var follow = new StoreFollow
            {
                UserId = userId,
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow
            };
            await _followRepo.CreateAsync(follow);
            return true;
        }

        // UC039: Unfollow Store
        public async Task<bool> UnfollowStoreAsync(long userId, long storeId)
        {
            var follows = await _followRepo.FindManyAsync(f => f.UserId == userId && f.StoreId == storeId);
            var follow = follows.FirstOrDefault();
            if (follow == null) return false;
            await _followRepo.DeleteAsync(follow.Id!);
            return true;
        }

        // UC038: View Following Store List
        public async Task<IEnumerable<StoreDto>> GetFollowingStoresAsync(long userId)
        {
            var follows = await _followRepo.FindManyAsync(f => f.UserId == userId);
            var storeIds = follows.Select(f => f.StoreId.ToString()).ToList();
            if (!storeIds.Any()) return Enumerable.Empty<StoreDto>();
            // Tìm theo Id (ObjectId dạng string)
            var stores = await _storeRepo.FindManyAsync(s => storeIds.Contains(s.Id!));
            return _mapper.Map<IEnumerable<StoreDto>>(stores);
        }

        // UC040: View Store Profile
        public async Task<StoreDto?> GetStoreProfileAsync(string id)
        {
            var store = await _storeRepo.GetByIdAsync(id);
            return store == null ? null : _mapper.Map<StoreDto>(store);
        }
    }
}