using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureGraphsDecimalIssue
{
    class Program
    {
        private DocumentClient documentClient;
        private ResourceResponse<DocumentCollection> documentCollection;

        static void Main(string[] args)
        {
            var program = new Program();
            program.Execute();
        }

        private void Execute()
        {
            var connectionUri = "https://INSERT_COSMOS_DB_ACCOUNTNAME.documents.azure.com:443/";
            var password = "INSERT_READ_WRITE_KEY";
            var database = "INSERT_DATABASE_NAME";
            var collection = "INSERT_COLLECTION_NAME";
            var connectionMode = ConnectionMode.Gateway;
            var protocol = Protocol.Https;

            this.documentClient = new DocumentClient(new Uri(connectionUri), password, new ConnectionPolicy { ConnectionMode = connectionMode, ConnectionProtocol = protocol });
            this.documentCollection = documentClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(database, collection), new RequestOptions { }).Result;


            //Expected result would be the value "1.23" in the property "price" and the value "-123" in the property "someNegativeValue"
            //Unfortunately, both values have "123"
            this.ExecuteQuery<JObject>("g.addV('Product').property('name','milk').property('price',1.23).property('someNegativeValue',-123)").Wait();
            Thread.Sleep(60000);
        }

        private async Task ExecuteQuery<T>(string requestScript)
        {
            var query = this.documentClient.CreateGremlinQuery<T>(this.documentCollection, requestScript);
            while (query.HasMoreResults)
            {
                var next = await query.ExecuteNextAsync<T>();
                foreach (var partialResult in next)
                {
                    Console.WriteLine(partialResult.ToString());
                }
            }
        }
    }
}
