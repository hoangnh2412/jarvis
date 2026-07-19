# Jarvis Authentication — Tổng quan & Sơ đồ hoá

Sơ đồ hoá toàn bộ stack authentication gồm 1 package lõi và 3 package vệ tinh:

| Package | Vai trò |
|---------|---------|
| **`Jarvis.Authentication`** | Lõi — entry point `AddJarvisAuthentication`, root options, password policy, composite scheme, hằng số scheme |
| **`Jarvis.Authentication.Basic`** | Satellite — HTTP Basic Auth (handler tự viết) |
| **`Jarvis.Authentication.Jwt`** | Satellite — JWT Bearer (wrap `Microsoft.AspNetCore.Authentication.JwtBearer`) |
| **`Jarvis.Authentication.ApiKey`** | Satellite — API Key header (wrap `AspNetCore.Authentication.ApiKey`) |

**Nguyên tắc kiến trúc:**
- Lõi không phụ thuộc satellite; satellite phụ thuộc lõi (dùng `JarvisAuthenticationSchemes`, options gốc).
- Mỗi satellite là một extension `AddCore*` trên `AuthenticationBuilder` — host tự chọn scheme nào bật.
- Nguồn credential là **extension point generic** (`IBasicCredentialProvider`, `IApiKeyProvider`, `IJwtTokenAccessChecker`) — mặc định đọc config, override được sang DB/Redis/MinIO/vault.
- Cấu hình bind từ section `Authentication` trong `appsettings.json`, validate lúc startup (`ValidateOnStart`).

---

## 1. Sơ đồ package & phụ thuộc

Toàn bộ cạnh chảy **một chiều từ trên xuống** (không có mũi tên bắn ngược lên) nên không đường nào đè lên nhau. Mũi tên **liền** = luồng đăng ký / phụ thuộc external; mũi tên **đứt** = satellite dùng contract của lõi (gom thành *một* đường vào cả cụm).

```mermaid
graph TB
    HOST["🖥️ Host / Program.cs"]
    SCE["① Jarvis.Authentication — LÕI<br/>AddJarvisAuthentication() · AddJarvisCompositeScheme()"]

    subgraph Sats["② Satellite — mỗi package một scheme"]
        direction LR
        JEXT["Jwt<br/>AddCoreJwtBearer&lt;T&gt;<br/>IJwtTokenAccessChecker"]
        AEXT["ApiKey<br/>AddCoreApiKey&lt;T&gt;<br/>ConfigApiKeyProvider"]
        BEXT["Basic<br/>AddCoreBasic&lt;T&gt;<br/>JarvisBasicAuthenticationHandler"]
    end

    MSJwt["MS.AspNetCore.Authentication.JwtBearer"]
    PkgApiKey["AspNetCore.Authentication.ApiKey"]

    SHARED["③ Contract dùng chung của lõi<br/>JarvisAuthenticationSchemes · AuthenticationRootOptions<br/>IPasswordPolicyValidator · PasswordPolicyOptions"]

    HOST --> SCE
    SCE -->|"configureSchemes()"| Sats
    JEXT --> MSJwt
    AEXT --> PkgApiKey
    Sats -.->|"dùng contract lõi"| SHARED

    style HOST fill:#333,color:#fff
    style SCE fill:#1e3a5f,color:#fff
    style Sats fill:#20222b,color:#fff
    style JEXT fill:#2f5f3d,color:#fff
    style AEXT fill:#5f3d2f,color:#fff
    style BEXT fill:#3d2f5f,color:#fff
    style MSJwt fill:#555,color:#fff
    style PkgApiKey fill:#555,color:#fff
    style SHARED fill:#274b73,color:#fff
```

**Đọc sơ đồ (theo chiều xuống):**
- **Host** gọi lõi một lần, rồi lõi mở callback `configureSchemes` để nạp các satellite (② nằm gọn trong 1 cụm — chỉ một mũi tên vào, tránh 3 đường toả ra cắt nhau).
- **Basic tự viết handler** nên không có mũi tên xuống tầng NuGet; chỉ **Jwt → JwtBearer** và **ApiKey → AspNetCore.Authentication.ApiKey**.
- Cả cụm satellite **cùng dùng contract của lõi** (③) — biểu diễn bằng *một* đường đứt xuống dưới, thay cho 3 đường ngược lên như bản cũ.

---

## 2. Đăng ký (DI) — luồng khởi tạo

