using Microsoft.Extensions.VectorData;
using NashAI_app.Services;
using Project_Manassas.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Npgsql;

namespace NashAI_app.utils;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        var connectionString = Environment.GetEnvironmentVariable("NEON_API_KEY");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string (NEON_API_KEY) is not set.");
        }

        // ✅ Create and configure a data source for PostgreSQL + pgvector
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();  // Register pgvector type handler
        var dataSource = dataSourceBuilder.Build();

        // Register data source as a singleton for dependency injection
        services.AddSingleton(dataSource);

        // Application services
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEquipmentService, EquipmentService>();

        // ✅ Inject NpgsqlDataSource instead of a raw connection string
        services.AddScoped<IVectorSearchService, PostgresVectorSearchService>();

        services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
        services.AddScoped<SemanticSearchVB>();
        services.AddScoped<IRagService, RagService>();

        
        return services;
    }
}