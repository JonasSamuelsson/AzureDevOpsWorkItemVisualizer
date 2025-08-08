using AzureDevOpsWorkItemVisualizer.Core.Model;
using System.Collections.Generic;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public class CrawlerOptions
   {
      public bool IncludeFinishedWorkItems { get; set; }
      public bool IncludeRelatedWorkItems { get; set; }
      public bool OptimizeLinks { get; set; }
      public ISet<WorkItemType> WorkItemTypes { get; } = new HashSet<WorkItemType>();
   }
}