`AddJarvisAuthentication` là entry point bắt buộc gọi đầu tiên; các `AddCore*` chạy trong callback `configureSchemes`.

```mermaid
sequenceDiagram
    participant Host as Program.cs (Host)
    participant Core as AddJarvisAuthentication
    participant AB as AuthenticationBuilder
    participant Sat as AddCore* (satellite)

    Host->>Core: AddJarvisAuthentication(config, configureSchemes)
    Core->>Core: bind "Authentication" → AuthenticationRootOptions
    Core->>Core: Configure + ValidateOnStart<br/>+ RootOptionsValidator
    Core->>Core: TryAddSingleton IPasswordPolicyValidator
    Core->>AB: services.AddAuthentication()<br/>set Default Authenticate/Challenge scheme
    Core->>Sat: configureSchemes(builder)
    Note over Sat: auth.AddCoreJwtBearer(...)<br/>auth.AddCoreApiKey<ConfigApiKeyProvider>(...)<br/>auth.AddCoreBasic<ConfigBasicCredentialProvider>(...)<br/>auth.AddJarvisCompositeScheme(...)
    Sat-->>Host: AuthenticationBuilder
```

**Ví dụ host:**
```csharp
builder.Services.AddJarvisAuthentication(configuration, auth =>
{
    auth.AddCoreJwtBearer(configuration, "Bearer");
    auth.AddCoreApiKey<ConfigApiKeyProvider>(configuration, JarvisAuthenticationSchemes.ApiKey);
    auth.AddCoreBasic<ConfigBasicCredentialProvider>(configuration);
    auth.AddJarvisCompositeScheme(includeBasic: true);   // gộp nhiều scheme
});
```

---

## 3. Composite scheme — chọn scheme theo request

`AddJarvisCompositeScheme` tạo một **policy scheme** (`"Composite"`) không tự xác thực mà *forward* sang scheme con dựa trên header. Đặt `DefaultAuthenticateScheme = "Composite"` để 1 endpoint chấp nhận nhiều loại credential.

```mermaid
flowchart TD
    REQ([Incoming request]) --> HASKEY{Có header<br/>X-API-KEY?}
    HASKEY -->|Có| APIKEY[["→ ApiKey scheme<br/>(Default)"]]
    HASKEY -->|Không| AUTHZ{Authorization<br/>header?}
    AUTHZ -->|Basic ...<br/>và includeBasic| BASIC[["→ Basic scheme"]]
    AUTHZ -->|Bearer ...| BEARER[["→ Bearer scheme"]]
    AUTHZ -->|Không khớp| FALLBACK{includeBasic?}
    FALLBACK -->|true| BASIC
    FALLBACK -->|false| BEARER

    style APIKEY fill:#5f3d2f,color:#fff
    style BASIC fill:#3d2f5f,color:#fff
    style BEARER fill:#2f5f3d,color:#fff
```

> Thứ tự ưu tiên: **ApiKey header → Basic (nếu bật) → Bearer**. Fallback cuối cùng là Basic (nếu `includeBasic`) hoặc Bearer.

---

## 4. Cấu trúc cấu hình (`appsettings.json`)

```mermaid
graph TD
    A["Authentication"] --> T["Type: Jwt | ApiKey | ..."]
    A --> DAS["DefaultAuthenticateScheme"]
    A --> DCS["DefaultChallengeScheme"]
    A --> SC["Schemes.{Jwt|ApiKey|Basic}.Enabled"]
    A --> PP["PasswordPolicy<br/>MinLength, RequireDigit, ..."]
    A --> JWT["Jwt:{scheme}<br/>Authority | IssuerSigningKeys, ..."]
    A --> AK["ApiKey:{realm}<br/>KeyName, Key"]
    A --> BS["Basic:{realm}<br/>Realm, Users{user→pwd,roles}"]

    style A fill:#1e3a5f,color:#fff
```

| Section | Bind vào | Ghi chú |
|---------|----------|---------|
| `Authentication` | `AuthenticationRootOptions` | Root — `Type` bắt buộc (validator) |
| `Authentication:Schemes` | `AuthenticationSchemesEnableOptions` | Toggle scheme qua config, không sửa code |
| `Authentication:Jwt:{scheme}` | `AuthenticationJwtOption` | Mỗi scheme là 1 named option |
| `Authentication:ApiKey:{realm}` | `AuthenticationApiKeyOption` | Multi-realm; realm mặc định `Default` |
| `Authentication:Basic:{realm}` | `AuthenticationBasicOption` | `Users` = dictionary username → credential |

