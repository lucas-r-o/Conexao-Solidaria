namespace Models.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidTaxId(this string taxId)
        {
            if (string.IsNullOrWhiteSpace(taxId))
                return false;

            // Remove non-digits
            var position = 0;
            var digits = new int[11];
            foreach (var c in taxId)
            {
                if (char.IsDigit(c))
                {
                    if (position >= 11) return false;
                    digits[position++] = c - '0';
                }
            }

            if (position != 11)
                return false;

            // Check if all digits are the same
            bool allEqual = true;
            for (int i = 1; i < 11; i++)
            {
                if (digits[i] != digits[0])
                {
                    allEqual = false;
                    break;
                }
            }
            if (allEqual)
                return false;

            // First check digit
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += digits[i] * (10 - i);

            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;

            if (digits[9] != firstDigit)
                return false;

            // Second check digit
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += digits[i] * (11 - i);

            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

            return digits[10] == secondDigit;
        }
    }
}
