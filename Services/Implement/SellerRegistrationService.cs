using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;

namespace LocalMartOnline.Services.Implement
{
    public class SellerRegistrationervice : ISellerRegistrationervice
    {
        private readonly IRepository<SellerRegistration> _sellerRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IMapper _mapper;
        public SellerRegistrationervice(IRepository<SellerRegistration> sellerRepo, IRepository<User> userRepo, IRepository<Store> storeRepo, IMapper mapper)
        {
            _sellerRepo = sellerRepo;
            _userRepo = userRepo;
            _storeRepo = storeRepo;
            _mapper = mapper;
        }

        public async Task<bool> RegisterAsync(string userId, SellerRegistrationRequestDTO dto)
        {
            var registration = _mapper.Map<SellerRegistration>(dto);
            registration.UserId = userId;
            registration.Status = "Pending";
            registration.CreatedAt = DateTime.UtcNow;
            registration.UpdatedAt = DateTime.UtcNow;
            await _sellerRepo.CreateAsync(registration);
            return true;
        }

        public async Task<SellerRegistrationRequestDTO?> GetMyRegistrationAsync(string userId)
        {
            var myReg = await _sellerRepo.FindOneAsync(r => r.UserId == userId);
            if (myReg == null) return null;
            return _mapper.Map<SellerRegistrationRequestDTO>(myReg);
        }

        public async Task<IEnumerable<SellerRegistrationRequestDTO>> GetAllRegistrationsAsync()
        {
            var regs = await _sellerRepo.GetAllAsync();
            return regs.Select(r => _mapper.Map<SellerRegistrationRequestDTO>(r));
        }

        public async Task<bool> ApproveAsync(SellerRegistrationApproveDTO dto)
        {
            var reg = await _sellerRepo.FindOneAsync(r => r.Id == dto.RegistrationId);
            if (reg == null) return false;
            reg.Status = dto.Approve ? "Approved" : "Rejected";
            reg.RejectionReason = dto.Approve ? null : dto.RejectionReason;
            reg.UpdatedAt = DateTime.UtcNow;
            await _sellerRepo.UpdateAsync(reg.Id!, reg);
            if (dto.Approve)
            {
                var store = new Store
                {
                    Name = reg.StoreName,
                    Address = reg.StoreAddress,
                    MarketId = reg.MarketId,
                    SellerId = reg.UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = "Active"
                };
                await _storeRepo.CreateAsync(store);
            }
            return true;
        }

        public async Task<SellerProfileDTO?> GetSellerProfileAsync(string userId)
        {
            var reg = await _sellerRepo.FindOneAsync(r => r.UserId == userId && r.Status == "Approved");
            if (reg == null) return null;
            var user = await _userRepo.FindOneAsync(u => u.Id == userId);
            if (user == null) return null;
            return new SellerProfileDTO
            {
                StoreName = reg.StoreName,
                StoreAddress = reg.StoreAddress,
                MarketId = reg.MarketId,
                BusinessLicense = reg.BusinessLicense,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl
            };
        }
    }
}
