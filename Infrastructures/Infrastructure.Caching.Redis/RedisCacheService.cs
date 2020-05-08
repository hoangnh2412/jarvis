using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Infrastructure.Caching.Redis
{
    public class RedisCacheService : RedisCache, ICacheService
    {
        private volatile ConnectionMultiplexer _connection;
        private IDatabase _cache;

        private readonly RedisCacheOptions _options;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisCacheService(IOptions<RedisCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;
        }

        public List<string> GetKeys(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            Connect();

            var keys = new List<string>();
            var servers = GetServers(_options.Configuration);
            foreach (var item in servers)
            {
                var server = _connection.GetServer(item.Key, item.Value);
                var data = server.Keys(pattern: $"{_options.InstanceName}{pattern}").Select(x => x.ToString());
                keys.AddRange(data);
            }
            return keys;
        }

        public List<string> ScanKeys(string pattern)
        {
            throw new NotImplementedException();
        }

        public async Task<List<T>> HashGetAsync<T>(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            key = $"{_options.InstanceName}:{key}";
            await ConnectAsync();

            var data = await _cache.HashGetAllAsync(key);
            return data.Select(x => JsonConvert.DeserializeObject<T>(x.Value.ToString())).ToList();
        }

        public async Task<T> HashGetAsync<T>(string key, string hashField, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            key = $"{_options.InstanceName}:{key}";
            await ConnectAsync();

            var data = await _cache.HashGetAsync(key, hashField);
            return JsonConvert.DeserializeObject<T>(data.ToString());
        }

        public async Task HashSetAsync(string key, Dictionary<string, string> data, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            key = $"{_options.InstanceName}:{key}";
            await ConnectAsync();

            await _cache.HashSetAsync(key, data.Select(x => new HashEntry(x.Key, x.Value)).ToArray());

            if (options != null && options.AbsoluteExpirationRelativeToNow.HasValue)
                await _cache.KeyExpireAsync(key, options.AbsoluteExpirationRelativeToNow.Value);
        }

        public async Task HashSetAsync(string key, string hashField, string data, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            key = $"{_options.InstanceName}:{key}";
            await ConnectAsync();

            await _cache.HashSetAsync(key, hashField, data);
        }

        public async Task HashDeleteAsync(string key, string hashField, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            key = $"{_options.InstanceName}:{key}";
            await ConnectAsync();

            await _cache.HashDeleteAsync(key, hashField);
        }



        private async Task ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            if (_cache != null)
            {
                return;
            }

            await _connectionLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (_cache == null)
                {
                    if (_options.ConfigurationOptions != null)
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions).ConfigureAwait(false);
                    }
                    else
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration).ConfigureAwait(false);
                    }

                    _cache = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private void Connect()
        {
            if (_cache != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_cache == null)
                {
                    if (_options.ConfigurationOptions != null)
                    {
                        _connection = ConnectionMultiplexer.Connect(_options.ConfigurationOptions);
                    }
                    else
                    {
                        _connection = ConnectionMultiplexer.Connect(_options.Configuration);
                    }

                    _cache = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private List<KeyValuePair<string, int>> GetServers(string configuration)
        {
            var servers = new List<KeyValuePair<string, int>>();
            var hosts = configuration.Split(',');
            foreach (var item in hosts)
            {
                var splited2 = item.Split(':');
                if (splited2.Length == 1)
                    servers.Add(new KeyValuePair<string, int>(splited2[0], 6379));
                else
                    servers.Add(new KeyValuePair<string, int>(splited2[0], int.Parse(splited2[1])));
            }
            return servers;
        }

    }
}
