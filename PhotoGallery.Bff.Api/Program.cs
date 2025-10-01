using PhotoGallery.Bff.Api.Clients;
using PhotoGallery.Bff.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add PhotoService.
var photoServiceUrl = builder.Configuration["Services:PhotoService"]
    ?? throw new InvalidOperationException("PhotoService URL is not configured.");

builder.Services.AddHttpClient<PhotoServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(photoServiceUrl);
    });

// Add CategoryService
var categoryServiceUrl = builder.Configuration["Services:CategoryService"]
    ?? throw new InvalidOperationException("CategoryService URL is not configured.");

builder.Services.AddHttpClient<CategoryServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(categoryServiceUrl);
    });

// Inject PGOrchestrator service.
builder.Services.AddScoped<PhotoGalleryOrchestrator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
