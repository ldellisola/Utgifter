using System.Security.Cryptography;
using System.Text;

namespace Utgifter.Api.Models;

public record Expense(
    Guid Id,
    DateOnly Date,
    string Person,
    string Store,
    string City,
    string OriginalCurrency,
    decimal Amount,
    string Hash,
    string? Category = null,
    bool Shared = true,
    bool Trip = false
)
{

    public static Expense New(
        DateOnly date,
        string person,
        string store,
        string city,
        string originalCurrency,
        decimal amount,
        string? category = null,
        bool shared = true,
        bool trip = false
    )
        => new (Guid.NewGuid(), date, person, store, city, originalCurrency, amount,
            CalculateHash(date, person, store, amount), category, shared, trip);

    /// <summary>
    /// Hashes represent initial data of an expense. It is used compare expenses.
    /// They should not be updated if a field of an expense is updated.
    /// </summary>
    private static string CalculateHash(DateOnly date, string person, string store, decimal amount)
    {
        var input = $"{date}.{person}.{store}.{amount}";
        var bytes = Encoding.ASCII.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}