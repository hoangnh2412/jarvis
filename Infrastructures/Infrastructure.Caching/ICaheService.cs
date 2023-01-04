using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Caching
{
    public interface ICacheService : IDistributedCache
    {
        /// <summary>
        /// Search cache key by pattern use KEYS command: https://redis.io/commands/keys
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        Task<List<string>> GetKeysAsync(string pattern, CancellationToken token = default);

        Task KeyExistAsync(string pattern, CancellationToken token = default);

        Task<Dictionary<string, string>> HashGetAsync(string key, CancellationToken token = default);

        Task<string> HashGetAsync(string key, string hashField, CancellationToken token = default);

        /// <summary>
        /// Get all item in hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> HashGetAsync<T>(string key, CancellationToken token = default);

        /// <summary>
        /// Get items in hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> HashGetAsync<T>(string key, List<string> hashFields, CancellationToken token = default);

        /// <summary>
        /// Get items in hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<string>> HashGetAsync(string key, List<string> hashFields, CancellationToken token = default);

        /// <summary>
        /// Get an item in hash table by hash key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> HashGetAsync<T>(string key, string hashField, CancellationToken token = default);

        /// <summary>
        /// Set hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task HashSetAsync(string key, Dictionary<string, string> data, DistributedCacheEntryOptions options = null, CancellationToken token = default);
        Task HashSetAsync<T>(string key, Dictionary<string, T> data, DistributedCacheEntryOptions options = null, CancellationToken token = default);

        /// <summary>
        /// Set an item into hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task HashSetAsync(string key, string hashField, string data, DistributedCacheEntryOptions options = null, CancellationToken token = default);

        /// <summary>
        /// Delete an item in hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task HashDeleteAsync(string key, string hashField, CancellationToken token = default);

        /// <summary>
        /// Delete items in hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task HashDeleteAsync(string key, string[] hashFields, CancellationToken token = default);

        /// <summary>
        /// Push item into queue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<long> PushAsync(string key, string value, CancellationToken token = default);

        /// <summary>
        /// Push many items into queue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<long> PushAsync(string key, string[] values, CancellationToken token = default);

        /// <summary>
        /// Get an item from queue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<string> PopAsync(string key, CancellationToken token = default);

        Task ExecuteCommandAsync(string key);
    }
}