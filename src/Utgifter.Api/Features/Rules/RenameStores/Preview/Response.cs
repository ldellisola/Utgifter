using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Rules.RenameStores.Preview;

public record Response(Expense[] Expenses, string ExpectedStore, string? NewStore);
