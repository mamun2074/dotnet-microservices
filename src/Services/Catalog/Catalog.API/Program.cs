
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container

var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    // Validation behavior
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    // log behavior
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Fluent Validatoin 
builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddCarter();
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();
// seeding condition
if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<CatalogInitialData>();
}

// add custome exception handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

// Helth check
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

var app = builder.Build();
// configure the http request pipeline
app.MapCarter();

// configure exception option
app.UseExceptionHandler(option => { });
// helth check
app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
