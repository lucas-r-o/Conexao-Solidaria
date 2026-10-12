using Models.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class CpfValidatorAttribute : ValidationAttribute
    {
        private const string ERROR_MESSAGE = "CPF inválido.";
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && !value.ToString()!.IsValidCpf())
                return new ValidationResult(ErrorMessage ?? ERROR_MESSAGE);

            return ValidationResult.Success;
        }
    }
}
