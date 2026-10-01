namespace BkkTennis.Api.Services;

public static class PhoneMask
{
    /// <summary>
    /// Enough for the front desk to recognise a customer, not enough to contact them: Thai numbers as
    /// <c>081-***-5678</c>, anything else as <c>***0123</c>. Null in, null out.
    /// </summary>
    public static string? Mask(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return null;

        var national = phone.StartsWith("+66") && digits.Length == 11 ? "0" + digits[2..] : digits;
        return national.Length == 10 && national[0] == '0'
            ? $"{national[..3]}-***-{national[^4..]}"
            : "***" + digits[^Math.Min(4, digits.Length)..];
    }
}
