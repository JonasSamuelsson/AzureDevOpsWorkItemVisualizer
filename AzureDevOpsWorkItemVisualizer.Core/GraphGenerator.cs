using AzureDevOpsWorkItemVisualizer.Core.Model;
using Handyman.Extensions;
using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace AzureDevOpsWorkItemVisualizer.Core
{
   public class GraphGenerator
   {
      private const string AssigneeIcon = "&#xF264;";
      private const string TagIcon = "&#xF023;";
      private const string GithubIcon = "&#xEDCB;";
      private const string GraphTextFontFace = "Helvetica Neue";

      public string GenerateGraph(WorkItemCollection data, Options options)
      {
         var builder = new StringBuilder();

         builder.AppendLine("digraph {");

         builder.AppendLine($"  rankdir = {options.RankDir};");
         builder.AppendLine($"  node [fontname=\"{GraphTextFontFace}\"];");
         builder.AppendLine($"  edge [fontname=\"{GraphTextFontFace}\"];");

         foreach (var item in data.WorkItems.OrderBy(x => x.Id))
         {
            var attributes = new Dictionary<string, string>();

            var commits = item.Commits == 0 ? "" : item.Commits == 1 ? $"{item.Commits} commit" : $"{item.Commits} commits";
            var workItemAndState = $"{item.Type} {item.Id}";
            if (!string.IsNullOrWhiteSpace(item.State))
               workItemAndState = $"{workItemAndState} - {item.State}";

            var iconMetadata = string.Concat(new[]
            {
               BuildIconMetadataSegment(item.Tags.Join(", "), TagIcon),
               BuildIconMetadataSegment(item.AssignedTo, AssigneeIcon),
               BuildIconMetadataSegment(commits, GithubIcon)
            });

            var metadata = BuildTextSegment(workItemAndState);
            if (!string.IsNullOrWhiteSpace(iconMetadata))
               metadata = string.IsNullOrWhiteSpace(metadata) ? iconMetadata : $"{metadata}{iconMetadata}";

            var name = BuildTextSegment(item.Name);

            var highlight = options.HighlightedWorkItemIds.Contains(item.Id);
            var nodeTypeClass = GetWorkItemTypeClass(item.Type);
            var nodeState = item.IsFinished ? "finished" : "active";
            var nodeHighlight = highlight ? "highlighted" : "normal";
            var nodeClasses = string.Join(" ", new[]
            {
               "wi-node",
               $"wi-node-{nodeTypeClass}",
               item.IsFinished ? "is-finished" : "is-active",
               highlight ? "is-highlighted" : null
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            attributes["label"] = $"<<table border=\"0\"><tr><td>{metadata}</td></tr><tr><td>{name}</td></tr></table>>";
            attributes["id"] = $"\"wi-node-{nodeTypeClass}-{nodeState}-{nodeHighlight}-{item.Id}\"";
            attributes["class"] = $"\"{nodeClasses}\"";
            attributes["shape"] = "box";
            attributes["style"] = highlight ? "\"bold,filled,rounded\"" : "\"filled,rounded\"";
            attributes["fontsize"] = highlight ? "18" : "14";
            attributes["URL"] = $"\"https://dev.azure.com/{options.AzureDevOpsOrganization}/{options.AzureDevOpsProject}/_workitems/edit/{item.Id}\"";
            attributes["target"] = "\"_blank\"";

            builder.AppendLine($"  {item.Id} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         foreach (var link in data.Links.OrderBy(x => x.FromWorkItemId).ThenBy(x => x.ToWorkItemId).ThenBy(x => x.Type))
         {
            var linkTypeClass = link.Type.ToString().Kebaberize();
            var attributes = new Dictionary<string, string>
            {
               ["label"] = $"\"{link.Type.ToString().Humanize()}\"",
               ["class"] = $"\"wi-link wi-link-{linkTypeClass}\""
            };

            if (link.Type == LinkType.Related)
            {
               attributes["constraint"] = "false";
               attributes["dir"] = "both";
               attributes["style"] = "dashed";
            }

            builder.AppendLine($"  {link.FromWorkItemId} -> {link.ToWorkItemId} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         var sameRankLinks = data.Links
            .SelectMany(x => new[] { (WorkItemId: x.FromWorkItemId, Link: x), (WorkItemId: x.ToWorkItemId, Link: x) })
            .GroupBy(x => x.WorkItemId, x => x.Link)
            .Where(g => g.Count() == 1)
            .Select(g => g.Single())
            .Where(x => x.Type == LinkType.Related)
            .ToList();

         foreach (var link in sameRankLinks.OrderBy(x => x.FromWorkItemId).ThenBy(x => x.ToWorkItemId))
         {
            builder.AppendLine($"  {{rank=same;{link.FromWorkItemId};{link.ToWorkItemId}}}");
         }

         builder.AppendLine("}");

         return builder.ToString();
      }

      private static string BuildIconMetadataSegment(string value, string iconGlyph)
      {
         if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(iconGlyph))
            return string.Empty;

         return $"<FONT FACE=\"remixicon\" POINT-SIZE=\"12\">{iconGlyph}</FONT>&nbsp;{BuildTextSegment($"{value}")}";
      }

      private static string BuildTextSegment(string value)
      {
         if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

         var encodedValue = WebUtility.HtmlEncode(value);
         return $"<FONT FACE=\"{GraphTextFontFace}\">{encodedValue}</FONT>";
      }

      private static string GetWorkItemTypeClass(WorkItemType type)
      {
         return type switch
         {
            WorkItemType.Bug => "bug",
            WorkItemType.Epic => "epic",
            WorkItemType.Feature => "feature",
            WorkItemType.PBI => "pbi",
            WorkItemType.Task => "task",
            _ => "unknown"
         };
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
