using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Middlewares;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") ?? throw new ArgumentNullException("Seq"))
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddHttpLogging();
builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "Directory Service API";
    settings.Version = "v1";

    settings.SchemaSettings.SchemaType = SchemaType.OpenApi3;

    settings.SchemaSettings.GenerateEnumMappingDescription = true;

    settings.SchemaSettings.SchemaProcessors.Add(new EnvelopeSchemaProcessor());
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddScoped<ApplicationDbContext>();

builder.Services.AddScoped<ITransactionManager, TransactionManager>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddApplication();

var app = builder.Build();
app.UseExceptionMiddleware();
app.UseHttpLogging();
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();