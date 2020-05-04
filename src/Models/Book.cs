using System;
using Elasticsearch.Net;
using Nest;

namespace ElasticsearchNETCoreSample.Models
{
    public enum BookGenre
    {
        None = 0,
        Adventure,
        Biography,
        Drama,
        HistoricalFiction,
        Science
    }

    // You can read more about attribute mapping in NEST at:
    // https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/attribute-mapping.html
    [ElasticsearchType(RelationName = "book", IdProperty = nameof(Id))]
    public class Book
    {
        public int Id { get; set; }

        public Author Author { get; set; }

        [Text(Boost = 1.5)]
        public string Title { get; set; }

        public string Opening { get; set; }

        [StringEnum]
        public BookGenre Genre { get; set; }

        [Ignore]
        public int InitialPublishYear { get; set; }
    }
}
