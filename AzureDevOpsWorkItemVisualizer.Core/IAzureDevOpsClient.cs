using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDevOpsWorkItemVisualizer.Core.Model;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public interface IAzureDevOpsClient
   {
      Task<WorkItemCollection> GetData(ISet<int> workItemIds, ISet<WorkItemType> workItemTypes, bool includeFinished);
   }
}