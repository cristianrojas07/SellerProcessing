using Application.Abstractions.Messaging;
using Application.DTOs;
using Application.Features.Sellers.Commands.CreateSeller;
using Application.Features.Sellers.Commands.DeleteSeller;
using Application.Features.Sellers.Commands.UpdateSeller;
using Application.Features.Sellers.Commands.UploadSellers;
using Application.Features.Sellers.Queries.GetSellers;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Manual Handler Registration
        services.AddScoped<IQueryHandler<GetSellersQuery, PagedList<SellerDto>>, GetSellersHandler>();

        services.AddScoped<ICommandHandler<CreateSellerCommand, Guid>, CreateSellerHandler>();
        services.AddScoped<ICommandHandler<UpdateSellerCommand>, UpdateSellerHandler>();
        services.AddScoped<ICommandHandler<DeleteSellerCommand>, DeleteSellerHandler>();
        services.AddScoped<ICommandHandler<UploadSellersCommand>, UploadSellersHandler>();

        return services;
    }
}