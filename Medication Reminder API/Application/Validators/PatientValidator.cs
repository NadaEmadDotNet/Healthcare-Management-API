using FluentValidation;

namespace Medication_Reminder_API.Application.Validators
{
    public class PatientValidator : AbstractValidator<PatientDto>
    {
        public PatientValidator()
        {
            RuleFor(x => x.
            Name)
                .NotEmpty()
                .WithMessage("Patient name is required.")
                .Length(2, 100)
                .WithMessage("Patient name must be between 2 and 100 characters.");

            RuleFor(x => x.Age)
                .GreaterThan(0)
                .WithMessage("Age must be greater than 0.")
                .LessThan(150)
                .WithMessage("Age must be less than 150.");

            RuleFor(x => x.Gender)
                .NotEmpty()
                .WithMessage("Gender is required.")
                .Must(g => g == "Male" || g == "Female")
                .WithMessage(" Gender must be 'Male', 'Female' ");

            RuleFor(x => x.ChronicConditions)
                .MaximumLength(500)
                .WithMessage("Chronic conditions must be at most 500 characters.");

        }
    }
}
