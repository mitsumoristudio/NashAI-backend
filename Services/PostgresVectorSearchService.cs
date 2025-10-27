using NashAI_app.Model;
using Npgsql;
using Dapper;

namespace NashAI_app.Services;

public class PostgresVectorSearchService: IVectorSearchService
{
    
    private readonly string _connectionString;
    private readonly IEmbeddingService _embeddingService;

    public PostgresVectorSearchService(string connectionString, IEmbeddingService embeddingService)
    {
        _connectionString = connectionString;
        _embeddingService = embeddingService;
    }
    
    public async Task<IReadOnlyList<SearchResultModel>> SearchVectorAsync(string query, string? documentId, int maxResults)
    {
        var queryVector = await _embeddingService.CreateEmbeddingAsync(query);
        
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT document_id AS DocumentId, page_number AS PageNumber, text AS Text,
                   1 - (embedding <#> @queryVector) AS score
            FROM ingested_chunks
            WHERE (@filter IS NULL OR document_id = @filter)
            ORDER BY embedding <#> @queryVector
            LIMIT @maxResults";
        
        var results = await connection.QueryAsync<SearchResultModel>(sql, new { queryVector, filter = documentId, maxResults });
        return results.ToList();
    }

    public async Task UpdateVectorAsync(IEnumerable<IngestedChunk> ingestedchunks)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        foreach (var r in ingestedchunks)
        {
            var sql = @"
                INSERT INTO ingested_chunks (document_id, page_number, text, embedding)
                VALUES (@DocumentId, @PageNumber, @Text, @Vector)
                ON CONFLICT (document_id, page_number) 
                DO UPDATE SET text = @Text, embedding = @Vector";
            await connection.ExecuteAsync(sql, r);
        }
        
        
    }
}