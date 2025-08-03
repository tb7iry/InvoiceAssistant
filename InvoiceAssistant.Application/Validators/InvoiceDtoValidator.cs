using FluentValidation;
using InvoiceAssistant.Application.Dtos;


namespace InvoiceAssistant.Application.Validators;

public class InvoiceDtoValidator : AbstractValidator<InvoiceDto>
{
    public InvoiceDtoValidator()
    {
        RuleFor(x => x.InvoiceNumber).NotEmpty();
        RuleFor(x => x.ClientName).NotEmpty();
        RuleFor(x => x.IssueDate).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleForEach(x => x.InvoiceDetails).SetValidator(new InvoiceDetailDtoValidator());
    }
}
