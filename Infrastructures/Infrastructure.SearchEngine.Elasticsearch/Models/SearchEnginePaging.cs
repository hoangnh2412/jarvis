using System;
using Infrastructure.Database.Models;

namespace Infrastructure.SearchEngine.Elasticsearch
{
    public class SearchEnginePaging : Paging
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}