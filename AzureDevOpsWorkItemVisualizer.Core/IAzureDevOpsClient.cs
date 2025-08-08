using AzureDevOpsWorkItemVisualizer.Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public interface IAzureDevOpsClient
   {
      Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeRelated, bool includeFinished);
   }
}