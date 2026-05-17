---
name: jarvis-dotnet-notification
description: Cài Jarvis.Notification — email qua Mailkit SMTP. Dùng khi app gửi email qua IEmailSender và NotificationBuilder.
dependencies:
  - Jarvis.Notification
  - Jarvis.Notification.Mailkit
---

# Notification

## Packages

| Project | PackageId | Ghi chú |
|---|---|---|
| Jarvis.Notification | — | Core, `NotificationBuilder` |
| Jarvis.Notification.Mailkit | — | `MailkitSender` : `IEmailSender` |
| Jarvis.Notification.AwsSES | — | stub |
| Jarvis.Notification.AwsSNS | — | stub |

## Wiring

```csharp
var notificationBuilder = new NotificationBuilder(builder.Services);
notificationBuilder
    .AddSmtp(builder.Configuration.GetSection("Smtp"))
    .AddEmailSender<MailkitSender>();
```

## appsettings.json

```json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UserName": "",
    "Password": "",
    "Socket": "StartTls"
  }
}
```

Secrets qua env/secret store — không commit password.

## Gửi email

Resolve `IEmailSender` keyed theo tên type (`MailkitSender`) hoặc inject trực tiếp implementation đã đăng ký.
