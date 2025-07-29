using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;

namespace Utgifter.Api.Features.Categories.Get;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : EndpointWithoutRequest<Response>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    public override void Configure()
    {
        Get("/categories");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var categories = await connection.QueryAsync<string>(
            "select distinct category from expenses where category is not null order by category"
        );
        
        await Send.OkAsync(new Response(categories.ToArray()), ct);
    }
}