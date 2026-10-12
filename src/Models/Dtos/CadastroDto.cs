using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class CadastroDto
    {
        private const string REGEX_PASSWORD = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$";
        [Required(ErrorMessage = "Nome obrigatório.")]
        [RegularExpression(@"^\S+\s+\S+.*$", ErrorMessage = "Informe o nome completo (nome e sobrenome).")]
        public string Nome { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;
        [CpfValidator(ErrorMessage = "CPF inválido.")]
        [Required(ErrorMessage = "CPF é obrigatório.")]
        public string CPF { get; set; } = string.Empty;
        [Required(ErrorMessage = "Senha obrigatório.")]
        [RegularExpression(REGEX_PASSWORD, ErrorMessage = "A senha deve ter no mínimo 8 caracteres, letra maiúscula, minúscula, número e caractere especial.")]
        public string SenhaHash { get; set; } = string.Empty;
    }
}
