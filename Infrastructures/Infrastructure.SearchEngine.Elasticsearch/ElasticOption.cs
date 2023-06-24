using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.SearchEngine.Elasticsearch
{
    public class ElasticOption
    {
        public List<string> Endpoints { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string IndexPrefix { get; set; }
    }
}