// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Endpoints;
using backend.Services;
using backend;
using Microsoft.OpenApi.Models;
using backend.Configs;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("Starting application...");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 string DB_Host=Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
Console.WriteLine($"DB_HOST: {DB_Host}");
string DB_User = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
Console.WriteLine($"DB_USER: {DB_User}");
string DB_Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "root";
Console.WriteLine($"DB_PASSWORD: {DB_Password}");
string DB_Name = Environment.GetEnvironmentVariable("DB_NAME") ?? "memos";
Console.WriteLine($"DB_NAME: {DB_Name}");
string DB_Port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
Console.WriteLine($"DB_PORT: {DB_Port}");
string connectionString = $"Server={DB_Host};Database={DB_Name};User={DB_User};Password={DB_Password};Port={DB_Port};";
Console.WriteLine($"Connection String: {connectionString}");
builder.Services.AddDbContext<MemoDbContext>(options =>
    options.UseMySQL(connectionString));
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<QRCodeService>();
builder.Services.AddScoped<AuthService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
// Bind JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add authorization
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
var app = builder.Build();
app.UseCors("AllowAllOrigins");
// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MemoDbContext>();
    try
    {
        // Create database if it doesn't exist and apply migrations
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log error (use your logging mechanism)
        Console.WriteLine($"Error applying migrations: {ex.Message}");
        throw;
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapAuthEndpoints();
app.MapWeddingEndpoints();
app.MapMediaEndpoints();
//app.MapQRCodeEndpoints();
app.MapPlannerEndpoints();
app.MapHowWeMetEndpoints();
app.MapProposalEndpoints();
app.MapGuestEndpoints();

app.Run();
