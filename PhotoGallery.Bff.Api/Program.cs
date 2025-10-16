using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PhotoGallery.Bff.Api.Clients;
using PhotoGallery.Bff.Api.Services;
using PhotoGallery.Bff.Api.Shared;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add AuthService.
var authServiceUrl = builder.Configuration["Services:AuthService"]
    ?? throw new InvalidOperationException("AuthService URL is not configured.");

builder.Services.AddHttpClient<AuthServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(authServiceUrl);
    });

// Add UserService.
var userServiceUrl = builder.Configuration["Services:UserService"]
    ?? throw new InvalidOperationException("UserService URL is not configured.");

builder.Services.AddHttpClient<UserServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(userServiceUrl);
    }).AddHttpMessageHandler<JwtForwardHandler>();

// Add PhotoService.
var photoServiceUrl = builder.Configuration["Services:PhotoService"]
    ?? throw new InvalidOperationException("PhotoService URL is not configured.");

builder.Services.AddHttpClient<PhotoServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(photoServiceUrl);
    }).AddHttpMessageHandler<JwtForwardHandler>();

// Add CategoryService
var categoryServiceUrl = builder.Configuration["Services:CategoryService"]
    ?? throw new InvalidOperationException("CategoryService URL is not configured.");

builder.Services.AddHttpClient<CategoryServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(categoryServiceUrl);
    }).AddHttpMessageHandler<JwtForwardHandler>();

// Add HttpContextAccessor to access HTTP context in services.
builder.Services.AddHttpContextAccessor();

// Register JWT forward handler.
builder.Services.AddTransient<JwtForwardHandler>();

// Inject PGOrchestrator service.
builder.Services.AddScoped<PhotoGalleryOrchestrator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Authentication & Authorization
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        //// If use Cookie, uncomment the following code to read token from cookie.
        //options.Events = new JwtBearerEvents
        //{
        //    OnMessageReceived = context =>
        //    {
        //        // Read token from cookie
        //        if (context.Request.Cookies.ContainsKey("pgbff_auth_token"))
        //        {
        //            context.Token = context.Request.Cookies["pgbff_auth_token"];
        //        }
        //        return Task.CompletedTask;
        //    }
        //};

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

// CORS setup for cookie-based flow
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["Frontend:Origin"]!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // important for cookies
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
