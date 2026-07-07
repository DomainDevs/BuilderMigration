using DataToolkit.MigrationBuilder.Configuration;
using DataToolkit.MigrationBuilder.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuración del Migration Builder
builder.Services.Configure<MigrationOptions>(
    builder.Configuration.GetSection(MigrationOptions.SectionName));

// Configuración completa (SourceDB, DestinationDB y Migration)
builder.Services.Configure<MigrationConfiguration>(
    builder.Configuration);

// DataToolkit
builder.Services.AddDataToolkitSample(builder.Configuration);

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