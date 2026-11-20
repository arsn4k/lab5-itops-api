using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using Lab5.API.Middleware;
using Lab5.API.Startup;
using Lab5.API.Validators;
using Lab5.Infrastructure.Data;
using Lab5.Infrastructure.Repositories;
using Lab5.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Хмарні платформи (Render, Railway тощо) передають порт змінною PORT.
var cloudPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(cloudPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{cloudPort}");
}

// ---------- контролери, JSON, валідація ----------
builder.Services.AddControllers(options =>
        // null у "name" має обробляти FluentValidation, а не неявний [Required]
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddValidatorsFromAssemblyContaining<CreateServerRequestValidator>();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

// ---------- база даних (SQLite + EF Core) ----------
// Рядок підключення береться з конфігурації: appsettings*.json, user-secrets
// або змінної середовища ConnectionStrings__Lab5Database.
var connectionString = builder.Configuration.GetConnectionString("Lab5Database")
    ?? throw new InvalidOperationException("Не налаштовано рядок підключення Lab5Database.");

var sqlite = new SqliteConnectionStringBuilder(connectionString);
if (!Path.IsPathRooted(sqlite.DataSource))
{
    // відносний шлях рахуємо від папки проєкту, щоб файл БД не залежав від поточного каталогу
    sqlite.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource);
}
var dbDirectory = Path.GetDirectoryName(sqlite.DataSource);
if (!string.IsNullOrEmpty(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

builder.Services.AddDbContext<Lab5DbContext>(options => options.UseSqlite(sqlite.ConnectionString));
builder.Services.AddScoped<IServerRepository, EfServerRepository>();
builder.Services.AddScoped<IServerService, ServerService>();
builder.Services.AddHostedService<DatabaseInitializer>();

// ---------- health checks ----------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Lab5DbContext>("database", tags: new[] { "ready" });

var app = builder.Build();

// Централізована обробка помилок: першою в конвеєрі після вбудованих компонентів.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// У контейнері та в хмарі HTTPS завершується на платформі, тому перенаправлення можна вимкнути.
if (app.Configuration.GetValue("Hosting:UseHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.MapControllers();

// /health      - загальний стан (усі перевірки)
// /health/live - застосунок запущений (без залежностей)
// /health/ready - застосунок готовий приймати запити (перевірка БД)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

if (app.Environment.IsDevelopment())
{
    // Лише для перевірки middleware (вправа 1): викликає необроблений виняток.
    app.MapGet("/debug/throw", ThrowTestError);
}

app.Logger.LogInformation("Застосунок Lab5.API запускається у середовищі {Environment}", app.Environment.EnvironmentName);
app.Run();

static IResult ThrowTestError() =>
    throw new InvalidOperationException("Тестовий виняток для перевірки ExceptionHandlingMiddleware");
