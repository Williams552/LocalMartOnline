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
            services.AddScoped<IFastBargainRepository>(sp => {
                var db = sp.GetRequiredService<MongoDB.Driver.IMongoDatabase>();
                return new FastBargainRepository(db);
            });
            services.AddScoped<IFastBargainService, Implement.FastBargainService>();
            services.AddStackExchangeRedisCache(options =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                options.Configuration = configuration["Redis:ConnectionString"];
            });
            return services;
        }
    }
}
