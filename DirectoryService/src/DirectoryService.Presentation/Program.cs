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
using Microsoft.OpenApi.Models;
using Serilog;
using Shared;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") ?? throw new ArgumentNullException("Seq"))
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddHttpLogging();
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonPropertyInfo?.GetType() != typeof(Envelope<Errors>))
        {
            return Task.CompletedTask;
        }

        if (schema.Properties.TryGetValue("errors", out var errors))
        {
            errors.Items.Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Error" };
        }

        return Task.CompletedTask;
    });
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
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();