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
        List<string> GetKeys(string pattern);

        /// <summary>
        /// Search cache key by pattern use SCAN command (recommend): https://redis.io/commands/scan
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        List<string> ScanKeys(string pattern);

        /// <summary>
        /// Get all item in hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> HashGetAsync<T>(string key, CancellationToken token = default);

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
        Task HashSetAsync(string key, Dictionary<string, string> data, DistributedCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// Set an item into hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task HashSetAsync(string key, string hashField, string data, CancellationToken token = default);

        /// <summary>
        /// Delete an item in hash table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task HashDeleteAsync(string key, string hashField, CancellationToken token = default);
    }
}