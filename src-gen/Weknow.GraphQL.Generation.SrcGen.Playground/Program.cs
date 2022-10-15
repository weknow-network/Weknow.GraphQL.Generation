using System;

using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Microsoft.Extensions.DependencyInjection;

using static System.Net.WebRequestMethods;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;

// GraphQL GitHubExplorer: https://docs.github.com/en/graphql/overview/explorer


namespace Weknow.GraphQL.Generation.SrcGen.Playground
{
    class Program
    {
        private static readonly string TOKEN_PLAN = Environment.GetEnvironmentVariable("GITHUB_API_TOKEN") ?? string.Empty;
        private static readonly string TOKEN = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(TOKEN_PLAN));

        private const string GITHUB_API_URL = "https://api.github.com/graphql";

        private static readonly GraphQLRequest QUERY = new GraphQLRequest
        {
            Query = @"
                {
  repository(owner: ""weknow-network"", name: ""Event-Source-Backbone"") {
    createdAt
    forkCount
  }
}"
        };
        //private static readonly GraphQLRequest QUERY = new GraphQLRequest
        //{
        //    Query = @"
        //        {
        //          repository($owner: String!,$name: String!) {
        //            repository(owner: $owner, name: $name) {
        //              createdAt
        //              forkCount
        //              commitComments {
        //                totalCount
        //              }
        //            }
        //          }
        //        }",
        //    Variables = new
        //    {
        //        owner = "weknow-network",
        //        name = "Event-Source-Backbone"
        //    }
        //};

        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();


            var options = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(GITHUB_API_URL),
            };
            serviceCollection
                .AddSingleton<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>()
                .AddSingleton(options)
                .AddSingleton<IGraphQLClient>(sp =>
                {
                    var serializer = sp.GetRequiredService<IGraphQLWebsocketJsonSerializer>();
                    var graphClient = new GraphQLHttpClient("https://api.github.com/graphql", serializer);
                    var auth = new AuthenticationHeaderValue("Basic", TOKEN);
                    graphClient.HttpClient.DefaultRequestHeaders.Authorization = auth;
                    return graphClient;
                });

            IServiceProvider services = serviceCollection.BuildServiceProvider();
            IGraphQLClient client = services.GetRequiredService<IGraphQLClient>();
            //client.HttpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {TOKEN}");

            var graphQLResponse1 = await client.SendQueryAsync<GitHubRepo>(QUERY);

        }
    }
}
