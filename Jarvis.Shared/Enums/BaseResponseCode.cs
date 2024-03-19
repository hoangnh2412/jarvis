using Jarvis.Shared.Constants;

namespace Jarvis.Shared.Enums;

public partial class BaseResponseCode : Enumeration
{
    public BaseResponseCode(int id, string name) : base(id, name)
    {
    }

    public static BaseResponseCode OK = new BaseResponseCode(200, nameof(OK));
    public static BaseResponseCode NoContent = new BaseResponseCode(204, nameof(NoContent));
    public static BaseResponseCode BadRequest = new BaseResponseCode(400, nameof(BadRequest));
    public static BaseResponseCode Unauthorized = new BaseResponseCode(401, nameof(Unauthorized));
    public static BaseResponseCode Forbidden = new BaseResponseCode(403, nameof(Forbidden));
    public static BaseResponseCode NotFound = new BaseResponseCode(404, nameof(NotFound));
    public static BaseResponseCode Conflict = new BaseResponseCode(409, nameof(Conflict));
    public static BaseResponseCode Unknown = new BaseResponseCode(999999, nameof(Unknown));
}
