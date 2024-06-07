using Dapper;
using FastEndpoints;
using FastEndpoints.Swagger;
using OfficeOpenXml;
using Utgifter.Api.Configuration;
using Utgifter.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

SqlMapper.AddTypeHandler(new SqlDateOnlyMapper());
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

builder.Services.ConfigureOptions<DataBaseOptionsSetup>();

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument();

var app = builder.Build();

app.UseFastEndpoints(t => t.Endpoints.RoutePrefix = "api")
    .UseDefaultExceptionHandler()
    .UseSwaggerGen();

app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();