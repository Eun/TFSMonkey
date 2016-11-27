using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TFSCore
{
	public abstract class IHistoryItem
	{
		public abstract int ChangeSetId { get; set; }
		public abstract string Committer { get; set; }
		public abstract string CommitterDisplayName { get; set; }
		public abstract string Comment { get; set; }
		public abstract DateTime CreationDate { get; set; }
		public abstract string Owner { get; set; }
		public abstract string OwnerDisplayName { get; set; }

		internal Changeset ChangeSet { get; set; }

		internal ILinking LinkingService { get; set; }

		internal WorkItemFactory WorkItemFactory { get; set; }

		internal static readonly LinkFilter[] LinkFilters = new LinkFilter[1] { new LinkFilter
		{
			FilterType = FilterType.ToolType,
			FilterValues = new String[1] { ToolNames.WorkItemTracking }
		}};


		public abstract ICollection<IWorkItem> WorkItems { get; }

		public static void CopyTo(IHistoryItem item, IHistoryItem newItem)
		{
			item.ChangeSetId = newItem.ChangeSetId;
			item.Committer = newItem.Committer;
			item.CommitterDisplayName = newItem.CommitterDisplayName;
			item.Comment = newItem.Comment;
			item.CreationDate = newItem.CreationDate;
			item.Owner = newItem.Owner;
			item.OwnerDisplayName = newItem.OwnerDisplayName;
			item.ChangeSet = newItem.ChangeSet;
			item.WorkItemFactory = newItem.WorkItemFactory;
			item.LinkingService = newItem.LinkingService;
		}
	}

	public class HistoryItem : IHistoryItem
	{
		public override int ChangeSetId { get; set; }
		public override string Committer { get; set; }
		public override string CommitterDisplayName { get; set; }
		public override string Comment { get; set; }
		public override DateTime CreationDate { get; set; }
		public override string Owner { get; set; }
		public override string OwnerDisplayName { get; set; }


		public override ICollection<IWorkItem> WorkItems
		{
			get
			{
				if (ChangeSet == null)
					return new List<IWorkItem>();
				Artifact[] artifacts = LinkingService.GetReferencingArtifacts(new[] {ChangeSet.ArtifactUri.AbsoluteUri}, LinkFilters);
				return IWorkItem.FromArtifacts(artifacts, WorkItemFactory);
			}
		}
	}
}