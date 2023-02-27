namespace AdventureBotUI.Client.Services;
public static class ConstantsLib
{
    public static bool IsValidEmail(string email)
    {
        if(string.IsNullOrEmpty(email))
        {
            return false;
        }
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith(".")) {
            return false; // suggested by @TK-421
        }
        try {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch {
            return false;
        }
    }
    public const string TenantName = "leenet.link";
}