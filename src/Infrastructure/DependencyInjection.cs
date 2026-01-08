using Application.Abstractions.Repositories;
using Application.Common.Settings;
using Domain.Abstractions.Messaging;
using Domain.Abstractions.Repositories;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<ISellerReadRepository, SellerRepository>();

        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        // Messaging (RabbitMQ)
        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

            return new ConnectionFactory
            {
                HostName = options.HostName,
                UserName = options.UserName,
                Password = options.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
        });

        services.AddSingleton<RabbitMqPersistentConnection>();
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}