using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ElasticsearchNETCoreSample.Models;
using ElasticsearchNETCoreSample.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;

namespace ElasticsearchNETCoreSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<HomeController> _logger; 

        public HomeController(
            IElasticClient elasticClient,
            ILogger<HomeController> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Text from public domain books.  Can be downloaded from multiple sources,
            // but for example: https://dp.la
            var books = new List<Book>()
            {
                new Book
                {
                    Id = 1,
                    Title = "Narrative of the Life of Frederick Douglass",
                    Opening = "I was born in Tuckahoe, near Hillsborough, and about twelve miles from Easton, in Talbot county, Maryland. I have no accurate knowledge of my age, never having seen any authentic record containing it. By far the larger part of the slaves know as little of their ages as horses know of theirs, and it is the wish of most masters within my knowledge to keep their slaves thus ignorant.",
                    Genre = BookGenre.Biography,
                    Author = new Author
                    {
                        FirstName = "Frederick",
                        LastName = "Douglass"
                    },
                    InitialPublishYear = 1845
                },
                new Book
                {
                    Id = 2,
                    Title = "A Tale of Two Cities",
                    Opening = "It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness, it was the epoch of belief, it was the epoch of incredulity, it was the season of Light, it was the season of Darkness, it was the spring of hope, it was the winter of despair, we had everything before us, we had nothing before us, we were all going direct to Heaven, we were all going direct the other way﻿—in short, the period was so far like the present period, that some of its noisiest authorities insisted on its being received, for good or for evil, in the superlative degree of comparison only.",
                    Genre = BookGenre.HistoricalFiction,
                    Author = new Author
                    {
                        FirstName = "Charles",
                        LastName = "Dickens"
                    },
                    InitialPublishYear = 1859
                },
                new Book
                {
                    Id = 3,
                    Title = "On the Origin of Species",
                    Opening = "When we compare the individuals of the same variety or sub-variety of our older cultivated plants and animals, one of the first points which strikes us is, that they generally differ more from each other than do the individuals of any one species or variety in a state of nature. And if we reflect on the vast diversity of the plants and animals which have been cultivated, and which have varied during all ages under the most different climates and treatment, we are driven to conclude that this great variability is due to our domestic productions having been raised under conditions of life not so uniform as, and somewhat different from, those to which the parent species had been exposed under nature.",
                    Genre = BookGenre.Science,
                    Author = new Author
                    {
                        FirstName = "Charles",
                        LastName = "Darwin"
                    },
                    InitialPublishYear = 1859
                },
                new Book
                {
                    Id = 4,
                    Title = "Oh Pioneers!",
                    Opening = "One January day, thirty years ago, the little town of Hanover, anchored on a windy Nebraska tableland, was trying not to be blown away. A mist of fine snowflakes was curling and eddying about the cluster of low drab buildings huddled on the gray prairie, under a gray sky. The dwelling-houses were set about haphazard on the tough prairie sod; some of them looked as if they had been moved in overnight, and others as if they were straying off by themselves, headed straight for the open plain.",
                    Genre = BookGenre.HistoricalFiction,
                    Author = new Author
                    {
                        FirstName = "Willa",
                        LastName = "Cather"
                    },
                    InitialPublishYear = 1913
                },
                new Book
                {
                    Id = 5,
                    Title = "Moby Dick",
                    Opening = "Call me Ishmael. Some years ago﻿—never mind how long precisely﻿—having little or no money in my purse, and nothing particular to interest me on shore, I thought I would sail about a little and see the watery part of the world. It is a way I have of driving off the spleen and regulating the circulation.",
                    Genre = BookGenre.Adventure,
                    Author = new Author
                    {
                        FirstName = "Herman",
                        LastName = "Melville"
                    },
                    InitialPublishYear = 1851
                }
            };

            foreach (var book in books)
            {
                var existsResponse = await _elasticClient.DocumentExistsAsync<Book>(book);

                // If the document already exists, we're going to update it; otherwise insert it
                // Note:  You may get existsResponse.IsValid = false for a number of issues
                // ranging from an actual server issue, to mismatches with indices (e.g. a
                // mismatch on the datatype of Id).
                if (existsResponse.IsValid && existsResponse.Exists)
                {
                    var updateResponse = await _elasticClient.UpdateAsync<Book>(book, u => u.Doc(book));

                    if (!updateResponse.IsValid)
                    {
                        var errorMsg = "Problem updating document in Elasticsearch.";
                        _logger.LogError(updateResponse.OriginalException, errorMsg);
                        throw new Exception(errorMsg);
                    }
                }
                else
                {
                    var insertResponse = await _elasticClient.IndexDocumentAsync(book);

                    if (!insertResponse.IsValid)
                    {
                        var errorMsg = "Problem inserting document to Elasticsearch.";
                        _logger.LogError(insertResponse.OriginalException, errorMsg);
                        throw new Exception(errorMsg);
                    }
                }
            }

            var vm = new HomeViewModel
            {
                InsertedData = JsonConvert.SerializeObject(books, Formatting.Indented)
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
