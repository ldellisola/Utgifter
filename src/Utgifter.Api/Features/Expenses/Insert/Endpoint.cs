using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;

namespace Utgifter.Api.Features.Expenses.Insert;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    
    public override void Configure()
    {
        AllowAnonymous();
        Post("/expenses");
    }
    
    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            insert into Expenses (id, person, date, amount, originalcurrency, city, store, trip, shared, category)
            values (@Id, @Person, @Date, @Amount, @OriginalCurrency, @City, @Store, @Trip, @Shared, @Category)
            """,
            req.Expenses.Select(t=> t with
            {
                Store = t.Store.Trim().ToUpperInvariant(),
                Category = t.Category?.Trim().ToUpperInvariant()
            
            })
        );

        await SendAsync(null, 201, ct);
    }
}