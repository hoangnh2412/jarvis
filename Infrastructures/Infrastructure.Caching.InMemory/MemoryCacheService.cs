using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infrastructure.Caching.InMemory
{
    public class MemoryCacheService : MemoryDistributedCache, ICacheService
    {
        public MemoryCacheService(IOptions<MemoryDistributedCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
        }

        public MemoryCacheService(IOptions<MemoryDistributedCacheOptions> optionsAccessor, ILoggerFactory loggerFactory) : base(optionsAccessor, loggerFactory)
        {
        }

        public Task ExecuteCommandAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetKeysAsync(string pattern, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task HashDeleteAsync(string key, string hashField, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task HashDeleteAsync(string key, string[] hashFields, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> HashGetAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> HashGetAsync(string key, string hashField, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<T>> HashGetAsync<T>(string key, CancellationToken token = default)
        {
            var bytes = await base.GetAsync(key);
            if (bytes == null)
                return new List<T>();

            var data = Encoding.UTF8.GetString(bytes);
            if (data == null)
                return new List<T>();

            var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            var objs = obj.Select(x => JsonConvert.DeserializeObject<T>(x.Value.ToString())).ToList();
            return objs;
        }

        public Task<List<T>> HashGetAsync<T>(string key, List<string> hashFields, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> HashGetAsync(string key, List<string> hashFields, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> HashGetAsync<T>(string key, string hashField, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task HashSetAsync(string key, Dictionary<string, string> data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
        {
            var json = JsonConvert.SerializeObject(data);
            await base.SetAsync(key, Encoding.UTF8.GetBytes(json), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

        public Task HashSetAsync<T>(string key, Dictionary<string, T> data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task HashSetAsync(string key, string hashField, string data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task KeyExistAsync(string pattern, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> PopAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> PushAsync(string key, string value, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> PushAsync(string key, string[] values, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task<T> QueryCacheKeyAsync<T>(string cache, Func<Task<T>> query, DistributedCacheEntryOptions options = null, CancellationToken token = default)
        {
            var bytes = await GetAsync(cache);
            if (bytes != null)
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));

            var data = await query.Invoke();
            await SetAsync(cache, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), options);
            return data;
        }

        public async Task<T> QueryHashKeyAsync<T>(string cache, string key, Func<Task<T>> query, DistributedCacheEntryOptions options = null, CancellationToken token = default)
        {
            var data = await HashGetAsync<T>(cache, key);
            if (data != null)
                return data;

            var item = await query.Invoke();
            await HashSetAsync(cache, key, JsonConvert.SerializeObject(item));
            return item;
        }

        public async Task<List<T>> QueryHashKeysAsync<T>(string cache, List<string> keys, Func<Task<List<T>>> query, Func<List<T>, Dictionary<string, T>> parser, DistributedCacheEntryOptions options = null, CancellationToken token = default)
        {
            var data = await HashGetAsync<T>(cache, keys);
            if (data != null)
                return data;

            var item = await query.Invoke();
            await HashSetAsync(cache, parser.Invoke(item));
            return item;
        }
    }
}
