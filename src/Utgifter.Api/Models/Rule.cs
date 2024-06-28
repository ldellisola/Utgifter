namespace Utgifter.Api.Models;

public record Rule(
    Guid Id,
    string ExpectedStore,
    string? NewStore,
    string? NewCategory,
    bool? Shared,
    bool? Trip
    );