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
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var request = context.Request;
            var domain = $"{request.Scheme}://{request.Host}";
            var urlCallBack = domain + _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? "");
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? "");
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? "");
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
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
        public async Task<IEnumerable<PendingPaymentDto>> GetPendingPaymentsAsync(string sellerId)
        {
            // Lấy store của seller
            var stores = await _storeRepo.FindManyAsync(s => s.SellerId == sellerId);
            var store = stores.FirstOrDefault();
            if (store == null) return Enumerable.Empty<PendingPaymentDto>();

            // Lấy các payments pending của seller
            var allPayments = await _paymentRepo.FindManyAsync(p => 
                p.SellerId == sellerId && 
                p.PaymentStatus == MarketFeePaymentStatus.Pending);

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
                    IsOverdue = isOverdue,
                    DaysOverdue = daysOverdue
                });
            }

            return pendingPayments.OrderBy(p => p.DueDate);
        }

        public async Task<string> CreateMarketFeePaymentUrlAsync(string paymentId, HttpContext context)
        {
            // Tìm payment record
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null)
                throw new InvalidOperationException("Không tìm thấy thanh toán");

            if (payment.PaymentStatus != MarketFeePaymentStatus.Pending)
                throw new InvalidOperationException("Thanh toán này đã được xử lý");

            var timeZoneId = _configuration["TimeZoneId"] ?? "UTC";
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, timeZoneById);
            
            var pay = new VnPayLibrary();
            var request = context.Request;
            var domain = $"{request.Scheme}://{request.Host}";
            var urlCallBack = domain + "/api/VnPay/marketfee-callback";

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? "");
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? "");
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? "");
            pay.AddRequestData("vnp_Amount", ((int)(payment.Amount * 100)).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"] ?? "");
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"] ?? "");
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toan phi thue thang - {payment.Amount} VND");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", paymentId);

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"] ?? "", _configuration["Vnpay:HashSecret"] ?? "");

            return paymentUrl;
        }

        public async Task<bool> ProcessMarketFeePaymentCallbackAsync(IQueryCollection collections)
        {
            try
            {
                var pay = new VnPayLibrary();
                var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"] ?? string.Empty);

                if (response.Success && response.VnPayResponseCode == "00")
                {
                    // Lấy paymentId từ TxnRef
                    var paymentId = collections["vnp_TxnRef"].ToString();
                    var transactionId = collections["vnp_TransactionNo"].ToString();
                    var amount = decimal.Parse(collections["vnp_Amount"].ToString()) / 100;

                    // Cập nhật payment status
                    var payment = await _paymentRepo.GetByIdAsync(paymentId);
                    if (payment != null)
                    {
                        payment.PaymentStatus = MarketFeePaymentStatus.Completed;
                        payment.PaymentDate = DateTime.Now;

                        await _paymentRepo.UpdateAsync(paymentId, payment);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
