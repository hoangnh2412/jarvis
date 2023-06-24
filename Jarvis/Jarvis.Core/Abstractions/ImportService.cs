using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Services;

namespace Jarvis.Core.Abstractions
{
    public abstract class ImportService<TUnitOfWork, TKey, TEntity, TModel, TImportInput> 
        : IImportService<TUnitOfWork, TKey, TEntity, TModel, TImportInput>
        where TUnitOfWork : IUnitOfWork
        where TEntity : class, IEntity<TKey>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity, ILogDeletedVersionEntity<TKey>
    {
        private readonly TUnitOfWork _uow;
        private readonly IDomainWorkContext _workContext;

        public ImportService(
            TUnitOfWork uow,
            IDomainWorkContext workContext)
        {
            _uow = uow;
            _workContext = workContext;
        }

        public async Task<int> ImportAsync(byte[] bytes, bool useBulk = false)
        {
            await OnImportBeginAsync(bytes);

            var items = Read(bytes, (item) =>
            {
                return Parse(item);
            });

            var repo = _uow.GetRepository<IRepository<TEntity>>();
            await repo.InsertsAsync(items);
            var result = await _uow.CommitAsync();

            if (result > 0)
                await OnImportSuccessAsync(items, result);
            else
                await OnImportFailAsync(items, result);

            await OnImportEndAsync(items, result);
            return result;
        }

        protected virtual IEnumerable<TEntity> Read(byte[] bytes, Func<TImportInput, TEntity> funcParser, CsvConfiguration config = null)
        {
            if (config == null)
            {
                config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = Environment.NewLine,
                    HasHeaderRecord = true
                };
            }

            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var reader = new StreamReader(memoryStream, System.Text.Encoding.UTF8))
                {
                    using (var csv = new CsvReader(reader, config))
                    {
                        var items = csv.GetRecords<TImportInput>();

                        foreach (var item in items)
                        {
                            yield return funcParser.Invoke(item);
                        }
                    }
                }
            }
        }

        protected virtual TEntity Parse(TImportInput input)
        {
            var entity = MapToEntity(input);

            entity.TenantCode = _workContext.GetTenantKey();

            entity.Key = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedAtUtc = DateTime.UtcNow;
            entity.CreatedBy = _workContext.GetUserKey();

            return entity;
        }

        protected abstract TEntity MapToEntity(TImportInput input);

        protected virtual Task OnImportEndAsync(IEnumerable<TEntity> input, int result)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnImportFailAsync(IEnumerable<TEntity> input, int result)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnImportSuccessAsync(IEnumerable<TEntity> input, int result)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnImportBeginAsync(byte[] bytes)
        {
            return Task.CompletedTask;
        }

    }
}