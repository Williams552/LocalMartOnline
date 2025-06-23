using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services;
using LocalMartOnline.Services.Implement;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Load appsettings.Local.json nếu tồn tại
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Đăng ký NotificationService và IRepository<User> cho DI
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRepository<LocalMartOnline.Models.User>>(provider =>
    new LocalMartOnline.Repositories.Repository<LocalMartOnline.Models.User>(
        provider.GetRequiredService<MongoDBService>(), "User"));

// Swagger config
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "LocalMartOnline API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// MongoDB & Repository DI
builder.Services.AddMongoDbAndRepositories();

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();

// Đăng ký Repository cho Category
builder.Services.AddScoped<IRepository<Category>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<Category>(mongoService, "Categories");
});

builder.Services.AddScoped<IRepository<CategoryRegistration>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<CategoryRegistration>(mongoService, "CategoryRegistrations");
});
// Đăng ký Repository cho Store
builder.Services.AddScoped<IRepository<Store>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<Store>(mongoService, "Stores");
});

// Đăng ký Repository cho StoreFollow
builder.Services.AddScoped<IRepository<StoreFollow>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<StoreFollow>(mongoService, "StoreFollows");
});
// Đăng ký Repository cho Product
builder.Services.AddScoped<IRepository<Product>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<Product>(mongoService, "Products");
});

// Đăng ký Repository cho ProductImage
builder.Services.AddScoped<IRepository<ProductImage>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<ProductImage>(mongoService, "ProductImages");
});
builder.Services.AddScoped<IRepository<Order>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<Order>(mongoService, "Orders");
});
builder.Services.AddScoped<IRepository<OrderItem>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<OrderItem>(mongoService, "OrderItems");
});

// FAQ and PlatformPolicy
builder.Services.AddScoped<IRepository<LocalMartOnline.Models.Faq>>(provider =>
    new LocalMartOnline.Repositories.Repository<LocalMartOnline.Models.Faq>(
        provider.GetRequiredService<MongoDBService>(), "Faqs"));
builder.Services.AddScoped<LocalMartOnline.Services.Interface.IFaqService, LocalMartOnline.Services.Implement.FaqService>();

builder.Services.AddScoped<IRepository<LocalMartOnline.Models.PlatformPolicy>>(provider =>
    new LocalMartOnline.Repositories.Repository<LocalMartOnline.Models.PlatformPolicy>(
        provider.GetRequiredService<MongoDBService>(), "PlatformPolicies"));
builder.Services.AddScoped<LocalMartOnline.Services.Interface.IPlatformPolicyService, LocalMartOnline.Services.Implement.PlatformPolicyService>();

// Market 
builder.Services.AddScoped<IRepository<LocalMartOnline.Models.Market>>(provider =>
    new LocalMartOnline.Repositories.Repository<LocalMartOnline.Models.Market>(
        provider.GetRequiredService<MongoDBService>(), "Markets"));
// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingService).Assembly);


builder.Services.AddScoped<IMarketService, MarketService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRegistrationService, CategoryRegistrationService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IPlatformPolicyService, PlatformPolicyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocalMartOnline API V1");
        c.RoutePrefix = "Swagger"; // Đặt swagger làm trang mặc định
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();