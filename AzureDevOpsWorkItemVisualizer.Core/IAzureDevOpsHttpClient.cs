using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public interface IAzureDevOpsHttpClient
   {
      Task<IEnumerable<T>> GetWorkItems<T>(IEnumerable<int> ids);
   }
}