using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using Dapper;
using Utgifter.Api.Configuration;
using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Rules.Create;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request,Rule>
{
    private readonly string _connectionString = options.Value.ConnectionString;
    
    public override void Configure()
    {
        Post("/rules");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var (expectedStore, newStore, newCategory,shared, trip) = request;
        
        var rule = new Rule(Guid.NewGuid(),expectedStore,newStore, newCategory, shared, trip);
        rule = await InsertRule(rule);
        await SendAsync(rule, 201, cancellationToken);
    }
    
    
    private async Task<Rule> InsertRule(Rule rule)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync("""
                                      insert into rules (Id, ExpectedStore, NewStore, NewCategory, Shared, Trip) 
                                      values (@Id, @ExpectedStore, @NewStore, @NewCategory, @Shared, @Trip)
                                      """, rule with
            {
                ExpectedStore = rule.ExpectedStore.Trim().ToUpperInvariant(),
                NewStore = rule.NewStore?.Trim().ToUpperInvariant(),
                NewCategory = rule.NewCategory?.Trim().ToUpperInvariant(),
            }
        );

        return rule;
    }
}