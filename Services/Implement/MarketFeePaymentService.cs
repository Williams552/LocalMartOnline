using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.Store;
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

        // Methods moved from StoreService
        public async Task<GetAllStoresWithPaymentResponseDto> GetAllStoresWithPaymentInfoAsync(GetAllStoresWithPaymentRequestDto request)
        {
            // Determine target month and year
            var targetMonth = request.Month ?? DateTime.Now.Month;
            var targetYear = request.Year ?? DateTime.Now.Year;
            
            // Generate monthly payments first to ensure data is up-to-date
            await GenerateMonthlyPaymentsAsync(targetMonth, targetYear);

            // Get all payments for the target month/year first (payment-centric approach)
            var allPayments = await _repo.GetAllAsync();
            var filteredPayments = allPayments.Where(p => 
                p.DueDate.Month == targetMonth && 
                p.DueDate.Year == targetYear);

            // Apply payment status filter if provided
            if (!string.IsNullOrEmpty(request.PaymentStatus))
                filteredPayments = filteredPayments.Where(p => p.PaymentStatus.ToString() == request.PaymentStatus);

            // Get related data
            var allUsers = await _userRepo.GetAllAsync();
            var allStores = await _storeRepo.GetAllAsync();
            var allMarkets = await _marketRepo.GetAllAsync();
            var allMarketFees = await _marketFeeRepo.GetAllAsync();
            var allMarketFeeTypes = await _marketFeeTypeRepo.GetAllAsync();

            var storesWithPaymentInfo = new List<StoreWithPaymentInfoDto>();

            foreach (var payment in filteredPayments)
            {
                // Get related entities using payment data
                var seller = allUsers.FirstOrDefault(u => u.Id == payment.SellerId);
                var store = allStores.FirstOrDefault(s => s.SellerId == payment.SellerId);
                var marketFee = allMarketFees.FirstOrDefault(mf => mf.Id == payment.FeeId);
                
                if (seller == null || store == null || marketFee == null) continue;

                var market = allMarkets.FirstOrDefault(m => m.Id == store.MarketId);
                var marketFeeType = allMarketFeeTypes.FirstOrDefault(ft => ft.Id == marketFee.MarketFeeTypeId);
                
                if (market == null || marketFeeType == null) continue;

                // Apply market filter if provided
                if (!string.IsNullOrEmpty(request.MarketId) && store.MarketId != request.MarketId)
                    continue;

                // Apply fee type filter if provided
                if (!string.IsNullOrEmpty(request.FeeTypeId) && marketFee.MarketFeeTypeId != request.FeeTypeId)
                    continue;

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(request.SearchKeyword))
                {
                    var keyword = request.SearchKeyword.ToLower();
                    if (!store.Name.ToLower().Contains(keyword) && 
                        !seller.Username.ToLower().Contains(keyword))
                        continue;
                }

                // Calculate overdue status
                var isOverdue = payment.DueDate < DateTime.Now && payment.PaymentStatus != MarketFeePaymentStatus.Completed;
                var daysOverdue = isOverdue ? (DateTime.Now - payment.DueDate).Days : 0;

                var storeInfo = new StoreWithPaymentInfoDto
                {
                    PaymentId = payment.PaymentId,
                    StoreName = store.Name,
                    SellerName = seller.FullName,
                    SellerPhone = seller.PhoneNumber ?? string.Empty,
                    MarketName = market.Name,
                    FeeTypeName = marketFeeType.FeeType,
                    MonthlyRentalFee = payment.Amount,
                    DueDate = payment.DueDate,
                    PaymentStatus = payment.PaymentStatus.ToString(),
                    PaymentDate = payment.PaymentDate,
                    IsOverdue = isOverdue,
                    DaysOverdue = daysOverdue
                };

                storesWithPaymentInfo.Add(storeInfo);
            }

            // Apply pagination
            var totalCount = storesWithPaymentInfo.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var pagedStores = storesWithPaymentInfo
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new GetAllStoresWithPaymentResponseDto
            {
                Stores = pagedStores,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> UpdateStorePaymentStatusAsync(string paymentId, UpdateStorePaymentStatusDto dto)
        {
            var payment = await _repo.GetByIdAsync(paymentId);
            if (payment == null) return false;

            if (Enum.TryParse<MarketFeePaymentStatus>(dto.PaymentStatus, out var newStatus))
            {
                payment.PaymentStatus = newStatus;
                payment.PaymentDate = dto.PaymentDate;

                await _repo.UpdateAsync(payment.PaymentId, payment);
                return true;
            }

            return false;
        }

        private async Task CreateMonthlyPaymentAsync(string sellerId, string feeId, decimal amount, DateTime dueDate)
        {
            var newPayment = new MarketFeePayment
            {
                SellerId = sellerId,
                FeeId = feeId,
                Amount = amount,
                DueDate = dueDate,
                PaymentStatus = MarketFeePaymentStatus.Pending,
                CreatedAt = DateTime.Now
            };

            await _repo.CreateAsync(newPayment);
        }

        public async Task<int> GenerateMonthlyPaymentsAsync(int? month = null, int? year = null)
        {
            try
            {
                // Auto-detect current month/year if not provided
                var targetMonth = month ?? DateTime.Now.Month;
                var targetYear = year ?? DateTime.Now.Year;

                // Get all active stores
                var allStores = await _storeRepo.FindManyAsync(s => s.Status == "Open" || s.Status == "Closed");
                if (!allStores.Any()) return 0;

                // Get related data
                var allMarkets = await _marketRepo.GetAllAsync();
                var allMarketFees = await _marketFeeRepo.GetAllAsync();
                var allMarketFeeTypes = await _marketFeeTypeRepo.GetAllAsync();
                var allPayments = await _repo.GetAllAsync();

                // Find "Phí Thuê Tháng" fee type
                var monthlyRentFeeType = allMarketFeeTypes.FirstOrDefault(ft => 
                    ft.FeeType.Equals("Phí Thuê Tháng", StringComparison.OrdinalIgnoreCase) && 
                    !ft.IsDeleted);

                if (monthlyRentFeeType == null) return 0;

                int createdCount = 0;

                foreach (var store in allStores)
                {
                    // Get MarketFee for this market with "Phí Thuê Tháng" type
                    var monthlyRentMarketFee = allMarketFees.FirstOrDefault(mf => 
                        mf.MarketId == store.MarketId && 
                        mf.MarketFeeTypeId == monthlyRentFeeType.Id);

                    if (monthlyRentMarketFee == null) continue;

                    // Check if payment already exists for this month/year - DUPLICATE CHECK
                    var existingPayment = allPayments.FirstOrDefault(p => 
                        p.SellerId == store.SellerId && 
                        p.FeeId == monthlyRentMarketFee.Id &&
                        p.DueDate.Month == targetMonth &&
                        p.DueDate.Year == targetYear);

                    if (existingPayment != null) continue; // Skip if already exists - PREVENT DUPLICATE

                    // Calculate due date from PaymentDay in MarketFee
                    var dueDate = new DateTime(targetYear, targetMonth, monthlyRentMarketFee.PaymentDay);
                    
                    // Ensure dueDate is valid (handle cases where PaymentDay > days in month)
                    var daysInMonth = DateTime.DaysInMonth(targetYear, targetMonth);
                    if (monthlyRentMarketFee.PaymentDay > daysInMonth)
                    {
                        dueDate = new DateTime(targetYear, targetMonth, daysInMonth);
                    }

                    // Create new payment
                    await CreateMonthlyPaymentAsync(store.SellerId, monthlyRentMarketFee.Id, monthlyRentMarketFee.Amount, dueDate);
                    createdCount++;
                }

                return createdCount;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khi tạo thanh toán tháng {month ?? DateTime.Now.Month}/{year ?? DateTime.Now.Year}: {ex.Message}");
            }
        }

        public async Task<AdminCreatePaymentResponseDto> CreatePaymentByAdminAsync(AdminCreatePaymentDto dto)
        {
            try
            {
                // Validate user exists
                var user = await _userRepo.GetByIdAsync(dto.UserId);
                if (user == null)
                    throw new InvalidOperationException("Không tìm thấy người dùng với ID này");

                // Find store by sellerId (UserId is sellerId in this context)
                var stores = await _storeRepo.FindManyAsync(s => s.SellerId == dto.UserId);
                var store = stores.FirstOrDefault();
                if (store == null)
                    throw new InvalidOperationException("Không tìm thấy cửa hàng nào cho người dùng này");

                // Validate fee type exists
                var feeType = await _marketFeeTypeRepo.GetByIdAsync(dto.FeeTypeId);
                if (feeType == null)
                    throw new InvalidOperationException("Không tìm thấy loại phí với ID này");

                // Find MarketFee by MarketId (from Store) + FeeTypeId
                var allMarketFees = await _marketFeeRepo.GetAllAsync();
                var marketFee = allMarketFees.FirstOrDefault(mf => 
                    mf.MarketId == store.MarketId && 
                    mf.MarketFeeTypeId == dto.FeeTypeId);
                
                if (marketFee == null)
                    throw new InvalidOperationException($"Không tìm thấy cấu hình phí '{feeType.FeeType}' cho chợ này");

                // Get market info
                var market = await _marketRepo.GetByIdAsync(store.MarketId);
                if (market == null)
                    throw new InvalidOperationException("Không tìm thấy thông tin chợ");

                // Debug: Log tất cả MarketFees để kiểm tra
                Console.WriteLine($"DEBUG: Total MarketFees: {allMarketFees.Count()}");
                Console.WriteLine($"DEBUG: Looking for FeeTypeId: {dto.FeeTypeId} in MarketId: {store.MarketId}");
                
                foreach (var fee in allMarketFees.Take(5)) // Log 5 fees đầu tiên
                {
                    Console.WriteLine($"DEBUG: MarketFee - Id: {fee.Id}, Name: {fee.Name}, MarketId: {fee.MarketId}, FeeTypeId: {fee.MarketFeeTypeId}");
                }

                // Calculate due date from PaymentDay in MarketFee (current month)
                var currentDate = DateTime.Now;
                var dueDate = new DateTime(currentDate.Year, currentDate.Month, marketFee.PaymentDay);
                
                // Ensure dueDate is valid (handle cases where PaymentDay > days in month)
                var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                if (marketFee.PaymentDay > daysInMonth)
                {
                    dueDate = new DateTime(currentDate.Year, currentDate.Month, daysInMonth);
                }

                // Check for duplicate payment for this user + fee in current month/year
                var existingPayment = await _repo.FindOneAsync(p => 
                    p.SellerId == dto.UserId && 
                    p.FeeId == marketFee.Id &&
                    p.DueDate.Month == currentDate.Month &&
                    p.DueDate.Year == currentDate.Year);
                
                if (existingPayment != null)
                    throw new InvalidOperationException($"Phí '{marketFee.Name}' tháng {currentDate.Month}/{currentDate.Year} cho người dùng này đã tồn tại");

                // Create new payment
                var newPayment = new MarketFeePayment
                {
                    SellerId = dto.UserId,
                    FeeId = marketFee.Id, // Sử dụng MarketFee.Id tìm được
                    Amount = marketFee.Amount,
                    DueDate = dueDate,
                    PaymentStatus = MarketFeePaymentStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                await _repo.CreateAsync(newPayment);

                // Return response DTO
                return new AdminCreatePaymentResponseDto
                {
                    PaymentId = newPayment.PaymentId,
                    UserName = user.Username,
                    FeeName = marketFee.Name,
                    FeeTypeName = feeType.FeeType,
                    MarketName = market.Name,
                    Amount = newPayment.Amount,
                    DueDate = newPayment.DueDate,
                    PaymentStatus = newPayment.PaymentStatus.ToString(),
                    CreatedAt = newPayment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khi tạo thanh toán: {ex.Message}");
            }
        }

        public async Task<AdminCreatePaymentForMarketResponseDto> CreatePaymentForMarketAsync(AdminCreatePaymentForMarketDto dto)
        {
            try
            {
                // Validate market exists
                var market = await _marketRepo.GetByIdAsync(dto.MarketId);
                if (market == null)
                    throw new InvalidOperationException("Không tìm thấy chợ với ID này");

                // Validate fee type exists
                var feeType = await _marketFeeTypeRepo.GetByIdAsync(dto.FeeTypeId);
                if (feeType == null)
                    throw new InvalidOperationException("Không tìm thấy loại phí với ID này");

                // Find MarketFee by MarketId + FeeTypeId
                var allMarketFees = await _marketFeeRepo.GetAllAsync();
                var marketFee = allMarketFees.FirstOrDefault(mf => 
                    mf.MarketId == dto.MarketId && 
                    mf.MarketFeeTypeId == dto.FeeTypeId);
                
                if (marketFee == null)
                    throw new InvalidOperationException($"Không tìm thấy cấu hình phí '{feeType.FeeType}' cho chợ '{market.Name}'");

                Console.WriteLine($"DEBUG: Found MarketFee: {marketFee.Id}, Name: {marketFee.Name}");

                // Calculate due date from PaymentDay in MarketFee (current month)
                var currentDate = DateTime.Now;
                var dueDate = new DateTime(currentDate.Year, currentDate.Month, marketFee.PaymentDay);
                
                // Ensure dueDate is valid (handle cases where PaymentDay > days in month)
                var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                if (marketFee.PaymentDay > daysInMonth)
                {
                    dueDate = new DateTime(currentDate.Year, currentDate.Month, daysInMonth);
                }

                // Get existing payments to avoid duplicates for this fee in current month/year
                var existingPayments = await _repo.FindManyAsync(p => p.FeeId == marketFee.Id &&
                    p.DueDate.Month == currentDate.Month &&
                    p.DueDate.Year == currentDate.Year);

                // Get all active stores in the market
                var stores = await _storeRepo.FindManyAsync(s => s.MarketId == dto.MarketId && 
                    (s.Status == "Open" || s.Status == "Closed"));
                var storesList = stores.ToList();

                if (!storesList.Any())
                    throw new InvalidOperationException("Không tìm thấy cửa hàng nào trong chợ này");
                var existingPaymentSellerIds = existingPayments.Select(p => p.SellerId).ToHashSet();

                var failedSellerIds = new List<string>();
                var skippedSellerIds = new List<string>();
                int successfulCount = 0;

                foreach (var store in storesList)
                {
                    try
                    {
                        // Skip if payment already exists for this seller and fee on the same due date
                        if (existingPaymentSellerIds.Contains(store.SellerId))
                        {
                            skippedSellerIds.Add(store.SellerId);
                            continue;
                        }

                        // Validate seller exists
                        var seller = await _userRepo.GetByIdAsync(store.SellerId);
                        if (seller == null)
                        {
                            failedSellerIds.Add(store.SellerId);
                            continue;
                        }

                        // Create new payment for this seller
                        var newPayment = new MarketFeePayment
                        {
                            SellerId = store.SellerId,
                            FeeId = marketFee.Id,
                            Amount = marketFee.Amount, // Lấy amount từ MarketFee
                            DueDate = dueDate,
                            PaymentStatus = MarketFeePaymentStatus.Pending,
                            CreatedAt = DateTime.Now
                        };

                        await _repo.CreateAsync(newPayment);
                        successfulCount++;
                    }
                    catch (Exception)
                    {
                        failedSellerIds.Add(store.SellerId);
                    }
                }

                return new AdminCreatePaymentForMarketResponseDto
                {
                    MarketName = market.Name,
                    FeeName = marketFee.Name,
                    FeeTypeName = feeType.FeeType,
                    Amount = marketFee.Amount, // Lấy amount từ MarketFee
                    DueDate = dueDate,
                    TotalSellersAffected = storesList.Count,
                    SuccessfulPaymentsCreated = successfulCount,
                    FailedSellerIds = failedSellerIds,
                    SkippedSellerIds = skippedSellerIds,
                    CreatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khi tạo thanh toán cho chợ: {ex.Message}");
            }
        }
    }
}
