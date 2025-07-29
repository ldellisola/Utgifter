using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;
using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Expenses.List;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    
    public override void Configure()
    {
        Get("/expenses");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var expenses = await connection.QueryAsync<Expense>(
            """
            select id, date, person, store, city, originalCurrency, amount,hash, category, shared, trip 
            from Expenses
            ORDER BY Date DESC, id DESC
            LIMIT @PageSize OFFSET @PageSize * @PageNumber
            """,
            new { req.PageSize, req.PageNumber }
        );
        await Send.OkAsync(new Response(expenses.ToArray()), ct);
    }
}