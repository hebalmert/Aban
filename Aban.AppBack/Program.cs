using Asp.Versioning;
using Microsoft.OpenApi.Models;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders Backend - V1", Version = "1.0" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Orders Backend - V2", Version = "2.0" });

    options.DocInclusionPredicate((version, desc) =>
    {
        var versions = desc.ActionDescriptor.EndpointMetadata.OfType<ApiVersionAttribute>().SelectMany(attr => attr.Versions);
        return versions.Any(v => $"v{v.MajorVersion}" == version);
    });

    options.CustomSchemaIds(type => type.Name.Replace("Controller", "").Replace("V", ""));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.<br />
                        Enter 'Bearer' [space] and then your token in the text input below.<br />
                        Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});




var app = builder.Build();

//Pipeline de captura de ejecucion, solo en desarrollo muestra Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders Backend - V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Orders Backend - V2");
    });
    Task.Run(() => OpenBrowser("https://localhost:7136/swagger"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


//Para poder abrir el navegador directo en /swagger
static void OpenBrowser(string url)
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al abrir el navegador: {ex.Message}");
    }
}