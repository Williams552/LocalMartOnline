using MongoDB.Driver;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models;
using LocalMartOnline.Services;
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
builder.Services.AddScoped<IGenericRepository<LocalMartOnline.Models.User>>(sp =>
{
    var mongoService = sp.GetRequiredService<LocalMartOnline.Services.MongoDBService>();
    return new LocalMartOnline.Repositories.GenericRepository<LocalMartOnline.Models.User>(mongoService, "Users");
});
builder.Services.AddScoped<IVnPayService, VnPayService>();

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
