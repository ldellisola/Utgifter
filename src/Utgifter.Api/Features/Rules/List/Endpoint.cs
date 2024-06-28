using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;
using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Rules.List;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request,Response>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    
    public override void Configure()
    {
        Get("/rules");
        AllowAnonymous();
    }
    
    public override  async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var rules = await GetPaginatedRules(request.PageNumber, request.PageSize);
        await SendOkAsync(new Response(rules.ToArray()), cancellationToken);
    }
    
    private async Task<IEnumerable<Rule>> GetPaginatedRules(int pageNumber, int pageSize)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        return await connection.QueryAsync<Rule>(
            """
                        select id, expectedstore, newstore, newcategory, shared,trip
                        from Rules
                        ORDER BY expectedstore
                        LIMIT @PageSize OFFSET @PageSize * @PageNumber
            """,
            new { pageSize, pageNumber }
        );
    }
}