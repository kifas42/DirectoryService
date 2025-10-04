using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Presentation;
using Microsoft.OpenApi.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<CreateLocationHandler>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();