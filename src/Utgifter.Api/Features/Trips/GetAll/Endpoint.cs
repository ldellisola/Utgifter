using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;

namespace Utgifter.Api.Features.Trips.GetAll;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : EndpointWithoutRequest<Response>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    public override void Configure()
    {
        Get("/trips");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var trips= await connection.QueryAsync<string>(
            "select distinct trip from expenses where trip is not null order by trip"
        );
        
        await Send.OkAsync(new Response(trips.ToArray()), ct);
    }
}