using AzureDevOpsWorkItemVisualizer.Core;
using AzureDevOpsWorkItemVisualizer.Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Console
{
   public static class Program
   {
      public static async Task Main()
      {
         var options = new AzureDevOpsClientOptions
         {
         };

         var workItemIds = new HashSet<int>
         {
         };

         var includeWorkItemTypes = new HashSet<WorkItemType>
         {
            WorkItemType.Epic,
            WorkItemType.Feature,
            WorkItemType.PBI
         };

         var client = new AzureDevOpsClient(options);
         var crawler = new DirectLinksCrawler(client);
         var data = await crawler.GetData(workItemIds, includeWorkItemTypes, includeFinishedWorkItems: false);

         var graph = new GraphGenerator().GenerateGraph(data, workItemIds, "LR");

         // http://magjac.com/graphviz-visual-editor/ can be used to test the graph

         System.Console.WriteLine(graph);
      }
   }
}