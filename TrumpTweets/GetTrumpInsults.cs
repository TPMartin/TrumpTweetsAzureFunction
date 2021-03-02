using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using System;

namespace TrumpTweets
{
    public class InsultTweet
    {
        public int InsultId { get; set; }
        public string Date { get; set; }
        public string Target { get; set; }
        public string Tweet { get; set; }
        public string Id { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }
    public static class GetTrumpTweets
    {
        [FunctionName("GetTrumpTweets")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, 
            ILogger log)
        {
            StringValues key; 
            if (req.Headers.ContainsKey("apikey") && req.Headers.TryGetValue("apikey", out key))
            {
                if (key.FirstOrDefault() == Environment.GetEnvironmentVariable("APIKEY"))
                {
                    StringValues target;
                    if (req.Headers.ContainsKey("target") && req.Headers.TryGetValue("target", out target))
                    {
                        string cosmosUrl = "https://tmtrumptweets.documents.azure.com:443/";
                        string cosmosKey = Environment.GetEnvironmentVariable("COSMOSKEY");
                        string dbName = "trumptweetsdb";
                        string containerName = "trumptweetscontainer";

                        CosmosClient client = new CosmosClient(cosmosUrl, cosmosKey);
                        Database database = client.GetDatabase(dbName);
                        Container container = database.GetContainer(containerName);

                        var response = container.GetItemLinqQueryable<InsultTweet>(true).Where(x => x.Target.Contains(target));
                        List<string> insults = new List<string>();
                        foreach (var tweet in response)
                        {
                            insults.Add(tweet.Tweet);
                        }
                        var insultListObject = JsonConvert.SerializeObject(insults, Formatting.Indented);

                        return new OkObjectResult(insultListObject);
                    }
                    else
                    {
                        return new BadRequestResult();
                    }
                }
                else
                {
                    return new UnauthorizedResult();
                }
            }
            else
            {
                return new UnauthorizedResult();
            }

        }
    }
}
