using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class DataFimValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime data && data.Date < DateTime.UtcNow.Date)
                return new ValidationResult(ErrorMessage ?? "Data Tem que ser superior a data atual.");

            return ValidationResult.Success;
        }
    }
}
