using System.Text;
using InforceTestTask.Models;
using InforceTestTask.Services.Implementations;
using InforceTestTask.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:44412")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

builder.Services.AddDbContext<TestTaskAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InforceTestTaskDB")));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireUser", policy =>
        policy.RequireRole("User"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
           ValidateIssuer = true,
           ValidIssuer = builder.Configuration["AppSettings:Issuer"],
           ValidateAudience = true,
           ValidAudience = builder.Configuration["AppSettings:Audience"],
           ValidateLifetime = true,
           IssuerSigningKey = new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:AccessToken"]!)),
           ValidateIssuerSigningKey = true
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Enable OpenAPI in development
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();