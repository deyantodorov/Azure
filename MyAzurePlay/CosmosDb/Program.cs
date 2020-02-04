using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;

namespace CosmosDb
{
    public class Program
    {
        private static DocumentClient _client;
        private const string _databaseId = "myDatabase";
        private const string _collectionId = "Families";
        private const string _endpoint = @"https://mycosmos1672596425.documents.azure.com:443/";
        private const string _key = "nHtNDpC2RWXCOzFlVmM5BTz3gADvRn0aB9HiaaimDCLEwHyIVHcsrADBnHkQk1IxMPAsKKi6tGr5StG55SVphw==";

        private static async Task Main()
        {
            await RunAsync();
        }

        private static async Task RunAsync()
        {
            _client = new DocumentClient(new Uri(_endpoint), _key);

            await _client.CreateDatabaseIfNotExistsAsync(new Database
            {
                Id = _databaseId
            });

            await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_databaseId),
                new DocumentCollection
                {
                    Id = _collectionId,
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new Collection<string>(new[] { "/id" })
                    }
                });

            var family1 = JObject.Parse(await File.ReadAllTextAsync("data/andersen.json"));
            var family2 = JObject.Parse(await File.ReadAllTextAsync("data/wakefield.json"));

            await CreateDocumentIfNotExistsAsync(_databaseId, _collectionId, family1["id"].ToString(), family1);

            await CreateDocumentIfNotExistsAsync(_databaseId, _collectionId, family1["id"].ToString(), family2);

            ExecuteSqlQuery(_databaseId, _collectionId, 
                @"
                SELECT *
                FROM Families f
                WHERE f.id = 'AndersenFamily'
                ");

            ExecuteSqlQuery(_databaseId, _collectionId, 
                @"
                SELECT {""Name"":f.id, ""City"":f.address.city} AS Family
                    FROM Families f
                    WHERE f.address.city = f.address.state
                ");

            ExecuteSqlQuery(_databaseId, _collectionId,
                @"
                SELECT c.givenName
                    FROM Families f
                    JOIN c IN f.children
                    WHERE f.id = 'WakefieldFamily'
                    ORDER BY f.address.city ASC");

        }

        private static async Task CreateDocumentIfNotExistsAsync(
            string databaseId,
            string collectionId,
            string documentId,
            JObject data)
        {
            try
            {
                await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, documentId),
                    new RequestOptions
                    {
                        PartitionKey = new PartitionKey(documentId)
                    });

                Console.WriteLine($"Family {documentId} already exists in the database");
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), data);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task<string> GetDocumentByIdAsync(
            string databaseId,
            string collectionId,
            string documentId
        )
        {
            var response = await _client.ReadDocumentAsync(
                UriFactory.CreateDocumentUri(databaseId, collectionId, documentId), new RequestOptions
                {
                    PartitionKey = new PartitionKey(documentId)
                });

            Console.WriteLine(response.Resource);

            return response.Resource.ToString();
        }

        private static void ExecuteSqlQuery(
            string databaseId,
            string collectionId,
            string sql
        )
        {
            Console.WriteLine($"SQL: {sql}");

            var queryOptions = new FeedOptions()
            {
                MaxItemCount = -1,
                EnableCrossPartitionQuery = true,
            };

            var sqlQuery = _client.CreateDocumentQuery<JObject>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), sql, queryOptions);

            foreach (var result in sqlQuery)
            {
                Console.WriteLine(result);
            }
        }
    }
}
