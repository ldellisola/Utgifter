using FluentValidation;

namespace Utgifter.Api.Features.Rules.Update;

internal sealed record Request(
    Guid Id, 
    string ExpectedStore, 
    string? NewStore = null, 
    string? NewCategory = null,
    bool? Shared = null,
    bool? Trip = null
    );
    
internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.ExpectedStore).NotEmpty();
        RuleFor(t => t)
            .Must(t => t.NewCategory != null || t.NewStore != null || t.Shared != null || t.Trip != null)
            .WithMessage("At least one of NewCategory, NewStore, Shared or Trip must be set.");

    }
}