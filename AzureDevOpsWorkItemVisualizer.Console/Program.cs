using AzureDevOpsWorkItemVisualizer.Core;
using AzureDevOpsWorkItemVisualizer.Core.Model;
using Handyman.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Console
{
   public static class Program
   {
      public static async Task Main()
      {
         var clientOptions = new AzureDevOpsClientOptions();
         var workItemIds = new HashSet<int>();

#if DEBUG
         if (TryReadSettings(out var settings))
         {
            clientOptions.Organization = settings.GetValueOrDefault("Organization", () => string.Empty);
            clientOptions.PersonalAccessToken = settings.GetValueOrDefault("PersonalAccessToken", () => string.Empty);
            clientOptions.Project = settings.GetValueOrDefault("Project", () => string.Empty);
            workItemIds = settings.GetValueOrDefault("WorkItemIds", () => string.Empty)
               .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(int.Parse)
               .ToHashSet();
         }
#endif

         var client = new AzureDevOpsClient(clientOptions);
         var crawler = new Crawler(client);
         var crawlerOptions = new CrawlerOptions
         {
            IncludeFinishedWorkItems = false,
            IncludeRelatedWorkItems = false,
            OptimizeLinks = true,
            WorkItemTypes =
            {
               WorkItemType.Epic,
               WorkItemType.Feature,
               WorkItemType.PBI
            }
         };
         var data = await crawler.FetchData(workItemIds, crawlerOptions);

         var graph = new GraphGenerator().GenerateGraph(data, new GraphGenerator.Options
         {
            AzureDevOpsOrganization = clientOptions.Organization,
            AzureDevOpsProject = clientOptions.Project,
            HighlightedWorkItemIds = workItemIds,
            RankDir = "TD"
         });

         // http://magjac.com/graphviz-visual-editor/ can be used to test the graph

         System.Console.WriteLine(graph);
      }

      private static bool TryReadSettings(out Dictionary<string, string> settings)
      {
         var file = "AzureDevOpsWorkItemsVisualizer.Console.settings.json";
         var dir = Environment.CurrentDirectory;

         while (dir != null)
         {
            var path = Path.Combine(dir, file);

            if (File.Exists(path))
            {
               settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
               return true;
            }

            dir = Path.GetDirectoryName(dir);
         }

         settings = null;
         return false;
      }
   }
}