using AzureDevOpsWorkItemVisualizer.Core.Model;
using Handyman.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public class GraphGenerator
   {
      private static readonly Dictionary<WorkItemType, string> BackgroundColors = new Dictionary<WorkItemType, string>
      {
         { WorkItemType.Bug, "Red" },
         { WorkItemType.Epic, "DarkOrange" },
         { WorkItemType.Feature, "Indigo" },
         { WorkItemType.PBI, "DeepSkyBlue" },
         { WorkItemType.Task, "Yellow" }
      };

      private static readonly Dictionary<WorkItemType, string> FontColors = new Dictionary<WorkItemType, string>
      {
         { WorkItemType.Bug, "White" },
         { WorkItemType.Epic, "White" },
         { WorkItemType.Feature, "White" },
         { WorkItemType.PBI, "Black" },
         { WorkItemType.Task, "Black" }
      };

      public string GenerateGraph(WorkItemCollection data, Options options)
      {
         var builder = new StringBuilder();

         builder.AppendLine("digraph {");

         builder.AppendLine($"  rankdir = {options.RankDir};");

         foreach (var item in data.WorkItems.OrderBy(x => x.Id))
         {
            var attributes = new Dictionary<string, string>();

            var segments = new object[] { $"{item.Type} {item.Id}", item.State, item.Tags.Join(", "), item.AssignedTo };
            var metadata = string.Join(" / ", segments.Select(x => x.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)));

            var name = WebUtility.HtmlEncode(item.Name);

            var highlight = options.HighlightedWorkItemIds.Contains(item.Id);

            attributes["label"] = $"<<table border=\"0\"><tr><td>{metadata}</td></tr><tr><td>{name}</td></tr></table>>";
            attributes["shape"] = "box";
            attributes["style"] = highlight ? "\"bold,filled,rounded\"" : "\"filled,rounded\"";
            attributes["color"] = item.IsFinished ? BackgroundColors[item.Type] : "Black";
            attributes["fillcolor"] = item.IsFinished ? "transparent" : BackgroundColors[item.Type];
            attributes["fontcolor"] = item.IsFinished ? "Black" : FontColors[item.Type];
            attributes["fontsize"] = highlight ? "18" : "14";
            attributes["URL"] = $"https://dev.azure.com/{options.AzureDevOpsOrganization}/{options.AzureDevOpsProject}/_workitems/edit/{item.Id}";
            attributes["target"] = "_blank";

            builder.AppendLine($"  {item.Id} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         foreach (var link in data.Links.OrderBy(x => x.Identifier))
         {
            var attributes = new Dictionary<string, string>();
            attributes["label"] = link.Type.ToString();
            builder.AppendLine($"  {link.FromWorkItemId} -> {link.ToWorkItemId} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         builder.AppendLine("}");

         return builder.ToString();
      }

      public class Options
      {
         public string AzureDevOpsOrganization { get; set; }
         public string AzureDevOpsProject { get; set; }
         public ISet<int> HighlightedWorkItemIds { get; set; }
         public string RankDir { get; set; }
      }
   }
}