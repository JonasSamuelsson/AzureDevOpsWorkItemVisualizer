using AzureDevOpsWorkItemVisualizer.Core.Model;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AzureDevOpsWorkItemVisualizer.Core.Tests;

public class CrawlerTests
{
   [Fact]
   public void ShouldOptimizeLinks()
   {
      var links = new HashSet<Link>
      {
         new() { FromWorkItemId = 1, ToWorkItemId = 2 },
         new() { FromWorkItemId = 1, ToWorkItemId = 3 },
         new() { FromWorkItemId = 2, ToWorkItemId = 3 }
      };

      Crawler.OptimizeLinks(links);

      links
         .OrderBy(x => x.FromWorkItemId)
         .ShouldBe(new Link[]
         {
            new() { FromWorkItemId = 1, ToWorkItemId = 2 },
            new() { FromWorkItemId = 2, ToWorkItemId = 3 }
         });
   }
}