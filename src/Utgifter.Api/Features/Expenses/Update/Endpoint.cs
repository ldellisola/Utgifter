using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;

namespace Utgifter.Api.Features.Expenses.Update;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    
    public override void Configure()
    {
        Put("/expenses");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        var transaction = await connection.BeginTransactionAsync(ct);
        foreach (var expense in req.Expenses)
        {
            await connection.ExecuteAsync(
                """
                update Expenses
                set person = @Person,
                    date = @Date,
                    amount = @Amount,
                    originalCurrency = @OriginalCurrency,
                    city = @City,
                    store = @Store,
                    trip = @Trip,
                    shared = @Shared,
                    category = @Category
                where id = @Id
                """,
                expense with
                {
                    Store = expense.Store.Trim().ToUpperInvariant(),
                    Category = expense.Category?.ToUpperInvariant(),
                    Trip = expense.Trip?.Trim().ToUpperInvariant()
                },
                transaction
            );
        }
        await transaction.CommitAsync(ct);
        await SendOkAsync(ct);
    }
}