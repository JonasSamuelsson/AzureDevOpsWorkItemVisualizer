using AzureDevOpsWorkItemVisualizer.Core.Model;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AzureDevOpsWorkItemVisualizer.Core.Tests;

public class CrawlerTests
{
   [Fact]
   public async Task ShouldGetData()
   {
      var crawler = new Crawler(TestClient.Create());
      var options = new CrawlerOptions
      {
         IncludeFinishedWorkItems = false,
         IncludeRelatedWorkItems = false,
         OptimizeLinks = false,
         WorkItemTypes =
         {
            WorkItemType.Feature
         }
      };

      var collection = await crawler.FetchData(new HashSet<int> { 2 }, options);

      collection.Links.ShouldContain(x => x.FromWorkItemId == 1 && x.ToWorkItemId == 2);
      collection.Links.ShouldContain(x => x.FromWorkItemId == 2 && x.ToWorkItemId == 3);
      collection.Links.ShouldContain(x => x.FromWorkItemId == 3 && x.ToWorkItemId == 4);

      collection.Links.Count.ShouldBe(3);

      collection.WorkItems.Select(x => x.Id).OrderBy(x => x).ShouldBe(new[] { 1, 2, 3, 4 });
   }

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