using System;
using System.Collections.Generic;
using System.Text;
using Jarvis.Core.Constants;

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
        public Guid TenantCode { get; set; }

        /// <summary>
        /// id của tk đăng nhập hiện tại
        /// </summary>
        public Guid IdUser { get; set; }

        /// <summary>
        /// tên quyền
        /// </summary>
        public string ClaimOfOperation { get; set; }

        /// <summary>
        /// quyền doanh nghiệp
        /// </summary>
        public ClaimOfResource ClaimOfResource { get; set; }

        /// <summary>
        /// quyền chi nhánh
        /// </summary>
        public ClaimOfChildResource ClaimOfChildResource { get; set; }
    }
}
