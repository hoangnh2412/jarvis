# Incident — sự cố vận hành

Chỗ đứng chính thức của **báo cáo sự cố** và **postmortem**. Trước đây hai template này không có nhà, nên report nằm rải trong `brainstorm/` rồi mất dấu.

| Loại | Template | Khi nào |
|------|----------|---------|
| Incident report | [TPL-incident-report](../../../templates/TPL-incident-report.md) | Mọi sự cố SEV1–3 — ghi trong lúc còn nóng |
| Postmortem | [TPL-postmortem](../../../templates/TPL-postmortem.md) | Sự cố nghiêm trọng hoặc **lặp lại** — phân tích blameless sau khi đã xử lý |

## Đặt tên

```text
incident/
├── INC-001-2026-08-21-thanh-toan-timeout.md
└── INC-002-2026-09-03-mat-ket-noi-kho/
    ├── INC-002-....md          ← report
    └── postmortem.md           ← nếu cần phân tích sâu
```

Mã `INC-NNN` **bất biến**, cấp theo thứ tự thời gian.

## Vì sao ở `06-changes/`

Sự cố là **đầu vào của thay đổi**: nó thường đẻ ra CR. Để cạnh CR thì đọc được cả chuỗi *sự cố → thay đổi → kết quả* mà không phải nhảy thư mục.

## Liên quan tới as-built

Postmortem hay lộ ra business rule **không ai biết là có** — hệ hành xử theo một luật chôn trong code từ nhiều năm trước. Đó là đầu vào tốt cho [as-built](../../../skills/as-built/SKILL.md): có sự cố thật làm bằng chứng, có người còn nhớ ngữ cảnh.

Rule khai quật được từ postmortem → `docs/03-modules/{mod}/DOC-04-business-rules.md`, kèm ID và trỏ ngược về `INC-NNN`.
