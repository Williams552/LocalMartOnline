using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Payment;
using LocalMartOnline.Repositories;

namespace LocalMartOnline.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<MarketFeePayment> _paymentRepo;
        private readonly IRepository<MarketFee> _marketFeeRepo;
        private readonly IRepository<MarketFeeType> _marketFeeTypeRepo;
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<Market> _marketRepo;

        public VnPayService(
            IConfiguration configuration,
            IRepository<MarketFeePayment> paymentRepo,
            IRepository<MarketFee> marketFeeRepo,
            IRepository<MarketFeeType> marketFeeTypeRepo,
            IRepository<Store> storeRepo,
            IRepository<Market> marketRepo)
        {
            _configuration = configuration;
            _paymentRepo = paymentRepo;
            _marketFeeRepo = marketFeeRepo;
            _marketFeeTypeRepo = marketFeeTypeRepo;
            _storeRepo = storeRepo;
            _marketRepo = marketRepo;
        }
        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneId = _configuration["TimeZoneId"] ?? "UTC";
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var convertedTime = TimeZoneInfo.ConvertTime(timeNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var request = context.Request;
            var domain = $"{request.Scheme}://{request.Host}";
            var urlCallBack = domain + _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? "");
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? "");
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? "");
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", convertedTime.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"] ?? "");
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"] ?? "");
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType ?? string.Empty);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"] ?? "", _configuration["Vnpay:HashSecret"] ?? "");

            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"] ?? string.Empty);

            return response;
        }

        // MarketFeePayment methods
        public async Task<List<PendingPaymentDto>> GetPendingPaymentsAsync(string sellerId)
        {
            // Lấy store của seller
            var stores = await _storeRepo.FindManyAsync(s => s.SellerId == sellerId);
            var store = stores.FirstOrDefault();
            if (store == null) return new List<PendingPaymentDto>();

            // Lấy tất cả payments của seller (không chỉ pending để có thể thấy các status khác)
            var allPayments = await _paymentRepo.FindManyAsync(p => p.SellerId == sellerId);

            var allMarketFees = await _marketFeeRepo.GetAllAsync();
            var allMarketFeeTypes = await _marketFeeTypeRepo.GetAllAsync();
            var allMarkets = await _marketRepo.GetAllAsync();

            var pendingPayments = new List<PendingPaymentDto>();

            foreach (var payment in allPayments)
            {
                var marketFee = allMarketFees.FirstOrDefault(mf => mf.Id == payment.FeeId);
                if (marketFee == null) continue;

                var feeType = allMarketFeeTypes.FirstOrDefault(ft => ft.Id == marketFee.MarketFeeTypeId);
                var market = allMarkets.FirstOrDefault(m => m.Id == marketFee.MarketId);

                var isOverdue = payment.DueDate < DateTime.Now;
                var daysOverdue = isOverdue ? (DateTime.Now - payment.DueDate).Days : 0;

                pendingPayments.Add(new PendingPaymentDto
                {
                    PaymentId = payment.PaymentId,
                    FeeTypeName = feeType?.FeeType ?? "Unknown",
                    MarketName = market?.Name ?? "Unknown",
                    Amount = payment.Amount,
                    DueDate = payment.DueDate,
                    PaymentStatus = payment.PaymentStatus.ToString(), // Thêm PaymentStatus
                    IsOverdue = isOverdue,
                    DaysOverdue = daysOverdue
                });
            }

            return pendingPayments.OrderBy(p => p.DueDate).ToList();
        }

        public async Task<CreatePaymentUrlResponseDto> CreateMarketFeePaymentUrlAsync(CreatePaymentUrlRequestDto request, HttpContext context)
        {
            try
            {
                // Tìm payment record
                var payment = await _paymentRepo.GetByIdAsync(request.PaymentId);
                if (payment == null)
                {
                    throw new InvalidOperationException("Không tìm thấy thanh toán");
                }

                if (payment.PaymentStatus != MarketFeePaymentStatus.Pending)
                {
                    throw new InvalidOperationException("Thanh toán này đã được xử lý");
                }

                // Optimize timezone handling
                var timeZoneId = _configuration["TimeZoneId"] ?? "SE Asia Standard Time";
                TimeZoneInfo timeZoneById;
                try
                {
                    timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
                catch
                {
                    // Fallback to UTC if timezone not found
                    timeZoneById = TimeZoneInfo.Utc;
                }

                var timeNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                var convertedTime = TimeZoneInfo.ConvertTime(timeNow, timeZoneById);
                
                var pay = new VnPayLibrary();
                var httpRequest = context.Request;
                var domain = $"{httpRequest.Scheme}://{httpRequest.Host}";
                var urlCallBack = domain + "/api/VnPay/marketfee-callback";

                // Build VnPay request efficiently
                // Tạo unique TxnRef để tránh duplicate transaction trên VnPay
                var uniqueTxnRef = $"{request.PaymentId}_{DateTime.Now:yyyyMMddHHmmss}";
                
                var vnpData = new Dictionary<string, string>
                {
                    ["vnp_Version"] = _configuration["Vnpay:Version"] ?? "2.1.0",
                    ["vnp_Command"] = _configuration["Vnpay:Command"] ?? "pay",
                    ["vnp_TmnCode"] = _configuration["Vnpay:TmnCode"] ?? "",
                    ["vnp_Amount"] = ((int)(payment.Amount * 100)).ToString(),
                    ["vnp_CreateDate"] = convertedTime.ToString("yyyyMMddHHmmss"),
                    ["vnp_CurrCode"] = _configuration["Vnpay:CurrCode"] ?? "VND",
                    ["vnp_IpAddr"] = pay.GetIpAddress(context),
                    ["vnp_Locale"] = _configuration["Vnpay:Locale"] ?? "vn",
                    ["vnp_OrderInfo"] = "Thanh toan phi cho thue", // Đơn giản hóa, tránh ký tự đặc biệt
                    ["vnp_OrderType"] = "other",
                    ["vnp_ReturnUrl"] = urlCallBack,
                    ["vnp_TxnRef"] = uniqueTxnRef // Sử dụng unique TxnRef
                };

                foreach (var item in vnpData)
                {
                    pay.AddRequestData(item.Key, item.Value);
                }

                var hashSecret = _configuration["Vnpay:HashSecret"] ?? "";
                var baseUrl = _configuration["Vnpay:BaseUrl"] ?? "";
                
                var paymentUrl = pay.CreateRequestUrl(baseUrl, hashSecret);

                Console.WriteLine($"DEBUG: Generated payment URL: {paymentUrl}");
                Console.WriteLine($"DEBUG: Successfully created payment URL for PaymentId: {request.PaymentId}");

                return new CreatePaymentUrlResponseDto
                {
                    PaymentUrl = paymentUrl,
                    PaymentId = request.PaymentId
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> ProcessMarketFeePaymentCallbackAsync(IQueryCollection collections)
        {
            try
            {
                var hashSecret = _configuration["Vnpay:HashSecret"] ?? string.Empty;
                
                var pay = new VnPayLibrary();
                var response = pay.GetFullResponseData(collections, hashSecret);

                if (response.Success && response.VnPayResponseCode == "00")
                {
                    // Lấy paymentId từ TxnRef - parse từ unique format
                    var txnRef = collections["vnp_TxnRef"].ToString();
                    var paymentId = ExtractPaymentIdFromTxnRef(txnRef);
                    var transactionId = collections["vnp_TransactionNo"].ToString();
                    var amountString = collections["vnp_Amount"].ToString();
                    
                    if (!decimal.TryParse(amountString, out var amountInCents))
                    {
                        return false;
                    }
                    
                    var amount = amountInCents / 100;

                    // Cập nhật payment status
                    var payment = await _paymentRepo.GetByIdAsync(paymentId);
                    if (payment != null)
                    {
                        if (payment.PaymentStatus == MarketFeePaymentStatus.Pending)
                        {
                            payment.PaymentStatus = MarketFeePaymentStatus.Completed;
                            payment.PaymentDate = DateTime.Now;

                            await _paymentRepo.UpdateAsync(paymentId, payment);
                            return true;
                        }
                        else
                        {
                            return true; // Already processed, consider as success
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // Update payment status to Failed for non-successful payments
                    var txnRef = collections["vnp_TxnRef"].ToString();
                    var paymentId = ExtractPaymentIdFromTxnRef(txnRef);
                    if (!string.IsNullOrEmpty(paymentId))
                    {
                        var payment = await _paymentRepo.GetByIdAsync(paymentId);
                        if (payment != null && payment.PaymentStatus == MarketFeePaymentStatus.Pending)
                        {
                            payment.PaymentStatus = MarketFeePaymentStatus.Failed;
                            payment.PaymentDate = DateTime.Now;
                            await _paymentRepo.UpdateAsync(paymentId, payment);
                        }
                    }
                    
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // Helper method để extract PaymentId từ unique TxnRef
        private string ExtractPaymentIdFromTxnRef(string txnRef)
        {
            try
            {
                // TxnRef format: {PaymentId}_{yyyyMMddHHmmss}
                if (string.IsNullOrEmpty(txnRef))
                    return "";

                var lastUnderscoreIndex = txnRef.LastIndexOf('_');
                if (lastUnderscoreIndex > 0)
                {
                    var paymentId = txnRef.Substring(0, lastUnderscoreIndex);
                    return paymentId;
                }

                // Fallback: if no underscore found, return the whole string
                return txnRef;
            }
            catch
            {
                return "";
            }
        }
    }
}
