using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;
using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Rules.RenameStores.Apply;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : EndpointWithoutRequest<Response>
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public override void Configure()
    {
        Post("/rules/{RuleId}/rename-apply");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var ruleId = Route<Guid>("RuleId");
        
        await using var connection = new NpgsqlConnection(_connectionString);

        var rule = await connection.QuerySingleOrDefaultAsync<Rule>(
            """
            SELECT id, expectedstore, newstore, newcategory, shared, trip
            FROM Rules
            WHERE id = @RuleId
            """,
            new { RuleId = ruleId }
        );

        if (rule is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(rule.NewStore))
        {
            AddError("Rule does not have a NewStore defined");
            ThrowIfAnyErrors();
        }

        // Update store name WITHOUT updating the hash field
        var affectedRows = await connection.ExecuteAsync(
            """
            UPDATE Expenses
            SET store = @NewStore
            WHERE 
                ((@ExpectedStore NOT LIKE '*%' AND @ExpectedStore NOT LIKE '%*' AND store = @ExpectedStore)
                OR 
                (@ExpectedStore LIKE '%*' AND @ExpectedStore NOT LIKE '*%' AND store LIKE SUBSTRING(@ExpectedStore FROM 1 FOR LENGTH(@ExpectedStore)-1) || '%')
                OR
                (@ExpectedStore LIKE '*%' AND @ExpectedStore NOT LIKE '%*' AND store LIKE '%' || SUBSTRING(@ExpectedStore FROM 2))
                OR
                (@ExpectedStore LIKE '*%' AND @ExpectedStore LIKE '%*' AND store LIKE '%' || SUBSTRING(@ExpectedStore FROM 2 FOR LENGTH(@ExpectedStore)-2) || '%'))
            """,
            new
            {
                NewStore = rule.NewStore!.Trim().ToUpperInvariant(),
                rule.ExpectedStore
            }
        );

        await Send.OkAsync(new Response(affectedRows), ct);
    }
}