---

## 5. JWT Bearer

### 5.1 Sơ đồ thành phần

```mermaid
classDiagram
    class AuthenticationBuilderExtension {
        +AddCoreJwtBearer(...) [6 overloads]
        +ConfigureJwtBearer(options, authOption)
        +AttachTokenAccessChecker(options)
        -ValidateMaxLifetime(...)
    }
    class AuthenticationJwtOption {
        +string Authority
        +string Audience
        +string[] IssuerSigningKeys
        +bool ValidateIssuer/Audience/...
        +int MaxExpireMinutes
    }
    class AuthenticationJwtOptionValidator {
        +Validate() : cần Authority OR IssuerSigningKeys
    }
    class IJwtTokenAccessChecker {
        <<interface>>
        +IsAllowedAsync(principal, rawToken, ct) bool
    }
    class AllowAllJwtTokenAccessChecker {
        +IsAllowedAsync() then true
    }
    IJwtTokenAccessChecker <|.. AllowAllJwtTokenAccessChecker
    AuthenticationBuilderExtension --> AuthenticationJwtOption : bind + map
    AuthenticationBuilderExtension --> IJwtTokenAccessChecker : gan OnTokenValidated
    AuthenticationJwtOptionValidator --> AuthenticationJwtOption : validate startup
```

### 5.2 Hai chế độ validate

```mermaid
flowchart LR
    OPT[AuthenticationJwtOption] --> Q{Có Authority?}
    Q -->|Có| OIDC["OIDC metadata mode<br/>Authority + Audience<br/>ValidateIssuerSigningKey=false<br/>ValidateIssuer=true"]
    Q -->|Không| SYM["Symmetric key mode (dev/test)<br/>IssuerSigningKeys → SymmetricSecurityKey<br/>ValidateIssuerSigningKey theo config"]
```

### 5.3 Luồng xác thực token

```mermaid
sequenceDiagram
    participant C as Client
    participant JB as JwtBearerHandler
    participant EV as OnTokenValidated
    participant CH as IJwtTokenAccessChecker

    C->>JB: Authorization: Bearer token
    JB->>JB: validate chữ ký + lifetime<br/>(ClockSkew=0)
    alt MaxExpireMinutes > 0
        JB->>JB: ValidateMaxLifetime<br/>(expires - notBefore ≤ Max)
    end
    JB->>EV: token hợp lệ → principal
    EV->>CH: IsAllowedAsync(principal, rawToken)
    Note over CH: blacklist / revoke / whitelist<br/>(mặc định AllowAll → true)
    alt allowed = false
        CH-->>JB: context.Fail("revoked")
    else allowed = true
        CH-->>C: 200 (authenticated)
    end
```

> **Extension point:** `AddCoreJwtBearer<RedisJwtRevocationChecker>(...)` để cắm revoke-list. Checker đăng ký **Singleton** → tra DB dùng `IDbContextFactory`/`IServiceScopeFactory`.

---

## 6. API Key

### 6.1 Sơ đồ thành phần

```mermaid
classDiagram
    class AuthenticationBuilderExtension {
        +AddCoreApiKey~T~(config, scheme, ...)
        -ConfigureApiKeyRealms(...)
    }
    class ConfigApiKeyProvider {
        +ProvideAsync(key) IApiKey
        -Validate(realm, secret, rawKey)
        -TryParseRealmKey(key)
    }
    class ApiKeyModel {
        +string Key
        +string OwnerName
        +Claims
    }
    class AuthenticationApiKeyOption {
        +string KeyName (header)
        +string Key (secret)
    }
    class ApiKeyProviderOptions {
        +string DefaultRealm
        +bool RequireConfigKey
    }
    class AuthenticationApiKeyOptionValidator {
        +Validate() : KeyName bat buoc,<br/>Key bat buoc neu RequireConfigKey
    }
    ConfigApiKeyProvider ..|> IApiKeyProvider
    ConfigApiKeyProvider --> ApiKeyModel : tao khi khop
    ConfigApiKeyProvider --> AuthenticationApiKeyOption : doc theo realm
    AuthenticationBuilderExtension --> ConfigApiKeyProvider : Singleton
    AuthenticationBuilderExtension --> ApiKeyProviderOptions : cau hinh
```

