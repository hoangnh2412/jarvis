namespace Jarvis.Domain.Shared.Constants;

public static class RegexValidate
{
    /// <summary>
    /// The general regex expresion for validate email address. https://emailregex.com
    /// </summary>
    public const string EmailGeneral = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    
    /// <summary>
    /// The standard regex expression for validate email address (RFC 5322 Official Standard: https://www.ietf.org/rfc/rfc5322.txt)
    /// </summary>
    public const string EmailStandard = @"^(?("")("".+?(?<!\\)""@)|(([0-9A-Za-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9A-Za-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9A-Za-z][-0-9A-Za-z]*[0-9A-Za-z]*\.)+[a-zA-Z0-9][\-a-zA-Z0-9]{0,22}[a-zA-Z0-9]))$";
    
    /// <summary>
    /// The regex expression for validate phone number start with +
    /// </summary>
    public const string PhoneNumber = @"^\+\d+$";
    
    /// <summary>
    /// The regex expression for validate version stardard semantic version. https://semver.org
    /// </summary>
    public const string Version = @"^\d+\.\d+\.\d+$";
}