using Microsoft.Extensions.VectorData;
using NashAI_app.Services;
using Project_Manassas.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace NashAI_app.utils;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        
        // Add VectorStoreCollection for SemanticSearchVB
        services.AddScoped<VectorStoreCollection<string, IngestedChunk>>();

        // Added SemanticSearch before RagService. 
        services.AddScoped<SemanticSearchVB>();
        
        services.AddScoped<IRagService, RagService>();
        
        return services;
    }
}