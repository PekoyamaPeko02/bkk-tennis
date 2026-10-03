using System.Globalization;
using System.Text;

namespace BkkTennis.Api.Services;

/// <summary>
/// Builds an EMVCo "Thai QR / PromptPay" payload string (the text that goes inside the QR).
/// Used by the mock payment provider; a real gateway returns its own QR.
/// </summary>
public static class PromptPay
{
    private const string AidPromptPay = "A000000677010111";

    /// <param name="target">Mobile number (0812345678), national ID / tax ID (13 digits) or e-wallet ID (15 digits).</param>
    public static string Payload(string target, decimal? amount)
    {
        var digits = new string(target.Where(char.IsDigit).ToArray());
        string accountTag, account;
        if (digits.Length >= 15) { accountTag = "03"; account = digits; }
        else if (digits.Length >= 13) { accountTag = "02"; account = digits; }
        else
        {
            accountTag = "01";
            // 0812345678 -> 0066812345678
            account = ("66" + digits.TrimStart('0')).PadLeft(13, '0');
        }

        var sb = new StringBuilder();
        sb.Append(Tlv("00", "01"));
        sb.Append(Tlv("01", amount is null ? "11" : "12"));
        sb.Append(Tlv("29", Tlv("00", AidPromptPay) + Tlv(accountTag, account)));
        sb.Append(Tlv("53", "764"));
        if (amount is not null)
            sb.Append(Tlv("54", amount.Value.ToString("0.00", CultureInfo.InvariantCulture)));
        sb.Append(Tlv("58", "TH"));
        sb.Append("6304");
        sb.Append(Crc16(sb.ToString()).ToString("X4"));
        return sb.ToString();
    }

    private static string Tlv(string id, string value) => id + value.Length.ToString("00") + value;

    /// <summary>CRC-16/CCITT-FALSE (poly 0x1021, init 0xFFFF) as required by EMVCo.</summary>
    internal static ushort Crc16(string data)
    {
        ushort crc = 0xFFFF;
        foreach (var b in Encoding.ASCII.GetBytes(data))
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
                crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
        }
        return crc;
    }
}
