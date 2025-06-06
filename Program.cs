using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB DI
builder.Services.AddSingleton(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["MongoDB:ConnectionString"];
    return new MongoDB.Driver.MongoClient(connectionString);
});
builder.Services.AddSingleton(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var client = sp.GetRequiredService<MongoDB.Driver.MongoClient>();
    var dbName = configuration["MongoDB:DatabaseName"];
    return client.GetDatabase(dbName);
});
builder.Services.AddScoped<LocalMartOnline.Services.MongoDBService>();
builder.Services.AddScoped<IRepository<LocalMartOnline.Models.User>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.Repository<LocalMartOnline.Models.User>(mongoService, "Users");
});

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
builder.Services.AddScoped<LocalMartOnline.Services.Interface.IProductService, LocalMartOnline.Services.Implement.ProductService>();
builder.Services.AddScoped<LocalMartOnline.Services.Interface.IStoreService, LocalMartOnline.Services.Implement.StoreService>();
builder.Services.AddScoped<LocalMartOnline.Services.Interface.ICategoryService, LocalMartOnline.Services.Implement.CategoryService>();
builder.Services.AddScoped<LocalMartOnline.Services.Interface.ICategoryRegistrationService, LocalMartOnline.Services.Implement.CategoryRegistrationService>();
builder.Services.AddAutoMapper(typeof(MappingService).Assembly);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
