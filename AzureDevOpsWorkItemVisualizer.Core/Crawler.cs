using AzureDevOpsWorkItemVisualizer.Core.Model;
using Handyman.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public abstract class Crawler : ICrawler
   {
      public async Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, CrawlerOptions options)
      {
         var collection = await GetData(workItemIds, workItemTypes, options.IncludeFinishedWorkItems);

         if (options.OptimizeLinks)
         {
            OptimizeLinks(collection.Links);
         }

         return collection;
      }

      public abstract Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinishedWorkItems);

      public static void OptimizeLinks(ISet<Link> links)
      {
         var fromIds = links
            .Select(x => x.FromWorkItemId)
            .Distinct()
            .ToList();

         var linksByFromId = links.ToLookup(x => x.FromWorkItemId);

         foreach (var link in links.ToList())
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