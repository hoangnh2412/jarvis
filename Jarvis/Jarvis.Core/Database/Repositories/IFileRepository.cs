using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Database.Repositories
{
    public interface IFileRepository : IRepository<File>
    {
        Task<File> GetByIdAsync(Guid id);

        /// <summary>
        /// lấy các file theo id
        /// </summary>
        Task<List<File>> QueryByIdsAsync(List<Guid> idDocuments);

        /// <summary>
        /// lấy ra các file được tạo trong khoảng thời gian
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        Task<List<File>> QueryFileInRangeAsync(DateTime from, DateTime to);
    }
}
