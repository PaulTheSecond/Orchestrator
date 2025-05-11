using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrchestratorApp.Data;
using OrchestratorApp.Services;
using Serilog;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SpaServices.Extensions;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL with EF Core
var pgHost = Environment.GetEnvironmentVariable("PGHOST");
var pgPort = Environment.GetEnvironmentVariable("PGPORT");
var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pgUser = Environment.GetEnvironmentVariable("PGUSER");
var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

var connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword}";

// Включение поддержки JsonB и массивов в Npgsql
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure CORS for Angular client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configure SPA services
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

// Uncomment to enable RabbitMQ when a server is available
// builder.Services.AddSingleton<IBus>(RabbitHutch.CreateBus(builder.Configuration.GetConnectionString("RabbitMQ")));
builder.Services.AddSingleton<IBus>(sp => null); // Temporary placeholder

// Register application services
builder.Services.AddScoped<ProcedureTemplateService>();
builder.Services.AddScoped<ContestTemplateService>();
builder.Services.AddScoped<ProcedureInstanceService>();
builder.Services.AddScoped<ContestInstanceService>();
builder.Services.AddScoped<ApplicationInstanceService>();
builder.Services.AddScoped<OrchestrationService>();
builder.Services.AddScoped<RabbitMQService>();

var app = builder.Build();

try 
{
    // Ensure database is created and migrations are applied
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Log.Information("Attempting to migrate database...");
        
        // Make sure the database exists
        dbContext.Database.EnsureCreated();
        
        // Apply migrations (comment this out if using EnsureCreated)
        // dbContext.Database.Migrate();
        
        Log.Information("Database migration completed successfully.");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while migrating the database.");
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Configure static files with proper defaults
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFiles();
app.UseCors();
app.UseRouting();
app.UseAuthorization();

// Configure API routes
app.MapControllers();

// Configure SPA with optimized options
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    
    // Используем статические файлы из wwwroot
    spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
    {
        OnPrepareResponse = context =>
        {
            // Disable caching for index.html
            if (context.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
            {
                context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                context.Context.Response.Headers.Append("Pragma", "no-cache");
                context.Context.Response.Headers.Append("Expires", "0");
            }
        }
    };
    
    // В режиме разработки не используем проксирование на Angular CLI
    if (app.Environment.IsDevelopment())
    {
        // Используем готовые файлы из wwwroot
        Console.WriteLine("Using production files from wwwroot in development");
    }
});

// Configure RabbitMQ subscriptions
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var rabbitMQService = scope.ServiceProvider.GetRequiredService<RabbitMQService>();
        rabbitMQService.ConfigureSubscriptions();
    }
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while configuring RabbitMQ subscriptions.");
}

Log.Information("Application starting...");
app.Run();
