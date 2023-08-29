using Frost.Server.EntityModels;
using Frost.Server.Hubs;
using Frost.Server.Mocks;
using Frost.Server.Repositories;
using Frost.Server.Services;
using Frost.Server.Services.AuthServices;
using Frost.Server.Services.HostedServices;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.Interfaces;
using Frost.Server.Services.MailServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Text;
using static Frost.Server.Services.HostedServices.NotificationHostedService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:ValidIssuer"],
        ValidAudience = builder.Configuration["JwtSettings:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["IssuerSigningKey"]))
    };
});

builder.Services.AddDbContext<FrostDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration["ConnectionString"]);
});
builder.Services.AddHostedService<NotificationHostedService>();
builder.Services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IGooglePlacesApiService, GooglePlacesApiService>();
builder.Services.AddScoped<IPropertyRepository,PropertyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPropertyImageService, PropertyImageService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IUserImageService, UserImageService>();
builder.Services.AddScoped<IFiltersRepository, FiltersRepository>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>(ChatHub.HubUrl);
app.MapFallbackToFile("index.html");

app.Run();
