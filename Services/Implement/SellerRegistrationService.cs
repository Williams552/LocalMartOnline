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
            registration.CreatedAt = DateTime.Now;
            registration.UpdatedAt = DateTime.Now;
            await _sellerRepo.CreateAsync(registration);
            return true;
        }

        public async Task<SellerRegistrationResponseDTO?> GetMyRegistrationAsync(string userId)
        {
            var myReg = await _sellerRepo.FindOneAsync(r => r.UserId == userId);
            if (myReg == null) return null;
            var dto = _mapper.Map<SellerRegistrationResponseDTO>(myReg);
            // Lấy thông tin user
            var user = await _userRepo.FindOneAsync(u => u.Id == userId);
            if (user != null)
            {
                dto.Name = user.FullName;
                dto.Email = user.Email;
                dto.PhoneNumber = user.PhoneNumber;
            }
            return dto;
        }

        public async Task<IEnumerable<SellerRegistrationResponseDTO>> GetAllRegistrationsAsync()
        {
            var regs = await _sellerRepo.GetAllAsync();
            var userIds = regs.Select(r => r.UserId).Distinct().ToList();
            var users = await _userRepo.FindManyAsync(u => userIds.Contains(u.Id));
            var userDict = users.ToDictionary(u => u.Id, u => u);
            var result = new List<SellerRegistrationResponseDTO>();
            foreach (var reg in regs)
            {
                var dto = _mapper.Map<SellerRegistrationResponseDTO>(reg);
                if (userDict.TryGetValue(reg.UserId, out var user))
                {
                    dto.Name = user.FullName;
                    dto.Email = user.Email;
                    dto.PhoneNumber = user.PhoneNumber;
                }
                result.Add(dto);
            }
            return result;
        }

        public async Task<bool> ApproveAsync(SellerRegistrationApproveDTO dto)
        {
            var reg = await _sellerRepo.FindOneAsync(r => r.Id == dto.RegistrationId);
            if (reg == null) return false;

            // Validation: Khi approve = true thì phải có license dates
            if (dto.Approve)
            {
                if (!dto.LicenseEffectiveDate.HasValue || !dto.LicenseExpiryDate.HasValue)
                {
                    throw new ArgumentException("License dates are required when approving registration.");
                }
                if (dto.LicenseExpiryDate <= dto.LicenseEffectiveDate)
                {
                    throw new ArgumentException("License expiry date must be after effective date.");
                }
            }

            reg.Status = dto.Approve ? "Approved" : "Rejected";
            reg.RejectionReason = dto.Approve ? null : dto.RejectionReason;
            reg.UpdatedAt = DateTime.Now;
            
            // Chỉ cập nhật license dates khi approve
            if (dto.Approve)
            {
                reg.LicenseEffectiveDate = dto.LicenseEffectiveDate;
                reg.LicenseExpiryDate = dto.LicenseExpiryDate;
            }
            
            await _sellerRepo.UpdateAsync(reg.Id!, reg);
            if (dto.Approve)
            {
                var store = new Store
                {
                    Name = reg.StoreName,
                    Address = reg.StoreAddress,
                    MarketId = reg.MarketId,
                    SellerId = reg.UserId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "Active"
                };
                await _storeRepo.CreateAsync(store);

                // Cập nhật Role của user thành Seller
                var user = await _userRepo.FindOneAsync(u => u.Id == reg.UserId);
                if (user != null && user.Role != "Seller")
                {
                    user.Role = "Seller";
                    await _userRepo.UpdateAsync(user.Id!, user);
                }
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
