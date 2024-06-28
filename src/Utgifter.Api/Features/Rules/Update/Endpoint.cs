using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;
using Utgifter.Api.Models;
using Dapper;

namespace Utgifter.Api.Features.Rules.Update;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    
    public override void Configure()
    {
        Put("/rules/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var (id, expectedStore, newStore, newCategory,shared, trip) = request;
        var rule = new Rule(id, expectedStore, newStore, newCategory, shared, trip);
        await UpdateRule(rule);
        await SendNoContentAsync(cancellationToken);
    }


    private async Task UpdateRule(Rule rule)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            update Rules 
            set ExpectedStore = @ExpectedStore, NewStore = @NewStore, NewCategory = @NewCategory, Shared = @Shared, Trip = @Trip
            where id = @Id
            """, rule with 
            {
                ExpectedStore = rule.ExpectedStore.Trim().ToUpperInvariant(),
                NewStore = rule.NewStore?.Trim().ToUpperInvariant(),
                NewCategory = rule.NewCategory?.Trim().ToUpperInvariant(),
            }
        );
    }
}