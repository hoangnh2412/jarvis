using System;
using System.Collections.Generic;

namespace Jarvis.Core.Models
{
    public class WorkContextModel
    {
        /// <summary>
        /// TenantCode của công ty/chi nhánh đang cần xử lý hoặc lọc dữ liệu
        /// Ví dụ: Công ty A có chi nhánh B và C, tài khoản X thuộc công ty A, tài khoản Y thuộc B
        /// - Nếu X đăng nhập thì TenantCode = A
        /// - Nếu X chuyển chi nhánh từ A sang B thì Tenantcode = B
        /// Khi CRUD (kể cả tài khoản X đăng nhập và chuyển chi nhánh như trường hợp trên) cần lưu các field TenantCode trên DB theo field này để dữ liệu được ở đúng Tenant
        /// </summary>
        /// <value></value>
        public Guid TenantCode { get; set; }

        /// <summary>
        /// UserCode của tài khoản đang đăng nhập
        /// </summary>
        /// <value></value>
        public Guid UserCode { get; set; }

        /// <summary>
        /// OrganizationUnitCode của phòng ban/tổ chức cần xử lý hoặc lọc dữ liệu
        /// </summary>
        /// <value></value>
        public Guid? OrganizationUnitCode { get; set; }

        /// <summary>
        /// OrganizationUnitCodes phòng ban/tổ chức con của OrganizationUnitCode
        /// </summary>
        /// <value></value>
        public List<Guid> OrganizationUnitCodes { get; set; }
    }
}