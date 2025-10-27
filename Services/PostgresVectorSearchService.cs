using NashAI_app.Model;
using Npgsql;
using Dapper;
using Pgvector;
using NpgsqlTypes;
using Pgvector.Npgsql;

namespace NashAI_app.Services;

public class PostgresVectorSearchService: IVectorSearchService
{
    
    private readonly NpgsqlDataSource _dataSource;
    private readonly IEmbeddingService _embeddingService;
    private readonly string _connectionString;

    public PostgresVectorSearchService(NpgsqlDataSource dataSource, IEmbeddingService embeddingService)
    {
        _dataSource = dataSource;
        _embeddingService = embeddingService; 
    }
    
    public async Task<IEnumerable<DocumentEmbeddingVB>> SearchVectorAsync(string query, string? documentId, int maxResults)
    {
        
     //    var queryEmbedding = await _embeddingService.CreateEmbeddingAsync(query);
     var queryVector = new Vector(await _embeddingService.CreateEmbeddingAsync(query));
     
     var results = new List<DocumentEmbeddingVB>();

     await using var connection = await _dataSource.OpenConnectionAsync();
        

        var sql = @"
            SELECT 
                ""DocumentId"" AS DocumentId, 
                ""PageNumber"" AS PageNumber, 
                ""Content"" AS Content,
                1 - (""Embeddings"" <#> @queryVector) AS Similarity
            FROM    document_embedding
            WHERE (@filter IS NULL OR ""DocumentId"" = COALESCE(@filter, ""DocumentId""))
            ORDER BY ""Embeddings"" <#> @queryVector
            LIMIT @maxResults";
        
        await using var cmd = new NpgsqlCommand(sql, connection);

        // Explicitly tell Npgsql the type is vector
        cmd.Parameters.AddWithValue("queryVector", queryVector);
        
        // filter parameter (explicit type)
        var filterParam = cmd.Parameters.Add("@filter", NpgsqlTypes.NpgsqlDbType.Text);
        filterParam.Value = documentId ?? (object)DBNull.Value;
        
        cmd.Parameters.AddWithValue("@maxResults", maxResults);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new DocumentEmbeddingVB
            {
                DocumentId = reader.GetString(reader.GetOrdinal("DocumentId")),
                PageNumber = reader.GetInt32(reader.GetOrdinal("PageNumber")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
            //    Embeddings = default,
            });
        }

        return results.ToList();
        
    }

    public async Task UpdateVectorAsync(IEnumerable<IngestedChunk> ingestedchunks)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        foreach (var r in ingestedchunks)
        {
            var sql = @"
                INSERT INTO document_embedding (""DocumentId"", ""PageNumber"", ""Content"", ""Embeddings"")
                VALUES (@DocumentId, @PageNumber, @Content, @Embeddings)
                ON CONFLICT (""DocumentId"", ""PageNumber"")
                DO UPDATE SET ""Content"" = @Content, ""Embeddings"" = @Embeddings";

            await connection.ExecuteAsync(sql, r);
        }
    }
}