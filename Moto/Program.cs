using CloudinaryDotNet;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moto.Consumers;
using Moto.Models;
using Moto.Services;
using Serilog;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<UpdateProductQuantityConsumer>();

//builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
//{
//    builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
//}));

builder.Services.AddCors();

//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(
//        builder =>
//        {
//            builder.WithOrigins("http://localhost:3000")
//                   .AllowAnyHeader()
//                   .AllowAnyMethod()
//                   .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
//        });
//});


builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
}
); ;




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MotoDBContext>(
    option =>
    {
        option.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnectionString"));
    });

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnectionString"))
);

builder.Services.AddScoped<MotoProducer, MotoProducer>();

builder.Services.AddStackExchangeRedisCache(option =>
    option.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString")
);

builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<MotoDBContext>()
                .AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(options =>
{

    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;


    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
    options.User.RequireUniqueEmail = true;


    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = false;

});

builder.Services.Configure<FormOptions>(options =>
{
    options.MemoryBufferThreshold = Int32.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
});

builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = "/Login";
    option.LogoutPath = "/Logout";
    option.Cookie.SameSite = SameSiteMode.None;
});

builder.Services.AddSingleton<IResponseCacheService, ResponseCacheService>();

builder.Services.AddSingleton(s =>
{
    var cloud = builder.Configuration.GetSection("Cloudinary").GetValue(typeof(string), "cloud").ToString();
    var apiKey = builder.Configuration.GetSection("Cloudinary").GetValue(typeof(string), "apiKey").ToString();
    var apiSecret = builder.Configuration.GetSection("Cloudinary").GetValue(typeof(string), "apiSecret").ToString();
    return new Cloudinary(new Account(cloud, apiKey, apiSecret));
});


var logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

app.UseCors(builder =>
{
    builder
          .WithOrigins("http://localhost:3000", "https://localhost:4200")
          .SetIsOriginAllowedToAllowWildcardSubdomains()
          .AllowAnyHeader()
          .AllowCredentials()
          .WithMethods("GET", "PUT", "POST", "DELETE", "OPTIONS")
          .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));

}
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors("corsapp");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


