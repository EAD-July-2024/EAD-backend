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
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<FCMTokenRepository>();

builder.Services.AddScoped<RatingRepository>();
builder.Services.AddSingleton<FirebaseService>();



builder.Services.AddScoped<OrderItemRepository>();
builder.Services.AddScoped<CategoryRepository>();


builder.Services.AddScoped<JWTService>();

// Add CORS services to allow any origin, method, and header
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Log token validation details
            var claims = context.Principal.Claims;
            Console.WriteLine("User claims:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            // Log when the user is forbidden
            Console.WriteLine("User is forbidden.");
            return Task.CompletedTask;
        },
    };

    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("RequireVendorRole", policy => policy.RequireRole("Vendor"));
    options.AddPolicy("RequireCSRRole", policy => policy.RequireRole("CustomerServiceRepresentative"));
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

// Enable CORS
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
