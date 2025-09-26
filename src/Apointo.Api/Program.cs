using System.IO;
using System.Security.Claims;
using Apointo.Api.Diagnostics;
using Apointo.Api.Extensions;
using Apointo.Api.Mappings;
using Apointo.Application;
using Apointo.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logDirectory);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Host.UseSerilog((context, services, configuration) =>
{
    var environment = context.HostingEnvironment;

    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .Enrich.WithSpan()
        .Enrich.WithExceptionDetails()
        .WriteTo.Console(new RenderedCompactJsonFormatter())
        .WriteTo.File(
            formatter: new CompactJsonFormatter(),
            path: Path.Combine(logDirectory, "apointo-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: environment.IsProduction() ? 30 : 10,
            restrictedToMinimumLevel: LogEventLevel.Information);
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]?>() ?? new[] { "http://localhost:5173" };

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddAutoMapper(typeof(ApiMappingProfile));

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Apointo API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = static context =>
    {
        context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

await app.InitialiseDatabaseAsync();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, exception) =>
    {
        if (exception is not null)
        {
            return LogEventLevel.Error;
        }

        if (httpContext.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        if (httpContext.Response.StatusCode >= 400)
        {
            return LogEventLevel.Warning;
        }

        return elapsed > 1000 ? LogEventLevel.Warning : LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? string.Empty);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                diagnosticContext.Set("UserId", userId);
            }
        }

        if (httpContext.Connection.RemoteIpAddress is { } remoteIpAddress)
        {
            diagnosticContext.Set("ClientIp", remoteIpAddress.ToString());
        }
    };
});

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Apointo API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();








