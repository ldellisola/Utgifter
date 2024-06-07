using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Expenses.Upload;

internal sealed record Response(
    List<Expense> ExistingExpenses,
    List<Expense> NewExpenses
    );