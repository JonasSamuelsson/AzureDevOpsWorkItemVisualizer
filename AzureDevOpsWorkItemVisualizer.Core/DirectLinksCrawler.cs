//using AzureDevOpsWorkItemVisualizer.Core.Model;
//using Handyman.Extensions;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace AzureDevOpsWorkItemVisualizer.Core
//{
//   public class DirectLinksCrawler : Crawler
//   {
//      private readonly IAzureDevOpsClient _client;

//      public DirectLinksCrawler(IAzureDevOpsClient client)
//      {
//         _client = client;
//      }

//      public override async Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinishedWorkItems)
//      {
//         var allRelated = new HashSet<Link>();
//         var fromWorkItemIds = workItemIds.ToSet();
//         var toWorkItemIds = workItemIds.ToSet();

//         var result = new WorkItemCollection();

//         while (true)
//         {
//            workItemIds = fromWorkItemIds
//               .Concat(toWorkItemIds)
//               .Except(result.WorkItems.Select(x => x.Id))
//               .ToSet();

//            if (workItemIds.Any() == false)
//               break;

//            var response = await _client.GetData(workItemIds, workItemTypes, includeFinishedWorkItems);

//            response.WorkItems.ForEach(x => result.WorkItems.Add(x));

//            response.Links
//               .Where(x => x.Type == LinkType.Related)
//               .ForEach(x => allRelated.Add(x));

//            fromWorkItemIds = response.Links
//               .Where(x => x.Type != LinkType.Related)
//               .Where(x => fromWorkItemIds.Contains(x.FromWorkItemId))
//               .Visit(x => result.Links.Add(x))
//               .Select(x => x.ToWorkItemId)
//               .ToSet();

//            toWorkItemIds = response.Links
//               .Where(x => x.Type != LinkType.Related)
//               .Where(x => toWorkItemIds.Contains(x.ToWorkItemId))
//               .Visit(x => result.Links.Add(x))
//               .Select(x => x.FromWorkItemId)
//               .ToSet();
//         }

//         workItemIds = result.WorkItems
//            .Select(x => x.Id)
//            .ToSet();

//         allRelated
//            .Where(x => workItemIds.Contains(x.FromWorkItemId))
//            .Where(x => workItemIds.Contains(x.ToWorkItemId))
//            .ForEach(x => result.Links.Add(x));

//         return result;
//      }
//   }
//}

