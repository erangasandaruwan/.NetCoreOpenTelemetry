using OTel.Jaeger.App.Receiver;
using OTel.Jaeger.App.Sender;
using OTel.Jaeger.App.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<IEventHubSenderService, EventHubSenderService>();
builder.Services.AddTransient<IEventHubReceiverService, EventHubReceiverService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureOpenTelemetry(builder.Configuration);

var app = builder.Build();

EventHubClient.ConfigureClients(app.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
