using Handyman.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public class AzureDevOpsHttpClient : IAzureDevOpsHttpClient
   {
      private static readonly HttpClient HttpClient = new HttpClient();

      private readonly AzureDevOpsClientOptions _options;

      public AzureDevOpsHttpClient(AzureDevOpsClientOptions options)
      {
         _options = options;
      }

      public async Task<IEnumerable<T>> GetWorkItems<T>(IEnumerable<int> ids)
      {
         return (await ids
               .Distinct()
               .OrderBy(x => x)
               .Chunk(200)
               .Select(x => $"workitems?api-version=6.0&$expand=relations&ids={string.Join(",", x)}")
               .Select(Load<T[]>)
               .WhenAll())
            .SelectMany(x => x);
      }

      private async Task<T> Load<T>(string uri)
      {
         var organization = _options.Organization;
         var project = _options.Project;
         var baseUri = new Uri($"https://dev.azure.com/{organization}/{project}/_apis/wit/");

         var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($".:{_options.PersonalAccessToken}"));

         var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, uri))
         {
            Headers =
            {
               Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
               Authorization = new AuthenticationHeaderValue("Basic", auth)
            }
         };

         var response = await HttpClient.SendAsync(request);

         response.EnsureSuccessStatusCode();

         return (await response.Content.ReadAsAsync<Response<T>>()).Value;
      }

      public class Response<T>
      {
         public T Value { get; set; }
      }
   }
}
