using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Expenses.Insert;

internal sealed record Request(Expense[] Expenses);