using Models.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class TaxIdValidatorAttribute : ValidationAttribute
    {
        private const string ERROR_MESSAGE = "TaxId inválido.";
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && !value.ToString()!.IsValidTaxId())
                return new ValidationResult(ErrorMessage ?? ERROR_MESSAGE);

            return ValidationResult.Success;
        }
    }
}
