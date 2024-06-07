namespace Utgifter.Api.Models;

public record Expense(
    Guid Id,
    DateOnly Date,
    string Person,
    string Store,
    string City,
    string OriginalCurrency,
    decimal Amount,
    string? Category = null,
    bool Shared = true,
    bool Trip = false
    );