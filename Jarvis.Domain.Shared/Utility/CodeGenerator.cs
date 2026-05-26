namespace Jarvis.Domain.Shared.Utility;

public static class CodeGenerator
{
    private const string CaseIdCharacters = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LowerCharacters = "abcdefghjkmnopqursuvwxyz";
    private const string UpperCharacters = "ABCDEFGHJKMNOPQRSTUVWXYZ";
    private const string Digits = "123456789";
    private const string SpecialCharacters = @"!@#$%^&*()";

    public static string GenerateRandomCode(int length = 7)
    {
        var caseId = string.Empty;
        var bytes = BitConverter.GetBytes(DateTime.Now.Ticks).Reverse().ToList().Shuffle();
        for (int i = 0; i < length; i++)
        {
            var item = bytes.ElementAt(i);
            var index = item % CaseIdCharacters.Length;
            caseId += CaseIdCharacters[index];
        }

        return caseId;
    }

    public static List<T> Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        var rng = new Random();
        while (n > 1)
        {
            int k = rng.Next(n--);
            (list[k], list[n]) = (list[n], list[k]);
        }

        return list;
    }
}
