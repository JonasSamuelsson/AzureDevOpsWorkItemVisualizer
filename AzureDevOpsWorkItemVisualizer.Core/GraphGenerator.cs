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
      public string GenerateGraph(WorkItemCollection data, ISet<int> highlightWorkItemIds, string rankdir)
      {
         var builder = new StringBuilder();

         builder.AppendLine("digraph g {");

         builder.AppendLine($"  rankdir = {rankdir};");

         foreach (var item in data.WorkItems.OrderBy(x => x.Id))
         {
            var attributes = new Dictionary<string, string>();

            var segments = new[] { (object)$"{item.Type} {item.Id}", item.State, item.Tags.Join(", "), item.AssignedTo };
            var metadata = string.Join(" / ", segments.Select(x => x.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)));
            var name = WebUtility.HtmlEncode(item.Name);

            var highlight = highlightWorkItemIds.Contains(item.Id);

            attributes["label"] = $"<<table border=\"0\"><tr><td>{metadata}</td></tr><tr><td>{name}</td></tr></table>>";
            attributes["shape"] = "box";
            attributes["style"] = highlight ? "\"bold,filled,rounded\"" : "\"filled,rounded\"";
            attributes["fillcolor"] = item.IsFinished
               ? "transparent"
               : item.Type == WorkItemType.Epic
                  ? "orange"
                  : item.Type == WorkItemType.Feature
                     ? "lightskyblue"
                     : "palegreen";
            attributes["fontcolor"] = highlight ? "black" : "gray25";
            attributes["fontsize"] = highlight ? "16" : "14";
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
   }
}