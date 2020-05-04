using System;
using System.Collections.Generic;
using ElasticsearchNETCoreSample.Models;

namespace ElasticsearchNETCoreSample.ViewModels
{
    public class SearchViewModel
    {
        public string Term { get; set; }
        public List<Book> Results { get; set; }
    }
}
