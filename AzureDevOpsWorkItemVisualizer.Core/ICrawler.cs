using AzureDevOpsWorkItemVisualizer.Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public interface ICrawler
   {
      Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, CrawlerOptions options);
   }
}