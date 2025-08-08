using AzureDevOpsWorkItemVisualizer.Core.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Core.Tests
{
   internal class TestClient : IAzureDevOpsClient
   {
      public IEnumerable<Link> Links { get; set; }
      public IEnumerable<WorkItem> WorkItems { get; set; }

      public Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeRelated, bool includeFinished)
      {
         var ids = workItemIds.ToList();

         var links = Links.Where(x => ids.Contains(x.FromWorkItemId) || ids.Contains(x.ToWorkItemId)).ToHashSet();
         var workItems = WorkItems.Where(x => ids.Contains(x.Id)).ToHashSet();

         return Task.FromResult(new WorkItemCollection
         {
            Links = links,
            WorkItems = workItems
         });
      }

      public static TestClient Create()
      {
         //   5-6
         //  /   \
         // 1-2-3-4
         //  \ 
         //   7-8
         //  /
         // 9

         return new TestClient
         {
            Links = new[]
            {
               new Link { FromWorkItemId = 1, ToWorkItemId = 2, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 2, ToWorkItemId = 3, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 3, ToWorkItemId = 4, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 1, ToWorkItemId = 5, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 5, ToWorkItemId = 6, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 6, ToWorkItemId = 4, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 1, ToWorkItemId = 7, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 7, ToWorkItemId = 8, Type = LinkType.HasChild },
               new Link { FromWorkItemId = 9, ToWorkItemId = 7, Type = LinkType.HasChild },
            },
            WorkItems = new[]
            {
               new WorkItem { Id = 1, Type = WorkItemType.Feature },
               new WorkItem { Id = 2, Type = WorkItemType.Feature },
               new WorkItem { Id = 3, Type = WorkItemType.Feature },
               new WorkItem { Id = 4, Type = WorkItemType.Feature },
               new WorkItem { Id = 5, Type = WorkItemType.Feature },
               new WorkItem { Id = 6, Type = WorkItemType.Feature },
               new WorkItem { Id = 7, Type = WorkItemType.Feature },
               new WorkItem { Id = 8, Type = WorkItemType.Feature },
               new WorkItem { Id = 9, Type = WorkItemType.Feature }
            }
         };
      }
   }
}