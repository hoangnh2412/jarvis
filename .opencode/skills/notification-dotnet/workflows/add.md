# Workflow: Chỉnh notification

Áp dụng khi **đã có** Mailkit và cần đổi SMTP hoặc implementation sender.

## Checklist

```text
- [ ] 1. Cập nhật section Smtp (host/port/TLS)
- [ ] 2. Secret rotate qua env — không commit
- [ ] 3. Nếu đổi sender type — đăng ký lại AddEmailSender<T>
- [ ] 4. Test gửi từ staging
```

## Đổi implementation

Chỉ khi có class implement `IEmailSender` khác — gọi `.AddEmailSender<YourSender>()`.

## AWS (stub)

`Jarvis.Notification.AwsSES` / `AwsSNS` chưa có workflow chính thức — implement tùy app hoặc chờ package hoàn thiện.
