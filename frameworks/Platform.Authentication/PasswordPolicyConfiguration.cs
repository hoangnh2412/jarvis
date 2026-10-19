namespace Platform.Authentication;

/// <summary>Áp dụng <see cref="PasswordPolicyRootOptions"/> thành <see cref="PasswordPolicyOptions"/> effectives.</summary>
public static class PasswordPolicyConfiguration
{
    /// <summary>
    /// <list type="bullet">
    /// <item><see cref="PasswordPolicySource.Configuration"/> — <see cref="PasswordPolicyRootOptions.Defaults"/> từ appsettings.</item>
    /// <item><paramref name="fromStorage"/> khi <see cref="PasswordPolicySource.Storage"/> (bắt buộc, không null).</item>
    /// <item><c>Source</c> không set — giá trị mặc định của <see cref="PasswordPolicyOptions"/> (property initializer).</item>
    /// </list>
    /// </summary>
    public static PasswordPolicyOptions Resolve(
        PasswordPolicyRootOptions settings,
        PasswordPolicyOptions? fromStorage = null)
    {
        return settings.Source switch
        {
            PasswordPolicySource.Configuration => settings.Defaults,
            PasswordPolicySource.Storage => fromStorage
                ?? throw new InvalidOperationException(
                    "Password policy was not returned from storage for the current scope."),
            _ => new PasswordPolicyOptions(),
        };
    }
}
