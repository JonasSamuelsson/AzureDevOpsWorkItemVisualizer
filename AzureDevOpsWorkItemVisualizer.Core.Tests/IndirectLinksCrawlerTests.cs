//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using AzureDevOpsWorkItemVisualizer.Core.Model;
//using Shouldly;
//using Xunit;

//namespace AzureDevOpsWorkItemVisualizer.Core.Tests
//{
//   public class IndirectLinksCrawlerTests
//   {
//      [Fact]
//      public async Task ShouldCrawl()
//      {
//         var crawler = new IndirectLinksCrawler(TestClient.Create());

//         var collection = await crawler.GetData(new HashSet<int> { 2 }, new HashSet<WorkItemType> { WorkItemType.Feature }, false);

//         collection.Links.ShouldContain(x => x.FromWorkItemId == 1 && x.ToWorkItemId == 2);
//         collection.Links.ShouldContain(x => x.FromWorkItemId == 2 && x.ToWorkItemId == 3);
//         collection.Links.ShouldContain(x => x.FromWorkItemId == 3 && x.ToWorkItemId == 4);

//         collection.Links.ShouldContain(x => x.FromWorkItemId == 1 && x.ToWorkItemId == 5);
//         collection.Links.ShouldContain(x => x.FromWorkItemId == 5 && x.ToWorkItemId == 6);
//         collection.Links.ShouldContain(x => x.FromWorkItemId == 6 && x.ToWorkItemId == 4);

//         collection.Links.Count.ShouldBe(6);

//         collection.WorkItems.Select(x => x.Id).OrderBy(x => x).ShouldBe(new[] { 1, 2, 3, 4, 5, 6 });
//      }
//   }
//}

