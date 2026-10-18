using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class RegistrationDto
    {
        private const string REGEX_PASSWORD = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$";
        [Required(ErrorMessage = "Name obrigatório.")]
        [RegularExpression(@"^\S+\s+\S+.*$", ErrorMessage = "Informe o nome completo (nome e sobrenome).")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;
        [TaxIdValidator(ErrorMessage = "Tax ID is invalid.")]
        [Required(ErrorMessage = "TaxId é obrigatório.")]
        public string TaxId { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password obrigatório.")]
        [RegularExpression(REGEX_PASSWORD, ErrorMessage = "A senha deve ter no mínimo 8 caracteres, letra maiúscula, minúscula, número e caractere especial.")]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
