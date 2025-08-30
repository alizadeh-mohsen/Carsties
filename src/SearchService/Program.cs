using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        //var hostAddress = builder.Configuration["EventBusSettings:HostAddress"];
        //if (!string.IsNullOrEmpty(hostAddress))
        //{
        //    // Use configured URI (includes username/password if provided)
        //    cfg.Host(new Uri(hostAddress));
        //}
        //else
        //{
        //    // Fallback to default docker service name and credentials
        //    cfg.Host("rabbitmq", h =>
        //    {
        //        h.Username("guest");
        //        h.Password("guest");
        //    });
        //}

        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
            e.UseMessageRetry(r => r.Interval(5, 5));
        });
        cfg.ConfigureEndpoints(context);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDbAsync(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
   => HttpPolicyExtensions.HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
