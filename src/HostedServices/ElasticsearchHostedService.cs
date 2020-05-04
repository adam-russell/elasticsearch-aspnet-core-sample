using System;
using System.Threading;
using System.Threading.Tasks;
using ElasticsearchNETCoreSample.Models;
using Microsoft.Extensions.Hosting;
using Nest;

namespace ElasticsearchNETCoreSample.HostedServices
{
    // See https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-3/
    // for a great article on running async tasks on app startup in .NET Core 3
    public class ElasticsearchHostedService : IHostedService
    {
        private readonly IElasticClient _elasticClient;

        public ElasticsearchHostedService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // We're creating a single index below, but you could do other initialization
            // tasks for Elasticsearch here.
            var booksIndexName = "books";

            // The check for whether this index exists and subsequently deleting
            // it if it does is for demo purposes!  This is so we can make changes
            // in our code and have them reflected in the index.  In production,
            // you would not want to do this.
            if ((await _elasticClient.Indices.ExistsAsync(booksIndexName)).Exists)
                await _elasticClient.Indices.DeleteAsync(booksIndexName);

            var createMoviesIndexResponse = await _elasticClient.Indices.CreateAsync(booksIndexName, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .TokenFilters(tf => tf
                            .Stop("english_stop", st => st
                                .StopWords("_english_")
                            )
                            .Stemmer("english_stemmer", st => st
                                .Language("english")
                            )
                            .Stemmer("light_english_stemmer", st => st
                                .Language("light_english")
                            )
                            .Stemmer("english_possessive_stemmer", st => st
                                .Language("possessive_english")
                            )
                            .Synonym("book_synonyms", st => st
                                // If you have a lot of synonyms, it's probably better to create a synonyms
                                // text file and use .SynonymsPath here instead.
                                .Synonyms(
                                    "haphazard,indiscriminate,erratic",
                                    "incredulity,amazement,skepticism")
                            )
                        )
                        .Analyzers(aa => aa
                            .Custom("light_english", ca => ca
                                .Tokenizer("standard")
                                .Filters("light_english_stemmer", "english_possessive_stemmer", "lowercase", "asciifolding")
                            )
                            .Custom("full_english", ca => ca
                                .Tokenizer("standard")
                                .Filters("english_possessive_stemmer",
                                        "lowercase",
                                        "english_stop",
                                        "english_stemmer",
                                        "asciifolding")
                            )
                            .Custom("full_english_synopsis", ca => ca
                                .Tokenizer("standard")
                                .Filters("book_synonyms",
                                        "english_possessive_stemmer",
                                        "lowercase",
                                        "english_stop",
                                        "english_stemmer",
                                        "asciifolding")
                            )
                        )
                    )
                )
                .Map<Book>(m => m
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Title)
                            .Analyzer("light_english")
                        )
                        .Text(t => t
                            .Name(n => n.Opening)
                            .Analyzer("full_english_synopsis")
                        )
                    )
                )
            );
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
