using PharMarket.Services;

namespace PharMarket.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IQrCodeService, QrCodeService>();

        return services;
    }
}
