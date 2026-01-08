using Application.Common.Settings;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

    var factory = new ConnectionFactory
    {
        HostName = options.HostName,
        UserName = options.UserName,
        Password = options.Password,
        DispatchConsumersAsync = true,
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
    };

    return factory;
});

builder.Services.AddSingleton<RabbitMqPersistentConnection>();

builder.Services.AddHostedService<SellerCreationWorker>();

var host = builder.Build();
host.Run();