### 6.2 Multi-realm & luồng validate

Header có thể mang prefix `realm:` để chọn realm; không có prefix → realm mặc định (`Default`).

```mermaid
flowchart TD
    H([Header X-API-KEY: value]) --> P{"value chua dau ':' ?"}
    P -->|"Integration:secret"| R1["realm = Integration<br/>secret = secret"]
    P -->|"my-secret"| R2["realm = DefaultRealm (Default)<br/>secret = my-secret"]
    R1 --> V
    R2 --> V
    V{"option.Key rong?"} -->|Có| WARN["log warning → null<br/>(dung custom provider cho DB/Redis)"]
    V -->|Không| EQ{"Key == secret<br/>(Ordinal)?"}
    EQ -->|Không| FAIL[null → 401]
    EQ -->|Có| OK["ApiKeyModel(rawKey, realm)"]

    style OK fill:#2f5f3d,color:#fff
    style FAIL fill:#5f2f2f,color:#fff
    style WARN fill:#5f5f2f,color:#fff
```

> **`RequireConfigKey`:** tự động `true` khi provider là `ConfigApiKeyProvider` (bắt buộc `Key` trong config). Custom provider (DB/vault) → `false`, chỉ cần `KeyName`.

---

## 7. HTTP Basic

### 7.1 Sơ đồ thành phần

```mermaid
classDiagram
    class AuthenticationBuilderExtension {
        +AddCoreBasic~T~(config, scheme, realm, ...)
        -RegisterBasicScheme(...)
    }
    class JarvisBasicAuthenticationHandler {
        +HandleAuthenticateAsync()
        +HandleChallengeAsync() WWW-Authenticate
    }
    class IBasicCredentialProvider {
        <<interface>>
        +AuthenticateAsync(scheme, user, pwd, ct)
    }
    class ConfigBasicCredentialProvider {
        +AuthenticateAsync() doc Options.Users
    }
    class BasicValidationResult {
        +string Username
        +Claims
        +Validate(user, pwd, credential)$
    }
    class AuthenticationBasicOption {
        +string Realm
        +Dictionary Users
    }
    IBasicCredentialProvider <|.. ConfigBasicCredentialProvider
    JarvisBasicAuthenticationHandler --> IBasicCredentialProvider : uy quyen
    ConfigBasicCredentialProvider --> AuthenticationBasicOption : doc Users
    ConfigBasicCredentialProvider --> BasicValidationResult : tao khi khop
    AuthenticationBuilderExtension --> JarvisBasicAuthenticationHandler : AddScheme
```

### 7.2 Luồng xác thực

```mermaid
sequenceDiagram
    participant C as Client
    participant H as JarvisBasicAuthenticationHandler
    participant P as IBasicCredentialProvider

    C->>H: Authorization: Basic base64(user:pass)
    alt khong co header / khong phai "Basic "
        H-->>C: NoResult (bo qua)
    end
    H->>H: base64 decode → "user:pass"
    alt format sai / base64 loi
        H-->>C: Fail 401
    end
    H->>P: AuthenticateAsync(scheme, user, pass, ct)
    P->>P: Users[user]? so khop Password (Ordinal)
    alt khong khop
        P-->>H: null
        H-->>C: Fail 401
    else khop
        P-->>H: BasicValidationResult (claims: Name + Roles)
        H-->>C: Success (ClaimsPrincipal)
    end
    Note over H: Challenge → WWW-Authenticate: Basic realm="..."
```

> ⚠️ Basic dùng password **plain-text** trong config (`BasicUserCredential.Password`) → chỉ dùng dev/test hoặc service-to-service nội bộ. Nguồn thật (DB có hash) → implement `IBasicCredentialProvider` riêng.

---

## 8. Extension points — bảng tổng hợp

| Scheme | Interface | Mặc định | Lifetime DI | Override để... |
|--------|-----------|----------|-------------|----------------|
| JWT | `IJwtTokenAccessChecker` | `AllowAllJwtTokenAccessChecker` | Singleton | Revoke list, blacklist, whitelist (Redis/DB) |
| API Key | `IApiKeyProvider` *(external)* | `ConfigApiKeyProvider` | Singleton | Đọc key từ DB, vault, Redis, MinIO |
| Basic | `IBasicCredentialProvider` | `ConfigBasicCredentialProvider` | Singleton | Đọc user từ DB có password hash |
| Password | `IPasswordPolicyValidator` | `DefaultPasswordPolicyValidator` | Singleton (`TryAdd`) | Password history, breach check |

