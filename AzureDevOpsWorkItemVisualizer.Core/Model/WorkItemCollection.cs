using System.Collections.Generic;

namespace AzureDevOpsWorkItemVisualizer.Core.Model
{
   public class WorkItemCollection
   {
      public ISet<Link> Links { get; set; } = new HashSet<Link>();
      public ISet<WorkItem> WorkItems { get; set; } = new HashSet<WorkItem>();
   }
}