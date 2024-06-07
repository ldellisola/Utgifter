using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;

namespace Utgifter.Api.Features.Expenses.Delete;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    public override void Configure()
    {
        AllowAnonymous();
        Delete("/expenses/{Id}");
    }
    
    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "delete from Expenses where id = @Id",
            new {req.Id}
        );
        await SendOkAsync(ct);
    }
}