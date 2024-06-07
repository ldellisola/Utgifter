using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Expenses.List;

internal sealed record Response(Expense[] Expenses);