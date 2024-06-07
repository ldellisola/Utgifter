using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Expenses.Update;

internal sealed record Request(Expense[] Expenses);