namespace Jarvis.Swashbuckle;

/// <summary>
/// Quy ước team cho Swagger/OpenAPI (mô tả DTO + ví dụ trong UI). Class tĩnh chỉ để gom tài liệu; không cần gọi runtime.
/// </summary>
/// <remarks>
/// <para><b>Cách dùng file này</b></para>
/// <list type="number">
/// <item>
/// <description>
/// Không cần <c>using</c> hay khởi tạo: mở file trong IDE hoặc xem IntelliSense khi gõ <c>SwaggerExamplesConvention</c> để nhớ checklist.
/// </description>
/// </item>
/// <item>
/// <description>
/// Trên project host (Web API): bật <c>&lt;GenerateDocumentationFile&gt;true&lt;/GenerateDocumentationFile&gt;</c> (và tùy chọn <c>NoWarn 1591</c> nếu chưa doc hết),
/// để Swashbuckle (trong <c>AddCoreSwagger</c>) đọc file XML và hiển thị <c>summary</c>/<c>example</c> trên schema.
/// </description>
/// </item>
/// <item>
/// <description>
/// Thêm package <c>Swashbuckle.AspNetCore.Filters</c> trên host nếu dùng attribute/provider (đã có sẵn khi host reference <c>Jarvis.Swashbuckle</c> thường vẫn cần reference trực tiếp package này cho attribute).
/// </description>
/// </item>
/// <item>
/// <description>
/// Với mỗi request/response quan trọng: tạo DTO có XML (<c>summary</c>, <c>param</c>, <c>example</c> trên property khi cần).
/// </description>
/// </item>
/// <item>
/// <description>
/// Để có ô &quot;Example Value&quot; trong Swagger UI: implement <c>IExamplesProvider&lt;T&gt;</c> (hoặc request example tương ứng) và gắn
/// <c>[SwaggerResponseExample(200, typeof(MyProvider))]</c> / <c>[SwaggerRequestExample(typeof(...))]</c> lên action.
/// <c>AddCoreSwagger</c> đã gọi <c>AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly())</c> — provider phải nằm trong assembly entry (thường là project host).
/// </description>
/// </item>
/// <item>
/// <description>
/// Tham khảo mẫu đầy đủ trong solution Sample: <c>Sample.Models.UserV1GetData</c> + <c>Sample.Swagger.UserV1GetResponseExampleProvider</c> + <c>User1Controller.GetV1</c>.
/// </description>
/// </item>
/// <item>
/// <description>
/// Upload file: không dùng <c>[FromForm]</c> trên <c>IFormFile</c> (Swashbuckle lỗi generate); dùng <c>IFormFile</c> thuần hoặc DTO form và <c>[Consumes(&quot;multipart/form-data&quot;)]</c>.
/// </description>
/// </item>
/// </list>
/// <para>
/// Tóm tắt kỹ thuật: XML doc trên DTO; <c>Swashbuckle.AspNetCore.Filters</c> cho ví dụ request/response; <c>AddCoreSwagger</c> đã gọi <c>AddSwaggerExamplesFromAssemblies(entryAssembly)</c>.
/// </para>
/// </remarks>
public static class SwaggerExamplesConvention
{
}
