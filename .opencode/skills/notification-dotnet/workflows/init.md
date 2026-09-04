# Workflow: Khởi tạo Jarvis Notification

Áp dụng khi Host **chưa** có `NotificationBuilder` / `IEmailSender`.

## Checklist

```text
- [ ] 1. Reference Jarvis.Notification + Jarvis.Notification.Mailkit
- [ ] 2. NotificationBuilder AddSmtp + AddEmailSender<MailkitSender>
- [ ] 3. appsettings Smtp (secret qua env)
- [ ] 4. Service/handler inject IEmailSender — gửi test
- [ ] 5. dotnet build
```

## Bước 1 — Packages

ProjectReference hoặc NuGet tới `Jarvis.Notification`, `Jarvis.Notification.Mailkit`.

## Bước 2 — Host extension

[templates/notification-setup.cs](../templates/notification-setup.cs)

Đặt trong `HostLayerExtension` hoặc `Program.cs` trước `Build()`.

## Bước 3 — appsettings

[templates/appsettings-smtp.json](../templates/appsettings-smtp.json)

Production: `Password` từ `SMTP_PASSWORD` env hoặc secret manager.

## Bước 4 — Gửi email

```csharp
public class WelcomeService(IEmailSender emailSender)
{
    public Task SendAsync(string to, CancellationToken ct) =>
        emailSender.SendAsync(/* message theo API package */, ct);
}
```

API chi tiết message — xem `Jarvis.Notification` contracts trong repo.

## Anti-patterns

- Commit SMTP password
- Gửi email sync blocking trên request thread — dùng queue/background nếu volume cao
