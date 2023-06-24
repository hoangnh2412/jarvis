using System;

namespace Jarvis.Core.Models
{
    public class ContextModel
    {
        /// <summary>
        /// lưu thông tin của tài khoản đăng nhập hiện tại
        /// </summary>
        public SessionModel Session { get; set; }

        /// <summary>
        /// tenantCode của chi nhánh/công ty được chọn hiện tại
        /// </summary>
        public Guid TenantKey { get; set; }

        /// <summary>
        /// id của tk đăng nhập hiện tại
        /// </summary>
        public Guid UserKey { get; set; }
    }
}
