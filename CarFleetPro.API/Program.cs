using CarFleetPro.API.Data;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı (Kopmalara ve Batch çakışmalarına karşı tam koruma)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions => 
    {
        npgsqlOptions.EnableRetryOnFailure();
        npgsqlOptions.MaxBatchSize(1); // İŞTE GERÇEK YERİ BURASI!
    }));

// 2. ASP.NET Identity Kurulumu
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 3. JWT Kimlik Doğrulama Ayarları
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// 🔧 E-posta Servisi (Şifre sıfırlama için)
builder.Services.AddScoped<CarFleetPro.API.Services.IEmailService, CarFleetPro.API.Services.SmtpEmailService>();

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🚀 PERFORMANS: Response Compression (GZip + Brotli)
// JSON payload'ları %60-70 küçülür → Mobil ağ'da ciddi hız artışı
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // HTTPS üzerinden de sıkıştır
    options.Providers.Add<BrotliCompressionProvider>();  // Modern tarayıcılar için (daha iyi oran)
    options.Providers.Add<GzipCompressionProvider>();    // Eski cihazlar için fallback
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json" }); // JSON response'ları da sıkıştır
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest; // Hız/sıkıştırma dengesi — mobil için ideal
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize; // GZip fallback'te max sıkıştırma
});

// 4. Swagger Ayarları (Senin bizzat çözdüğün, tam uyumlu versiyon)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CarFleetPro API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token'ınızı buraya yapıştırın. Örnek: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Mobil test için kapalı — Android emülatörü dev sertifikasına güvenmez

// 🚀 PERFORMANS: Response Compression middleware'i — routing'den ÖNCE olmalı!
app.UseResponseCompression();

// Sıralama çok önemli: Önce kimlik sor, sonra yetkiye bak
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();