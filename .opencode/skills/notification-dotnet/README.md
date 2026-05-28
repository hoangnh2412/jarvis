# notification-dotnet

Skill tích hợp **Jarvis.Notification** — gửi email qua SMTP (Mailkit).

Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Chưa có `IEmailSender` | [workflows/init.md](./workflows/init.md) |
| Đổi SMTP / sender | [workflows/add.md](./workflows/add.md) |

## Cách gọi

```text
@.opencode/skills/notification-dotnet/providers/mailkit-smtp/SKILL.md

Wire Mailkit SMTP cho MyApp.Host — section Smtp, không commit password.
```

## Wiring tóm tắt

```csharp
var notificationBuilder = new NotificationBuilder(builder.Services);
notificationBuilder
    .AddSmtp(builder.Configuration.GetSection("Smtp"))
    .AddEmailSender<MailkitSender>();
```

Mẫu: [templates/notification-setup.cs](./templates/notification-setup.cs).

## Packages

| Project | Ghi chú |
|---------|---------|
| Jarvis.Notification | Core |
| Jarvis.Notification.Mailkit | SMTP |
| AwsSES / AwsSNS | stub — chưa skill provider |

## Liên quan

- [jarvis-dotnet](../jarvis-dotnet/README.md) — scaffold Host
