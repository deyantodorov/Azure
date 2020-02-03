using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StorageTables
{
    public class Tables
    {
        private const string connectionString = "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=storage16916;AccountKey=qbNTjUPQLLkj9MHPJE3MqQpz71y7GjbNJKfY5kwye02+Gpmg4M8MD4pWRkPm4eXJ9vAVyjkreke1DHNTrv6Shg==";

        public static async Task RunAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var tableClient = storageAccount.CreateCloudTableClient();

            var gamersTable = tableClient.GetTableReference("Gamers");

            await gamersTable.CreateIfNotExistsAsync();

            await DeleteAllGamersAsync(gamersTable);

            var gamer1 = new Gamer("bleu@game.net", "France", "Bleu", "123123123");
            await AddAsync(gamersTable, gamer1);

            var gamers = new List<Gamer> {
                new Gamer("mike@game.net", "US", "Mike", "555-1212"),
                new Gamer("mike@contoso.net", "US", "Mike", "555-1234")
            };

            await AddBatchAsync(gamersTable, gamers);

            var bleu = await GetAsync<Gamer>(gamersTable, "France", "bleu@game.net");
            Console.WriteLine(bleu);

            gamers = await FindGamersByNameAsync(gamersTable, "Mike");
            gamers.ForEach(Console.WriteLine);
        }

        public static async Task AddAsync<T>(CloudTable table, T entity) where T : TableEntity
        {
            var insert = TableOperation.Insert(entity);
            await table.ExecuteAsync(insert);
        }

        public static async Task AddBatchAsync<T>(CloudTable table, IEnumerable<T> entities) where T : TableEntity
        {
            var batch = new TableBatchOperation();

            foreach (var entity in entities)
            {
                batch.Insert(entity);
            }

            await table.ExecuteBatchAsync(batch);
        }

        private static async Task DeleteAllGamersAsync(CloudTable table)
        {
            var gamers = new List<Gamer>()
            {
                await GetAsync<Gamer>(table, "US", "mike@game.net"),
                await GetAsync<Gamer>(table, "US", "mike@contoso.net"),
                await GetAsync<Gamer>(table, "France", "bleu@game.net")
            };

            gamers.ForEach(async gamer =>
            {
                if (gamer != null)
                {
                    await DeleteAsync(table, gamer);
                }
            });
        }

        public static async Task<T> GetAsync<T>(CloudTable table, string pk, string rk) where T : TableEntity
        {
            var retrieve = TableOperation.Retrieve<Gamer>(pk, rk);
            var result = await table.ExecuteAsync(retrieve);

            return (T)result.Result;
        }

        public static async Task DeleteAsync<T>(CloudTable table, T entity) where T : TableEntity
        {
            var retrieve = TableOperation.Delete(entity);
            await table.ExecuteAsync(retrieve);
        }

        public static async Task<List<Gamer>> FindGamersByNameAsync(CloudTable table, string name)
        {
            var filterCondition = TableQuery.GenerateFilterCondition("Name", QueryComparisons.Equal, name);

            var query = new TableQuery<Gamer>().Where(filterCondition);

            var results = await table.ExecuteQuerySegmentedAsync(query, null);

            return results.ToList();
        }
    }
}
