using System.Security.Claims;
using System.Text;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddControllers();
// Register MongoDBSettings as a singleton service
builder.Services.AddSingleton(serviceProvider =>  
    serviceProvider.GetRequiredService<IOptions<MongoDBSettings>>().Value
);

// Register MongoDBService as a singleton
builder.Services.AddScoped<MongoDBService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<VendorRepository>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddScoped<JWTService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        //ValidateIssuer = false,
        //ValidateAudience = false,
        ValidateLifetime = true,
        //ValidateIssuerSigningKey = true,
        //ValidIssuer = builder.Configuration["Jwt:Issuer"],
        //ValidAudience = builder.Configuration["Jwt:Audience"],
        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = "role"

        //RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("RequireVendorRole", policy => policy.RequireRole(UserRoles.Vendor));
    options.AddPolicy("RequireCSRRole", policy => policy.RequireRole(UserRoles.CSR));
});


var app = builder.Build();

Console.WriteLine(builder.Configuration["Jwt:Issuer"]);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add logging
app.UseAuthentication();
app.UseAuthorization();




app.UseHttpsRedirection();

app.MapControllers();

app.Run();

