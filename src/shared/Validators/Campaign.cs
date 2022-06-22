namespace pledgemanager.shared.Validators;

using FluentValidation;
using pledgemanager.shared.Models;

public class CampaignValidator : AbstractValidator<Campaign>
{
    public CampaignValidator()
    {
        RuleFor(p => p.Title)
            .NotNull().WithMessage("You must enter a title");

        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("You must enter a description");

        RuleFor(p => p.ImageUrl)
            .NotEmpty().WithMessage("You must enter avalid image URL");

        RuleFor(p => p.LastItemsCount)
            .GreaterThanOrEqualTo(5).WithMessage("Last Items must be greater than 5")
            .LessThan(50).WithMessage("Last Items must be less than 50");    

        RuleFor(p => p.Behavior.AutoApprovePledgeIfAmountLE)
            .GreaterThanOrEqualTo(100).WithMessage("Amount must be greater than 100");

        RuleFor(p => p.Behavior.EmphasisPledgeDialogStartup)
            .LessThanOrEqualTo(1000).WithMessage("Emphasis startup must be less than 1000 msecs");

        RuleFor(p => p.Behavior.EmphasisPledgeDialogVisibility)
            .LessThanOrEqualTo(15000).WithMessage("Emphasis visibility duration must be less than 15000 msecs");
    }
}
