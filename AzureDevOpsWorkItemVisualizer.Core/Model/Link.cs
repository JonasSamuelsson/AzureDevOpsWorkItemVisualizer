using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AzureDevOpsWorkItemVisualizer.Core.Model
{
   [DebuggerDisplay("{Identifier}")]
   public class Link
   {
      public int FromWorkItemId { get; set; }
      public int ToWorkItemId { get; set; }
      public LinkType Type { get; set; }

      public string Identifier => $"{Type}; from:{FromWorkItemId}, to:{ToWorkItemId}:";

      protected bool Equals(Link other)
      {
         return Identifier == other.Identifier;
      }

      public override bool Equals(object obj)
      {
         if (ReferenceEquals(null, obj)) return false;
         if (ReferenceEquals(this, obj)) return true;
         if (obj.GetType() != this.GetType()) return false;
         return Equals((Link)obj);
      }

      public override int GetHashCode()
      {
         return Identifier.GetHashCode(StringComparison.Ordinal);
      }

      public static bool operator ==(Link left, Link right)
      {
         return Equals(left, right);
      }

      public static bool operator !=(Link left, Link right)
      {
         return !Equals(left, right);
      }

      public static bool TryCreate(int sourceWorkItemId, int targetWorkItemId, string relationName, out Link link)
      {
         if (relationName == null || RelationLinkTypes.TryGetValue(relationName, out var type) == false)
         {
            link = null;
            return false;
         }

         var isReversed = IsReversed(relationName, sourceWorkItemId, targetWorkItemId);

         link = new Link
         {
            FromWorkItemId = isReversed ? targetWorkItemId : sourceWorkItemId,
            ToWorkItemId = isReversed ? sourceWorkItemId : targetWorkItemId,
            Type = type
         };

         return true;
      }

      private static bool IsReversed(string relationName, int sourceWorkItemId, int targetWorkItemId)
      {
         if (ReverseRelations.Contains(relationName))
         {
            return true;
         }

         if (relationName == "Related")
         {
            return sourceWorkItemId > targetWorkItemId;
         }

         return false;
      }

      private static readonly Dictionary<string, LinkType> RelationLinkTypes = new Dictionary<string, LinkType>
      {
         { "Child", LinkType.HasChild },
         { "Parent", LinkType.HasChild },
         { "Predecessor", LinkType.DependsOn },
         { "Related", LinkType.Related },
         { "Successor", LinkType.DependsOn }
      };

      private static readonly ISet<string> ReverseRelations = new HashSet<string> { "Parent", "Successor" };
   }
}