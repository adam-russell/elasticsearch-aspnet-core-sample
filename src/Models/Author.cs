using System;
using Nest;

namespace ElasticsearchNETCoreSample.Models
{
    [ElasticsearchType(RelationName = "author")]
    public class Author
    {
        public string FirstName { get; set; }
        public string LastName { get; set;}
    }
}
