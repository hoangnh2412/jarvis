---
name: notification-dotnet-mailkit-smtp
description: Đăng ký Jarvis Notification Mailkit SMTP — NotificationBuilder AddSmtp và MailkitSender IEmailSender. Dùng khi gửi email qua SMTP transactional.
dependencies:
  - Jarvis.Notification
  - Jarvis.Notification.Mailkit
---

# Mailkit SMTP

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

Override `Password` / `UserName` bằng biến môi trường trong production.

## Resolve sender

- Keyed theo tên type `MailkitSender`, hoặc
- Inject `MailkitSender` / `IEmailSender` trực tiếp trong service

## Validate

- Gửi mail test tới inbox dev
- TLS/port đúng provider (587 StartTls vs 465)
