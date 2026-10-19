# Authentication — storage-backed providers

Tổng quan: [README.md](./README.md).

## Config (`appsettings`) — `Source` theo profile

Trong `Authentication:Schemes:{Kind}:Profiles:{name}`:

| `Source` | Credential | Các field còn lại |
|----------|------------|-------------------|
| `Configuration` | `Key` / `Users` / `IssuerSigningKeys` trong config | `KeyName`, `Realm`, JWT flags, … |
| `Storage` | `IApiKeyProvider` / `IBasicCredentialProvider` (DB, …) | Không secret trong JSON |

ApiKey: có thể `Default` = Storage và `Integration` = Configuration cùng lúc (`AddCoreApiKeyWithProfileSources`).

## Chi tiết profile

| Kind | Section | Fields |
|------|---------|--------|
| ApiKey | `Authentication:Schemes:ApiKey:Profiles:{Profile}` | `KeyName` |
| Basic | `Authentication:Schemes:Basic:Profiles:{Profile}` | `Realm` |
| Bearer | `Authentication:Schemes:Bearer:Profiles:{Profile}` | `Authority`, signing, audience, … |

Không đặt password, API key secret, hoặc bảng `Users` trong config.

## Password policy

| `Authentication:PasswordPolicy` | Ý nghĩa |
|----------------------------------|---------|
| `Source` | Bỏ trống → default class `PasswordPolicyOptions`; `Configuration` → `Defaults`; `Storage` → bắt buộc `IPasswordPolicyProvider` |
| `Defaults` | Dùng khi `Source` = `Configuration` |

Đăng ký `IPasswordPolicyScopeProvider` (tenant/org/user) + `IPasswordPolicyProvider` khi `Source` = `Storage`.

## Platform

- `AddCoreApiKey<T>` / `AddCoreBasic<T>` — `T` tra storage.
- `AuthenticationApiKeyOptionValidator` — chỉ `KeyName`.
- `ConfigApiKeyProvider` / `ConfigBasicCredentialProvider` — unit test (secret trong memory config).

## Sample

| Bảng | Hash | Tenant |
|------|------|--------|
| `BasicAuthUser` | PBKDF2-SHA256 (`AuthenticationSecretHashing`) | `TenantId` + unique `(TenantId, Username)` |
| `ApiKeyCredential` | SHA-256 base64 | `TenantId` + unique `(TenantId, Profile, KeyHash)` |
| `TenantPasswordPolicy` | rule độ mạnh mật khẩu | unique `TenantId` |

**Tenant:** header `X-Tenant-Id` (hoặc `TenantHeaderKey`). Không gửi header → fallback `MultitenancyEfJobRunner.MasterTenantRegistryId` (dev).

**Dev credentials (plaintext chỉ để test client, không có trong DB):**

| Loại | Giá trị |
|------|---------|
| Tenant | `dd348565-5d31-46bb-aef7-aa7f0c4a1866` |
| Basic | `sampleuser` / `samplepass` |
| ApiKey | `dev-sample-api-key` (profile `Default`) |

Migration: `20261019130000_AuthCredentialHashAndTenant`.

**Sau này:** gộp Basic vào Identity `UserManager`; pepper PBKDF2 từ vault; rotate API key qua admin API.
