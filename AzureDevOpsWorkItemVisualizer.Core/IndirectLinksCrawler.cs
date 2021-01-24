using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsWorkItemVisualizer.Core.Model;
using Handyman.Extensions;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public class IndirectLinksCrawler : ICrawler
   {
      private readonly IAzureDevOpsClient _client;

      public IndirectLinksCrawler(IAzureDevOpsClient client)
      {
         _client = client;
      }

      public async Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinishedWorkItems)
      {
         var data = await LoadData(workItemIds, workItemTypes, includeFinishedWorkItems);

         return BuildGraph(workItemIds, data);
      }

      private async Task<WorkItemCollection> LoadData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinished)
      {
         var data = new WorkItemCollection();

         while (true)
         {
            // get all work items from origin to top
            var response = await _client.GetData(workItemIds, workItemTypes, includeFinished);

            response.Links.ForEach(x => data.Links.Add(x));
            response.WorkItems.ForEach(x => data.WorkItems.Add(x));

            var loadedWorkItemIds = data.WorkItems.Select(x => x.Id).ToSet();

            workItemIds = data.Links
               .Select(x => x.FromWorkItemId)
               .Where(x => loadedWorkItemIds.Contains(x) == false)
               .ToSet();

            if (workItemIds.Any() == false)
               break;
         }

         while (true)
         {
            // get all work items from top to bottom
            var loadedWorkItemIds = data.WorkItems
               .Select(x => x.Id)
               .ToSet();

            workItemIds = data.Links
               .Select(x => x.ToWorkItemId)
               .Where(x => loadedWorkItemIds.Contains(x) == false)
               .ToSet();

            if (workItemIds.Any() == false)
               break;

            var response = await _client.GetData(workItemIds, workItemTypes, includeFinished);

            response.Links.ForEach(x => data.Links.Add(x));
            response.WorkItems.ForEach(x => data.WorkItems.Add(x));
         }

         return data;
      }

      private static WorkItemCollection BuildGraph(ISet<int> workItemIds, WorkItemCollection data)
      {
         var fromWorkItemIds = GetEdgeWorkItemIds(data.Links, workItemIds, x => x.ToWorkItemId, x => x.FromWorkItemId);
         var toWorkItemIds = GetEdgeWorkItemIds(data.Links, workItemIds, x => x.FromWorkItemId, x => x.ToWorkItemId);

         workItemIds = GetWorkItemIds(data.Links, fromWorkItemIds, toWorkItemIds);

         return new WorkItemCollection
         {
            Links = data.Links
               .Where(x => workItemIds.Contains(x.FromWorkItemId) && workItemIds.Contains(x.ToWorkItemId))
               .ToSet(),
            WorkItems = data.WorkItems
               .Where(x => workItemIds.Contains(x.Id))
               .ToSet()
         };
      }

      private static ISet<int> GetEdgeWorkItemIds(ISet<Link> links, ISet<int> workItemIds, Func<Link, int> sourceIdSelector, Func<Link, int> targetIdSelector)
      {
         var result = new HashSet<int>();

         while (workItemIds.Any())
         {
            workItemIds
               .Where(x => links.Any(y => x == sourceIdSelector(y)) == false)
               .ForEach(x => result.Add(x));

            workItemIds = links
               .Where(x => workItemIds.Contains(sourceIdSelector(x)))
               .Select(targetIdSelector)
               .ToSet();
         }

         return result;
      }

      private static ISet<int> GetWorkItemIds(ISet<Link> links, ISet<int> fromWorkItemIds, ISet<int> toWorkItemIds)
      {
         var workItemIds = new HashSet<int>();

         foreach (var fromWorkItemId in fromWorkItemIds)
         {
            foreach (var path in GetPaths(links, fromWorkItemId, toWorkItemIds))
            {
               path.ForEach(x => workItemIds.Add(x));
            }
         }

         return workItemIds;
      }

      private static IEnumerable<IEnumerable<int>> GetPaths(ISet<Link> links, int fromWorkItemId, ISet<int> toWorkItemIds)
      {
         if (toWorkItemIds.Contains(fromWorkItemId))
         {
            yield return new[] { fromWorkItemId };
            yield break;
         }

         foreach (var link in links.Where(x => x.FromWorkItemId == fromWorkItemId))
         {
            foreach (var path in GetPaths(links, link.ToWorkItemId, toWorkItemIds))
            {
               var list = new List<int>(new[] { fromWorkItemId });
               list.AddRange(path);
               yield return list;
            }
         }
      }
   }
}