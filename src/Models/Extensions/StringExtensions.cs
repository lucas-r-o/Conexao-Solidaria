namespace Models.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidCpf(this string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove non-digits
            var posicao = 0;
            var digitos = new int[11];
            foreach (var c in cpf)
            {
                if (char.IsDigit(c))
                {
                    if (posicao >= 11) return false;
                    digitos[posicao++] = c - '0';
                }
            }

            if (posicao != 11)
                return false;

            // Check if all digits are the same
            bool todosIguais = true;
            for (int i = 1; i < 11; i++)
            {
                if (digitos[i] != digitos[0])
                {
                    todosIguais = false;
                    break;
                }
            }
            if (todosIguais)
                return false;

            // First check digit
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += digitos[i] * (10 - i);

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            if (digitos[9] != digito1)
                return false;

            // Second check digit
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += digitos[i] * (11 - i);

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return digitos[10] == digito2;
        }
    }
}
