using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Infrastructure.Database.Models;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace Infrastructure.SearchEngine.Elasticsearch.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticClient _highlevelClient;
        private readonly ElasticOption _options;
        private readonly ILogger<ElasticsearchService> _logger;

        public ElasticsearchService(
            IOptions<ElasticOption> options,
            ILogger<ElasticsearchService> logger)
        {
            _options = options.Value;
            _logger = logger;

            var nodes = new List<Uri>();
            foreach (var item in options.Value.Endpoints)
            {
                nodes.Add(new Uri(item));
            }

            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool);
            settings.BasicAuthentication(options.Value.UserName, options.Value.Password);
            settings.ThrowExceptions(alwaysThrow: true);
            settings.DisableDirectStreaming();
            settings.EnableDebugMode();
            _highlevelClient = new ElasticClient(settings);
        }

        public async Task<Paged<T>> PaginationAsync<T>(SearchEnginePaging request, SearchDescriptor<T> selector) where T : class
        {
            var size = request.Size ?? 10;
            var page = request.Page ?? 1;
            selector
                .From((page - 1) * size)
                .Size(size);

            _logger.LogDebug(_highlevelClient.RequestResponseSerializer.SerializeToString(selector));
            var response = await _highlevelClient.SearchAsync<T>(selector);

            var pages = response.Total / size;
            if (pages < page)
                pages = 1;

            if (response.Total % size > 0 && response.Total > size)
                pages++;

            return new Paged<T>
            {
                Page = page,
                Q = request.Q,
                Size = size,
                TotalItems = (int)response.Total,
                TotalPages = (int)pages,
                Data = response.Documents
            };
        }

        public SearchDescriptor<T> SearchBuilder<T>(
            string index,
            SearchEnginePaging paging,
            Func<QueryContainerDescriptor<T>, QueryContainer> query = null,
            Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort = null,
            Func<SourceFilterDescriptor<T>, ISourceFilter> source = null) where T : class
        {
            var selector = new SearchDescriptor<T>();

            var indexes = new List<string>();
            if (paging.From != null && paging.To != null)
            {
                var months = DateTimeExtension.MonthsBetween(paging.From.Value, paging.To.Value);
                foreach (var item in months)
                {
                    indexes.Add($"{_options.IndexPrefix}_{index}_{item.Year}-{item.Month}");
                }
            }
            else
            {
                indexes.Add($"{_options.IndexPrefix}_{index}_*");
            }

            selector
                .Index(indexes.ToArray())
                .TrackTotalHits(true)
                .From((paging.Page - 1) * paging.Size)
                .Size(paging.Size);

            if (query != null)
                selector.Query(descriptor => query.Invoke(descriptor));

            if (sort != null)
                selector.Sort(descriptor => sort.Invoke(descriptor));

            if (source != null)
                selector.Source(descriptor => source.Invoke(descriptor));

            return selector;
        }
    }
}