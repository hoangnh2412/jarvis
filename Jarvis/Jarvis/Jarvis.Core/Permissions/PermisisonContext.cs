using System.Collections.Generic;

namespace Jarvis.Core.Permissions
{
    public class PermisisonContext
    {
        /// <summary>
        /// Id quyền, sử dụng int để áp dụng bitwise
        /// </summary>
        /// <value></value>
        public int Id { get; set; }

        /// <summary>
        /// Tên quyền
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Thao tác sử dụng (Controller.Action)
        /// </summary>
        /// <value></value>
        public string ClaimOfOperations { get; set; }

        /// <summary>
        /// Tài nguyên sử dụng
        /// </summary>
        /// <value></value>
        public string ClaimOfResource { get; set; }

        /// <summary>
        /// Điều kiện dc sử dụng
        /// </summary>
        /// <value></value>
        public string Condition { get; set; }
    }
}