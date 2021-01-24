using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDevOpsWorkItemVisualizer.Core.Model;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public interface ICrawler
   {
      Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinishedWorkItems);
   }
}