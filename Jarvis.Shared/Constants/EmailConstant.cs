namespace Jarvis.Shared.Constants;

public static class EmailConstant
{
    public const string RegexValidateEmailBasic = @"^(([^<>()[\]\.,;:\s@\""]+(\.[^<>()[\]\.,;:\s@\""]+)*)|(\"".+\""))@(([^<>()[\]\.,;:\s@\""]+\.)+[^<>()[\]\.,;:\s@\""]{2,})$";
    public const string RegexValidateEmailFull = @"^(?("")("".+?(?<!\\)""@)|(([0-9A-Za-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9A-Za-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9A-Za-z][-0-9A-Za-z]*[0-9A-Za-z]*\.)+[a-zA-Z0-9][\-a-zA-Z0-9]{0,22}[a-zA-Z0-9]))$";
}