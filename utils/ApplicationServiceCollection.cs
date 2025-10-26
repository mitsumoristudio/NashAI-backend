using NashAI_app.Services;
using Project_Manassas.Service;

namespace NashAI_app.utils;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
     //   services.AddScoped<IRagService, RagService>();
        
        return services;
    }
}