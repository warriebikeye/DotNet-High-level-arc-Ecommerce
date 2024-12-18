using ReviewServicex.BackgroundWorkers;
using System.Diagnostics.Metrics;
using ReviewServicex.Services;
using Serilog;
using Cassandra;
using Confluent.Kafka;
using AspNetCoreRateLimit;
using ReviewServicex.Services;
using ReviewServicex.BackgroundWorkers;

// Add services to the container.
// ReviewService/Program.cs

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console();
    config.WriteTo.File("logs/review-service-.txt", rollingInterval: RollingInterval.Day);
});

// Add services
builder.Services.AddSingleton<Cassandra.ISession>(sp =>
{
    var cluster = Cluster.Builder().AddContactPoint(builder.Configuration["Cassandra:ContactPoint"]).Build();
    return cluster.Connect(builder.Configuration["Cassandra:Keyspace"]);
});
builder.Services.AddScoped<ReviewService>();

builder.Services.AddHostedService<ReviewEventConsumer>();

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig { BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] };
    return new ProducerBuilder<string, string>(config).Build();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Enable rate limiting middleware
app.UseIpRateLimiting();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
