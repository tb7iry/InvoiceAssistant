using FluentValidation;
using InvoiceAssistant.Application.Dtos;

namespace InvoiceAssistant.Application.Validators;

public class InvoiceDetailDtoValidator : AbstractValidator<InvoiceDetailDto>
{
    public InvoiceDetailDtoValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
