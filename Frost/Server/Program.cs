using Frost.Server.Hubs;
using Frost.Server.Mocks;
using Frost.Server.Services;
using Frost.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
        ValidIssuer = "localhost:44350",
        ValidAudience = "localhost:44350",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["IssuerSigningKey"]))
    };
});
builder.Services.AddScoped<IGooglePlacesApiService, GooglePlacesApiService>();
builder.Services.AddScoped<IPropertyService,PropertyDBServiceMock>();
builder.Services.AddScoped<IUserDbService, UserDbServiceMock>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPropertyImageService, PropertyImageService>();
builder.Services.AddScoped<IChatDbService, ChatDbServiceMock>();
builder.Services.AddScoped<IMessageDbService, MessageDbServiceMock>();
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
