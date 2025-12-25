using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;
using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Rules.RenameStores.Preview;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request, Response>
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public override void Configure()
    {
        Get("/rules/{RuleId}/rename-preview");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var rule = await connection.QuerySingleOrDefaultAsync<Rule>(
            """
            SELECT id, expectedstore, newstore, newcategory, shared, trip
            FROM Rules
            WHERE id = @RuleId
            """,
            new { req.RuleId }
        );

        if (rule is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var expenses = await connection.QueryAsync<Expense>(
            """
            SELECT id, date, person, store, city, originalCurrency, amount, hash, category, shared, trip
            FROM Expenses
            WHERE 
                ((@ExpectedStore NOT LIKE '*%' AND @ExpectedStore NOT LIKE '%*' AND store = @ExpectedStore)
                OR 
                (@ExpectedStore LIKE '%*' AND @ExpectedStore NOT LIKE '*%' AND store LIKE SUBSTRING(@ExpectedStore FROM 1 FOR LENGTH(@ExpectedStore)-1) || '%')
                OR
                (@ExpectedStore LIKE '*%' AND @ExpectedStore NOT LIKE '%*' AND store LIKE '%' || SUBSTRING(@ExpectedStore FROM 2))
                OR
                (@ExpectedStore LIKE '*%' AND @ExpectedStore LIKE '%*' AND store LIKE '%' || SUBSTRING(@ExpectedStore FROM 2 FOR LENGTH(@ExpectedStore)-2) || '%'))
            ORDER BY date DESC
            """,
            new { rule.ExpectedStore }
        );

        await Send.OkAsync(new Response(expenses.ToArray(), rule.ExpectedStore, rule.NewStore), ct);
    }
}
