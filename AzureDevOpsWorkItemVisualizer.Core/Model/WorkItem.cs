﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AzureDevOpsWorkItemVisualizer.Core.Model
{
   [DebuggerDisplay("{Id}:{Type}")]
   public class WorkItem
   {
      public string AssignedTo { get; set; }
      public int Commits { get; set; }
      public int Id { get; set; }
      public bool IsFinished { get; set; }
      public string Name { get; set; }
      public string State { get; set; }
      public IEnumerable<string> Tags { get; set; }
      public WorkItemType Type { get; set; }

      protected bool Equals(WorkItem other)
      {
         return Id == other.Id;
      }

      public override bool Equals(object obj)
      {
         if (ReferenceEquals(null, obj)) return false;
         if (ReferenceEquals(this, obj)) return true;
         if (obj.GetType() != this.GetType()) return false;
         return Equals((WorkItem)obj);
      }

      public override int GetHashCode()
      {
         return Id;
      }

      public static bool operator ==(WorkItem left, WorkItem right)
      {
         return Equals(left, right);
      }

      public static bool operator !=(WorkItem left, WorkItem right)
      {
         return !Equals(left, right);
      }

      public static bool TryCreate(AzureDevOpsClient.DevOpsItem item, out WorkItem workItem)
      {
         workItem = null;

         if (SupportedWorkItemTypes.TryGetValue(item.Fields.Type, out var workItemType) == false)
            return false;

         workItem = new WorkItem
         {
            AssignedTo = item.Fields.AssignedTo?.Name ?? string.Empty,
            Commits = item.Relations.Count(x => x.Attributes.Name == "GitHub Commit"),
            Id = item.Id,
            IsFinished = IsFinishedState(item.Fields.State),
            Name = item.Fields.Title,
            State = item.Fields.State,
            Tags = ParseWorkItemTags(item.Fields.Tags),
            Type = workItemType
         };

         return true;
      }

      public static bool IsFinishedState(string state)
      {
         return state == "Closed" || state == "Done" || state == "Removed";
      }

      private static IEnumerable<string> ParseWorkItemTags(string tags)
      {
         return tags?.Split(';').Select(x => x.Trim()).OrderBy(x => x) ?? Enumerable.Empty<string>();
      }

      private static readonly Dictionary<string, WorkItemType> SupportedWorkItemTypes =
         new Dictionary<string, WorkItemType>
         {
            { "Bug", WorkItemType.Bug },
            { "Epic", WorkItemType.Epic },
            { "Feature", WorkItemType.Feature },
            { "Product Backlog Item", WorkItemType.PBI },
            { "Task", WorkItemType.Task }
         };
   }
}