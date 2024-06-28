using FastEndpoints;
using FluentValidation;

namespace Utgifter.Api.Features.Rules.Create;

internal sealed record Request(string ExpectedStore, string? NewStore = null, string? NewCategory = null, bool? Shared = null, bool? Trip = null);


internal sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.ExpectedStore).NotEmpty();
        RuleFor(t=> t)
            .Must(t => 
                !string.IsNullOrWhiteSpace(t.NewCategory) || 
                !string.IsNullOrWhiteSpace(t.NewStore) || 
                t.Shared != null || 
                t.Trip != null
                )
            .WithMessage("At least one of NewCategory, NewStore, Shared or Trip must be set.");

    }
}