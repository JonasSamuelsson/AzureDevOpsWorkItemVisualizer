using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsWorkItemVisualizer.Core.Model;
using Handyman.Extensions;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public class DirectLinksCrawler : ICrawler
   {
      private readonly IAzureDevOpsClient _client;

      public DirectLinksCrawler(IAzureDevOpsClient client)
      {
         _client = client;
      }

      public async Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinishedWorkItems)
      {
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

            var response = await _client.GetData(workItemIds, workItemTypes, includeFinishedWorkItems);

            response.WorkItems.ForEach(x => result.WorkItems.Add(x));

            fromWorkItemIds = response.Links
               .Where(x => fromWorkItemIds.Contains(x.FromWorkItemId))
               .Visit(x => result.Links.Add(x))
               .Select(x => x.ToWorkItemId)
               .ToSet();

            toWorkItemIds = response.Links
               .Where(x => toWorkItemIds.Contains(x.ToWorkItemId))
               .Visit(x => result.Links.Add(x))
               .Select(x => x.FromWorkItemId)
               .ToSet();
         }

         return result;
      }
   }
}