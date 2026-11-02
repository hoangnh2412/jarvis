namespace Platform.Modules.Account;

/// <summary>Realm trong claim <c>realm</c> của token: tầng App (tenant/org) hay tầng Platform (root).</summary>
public static class AccountRealms
{
    public const string App = "app";

    public const string Platform = "platform";
}
