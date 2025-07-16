// Extension methods for registering MongoDB and repositories into the DI container
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services;
using LocalMartOnline.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Services.Implement;

namespace LocalMartOnline.Services
{
    public static class ServiceCollectionExtensions
    {
        // Đăng ký MongoDB client, database, MongoDBService và các repository vào DI container
        public static IServiceCollection AddMongoDbAndRepositories(this IServiceCollection services)
        {
            services.AddSingleton(sp => {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration["MongoDB:ConnectionString"];
                return new MongoDB.Driver.MongoClient(connectionString);
            });
            services.AddSingleton(sp => {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var client = sp.GetRequiredService<MongoDB.Driver.MongoClient>();
                var dbName = configuration["MongoDB:DatabaseName"];
                return client.GetDatabase(dbName);
            });
            services.AddScoped<MongoDBService>();
            services.AddScoped<IRepository<User>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<User>(mongoService, "Users");
            });
            services.AddScoped<IRepository<SellerRegistration>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<SellerRegistration>(mongoService, "SellerRegistrations");
            });
            services.AddScoped<IRepository<ProxyShopperRegistration>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<ProxyShopperRegistration>(mongoService, "ProxyShopperRegistrations");
            });
            services.AddScoped<IRepository<MarketFee>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<MarketFee>(mongoService, "MarketFees");
            });
            services.AddScoped<IRepository<MarketFeePayment>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<MarketFeePayment>(mongoService, "MarketFeePayments");
            });
            services.AddScoped<IRepository<FastBargain>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<FastBargain>(mongoService, "FastBargains");
            });
            services.AddScoped<IFastBargainService, Implement.FastBargainService>();
            
            // Add Cart Service here
            services.AddScoped<ICartService, CartService>();
            
            services.AddStackExchangeRedisCache(options =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                options.Configuration = configuration["Redis:ConnectionString"];
            });
            services.AddScoped<IRepository<Product>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<Product>(mongoService, "Products");
            });
            services.AddScoped<IRepository<Order>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<Order>(mongoService, "Orders");
            });
            services.AddScoped<IRepository<Store>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<Store>(mongoService, "Stores");
            });
            services.AddScoped<IRepository<Category>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<Category>(mongoService, "Categories");
            });
            services.AddScoped<IRepository<CategoryRegistration>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<CategoryRegistration>(mongoService, "CategoryRegistrations");
            });
            services.AddScoped<IRepository<ProductImage>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<ProductImage>(mongoService, "ProductImages");
            });
            services.AddScoped<IRepository<OrderItem>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<OrderItem>(mongoService, "OrderItems");
            });
            services.AddScoped<IRepository<StoreFollow>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<StoreFollow>(mongoService, "StoreFollows");
            });
            services.AddScoped<IRepository<ProductUnit>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<ProductUnit>(mongoService, "ProductUnits");
            });
            services.AddScoped<IRepository<Favorite>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<Favorite>(mongoService, "Favorites");
            });
            services.AddScoped<IRepository<Notification>>(sp => {
                var mongoService = sp.GetRequiredService<MongoDBService>();
                return new Repository<Notification>(mongoService, "Notifications");
            });
            return services;
        }

        public static IServiceCollection AddLocalMartServices(this IServiceCollection services)
        {
            // Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IVnPayService, VnPayService>();

            // Business Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICategoryRegistrationService, CategoryRegistrationService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            // ICartService đã được đăng ký trong AddMongoDbAndRepositories
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IMarketFeeService, MarketFeeService>();
            services.AddScoped<IMarketFeePaymentService, MarketFeePaymentService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IMarketService, MarketService>();
            services.AddScoped<IFaqService, FaqService>();
            services.AddScoped<IPlatformPolicyService, PlatformPolicyService>();
            services.AddScoped<IProductUnitService, ProductUnitService>();

            services.AddScoped<IReviewService, ReviewService>();

            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISellerAnalyticsService, SellerAnalyticsService>();
            return services;
        }

        public static IServiceCollection AddLocalMartRepositories(this IServiceCollection services)
        {
            // Helper method to register repository for entity
            void AddRepository<TEntity>(string collectionName) where TEntity : class
            {
                services.AddScoped<IRepository<TEntity>>(sp =>
                {
                    var mongoService = sp.GetRequiredService<MongoDBService>();
                    return new Repository<TEntity>(mongoService, collectionName);
                });
            }
            AddRepository<User>("Users");
            AddRepository<Category>("Categories");
            AddRepository<CategoryRegistration>("CategoryRegistrations");
            AddRepository<Store>("Stores");
            AddRepository<StoreFollow>("StoreFollows");
            AddRepository<Product>("Products");
            AddRepository<ProductImage>("ProductImages");
            AddRepository<Favorite>("Favorites");
            AddRepository<Cart>("Carts");
            AddRepository<CartItem>("CartItems");
            AddRepository<Order>("Orders");
            AddRepository<OrderItem>("OrderItems");
            AddRepository<Faq>("Faqs");
            AddRepository<PlatformPolicy>("PlatformPolicies");
            AddRepository<Market>("Markets");
            AddRepository<ProductUnit>("ProductUnits");
            AddRepository<Favorite>("Favorites");
            AddRepository<Notification>("Notifications");
            return services;
        }
    }
}
