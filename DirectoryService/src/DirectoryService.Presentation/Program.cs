using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Middlewares;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") ?? throw new ArgumentNullException("Seq"))
    .CreateLogger();

string environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", true, true);
builder.Configuration.AddEnvironmentVariables();
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

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();
app.UseExceptionMiddleware();
app.UseHttpLogging();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseOpenApi();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();