using System;
using System.Threading.Tasks;

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Spatial;

using Index = Microsoft.Azure.Search.Models.Index;

namespace AzureSearch
{
    public static class Program
    {
        private static async Task Main()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var searchServiceName = configuration["SearchServiceName"];
            var adminApiKey = configuration["SearchServiceAdminApiKey"];
            var queryApiKey = configuration["SearchServiceQueryApiKey"];

            var serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            var definition = new Index
            {
                Name = "hotels",
                Fields = FieldBuilder.BuildForType<Hotel>(),
            };

            serviceClient.Indexes.Create(definition);

            var indexClientForUpload = serviceClient.Indexes.GetClient("hotels");

            var hotels = new Hotel[]
            {
                new Hotel()
                {
                    HotelId = "1",
                    BaseRate = 199.0,
                    Description = "Best hotel in town",
                    DescriptionFr = "Meilleur hôtel en ville",
                    HotelName = "Fancy Stay",
                    Category = "Luxury",
                    Tags = new[] { "pool", "view", "wifi", "concierge" },
                    ParkingIncluded = false,
                    SmokingAllowed = false,
                    LastRenovationDate = new DateTimeOffset(2010, 6, 27, 0, 0, 0, TimeSpan.Zero),
                    Rating = 5,
                    Location = GeographyPoint.Create(47.678581, -122.131577)
                },
                new Hotel()
                {
                    HotelId = "2",
                    BaseRate = 79.99,
                    Description = "Cheapest hotel in town",
                    DescriptionFr = "Hôtel le moins cher en ville",
                    HotelName = "Roach Motel",
                    Category = "Budget",
                    Tags = new[] { "motel", "budget" },
                    ParkingIncluded = true,
                    SmokingAllowed = true,
                    LastRenovationDate = new DateTimeOffset(1982, 4, 28, 0, 0, 0, TimeSpan.Zero),
                    Rating = 1,
                    Location = GeographyPoint.Create(49.678581, -122.131577)
                },
                new Hotel()
                {
                    HotelId = "3",
                    BaseRate = 129.99,
                    Description = "Close to town hall and the river"
                }
            };

            var batch = IndexBatch.Upload(hotels);
            await indexClientForUpload.Documents.IndexAsync(batch);

            var indexClientForQuery = new SearchIndexClient(searchServiceName, "hotels", new SearchCredentials(queryApiKey));

            // Search the entire index for the term 'budget' and return only the hotelName field
            var parameters = new SearchParameters()
            {
                Select = new[] { "hotelName" }
            };

            var results = await indexClientForQuery.Documents.SearchAsync<Hotel>("budget", parameters);

            WriteDocuments(results);

            // Apply a filter to the index to find hotels cheaper than $150 per night,
            // and return the hotelId and description
            parameters = new SearchParameters();
            parameters.Filter = "baseRate lt 150";
            parameters.Select = new[] { "hotelId", "description" };

            results = await indexClientForQuery.Documents.SearchAsync<Hotel>("*", parameters);

            WriteDocuments(results);

            // Search the entire index, order by a specific field (lastRenovationDate
            // in descending order, take the top two results, and show only hotelName and lastRenovationDate
            parameters = new SearchParameters()
            {
                OrderBy = new[] { "lastRenovationDate desc" },
                Select = new[] { "hotelName", "lastRenovationDate" },
                Top = 2
            };

            results = indexClientForQuery.Documents.Search<Hotel>("*", parameters);

            WriteDocuments(results);

            // Search the entire index for the term 'motel'
            parameters = new SearchParameters();
            results = indexClientForQuery.Documents.Search<Hotel>("motel", parameters);
            WriteDocuments(results);
        }

        private static void WriteDocuments(DocumentSearchResult<Hotel> results)
        {
            foreach (SearchResult<Hotel> result in results.Results)
            {
                Console.WriteLine(result.Document);
            }

            Console.WriteLine();
        }
    }
}
