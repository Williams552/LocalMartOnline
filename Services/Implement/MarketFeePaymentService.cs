using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LocalMartOnline.Services.Implement
{
    public class MarketFeePaymentService : IMarketFeePaymentService
    {
        private readonly IRepository<MarketFeePayment> _repo;
        private readonly IRepository<MarketFee> _marketFeeRepo;
        private readonly IRepository<MarketFeeType> _marketFeeTypeRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Market> _marketRepo;
        private readonly IMapper _mapper;
        
        public MarketFeePaymentService(
            IRepository<MarketFeePayment> repo, 
            IRepository<MarketFee> marketFeeRepo,
            IRepository<MarketFeeType> marketFeeTypeRepo,
            IRepository<Store> storeRepo,
            IRepository<User> userRepo,
            IRepository<Market> marketRepo,
            IMapper mapper)
        {
            _repo = repo;
            _marketFeeRepo = marketFeeRepo;
            _marketFeeTypeRepo = marketFeeTypeRepo;
            _storeRepo = storeRepo;
            _userRepo = userRepo;
            _marketRepo = marketRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MarketFeePaymentDto>> GetPaymentsBySellerAsync(string sellerId)
        {
            var payments = await _repo.FindManyAsync(p => p.SellerId == sellerId);
            var allMarketFees = await _marketFeeRepo.GetAllAsync();
            var allMarketFeeTypes = await _marketFeeTypeRepo.GetAllAsync();
            var allUsers = await _userRepo.GetAllAsync();
            var allMarkets = await _marketRepo.GetAllAsync();
            
            var result = new List<MarketFeePaymentDto>();
            
            foreach (var payment in payments)
            {
                var marketFee = allMarketFees.FirstOrDefault(mf => mf.Id == payment.FeeId);
                var feeType = marketFee != null ? allMarketFeeTypes.FirstOrDefault(ft => ft.Id == marketFee.MarketFeeTypeId) : null;
                var seller = allUsers.FirstOrDefault(u => u.Id == payment.SellerId);
                var market = marketFee != null ? allMarkets.FirstOrDefault(m => m.Id == marketFee.MarketId) : null;
                
                result.Add(new MarketFeePaymentDto
                {
                    PaymentId = payment.PaymentId,
                    SellerId = payment.SellerId,
                    SellerName = seller?.Username ?? string.Empty,
                    MarketName = market?.Name ?? string.Empty,
                    FeeId = payment.FeeId,
                    FeeName = marketFee?.Name ?? string.Empty,
                    FeeTypeName = feeType?.FeeType ?? string.Empty,
                    Amount = payment.Amount,
                    PaymentStatus = payment.PaymentStatus.ToString(),
                    CreatedAt = payment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            
            return result;
        }

        public async Task<MarketFeePaymentDto?> GetPaymentByIdAsync(string paymentId)
        {
            var payment = await _repo.GetByIdAsync(paymentId);
            if (payment == null) return null;
            
            var allMarketFees = await _marketFeeRepo.GetAllAsync();
            var allMarketFeeTypes = await _marketFeeTypeRepo.GetAllAsync();
            var allUsers = await _userRepo.GetAllAsync();
            var allMarkets = await _marketRepo.GetAllAsync();
            
            var marketFee = allMarketFees.FirstOrDefault(mf => mf.Id == payment.FeeId);
            var feeType = marketFee != null ? allMarketFeeTypes.FirstOrDefault(ft => ft.Id == marketFee.MarketFeeTypeId) : null;
            var seller = allUsers.FirstOrDefault(u => u.Id == payment.SellerId);
            var market = marketFee != null ? allMarkets.FirstOrDefault(m => m.Id == marketFee.MarketId) : null;
            
            return new MarketFeePaymentDto
            {
                PaymentId = payment.PaymentId,
                SellerId = payment.SellerId,
                SellerName = seller?.Username ?? string.Empty,
                MarketName = market?.Name ?? string.Empty,
                FeeId = payment.FeeId,
                FeeName = marketFee?.Name ?? string.Empty,
                FeeTypeName = feeType?.FeeType ?? string.Empty,
                Amount = payment.Amount,
                PaymentStatus = payment.PaymentStatus.ToString(),
                CreatedAt = payment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        public async Task<MarketFeePaymentDto> CreatePaymentAsync(MarketFeePaymentCreateDto dto)
        {
            var payment = _mapper.Map<MarketFeePayment>(dto);
            payment.PaymentStatus = MarketFeePaymentStatus.Pending;
            payment.CreatedAt = DateTime.Now;
            await _repo.CreateAsync(payment);
            return _mapper.Map<MarketFeePaymentDto>(payment);
        }

        public async Task<bool> UpdatePaymentStatusAsync(string paymentId, string status)
        {
            var payment = await _repo.GetByIdAsync(paymentId);
            if (payment == null) return false;
            if (Enum.TryParse<MarketFeePaymentStatus>(status, out var newStatus))
            {
                payment.PaymentStatus = newStatus;
                await _repo.UpdateAsync(paymentId, payment);
                return true;
            }
            return false;
        }

        public async Task<GetSellersPaymentStatusResponseDto> GetSellersPaymentStatusAsync(GetSellersPaymentStatusRequestDto request)
        {
            // Get all stores in the market
            var stores = await _storeRepo.FindManyAsync(s => s.MarketId == request.MarketId);
            var storesList = stores.ToList();
            
            // Get market info
            var market = await _marketRepo.GetByIdAsync(request.MarketId);
            var marketName = market?.Name ?? "Unknown Market";
            
            var sellerPaymentStatuses = new List<SellerPaymentStatusDto>();
            int totalSellers = storesList.Count;
            int completedCount = 0;
            int pendingCount = 0;
            int overdueCount = 0;
            decimal totalAmountDue = 0;

            foreach (var store in storesList)
            {
                // Get seller information
                var seller = await _userRepo.GetByIdAsync(store.SellerId);
                if (seller == null) continue;

                // Get market fees for this market (default to monthly rent fee type)
                var marketFees = await _marketFeeRepo.FindManyAsync(mf => mf.MarketId == request.MarketId);
                var monthlyRentFee = marketFees.FirstOrDefault();
                
                if (monthlyRentFee == null) continue;

                // Get payment record for this seller and fee
                var payment = await _repo.FindOneAsync(p => p.SellerId == store.SellerId && p.FeeId == monthlyRentFee.Id);
                
                var paymentStatus = payment?.PaymentStatus ?? MarketFeePaymentStatus.Pending;
                var amountDue = payment?.Amount ?? monthlyRentFee.Amount;
                
                // Calculate if overdue (assuming monthly payments, check if no payment this month)
                var isOverdue = false;
                var daysOverdue = 0;
                if (paymentStatus != MarketFeePaymentStatus.Completed)
                {
                    var dueDate = new DateTime(request.Year, request.Month, monthlyRentFee.PaymentDay);
                    if (DateTime.Now > dueDate)
                    {
                        isOverdue = true;
                        daysOverdue = (DateTime.Now - dueDate).Days;
                    }
                }

                // Get fee type name
                string feeTypeName = "Phí thuê tháng"; // Default
                if (!string.IsNullOrEmpty(monthlyRentFee.MarketFeeTypeId))
                {
                    var feeType = await _marketFeeTypeRepo.GetByIdAsync(monthlyRentFee.MarketFeeTypeId);
                    if (feeType != null)
                        feeTypeName = feeType.FeeType;
                }

                sellerPaymentStatuses.Add(new SellerPaymentStatusDto
                {
                    SellerId = store.SellerId,
                    SellerName = seller.Username,
                    StoreName = store.Name,
                    MarketName = marketName,
                    MonthlyRentalFee = amountDue,
                    PaymentStatus = paymentStatus.ToString(),
                    FeeTypeName = feeTypeName,
                    IsOverdue = isOverdue,
                    DaysOverdue = daysOverdue,
                    LastPaymentDate = payment?.CreatedAt
                });

                // Update counters
                if (paymentStatus == MarketFeePaymentStatus.Completed)
                    completedCount++;
                else if (isOverdue)
                {
                    overdueCount++;
                    totalAmountDue += amountDue;
                }
                else
                {
                    pendingCount++;
                    totalAmountDue += amountDue;
                }
            }

            return new GetSellersPaymentStatusResponseDto
            {
                Sellers = sellerPaymentStatuses,
                TotalSellers = totalSellers,
                CompletedCount = completedCount,
                PendingCount = pendingCount,
                OverdueCount = overdueCount,
                TotalAmountDue = totalAmountDue
            };
        }

        public async Task<bool> UpdatePaymentStatusByAdminAsync(UpdatePaymentStatusDto dto)
        {
            // Get market fees for this market
            var marketFees = await _marketFeeRepo.FindManyAsync(mf => mf.MarketId == dto.MarketId);
            var monthlyRentFee = marketFees.FirstOrDefault();
            if (monthlyRentFee == null) return false;

            var payment = await _repo.FindOneAsync(p => p.SellerId == dto.SellerId && p.FeeId == monthlyRentFee.Id);
            
            if (payment == null)
            {
                // Create new payment record if doesn't exist
                payment = new MarketFeePayment
                {
                    SellerId = dto.SellerId,
                    FeeId = monthlyRentFee.Id,
                    Amount = monthlyRentFee.Amount,
                    PaymentStatus = MarketFeePaymentStatus.Pending,
                    CreatedAt = DateTime.Now
                };
                await _repo.CreateAsync(payment);
            }

            // Update payment status
            if (Enum.TryParse<MarketFeePaymentStatus>(dto.PaymentStatus, out var newStatus))
            {
                payment.PaymentStatus = newStatus;
                payment.CreatedAt = DateTime.Now; // Update timestamp when status changes

                await _repo.UpdateAsync(payment.PaymentId, payment);
                return true;
            }

            return false;
        }

        public async Task<GetAllMarketFeePaymentsResponseDto> GetAllPaymentsAsync(GetAllMarketFeePaymentsRequestDto request)
        {
            // Build query filters
            var allPayments = await _repo.GetAllAsync();
            var filteredPayments = allPayments.AsEnumerable();

            // Get all related data for joining
            var allStores = await _storeRepo.GetAllAsync();
            var allUsers = await _userRepo.GetAllAsync();
            var allMarkets = await _marketRepo.GetAllAsync();
            var allMarketFees = await _marketFeeRepo.GetAllAsync();
            var allMarketFeeTypes = await _marketFeeTypeRepo.GetAllAsync();

            var paymentDetails = new List<MarketFeePaymentDetailDto>();

            foreach (var payment in filteredPayments)
            {
                // Get related entities
                var marketFee = allMarketFees.FirstOrDefault(mf => mf.Id == payment.FeeId);
                if (marketFee == null) continue;

                var market = allMarkets.FirstOrDefault(m => m.Id == marketFee.MarketId);
                var marketFeeType = allMarketFeeTypes.FirstOrDefault(mft => mft.Id == marketFee.MarketFeeTypeId);
                var seller = allUsers.FirstOrDefault(u => u.Id == payment.SellerId);
                var store = allStores.FirstOrDefault(s => s.SellerId == payment.SellerId);

                if (market == null || seller == null || store == null) continue;

                // Apply filters
                if (!string.IsNullOrEmpty(request.MarketId) && market.Id != request.MarketId) continue;
                if (!string.IsNullOrEmpty(request.FeeTypeId) && marketFee.MarketFeeTypeId != request.FeeTypeId) continue;
                if (!string.IsNullOrEmpty(request.PaymentStatus) && payment.PaymentStatus.ToString() != request.PaymentStatus) continue;

                // Apply search keyword filter
                if (!string.IsNullOrEmpty(request.SearchKeyword))
                {
                    var keyword = request.SearchKeyword.ToLower();
                    var matchesSearch = seller.Username.ToLower().Contains(keyword) ||
                                      store.Name.ToLower().Contains(keyword) ||
                                      market.Name.ToLower().Contains(keyword);
                    if (!matchesSearch) continue;
                }

                // Calculate overdue status
                var isOverdue = payment.DueDate < DateTime.Now && payment.PaymentStatus != MarketFeePaymentStatus.Completed;
                var daysOverdue = isOverdue ? (DateTime.Now - payment.DueDate).Days : 0;

                paymentDetails.Add(new MarketFeePaymentDetailDto
                {
                    PaymentId = payment.PaymentId,
                    MarketName = market.Name,
                    MarketId = market.Id ?? string.Empty,
                    FeeTypeName = marketFeeType?.FeeType ?? "Unknown",
                    FeeTypeId = marketFee.MarketFeeTypeId,
                    FeeName = marketFee.Name,
                    FeeAmount = payment.Amount,
                    StoreName = store.Name,
                    StoreId = store.Id ?? string.Empty,
                    SellerName = seller.Username,
                    SellerId = seller.Id ?? string.Empty,
                    DueDate = payment.DueDate,
                    PaymentStatus = payment.PaymentStatus.ToString(),
                    PaymentDate = payment.PaymentDate,
                    CreatedAt = payment.CreatedAt,
                    AdminNotes = payment.AdminNotes,
                    IsOverdue = isOverdue,
                    DaysOverdue = daysOverdue
                });
            }

            // Apply pagination
            var totalCount = paymentDetails.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var pagedPayments = paymentDetails
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new GetAllMarketFeePaymentsResponseDto
            {
                Payments = pagedPayments,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
