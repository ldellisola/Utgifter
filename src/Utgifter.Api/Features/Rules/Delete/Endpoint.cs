using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;

namespace Utgifter.Api.Features.Rules.Delete;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    public override void Configure()
    {
        Delete("/rules/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        if (!DeleteRule(request.Id))
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        await SendNoContentAsync(cancellationToken);
    }
    
    
    private bool DeleteRule(Guid id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Execute("delete from Rules where id = @Id", new {Id = id}) > 0;
    }
}