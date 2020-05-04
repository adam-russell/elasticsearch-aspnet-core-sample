using System;
using ElasticsearchNETCoreSample.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace ElasticsearchNETCoreSample.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new ConnectionSettings(new Uri(configuration["ElasticsearchSettings:uri"]));

            var defaultIndex = configuration["ElasticsearchSettings:defaultIndex"];

            if (!string.IsNullOrEmpty(defaultIndex))
                settings = settings.DefaultIndex(defaultIndex);

            // The authentication options below are set if you have non-null/empty
            // settings in the configuration.  These are just samples -- there are
            // other authentication methods available.
            var apiKeyId = configuration["ElasticsearchSettings:apiKeyId"];
            var apiKey = configuration["ElasticsearchSettings:apiKey"];

            if (!string.IsNullOrEmpty(apiKeyId) && !string.IsNullOrEmpty(apiKey))
            {
                settings = settings.ApiKeyAuthentication(apiKeyId, apiKey);
            }
            else
            {
                var basicAuthUser = configuration["ElasticsearchSettings:basicAuthUser"];
                var basicAuthPassword = configuration["ElasticsearchSettings:basicAuthPassword"];

                if (!string.IsNullOrEmpty(basicAuthUser) && !string.IsNullOrEmpty(basicAuthPassword))
                    settings = settings.BasicAuthentication(basicAuthUser, basicAuthPassword);
            }

            var client = new ElasticClient(settings);

            // ElasticClient is thread-safe
            // See https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/lifetimes.html
            services.AddSingleton<IElasticClient>(client);
        }
    }
}
