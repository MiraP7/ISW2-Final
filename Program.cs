using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Middleware;
using ISW2_Primer_parcial.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

// Registrar servicios SOAP
builder.Services.AddScoped<CalculatorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Agregar middleware de autenticación por API Key
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.MapControllers();

app.Run();