> **Lưu ý Singleton:** không inject scoped `DbContext` trực tiếp — dùng `IDbContextFactory<TContext>` hoặc `IServiceScopeFactory`. Redis/MinIO client thường Singleton-safe.

---

## 9. Password policy (lõi)

```mermaid
flowchart LR
    IN([password]) --> V[DefaultPasswordPolicyValidator]
    V --> C1{Length ≥ MinLength?}
    V --> C2{RequireDigit → co so?}
    V --> C3{RequireUppercase → co hoa?}
    V --> C4{RequireLowercase → co thuong?}
    V --> C5{RequireNonAlphanumeric → co ky tu dac biet?}
    C1 & C2 & C3 & C4 & C5 --> R{Có lỗi?}
    R -->|Không| OK[PasswordValidationResult.Success]
    R -->|Có| FAIL["PasswordValidationResult.Failed(errors)"]

    style OK fill:#2f5f3d,color:#fff
    style FAIL fill:#5f2f2f,color:#fff
```

> Validator mặc định chỉ enforce các rule độ mạnh trên. Cần thêm (password history, breach check, hết hạn mật khẩu) → override `IPasswordPolicyValidator` trong DI.

---

## 10. Toàn cảnh runtime — một request qua Composite

```mermaid
flowchart TD
    REQ([HTTP Request]) --> DEF["DefaultAuthenticateScheme = Composite"]
    DEF --> SEL{"ForwardDefaultSelector<br/>(theo header)"}

    SEL -->|X-API-KEY| AK["ApiKey handler"]
    SEL -->|Basic| BS["JarvisBasicAuthenticationHandler"]
    SEL -->|Bearer| JW["JwtBearerHandler"]

    AK --> AKP["ConfigApiKeyProvider.ProvideAsync"]
    BS --> BSP["IBasicCredentialProvider.AuthenticateAsync"]
    JW --> JWC["IJwtTokenAccessChecker.IsAllowedAsync"]

    AKP --> RESULT{{"ClaimsPrincipal<br/>hay 401"}}
    BSP --> RESULT
    JWC --> RESULT

    style DEF fill:#1e3a5f,color:#fff
    style RESULT fill:#2f5f3d,color:#fff
```

---

## Phụ lục — Bản đồ file → chức năng

| File | Chức năng |
|------|-----------|
| `Jarvis.Authentication/AuthenticationServiceCollectionExtensions.cs` | Entry point `AddJarvisAuthentication` |
| `Jarvis.Authentication/AuthenticationRootOptions.cs` + `...Validator.cs` | Root config + validate `Type` |
| `Jarvis.Authentication/AuthenticationSchemes(Enable)Options.cs` | Toggle scheme qua config |
| `Jarvis.Authentication/JarvisAuthenticationSchemes.cs` | Hằng số scheme (Composite/Default/Basic/Bearer) |
| `Jarvis.Authentication/AuthenticationBuilderExtensions.cs` | `AddJarvisCompositeScheme` (param `bearerScheme`) |
| `Jarvis.Authentication/*Password*.cs`, `IPasswordPolicyValidator.cs` | Password policy |
| `Jarvis.Authentication.Jwt/AuthenticationBuilderExtension.cs` | `AddCoreJwtBearer`, map options, access checker |
| `Jarvis.Authentication.Jwt/AuthenticationJwtOption(Validator).cs` | JWT options + validate |
| `Jarvis.Authentication.Jwt/*JwtTokenAccessChecker.cs` | Revoke/whitelist hook |
| `Jarvis.Authentication.ApiKey/AuthenticationBuilderExtension.cs` | `AddCoreApiKey`, multi-realm |
| `Jarvis.Authentication.ApiKey/ConfigApiKeyProvider.cs` | Validate key từ config |
| `Jarvis.Authentication.ApiKey/ApiKey*Option*.cs`, `ApiKeyModel.cs` | Options + model |
| `Jarvis.Authentication.Basic/AuthenticationBuilderExtension.cs` | `AddCoreBasic` |
| `Jarvis.Authentication.Basic/JarvisBasicAuthenticationHandler.cs` | Handler Basic tự viết |
| `Jarvis.Authentication.Basic/*Credential*.cs`, `BasicValidationResult.cs` | Provider + kết quả validate |
