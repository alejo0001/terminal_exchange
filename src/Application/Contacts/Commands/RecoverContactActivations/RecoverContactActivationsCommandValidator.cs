using FluentValidation;

namespace CrmAPI.Application.Contacts.Commands.RecoverContactActivations
{
    public class RecoverContactActivationsCommandValidator : AbstractValidator<RecoverContactActivationsCommand>
    {
        public RecoverContactActivationsCommandValidator()
        {
            RuleFor(x => x.ProcessId)
               .GreaterThan(0).WithMessage("El ID del proceso debe ser mayor a 0.");
        }
    }
}
