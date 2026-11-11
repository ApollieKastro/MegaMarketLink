namespace MegaMarketLibnk.Helpers;
public static class Validation
{
    private const string CaptchaAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ1234567890";
    public static bool IsValidEmail(string email) =>
        email.Contains('@') && email.Contains('.') && !email.Contains(' ');
    public static bool IsValidPassword(string password) =>
        password.Length >= 6 && password.Any(char.IsLetter) && password.Any(char.IsDigit);
    public static bool IsValidPhone(string phone) =>
        phone.Count(char.IsDigit) == 11;
    public static bool IsValidInn(string inn)
    {
        inn = inn.Trim();
        return (inn.Length == 10 || inn.Length == 12) && inn.All(char.IsDigit);
    }
    public static string GenerateCaptchaCode()
    {
        var chars = new char[4];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = CaptchaAlphabet[Random.Shared.Next(CaptchaAlphabet.Length)];
        return new string(chars);
    }
}
