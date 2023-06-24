using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Models;
using Nest;

namespace Infrastructure.SearchEngine.Elasticsearch.Services
{
    public interface IElasticsearchService
    {
        Task<Paged<T>> PaginationAsync<T>(SearchEnginePaging request, SearchDescriptor<T> selector) where T : class;

        SearchDescriptor<T> SearchBuilder<T>(
            string index,
            SearchEnginePaging paging,
            Func<QueryContainerDescriptor<T>, QueryContainer> query = null,
            Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort = null,
            Func<SourceFilterDescriptor<T>, ISourceFilter> source = null) where T : class;
    }
}