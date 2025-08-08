using AzureDevOpsWorkItemVisualizer.Core.Model;
using Handyman.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public class Crawler
   {
      private readonly IAzureDevOpsClient _client;

      public Crawler(IAzureDevOpsClient client)
      {
         _client = client;
      }

      public async Task<WorkItemCollection> GetData(ISet<int> workItemIds, CrawlerOptions options)
      {
         var collection = await FetchData(workItemIds, options);

         if (options.OptimizeLinks)
         {
            OptimizeLinks(collection.Links);
         }

         return collection;
      }

      public async Task<WorkItemCollection> FetchData(ISet<int> workItemIds, CrawlerOptions options)
      {
         var allRelatedLinks = new HashSet<Link>();
         var fromWorkItemIds = workItemIds.ToSet();
         var toWorkItemIds = workItemIds.ToSet();

         var result = new WorkItemCollection();

         while (true)
         {
            workItemIds = fromWorkItemIds
               .Concat(toWorkItemIds)
               .Except(result.WorkItems.Select(x => x.Id))
               .ToSet();

            if (workItemIds.Any() == false)
               break;

            var response = await FetchData(_client, workItemIds, options);

            response.WorkItems.ForEach(x => result.WorkItems.Add(x));

            response.Links
               .Where(x => x.Type == LinkType.Related)
               .ForEach(x => allRelatedLinks.Add(x));

            fromWorkItemIds = response.Links
               .Where(x => x.Type != LinkType.Related)
               .Where(x => fromWorkItemIds.Contains(x.FromWorkItemId))
               .Visit(x => result.Links.Add(x))
               .Select(x => x.ToWorkItemId)
               .ToSet();

            toWorkItemIds = response.Links
               .Where(x => x.Type != LinkType.Related)
               .Where(x => toWorkItemIds.Contains(x.ToWorkItemId))
               .Visit(x => result.Links.Add(x))
               .Select(x => x.FromWorkItemId)
               .ToSet();
         }

         workItemIds = result.WorkItems
            .Select(x => x.Id)
            .ToSet();

         var relatedWorkItemIds = allRelatedLinks
            .SelectMany(x => new[] { x.FromWorkItemId, x.ToWorkItemId })
            .Where(x => !workItemIds.Contains(x))
            .ToSet();

         if (relatedWorkItemIds.Any())
         {
            var response = await FetchData(_client, relatedWorkItemIds, options);

            response.WorkItems.ForEach(x => result.WorkItems.Add(x));
         }

         allRelatedLinks.ForEach(x => result.Links.Add(x));

         return result;
      }

      private static Task<WorkItemCollection> FetchData(IAzureDevOpsClient client, ISet<int> workItemIds, CrawlerOptions options)
      {
         var workItemTypes = options.WorkItemTypes;
         var includeRelatedWorkItems = options.IncludeRelatedWorkItems;
         var includeFinishedWorkItems = options.IncludeFinishedWorkItems;

         return client.GetData(workItemIds, workItemTypes, includeRelatedWorkItems, includeFinishedWorkItems);
      }

      public static void OptimizeLinks(ISet<Link> links)
      {
         var candidates = links
            .Where(x => x.Type != LinkType.Related)
            .ToList();

         var linksByFromId = candidates.ToLookup(x => x.FromWorkItemId);

         foreach (var link in candidates.ToList())
         {
            var sources = linksByFromId
               .GetElementsOrEmpty(link.FromWorkItemId)
               .Where(x => x != link)
               .ToList();

            foreach (var source in sources)
            {
               if (!IsLinked(source.ToWorkItemId, link.ToWorkItemId, linksByFromId))
                  continue;

               links.Remove(link);
               break;
            }
         }
      }

      private static bool IsLinked(int fromWorkItemId, int toWorkItemId, ILookup<int, Link> linksByFromId)
      {
         if (fromWorkItemId == toWorkItemId)
            return true;

         return linksByFromId
            .GetElementsOrEmpty(fromWorkItemId)
            .Any(x => IsLinked(x.ToWorkItemId, toWorkItemId, linksByFromId));
      }
   }
}