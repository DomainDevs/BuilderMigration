using DataToolkit.MigrationBuilder.Configuration;
using DataToolkit.MigrationBuilder.Infrastructure.Connect;
using DataToolkit.MigrationBuilder.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración del Builder
builder.Services.Configure<MigrationOptions>(
    builder.Configuration.GetSection(MigrationOptions.SectionName));

// DataToolkit
builder.Services.AddBuilderDataToolkit(builder.Configuration);

// Servicios del Builder
builder.Services.AddBuilderServices();

// ASP.NET Core
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();