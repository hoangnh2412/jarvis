using System;
using System.Threading.Tasks;

namespace Jarvis.Core.Abstractions
{
    public interface IUserInfoService
    {
        /// <summary>
        /// Lấy thông tin tài khoản
        /// </summary>
        /// <param name="id">IdUser: Lấy từ bảng core_user</param>
        /// <returns></returns>
        Task<string> GetAsync(Guid id);

        /// <summary>
        /// Tạo thông tin tài khoản
        /// </summary>
        /// <param name="id">IdUser: Lấy từ bảng core_user</param>
        /// <param name="metadata">Thông tin tài khoản (dạng JSON)</param>
        Task CreateAsync(Guid id, string metadata);

        /// <summary>
        /// Sửa thông tin tài khoản
        /// </summary>
        /// <param name="id">IdUser: Lấy từ bảng core_user</param>
        /// <param name="metadata">Thông tin tài khoản (dạng JSON)</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, string metadata);

        /// <summary>
        /// Xóa thông tin tài khoản
        /// </summary>
        /// <param name="id">IdUser: Lấy từ bảng core_user</param>
        /// <returns></returns>
        Task DeleteAsync(Guid id);
    }
}