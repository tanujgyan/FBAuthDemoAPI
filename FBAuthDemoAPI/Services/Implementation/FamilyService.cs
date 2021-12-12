using FBAuthDemoAPI.Models;
using FBAuthDemoAPI.Services.Contract;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FBAuthDemoAPI.Services.Implementation
{
    public class FamilyService : IFamilyService
    {
        private Container _container;
        private static readonly JsonSerializer Serializer = new JsonSerializer();
        public FamilyService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddFamilyDataAsync(Family family)
        {
            await this._container.CreateItemAsync<Family>(family, new PartitionKey(family.Id));
        }
        public async Task DeleteFamilyDataAsync(string id)
        {
            if (await GetFamilyDataFromId(id))
            {
                await this._container.DeleteItemAsync<Family>(id, new PartitionKey($"{id}"));
            }
        }
        public async Task<IEnumerable<Family>> GetFamilyDataAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Family>(new QueryDefinition(queryString));
            List<Family> results = new List<Family>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            return results;
        }

        public async Task UpdateFamilyDataAsync(Family family)
        {
            if (await GetFamilyDataFromId(family.Id))
            {
                await this._container.ReplaceItemAsync<Family>(family, family.Id, new PartitionKey(family.Id));
            }
            

        }
        private async Task<bool> GetFamilyDataFromId(string id)
        {
            //use parameterized query to avoid sql injection
            string query = $"select * from c where c.id=@familyId";
            QueryDefinition queryDefinition = new QueryDefinition(query).WithParameter("@familyId", id);
            List<Family> familyResults = new List<Family>();
            // Item stream operations do not throw exceptions for better performance.
            // Use GetItemQueryStreamIterator instead of GetItemQueryIterator
            //As an exercise change the Get method to use GetItemQueryStreamIterator instead of GetItemQueryIterator
            FeedIterator streamResultSet = _container.GetItemQueryStreamIterator(
             queryDefinition,
             requestOptions: new QueryRequestOptions()
             {
                 PartitionKey = new PartitionKey(id),
                 MaxItemCount = 10,
                 MaxConcurrency = 1
             });
            while (streamResultSet.HasMoreResults)
            {
                using (ResponseMessage responseMessage = await streamResultSet.ReadNextAsync())
                {

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        dynamic streamResponse = FromStream<dynamic>(responseMessage.Content);
                        List<Family> familyResult = streamResponse.Documents.ToObject<List<Family>>();
                        familyResults.AddRange(familyResult);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (familyResults != null && familyResults.Count > 0)
            {
                return true;
            }
            return false;
        }
        private static T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (StreamReader sr = new StreamReader(stream))
                {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
                    {
                        return Serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }
    }
}
